namespace MoanMod.SettingsMenu;

/// <summary>Registers MoanMod's settings with the Mod Settings Menu.</summary>
public interface ISettingsMenuService
{
    /// <summary>Creates all MelonPreferences entries, applies them to the live config, and builds the MSM page.</summary>
    void Initialize();
}
