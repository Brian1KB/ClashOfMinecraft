using System.Linq;
using System.Threading.Tasks;
using MiNET;
using MiNET.Entities;
using MiNET.Net;
using MiNET.Utils;
using MiNET.Worlds;

namespace ClashOfMinecraft.Items
{
    public class CrBombItem : Entity
    {
        public byte Fuse { get; set; }
        public bool Fire { get; set; }
        private bool CheckPosition = true;

        public CrBombItem(Level level) : base(65, level)
        {
        }

        public override MetadataDictionary GetMetadata()
        {
            MetadataDictionary metadata = new MetadataDictionary();
            metadata[16] = new MetadataByte(Fuse);

            return metadata;
        }

        public override void SpawnEntity()
        {
            KnownPosition.X += 0.5f;
            KnownPosition.Y += 0.5f;
            KnownPosition.Z += 0.5f;
            Fire = false;
            Fuse = (byte) 60;

            base.SpawnEntity();
        }

        public override void OnTick()
        {
            Fuse--;

            if (Fuse == 0)
            {
                DespawnEntity();
                Explode();
            }
            else
            {
                var entityData = McpeSetEntityData.CreateObject();
                entityData.entityId = EntityId;
                entityData.metadata = GetMetadata();
                Level.RelayBroadcast(entityData);
                if (CheckPosition) PositionCheck();
            }
        }

        private void PositionCheck()
        {
            var check = KnownPosition.GetCoordinates3D() + Level.Down;
            if (!Level.GetBlock(check).IsSolid)
            {
                KnownPosition.Y -= 1;
            }
            else
            {
                CheckPosition = false;
            }
        }

        private void Explode()
        {
            new Task(() =>
            {
                var mcpeExplode = McpeExplode.CreateObject();
                mcpeExplode.x = KnownPosition.X;
                mcpeExplode.y = KnownPosition.Y;
                mcpeExplode.z = KnownPosition.Z;
                mcpeExplode.radius = 3;
                mcpeExplode.records = new Records();
                Level.RelayBroadcast(mcpeExplode);
            }).Start();

            foreach (var player in Level.Players.Values.ToArray())
            {
                var distance = KnownPosition.DistanceTo(player.KnownPosition);

                if (distance < 5)
                {
                    player.HealthManager.TakeHit(this, (int) (50 / distance), DamageCause.EntityExplosion);
                }
            }
        }
    }
}