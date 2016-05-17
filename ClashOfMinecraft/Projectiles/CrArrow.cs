using MiNET;
using MiNET.Entities.Projectiles;
using MiNET.Worlds;

namespace ClashOfMinecraft.Projectiles
{
    internal class CrArrow : Arrow
    {
        public CrArrow(Player shooter, Level level, bool isCritical, int damage) : base(shooter, level, isCritical)
        {
            Damage = damage;
        }
    }
}