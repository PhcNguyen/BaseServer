using NPServer.Core.Interfaces.Session;
using NPServer.Infrastructure.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace NPServer.AdminPanel
{
    public class SessionVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<SessionData> Sessions { get; set; } = [];

        public SessionVM()
        {
            // Constructor mặc định, nếu cần.
        }

        public SessionVM(ISessionManager sessionManager)
        {
            // Đăng ký sự kiện từ SessionManager
            sessionManager.SessionAdded += OnSessionAdded;
            sessionManager.SessionRemoved += OnSessionRemoved;
        }

        private void UpdateSN()
        {
            for (int i = 0; i < Sessions.Count; i++)
            {
                Sessions[i].SerialNumber = i + 1; // Đánh số từ 1
            }
        }

        private void OnSessionAdded(ISessionClient session)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Sessions.Add(new SessionData
                {
                    ID = session.Id.ToString(),
                    EndPoint = session.EndPoint,
                    Role = session.Role.ToString(),
                    FirstRecordingTime = DateTime.UtcNow.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff")
                });
            });

            UpdateSN();
        }

        private void OnSessionRemoved(UniqueId sessionId)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var sessionToRemove = Sessions.FirstOrDefault(s => s.ID == sessionId.ToString());
                if (sessionToRemove != null)
                {
                    Sessions.Remove(sessionToRemove);
                }
            });

            UpdateSN();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static implicit operator DataGrid(SessionVM v)
        {
            var dataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                CanUserReorderColumns = false,
                CanUserResizeColumns = false,
                CanUserSortColumns = false,
                ItemsSource = v.Sessions
            };

            return dataGrid;
        }
    }
}