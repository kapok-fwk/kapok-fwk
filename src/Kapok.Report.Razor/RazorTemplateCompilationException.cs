using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Kapok.Report.Razor
{
    /// <summary>
    /// Is thrown when the compilation of a Razor template was not successful.
    /// </summary>
    public class RazorTemplateCompilationException : Exception
    {
        public RazorTemplateCompilationException(ImmutableArray<Diagnostic> diagnostics)
            : base("Error during Razor template compilation:\n" + string.Join("\n", diagnostics.Select(d => d.ToString())))
        {
            Diagnostics = diagnostics;
        }

        public ImmutableArray<Diagnostic> Diagnostics { get; }
    }
}