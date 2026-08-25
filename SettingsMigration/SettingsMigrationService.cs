using MoanMod.MoanModPreferences;
using MoanMod.PopupService;
using MoanMod.SettingsMenu;
using UnityEngine;

namespace MoanMod.SettingsMigration;

/// <inheritdoc cref="ISettingsMigrationService"/>
public sealed class SettingsMigrationService : ISettingsMigrationService
{
    private readonly IMoanModPreferences _preferences;
    private readonly IPopupService _popupService;
    private readonly ISettingsMenuService _settingsMenu;
    private readonly bool _hadPriorInstall;

    public SettingsMigrationService(IMoanModPreferences preferences, IPopupService popupService, ISettingsMenuService settingsMenu)
    {
        _preferences = preferences;
        _popupService = popupService;
        _settingsMenu = settingsMenu;
        _hadPriorInstall = preferences.NoticePopupShown;
    }

    public System.Collections.IEnumerator PromptIfNeeded(SemanticVersion currentVersion)
    {
        if (ShouldPromptTuningReset(currentVersion))
        {
            var showingPrompt = true;
            ShowTuningResetPrompt(() => showingPrompt = false);
            yield return new WaitWhile((Func<bool>)(() => showingPrompt));
        }

        _preferences.LastRunModVersion = currentVersion.ToString();
    }

    private bool ShouldPromptTuningReset(SemanticVersion currentVersion)
    {
        if (!_hadPriorInstall) return false;

        bool hasRecordedVersion = SemanticVersion.TryParse(_preferences.LastRunModVersion, out var lastRunVersion);
        return !hasRecordedVersion || lastRunVersion < currentVersion;
    }

    private void ShowTuningResetPrompt(Action onDismiss)
    {
        string title = "MoanMod - Settings Update";
        string message = "This update may have changed MoanMod's default tuning values (sensitivity, clustering, breathing, etc.), editable from the Mod Settings Menu.\n\nIf you never touched MoanMod's settings, resetting is safe and recommended, it will not change anything you would notice. If you specifically customized values, you can keep them instead.";

        var choices = new[]
        {
            new PopupChoice("Reset to New Defaults", () =>
            {
                _settingsMenu.ResetToDefaults();
                onDismiss?.Invoke();
            }),
            new PopupChoice("Keep My Current Settings", () => onDismiss?.Invoke()),
        };

        _popupService.ChoicePopup(title, message, choices);
    }
}
