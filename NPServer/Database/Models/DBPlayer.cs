namespace NPServer.Database.Models
{
    public class DBPlayer
    {
        public long DbGuid { get; set; }
        public byte[] ArchiveData { get; set; }
        public long StartTarget { get; set; }
        public long StartTargetRegionOverride { get; set; }
        public int AOIVolume { get; set; }

        public DBPlayer()
        {
            ArchiveData = []; // Hoặc new byte[0]
            Reset();
        }

        public DBPlayer(long dbGuid)
        {
            ArchiveData = [];
            DbGuid = dbGuid;
            Reset();
        }

        public void Reset()
        {
            ArchiveData = [];
            StartTarget = unchecked((long)15338215617681369199);
            StartTargetRegionOverride = 0;
            AOIVolume = 3200;
        }
    }
}