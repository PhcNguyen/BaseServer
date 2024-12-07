using NPServer.Infrastructure.Configuration.Abstract;

namespace NPServer.Database.SQLite
{
    public class SQLiteDBManagerConfig : ConfigContainer
    {
        public string FileName { get; private set; } = "Account.db";
        public int MaxBackupNumber { get; private set; } = 5;
        public int BackupIntervalMinutes { get; private set; } = 15;
    }
}