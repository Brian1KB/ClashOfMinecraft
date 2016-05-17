using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MiNET;
using MiNET.Entities;
using MiNET.Utils;

namespace ClashOfMinecraft
{
    internal class CrHealthManager : HealthManager
    {
        public CrHealthManager(Entity entity) : base(entity)
        {
        }

        public override void TakeHit(Entity source, int damage = 1, DamageCause cause = DamageCause.Unknown)
        {
            if (Health - (damage*10) <= 0)
            {
                Kill();
            }
            else
            {
                base.TakeHit(source, damage, cause);
                ((CrPlayer) Entity).SetNameTag(TextUtils.Center(ChatColors.DarkGray + "» " + ((CrPlayer)Entity).ColouredRank + ChatColors.DarkGray + " « " + ChatColors.Yellow + ((CrPlayer) Entity).Username + "\n" + ChatColors.Green + ChatColors.Red + Hearts + ChatColors.Green + " Hearts"));
            }
        }

        public override void Regen(int amount = 1)
        {
        }

        public override void Kill()
        {
            var player = (CrPlayer)Entity;

            ResetHealth();
            player.SendUpdateAttributes();
            player.BroadcastEntityEvent();

            Task.Run(delegate
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                Console.WriteLine("[Performance Check][Kill] Beginning");

                if (player.IsInBattle())
                {
                    var battle = player.GetBattle();

                    Console.WriteLine(battle != null);

                    battle.EndBattle(player);
                }

                Console.WriteLine("[Performance Check][Kill] Task elapsed time: " + watch.ElapsedMilliseconds);
                watch.Stop();
            });
        }
    }
}