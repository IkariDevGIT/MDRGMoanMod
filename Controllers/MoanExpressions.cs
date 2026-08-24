namespace MoanMod.Controllers
{
    public class MoanExpressions
    {
        public static void Apply(Il2Cpp.ModelBrain brain, float duration)
        {
            var expression = brain?.ConnectedController?.Expression;
            if (expression == null) return;

            float currentLewdness = expression._lastExpressionValues.Lewdness;
            if (currentLewdness < MoanModConfig.Expressions.LewdnessThreshold)
            {
                expression.AddModifier(
                    Il2Cpp.Live2DExpression.ExpressionModifierTypeEnum.Lewdness,
                    MoanModConfig.Expressions.LewdnessThreshold,
                    duration);
            }

            expression.AddModifier(
                Il2Cpp.Live2DExpression.ExpressionModifierTypeEnum.Happiness,
                MoanModConfig.Expressions.HappinessIncrease,
                duration);
        }
    }
}
