using MelonLoader;
using ModSettingsMenu;
using MoanMod.Config;
using MoanMod.MoanModPreferences;

namespace MoanMod.SettingsMenu;

/// <inheritdoc cref="ISettingsMenuService"/>
public sealed class MsmSettingsMenuService : ISettingsMenuService
{
    private const string ModId = "moanMod";

    private readonly IModConfig _config;
    private readonly IMoanModPreferences _preferences;

    private MelonPreferences_Category _category;

    private MelonPreferences_Entry<float> _mouthOpenMin;
    private MelonPreferences_Entry<float> _mouthOpenMax;
    private MelonPreferences_Entry<float> _breathMouthOpenMin;
    private MelonPreferences_Entry<float> _breathMouthOpenMax;
    private MelonPreferences_Entry<float> _sexSceneStartCooldown;
    private MelonPreferences_Entry<float> _thresholdCheckInterval;
    private MelonPreferences_Entry<float> _thresholdBaseLow;
    private MelonPreferences_Entry<float> _thresholdBaseHigh;
    private MelonPreferences_Entry<float> _thresholdPleasureCap;
    private MelonPreferences_Entry<float> _headpatPenalty;
    private MelonPreferences_Entry<float> _cowgirlMultiplier;
    private MelonPreferences_Entry<float> _headpatMovementMin;
    private MelonPreferences_Entry<int> _clusterMaxMoans;
    private MelonPreferences_Entry<float> _clusterDelayMin;
    private MelonPreferences_Entry<float> _clusterDelayMax;
    private MelonPreferences_Entry<int> _clusterRepeatCooldown;
    private MelonPreferences_Entry<float> _clusterRepeatChance;
    private MelonPreferences_Entry<float>[] _clusterProbabilities;
    private MelonPreferences_Entry<float> _lewdnessThreshold;
    private MelonPreferences_Entry<float> _happinessIncrease;
    private MelonPreferences_Entry<float> _breathDelayAfterMoanMin;
    private MelonPreferences_Entry<float> _breathDelayAfterMoanMax;
    private MelonPreferences_Entry<float> _breathMoanTrackingWindow;
    private MelonPreferences_Entry<float>[] _breathProbabilities;

    public MsmSettingsMenuService(IModConfig config, IMoanModPreferences preferences)
    {
        _config = config;
        _preferences = preferences;
    }

    public void Initialize()
    {
        CreateEntries();
        ApplyAllToConfig();

        MSM.RegisterMod(ModId, "Moan Mod");
        BuildLeftPanel();
        BuildRightPanel();
    }

    private void CreateEntries()
    {
        _category = MelonPreferences.CreateCategory("MoanModTuning");

        var d = MoanModDefaults.MouthOpen;
        _mouthOpenMin = _category.CreateEntry("MouthOpenMin", d.Min);
        _mouthOpenMax = _category.CreateEntry("MouthOpenMax", d.Max);

        var bd = MoanModDefaults.BreathMouthOpen;
        _breathMouthOpenMin = _category.CreateEntry("BreathMouthOpenMin", bd.Min);
        _breathMouthOpenMax = _category.CreateEntry("BreathMouthOpenMax", bd.Max);

        _sexSceneStartCooldown = _category.CreateEntry("SexSceneStartCooldown", MoanModDefaults.SexSceneStartCooldown);

        var t = MoanModDefaults.Threshold;
        _thresholdCheckInterval = _category.CreateEntry("ThresholdCheckInterval", t.CheckInterval);
        _thresholdBaseLow = _category.CreateEntry("ThresholdBaseLow", t.BaseLow);
        _thresholdBaseHigh = _category.CreateEntry("ThresholdBaseHigh", t.BaseHigh);
        _thresholdPleasureCap = _category.CreateEntry("ThresholdPleasureCap", t.PleasureCap);

        var m = MoanModDefaults.Modifiers;
        _headpatPenalty = _category.CreateEntry("HeadpatPenalty", m.HeadpatPenalty);
        _cowgirlMultiplier = _category.CreateEntry("CowgirlMultiplier", m.CowgirlMultiplier);
        _headpatMovementMin = _category.CreateEntry("HeadpatMovementMin", m.HeadpatMovementMin);

        var c = MoanModDefaults.Cluster;
        _clusterMaxMoans = _category.CreateEntry("ClusterMaxMoans", c.MaxMoans);
        _clusterDelayMin = _category.CreateEntry("ClusterDelayMin", c.Delay.Min);
        _clusterDelayMax = _category.CreateEntry("ClusterDelayMax", c.Delay.Max);
        _clusterRepeatCooldown = _category.CreateEntry("ClusterRepeatCooldown", c.RepeatCooldown);
        _clusterRepeatChance = _category.CreateEntry("ClusterRepeatChance", c.RepeatChance);
        _clusterProbabilities = new MelonPreferences_Entry<float>[c.Probabilities.Length];
        for (int i = 0; i < c.Probabilities.Length; i++)
            _clusterProbabilities[i] = _category.CreateEntry($"ClusterProbability{i + 1}", c.Probabilities[i]);

        var e = MoanModDefaults.Expressions;
        _lewdnessThreshold = _category.CreateEntry("LewdnessThreshold", e.LewdnessThreshold);
        _happinessIncrease = _category.CreateEntry("HappinessIncrease", e.HappinessIncrease);

        var b = MoanModDefaults.Breath;
        _breathDelayAfterMoanMin = _category.CreateEntry("BreathDelayAfterMoanMin", b.DelayAfterMoan.Min);
        _breathDelayAfterMoanMax = _category.CreateEntry("BreathDelayAfterMoanMax", b.DelayAfterMoan.Max);
        _breathMoanTrackingWindow = _category.CreateEntry("BreathMoanTrackingWindow", b.MoanTrackingWindow);
        _breathProbabilities = new MelonPreferences_Entry<float>[b.Probabilities.Length];
        for (int i = 0; i < b.Probabilities.Length; i++)
            _breathProbabilities[i] = _category.CreateEntry($"BreathProbability{i + 1}", b.Probabilities[i]);

        _category.SaveToFile(printmsg: false);
    }

    private void ApplyAllToConfig()
    {
        _config.MouthOpen = new FloatRange(_mouthOpenMin.Value, _mouthOpenMax.Value);
        _config.BreathMouthOpen = new FloatRange(_breathMouthOpenMin.Value, _breathMouthOpenMax.Value);
        _config.SexSceneStartCooldown = _sexSceneStartCooldown.Value;

        _config.Threshold = new ThresholdSettings(
            _thresholdCheckInterval.Value, _thresholdBaseLow.Value, _thresholdBaseHigh.Value, _thresholdPleasureCap.Value);

        _config.Modifiers = new ModifierSettings(
            _headpatPenalty.Value, _cowgirlMultiplier.Value, _headpatMovementMin.Value);

        _config.Cluster = new ClusterSettings(
            _clusterMaxMoans.Value,
            new FloatRange(_clusterDelayMin.Value, _clusterDelayMax.Value),
            _clusterRepeatCooldown.Value,
            _clusterRepeatChance.Value,
            ReadValues(_clusterProbabilities));

        _config.Expressions = new ExpressionSettings(_lewdnessThreshold.Value, _happinessIncrease.Value);

        _config.Breath = new BreathSettings(
            new FloatRange(_breathDelayAfterMoanMin.Value, _breathDelayAfterMoanMax.Value),
            _breathMoanTrackingWindow.Value,
            ReadValues(_breathProbabilities));
    }

    private static float[] ReadValues(MelonPreferences_Entry<float>[] entries)
    {
        var values = new float[entries.Length];
        for (int i = 0; i < entries.Length; i++) values[i] = entries[i].Value;
        return values;
    }

    private void BuildLeftPanel()
    {
        const PanelSide side = PanelSide.LeftPanel;

        MSM.AddLabel(ModId, side, "Mouth", Utilities.FontSize.Medium);
        FloatSlider(side, "Mouth Open Min", _mouthOpenMin, 0f, 1f, 101);
        FloatSlider(side, "Mouth Open Max", _mouthOpenMax, 0f, 1f, 101);
        FloatSlider(side, "Breath Mouth Open Min", _breathMouthOpenMin, 0f, 1f, 101);
        FloatSlider(side, "Breath Mouth Open Max", _breathMouthOpenMax, 0f, 1f, 101);
        MSM.AddPadding(ModId, side);

        MSM.AddLabel(ModId, side, "Pleasure Sensitivity", Utilities.FontSize.Medium);
        FloatSlider(side, "Check Interval", _thresholdCheckInterval, 0.05f, 2f, 40, "How often to check pleasure changes, in seconds.");
        FloatSlider(side, "Base Low", _thresholdBaseLow, 0f, 0.1f, 101, "Threshold at 0 pleasure (less sensitive).");
        FloatSlider(side, "Base High", _thresholdBaseHigh, 0f, 0.1f, 101, "Threshold at max pleasure (more sensitive).");
        FloatSlider(side, "Pleasure Cap", _thresholdPleasureCap, 0f, 1f, 101, "Pleasure value at which sensitivity maxes out.");
        MSM.AddPadding(ModId, side);

        MSM.AddLabel(ModId, side, "Modifiers", Utilities.FontSize.Medium);
        FloatSlider(side, "Headpat Penalty", _headpatPenalty, 0f, 0.1f, 101, "Added to the threshold while being petted.");
        FloatSlider(side, "Cowgirl Multiplier", _cowgirlMultiplier, 0f, 2f, 101, "Threshold multiplier during cowgirl.");
        FloatSlider(side, "Headpat Movement Min", _headpatMovementMin, 0f, 0.01f, 101, "Minimum hand movement to count as petting.");
        MSM.AddPadding(ModId, side);

        MSM.AddLabel(ModId, side, "Expressions", Utilities.FontSize.Medium);
        FloatSlider(side, "Lewdness Threshold", _lewdnessThreshold, 0f, 1f, 101);
        FloatSlider(side, "Happiness Increase", _happinessIncrease, 0f, 1f, 101);
        MSM.AddPadding(ModId, side);

        MSM.AddCheckbox(ModId, side, "Enable Update Checking",
            () => _preferences.UpdateCheckingEnabled,
            value => _preferences.UpdateCheckingEnabled = value,
            "Automatically check for new MoanMod releases on startup.");

        MSM.AddButton(ModId, side, "Reset to Defaults", ResetToDefaults, ButtonColor.Red);
    }

    private void BuildRightPanel()
    {
        const PanelSide side = PanelSide.RightPanel;

        MSM.AddLabel(ModId, side, "Sex Scene", Utilities.FontSize.Medium);
        FloatSlider(side, "Start Cooldown", _sexSceneStartCooldown, 0f, 10f, 101);
        MSM.AddPadding(ModId, side);

        MSM.AddLabel(ModId, side, "Moan Clustering", Utilities.FontSize.Medium);
        IntSlider(side, "Max Moans", _clusterMaxMoans, 1, 15);
        FloatSlider(side, "Delay Min", _clusterDelayMin, 0f, 1f, 101);
        FloatSlider(side, "Delay Max", _clusterDelayMax, 0f, 1f, 101);
        IntSlider(side, "Repeat Cooldown", _clusterRepeatCooldown, 0, 10);
        FloatSlider(side, "Repeat Chance", _clusterRepeatChance, 0f, 1f, 101);
        for (int i = 0; i < _clusterProbabilities.Length; i++)
            FloatSlider(side, $"Probability #{i + 1}", _clusterProbabilities[i], 0f, 1f, 101);
        MSM.AddPadding(ModId, side);

        MSM.AddLabel(ModId, side, "Breathing", Utilities.FontSize.Medium);
        FloatSlider(side, "Delay After Moan Min", _breathDelayAfterMoanMin, 0f, 1f, 101);
        FloatSlider(side, "Delay After Moan Max", _breathDelayAfterMoanMax, 0f, 1f, 101);
        FloatSlider(side, "Moan Tracking Window", _breathMoanTrackingWindow, 1f, 30f, 101, "How far back to look when counting recent moans, in seconds.");
        for (int i = 0; i < _breathProbabilities.Length; i++)
            FloatSlider(side, $"Probability #{i + 1}", _breathProbabilities[i], 0f, 1f, 101);
    }

    private void FloatSlider(PanelSide side, string label, MelonPreferences_Entry<float> entry, float min, float max, int steps, string tooltip = "")
    {
        MSM.AddSlider(ModId, side, label, min, max, () => entry.Value, steps, value =>
        {
            entry.Value = value;
            _category.SaveToFile(printmsg: false);
            ApplyAllToConfig();
        }, tooltip);
    }

    private void IntSlider(PanelSide side, string label, MelonPreferences_Entry<int> entry, int min, int max, string tooltip = "")
    {
        MSM.AddSlider(ModId, side, label, min, max, () => entry.Value, value =>
        {
            entry.Value = value;
            _category.SaveToFile(printmsg: false);
            ApplyAllToConfig();
        }, tooltip);
    }

    private void ResetToDefaults()
    {
        _mouthOpenMin.Value = MoanModDefaults.MouthOpen.Min;
        _mouthOpenMax.Value = MoanModDefaults.MouthOpen.Max;
        _breathMouthOpenMin.Value = MoanModDefaults.BreathMouthOpen.Min;
        _breathMouthOpenMax.Value = MoanModDefaults.BreathMouthOpen.Max;
        _sexSceneStartCooldown.Value = MoanModDefaults.SexSceneStartCooldown;

        _thresholdCheckInterval.Value = MoanModDefaults.Threshold.CheckInterval;
        _thresholdBaseLow.Value = MoanModDefaults.Threshold.BaseLow;
        _thresholdBaseHigh.Value = MoanModDefaults.Threshold.BaseHigh;
        _thresholdPleasureCap.Value = MoanModDefaults.Threshold.PleasureCap;

        _headpatPenalty.Value = MoanModDefaults.Modifiers.HeadpatPenalty;
        _cowgirlMultiplier.Value = MoanModDefaults.Modifiers.CowgirlMultiplier;
        _headpatMovementMin.Value = MoanModDefaults.Modifiers.HeadpatMovementMin;

        _clusterMaxMoans.Value = MoanModDefaults.Cluster.MaxMoans;
        _clusterDelayMin.Value = MoanModDefaults.Cluster.Delay.Min;
        _clusterDelayMax.Value = MoanModDefaults.Cluster.Delay.Max;
        _clusterRepeatCooldown.Value = MoanModDefaults.Cluster.RepeatCooldown;
        _clusterRepeatChance.Value = MoanModDefaults.Cluster.RepeatChance;
        for (int i = 0; i < _clusterProbabilities.Length; i++)
            _clusterProbabilities[i].Value = MoanModDefaults.Cluster.Probabilities[i];

        _lewdnessThreshold.Value = MoanModDefaults.Expressions.LewdnessThreshold;
        _happinessIncrease.Value = MoanModDefaults.Expressions.HappinessIncrease;

        _breathDelayAfterMoanMin.Value = MoanModDefaults.Breath.DelayAfterMoan.Min;
        _breathDelayAfterMoanMax.Value = MoanModDefaults.Breath.DelayAfterMoan.Max;
        _breathMoanTrackingWindow.Value = MoanModDefaults.Breath.MoanTrackingWindow;
        for (int i = 0; i < _breathProbabilities.Length; i++)
            _breathProbabilities[i].Value = MoanModDefaults.Breath.Probabilities[i];

        _category.SaveToFile(printmsg: false);
        ApplyAllToConfig();
        MSM.RefreshSettings(ModId);
    }
}
