using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Watt.Core;

namespace Watt.UI.Tools;

internal class InspectorView(AppState appState) : IToolView
{
    public string Id => "T0002";
    public string Name => "Inspector";
    public AppState AppState { get; set; } = appState;

    public View CreateView(AppState state)
    {
        return new Label
        {
            Text = "Inspector - Main View",
            X = 1,
            Y = 1,
            Width = Dim.Fill(2),
            Height = Dim.Fill(2),
        };
    }

    public View CreateToolbarView(AppState state)
    {
        return new Label
        {
            Text = "Toolbar - (Add buttons here)",
            X = 1,
            Y = 0,
            Width = Dim.Fill(2),
            Height = 1,
        };
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