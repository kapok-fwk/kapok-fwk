using System;
using System.IO;

namespace Kapok.Report
{
    [Obsolete("Use IMimeTypeReportProcessor instead with mime type text/csv")]
    public interface ICsvReportProcessor : IReportProcessor
    {
        void ProcessToCsvStream(Stream stream);
    }
}
