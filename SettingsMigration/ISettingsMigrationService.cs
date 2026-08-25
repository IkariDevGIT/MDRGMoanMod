namespace MoanMod.SettingsMigration;

/// <summary>When tuning defaults may have changed since the user's last install, offers to reset them.</summary>
public interface ISettingsMigrationService
{
    System.Collections.IEnumerator PromptIfNeeded();
}
