using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Gui;
using Watt.Core.Authentication;

namespace Watt.UI;

/// <summary>
/// Dialog for selecting and managing Dataverse environments.
/// </summary>
public class EnvironmentSelectorDialog : Dialog
{
    private readonly AuthenticationService _authService;
    private readonly DataverseConnectionManager _connectionManager;
    private ListView? _environmentList;
    private Label? _statusLabel;
    private List<EnvironmentDetails> _environments;

    public EnvironmentSelectorDialog(
        AuthenticationService authService,
        DataverseConnectionManager connectionManager)
    {
        _authService = authService;
        _connectionManager = connectionManager;
        _environments = _authService.GetAllEnvironments().ToList();

        InitializeUI();
    }

    private void InitializeUI()
    {
        Title = "Select Environment";
        Width = 60;
        Height = 20;

        var environmentNames = _environments.Select(e => 
            $"{e.Name} ({e.AuthMethod}) - {(e.IsAuthenticated ? "✓" : "✗")}").ToList();

        _environmentList = new ListView(environmentNames)
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(1),
            Height = Dim.Fill(4)
        };
        Add(_environmentList);

        _statusLabel = new Label("Select an environment")
        {
            X = 1,
            Y = Pos.Bottom(_environmentList) + 1,
            Width = Dim.Fill(1)
        };
        Add(_statusLabel);

        // Buttons
        var addButton = new Button("Add")
        {
            X = 1,
            Y = Pos.Bottom(_statusLabel) + 1
        };
        addButton.Clicked += AddEnvironment;
        Add(addButton);

        var connectButton = new Button("Connect")
        {
            X = Pos.Right(addButton) + 1,
            Y = Pos.Bottom(_statusLabel) + 1
        };
        connectButton.Clicked += ConnectToEnvironment;
        Add(connectButton);

        var deleteButton = new Button("Delete")
        {
            X = Pos.Right(connectButton) + 1,
            Y = Pos.Bottom(_statusLabel) + 1
        };
        deleteButton.Clicked += DeleteEnvironment;
        Add(deleteButton);

        var closeButton = new Button("Close")
        {
            X = Pos.Right(deleteButton) + 1,
            Y = Pos.Bottom(_statusLabel) + 1
        };
        closeButton.Clicked += () => RequestStop();
        Add(closeButton);
    }

    private void AddEnvironment()
    {
        var dialog = new AddEnvironmentDialog(_authService, _connectionManager);
        Application.Run(dialog);

        // Refresh the list
        _environments = _authService.GetAllEnvironments().ToList();
        var environmentNames = _environments.Select(e =>
            $"{e.Name} ({e.AuthMethod}) - {(e.IsAuthenticated ? "✓" : "✗")}").ToList();
        
        _environmentList!.SetSource(environmentNames);
    }

    private async void ConnectToEnvironment()
    {
        if (_environmentList!.SelectedItem < 0 || _environmentList.SelectedItem >= _environments.Count)
        {
            _statusLabel!.Text = "Please select an environment";
            return;
        }

        var selectedEnvironment = _environments[_environmentList.SelectedItem];
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
                    System.Threading.Thread.Sleep(1000);
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

    private async void DeleteEnvironment()
    {
        if (_environmentList!.SelectedItem < 0 || _environmentList.SelectedItem >= _environments.Count)
        {
            _statusLabel!.Text = "Please select an environment";
            return;
        }

        var selectedEnvironment = _environments[_environmentList.SelectedItem];
        
        if (MessageBox.Query("Confirm Delete", 
            $"Delete environment '{selectedEnvironment.Name}'?", "Yes", "No") == 0)
        {
            await _authService.DeleteEnvironmentAsync(selectedEnvironment.Id);
            _environments.Remove(selectedEnvironment);
            
            var environmentNames = _environments.Select(e =>
                $"{e.Name} ({e.AuthMethod}) - {(e.IsAuthenticated ? "✓" : "✗")}").ToList();
            
            _environmentList.SetSource(environmentNames);
            _statusLabel!.Text = "Environment deleted";
        }
    }
}
