namespace ToolClient.Core
{
    public class ConsoleManager(RichTextBox console)
    {
        private int _lineNumber = 0;
        private RichTextBox _console = console ?? throw new ArgumentNullException(nameof(console));

        public void PrintMessage(string text, Color? textColor = null, FontStyle fontStyle = FontStyle.Regular)
        {
            if (_console.InvokeRequired)
            {
                _console.Invoke((MethodInvoker)delegate
                {
                    Print(text, textColor, fontStyle);
                });
            }
            else
            {
                Print(text, textColor, fontStyle);
            }
        }

        private void Print(string text, Color? textColor, FontStyle fontStyle)
        {
            _console.SelectionStart = _console.TextLength;
            _console.SelectionLength = 0;
            _console.SelectionColor = textColor ?? Color.Black;
            _console.SelectionFont = new Font(_console.Font, fontStyle);
            _console.AppendText($"[{++_lineNumber}] {text}{Environment.NewLine}");
            _console.ScrollToCaret();
        }
    }
}
