using System.Timers;
using ClashOfMinecraft.Items;
using MiNET.Items;
using MiNET.Utils;
using MiNET.Worlds;

namespace ClashOfMinecraft
{
    internal class CrBattle
    {
        public Timer BattleTimer;

        private readonly BattleStartResponse _response;

        private readonly CrPlayer _playerOne;
        private readonly CrPlayer _playerTwo;

        private readonly PlayerLocation _spawnLocationOne;
        private readonly PlayerLocation _spawnLocationTwo;

        private readonly Level _respawnLevel;
        private readonly Level _battleLevel;

        public CrBattle(LevelFactory levelFactory, CrLevelManager levelManager, BattleStartResponse response)
        {
            _playerOne = (CrPlayer) levelManager.GetPlayer(response.PlayerName1);
            _playerTwo = (CrPlayer) levelManager.GetPlayer(response.PlayerName2);

            if (_playerOne == null || _playerTwo == null) return;

            var levelProvider = levelFactory.GetLevelProvider(@"C:\Users\Administrator\Desktop\MiNET Development Server\Worlds\" + response.Arena);

            _response = response;
            _respawnLevel = _playerOne.Level;
            _battleLevel = new Level("battle-level", levelProvider);
            _battleLevel.Initialize();

            var spawnLocations = levelManager.GetSpawnLocations(string.Format(@"C:\Users\Administrator\Desktop\MiNET Development Server\Worlds\{0}\{1}.json", response.Arena, response.Arena));

            _spawnLocationOne = spawnLocations.PlayerLocationOne;
            _spawnLocationTwo = spawnLocations.PlayerLocationTwo;

            PrepareLevel();
        }

        private void PrepareLevel()
        {
            _battleLevel.BlockPlace += delegate (object sender, BlockPlaceEventArgs args)
            {
                args.Player.Inventory.SetInventorySlot(args.Player.Inventory.InHandSlot,
                    args.Player.Inventory.GetItemInHand().Count > 1
                        ? ItemFactory.GetItem(46, 0, --args.Player.Inventory.GetItemInHand().Count)
                        : new ItemAir());

                if (args.Player.Inventory.GetItemInHand().Id == 46)
                {
                    new CrBombItem(_battleLevel)
                    {
                        KnownPosition = new PlayerLocation(args.ExistingBlock.Coordinates.X, args.ExistingBlock.Coordinates.Y, args.ExistingBlock.Coordinates.Z),
                        Fuse = 60
                    }.SpawnEntity();
                }

                args.Cancel = true;
            };

            _battleLevel.BlockBreak += delegate (object sender, BlockBreakEventArgs args)
            {
                args.Cancel = true;
            };
        }

        public void StartBattle()
        {
            _playerOne.SetBattle(this);
            _playerTwo.SetBattle(this);

            _playerOne.SetExperience(10, 1);
            _playerTwo.SetExperience(10, 1);

            _playerOne.SetBattleNameTag();
            _playerTwo.SetBattleNameTag();

            _response.PlayerMessage1.SendMessage(_playerOne);
            _response.PlayerMessage2.SendMessage(_playerTwo);

            _playerOne.SpawnLevel(_battleLevel, _spawnLocationOne);
            _playerTwo.SpawnLevel(_battleLevel, _spawnLocationTwo);

            CrItemBuilder.BuildItems(_playerOne, _response.PlayerItems1);
            CrItemBuilder.BuildItems(_playerTwo, _response.PlayerItems2);

            BattleTimer = new Timer();
            BattleTimer.Elapsed += BattleTick;
            BattleTimer.Interval = 1000;
            BattleTimer.Enabled = true;
        }

        public void EndBattle(CrPlayer loser)
        {
            _playerOne.SetBattle(null);
            _playerTwo.SetBattle(null);

            _playerOne.Inventory.Clear();
            _playerTwo.Inventory.Clear();

            _playerOne.SpawnLevel(_respawnLevel);
            _playerTwo.SpawnLevel(_respawnLevel);

            var battleEndResponse = CrApiWrapper.BattleEnd(loser == _playerOne ? _playerTwo.Username : _playerOne.Username, _response.BattleID, 1);

            var playerOneInfo = CrApiWrapper.PlayerInfo(_playerOne.Username);
            var playerTwoInfo = CrApiWrapper.PlayerInfo(_playerTwo.Username);

            battleEndResponse.PlayerMessage1.SendMessage(_playerOne);
            battleEndResponse.PlayerMessage2.SendMessage(_playerTwo);

            _playerOne.SetExperience(playerOneInfo.Rank, (float)(playerOneInfo.Rank - playerOneInfo.RankLower) / (playerOneInfo.RankHigher - playerOneInfo.RankLower));
            _playerTwo.SetExperience(playerTwoInfo.Rank, (float)(playerTwoInfo.Rank - playerTwoInfo.RankLower) / (playerTwoInfo.RankHigher - playerTwoInfo.RankLower));

            _playerOne.ColouredRank = playerOneInfo.PlayerColor.Replace('&', '§') + playerOneInfo.Rank;
            _playerTwo.ColouredRank = playerTwoInfo.PlayerColor.Replace('&', '§') + playerTwoInfo.Rank;

            _playerOne.SetHubNameTag();
            _playerTwo.SetHubNameTag();
        }

        private int _startTicks = 11;

        private void BattleTick(object sender, ElapsedEventArgs e)
        {
            _startTicks--;

            _playerOne.SetExperience(_startTicks, _startTicks / 11F);
            _playerTwo.SetExperience(_startTicks, _startTicks / 11F);

            if (_startTicks != 0) return;

            CrItemBuilder.ApplyItemsEffects(_playerOne, _response.PlayerItems1);
            CrItemBuilder.ApplyItemsEffects(_playerTwo, _response.PlayerItems2);

            BattleTimer.Enabled = false;
            BattleTimer = null;
        }
    }
}
