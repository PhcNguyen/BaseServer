using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using NPServer.UI.Core.Metadata;
using NPServer.Core.Interfaces.Session;
using NPServer.Shared.Services;
using System.Windows.Controls;
using System;

namespace NPServer.UI.ViewsModels;

public class SessionViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private readonly ISessionManager _sessionManager;

    public ObservableCollection<SessionData> Sessions { get; set; } = [];

    public SessionViewModel(ISessionManager sessionManager)
    {
        _sessionManager = sessionManager;

        // Đăng ký sự kiện từ SessionManager
        _sessionManager.SessionAdded += OnSessionAdded;
        _sessionManager.SessionRemoved += OnSessionRemoved;
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
        App.Current.Dispatcher.Invoke(() =>
        {
            Sessions.Add(new SessionData
            {
                ID = session.Id.ToString(),
                EndPoint = session.EndPoint,
                Role = session.Role.ToString(),
                FirstRecordingTime = DateTime.UtcNow.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff")
            });
        });

        this.UpdateSN();
    }

    private void OnSessionRemoved(UniqueId sessionId)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            var sessionToRemove = Sessions.FirstOrDefault(s => s.ID == sessionId.ToString());
            if (sessionToRemove != null)
            {
                Sessions.Remove(sessionToRemove);
            }
        });

        this.UpdateSN();
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public static implicit operator DataGrid(SessionViewModel v)
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
