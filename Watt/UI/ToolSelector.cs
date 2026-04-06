using System.Collections.ObjectModel;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using Watt.Core;

namespace Watt.UI;
public class ToolSelector : Dialog
{
    public ToolSelector(
        AppState appState, 
        ObservableCollection<string> toolNames)
    {
        var toolList = new ListView
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(2),    
            Height = Dim.Fill(2),
        };
        toolList.SetSource(toolNames);

        var focusedItem = 0;
        // When a tool is selected, raise the ToolSelected event with the index of the selected tool
        toolList.ValueChanged += (s, args) =>
        {
            if (args.NewValue is { } index && index >= 0 && index < toolNames.Count)
            {
                focusedItem = index;
            }
        };

        toolList.Accepting += (s, args) =>
        {
            if (focusedItem >= 0 && focusedItem < toolNames.Count)
            {
                ToolSelected?.Invoke(focusedItem);
                RequestStop();
            }
        };
        Add(toolList);
    }
    public event Action<int>? ToolSelected;
}