using MoanMod.MoanModPreferences;
using MoanMod.PopupService;
using MoanMod.UpdateDetection;
using UnityEngine;

namespace MoanMod.ReleaseNotices;

/// <inheritdoc cref="IReleaseNoticeService"/>
public sealed class ReleaseNoticeService : IReleaseNoticeService
{
    private readonly IModUpdateState _updateState;
    private readonly IMoanModPreferences _preferences;
    private readonly IPopupService _popupService;

    public ReleaseNoticeService(IModUpdateState updateState, IMoanModPreferences preferences, IPopupService popupService)
    {
        _updateState = updateState;
        _preferences = preferences;
        _popupService = popupService;
    }

    public System.Collections.IEnumerator ShowIfNeeded()
    {
        if (_updateState.IsFirstLaunch) yield return ShowNotice();
        else if (_updateState.IsUpgrade) yield return ShowWhatsNew();
    }

    private System.Collections.IEnumerator ShowNotice()
    {
        string message = "This is the second public release of MoanMod. It's still actively being improved, so please report any bugs via GitHub issues or to IkariDev on Discord. You're also welcome to open a PR and help make it better!\n\nHave fun!";

        var showing = true;
        _popupService.SimplePopup("MoanMod - Notice", message, () => showing = false);
        _preferences.NoticePopupShown = true;

        yield return new WaitWhile((Func<bool>)(() => showing));
    }

    private System.Collections.IEnumerator ShowWhatsNew()
    {
        var entries = ModChangelog.Entries
            .Where(e => _updateState.PreviousVersion == null || e.Version > _updateState.PreviousVersion)
            .ToList();

        if (entries.Count == 0) yield break;

        var lines = new List<string>();
        foreach (var entry in entries)
            lines.AddRange(entry.Highlights);
        string message = "MoanMod was updated. Here's what's new:\n\n" + string.Join("\n", lines);

        var showing = true;
        _popupService.SimplePopup("MoanMod - What's New", message, () => showing = false);

        yield return new WaitWhile((Func<bool>)(() => showing));
    }
}
