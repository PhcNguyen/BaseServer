namespace NPServer.Models
{
    /// <summary>
    /// Đại diện cho một người chơi trong cơ sở dữ liệu.
    /// </summary>
    public class DBPlayer
    {
        /// <summary>
        /// ID duy nhất của người chơi trong cơ sở dữ liệu.
        /// </summary>
        public long DbGuid { get; set; }

        /// <summary>
        /// Dữ liệu lưu trữ của người chơi.
        /// </summary>
        public byte[] ArchiveData { get; set; }

        /// <summary>
        /// Mục tiêu bắt đầu của người chơi.
        /// </summary>
        public long StartTarget { get; set; }

        /// <summary>
        /// Ghi đè vùng mục tiêu bắt đầu của người chơi.
        /// </summary>
        public long StartTargetRegionOverride { get; set; }

        /// <summary>
        /// Âm lượng AOI của người chơi.
        /// </summary>
        public int AOIVolume { get; set; }

        /// <summary>
        /// Khởi tạo một phiên bản mới của <see cref="DBPlayer"/>.
        /// </summary>
        public DBPlayer()
        {
            ArchiveData = [];
            Reset();
        }

        /// <summary>
        /// Khởi tạo một phiên bản mới của <see cref="DBPlayer"/> với ID cơ sở dữ liệu cụ thể.
        /// </summary>
        /// <param name="dbGuid">ID duy nhất của người chơi trong cơ sở dữ liệu.</param>
        public DBPlayer(long dbGuid)
        {
            ArchiveData = [];
            DbGuid = dbGuid;
            Reset();
        }

        /// <summary>
        /// Đặt lại thuộc tính của người chơi về giá trị mặc định.
        /// </summary>
        public void Reset()
        {
            ArchiveData = [];
            StartTarget = unchecked((long)15338215617681369199);
            StartTargetRegionOverride = 0;
            AOIVolume = 3200;
        }
    }
}