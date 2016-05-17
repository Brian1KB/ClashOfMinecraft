using MiNET;

namespace ClashOfMinecraft
{
    internal class CrMotdProvider : MotdProvider
    {
        public override string GetMotd(ServerInfo serverInfo)
        {
            NumberOfPlayers = serverInfo.NumberOfPlayers;
            MaxNumberOfPlayers = serverInfo.MaxNumberOfPlayers;

            return string.Format(@"MCPE;{0};60;0.14.2;{1};{2}", "§8§l» §cCraft§bRoyale §eBETA §8§l«", NumberOfPlayers, MaxNumberOfPlayers);
        }
    }
}