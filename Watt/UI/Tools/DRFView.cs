using Watt.Core;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Watt.Tools.DRF;
using System.Collections.ObjectModel;

namespace Watt.UI.Tools;

internal class DrfView : IToolView
{
    public string Name { get; } = "Duplicate Row Finder";
    public View View { get; set; }
    public readonly AppState AppState;
    private readonly DrfTool _drfTool;

    private TextView? _loadingView;
    private ListView? _entityList;

    public DrfView(AppState appState)
    {
        AppState = appState;
        _drfTool = new DrfTool(appState);
        View = new View()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill() - 1,
        };
    }

    public void InitializeUi()
    {
        var searchBar = new TextField
        {
            X = 1,
            Y = 1,
            Width = 30,
            Text = "Search entities..."
        };

        _loadingView = new TextView
        {
            Text = "Loading entities...",
            X = 1,
            Y = Pos.Bottom(searchBar),
            Width = Dim.Fill(2),
            Height = Dim.Fill(),
            ReadOnly = true,
            WordWrap = true,
        };

        _entityList = new ListView
        {
            X = 1,
            Y = Pos.Bottom(searchBar),
            Width = Dim.Fill(2),
            Height = Dim.Fill(),
            Visible = false,
        };

        View.Add(searchBar, _loadingView, _entityList);
    }

    public async Task LoadAsync()
    {
        if (_loadingView is null || _entityList is null || View is null)
            return;

        if (AppState.ServiceClient is not { IsReady: true })
        {
            _loadingView.Text = "No connection. Please select an environment first.";
            _loadingView.Visible = true;
            _entityList.Visible = false;
            return;
        }

        try
        {
            var entities = await _drfTool.GetAllEntitiesAsync();
            var entityNames = new ObservableCollection<string>(
                entities.ConvertAll(en => en.LogicalName));

            _entityList.SetSource<string>(entityNames);
            _loadingView.Visible = false;
            _entityList.Visible = true;
            View.SetNeedsDraw();
        }
        catch (Exception ex)
        {
            _loadingView.Text = $"Error: {ex.Message}";
        }
    }

    private async Task OnEntitySelected()
    {
        if (_entityList is null || _loadingView is null)
            return;
        int? selectedIndex = _entityList.SelectedItem;
        if (selectedIndex is null or < 0)
            return;
        var entityName = _entityList.Source.ToList()[selectedIndex.Value] as string;
        if (string.IsNullOrEmpty(entityName))
            return;
        var entityMetadata = await _drfTool.GetEntityMetadataAsync(entityName);
    }
}