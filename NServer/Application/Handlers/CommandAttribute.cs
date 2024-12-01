using NServer.Application.Handlers.Enums;

namespace NServer.Application.Handlers
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    internal class CommandAttribute(Cmd command) : System.Attribute
    {
        public Cmd Command { get; } = command;
    }
}