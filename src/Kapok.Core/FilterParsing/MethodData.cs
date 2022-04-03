using System.Linq.Expressions;
using System.Reflection;

namespace Kapok.Core.FilterParsing;

internal class MethodData
{
    public MethodData(MethodBase methodBase, ParameterInfo[] parameters)
    {
        MethodBase = methodBase;
        Parameters = parameters;
    }

    public MethodBase MethodBase { get; set; }
    public ParameterInfo[] Parameters { get; set; }
    public Expression[]? Args { get; set; }
}