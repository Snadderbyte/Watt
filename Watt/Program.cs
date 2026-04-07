using System.Collections.ObjectModel;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Watt.CLI;
using Watt.Core;
using Watt.Core.Authentication;
using Watt.UI.Tools;
using Watt.UI;

// --- CLI mode: handle env subcommands without launching the TUI ---
if (args.Length > 0)
{
    var cliAuthService = new AuthenticationService();
    await cliAuthService.InitializeAsync();
    var exitCode = await CliHandler.RunAsync(args, cliAuthService);
    await cliAuthService.DisposeAsync();
    return exitCode;
}

// --- TUI mode: launch the GUI ---
using var app = Application.Create().Init();

// Initialize authentication services before Application.Init() installs
var authService = new AuthenticationService();
await authService.InitializeAsync();
var activeEnvironment = authService.GetActiveEnvironment();
if (activeEnvironment == null)
{
    MessageBox.ErrorQuery(app, "No active environment found", "Please use the CLI to set an active environment before launching the TUI.\nWith 'watt env add <environmentName> <orgUrl>' and 'watt env set <environmentName>'", "Close");
    authService.DisposeAsync().AsTask().Wait();
    return 1;
}

var connectionManager = new DataverseConnectionManager(authService);

var appState = new AppState
{
    AuthenticationService = authService,
    ConnectionManager = connectionManager,
    ServiceClient = await connectionManager.GetConnectionAsync(activeEnvironment.Id)
};

var tools = new List<IToolView>
{
    new DrfView(appState),
    new InspectorView(appState)
};


var win = new Window()
{
    X = 0,
    Y = 0,
    Width = Dim.Fill(),
    Height = Dim.Fill(),
    BorderStyle = LineStyle.None,
};

tools.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
var toolNames = new ObservableCollection<string>(tools.ConvertAll(t => t.Name));

var toolSelector = new ToolSelector(appState, toolNames)
{
    Title = "Select a Tool",
    X = 0,
    Y = 0,
    Width = 30,
    Height = Dim.Fill(),
};

try
{
    int selectedToolIndex = 0;
    toolSelector.ToolSelected += index => selectedToolIndex = index;
    app.Run(toolSelector);
    appState.SelectedTool = tools[selectedToolIndex];
}
catch (Exception ex)
{
    MessageBox.ErrorQuery(app, "Error", $"An error occurred while running the application: {ex.Message}", "Close");
    return 1;
}

var selectedTool = appState.SelectedTool ?? tools[0];
selectedTool.View.X = 0;
selectedTool.View.Y = 0;
selectedTool.View.Width = Dim.Fill();
selectedTool.View.Height = Dim.Fill();
selectedTool.InitializeUi();
win.Initialized += async (_, _) => await selectedTool.LoadAsync();

var statusBar = new StatusBar(
[
    new Shortcut(Key.Q.WithCtrl, "Quit", () => app.RequestStop()),

    new Shortcut(Key.F1, "Help", () =>
    {
        MessageBox.Query(app, "Help", "Use Ctrl+Q to quit. Select a tool from the list to get started.", "Close");
    }),
    new Shortcut(Key.F5, "Refresh Connection", async () =>
    {
        var env = authService.GetActiveEnvironment();
        if (env != null)
        {
            appState.ServiceClient = await connectionManager.GetConnectionAsync(env.Id);
            MessageBox.Query(app, "Connection Refreshed", $"Connection for environment '{env.Name}' has been refreshed.", "Close");
        }
        else
        {
            MessageBox.ErrorQuery(app, "No Active Environment", "There is no active environment. Please set an active environment using the CLI.", "Close");
        }
    }),
    new Shortcut(Key.F5.WithShift, $"Env: {authService.GetActiveEnvironment()?.Name ?? "None"}", () =>
    {
        var env = authService.GetActiveEnvironment();
        if (env != null)
        {
            MessageBox.Query(app, "Current Environment", $"Name: {env.Name}\nURL: {env.OrgUrl}", "Close");
        }
        else
        {
            MessageBox.ErrorQuery(app, "No Active Environment", "There is no active environment. Please set an active environment using the CLI.", "Close");
        }
    })
]);

win.Add(selectedTool.View, statusBar);

try
{
    app.Run(win);
    win.Dispose();
}
finally
{
    await connectionManager.DisposeAsync();
    await authService.DisposeAsync();
}

return 0;