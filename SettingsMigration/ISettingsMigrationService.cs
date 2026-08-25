namespace MoanMod.SettingsMigration;

/// <summary>Detects mod updates and, when tuning defaults may have changed, offers to reset them.</summary>
public interface ISettingsMigrationService
{
    /// <summary>Prompts the user if this run follows an update from an older MoanMod version, then records the current version.</summary>
    System.Collections.IEnumerator PromptIfNeeded(SemanticVersion currentVersion);
}
