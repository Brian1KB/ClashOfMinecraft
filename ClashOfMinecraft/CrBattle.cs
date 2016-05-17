using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Timers;
using log4net;
using MiNET;
using MiNET.Items;
using MiNET.Utils;
using MiNET.Worlds;
using ClashOfMinecraft.Items;

namespace ClashOfMinecraft
{
    internal class CrBattle
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CraftRoyale));

        private readonly int battleId;

        private readonly Player playerOne;
        private readonly Player playerTwo;

        private readonly Level battleLevel;
        private readonly Level hubLevel;

        private List<CrItem> playerItemsOne;
        private List<CrItem> playerItemsTwo;

        public CrBattle(LevelFactory levelFactory, BattleStartResponse battleStartResponse, Player playerOne, Player playerTwo, Level hubLevel)
        {
            this.battleId = battleStartResponse.BattleID;
            this.hubLevel = hubLevel;
            this.playerOne = playerOne;
            this.playerTwo = playerTwo;

            var provider = levelFactory.GetLevelProvider(@"C:\Users\Administrator\Desktop\MiNET Development Server\Worlds\" + battleStartResponse.Arena);
            battleLevel = new Level("battle", provider, GameMode.Survival, Difficulty.Normal, 5);
            battleLevel.Initialize();

            PrepareBattleLevel(battleLevel, battleStartResponse);
        }

        private void PrepareBattleLevel(Level battleLevel, BattleStartResponse battleStartResponse)
        {
            battleLevel.BlockPlace += delegate (object sender, BlockPlaceEventArgs args)
            {
                args.Cancel = true;

                if (args.Player.Inventory.GetItemInHand().Count > 1)
                {
                    args.Player.Inventory.SetInventorySlot(args.Player.Inventory.InHandSlot, ItemFactory.GetItem(46, 0, --args.Player.Inventory.GetItemInHand().Count));
                }
                else
                {
                    args.Player.Inventory.SetInventorySlot(args.Player.Inventory.InHandSlot, new ItemAir());
                }

                if (args.Player.Inventory.GetItemInHand().Id == 46)
                {
                    new CrBombItem(battleLevel)
                    {
                        KnownPosition = new PlayerLocation(args.ExistingBlock.Coordinates.X, args.ExistingBlock.Coordinates.Y, args.ExistingBlock.Coordinates.Z),
                        Fuse = (byte)60
                    }.SpawnEntity();
                }
            };

            battleLevel.BlockBreak += delegate (object sender, BlockBreakEventArgs args)
            {
                args.Cancel = true;
            };

            var positionOne = new PlayerLocation(0, 0, 0);
            var positionTwo = new PlayerLocation(0, 0, 0);

            try
            {
                using (
                    FileStream stream =
                        new FileStream(
                            @"C:\Users\Administrator\Desktop\MiNET Development Server\Worlds\" + battleStartResponse.Arena + @"\" + battleStartResponse.Arena + ".json",
                            FileMode.Open))
                {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(MapConfiguration));
                    MapConfiguration config = (MapConfiguration)ser.ReadObject(stream);

                    float[] positionOneSplit = Array.ConvertAll(config.PositionOne.Split(':'), float.Parse);
                    float[] positionTwoSplit = Array.ConvertAll(config.PositionTwo.Split(':'), float.Parse);

                    positionOne = new PlayerLocation(positionOneSplit[0], positionOneSplit[1], positionOneSplit[2])
                    {
                        Pitch = positionOneSplit[3],
                        Yaw = positionOneSplit[4],
                        HeadYaw = positionOneSplit[4]
                    };

                    positionTwo = new PlayerLocation(positionTwoSplit[0], positionTwoSplit[1], positionTwoSplit[2])
                    {
                        Pitch = positionTwoSplit[3],
                        Yaw = positionTwoSplit[4],
                        HeadYaw = positionTwoSplit[4]
                    };
                }
            }
            catch (Exception e)
            {
                Log.Warn("Error: ", e);
            }

            ((CrPlayer)playerOne).SetBattle(this);
            ((CrPlayer)playerTwo).SetBattle(this);

            playerOne.ExperienceLevel = 10;
            playerOne.Experience = 1;
            playerTwo.ExperienceLevel = 10;
            playerTwo.Experience = 1;

            playerOne.SendUpdateAttributes();

            playerOne.SpawnLevel(battleLevel, positionOne);
            playerTwo.SpawnLevel(battleLevel, positionTwo);

            playerOne.SetNameTag(TextUtils.Center(ChatColors.DarkGray + "» " + ((CrPlayer)playerOne).ColouredRank + ChatColors.DarkGray + " « " + ChatColors.Yellow + playerOne.Username + "\n" + ChatColors.Green + ChatColors.Red + "20" + ChatColors.Green + " Hearts"));
            playerTwo.SetNameTag(TextUtils.Center(ChatColors.DarkGray + "» " + ((CrPlayer)playerTwo).ColouredRank + ChatColors.DarkGray + " « " + ChatColors.Yellow + playerTwo.Username + "\n" + ChatColors.Green + ChatColors.Red + "20" + ChatColors.Green + " Hearts"));

            playerItemsOne = battleStartResponse.PlayerItems1;
            playerItemsTwo = battleStartResponse.PlayerItems1;

            ParseItems(battleStartResponse.PlayerItems1, playerOne);
            ParseItems(battleStartResponse.PlayerItems2, playerTwo);

            battleStartResponse.PlayerMessage1.SendMessage(playerOne);
            battleStartResponse.PlayerMessage2.SendMessage(playerTwo);

            _startTimer = new Timer();
            _startTimer.Elapsed += new ElapsedEventHandler(StartTick);
            _startTimer.Interval = 1000;
            _startTimer.Enabled = true;
        }

        public Timer _startTimer;
        public int _startTicks = 11;

        private void StartTick(object sender, ElapsedEventArgs e)
        {
            _startTicks--;

            playerOne.ExperienceLevel = _startTicks;
            playerOne.Experience = _startTicks / 11F;
            playerTwo.ExperienceLevel = _startTicks;
            playerTwo.Experience = _startTicks / 11F;

            playerOne.SendUpdateAttributes();
            playerTwo.SendUpdateAttributes();

            if (_startTicks != 0) return;

            ApplyItemEffects();
            _startTimer.Enabled = false;
            _startTimer = null;
        }

        private void ParseItems(List<CrItem> playerItems, Player player)
        {
            foreach (var crItem in playerItems)
            {
                var item = CrItemBuilder.BuildItem(crItem);

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

            var players = new Player[1];
            players[0] = player == playerOne ? playerTwo : playerOne;

            player.SendPlayerInventory();
            player.SendArmorForPlayer(players);
        }

        private void ApplyItemEffects()
        {
            foreach (var item in playerItemsOne)
            {
                CrItemBuilder.ApplyItemEffects(item, playerOne);
            }

            foreach (var item in playerItemsTwo)
            {
                CrItemBuilder.ApplyItemEffects(item, playerTwo);
            }
        }

        public void EndBattle(CrPlayer loserPlayer)
        {
            ((CrPlayer)playerOne).SetBattle(null);
            ((CrPlayer)playerTwo).SetBattle(null);

            BattleEndResponse response;

            if (playerOne == loserPlayer)
            {
                response = CrApiWrapper.BattleEnd(playerTwo.Username, battleId, 1);
            }
            else
            {
                response = CrApiWrapper.BattleEnd(playerOne.Username, battleId, 1);
            }

            response.PlayerMessage1.SendMessage(playerOne);
            response.PlayerMessage2.SendMessage(playerTwo);

            playerOne.Inventory.Clear();
            playerTwo.Inventory.Clear();

            playerOne.SpawnLevel(hubLevel);
            playerTwo.SpawnLevel(hubLevel);

            var playerOneInfo = CrApiWrapper.PlayerInfo(playerOne.Username);
            var playerTwoInfo = CrApiWrapper.PlayerInfo(playerTwo.Username);

            playerOne.ExperienceLevel = playerOneInfo.Rank;
            playerOne.Experience = (float)(playerOneInfo.Rank - playerOneInfo.RankLower) / (playerOneInfo.RankHigher - playerOneInfo.RankLower);

            playerTwo.ExperienceLevel = playerTwoInfo.Rank;
            playerTwo.Experience = (float)(playerTwoInfo.Rank - playerTwoInfo.RankLower) / (playerTwoInfo.RankHigher - playerTwoInfo.RankLower);

            ((CrPlayer)playerOne).ColouredRank = playerOneInfo.PlayerColor.Replace('&', '§') + playerOneInfo.Rank;
            ((CrPlayer)playerTwo).ColouredRank = playerTwoInfo.PlayerColor.Replace('&', '§') + playerTwoInfo.Rank;

            playerOne.SetNameTag(ChatColors.DarkGray + "» " + ((CrPlayer)playerOne).ColouredRank + ChatColors.DarkGray + " « " + ChatColors.Yellow + playerOne.Username);
            playerTwo.SetNameTag(ChatColors.DarkGray + "» " + ((CrPlayer)playerTwo).ColouredRank + ChatColors.DarkGray + " « " + ChatColors.Yellow + playerTwo.Username);

            playerOne.SendUpdateAttributes();
            playerTwo.SendUpdateAttributes();
        }
    }
}
