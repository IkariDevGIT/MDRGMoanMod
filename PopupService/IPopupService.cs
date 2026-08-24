namespace MoanMod.PopupService;

/// <summary>
/// An interface representing... A popup choice x)
/// </summary>
public interface IPopupChoice
{
    /// <summary>
    /// Text shown on the button
    /// </summary>
    string Label { get; }          
    /// <summary>
    /// Action executed when this choice is selected
    /// </summary>
    Action OnSelected { get; }     
}


public interface IPopupService
{
    /// <summary>
    /// Show a simple dismissible popup
    /// </summary>
    /// <param name="title">The title of the popup</param>
    /// <param name="message">The body of the popup</param>
    /// <param name="onDismiss">Callback called when dismissed</param>
    void SimplePopup(string title, string message, Action onDismiss = null);
        
    /// <summary>
    /// Show a popup with multiple choices.
    /// </summary>
    void ChoicePopup(string title, string message, IPopupChoice[] choices);
}
