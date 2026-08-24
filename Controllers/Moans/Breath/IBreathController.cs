namespace MoanMod.Controllers;

/// <summary>
/// Manages the breath state and the moan frequency window
/// </summary>
public interface IBreathController : IMoanController
{
    /// <summary>True while a breath clip is playing or the post-breath silence is counting down</summary>
    bool IsActive { get; }

    /// <summary>Set to true after a breath plays, must be cleared by the next non-breath action</summary>
    bool LastActionWasBreath { get; set; }

    /// <summary>Records the current timestamp for the moan frequency window</summary>
    void AddMoanTimestamp();

    /// <summary>Returns the number of moans within the configured window</summary>
    int GetMoanCountInWindow();

    /// <summary>Whether the system wants to breath before the next moan</summary>
    bool ShouldBreathe(BrainContext ctx);

    /// <summary>
    /// Starts a breath -> silence -> <paramref name="onReady"/> sequence
    /// Returns <c>false</c> when no breath audio is available
    /// </summary>
    bool TryStartSequence(BrainContext ctx, Action onReady);
}
