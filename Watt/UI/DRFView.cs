using Watt.Core;
using Terminal.Gui;

namespace Watt.UI.DRF;

internal class DRFView:IToolView
{
    public string Id => "T0001";
    public string Name => "Duplicate Row Finder";
    public View CreateView(AppState state)
    {
        return new Label("DRF View - Displaying data from the DRF environment.");
    }
    public void OnActivated()
    {
        // Not implemented for this example, but you could add logic here to refresh data or set up the view when it's activated.
    }
    public void OnDeactivated()
    {
        // Not implemented for this example, but you could add logic here to clean up resources or save state when the view is deactivated.
    }
}
