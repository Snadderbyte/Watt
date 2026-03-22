using Watt.Core;
using Terminal.Gui;

namespace Watt.Tools;

internal class DataverseInspector : ITool
{
    public string Id => "dataverse_inspector";
    public string Name => "Dataverse Inspector";

    public View CreateView(AppState state)
    {
        var label = new Label("Dataverse Inspector");
        return label;
    }

    public void OnActivated()
    {
        // Handle activation logic here
    }

    public void OnDeactivated()
    {
        // Handle deactivation logic here
    }
}
