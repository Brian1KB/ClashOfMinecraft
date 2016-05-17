using MiNET;
using MiNET.Effects;

namespace ClashOfMinecraft
{
    internal class Stun : Effect
    {
        public Stun() : base(EffectType.Slowness)
        {
        }

        public override void SendAdd(Player player)
        {
            player.MovementSpeed = 0;
            player.SendUpdateAttributes();

            base.SendAdd(player);
        }

        public override void SendUpdate(Player player)
        {
            player.MovementSpeed = 0;
            player.SendUpdateAttributes();

            base.SendUpdate(player);
        }

        public override void SendRemove(Player player)
        {
            player.MovementSpeed = 0.1f;
            player.SendUpdateAttributes();

            base.SendRemove(player);
        }
    }
}
