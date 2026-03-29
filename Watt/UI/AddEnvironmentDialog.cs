using System;
using System.Threading.Tasks;
using NStack;
using Terminal.Gui;
using Watt.Core.Authentication;

namespace Watt.UI;

/// <summary>
/// Dialog for adding a new Dataverse environment and authenticating.
/// </summary>
public class AddEnvironmentDialog : Dialog
{
    private readonly AuthenticationService _authService;
    private readonly DataverseConnectionManager _connectionManager;
    private Label? _instructionsLabel;

    public AddEnvironmentDialog(
        AuthenticationService authService,
        DataverseConnectionManager connectionManager)
    {
        _authService = authService;
        _connectionManager = connectionManager;

        InitializeUI();
    }

    private void InitializeUI()
    {
        Title = "Add New Environment";
        Width = 70;
        Height = 12;

        _instructionsLabel = new Label("Enter environment details below:")
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(1)
        };
        Add(_instructionsLabel);

        var nameLabel = new Label("Environment Name:")
        {
            X = 1,
            Y = Pos.Bottom(_instructionsLabel) + 1
        };
        Add(nameLabel);

        var nameField = new TextField("")
        {
            X = Pos.Right(nameLabel) + 1,
            Y = Pos.Top(nameLabel),
            Width = 30
        };
        Add(nameField);

        var urlLabel = new Label("Org URL:")
        {
            X = 1,
            Y = Pos.Bottom(nameField) + 1
        };
        Add(urlLabel);

        var urlField = new TextField("https://")
        {
            X = Pos.Right(urlLabel) + 1,
            Y = Pos.Top(urlLabel),
            Width = 40
        };
        Add(urlField);

        var authMethodLabel = new Label("Auth Method:")
        {
            X = 1,
            Y = Pos.Bottom(urlField) + 1
        };
        Add(authMethodLabel);

        var authMethods = new ustring[] { "OAuth", "Client Secret", "Username/Password" };
        var authRadioGroup = new RadioGroup(authMethods)
        {
            X = Pos.Right(authMethodLabel) + 1,
            Y = Pos.Top(authMethodLabel)
        };
        Add(authRadioGroup);

        var nextButton = new Button("Next")
        {
            X = 2,
            Y = Pos.Bottom(authRadioGroup) + 1
        };
        nextButton.Clicked += async () =>
        {
            if (string.IsNullOrWhiteSpace(nameField.Text.ToString()))
            {
                MessageBox.ErrorQuery("Validation Error", "Environment name is required", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(urlField.Text.ToString()) || !urlField.Text.ToString()!.StartsWith("https://"))
            {
                MessageBox.ErrorQuery("Validation Error", "Valid organization URL is required", "OK");
                return;
            }

            var environmentId = Guid.NewGuid().ToString();
            var environment = new EnvironmentDetails
            {
                Id = environmentId,
                Name = nameField.Text.ToString()!,
                OrgUrl = urlField.Text.ToString()!,
                AuthMethod = (AuthenticationMethod)authRadioGroup.SelectedItem
            };

            await _authService.RegisterEnvironmentAsync(environment);

            var authDialog = new AuthenticationDialog(_authService, _connectionManager, environment);
            Application.Run(authDialog);

            RequestStop();
        };
        Add(nextButton);

        var cancelButton = new Button("Cancel")
        {
            X = Pos.Right(nextButton) + 2,
            Y = Pos.Top(nextButton)
        };
        cancelButton.Clicked += () => RequestStop();
        Add(cancelButton);
    }
}
