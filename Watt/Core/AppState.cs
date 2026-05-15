using Microsoft.PowerPlatform.Dataverse.Client;
using Terminal.Gui.ViewBase;
using Watt.Core.Authentication;
using Watt.UI.Tools;

namespace Watt.Core;

/// <summary>
/// Represents the global application state.
/// </summary>
public class AppState
{

    /// <summary>
    /// The current active Dataverse connection.
    /// </summary>
    public ServiceClient? ServiceClient { get; set; }

    /// <summary>
    /// Connection manager for Dataverse.
    /// </summary>
    public required DataverseConnectionManager ConnectionManager { get; set; }

    /// <summary>
    /// The currently selected tool view in the UI. This can be used to track which tool is active and manage state accordingly.
    /// </summary>
    public IToolView? SelectedTool { get; set; }
}
