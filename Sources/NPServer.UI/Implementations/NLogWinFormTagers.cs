using System;
using System.Windows.Controls;
using NPServer.Infrastructure.Logging.Interfaces;

namespace NPServer.UI.Implementations;

public class NLogWinFormTagers(TextBox textBox)
    : INLogWinFormTagers
{
    private readonly TextBox _textBox = textBox ?? throw new ArgumentNullException(nameof(textBox));
    private int _line = 0;

    public void AppendText(string text)
    {
        if(System.Windows.Application.Current.Dispatcher.CheckAccess())
        {
            // Thêm vào nội dung hiện tại của TextBox
            _textBox.Text += $"{++_line:D5} - " + text + Environment.NewLine;
            _textBox.ScrollToEnd();
        }
        else
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                _textBox.Text += $"{++_line:D5} - " + text + Environment.NewLine;
                _textBox.ScrollToEnd();
            });
        }
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