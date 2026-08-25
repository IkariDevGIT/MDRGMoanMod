using MoanMod.MoanModPreferences;

namespace MoanMod.UpdateDetection;

/// <inheritdoc cref="IModUpdateState"/>
public sealed class ModUpdateState : IModUpdateState
{
    private readonly IMoanModPreferences _preferences;
    private readonly SemanticVersion _currentVersion;

    public bool IsFirstLaunch { get; }
    public bool IsUpgrade { get; }
    public SemanticVersion PreviousVersion { get; }

    public ModUpdateState(IMoanModPreferences preferences, SemanticVersion currentVersion)
    {
        _preferences = preferences;
        _currentVersion = currentVersion;

        bool hadPriorInstall = preferences.NoticePopupShown;
        bool hasRecordedVersion = SemanticVersion.TryParse(preferences.LastRunModVersion, out var previousVersion);

        IsFirstLaunch = !hadPriorInstall;
        PreviousVersion = hasRecordedVersion ? previousVersion : null;
        IsUpgrade = hadPriorInstall && (!hasRecordedVersion || previousVersion < currentVersion);
    }

    public void MarkVersionSeen() => _preferences.LastRunModVersion = _currentVersion.ToString();
}
