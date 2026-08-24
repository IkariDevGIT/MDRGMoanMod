namespace MoanMod.Controllers;


public enum SceneType
{
    None, 
    Cowgirl,
    Shower,
    Generic
}

/// <summary>
/// Immutable snapshot of all game-state a controller might need for a single frame
/// </summary>
public sealed class BrainContext
{
    public Il2Cpp.ModelBrain Brain { get; }
    public float Pleasure { get; }
    public bool IsCumming { get; }
    public bool IsTalking { get; }
    public SceneType SceneType { get; }
    public int Lust { get; }
    public int Sympathy { get; }
    public bool PrologueFinished { get; }

    private BrainContext(
        Il2Cpp.ModelBrain brain,
        float pleasure,
        bool isCumming,
        bool isTalking,
        SceneType sceneType,
        int lust,
        int sympathy,
        bool prologueFinished)
    {
        Brain = brain;
        Pleasure = pleasure;
        IsCumming = isCumming;
        IsTalking = isTalking;
        SceneType = sceneType;
        Lust = lust;
        Sympathy = sympathy;
        PrologueFinished = prologueFinished;
    }

    private static SceneType SceneFrom(Il2Cpp.ModelBrain brain)
    {
        var state = brain?.CurrentState;
        if (state == null) return SceneType.None;
        if (state.TryCast<Il2Cpp.GenericFuckBrainState>() != null) return SceneType.Generic;
        if (state.TryCast<Il2Cpp.CowgirlBrainState>() != null) return SceneType.Cowgirl;
        if (state.TryCast<Il2Cpp.ShowerBrainState>() != null) return SceneType.Shower;

        return SceneType.None;
    }

    /// <summary>Reads all values from live game objects. Returns <c>null</c> if brain is missing.</summary>
    public static BrainContext TryCapture(Il2Cpp.ModelBrain brain)
    {
        if (brain == null) return null;

        var expression = brain.ConnectedController?.Expression;
        var gameVars = Il2Cpp.GameScript.Instance?.GameVariables;
        var stage = Il2Cpp.StorySingleton.Instance?.Stage1;

        return new BrainContext(
            brain: brain,
            pleasure: brain.Pleasure,
            isCumming: expression?.IsCumming ?? false,
            isTalking: brain.IsTalkingWithOverlay,
            sceneType: SceneFrom(brain),
            lust: gameVars?.lust ?? 0,
            sympathy: gameVars?.sympathy ?? 0,
            prologueFinished: stage?.IsPrologueFinished() ?? false
        );
    }
}