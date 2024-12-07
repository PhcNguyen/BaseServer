using NPServer.Infrastructure.Logging;
using System.Collections;
using System.Collections.Generic;

namespace NPServer.Models.Database
{
    /// <summary>
    /// Đại diện cho một tập hợp các thực thể <see cref="DBEntity"/> trong cơ sở dữ liệu thuộc về một <see cref="DBAccount"/> cụ thể.
    /// </summary>
    public class DBEntityCollection : IEnumerable<DBEntity>
    {
        /// <summary>
        /// Tất cả các thực thể <see cref="DBEntity"/> được lưu trữ trong bộ sưu tập này.
        /// </summary>
        private readonly Dictionary<long, DBEntity> _allEntities = [];

        /// <summary>
        /// Các thực thể <see cref="DBEntity"/> được chia thành từng nhóm theo container.
        /// </summary>
        private readonly Dictionary<long, List<DBEntity>> _bucketedEntities = [];

        /// <summary>
        /// Lấy các GUID của tất cả các thực thể trong bộ sưu tập.
        /// </summary>
        public IEnumerable<long> Guids { get => _allEntities.Keys; }

        /// <summary>
        /// Lấy tất cả các thực thể <see cref="DBEntity"/> trong bộ sưu tập.
        /// </summary>
        public IEnumerable<DBEntity> Entries { get => _allEntities.Values; }

        /// <summary>
        /// Lấy số lượng các thực thể trong bộ sưu tập.
        /// </summary>
        public int Count { get => _allEntities.Count; }

        /// <summary>
        /// Khởi tạo một phiên bản mới của <see cref="DBEntityCollection"/>.
        /// </summary>
        public DBEntityCollection()
        { }

        /// <summary>
        /// Khởi tạo một phiên bản mới của <see cref="DBEntityCollection"/> với một tập hợp các thực thể <see cref="DBEntity"/>.
        /// </summary>
        /// <param name="dbEntities">Tập hợp các thực thể <see cref="DBEntity"/> để thêm vào bộ sưu tập.</param>
        public DBEntityCollection(IEnumerable<DBEntity> dbEntities)
        {
            AddRange(dbEntities);
        }

        /// <summary>
        /// Thêm một thực thể <see cref="DBEntity"/> vào bộ sưu tập.
        /// </summary>
        /// <param name="dbEntity">Thực thể <see cref="DBEntity"/> cần thêm.</param>
        /// <returns>True nếu thêm thành công, ngược lại False.</returns>
        public bool Add(DBEntity dbEntity)
        {
            if (_allEntities.TryAdd(dbEntity.DbGuid, dbEntity) == false)
            {
                NPLog.Instance.Info($"Add(): Guid 0x{dbEntity.DbGuid} is already in use");
                return false;
            }

            if (_bucketedEntities.TryGetValue(dbEntity.ContainerDbGuid, out List<DBEntity>? bucket) == false)
            {
                bucket = [];
                _bucketedEntities.Add(dbEntity.ContainerDbGuid, bucket);
            }

            bucket.Add(dbEntity);

            return true;
        }

        /// <summary>
        /// Thêm một tập hợp các thực thể <see cref="DBEntity"/> vào bộ sưu tập.
        /// </summary>
        /// <param name="dbEntities">Tập hợp các thực thể <see cref="DBEntity"/> cần thêm.</param>
        /// <returns>True nếu thêm thành công ít nhất một thực thể, ngược lại False.</returns>
        public bool AddRange(IEnumerable<DBEntity> dbEntities)
        {
            bool success = true;

            foreach (DBEntity dbEntity in dbEntities)
                success |= Add(dbEntity);

            return success;
        }

        /// <summary>
        /// Xóa tất cả các thực thể khỏi bộ sưu tập.
        /// </summary>
        public void Clear()
        {
            _allEntities.Clear();

            foreach (List<DBEntity> bucket in _bucketedEntities.Values)
                bucket.Clear();
        }

        /// <summary>
        /// Lấy tất cả các thực thể <see cref="DBEntity"/> trong một container cụ thể.
        /// </summary>
        /// <param name="containerDbGuid">GUID của container.</param>
        /// <returns>Tập hợp các thực thể <see cref="DBEntity"/> trong container.</returns>
        public IEnumerable<DBEntity> GetEntriesForContainer(long containerDbGuid)
        {
            if (_bucketedEntities.TryGetValue(containerDbGuid, out List<DBEntity>? bucket) == false)
                return [];

            return bucket;
        }

        /// <summary>
        /// Lấy enumerator cho bộ sưu tập các thực thể <see cref="DBEntity"/>.
        /// </summary>
        /// <returns>Enumerator cho bộ sưu tập các thực thể <see cref="DBEntity"/>.</returns>
        public IEnumerator<DBEntity> GetEnumerator()
        {
            return _allEntities.Values.GetEnumerator();
        }

        /// <summary>
        /// Lấy enumerator không kiểu cho bộ sưu tập các thực thể <see cref="DBEntity"/>.
        /// </summary>
        /// <returns>Enumerator không kiểu cho bộ sưu tập các thực thể <see cref="DBEntity"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
