using Terminal.Gui;

namespace Watt.Core;

public interface ITool
{
    string Id { get; }
    string Name { get; }

    View CreateView(AppState state);
    void OnActivated();
    void OnDeactivated();
}