using UnityEngine;

namespace MoanMod.Controllers;

/// <inheritdoc cref="IHeadpatController"/>
public sealed class HeadpatController : IHeadpatController
{
    private float _lastX;
    private float _lastY;

    public bool IsActive { get; private set; }

    public void Tick(BrainContext ctx)
    {
        var headpat = ctx.Brain.ConnectedController?.TryCast<Il2Cpp.ILive2DController_Headpat>();

        if (headpat?.ParamHeadpat == null || headpat.ParamHeadpat.Value < 0.99f)
        {
            IsActive = false;
            return;
        }

        float x  = headpat.ParamHeadpatX?.Value ?? 0f;
        float y  = headpat.ParamHeadpatY?.Value ?? 0f;
        float dx = Mathf.Abs(x - _lastX);
        float dy = Mathf.Abs(y - _lastY);

        _lastX   = x;
        _lastY   = y;

        IsActive = dx >= MoanModConfig.Modifiers.HeadpatMovementMin
                || dy >= MoanModConfig.Modifiers.HeadpatMovementMin;
    }

    public void Reset()
    {
        _lastX   = 0f;
        _lastY   = 0f;
        IsActive = false;
    }
}
