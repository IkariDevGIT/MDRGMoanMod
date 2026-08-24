using UnityEngine;

namespace MoanMod.Controllers;

/// <inheritdoc cref="IMouthController"/>
public sealed class MouthController : IMouthController
{
    private bool  _shouldBeOpen;
    private bool  _wasOpen;
    private float _openAmount;
    private float _closeTimer;

    public void Open(float amount, float durationSeconds)
    {
        _openAmount  = amount;
        _shouldBeOpen = true;
        _closeTimer  = durationSeconds;
    }

    public void Tick(BrainContext ctx)
    {
        if (_closeTimer <= 0f) return;

        _closeTimer -= Time.deltaTime;
        if (_closeTimer <= 0f) _shouldBeOpen = false;
    }

    public void ApplyToLive2D(Il2Cpp.ILive2DController_Mouth mouthController)
    {
        if (mouthController?.ParamMouthOpen == null) return;

        if (_shouldBeOpen)
        {
            mouthController.ParamMouthOpen.UnclampedValue = _openAmount;
            _wasOpen = true;
        }
        else if (_wasOpen)
        {
            mouthController.ParamMouthOpen.UnclampedValue = 0f;
            _wasOpen = false;
        }
    }

    public void Reset()
    {
        _shouldBeOpen = false;
        _wasOpen      = false;
        _openAmount   = 0.7f;
        _closeTimer   = 0f;
    }
}
