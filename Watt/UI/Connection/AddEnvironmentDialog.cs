using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Watt.Core.Authentication;

namespace Watt.UI.Connection;

/// <summary>
/// Dialog for adding a new Dataverse environment and authenticating.
/// </summary>
public class AddEnvironmentDialog : Dialog
{
    private readonly IApplication _app;
    private readonly Action<Dialog> _runDialog;
    private readonly AuthenticationService _authService;
    private readonly DataverseConnectionManager _connectionManager;
    private Label? _instructionsLabel;
    private bool _isSubmitting;

    public AddEnvironmentDialog(
        IApplication app,
        Action<Dialog> runDialog,
        AuthenticationService authService,
        DataverseConnectionManager connectionManager)
    {
        _app = app;
        _runDialog = runDialog;
        _authService = authService;
        _connectionManager = connectionManager;

        InitializeUI();
    }

    private void InitializeUI()
    {
        Title = "Add New Environment";
        Width = Dim.Auto();
        Height = Dim.Auto();

        _instructionsLabel = new Label()
        {
            Text = "Enter environment details below:",
            X = 1,
            Y = 1,
            Width = Dim.Fill()
        };  
        Add(_instructionsLabel);

        var nameLabel = new Label()
        {
            Text = "Environment Name:",
            X = 1,
            Y = Pos.Bottom(_instructionsLabel) + 1
        };
        Add(nameLabel);

        var nameField = new TextField()
        {
            Text = "",
            X = Pos.Right(nameLabel) + 1,
            Y = Pos.Top(nameLabel),
            Width = 30
        };
        Add(nameField);

        var urlLabel = new Label()
        {
            Text = "Org URL:",
            X = 1,
            Y = Pos.Bottom(nameField) + 1
        };
        Add(urlLabel);

        var urlField = new TextField()
        {
            Text = "https://",
            X = Pos.Right(urlLabel) + 1,
            Y = Pos.Top(urlLabel),
            Width = 40
        };
        Add(urlField);
        
        var authMethodLabel = new Label()
        {
            Text = "Auth Method:",
            X = 1,
            Y = Pos.Bottom(urlField) + 1
        };
        Add(authMethodLabel);
        var authMethods = new string[] { "OAuth", "Client Secret", "Username/Password" };
        var authOptionSelector = new OptionSelector()
        {
            Labels = authMethods,
            X = Pos.Right(authMethodLabel) + 1,
            Y = Pos.Top(authMethodLabel)
        };
        Add(authOptionSelector);

        var nextButton = new Button()
        {
            Text = "Next",
            X = 2,
            Y = Pos.Bottom(authOptionSelector) + 1
        };
        nextButton.Accepting += (s, e) =>
        {
            e.Handled = true;

            if (_isSubmitting)
            {
                return;
            }

            _ = SubmitEnvironmentAsync(
                nameField.Text.ToString(),
                urlField.Text.ToString(),
                authOptionSelector.Value);
        };
        Add(nextButton);

        var cancelButton = new Button()
        {
            Text = "Cancel",
            X = Pos.Right(nextButton) + 2,
            Y = Pos.Top(nextButton)
        };
        cancelButton.Accepting += (s, e) => RequestStop();
        Add(cancelButton);
    }

    private async Task SubmitEnvironmentAsync(string? name, string? orgUrl, int? authMethodValue)
    {
        if (_isSubmitting)
        {
            return;
        }

        _isSubmitting = true;

        try
        {
            await SubmitEnvironment(name, orgUrl, authMethodValue);
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private async Task SubmitEnvironment(string? name, string? orgUrl, int? authMethodValue)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.ErrorQuery(_app, "Validation Error", "Environment name is required", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(orgUrl) || !orgUrl.StartsWith("https://"))
        {
            MessageBox.ErrorQuery(_app, "Validation Error", "Valid organization URL is required", "OK");
            return;
        }

        var environmentId = Guid.NewGuid().ToString();
        var environment = new EnvironmentDetails
        {
            Id = environmentId,
            Name = name,
            OrgUrl = orgUrl,
            AuthMethod = (AuthenticationMethod)(authMethodValue ?? 0)
        };

        await _authService.RegisterEnvironmentAsync(environment);

        var authDialog = new AuthenticationDialog(_authService, _connectionManager, environment);
        _runDialog(authDialog);
        bool authenticationSucceeded = authDialog.AuthenticationSucceeded || environment.IsAuthenticated;
        authDialog.Dispose();

        if (authenticationSucceeded)
        {
            RequestStop();
        }
    }
}
