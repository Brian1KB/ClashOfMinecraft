using MiNET.Entities;
using MiNET.Items;
using MiNET.Net;
using MiNET.Utils;
using MiNET.Worlds;

namespace ClashOfMinecraft
{
    internal class CrNpcFactory
    {
        public static Skin CreateSkin(string skinPath, bool slim = false)
        {
            var bytes = Skin.GetTextureFromFile(skinPath);

            return new Skin
            {
                Slim = slim,
                Texture = bytes
            };
        }

        public static PlayerMob CreateNpc(string username, Skin skin, Level level, PlayerLocation position)
        {
            var player = new PlayerMob(username, level)
            {
                Skin = skin,
                KnownPosition = position 
            };

            return player;
        }

        public static PlayerMob CreateNpc(string username, Skin skin, Level level, Item itemInHand, PlayerLocation position)
        {
            var player = new PlayerMob(username, level)
            {
                Skin = skin,
                KnownPosition = position,
                ItemInHand = itemInHand
            };

            return player;
        }

        public static PlayerMob CreateNpc(string username, Skin skin, Level level, Item itemInHand, short[] armour, PlayerLocation position)
        {
            var player = new PlayerMob(username, level)
            {
                Skin = skin,
                KnownPosition = position,
                ItemInHand = itemInHand,
                Helmet = armour[0],
                Chest = armour[1],
                Leggings = armour[2],
                Boots = armour[3]
            };

            return player;
        }
    }
}
