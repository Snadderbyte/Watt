using System.Collections.ObjectModel;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Watt.Core;
using Watt.Core.Authentication;
using Watt.UI;

using var app = Application.Create().Init();

// Initialize authentication services before Application.Init() installs
// its SynchronizationContext, to avoid async/await deadlocks.
var authService = new AuthenticationService();
await authService.InitializeAsync();

var connectionManager = new DataverseConnectionManager(authService);

var appState = new AppState
{
    AuthenticationService = authService,
    ConnectionManager = connectionManager
};

var tools = new List<IToolView>
{
    new DrfView(),
};

tools.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));

var win = new Window()
{
    Title =  "Watt",
    X = 0,
    Y = 0,
    Width = Dim.Fill(),
    Height = Dim.Fill()
};

// Create environment selector button in top area
var environmentStatusLabel = new Label()
{
    Text =  "Environment",
    X = 1,
    Y = 0,
    Width = 50
};

var selectEnvironmentButton = new Button()
{
    Text =  "Select Environment",
    X = Pos.Right(environmentStatusLabel),
    Y = 0
};
selectEnvironmentButton.Accepting += (s, e) =>
{
    var envDialog = new EnvironmentSelectorDialog(app, dialog => app.Run(dialog), authService, connectionManager);
    app.Run(envDialog);
    envDialog.Dispose();

    // Update status
    if (!string.IsNullOrEmpty(appState.CurrentEnvironmentId))
    {
        var env = authService.GetEnvironment(appState.CurrentEnvironmentId);
        if (env != null)
            environmentStatusLabel.Text = $"Connected: {env.Name}";
    }
};

var topBar = new FrameView()
{
    Text = "Connection",
    X = 0,
    Y = 0,
    Width = Dim.Fill(),
    Height = 3
};
topBar.Add(environmentStatusLabel, selectEnvironmentButton);

var toolNames = new ObservableCollection<string>(tools.ConvertAll(t => t.Name));
var listView = new ListView
{
    X = 0,
    Y = Pos.Bottom(topBar),
    Width = 25,
    Height = Dim.Fill()
};
listView.SetSource<string>(toolNames);

var mainPanel = new FrameView()
{
    Title = "Tools",
    X = Pos.Right(listView),
    Y = Pos.Bottom(topBar),
    Width = Dim.Fill(),
    Height = Dim.Fill()
};

listView.ValueChanged += (s, args) =>
{
    if (appState.Connection is not { IsReady: true })
    {
        MessageBox.ErrorQuery(app, "No Connection", "Please connect to an environment first", "OK");
        return;
    }

    if (args.NewValue is not { } index || index < 0 || index >= tools.Count)
        return;

    mainPanel.RemoveAll();
    var selectedTool = tools[index];
    selectedTool.OnActivated();
    var view = selectedTool.CreateView(appState);
    if (view is View v)
        mainPanel.Add(v);
};

win.Add(topBar, listView, mainPanel);

try
{
    app.Run(win);
    win.Dispose();
}
finally
{
    // Cleanup
    await connectionManager.DisposeAsync();
    await authService.DisposeAsync();
}