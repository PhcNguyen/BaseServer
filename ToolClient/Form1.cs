using ToolClient.Core.Network;
using ToolClient.Core;

namespace ToolClient
{
    public partial class Tool : Form
    {
        private TCPClientManager _tcpClientManager;
        private ConsoleManager _consoleManager;

        public Tool()
        {
            InitializeComponent();
            _tcpClientManager = new TCPClientManager();
            _tcpClientManager.ConsoleMessage += (message, color, fontStyle) => _consoleManager?.PrintMessage(message, color, fontStyle);

            _consoleManager = new ConsoleManager(Console);

            this.Initialization();
        }

        public void Initialization()
        {
            foreach (PacketFlags flag in Enum.GetValues(typeof(PacketFlags)))
            {
                // Thêm các giá trị có thể chọn từ enum vào ComboBox
                comboFlags.Items.Add(flag);
            }

            comboFlags.SelectedItem = PacketFlags.NONE;

            foreach (Cmd cmd in Enum.GetValues(typeof(Cmd)))
            {
                // Thêm các giá trị có thể chọn từ enum vào ComboBox
                comboCmd.Items.Add(cmd);
            }

            comboCmd.SelectedItem = Cmd.NONE;
        }

        private void Connect_Click(object sender, EventArgs e)
        {
            ConnectServer.Enabled = false;

            if (string.IsNullOrWhiteSpace(TextIP.Text) || string.IsNullOrWhiteSpace(TextPort.Text))
            {
                _consoleManager.PrintMessage("IP và Port không được để trống!", Color.Red, FontStyle.Bold);
                ConnectServer.Enabled = true;
                return;
            }

            _tcpClientManager.Connect(TextIP.Text, int.Parse(TextPort.Text));

            if (_tcpClientManager.IsConnected)
            {
                DisconnectServer.Enabled = true;
                SendData.Enabled = true;
            }
            else { ConnectServer.Enabled = true; }
        }

        private void Disconnect_Click(object sender, EventArgs e)
        {
            _tcpClientManager.Disconnect();
            DisconnectServer.Enabled = false;
            ConnectServer.Enabled = true;
            SendData.Enabled = false;
        }

        private void SendData_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextPayload.Text))
            {
                _consoleManager.PrintMessage("Payload không được để trống!", Color.Red, FontStyle.Bold);
                return;
            }

            if (comboCmd.SelectedItem == null)
            {
                _consoleManager.PrintMessage("Vui lòng chọn một cmd.");
                return;
            }

            if (comboFlags.SelectedItem == null)
            {
                _consoleManager.PrintMessage("Vui lòng chọn một flag.");
                return;
            }

            PacketFlags selectedFlags = (PacketFlags)comboFlags.SelectedItem;
            Cmd cmd = (Cmd)comboCmd.SelectedItem;

            var packet = new Packet((byte)selectedFlags, (sbyte)cmd, 0, ConverterHelper.ToBytes(TextPayload.Text));
            _tcpClientManager.SendData(packet.ToByteArray());
        }
    }
}