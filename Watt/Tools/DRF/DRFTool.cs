using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Watt.Tools.DRF;

internal class DrfTool
{
    public string Id => "T0001";
    public string Name => "Duplicate Row Finder";
    
    public ServiceClient Connection { get; private set; }
    
    public DrfTool(ServiceClient connection)
    {
        Connection = connection;
    }
}