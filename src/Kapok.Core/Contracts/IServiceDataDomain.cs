using System.Net.Http.Headers;

namespace Kapok.Data;

public interface IServiceDataDomain : IDataDomain
{
    Uri ServiceUri { get; }

    AuthenticationHeaderValue AuthenticationHeader { get; }
}