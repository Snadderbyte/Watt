using System;
using System.Threading.Tasks;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Watt.Core.Authentication;

namespace Watt.UI.Connection;

public class AddEnvironmentDialog : Dialog
{
    private readonly IApplication _app;
    private readonly Action<Dialog> _runDialog;
    private readonly AuthenticationService _authService;
    private bool _isSubmitting;

    public AddEnvironmentDialog(
        IApplication app,
        Action<Dialog> runDialog,
        AuthenticationService authService)
    {
        _app = app;
        _runDialog = runDialog;
        _authService = authService;

        InitializeUi();
    }

    private void InitializeUi()
    {
        Title  = "Add New Environment";
        Width  = Dim.Auto();
        Height = Dim.Auto();

        var instructionsLabel = new Label()
        {
            Text  = "Enter environment details below:",
            X     = 1,
            Y     = 1,
            Width = Dim.Fill()
        };
        Add(instructionsLabel);

        var nameLabel = new Label()
        {
            Text = "Environment Name:",
            X    = 1,
            Y    = Pos.Bottom(instructionsLabel) + 1
        };
        Add(nameLabel);

        var nameField = new TextField()
        {
            Text  = "",
            X     = Pos.Right(nameLabel) + 1,
            Y     = Pos.Top(nameLabel),
            Width = 30
        };
        Add(nameField);

        var urlLabel = new Label()
        {
            Text = "Org URL:",
            X    = 1,
            Y    = Pos.Bottom(nameField) + 1
        };
        Add(urlLabel);

        var urlField = new TextField()
        {
            Text  = "https://",
            X     = Pos.Right(urlLabel) + 1,
            Y     = Pos.Top(urlLabel),
            Width = 40
        };
        Add(urlField);

        var addButton = new Button()
        {
            Text = "Add",
            X    = 2,
            Y    = Pos.Bottom(urlField) + 1
        };
        addButton.Accepting += (s, e) =>
        {
            e.Handled = true;
            if (!_isSubmitting)
                _ = SubmitEnvironmentAsync(nameField.Text.ToString(), urlField.Text.ToString());
        };
        Add(addButton);

        var cancelButton = new Button()
        {
            Text = "Cancel",
            X    = Pos.Right(addButton) + 2,
            Y    = Pos.Top(addButton)
        };
        cancelButton.Accepting += (s, e) => RequestStop();
        Add(cancelButton);
    }

    private async Task SubmitEnvironmentAsync(string? name, string? orgUrl)
    {
        if (_isSubmitting) return;
        _isSubmitting = true;

        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.ErrorQuery(_app, "Validation Error", "Environment name is required", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(orgUrl) || !orgUrl.StartsWith("https://"))
            {
                MessageBox.ErrorQuery(_app, "Validation Error", "Valid organization URL is required (must start with https://)", "OK");
                return;
            }

            var environment = new EnvironmentDetails
            {
                Id     = Guid.NewGuid().ToString(),
                Name   = name,
                OrgUrl = orgUrl
            };

            await _authService.RegisterEnvironmentAsync(environment);
            RequestStop();
        }
        finally
        {
            _isSubmitting = false;
        }
    }
}
