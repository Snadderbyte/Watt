using System;
using System.Threading.Tasks;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Watt.Core.Authentication;

namespace Watt.UI.Connection;

/// <summary>
/// Dialog for authenticating with a Dataverse environment.
/// </summary>
public class AuthenticationDialog : Dialog
{
    private readonly AuthenticationService _authService;
    private readonly DataverseConnectionManager _connectionManager;
    private readonly EnvironmentDetails _environment;
    private Label? _statusLabel;

    private bool _isAuthenticating;
    public bool AuthenticationSucceeded { get; private set; }

    public AuthenticationDialog(
        AuthenticationService authService,
        DataverseConnectionManager connectionManager,
        EnvironmentDetails environment)
    {
        _authService = authService;
        _connectionManager = connectionManager;
        _environment = environment;

        InitializeUI();
    }

    private void InitializeUI()
    {
        Title = $"Authenticate - {_environment.Name}";
        Width = Dim.Auto();
        Height = Dim.Auto();

        _statusLabel = new Label()
        {
            Text = "Preparing to authenticate...",
            X = 1,
            Y = 1,
            Width = Dim.Fill(1)
        };
        Add(_statusLabel);

        Action authMethod = _environment.AuthMethod switch
        {
            AuthenticationMethod.OAuth => CreateOAuthUI,
            AuthenticationMethod.ClientSecret => CreateClientSecretUI,
            AuthenticationMethod.UsernamePassword => CreateUsernamePasswordUI,
            _ => () => { _statusLabel.Text = "Unsupported authentication method"; }
        };

        authMethod();
    }

    private void CreateOAuthUI()
    {
        var instructions = new Label()
        {
            Text = "Click 'Authenticate' to sign in with your account.",
            X = 1,
            Y = Pos.Bottom(_statusLabel!) + 1,
            Width = Dim.Fill(1)
        };
        Add(instructions);

        var authenticateButton = new Button()
        {
            Text = "Authenticate",
            X = 1,
            Y = Pos.Bottom(instructions) + 1
        };
        authenticateButton.Accepting += (s, e) =>
        {
            e.Handled = true;

            if (_isAuthenticating)
            {
                return;
            }

            _ = AuthenticateWithOAuthAsync();
        };
        Add(authenticateButton);

        var cancelButton = new Button()
        {
            Text = "Cancel",
            X = Pos.Right(authenticateButton) + 2,
            Y = Pos.Top(authenticateButton)
        };
        cancelButton.Accepting += (s, e) => RequestStop();
        Add(cancelButton);
    }

    private void CreateClientSecretUI()
    {
        var tenantLabel = new Label()
        {
            Text = "Tenant ID:",
            X = 1,
            Y = Pos.Bottom(_statusLabel!) + 1
        };
        Add(tenantLabel);

        var tenantField = new TextField()
        {
            Text = "",
            X = Pos.Right(tenantLabel) + 1,
            Y = Pos.Top(tenantLabel),
            Width = 50
        };
        Add(tenantField);

        var clientIdLabel = new Label()
        {
            Text = "Client ID:",
            X = 1,
            Y = Pos.Bottom(tenantField) + 1
        };
        Add(clientIdLabel);

        var clientIdField = new TextField()
        {
            Text = "",
            X = Pos.Right(clientIdLabel) + 1,
            Y = Pos.Top(clientIdLabel),
            Width = 50
        };
        Add(clientIdField);

        var secretLabel = new Label()
        {
            Text = "Client Secret:",
            X = 1,
            Y = Pos.Bottom(clientIdField) + 1
        };
        Add(secretLabel);

        var secretField = new TextField()
        {
            Text = "",
            X = Pos.Right(secretLabel) + 1,
            Y = Pos.Top(secretLabel),
            Width = 50,
            Secret = true
        };
        Add(secretField);

        var authenticateButton = new Button()
        {
            Text = "Authenticate",
            X = 1,
            Y = Pos.Bottom(secretField) + 1
        };
        authenticateButton.Accepting += (s, e) =>
        {
            e.Handled = true;

            if (_isAuthenticating)
            {
                return;
            }

            _ = AuthenticateWithClientSecretAsync(
                tenantField.Text.ToString()!,
                clientIdField.Text.ToString()!,
                secretField.Text.ToString()!);
        };
        Add(authenticateButton);

        var cancelButton = new Button()
        {
            Text = "Cancel",
            X = Pos.Right(authenticateButton) + 2,
            Y = Pos.Top(authenticateButton)
        };
        cancelButton.Accepting += (s, e) => RequestStop();
        Add(cancelButton);
    }

    private void CreateUsernamePasswordUI()
    {
        var usernameLabel = new Label()
        {
            Text = "Username:",
            X = 1,
            Y = Pos.Bottom(_statusLabel!) + 1
        };
        Add(usernameLabel);

        var usernameField = new TextField()
        {
            Text = "",
            X = Pos.Right(usernameLabel) + 1,
            Y = Pos.Top(usernameLabel),
            Width = 50
        };
        Add(usernameField);

        var passwordLabel = new Label()
        {
            Text = "Password:",
            X = 1,
            Y = Pos.Bottom(usernameField) + 1
        };
        Add(passwordLabel);

        var passwordField = new TextField()
        {
            Text = "",
            X = Pos.Right(passwordLabel) + 1,
            Y = Pos.Top(passwordLabel),
            Width = 50,
            Secret = true
        };
        Add(passwordField);

        var authenticateButton = new Button()
        {
            Text = "Authenticate",
            X = 1,
            Y = Pos.Bottom(passwordField) + 1
        };
        authenticateButton.Accepting += (s, e) =>
        {
            e.Handled = true;

            if (_isAuthenticating)
            {
                return;
            }

            _ = AuthenticateWithUsernamePasswordAsync(
                usernameField.Text.ToString()!,
                passwordField.Text.ToString()!);
        };
        Add(authenticateButton);

        var cancelButton = new Button()
        {
            Text = "Cancel",
            X = Pos.Right(authenticateButton) + 2,
            Y = Pos.Top(authenticateButton)
        };
        cancelButton.Accepting += (s, e) => RequestStop();
        Add(cancelButton);
    }

    private async Task AuthenticateWithOAuthAsync()
    {
        if (_isAuthenticating)
        {
            return;
        }

        _isAuthenticating = true;

        try
        {
            await AuthenticateWithOAuth();
        }
        finally
        {
            _isAuthenticating = false;
        }
    }

    private async Task AuthenticateWithClientSecretAsync(string tenantId, string clientId, string clientSecret)
    {
        if (_isAuthenticating)
        {
            return;
        }

        _isAuthenticating = true;

        try
        {
            await AuthenticateWithClientSecret(tenantId, clientId, clientSecret);
        }
        finally
        {
            _isAuthenticating = false;
        }
    }

    private async Task AuthenticateWithUsernamePasswordAsync(string username, string password)
    {
        if (_isAuthenticating)
        {
            return;
        }

        _isAuthenticating = true;

        try
        {
            await AuthenticateWithUsernamePassword(username, password);
        }
        finally
        {
            _isAuthenticating = false;
        }
    }

    private async Task AuthenticateWithOAuth()
    {
        _statusLabel!.Text = "Authenticating with OAuth...";
        try
        {
            var result = await _authService.AuthenticateWithOAuthAsync(_environment);
            if (result.IsSuccessful)
            {
                AuthenticationSucceeded = true;
                RequestStop();
            }
            else
            {
                _statusLabel.Text = $"Authentication failed: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"Error: {ex.Message}";
        }
    }

    private async Task AuthenticateWithClientSecret(string tenantId, string clientId, string clientSecret)
    {
        if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            _statusLabel!.Text = "All fields are required";
            return;
        }

        _statusLabel!.Text = "Authenticating with Client Secret...";
        try
        {
            var credentials = new ClientSecretCredentials
            {
                EnvironmentId = _environment.Id,
                TenantId = tenantId,
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            var result = await _authService.AuthenticateWithClientSecretAsync(_environment, credentials);
            if (result.IsSuccessful)
            {
                AuthenticationSucceeded = true;
                RequestStop();
            }
            else
            {
                _statusLabel.Text = $"Authentication failed: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"Error: {ex.Message}";
        }
    }

    private async Task AuthenticateWithUsernamePassword(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            _statusLabel!.Text = "Username and password are required";
            return;
        }

        _statusLabel!.Text = "Authenticating with Username/Password...";
        try
        {
            var credentials = new UsernamePasswordCredentials
            {
                EnvironmentId = _environment.Id,
                Username = username,
                Password = password
            };

            var result = await _authService.AuthenticateWithUsernamePasswordAsync(_environment, credentials);
            if (result.IsSuccessful)
            {
                AuthenticationSucceeded = true;
                RequestStop();
            }
            else
            {
                _statusLabel.Text = $"Authentication failed: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"Error: {ex.Message}";
        }
    }
}
