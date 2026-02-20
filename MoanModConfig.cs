namespace MoanMod
{
    // ==================== HELPER STRUCTS ====================

    public readonly struct FloatRange
    {
        public readonly float Min;
        public readonly float Max;

        public FloatRange(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }

    public readonly struct ThresholdSettings
    {
        public readonly float CheckInterval;
        public readonly float BaseLow;
        public readonly float BaseHigh;
        public readonly float PleasureCap;

        public ThresholdSettings(float checkInterval, float baseLow, float baseHigh, float pleasureCap)
        {
            CheckInterval = checkInterval;
            BaseLow = baseLow;
            BaseHigh = baseHigh;
            PleasureCap = pleasureCap;
        }
    }

    public readonly struct ModifierSettings
    {
        public readonly float HeadpatPenalty;
        public readonly float CowgirlMultiplier;
        public readonly float HeadpatMovementMin;

        public ModifierSettings(float headpatPenalty, float cowgirlMultiplier, float headpatMovementMin)
        {
            HeadpatPenalty = headpatPenalty;
            CowgirlMultiplier = cowgirlMultiplier;
            HeadpatMovementMin = headpatMovementMin;
        }
    }

    public readonly struct ExpressionSettings
    {
        public readonly float LewdnessThreshold;
        public readonly float HappinessIncrease;

        public ExpressionSettings(float lewdnessThreshold, float happinessIncrease)
        {
            LewdnessThreshold = lewdnessThreshold;
            HappinessIncrease = happinessIncrease;
        }
    }

    public readonly struct BreathSettings
    {
        public readonly FloatRange DelayAfterMoan;
        public readonly float MoanTrackingWindow;
        public readonly float[] Probabilities;

        public BreathSettings(FloatRange delayAfterMoan, float moanTrackingWindow, float[] probabilities)
        {
            DelayAfterMoan = delayAfterMoan;
            MoanTrackingWindow = moanTrackingWindow;
            Probabilities = probabilities;
        }
    }

    public readonly struct ClusterSettings
    {
        public readonly int MaxMoans;
        public readonly FloatRange Delay;
        public readonly int RepeatCooldown;
        public readonly float RepeatChance;
        public readonly float[] Probabilities;

        public ClusterSettings(int maxMoans, FloatRange delay, int repeatCooldown, float repeatChance, float[] probabilities)
        {
            MaxMoans = maxMoans;
            Delay = delay;
            RepeatCooldown = repeatCooldown;
            RepeatChance = repeatChance;
            Probabilities = probabilities;
        }
    }

    // ==================== CONFIGURATION ====================

    public static class MoanModConfig
    {
        // Game version
        public const string ExpectedGameVersion = "0.95";

        // Mouth animation
        public static readonly FloatRange MouthOpen = new(min: 0.4f, max: 0.8f);
        public static readonly FloatRange BreathMouthOpen = new(min: 0.2f, max: 0.55f);

        // Sex scene
        public const float SexSceneStartCooldown = 3.0f;

        // Pleasure-based thresholds
        public static readonly ThresholdSettings Threshold = new(
            checkInterval: 0.25f,      // How often to check pleasure changes (seconds)
            baseLow: 0.02f,            // Threshold at 0.0 pleasure (less sensitive)
            baseHigh: 0.0075f,         // Threshold at 0.8+ pleasure (more sensitive)
            pleasureCap: 0.8f          // Pleasure value for maximum sensitivity
        );

        // Threshold modifiers
        public static readonly ModifierSettings Modifiers = new(
            headpatPenalty: 0.01f,         // Added to threshold while being petted (makes less sensitive)
            cowgirlMultiplier: 0.75f,      // Multiplier for cowgirl position (0.75 = 25% more sensitive)
            headpatMovementMin: 0.001f     // Minimum X/Y movement to detect petting
        );

        // Sex moan clustering
        public static readonly ClusterSettings Cluster = new(
            maxMoans: 6,                   // Maximum moans in a cluster
            delay: new FloatRange(min: 0.05f, max: 0.2f),  // Additional delay after clip finishes before next moan
            repeatCooldown: 3,             // How many moans before same clip can repeat
            repeatChance: 0.5f,            // 50% chance to repeat previous moan
            probabilities: new[] { 0.65f, 0.50f, 0.35f, 0.20f, 0.10f, 0.05f }  // Probability for each additional moan
        );

        // Expression control during sex moans
        public static readonly ExpressionSettings Expressions = new(
            lewdnessThreshold: 0.65f,      // only set lewdness to this if below
            happinessIncrease: 0.6f        // add this much happiness
        );

        // Breathing system
        public static readonly BreathSettings Breath = new(
            delayAfterMoan: new FloatRange(min: 0.1f, max: 0.3f),    // delay after breath before moan
            moanTrackingWindow: 7.5f,                                  // track moans in last 7.5 seconds
            probabilities: new[] { 0.0f, 0.15f, 0.35f, 0.55f, 0.75f, 0.90f }  // 0-1, 2-3, 4-5, 6-7, 8+ moans/7.5s
        );
    }
}