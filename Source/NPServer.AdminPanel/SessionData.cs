using System.ComponentModel;

namespace NPServer.AdminPanel;

public class SessionData : INotifyPropertyChanged
{
    private int _serialNumber;

    public int SerialNumber
    {
        get => _serialNumber;
        set
        {
            if (_serialNumber != value)
            {
                _serialNumber = value;
                OnPropertyChanged(nameof(SerialNumber));
            }
        }
    }

    public string ID { get; set; }
    public string EndPoint { get; set; }
    public string Role { get; set; }
    public string FirstRecordingTime { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}