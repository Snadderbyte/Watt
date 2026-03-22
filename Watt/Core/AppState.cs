using System;
using System.Collections.Generic;
using System.Text;
namespace Watt.Core;

public class AppState
{
    public string CurrentEnvironment { get; set; }
    public object? Connection { get; set; }
}
