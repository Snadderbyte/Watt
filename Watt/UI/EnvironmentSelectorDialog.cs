using System.Collections.ObjectModel;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Watt.Core.Authentication;

namespace Watt.UI;

/// <summary>
/// Dialog for selecting and managing Dataverse environments.
/// </summary>
public class EnvironmentSelectorDialog : Dialog
{
    private readonly IApplication _app;
    private readonly Action<Dialog> _runDialog;
    private readonly AuthenticationService _authService;
    private readonly DataverseConnectionManager _connectionManager;
    private ListView? _environmentList;
    private Label? _statusLabel;
    private List<EnvironmentDetails> _environments;

    public EnvironmentSelectorDialog(
        IApplication app,
        Action<Dialog> runDialog,
        AuthenticationService authService,
        DataverseConnectionManager connectionManager)
    {
        _app = app;
        _runDialog = runDialog;
        _authService = authService;
        _connectionManager = connectionManager;
        _environments = _authService.GetAllEnvironments().ToList();

        InitializeUi();
    }

    private void InitializeUi()
    {
        Title = "Select Environment";
        Width = 60;
        Height = 20;

        var environmentNames = new ObservableCollection<string>(_environments.Select(e => 
            $"{e.Name} ({e.AuthMethod}) - {(e.IsAuthenticated ? "✓" : "✗")}").ToList());

        _environmentList = new ListView()
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(1),
            Height = Dim.Fill(4)
        };
        _environmentList.SetSource(environmentNames);
        Add(_environmentList);

        _statusLabel = new Label()
        {
            Text = "Select an environment",
            X = 1,
            Y = Pos.Bottom(_environmentList) + 1,
            Width = Dim.Fill(1)
        };
        Add(_statusLabel);

        // Buttons
        var addButton = new Button()
        {
            Text = "Add",
            X = 1,
            Y = Pos.Bottom(_statusLabel) + 1
        };
        addButton.Accepting += (s, e) => AddEnvironment();
        Add(addButton);

        var connectButton = new Button()
        {
            Text = "Connect",
            X = Pos.Right(addButton) + 1,
            Y = Pos.Bottom(_statusLabel) + 1
        };
        connectButton.Accepting += async (s, e) => await ConnectToEnvironment();
        Add(connectButton);

        var deleteButton = new Button()
        {
            Text = "Delete",
            X = Pos.Right(connectButton) + 1,
            Y = Pos.Bottom(_statusLabel) + 1
        };
        deleteButton.Accepting += async (s, e) => await DeleteEnvironment();
        Add(deleteButton);

        var closeButton = new Button()
        {
            Text = "Close",
            X = Pos.Right(deleteButton) + 1,
            Y = Pos.Bottom(_statusLabel) + 1
        };
        closeButton.Accepting += (s, e) => RequestStop();
        Add(closeButton);
    }

    private void AddEnvironment()
    {
        var dialog = new AddEnvironmentDialog(_app, _runDialog, _authService, _connectionManager);
        _runDialog(dialog);
        dialog.Dispose();

        // Refresh the list
        _environments = _authService.GetAllEnvironments().ToList();
        var environmentNames = new ObservableCollection<string>(_environments.Select(e =>
            $"{e.Name} ({e.AuthMethod}) - {(e.IsAuthenticated ? "✓" : "✗")}").ToList());
        
        _environmentList!.SetSource<string>(environmentNames);
    }

    private async Task ConnectToEnvironment()
    {
        int? selectedIndex = _environmentList!.SelectedItem;
        if (selectedIndex is null or < 0 || selectedIndex >= _environments.Count)
        {
            _statusLabel!.Text = "Please select an environment";
            return;
        }

        var selectedEnvironment = _environments[selectedIndex.Value];
        _statusLabel!.Text = $"Connecting to {selectedEnvironment.Name}...";

        try
        {
            var isValid = await _authService.ValidateCredentialsAsync(selectedEnvironment.Id);
            if (isValid)
            {
                var connection = await _connectionManager.GetConnectionAsync(selectedEnvironment.Id);
                if (connection != null)
                {
                    _statusLabel.Text = $"Connected to {selectedEnvironment.Name}";
                    RequestStop();
                }
                else
                {
                    _statusLabel.Text = "Failed to establish connection";
                }
            }
            else
            {
                _statusLabel.Text = "Credentials invalid or expired";
            }
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"Error: {ex.Message}";
        }
    }

    private async Task DeleteEnvironment()
    {
        int? selectedIndex = _environmentList!.SelectedItem;
        if (selectedIndex is null or < 0 || selectedIndex >= _environments.Count)
        {
            _statusLabel!.Text = "Please select an environment";
            return;
        }

        var selectedEnvironment = _environments[selectedIndex.Value];
        
        if (MessageBox.Query(_app, "Confirm Delete", 
            $"Delete environment '{selectedEnvironment.Name}'?", "Yes", "No") == 0)
        {
            await _authService.DeleteEnvironmentAsync(selectedEnvironment.Id);
            _environments.Remove(selectedEnvironment);
            
            var environmentNames = _environments.Select(e =>
                $"{e.Name} ({e.AuthMethod}) - {(e.IsAuthenticated ? "✓" : "✗")}").ToList();
            
            _environmentList.SetSource<string>(new ObservableCollection<string>(environmentNames));
            _statusLabel!.Text = "Environment deleted";
        }
    }
}
