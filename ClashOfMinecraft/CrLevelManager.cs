using MiNET;
using MiNET.Worlds;

namespace ClashOfMinecraft
{
    class CrLevelManager : LevelManager
    {
        public Player GetPlayer(string name)
        {
            foreach (var level in Levels)
            {
                foreach (var player in level.Players.Values)
                {
                    if (player.Username.ToLower().Equals(name.ToLower()))
                    {
                        return player;
                    }
                }
            }

            return null;
        }
    }
}
