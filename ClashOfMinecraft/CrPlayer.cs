using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using fNbt;
using MiNET;
using MiNET.Effects;
using MiNET.Entities;
using MiNET.Items;
using MiNET.Net;
using MiNET.Utils;
using MiNET.Worlds;

namespace ClashOfMinecraft
{
    internal class CrPlayer : Player
    {
        public string ColouredRank = "";
        private CrBattle _crBattle;
        private readonly CrCooldownTimer _npcCooldownTimer = new CrCooldownTimer(300);
         
        public CrPlayer(MiNetServer server, IPEndPoint endPoint) : base(server, endPoint)
        {
            HealthManager = new CrHealthManager(this);
            HungerManager = new AlwaysFullHungerManager(this);
        }

        public override void InitializePlayer()
        {
            base.InitializePlayer();

            Task.Run(delegate
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                Console.WriteLine("[Performance Check][Login] Beginning");
                CrApiWrapper.SetPlayerOnline(Username, 1);
                var playerInfo = CrApiWrapper.PlayerInfo(Username);

                ColouredRank = playerInfo.PlayerColor.Replace('&', '§') + playerInfo.Rank;
                ExperienceLevel = playerInfo.Rank;
                Experience = (float)(playerInfo.Rank - playerInfo.RankLower) / (playerInfo.RankHigher - playerInfo.RankLower);

                SetNameTag(ChatColors.DarkGray + "» " + ColouredRank + ChatColors.DarkGray + " « " + ChatColors.Yellow + Username);
                SendUpdateAttributes();

                SetEffect(new NightVision
                {
                    Duration = 1000000
                });

                SendMessage("Welcome back, " + ChatColors.Green + Username, MessageType.Tip);
                SendMessage(ChatColors.Gold + "To start a game, click on the NPCs!", MessageType.Popup);

                Console.WriteLine("[Performance Check][Login] Task elapsed time: " + watch.ElapsedMilliseconds);
                watch.Stop();
            });
        }

        public override void Disconnect(string reason, bool sendDisconnect = true)
        {
            Task.Run(delegate
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                Console.WriteLine("[Performance Check][Disconnect] Beginning");
                if (IsInBattle())
                {
                    var battle = GetBattle();
                    battle.EndBattle(this);
                }

                CrApiWrapper.SetPlayerOffline(Username, 1);
                Console.WriteLine("[Performance Check][Disconnect] Task elapsed time: "+watch.ElapsedMilliseconds);
                watch.Stop();
            });
            
            base.Disconnect(reason, sendDisconnect);
        }

        protected override bool AcceptPlayerMove(McpeMovePlayer message)
        {
            if (IsInBattle())
            {
                var battle = GetBattle();

                if (battle._startTimer != null)
                {
                    SetPosition(KnownPosition, false);
                    return false;
                }
            }

            return true;
        }

        protected override void HandleMessage(McpeText message)
        {
            var text = message.message;
            if (text.StartsWith("/") || text.StartsWith("."))
            {
                Server.PluginManager.HandleCommand(Server.UserManager, text, this);
            }
            else
            {
                Level.BroadcastMessage(ColouredRank + " " + ChatColors.Yellow + Username + ChatColors.DarkGray + " » " + ChatColors.Gray + text);
            }
        }

        protected override void HandleEntityEvent(McpeEntityEvent message)
        {
            switch (message.eventId)
            {
                case 9:
                    if (GameMode.Equals(GameMode.Survival))
                    {
                        var itemInHand = Inventory.GetItemInHand();

                        if (itemInHand is FoodItem)
                        {
                            var foodItem = (FoodItem) Inventory.GetItemInHand();
    
                            foodItem.Consume(this);
                            foodItem.Count--;

                            SendPlayerInventory();
                        }
                        else if (itemInHand is ItemPotion)
                        {
                            var potion = (ItemPotion) Inventory.GetItemInHand();
                            var strength = CalculateItemStrength(potion).Item1;

                            HealthManager.Regen((int) strength);

                            potion.Count--;
                            SendPlayerInventory();
                        }
                    }
                    break;
            }
        }

        protected override void HandleInteract(McpeInteract message)
        {
            var target = Level.GetEntity(message.targetEntityId);

            if (target == null) return;

            if (IsInBattle())
            {
                if (message.actionId != 2) return;

                var player = target as Player;

                if (player != null)
                {
                    var itemInHand = Inventory.GetItemInHand();
                    double damage = itemInHand.GetDamage();

                    switch (GetItemCustomID(itemInHand))
                    {
                        case 9:
                            damage = CalculateItemStrength(itemInHand).Item1;
                            break;
                        case 10:
                            damage = CalculateItemStrength(itemInHand).Item1;
                            break;
                    }

                    if (IsFalling)
                    {
                        damage += Level.Random.Next((int) (damage/2 + 2));

                        var animate = McpeAnimate.CreateObject();
                        animate.entityId = target.EntityId;
                        animate.actionId = 4;
                        Level.RelayBroadcast(animate);
                    }

                    player.HealthManager.TakeHit(this, (int)CalculatePlayerDamage(player, damage),
                        DamageCause.EntityAttack);
                }

                HungerManager.IncreaseExhaustion(0.3f);
            }
            else
            {
                if (target is PlayerMob && _npcCooldownTimer.CanExecute())
                {
                    _npcCooldownTimer.Execute();
                    target.HealthManager.TakeHit(this, 0, DamageCause.Custom);
                }
            }
        }

        public void SetBattle(CrBattle battle)
        {
            _crBattle = battle;
        }

        public CrBattle GetBattle()
        {
            return _crBattle;
        }

        public bool IsInBattle()
        {
            return _crBattle != null;
        }

        public Tuple<double, double> CalculateItemStrength(Item tool)
        {
            if (tool == null) return new Tuple<double, double>(0, 0);
            if (tool.ExtraData == null) return new Tuple<double, double>(0, 0);

            NbtCompound custom;
            if (!tool.ExtraData.TryGet("craftroyale", out custom)) return new Tuple<double, double>(0, 0);

            return new Tuple<double, double>(custom["Strength1"].DoubleValue, custom["Strength2"].DoubleValue);
        }

        private int GetItemCustomID(Item tool)
        {
            if (tool?.ExtraData == null) return -1;

            NbtCompound custom;
            if (!tool.ExtraData.TryGet("craftroyale", out custom)) return -1;

            return custom["ID"].IntValue;
        }

        public override double CalculatePlayerDamage(Player target, double damage)
        {
            double armourValue = 0;

            if (target.Inventory.Helmet != null)
            {
                armourValue += CalculateItemStrength(target.Inventory.Helmet).Item1;
            }

            if (target.Inventory.Chest != null)
            {
                armourValue += CalculateItemStrength(target.Inventory.Chest).Item1;
            }

            if (target.Inventory.Leggings != null)
            {
                armourValue += CalculateItemStrength(target.Inventory.Leggings).Item1;
            }

            if (target.Inventory.Boots != null)
            {
                armourValue += CalculateItemStrength(target.Inventory.Boots).Item1;
            }

            damage = damage * (1 - Math.Max(armourValue / 5, armourValue - damage / 2) / 25);
            damage = damage * (1 - Math.Min(20, 0) / 25);

            return damage;
        }
    }
}