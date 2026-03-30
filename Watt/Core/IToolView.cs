using Terminal.Gui.ViewBase;

namespace Watt.Core;

internal interface IToolView
{
    string Id { get; }
    string Name { get; }

    /// <summary>
    /// Creates a new view representation based on the specified application state.
    /// </summary>
    /// <param name="state">The current state of the application used to generate the view. Cannot be null.</param>
    /// <returns>A new instance of the view that reflects the provided application state.</returns>
    View CreateView(AppState state);

    /// <summary>
    /// Creates a new view representation for the toolbar based on the specified application state.
    /// </summary>
    /// <param name="state">The current state of the application used to generate the view. Cannot be null.</param>
    /// <returns>A new instance of the view that reflects the provided application state.</returns>
    View CreteToolbarView(AppState state);
    
    /// <summary>
    /// Handles logic that should occur when the component or application is activated.
    /// </summary>
    /// <remarks>Override this method to implement custom activation behavior, such as refreshing data or
    /// updating state when the component becomes active. The specific actions performed depend on the context in which
    /// the method is used.</remarks>
    void OnActivated();
    
    /// <summary>
    /// Handles logic to be executed when the component is deactivated.
    /// </summary>
    /// <remarks>Override this method to perform cleanup or state updates when the component is no longer
    /// active. This method is typically called as part of a component's lifecycle management.</remarks>
    void OnDeactivated();
}
