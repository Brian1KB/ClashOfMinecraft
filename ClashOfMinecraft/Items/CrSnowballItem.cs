using ClashOfMinecraft.Projectiles;
using MiNET;
using MiNET.Items;
using MiNET.Utils;
using MiNET.Worlds;

namespace ClashOfMinecraft.Items
{
    class CrSnowballItem : Item
    {
        public CrSnowballItem() : base(332)
		{
            MaxStackSize = 16;
        }

        public override void UseItem(Level world, Player player, BlockCoordinates blockCoordinates)
        {
            if (player.GameMode != GameMode.Creative)
            {
                Item itemStackInHand = player.Inventory.GetItemInHand();
                itemStackInHand.Count--;

                if (itemStackInHand.Count <= 0)
                {
                    player.Inventory.Slots[player.Inventory.Slots.IndexOf(itemStackInHand)] = new ItemAir();
                }
            }

            float force = 1.5f;

            CrSnowball snowBall = new CrSnowball(player, world);
            snowBall.KnownPosition = (PlayerLocation)player.KnownPosition.Clone();
            snowBall.KnownPosition.Y += 1.62f;
            snowBall.Velocity = snowBall.KnownPosition.GetDirection() * (force);
            snowBall.BroadcastMovement = false;
            snowBall.DespawnOnImpact = true;
            snowBall.SpawnEntity();
        }
    }
}
