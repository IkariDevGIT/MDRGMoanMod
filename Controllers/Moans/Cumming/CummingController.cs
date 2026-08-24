using MelonLoader;
using UnityEngine;

namespace MoanMod.Controllers;

/// <inheritdoc cref="ICummingController"/>
public sealed class CummingController : ICummingController
{
    private const float DefaultCooldown = 1.4f;
    private readonly AudioPlayer       _audioPlayer;
    private readonly IMouthController  _mouth;
    private readonly IBreathController _breath;

    private BrainContext _ctx;
    private float _moanTimer;
    private float _moanCooldown = DefaultCooldown;
    private float _endMoanTimer;

    public bool IsPendingEndMoan { get; private set; }
    public bool IsPlayingEndMoan { get; private set; }

    public CummingController(AudioPlayer audioPlayer, IMouthController mouth, IBreathController breath)
    {
        _audioPlayer = audioPlayer;
        _mouth       = mouth;
        _breath      = breath;
    }

    public void OnStart(BrainContext ctx)
    {
        IsPendingEndMoan = false;
        IsPlayingEndMoan = false;
        _moanTimer       = 0f;

        if (!ctx.PrologueFinished)
        {
            MelonLogger.Msg("Prologue not finished — moaning disabled");
            return;
        }

        MelonLogger.Msg($"Stats — Lust: {ctx.Lust}, Sympathy: {ctx.Sympathy}");

        if (ctx.Sympathy <= 5 || ctx.Lust <= 10)
        {
            MelonLogger.Msg("Stats too low for moaning");
            return;
        }

        _audioPlayer.PlayStartMoan(1f);
        _breath.AddMoanTimestamp();

        float startLength = _audioPlayer.LastPlayedLengthFor(AudioType.CumStart);
        if (startLength > 0f)
        {
            float amount = UnityEngine.Random.Range(MoanModConfig.MouthOpen.Min, MoanModConfig.MouthOpen.Max);
            _mouth.Open(amount, startLength);
            MelonLogger.Msg($"Playing start moan! Length: {startLength:F2}s, Mouth: {amount:F2}");
        }

        _audioPlayer.ResetCooldownsFor(AudioType.CumWhile);
        _moanTimer = startLength;
    }

    public void OnEnd(BrainContext ctx)
    {
        MelonLogger.Msg("=== Cumming Ended ===");
        _audioPlayer.ResetCooldownsFor(AudioType.CumWhile);

        if (_moanTimer > 0f)
        {
            _endMoanTimer    = _moanTimer;
            IsPendingEndMoan = true;
            MelonLogger.Msg($"Scheduled end moan in {_endMoanTimer:F2}s (after current moan finishes)");
        }
        else
        {
            PlayEndMoan();
        }

        ctx.Brain?.ConnectedController?.Expression?.ClearExpression();
    }

    public void Tick(BrainContext ctx)
    {
        _ctx = ctx;

        TickPendingEndMoan();

        if (!ctx.IsCumming || !_audioPlayer.HasAudioFor(AudioType.CumWhile) || _breath.IsActive) return;

        _moanTimer -= Time.deltaTime;
        if (_moanTimer > 0f) return;

        if (ctx.IsTalking)
        {
            MelonLogger.Msg("Skipping moan — robot is talking");
            _moanTimer = _moanCooldown;
            return;
        }

        _moanCooldown = CalculateMoanFrequency();

        if (_breath.ShouldBreathe(ctx))
            _breath.TryStartSequence(ctx, PlayCummingMoan);
        else
            PlayCummingMoan();
    }

    public void Reset()
    {
        _moanTimer       = 0f;
        _moanCooldown    = DefaultCooldown;
        _endMoanTimer    = 0f;
        IsPendingEndMoan = false;
        IsPlayingEndMoan = false;
    }

    private void TickPendingEndMoan()
    {
        if (!IsPendingEndMoan) return;

        _endMoanTimer -= Time.deltaTime;
        if (_endMoanTimer > 0f) return;

        PlayEndMoan();
        IsPendingEndMoan = false;
        IsPlayingEndMoan = true;
    }

    private void PlayCummingMoan()
    {
        _breath.LastActionWasBreath = false;
        _audioPlayer.PlayRandomMoan(1f);
        _breath.AddMoanTimestamp();

        float length = _audioPlayer.LastPlayedLengthFor(AudioType.CumWhile);
        string name  = _audioPlayer.LastPlayedNameFor(AudioType.CumWhile);
        _moanTimer   = length + _moanCooldown;

        float amount = UnityEngine.Random.Range(MoanModConfig.MouthOpen.Min, MoanModConfig.MouthOpen.Max);
        _mouth.Open(amount, length);

        MelonLogger.Msg(
            $"Played moan '{name}'! Clip: {length:F2}s, Cooldown: {_moanCooldown:F2}s, " +
            $"Next in: {_moanTimer:F2}s, Mouth: {amount:F2}");
    }

    private void PlayEndMoan()
    {
        _audioPlayer.PlayEndMoan(1f);

        float length = _audioPlayer.LastPlayedLengthFor(AudioType.CumEnd);
        if (length > 0f)
        {
            float amount = UnityEngine.Random.Range(MoanModConfig.MouthOpen.Min, MoanModConfig.MouthOpen.Max);
            _mouth.Open(amount, length);
        }

        MelonLogger.Msg("Playing end moan!");
    }

    private float CalculateMoanFrequency()
    {
        float lustNorm     = Mathf.Clamp01((_ctx.Lust      - 10f)  / 1990f);
        float sympathyNorm = Mathf.Clamp01((_ctx.Sympathy  -  5f)  / 1495f);
        float factor       = (lustNorm + sympathyNorm) / 2f;

        float rangeMin = 1.0f - (factor * 0.9f);
        float rangeMax = 1.8f - (factor * 1.3f);

        return UnityEngine.Random.Range(rangeMin, rangeMax);
    }
}
