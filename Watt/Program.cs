using Terminal.Gui;
using Watt.Core;
using Watt.Core.Authentication;
using Watt.UI;
using Watt.UI.DRF;

class Program
{
    static async Task Main(string[] args)
    {
        Application.Init();

        // Initialize authentication services
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
            new DRFView(),
        };

        tools.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));

        var win = new Window("Watt")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        // Create environment selector button in top area
        var environmentStatusLabel = new Label("No environment selected")
        {
            X = 1,
            Y = 0,
            Width = 50
        };

        var selectEnvironmentButton = new Button("Select Environment")
        {
            X = Pos.Right(environmentStatusLabel),
            Y = 0
        };
        selectEnvironmentButton.Clicked += () =>
        {
            var envDialog = new EnvironmentSelectorDialog(authService, connectionManager);
            Application.Run(envDialog);

            // Update status
            if (!string.IsNullOrEmpty(appState.CurrentEnvironmentId))
            {
                var env = authService.GetEnvironment(appState.CurrentEnvironmentId);
                if (env != null)
                    environmentStatusLabel.Text = $"Connected: {env.Name}";
            }
        };

        var topBar = new FrameView("Connection")
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = 3
        };
        topBar.Add(environmentStatusLabel, selectEnvironmentButton);

        var toolNames = tools.ConvertAll(t => t.Name);
        var listView = new ListView(toolNames)
        {
            X = 0,
            Y = Pos.Bottom(topBar),
            Width = 25,
            Height = Dim.Fill()
        };

        var mainPanel = new FrameView("Tool View")
        {
            X = Pos.Right(listView),
            Y = Pos.Bottom(topBar),
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        listView.SelectedItemChanged += args =>
        {
            if (appState.Connection == null || !appState.Connection.IsReady)
            {
                MessageBox.ErrorQuery("No Connection", "Please connect to an environment first", "OK");
                return;
            }

            mainPanel.RemoveAll();
            var selectedTool = tools[args.Item];
            selectedTool.OnActivated();
            var view = selectedTool.CreateView(appState);
            if (view is View v)
                mainPanel.Add(v);
        };

        win.Add(topBar, listView, mainPanel);
        Application.Top.Add(win);

        try
        {
            Application.Run();
        }
        finally
        {
            // Cleanup
            await connectionManager.DisposeAsync();
            await authService.DisposeAsync();
        }
    }
}