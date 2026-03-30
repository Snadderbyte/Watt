using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Watt.Core.Authentication;

namespace Watt.UI;

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
        Width = 70;
        Height = 12;

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
        nextButton.Accepting += async (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(nameField.Text.ToString()))
            {
                MessageBox.ErrorQuery(_app, "Validation Error", "Environment name is required", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(urlField.Text.ToString()) || !urlField.Text.ToString()!.StartsWith("https://"))
            {
                MessageBox.ErrorQuery(_app, "Validation Error", "Valid organization URL is required", "OK");
                return;
            }

            var environmentId = Guid.NewGuid().ToString();
            var environment = new EnvironmentDetails
            {
                Id = environmentId,
                Name = nameField.Text.ToString()!,
                OrgUrl = urlField.Text.ToString()!,
                AuthMethod = (AuthenticationMethod)(authOptionSelector.Value ?? 0)
            };

            await _authService.RegisterEnvironmentAsync(environment);

            var authDialog = new AuthenticationDialog(_authService, _connectionManager, environment);
            _runDialog(authDialog);
            authDialog.Dispose();

            RequestStop();
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
}
