using NETServer.Application.Helper;
using NETServer.Infrastructure.Services;
using NETServer.Infrastructure.Interfaces;

namespace NETServer.Application.Handlers.Client
{
    internal class AuthService
    {
        public void Register(IClientSession session, byte[] data)
        {
            string result = ByteHelper.ToString(data);
            List<string> resultList = new(result.Split('|'));

            

        }
        public void Login(IClientSession session, byte[] data) { }
        public void ChangePassword(IClientSession session, byte[] data) { }
    }
}
