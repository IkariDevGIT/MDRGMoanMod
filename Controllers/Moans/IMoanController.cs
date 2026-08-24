namespace MoanMod.Controllers;

/// <summary>Base interface for all per-frame sex-scene controllers</summary>
public interface IMoanController
{
    /// <summary>Called every frame while in a sex scene with a snapshot of game state</summary>
    void Tick(BrainContext ctx);

    /// <summary>Resets all internal state</summary>
    void Reset();
}
