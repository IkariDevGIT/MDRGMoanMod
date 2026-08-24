using MelonLoader;
using UnityEngine;

namespace MoanMod.Controllers;

/// <inheritdoc cref="IBreathController"/>
public sealed class BreathController : IBreathController
{
    private readonly AudioPlayer _audioPlayer;
    private readonly IMouthController _mouth;

    private readonly Queue<float> _moanTimestamps = new();
    private bool _breathPlaying;
    private float _breathTimer;
    private float _postBreathDelay;
    private Action _onReady;

    public bool IsActive => _breathPlaying || _postBreathDelay > 0f;
    public bool LastActionWasBreath { get; set; }

    public BreathController(AudioPlayer audioPlayer, IMouthController mouth)
    {
        _audioPlayer = audioPlayer;
        _mouth = mouth;
    }

    public void AddMoanTimestamp() => _moanTimestamps.Enqueue(Time.time);

    public int GetMoanCountInWindow()
    {
        float cutoff = Time.time - MoanModConfig.Breath.MoanTrackingWindow;
        while (_moanTimestamps.Count > 0 && _moanTimestamps.Peek() < cutoff)
            _moanTimestamps.Dequeue();
        return _moanTimestamps.Count;
    }

    public bool ShouldBreathe(BrainContext ctx)
    {
        if (!_audioPlayer.HasAudioFor(AudioType.Breath) || LastActionWasBreath || ctx.IsTalking) return false;

        int count = GetMoanCountInWindow();
        int tier  = Mathf.Min(count / 2, MoanModConfig.Breath.Probabilities.Length - 1);
        return UnityEngine.Random.Range(0f, 1f) < MoanModConfig.Breath.Probabilities[tier];
    }

    public bool TryStartSequence(BrainContext ctx, Action onReady)
    {
        float length = _audioPlayer.PlayBreath();

        if (length <= 0f) return false;

        _breathPlaying = true;
        _breathTimer = length;
        _onReady = onReady;
        LastActionWasBreath = true;

        float mouthAmount = UnityEngine.Random.Range(MoanModConfig.BreathMouthOpen.Min, MoanModConfig.BreathMouthOpen.Max);
        _mouth.Open(mouthAmount, length);

        MelonLogger.Msg(
            $"Breath '{_audioPlayer.LastPlayedNameFor(AudioType.Breath)}'! " +
            $"Length: {length:F2}s, Moans in last {MoanModConfig.Breath.MoanTrackingWindow:F1}s: {GetMoanCountInWindow()}");

        return true;
    }

    public void Tick(BrainContext ctx)
    {
        if (_breathPlaying)
        {
            _breathTimer -= Time.deltaTime;
            if (_breathTimer <= 0f)
            {
                _breathPlaying   = false;
                _postBreathDelay = UnityEngine.Random.Range(
                    MoanModConfig.Breath.DelayAfterMoan.Min,
                    MoanModConfig.Breath.DelayAfterMoan.Max
                );
            }
        }

        if (!_breathPlaying && _postBreathDelay > 0f)
        {
            _postBreathDelay -= Time.deltaTime;
            if (_postBreathDelay <= 0f)
            {
                var callback = _onReady;
                _onReady = null;
                callback?.Invoke();
            }
        }
    }

    public void Reset()
    {
        _moanTimestamps.Clear();
        LastActionWasBreath = false;
        _breathPlaying = false;
        _breathTimer = 0f;
        _postBreathDelay = 0f;
        _onReady = null;
    }
}
