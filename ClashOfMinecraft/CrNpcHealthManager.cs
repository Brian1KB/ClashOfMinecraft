using System;
using MiNET;
using MiNET.Entities;

namespace ClashOfMinecraft
{
    internal class CrNpcHealthManager : HealthManager
    {
        private Action<Entity> TakeHitAction;

        public CrNpcHealthManager(Entity entity, Action<Entity> takeHitAction) : base(entity)
        {
            TakeHitAction = takeHitAction;
        }

        public override void TakeHit(Entity source, int damage = 1, DamageCause cause = DamageCause.Unknown)
        {
            if (cause.Equals(DamageCause.Custom))
            {
                TakeHitAction.Invoke(source);
            }
        }
    }
}
