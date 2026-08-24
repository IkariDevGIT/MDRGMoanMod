namespace MoanMod.Controllers;

/// <summary>Manages the cumming moan lifecycle</summary>
public interface ICummingController : IMoanController
{
    /// <summary>True while the end moan clip has been scheduled but not yet played</summary>
    bool IsPendingEndMoan { get; }

    /// <summary>True while the end moan clip is actively playing</summary>
    bool IsPlayingEndMoan { get; }

    /// <summary>Called when cumming starts</summary>
    void OnStart(BrainContext ctx);

    /// <summary>
    /// Called when cumming ends
    /// Schedules or immediately plays the end moan and clears expression modifiers
    /// </summary>
    void OnEnd(BrainContext ctx);
}
