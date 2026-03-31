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
    private readonly Label _environmentStatusLabel;

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
        Width = Dim.Fill();
        Height = 3;

        _environmentStatusLabel = new Label()
        {
            Text = "Environment",
            X = 1,
            Y = 0,
            Width = 50
        };

        var selectEnvironmentButton = new Button()
        {
            Text = "Select Environment",
            X = Pos.Right(_environmentStatusLabel),
            Y = 0
        };
        selectEnvironmentButton.Accepting += OnSelectEnvironment;

        Add(_environmentStatusLabel, selectEnvironmentButton);
        RefreshEnvironmentStatus();
    }

    private void OnSelectEnvironment(object? sender, EventArgs e)
    {
        var envDialog = new EnvironmentSelectorDialog(_app, dialog => _app.Run(dialog), _authService, _connectionManager);
        _app.Run(envDialog);
        envDialog.Dispose();

        RefreshEnvironmentStatus();
    }

    private void RefreshEnvironmentStatus()
    {
        _environmentStatusLabel.Text = "Environment";

        if (string.IsNullOrEmpty(_appState.CurrentEnvironmentId))
            return;

        var environment = _authService.GetEnvironment(_appState.CurrentEnvironmentId);
        if (environment != null)
            _environmentStatusLabel.Text = $"Connected: {environment.Name}";
    }
}
