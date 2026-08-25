using MoanMod.PopupService;
using MoanMod.SettingsMenu;
using MoanMod.UpdateDetection;
using UnityEngine;

namespace MoanMod.SettingsMigration;

/// <inheritdoc cref="ISettingsMigrationService"/>
public sealed class SettingsMigrationService : ISettingsMigrationService
{
    private readonly IModUpdateState _updateState;
    private readonly IPopupService _popupService;
    private readonly ISettingsMenuService _settingsMenu;

    public SettingsMigrationService(IModUpdateState updateState, IPopupService popupService, ISettingsMenuService settingsMenu)
    {
        _updateState = updateState;
        _popupService = popupService;
        _settingsMenu = settingsMenu;
    }

    public System.Collections.IEnumerator PromptIfNeeded()
    {
        if (!_updateState.IsUpgrade) yield break;

        var showingPrompt = true;
        ShowTuningResetPrompt(() => showingPrompt = false);
        yield return new WaitWhile((Func<bool>)(() => showingPrompt));
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
