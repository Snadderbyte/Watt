using System.Runtime.CompilerServices;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Watt.Core;
using Watt.Core.Authentication;

namespace Watt.UI.Connection;

/// <summary>
/// Top bar containing the current environment status and environment selector action.
/// </summary>
public class TopBarView : FrameView
{
    private readonly IApplication _app;
    private readonly AppState _appState;
    private readonly AuthenticationService _authService;
    private readonly DataverseConnectionManager _connectionManager;
    private readonly Button _selectEnvironmentButton;

    public TopBarView(
        IApplication app,
        AppState appState,
        AuthenticationService authService,
        DataverseConnectionManager connectionManager)
    {
        _app = app;
        _appState = appState;
        _authService = authService;
        _connectionManager = connectionManager;

        X = 0;
        Y = 0;
        Width = 25;
        Height = 3;
        Title = "Environment";

        _selectEnvironmentButton = new Button()
        {
            X = 0,
            Y = 0
        };
        _selectEnvironmentButton.Accepting += OnSelectEnvironment;

        Add(_selectEnvironmentButton);
        RefreshEnvironmentStatus();
    }

    private void OnSelectEnvironment(object? sender, EventArgs e)
    {
        var envDialog = new EnvironmentSelectorDialog(_app, dialog => _app.Run(dialog), _authService, _connectionManager, _appState);
        _app.Run(envDialog);
        envDialog.Dispose();

        RefreshEnvironmentStatus();
    }

    private void RefreshEnvironmentStatus()
    {
        if (string.IsNullOrEmpty(_appState.CurrentEnvironmentId))
        {
            _selectEnvironmentButton.Text = "Select Environment";
            return;
        }

        var environment = _authService.GetEnvironment(_appState.CurrentEnvironmentId);
        if (environment != null)
            _selectEnvironmentButton.Text = environment.Name;
    }
}
