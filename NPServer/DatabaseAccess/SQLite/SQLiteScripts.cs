using NPServer.Infrastructure.Configuration;
using NPServer.Infrastructure.Helper;
using NPServer.Infrastructure.Logging;
using System.IO;

namespace NPServer.DatabaseAccess.SQLite
{
    public static class SQLiteScripts
    {
        public static string GetInitializationScript()
        {
            string filePath = Path.Combine(PathConfig.DataDirectory, "SQLite", "InitializeDatabase.sql");
            if (File.Exists(filePath) == false)
            {
                NPLog.Instance.Warning($"GetDatabaseInitializationScript(): Initialization script file not found at {FileHelper.GetRelativePath(filePath)}");
                return string.Empty;
            }

            return File.ReadAllText(filePath);
        }

        public static string GetMigrationScript(int currentVersion)
        {
            string filePath = Path.Combine(PathConfig.DataDirectory, "SQLite", "Migrations", $"{currentVersion}.sql");
            if (File.Exists(filePath) == false)
            {
                NPLog.Instance.Warning($"GetMigrationScript(): Migration script for version {currentVersion} not found at {FileHelper.GetRelativePath(filePath)}");
                return string.Empty;
            }

            return File.ReadAllText(filePath);
        }
    }
}