using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Watt.Core;

namespace Watt.UI.Tools;

internal class InspectorView : IToolView
{
    public string Name { get; } = "Inspector";
    public AppState AppState { get; set; }
    public View View { get; set; }
    public HelpDialog HelpDialog { get; set; } = new HelpDialog();

    public InspectorView(AppState appState)
    {
        AppState = appState;
        View = new View();
    }

    public void InitializeUi()
    {
        View.X = 0;
        View.Y = 0;
        View.Width = Dim.Fill();
        View.Height = Dim.Fill() - 1;

        var label = new Label
        {
            Text = Name,
            X = 1,
            Y = 1,
            Width = Dim.Fill(2),
            Height = Dim.Fill(2),
        };

        View.Add(label);
    }

    public Task LoadAsync() => Task.CompletedTask;

    public Task RefreshAsync() => Task.CompletedTask;
}