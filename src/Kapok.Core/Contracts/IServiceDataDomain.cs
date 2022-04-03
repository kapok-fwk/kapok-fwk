using System.Net.Http.Headers;

namespace Kapok.Core;

public interface IServiceDataDomain : IDataDomain
{
    Uri ServiceUri { get; }

    AuthenticationHeaderValue AuthenticationHeader { get; }
}