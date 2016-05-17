using System;
using System.Net.Http;
using Newtonsoft.Json;

namespace ClashOfMinecraft
{
    internal class CrApiWrapper
    {
        public static PlayerOnlineResponse SetPlayerOnline(string username, int serverId)
        {
            var requestUrl = "http://52.5.238.4/";
            var client = new HttpClient()
            {
                BaseAddress = new Uri(requestUrl)
            };

            var response = client.GetAsync("?action=PlayerOnline&PlayerName=" + username + "&ServerID=" + serverId).Result;

            return JsonConvert.DeserializeObject<PlayerOnlineResponse>(response.Content.ReadAsStringAsync().Result);
        }

        public static PlayerOfflineResponse SetPlayerOffline(string username, int serverId)
        {
            var requestUrl = "http://52.5.238.4/";
            var client = new HttpClient()
            {
                BaseAddress = new Uri(requestUrl),
            };

            var response = client.GetAsync("?action=PlayerOffline&PlayerName=" + username + "&ServerID=" + serverId).Result;

            return JsonConvert.DeserializeObject<PlayerOfflineResponse>(response.Content.ReadAsStringAsync().Result);
        }

        public static BattleQueueResponse BattleQueuePlayer(string username, int serverId, bool ranked)
        {
            var requestUrl = "http://52.5.238.4/";
            var client = new HttpClient()
            {
                BaseAddress = new Uri(requestUrl)
            };

            var response = client.GetAsync("?action=BattleQueuePlayer&PlayerName=" + username + "&ServerID=" + serverId + "&Ranked=" + ranked).Result;

            return JsonConvert.DeserializeObject<BattleQueueResponse>(response.Content.ReadAsStringAsync().Result);
        }

        public static BattleStartResponse BattleStart(int serverId)
        {
            var requestUrl = "http://52.5.238.4/";
            var client = new HttpClient()
            {
                BaseAddress = new Uri(requestUrl)
            };

            var response = client.GetAsync("?action=BattleStart&ServerID=" + serverId).Result;

            return JsonConvert.DeserializeObject<BattleStartResponse>(response.Content.ReadAsStringAsync().Result);
        }

        public static BattleEndResponse BattleEnd(string winnerUsername, int battleId, int serverId)
        {
            var requestUrl = "http://52.5.238.4/";
            var client = new HttpClient()
            {
                BaseAddress = new Uri(requestUrl)
            };

            var response = client.GetAsync("?action=BattleEnd&WinnerPlayerName=" + winnerUsername + "&ServerID=" + serverId + "&BattleID=" + battleId).Result;

            return JsonConvert.DeserializeObject<BattleEndResponse>(response.Content.ReadAsStringAsync().Result);
        }

        public static PlayerInfoResponse PlayerInfo(string username)
        {
            var requestUrl = "http://52.5.238.4/";

            var client = new HttpClient()
            {
                BaseAddress = new Uri(requestUrl)
            };

            var response = client.GetAsync("?action=PlayerInfo&PlayerName=" + username).Result;

            return JsonConvert.DeserializeObject<PlayerInfoResponse>(response.Content.ReadAsStringAsync().Result);
        }
    }
}
