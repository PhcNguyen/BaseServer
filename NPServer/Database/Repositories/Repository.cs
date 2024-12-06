using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPServer.Database.Repositories
{
    /// <summary>
    /// Lớp này đại diện cho một kho lưu trữ của đối tượng kiểu TValue, cung cấp các phương thức CRUD cơ bản.
    /// </summary>
    /// <typeparam name="TKey">Kiểu của khóa chính.</typeparam>
    /// <typeparam name="TValue">Kiểu của đối tượng được lưu trữ trong kho dữ liệu.</typeparam>
    public class Repository<TKey, TValue> : IRepository<TKey, TValue>
        where TValue : class
    {
        /// <summary>
        /// Lấy đối tượng từ cơ sở dữ liệu theo khóa chính.
        /// </summary>
        /// <param name="id">Khóa chính của đối tượng cần lấy.</param>
        /// <returns>Đối tượng tìm được, nếu không tìm thấy trả về null.</returns>
        public TValue GetById(TKey id)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session.Get<TValue>(id);
            }
        }

        /// <summary>
        /// Thêm một đối tượng vào cơ sở dữ liệu.
        /// </summary>
        /// <param name="obj">Đối tượng cần thêm vào cơ sở dữ liệu.</param>
        public void Add(TValue obj)
        {
            try
            {
                using (var session = NHibernateHelper.OpenSession())
                {
                    using (var transaction = session.BeginTransaction())
                    {
                        session.Save(obj);
                        transaction.Commit();
                    }
                }
            }
            catch (Exception p) { Console.WriteLine(p); }
        }

        /// <summary>
        /// Cập nhật thông tin của đối tượng trong cơ sở dữ liệu.
        /// </summary>
        /// <param name="obj">Đối tượng cần cập nhật.</param>
        public void Update(TValue obj)
        {
            try
            {
                using (var session = NHibernateHelper.OpenSession())
                {
                    using (var transaction = session.BeginTransaction())
                    {
                        session.Update(obj);
                        transaction.Commit();
                    }
                }
            }
            catch (Exception p) { Console.WriteLine(p); }
        }

        /// <summary>
        /// Xóa một đối tượng khỏi cơ sở dữ liệu.
        /// </summary>
        /// <param name="obj">Đối tượng cần xóa.</param>
        public void Remove(TValue obj)
        {
            try
            {
                using (var session = NHibernateHelper.OpenSession())
                {
                    using (var transaction = session.BeginTransaction())
                    {
                        session.Delete(obj);
                        transaction.Commit();
                    }
                }
            }
            catch (Exception p) { Console.WriteLine(p); }
        }

        /// <summary>
        /// Thêm hoặc cập nhật đối tượng trong cơ sở dữ liệu, tùy thuộc vào việc đối tượng đã tồn tại hay chưa.
        /// </summary>
        /// <param name="obj">Đối tượng cần thêm hoặc cập nhật.</param>
        public void AddOrUpdate(TValue obj)
        {
            try
            {
                using (var session = NHibernateHelper.OpenSession())
                {
                    using (var transaction = session.BeginTransaction())
                    {
                        session.SaveOrUpdate(obj);
                        transaction.Commit();
                    }
                }
            }
            catch (Exception p) { Console.WriteLine(p); }
        }

        /// <summary>
        /// Thêm nhiều đối tượng vào cơ sở dữ liệu.
        /// </summary>
        /// <param name="collection">Danh sách các đối tượng cần thêm.</param>
        public void Add(ICollection<TValue> collection)
        {
            foreach (var obj in collection)
            {
                Add(obj);
            }
        }

        /// <summary>
        /// Cập nhật nhiều đối tượng trong cơ sở dữ liệu.
        /// </summary>
        /// <param name="collection">Danh sách các đối tượng cần cập nhật.</param>
        public void Update(ICollection<TValue> collection)
        {
            foreach (var obj in collection)
            {
                Update(obj);
            }
        }

        /// <summary>
        /// Xóa nhiều đối tượng khỏi cơ sở dữ liệu.
        /// </summary>
        /// <param name="collection">Danh sách các đối tượng cần xóa.</param>
        public void Remove(ICollection<TValue> collection)
        {
            foreach (var obj in collection)
            {
                Remove(obj);
            }
        }

        /// <summary>
        /// Thêm hoặc cập nhật nhiều đối tượng trong cơ sở dữ liệu.
        /// </summary>
        /// <param name="collection">Danh sách các đối tượng cần thêm hoặc cập nhật.</param>
        public void AddOrUpdate(ICollection<TValue> collection)
        {
            foreach (var obj in collection)
            {
                AddOrUpdate(obj);
            }
        }
    }
}
