using NPServer.Infrastructure.Logging.Interfaces;
using System;
using System.Text;
using System.Windows.Controls;

namespace NPServer.AdminPanel;

public class NLogWinFormTagers(TextBox textBox)
    : INLogPrintTagers
{
    private readonly TextBox _textBox = textBox ?? throw new ArgumentNullException(nameof(textBox));
    private readonly StringBuilder _logBuilder = new();
    private int _line = 0;

    public void WriteLine(string text)
    {
        if (System.Windows.Application.Current.Dispatcher.CheckAccess())
        {
            AppendText(text);
        }
        else
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => AppendText(text));
        }
    }

    private void AppendText(string text)
    {
        _logBuilder.AppendLine($"{++_line:D5} - {text}");

        _textBox.Text = _logBuilder.ToString();
    }

    public void ClearText()
    {
        if (System.Windows.Application.Current.Dispatcher.CheckAccess())
        {
            _textBox.Clear();
            _textBox.ScrollToEnd();
        }
        else
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                _textBox.Clear();
                _textBox.ScrollToEnd();
            });
        }
    }
}