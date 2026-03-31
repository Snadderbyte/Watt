using Watt.Core;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Watt.UI.Tools ;

internal class DrfView : IToolView
{
    public string Id => "T0001";
    public string Name => "Duplicate Row Finder";

    public View CreateView(AppState state)
    {
        return new Label
        {
            Text = "Duplicate Row Finder - Main View",
            X = 1,
            Y = 1,
            Width = Dim.Fill(2),
            Height = Dim.Fill(2),
        };
    }

    public View CreteToolbarView(AppState state)
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