using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NETServer.Logging
{
    /// <summary>
    /// This static class provides functionality to search through log files
    /// for entries containing a specific keyword and optionally filtering by log level.
    /// </summary>
    public static class NLogSearch
    {
        /// <summary>
        /// Searches through log files for entries that contain the specified keyword
        /// and optionally filter by the provided log level.
        /// </summary>
        /// <param name="keyword">The keyword to search for in the log entries.</param>
        /// <param name="level">The log level to filter by (optional).</param>
        /// <returns>An enumerable of matching log entries as strings.</returns>
        //public static IEnumerable<string> Search(string keyword, NLogLevel level) { }
    }
}
