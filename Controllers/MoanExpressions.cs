using MoanMod.Config;

namespace MoanMod.Controllers;

/// <inheritdoc cref="IMoanExpressions"/>
public sealed class MoanExpressions : IMoanExpressions
{
    private readonly IModConfig _config;

    public MoanExpressions(IModConfig config)
    {
        _config = config;
    }

    public void Apply(Il2Cpp.ModelBrain brain, float duration)
    {
        var expression = brain?.ConnectedController?.Expression;
        if (expression == null) return;

        float currentLewdness = expression._lastExpressionValues.Lewdness;
        if (currentLewdness < _config.Expressions.LewdnessThreshold)
        {
            expression.AddModifier(
                Il2Cpp.Live2DExpression.ExpressionModifierTypeEnum.Lewdness,
                _config.Expressions.LewdnessThreshold,
                duration);
        }

        expression.AddModifier(
            Il2Cpp.Live2DExpression.ExpressionModifierTypeEnum.Happiness,
            _config.Expressions.HappinessIncrease,
            duration);
    }
}
