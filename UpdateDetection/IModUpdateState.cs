namespace MoanMod.UpdateDetection;

/// <summary>Shared "is this a fresh install or an update" detection, computed once per session.</summary>
public interface IModUpdateState
{
    bool IsFirstLaunch { get; }
    bool IsUpgrade { get; }
    SemanticVersion PreviousVersion { get; }

    /// <summary>Records the currently running version so this session's state isn't re-triggered next launch.</summary>
    void MarkVersionSeen();
}
