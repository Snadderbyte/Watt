using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;
using Watt.Core;

namespace Watt.Tools;

/// <summary>
/// Duplicate Row Finder
/// 
/// A tool to find duplicate rows in Dataverse.
/// </summary>
internal class DRFTool : ITool
{
    public string Id => "drf_tool";
    public string Name => "Duplicate Row Finder";

    public View CreateView(AppState state)
    {
        var label = new Label("Duplicate Row Finder");
        return label;
    }

    public void OnActivated()
    {
        // Handle activation logic here
    }

    public void OnDeactivated()
    {
        // Handle deactivation logic here
    }
}
