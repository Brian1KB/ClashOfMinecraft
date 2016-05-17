using System.Collections.Generic;
using MiNET;

namespace ClashOfMinecraft
{
    internal class PlayerOnlineResponse
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
    }

    internal class PlayerOfflineResponse
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
    }

    internal class BattleQueueResponse
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
    }

    internal class BattleStartResponse
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string PlayerName1 { get; set; }
        public List<CrItem> PlayerItems1 { get; set; }
        public PlayerMessage PlayerMessage1 { get; set; }
        public string PlayerName2 { get; set; }
        public List<CrItem> PlayerItems2 { get; set; }
        public PlayerMessage PlayerMessage2 { get; set; }
        public string Arena { get; set; }
        public int BattleID { get; set; }
    }

    internal class BattleEndResponse
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public PlayerMessage PlayerMessage1 { get; set; }
        public PlayerMessage PlayerMessage2 { get; set; }
    }

    public class PlayerMessage
    {
        public string Popup { get; set; }
        public string Tip { get; set; }
        public string Chat { get; set; }

        public void SendMessage(Player player)
        {
            if (Popup != "")
                player.SendMessage(Popup.Replace('&', '§'), MessageType.Popup);
            
            if(Tip != "")
                player.SendMessage(Tip.Replace('&', '§'), MessageType.Tip);

            if(Chat != "")
                player.SendMessage(Chat.Replace('&', '§'));
        }
    }

    internal class PlayerInfoResponse
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public int Rank { get; set; }
        public int RankLower { get; set; }
        public int RankHigher { get; set; }
        public List<CrLevelItem> Items { get; set; }
        public List<CrDeckItem> Deck { get; set; }
        public List<CrChest> Chests { get; set; } 
        public string PlayerColor { get; set; }
    }

    internal class CrChest
    {
    }

    internal class CrItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string MCPEID { get; set; }
        public int Level { get; set; }
        public double Strength { get; set; }
        public double Strength2 { get; set; }
    }

    internal class CrLevelItem
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public int Amount { get; set; }
        public int LevelUpAmount { get; set; }
    }

    internal class CrDeckItem
    {
        public string Name { get; set; }
        public int Level { get; set; }
    }
}