using MiNET;
using MiNET.Entities;
using MiNET.Utils;
using MiNET.Worlds;

namespace ClashOfMinecraft
{
    internal class CrStatisticHologram : Entity
    {
        public CrStatisticHologram(Level level, string hologramText, Vector3 position) : base(64, level)
        {
            NameTag = hologramText;
            KnownPosition = new PlayerLocation(position);
        }

        public override MetadataDictionary GetMetadata()
        {
            var metadata = base.GetMetadata();
            metadata[15] = new MetadataByte(1);
            return metadata;
        }

        public override void OnTick()
        {
        }

        public override void SpawnToPlayers(Player[] players)
        {
            foreach (var player in players)
            {
                var response = new PlayerInfoResponse();
                KnownPosition = player.KnownPosition;

                NameTag = TextUtils.Center(ChatFormatting.Bold + ChatColors.Red + "Craft" + ChatColors.Aqua + "Royale " + ChatColors.Yellow + "ALPHA" + ChatFormatting.Reset + "\n  "
                    + ChatColors.DarkGray + response.Name + ChatColors.Yellow + "'s Stats" + "\n "
                    + ChatColors.Yellow + "Rank: " + ChatColors.Green + response.Rank);
                
                base.SpawnToPlayers(players);
            }
        }
    }
}
