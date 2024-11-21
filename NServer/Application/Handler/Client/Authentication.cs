using NServer.Core.Database;
using NServer.Core.Logging;
using NServer.Core.Packet;
using NServer.Infrastructure.Helper;
using NServer.Interfaces.Core.Network;

namespace NServer.Application.Handler.Client
{
    internal class Authentication
    {
        [Command(Cmd.REGISTER)]
        public static async Task Register(ISession session, byte[] data)
        {
            Packets packet = new();
            string result = ByteConverter.ToString(data);
            string[] parts = result.Split('|');

            if (parts.Length != 2)
            {
                packet.SetCommand((short)Cmd.ERROR);
                packet.SetPayload("Invalid registration data format.");

                session.Send(packet.ToByteArray());
                return;
            }

            string email = parts[0];
            string password = parts[1];

            if (!Validator.IsEmailValid(email))
            {
                packet.SetCommand((short)Cmd.ERROR);
                packet.SetPayload("Invalid email format.");

                session.Send(packet.ToByteArray());
                return;
            }

            if (!Validator.IsPasswordValid(password))
            {
                packet.SetCommand((short)Cmd.ERROR);
                packet.SetPayload("Password does not meet the required criteria.");

                session.Send(packet.ToByteArray());
                return;
            }

            try
            {
                string query = "INSERT INTO account (email, password) VALUES (@params0, @params1)";

                if (await PostgreManager.ExecuteAsync(query, email, password))
                {
                    packet.SetCommand((short)Cmd.SUCCESS);
                    packet.SetPayload("Registration successful.");

                    session.Send(packet.ToByteArray());
                }
                else
                {
                    packet.SetCommand((short)Cmd.ERROR);
                    packet.SetPayload("Registration failed. Please try again.");

                    session.Send(packet.ToByteArray());
                }
            }
            catch (Exception ex)
            {
                NLog.Error($"Registration failed for {email}: {ex.Message}");

                packet.SetCommand((short)Cmd.ERROR);
                packet.SetPayload("An error occurred during registration.");

                session.Send(packet.ToByteArray());
            }
        }

        [Command(Cmd.LOGIN)]
        public void Login(ISession session, byte[] data)
        {

        }


        public void ChangePassword(ISession session, byte[] data) { }
    }
}
