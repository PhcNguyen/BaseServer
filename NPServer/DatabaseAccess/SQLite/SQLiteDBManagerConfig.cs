using NPServer.Infrastructure.Configuration.Abstract;

namespace NPServer.DatabaseAccess.SQLite
{
    public class SQLiteDBManagerConfig : AbstractConfigContainer
    {
        public string FileName { get; private set; } = "Account.db";
        public int MaxBackupNumber { get; private set; } = 5;
        public int BackupIntervalMinutes { get; private set; } = 15;
    }
}