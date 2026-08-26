namespace MoanMod.Config;

/// <inheritdoc cref="IModConfig"/>
public sealed class ModConfig : IModConfig
{
    public FloatRange MouthOpen { get; set; }
    public FloatRange BreathMouthOpen { get; set; }
    public float MoanVolume { get; set; }
    public float SexSceneStartCooldown { get; set; }
    public ThresholdSettings Threshold { get; set; }
    public ModifierSettings Modifiers { get; set; }
    public ClusterSettings Cluster { get; set; }
    public ExpressionSettings Expressions { get; set; }
    public BreathSettings Breath { get; set; }

    public ModConfig() => ResetToDefaults();

    public void ResetToDefaults()
    {
        MouthOpen = MoanModDefaults.MouthOpen;
        BreathMouthOpen = MoanModDefaults.BreathMouthOpen;
        MoanVolume = MoanModDefaults.MoanVolume;
        SexSceneStartCooldown = MoanModDefaults.SexSceneStartCooldown;
        Threshold = MoanModDefaults.Threshold;
        Modifiers = MoanModDefaults.Modifiers;
        Expressions = MoanModDefaults.Expressions;

        var cluster = MoanModDefaults.Cluster;
        Cluster = new ClusterSettings(cluster.MaxMoans, cluster.Delay, cluster.RepeatCooldown, cluster.RepeatChance, (float[])cluster.Probabilities.Clone());

        var breath = MoanModDefaults.Breath;
        Breath = new BreathSettings(breath.DelayAfterMoan, breath.MoanTrackingWindow, (float[])breath.Probabilities.Clone());
    }
}
