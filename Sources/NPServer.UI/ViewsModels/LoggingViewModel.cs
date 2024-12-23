using NPServer.Infrastructure.Logging.Interfaces;
using NPServer.UI.Core.Metadata;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace NPServer.UI.ViewsModels;

internal class LoggingViewModel : INLogPrintTagers, INotifyPropertyChanged
{
    private readonly ObservableCollection<LogEntry> _logEntries = [];
    private readonly int _itemsPerPage = 20; // Số lượng log mỗi trang
    private int _currentPage = 0; // Trang hiện tại

    public event PropertyChangedEventHandler PropertyChanged;

    public void WriteLine(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            _logEntries.Add(new LogEntry { Message = text });
            OnPropertyChanged(nameof(LogEntries));
        }
    }

    public IEnumerable<LogEntry> LogEntries => _logEntries.Skip(_currentPage * _itemsPerPage).Take(_itemsPerPage);

    public void PreviousPage()
    {
        if (_currentPage > 0)
        {
            _currentPage--;
            OnPropertyChanged(nameof(LogEntries));
        }
    }

    public void NextPage()
    {
        if ((_currentPage + 1) * _itemsPerPage < _logEntries.Count)
        {
            _currentPage++;
            OnPropertyChanged(nameof(LogEntries));
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
