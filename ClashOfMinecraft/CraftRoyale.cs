using System.Threading.Tasks;
using System.Timers;
using log4net;
using MiNET;
using MiNET.Entities;
using MiNET.Plugins;
using MiNET.Plugins.Attributes;
using MiNET.Utils;

namespace ClashOfMinecraft
{
    public class CraftRoyale : Plugin, IStartup
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CraftRoyale));
        private readonly LevelFactory _levelFactory = new LevelFactory();
        private Timer _queueTimer;

        public void Configure(MiNetServer server)
        {
            Log.Info("Startup begun.");

            server.MotdProvider = new CrMotdProvider();
            server.PlayerFactory = new CrPlayerFactory();
            server.LevelManager = new CrLevelManager();

            _queueTimer = new Timer();
            _queueTimer.Elapsed += HandleQueue;
            _queueTimer.Interval = 1000;
            _queueTimer.Enabled = true;

            Log.Info("Startup complete.");
        }

        protected override void OnEnable()
        {
            Context.LevelManager.LevelCreated += (sender, args) =>
            {
                var npc = CrNpcFactory.CreateNpc(TextUtils.Center(ChatFormatting.Bold + ChatColors.Red + "Craft" + ChatColors.Aqua + "Royale" + ChatColors.Yellow + " BETA" + ChatFormatting.Reset + "\n" + ChatFormatting.Bold + ChatColors.Blue + "»" + ChatColors.Green + " TAP TO PLAY " + ChatColors.Blue + "«"), CrNpcFactory.CreateSkin(@"C:\Users\Administrator\Desktop\MiNET Development Server\Skins\ultron-prime.png"), Context.LevelManager.Levels[0], new PlayerLocation(12.5, 6, 3.5));

                npc.KnownPosition.Yaw += 90;
                npc.KnownPosition.HeadYaw += 90;

                npc.HealthManager = new CrNpcHealthManager(npc, delegate (Entity entity)
                {
                    var player = entity as Player;

                    if (player == null) return;

                    JoinQueue(player);
                });

                npc.SpawnEntity();
            };
        }

        private void HandleQueue(object source, ElapsedEventArgs e)
        {
            Task.Run(delegate
            {
                var response = CrApiWrapper.BattleStart(1);

                if (Context.LevelManager.Levels.Count > 0)
                {
                    if (response.StatusCode == 0)
                    {
                        var battle = new CrBattle(_levelFactory, (CrLevelManager) Context.LevelManager, response);
                        battle.StartBattle();
                    }
                }
            });
        }

        [Command]
        public void Position(Player player)
        {
            player.SendMessage("X " + player.KnownPosition.X + " Y " + player.KnownPosition.Y + " Z " + player.KnownPosition.Z + " Pitch: " + player.KnownPosition.Pitch + " Yaw: " + player.KnownPosition.Yaw + " HeadYaw: " + player.KnownPosition.HeadYaw);
        }

        [Command]
        public void JoinQueue(Player player)
        {
            Task.Run(delegate
            {
                CrApiWrapper.BattleQueuePlayer(player.Username, 1, true);
            });

            player.SendMessage(ChatColors.Yellow + "Joined " + ChatColors.Aqua + "CraftRoyale" + ChatColors.Yellow + " queue");
        }
    }
}
