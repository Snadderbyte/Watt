using Watt.Core;
using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Watt.Tools.DRF;
using System.Collections.ObjectModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Label = Terminal.Gui.Views.Label;

namespace Watt.UI.Tools;

internal class DrfView : IToolView
{
    public string Name { get; } = "Duplicate Row Finder";
    public View View { get; set; }
    public HelpDialog HelpDialog { get; set; } = new HelpDialog();
    public readonly AppState AppState;
    private readonly DrfTool _drfTool;
    private Dialog? _loadingDialog;
    private FrameView? _entityFrame;
    private ListView? _entityList;
    private FrameView? _attributeFrame;
    private TableView? _attributeTable;
    private FrameView? _selectedAttributesFrame;
    private ListView? _selectedAttributesList;
    private ObservableCollection<string> _selectedAttributes = new ObservableCollection<string>();
    private FrameView? _resultFrame;
    private TableView? _resultTable;
    private TableView? _resultTableExpanded;
    private List<DrfTool.DuplicateGroup> _duplicateGroups = new();
    private string _selectedEntity = "";

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
            CanFocus = true,
        };
    }

    public void InitializeUi()
    {
        _entityFrame = new FrameView
        {
            X = 0,
            Y = 0,
            Width = Dim.Percent(25),
            Height = Dim.Percent(40),
            Title = "Entities",
        };
        _attributeFrame = new FrameView
        {
            X = Pos.Right(_entityFrame),
            Y = 0,
            Width = Dim.Percent(55),
            Height = Dim.Percent(40),
            Title = "Attributes",
        };
        _selectedAttributesFrame = new FrameView
        {
            X = Pos.Right(_attributeFrame),
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Percent(40),
            Title = "Selected Attributes",
        };
        _selectedAttributesList = new ListView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Source = new ListWrapper<string>(_selectedAttributes),
        };
        _selectedAttributesFrame.Add(_selectedAttributesList);

        _resultFrame = new FrameView
        {
            X = 0,
            Y = Pos.Bottom(_entityFrame),
            Width = Dim.Fill(),
            Height = Dim.Fill() - 1,
            Title = "Results",
        };
        var findDuplicatesButton = new Button
        {
            Text = "Search for Duplicates",
            X = 0,
            Y = 0,
        };
        findDuplicatesButton.Accepted += async (s, e) =>
        {
            if (string.IsNullOrEmpty(_selectedEntity) || _selectedAttributes.Count == 0)
            {
                MessageBox.ErrorQuery(View.App, "Please select an entity and at least one attribute.", "OK");
                return;
            }
            EnableLoadingSpinner();
            var attributes = _selectedAttributes.ToList();
            _duplicateGroups = await _drfTool.FindDuplicatesAsync(_selectedEntity, attributes);
            DisableLoadingSpinner();

            var columnDefinitions = new Dictionary<string, Func<DrfTool.DuplicateGroup, object>>();
            foreach (var attr in attributes)
            {
                var captured = attr;
                columnDefinitions[captured] = g => g.AttributeValues.GetValueOrDefault(captured, "");
            }
            columnDefinitions["Duplicate Count"] = g => g.DuplicateCount;
            _resultTable!.Table = new EnumerableTableSource<DrfTool.DuplicateGroup>(_duplicateGroups, columnDefinitions);
            _resultTableExpanded!.Table = null;
        };
        _resultTable = new TableView
        {
            X = 0,
            Y = Pos.Bottom(findDuplicatesButton),
            Width = Dim.Auto(),
            Height = Dim.Fill(),
        };
        _resultTable.CellActivated += (s, e) =>
        {
            if (e.Row >= 0 && e.Row < _duplicateGroups.Count)
                PopulateExpandedTable(_duplicateGroups[e.Row]);
        };
        _resultTableExpanded = new TableView
        {
            X = Pos.Right(_resultTable),
            Y = Pos.Bottom(findDuplicatesButton),
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        _resultFrame.Add(findDuplicatesButton, _resultTable, _resultTableExpanded);

        _entityFrame.Initialized += (s, e) => InitializeEntityList();
        View.Add(_entityFrame, _attributeFrame, _selectedAttributesFrame, _resultFrame);

        foreach (var frame in new[] { _entityFrame, _attributeFrame, _selectedAttributesFrame, _resultFrame })
        {
            frame.KeyDown += (s, e) =>
            {
                if (e == Key.Tab)
                {
                    (s as View)?.App?.Navigation?.AdvanceFocus(NavigationDirection.Forward, TabBehavior.TabGroup);
                    e.Handled = true;
                }
                else if (e == Key.Tab.WithShift)
                {
                    (s as View)?.App?.Navigation?.AdvanceFocus(NavigationDirection.Backward, TabBehavior.TabGroup);
                    e.Handled = true;
                }
            };
        }
    }
    public void InitializeHeplDialog()
    {
        
    }

    public async Task LoadAsync()
    {
    }
    public async Task RefreshAsync()
    {
        await LoadAsync();
    }
    public void EnableLoadingSpinner()
    {
        var spinner = new SpinnerView
        {
            X = Pos.Center(),
            Y = 1,
            AutoSpin = true,
        };
        var loadingLabel = new Label
        {
            Text = $"Loading {"test"}...",
            X = Pos.Center(),
            Y = 2,
        };
        _loadingDialog = new Dialog
        {
            Title = "Please Wait",
            X = Pos.Center(),
            Y = Pos.Center(),
            Width = 30,
            Height = 5,
        };
        _loadingDialog.Add(spinner, loadingLabel);
    }

    public void DisableLoadingSpinner()
    {
        _loadingDialog?.Dispose();
        _loadingDialog = null;
    }

    public void InitializeEntityList()
    {
        var entitySearchBar = new TextField
        {
            X = 0,
            Y = 0,
            Width = 30,
            Text = "Search entities...",
        };

        entitySearchBar.Accepted += async (s, e) =>
        {
            if (_entityList != null)
            {
                _entityList.Visible = false;
                _entityFrame?.Remove(_entityList);
                _entityList.Dispose();
                _entityList = null;
            }
            _entityList = new ListView
            {
                X = 0,
                Y = Pos.Bottom(entitySearchBar),
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };
            var entities = await _drfTool.GetEntitiesAsync(entitySearchBar.Text.ToString());
            var entityLogicalNames = new ObservableCollection<string>(
                entities.ConvertAll(en => en.LogicalName));

            _entityList.SetSource(entityLogicalNames);

            _entityList.Accepted += (s, e) =>
            {
                var selectedIndex = _entityList.SelectedItem;
                var source = _entityList.Source?.ToList();
                var selectedEntity = source != null && selectedIndex >= 0 && selectedIndex < source.Count
                    ? source[selectedIndex ?? -1]?.ToString() ?? ""
                    : "";
                if (!string.IsNullOrEmpty(selectedEntity))
                {
                    _selectedEntity = selectedEntity;
                    var attributes = _drfTool.GetAttributesAsync(_selectedEntity).Result;
                    InitializeAttributeList(attributes);
                }
                _selectedAttributes.Clear();
            };
            _entityFrame?.Add(_entityList);
        };
        _entityFrame?.Add(entitySearchBar);
    }
    public void InitializeAttributeList(List<AttributeMetadata> attributes)
    {
        if (_attributeTable != null)
        {
            _attributeTable.Visible = false;
            _attributeFrame?.Remove(_attributeTable);
            _attributeTable.Dispose();
            _attributeTable = null;
        }
        var searchBar = new TextField
        {
            X = 0,
            Y = 0,
            Width = 30,
            Text = "Search attributes...",
        };
        _attributeTable = new TableView
        {
            X = 0,
            Y = Pos.Bottom(searchBar),
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        var columnDefinitions = new Dictionary<string, Func<AttributeMetadata, object>>()
        {
            { "Logical Name", a => a.LogicalName },
            { "Display Name", a => a.DisplayName?.UserLocalizedLabel?.Label ?? "" },
            { "Type", a => a.AttributeType?.ToString() ?? "unknown" },
        };
        _attributeTable.Table = new EnumerableTableSource<AttributeMetadata>(attributes, columnDefinitions);

        _attributeTable.CellActivated += (s, entity) =>
        {
            if (entity.Row >= 0 && entity.Row < attributes.Count)
            {
                var selectedAttribute = attributes[entity.Row];
                AddSelectedAttribute(selectedAttribute.LogicalName);
            }
        };

        _attributeFrame?.Add(searchBar, _attributeTable);
    }

    public void AddSelectedAttribute(string attributeLogicalName)
    {
        if (_selectedAttributes.Contains(attributeLogicalName))
        {
            _selectedAttributes.Remove(attributeLogicalName);
        } else
        {
            _selectedAttributes.Add(attributeLogicalName);
        }
    }

    private void PopulateExpandedTable(DrfTool.DuplicateGroup group)
    {
        var columnDefinitions = new Dictionary<string, Func<Entity, object>>
        {
            { "Record ID", e => e.Id },
        };
        foreach (var attr in group.AttributeValues.Keys)
        {
            var captured = attr;
            columnDefinitions[captured] = entity => entity.Contains(captured) ? DrfTool.GetAttributeStringValue(entity[captured]) : "";
        }
        _resultTableExpanded!.Table = new EnumerableTableSource<Entity>(group.Records, columnDefinitions);
    }
}