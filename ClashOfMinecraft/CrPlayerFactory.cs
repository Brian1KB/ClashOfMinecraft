using System.Net;
using MiNET;
using MiNET.Utils;

namespace ClashOfMinecraft
{
    internal class CrPlayerFactory : PlayerFactory
    {
        public override Player CreatePlayer(MiNetServer server, IPEndPoint endPoint)
        {
            var player = new CrPlayer(server, endPoint);
            player.MaxViewDistance = 5;
            OnPlayerCreated(new PlayerEventArgs(player));
            return player;
        }
    }
}