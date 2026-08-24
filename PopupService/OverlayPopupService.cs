namespace MoanMod.PopupService;

// TODO: Ask ikari if he would like to use nullables maybe? so we can use records
public class PopupChoice : IPopupChoice
{
    public string Label { get; }
    public Action OnSelected { get; }

    public PopupChoice(string label, Action onSelected)
    {
        Label = label ?? throw new ArgumentNullException(nameof(label));
        OnSelected = onSelected;
    }
}

public class OverlayPopupService : IPopupService
{
    /// <summary>
    /// MDRG Connection layer
    /// </summary>
    private Il2Cpp.UiOverlay uiOverlay { get => Il2Cpp.UiOverlay.Instance; }

    /// <inheritdoc/>
    public void ChoicePopup(string title, string message, IPopupChoice[] choices)
    {
        // A single allocation, shouldn't be that bad
        var il2CppChoices = new Il2CppSystem.Collections.Generic.List<Il2Cpp.PopupChoice>(choices.Length);

        foreach (var c in choices)
            il2CppChoices.Add(new Il2Cpp.PopupChoice(c.Label, c.OnSelected));
        
        uiOverlay.Popup(title, message, il2CppChoices);
    }
    public void SimplePopup(string title, string message, Action onDismiss = null)
    {
        uiOverlay.OkPopup(title, message, onDismiss);
    }
}
