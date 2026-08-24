namespace MoanMod.Controllers;

/// <summary>
/// Detects active headpatting
/// </summary>
public interface IHeadpatController : IMoanController
{
    /// <summary>True if the hand was actively moving across the head during the last tick</summary>
    bool IsActive { get; }
}
