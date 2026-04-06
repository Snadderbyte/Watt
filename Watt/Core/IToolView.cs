using Terminal.Gui.ViewBase;

namespace Watt.Core;

public interface IToolView
{
    public View View { get; set; }
    /// <summary>
    /// The title of the view, used for display in the UI.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Initializes the UI components of the view. This is called when the view is first created.
    /// </summary>
    void InitializeUi();

    /// <summary>
    /// Loads any necessary data or state for the view. This can be called when the view is selected or needs to refresh its content.
    /// </summary>
    Task LoadAsync();
}