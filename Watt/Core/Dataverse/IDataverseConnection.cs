using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Watt.Core.Dataverse;

public interface IDataverseConnection
{
    IOrganizationServiceAsync2 _serviceClient { get; }
}