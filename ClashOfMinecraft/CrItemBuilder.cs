using System.Collections.Generic;
using System.Linq;
using ClashOfMinecraft.Items;
using fNbt;
using MiNET;
using MiNET.Effects;
using MiNET.Items;

namespace ClashOfMinecraft
{
    class CrItemBuilder
    {
        public static Item BuildItem(CrItem crItem)
        {
            var itemIdSplit = crItem.MCPEID.Split(':');
            var item = ItemFactory.GetItem(short.Parse(itemIdSplit[0]));

            switch (short.Parse(itemIdSplit[0]))
            {
                case 281:
                    item = new CrBowItem();
                    break;
                case 46:
                    item.Count = (byte)crItem.Strength2;
                    break;
                case 262:
                    item.Count = (byte)crItem.Strength2;
                    break;
                case 332:
                    item = new CrSnowballItem();
                    item.Count = (byte)crItem.Strength2;
                    break;
            }

            if (itemIdSplit.Length > 1)
            {
                item.Metadata = short.Parse(itemIdSplit[1]);
            }

            item.ExtraData = new NbtCompound
            {
                new NbtCompound("display")
                {
                    new NbtString("Name", crItem.Name.Replace('&', '§'))
                },
                new NbtCompound("craftroyale")
                {
                    new NbtInt("ID", int.Parse(crItem.ID)),
                    new NbtDouble("Strength1", crItem.Strength),
                    new NbtDouble("Strength2", crItem.Strength2)
                }
            };

            return item;
        }

        public static void ApplyItemEffects(Player player, CrItem item)
        {
            switch (int.Parse(item.ID))
            {
                case 5:
                    player.SetEffect(new Speed()
                    {
                        Duration = 7000000,
                        Level = 1 * (int) item.Strength
                    });
                    break;
            }
        }

        internal static void ApplyItemsEffects(CrPlayer player, List<CrItem> playerItems)
        {
            foreach (var item in playerItems)
            {
                ApplyItemEffects(player, item);
            }
        }

        internal static void BuildItems(CrPlayer player, List<CrItem> playerItems)
        {
            foreach (var crItem in playerItems)
            {
                var item = BuildItem(crItem);

                switch (item.ItemType)
                {
                    case ItemType.Helmet:
                        player.Inventory.Helmet = item;
                        break;
                    case ItemType.Chestplate:
                        player.Inventory.Chest = item;
                        break;
                    case ItemType.Leggings:
                        player.Inventory.Leggings = item;
                        break;
                    case ItemType.Boots:
                        player.Inventory.Boots = item;
                        break;
                    default:
                        player.Inventory.SetFirstEmptySlot(item, true, false);
                        break;
                }
            }

            player.SendPlayerInventory();
            player.SendArmorForPlayer(player.Level.Players.Values.ToArray());
        }
    }
}
