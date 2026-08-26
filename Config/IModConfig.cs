namespace MoanMod.Config;

/// <summary>Live tuning values every controller reads, adjustable at runtime via the Mod Settings Menu.</summary>
public interface IModConfig
{
    FloatRange MouthOpen { get; set; }
    FloatRange BreathMouthOpen { get; set; }
    float MoanVolume { get; set; }
    float SexSceneStartCooldown { get; set; }
    ThresholdSettings Threshold { get; set; }
    ModifierSettings Modifiers { get; set; }
    ClusterSettings Cluster { get; set; }
    ExpressionSettings Expressions { get; set; }
    BreathSettings Breath { get; set; }

    /// <summary>Resets every value back to MoanModDefaults.</summary>
    void ResetToDefaults();
}
