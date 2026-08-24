using MelonLoader;
using UnityEngine;

namespace MoanMod.Controllers;

/// <inheritdoc cref="ISexMoanController"/>
public sealed class SexMoanController : ISexMoanController
{
    private readonly AudioPlayer       _audioPlayer;
    private readonly IMouthController  _mouth;
    private readonly IBreathController _breath;
    private readonly IHeadpatController _headpat;
    private readonly ICummingController _cumming;

    // Keep last ctx so PlayInCluster (fired as a delayed breath callback) can use it.
    private BrainContext _ctx;

    private float _pleasureLogTimer;
    private float _lastPleasure;

    private int   _clusterCount;
    private float _clusterDelayTimer;
    private bool  _isInCluster;
    private float _sexMoanTimer;
    private float _sexMoanCooldown = 3.0f;

    public SexMoanController(
        AudioPlayer        audioPlayer,
        IMouthController   mouth,
        IBreathController  breath,
        IHeadpatController headpat,
        ICummingController cumming)
    {
        _audioPlayer = audioPlayer;
        _mouth       = mouth;
        _breath      = breath;
        _headpat     = headpat;
        _cumming     = cumming;
    }

    public void OnBrainFound() => _pleasureLogTimer = MoanModConfig.Threshold.CheckInterval;

    public void Tick(BrainContext ctx)
    {
        _ctx = ctx;
        TickSexMoanCooldown();
        TickPleasureCheck();
        TickCluster();
    }

    public void Reset()
    {
        _isInCluster       = false;
        _clusterCount      = 0;
        _clusterDelayTimer = 0f;
        _sexMoanTimer      = 0f;
        _pleasureLogTimer  = MoanModConfig.Threshold.CheckInterval;
        _lastPleasure      = 0f;
    }

    private void TickPleasureCheck()
    {
        _pleasureLogTimer -= Time.deltaTime;
        if (_pleasureLogTimer > 0f) return;

        _pleasureLogTimer = MoanModConfig.Threshold.CheckInterval;

        float pleasureChange = Mathf.Abs(_ctx.Pleasure - _lastPleasure);
        _lastPleasure = _ctx.Pleasure;

        float required = CalculatePleasureThreshold();

        bool canTrigger =
            pleasureChange > required
            && !_ctx.IsCumming
            && _sexMoanTimer <= 0f
            && !_isInCluster
            && !_cumming.IsPendingEndMoan
            && !_cumming.IsPlayingEndMoan
            && !_ctx.IsTalking
            && _audioPlayer.HasAudioFor(AudioType.Sex);

        if (!canTrigger) return;

        _clusterCount = 1;
        _isInCluster  = true;
        PlayInCluster();
    }

    private float CalculatePleasureThreshold()
    {
        float normalized = Mathf.Clamp01(_ctx.Pleasure / MoanModConfig.Threshold.PleasureCap);
        float thresholdRange = MoanModConfig.Threshold.BaseLow - MoanModConfig.Threshold.BaseHigh;
        float required = MoanModConfig.Threshold.BaseLow - (normalized * thresholdRange);

        if (_headpat.IsActive) required += MoanModConfig.Modifiers.HeadpatPenalty;
        if (_ctx.SceneType == SceneType.Cowgirl) required *= MoanModConfig.Modifiers.CowgirlMultiplier;

        return required;
    }

    private void TickSexMoanCooldown()
    {
        if (_sexMoanTimer <= 0f) return;

        _sexMoanTimer -= Time.deltaTime;
        if (_sexMoanTimer <= 0f)
        {
            _clusterCount = 0;
            _isInCluster  = false;
        }
    }

    private void TickCluster()
    {
        if (!_isInCluster || _clusterDelayTimer <= 0f || _breath.IsActive) return;

        if ((_clusterDelayTimer -= Time.deltaTime) > 0f) return;

        if (ShouldContinueCluster() && _clusterCount < MoanModConfig.Cluster.MaxMoans)
        {
            _clusterCount++;

            if (_breath.ShouldBreathe(_ctx)) _breath.TryStartSequence(_ctx, PlayInCluster);
            else PlayInCluster();
        }
        else
        {
            EndCluster();
        }
    }

    private void PlayInCluster()
    {
        _breath.LastActionWasBreath = false;
        _audioPlayer.PlaySexMoan(1f);
        _breath.AddMoanTimestamp();

        float length = _audioPlayer.LastPlayedLengthFor(AudioType.Sex);
        string name  = _audioPlayer.LastPlayedNameFor(AudioType.Sex);
        float amount = UnityEngine.Random.Range(MoanModConfig.MouthOpen.Min, MoanModConfig.MouthOpen.Max);

        _mouth.Open(amount, length);
        MoanExpressions.Apply(_ctx.Brain, length);

        _clusterDelayTimer = length + UnityEngine.Random.Range(MoanModConfig.Cluster.Delay.Min, MoanModConfig.Cluster.Delay.Max);

        MelonLogger.Msg($"Sex moan '{name}' (cluster #{_clusterCount})! Clip: {length:F2}s, Mouth: {amount:F2}");
    }

    private bool ShouldContinueCluster()
    {
        if (_clusterCount < 1 || _clusterCount > MoanModConfig.Cluster.Probabilities.Length)
            return false;

        return UnityEngine.Random.Range(0f, 1f) < MoanModConfig.Cluster.Probabilities[_clusterCount - 1];
    }

    private void EndCluster()
    {
        _sexMoanCooldown = CalculateSexMoanFrequency();
        _sexMoanTimer    = _sexMoanCooldown;
        _isInCluster     = false;

        MelonLogger.Msg($"Cluster ended after {_clusterCount} moans. Cooldown: {_sexMoanCooldown:F2}s");
    }

    private float CalculateSexMoanFrequency()
    {
        float pleasureFactor = _ctx.Pleasure;
        float lustFactor = Mathf.Clamp01((_ctx.Lust      - 200f) / 1800f);
        float sympathyFactor = Mathf.Clamp01((_ctx.Sympathy  - 150f) / 1350f);
        float combined = (pleasureFactor * 0.5f) + (lustFactor * 0.25f) + (sympathyFactor * 0.25f);

        float rangeMin = 3.0f - (combined * 2.5f);
        float rangeMax = 5.0f - (combined * 3.0f);

        return UnityEngine.Random.Range(rangeMin, rangeMax);
    }
}
