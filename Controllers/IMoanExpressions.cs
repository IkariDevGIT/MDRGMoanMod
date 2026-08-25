namespace MoanMod.Controllers;

/// <summary>Applies lewdness/happiness expression modifiers on top of a moan.</summary>
public interface IMoanExpressions
{
    void Apply(Il2Cpp.ModelBrain brain, float duration);
}
