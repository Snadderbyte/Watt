using Terminal.Gui;
using Watt.Tools;
using Watt.Core;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        Application.Init();

        var appState = new AppState();
        var tools = new List<ITool>
        {
            new DRFTool(),
            new DataverseInspector()
        };

        tools.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));

        var win = new Window("Watt")
        {
            X = 0,
            Y = 0, // Leave space for menu bar
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var toolNames = tools.ConvertAll(t => t.Name);
        var listView = new ListView(toolNames)
        {
            X = 0,
            Y = 0,
            Width = 25,
            Height = Dim.Fill()
        };

        var mainPanel = new FrameView("Tool View")
        {
            X = Pos.Right(listView),
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        listView.SelectedItemChanged += args =>
        {
            mainPanel.RemoveAll();
            var selectedTool = tools[args.Item];
            selectedTool.OnActivated();
            var view = selectedTool.CreateView(appState);
            if (view is View v)
                mainPanel.Add(v);
        };

        win.Add(listView, mainPanel);
        Application.Top.Add(win);

        Application.Run();
    }
}