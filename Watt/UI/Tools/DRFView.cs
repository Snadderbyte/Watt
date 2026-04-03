using Watt.Core;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Watt.Tools.DRF;
using System.Collections.ObjectModel;

namespace Watt.UI.Tools;

internal class DrfView(AppState appState) : IToolView
{
    public string Id => "T0001";
    public string Name => "Duplicate Row Finder";
    public AppState AppState { get; set; } = appState;

    private DrfTool _drfTool = new(appState);

    public View CreateView(AppState state)
    {
        var loadingView = new TextView
        {
            Text = "Loading entities...",
            X = 1,
            Y = 1,
            Width = Dim.Fill(2),
            Height = Dim.Fill(2),
            ReadOnly = true,
            WordWrap = true,
        };

        var listView = new ListView
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(2),
            Height = Dim.Fill(2),
            Visible = false,
        };

        var container = new View
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };

        container.Add(loadingView, listView);

        container.Initialized += async (s, e) =>
        {
            try
            {
                var entities = await _drfTool.GetAllEntitiesAsync();
                var entityNames = new ObservableCollection<string>(
                    entities.ConvertAll(en => en.LogicalName));

                listView.SetSource<string>(entityNames);
                loadingView.Visible = false;
                listView.Visible = true;
                container.SetNeedsDraw();
            }
            catch (Exception ex)
            {
                loadingView.Text = $"Error: {ex.Message}";
            }
        };

        return container;
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