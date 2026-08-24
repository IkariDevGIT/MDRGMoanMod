namespace MoanMod.Controllers;

/// <summary>
/// Owns mouth-open state and its auto-close timer.
/// The Live2D parameter is written through <see cref="ApplyToLive2D"/> once per frame.
/// </summary>
public interface IMouthController : IMoanController
{
    /// <summary>Opens the mouth at <paramref name="amount"/> and schedules it to close after <paramref name="durationSeconds"/>.</summary>
    void Open(float amount, float durationSeconds);

    /// <summary>Writes the current open/close state to the Live2D parameter.</summary>
    void ApplyToLive2D(Il2Cpp.ILive2DController_Mouth mouthController);
}
