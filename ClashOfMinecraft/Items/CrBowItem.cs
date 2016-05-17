using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClashOfMinecraft.Projectiles;
using log4net;
using MiNET;
using MiNET.Blocks;
using MiNET.Entities.Projectiles;
using MiNET.Items;
using MiNET.Utils;
using MiNET.Worlds;

namespace ClashOfMinecraft.Items
{
    class CrBowItem : Item
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ItemBow));

        public CrBowItem() : base(261)
		{
            MaxStackSize = 1;
        }

        public override void Release(Level world, Player player, BlockCoordinates blockCoordinates, long timeUsed)
        {
            var inventory = player.Inventory;
            double damage = 0;
            bool haveArrows = false;
            for (byte i = 0; i < inventory.Slots.Count; i++)
            {
                var itemStack = inventory.Slots[i];

                if (itemStack.Id == 262)
                {
                    if (--itemStack.Count <= 0)
                    {
                        damage = ((CrPlayer)player).CalculateItemStrength(itemStack).Item1;
                        player.SendPlayerInventory();
                    }
                    haveArrows = true;
                    break;
                }
            }

            if (!haveArrows) return;
            if (timeUsed < 6) return; // questionable, but we go with it for now.

            float force = CalculateForce(timeUsed);
            if (force < 0.1D) return;

            Log.Warn($"Force {force}, time {timeUsed}");

            CrArrow arrow = new CrArrow(player, world, !(force < 1.0), (int)damage);
            arrow.KnownPosition = (PlayerLocation)player.KnownPosition.Clone();
            arrow.KnownPosition.Y += 1.62f;

            arrow.Velocity = arrow.KnownPosition.GetDirection() * (force * 2.0f * 1.5f);
            arrow.KnownPosition.Yaw = (float)arrow.Velocity.GetYaw();
            arrow.KnownPosition.Pitch = (float)arrow.Velocity.GetPitch();
            arrow.BroadcastMovement = false;
            arrow.DespawnOnImpact = true;

            arrow.SpawnEntity();
        }

        private float CalculateForce(long timeUsed)
        {
            float force = timeUsed / 20.0F;

            force = ((force * force) + (force * 2.0F)) / 3.0F;
            if (force < 0.1D)
            {
                return 0;
            }

            if (force > 1.0F)
            {
                force = 1.0F;
            }

            return force;
        }

        public Vector3 GetShootVector(double motX, double motY, double motZ, double f, double f1)
        {
            double f2 = Math.Sqrt(motX * motX + motY * motY + motZ * motZ);

            motX /= f2;
            motY /= f2;
            motZ /= f2;

            motX *= f;
            motY *= f;
            motZ *= f;

            return new Vector3(motX, motY, motZ);
        }
    }
}
