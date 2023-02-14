using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.NPCs;
using Polarities.NPCs.Esophage;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Biomes
{
    //TODO: Pestilence soul drops?
    public class WorldEvilInvasion : ModBiome
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.Event;

        public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Pestilence");

        public override string BestiaryIcon => (GetType().Namespace + "." + Name).Replace('.', '/') + "_BestiaryIcon";
        public override string BackgroundPath => (GetType().Namespace + "." + Name).Replace('.', '/') + "_MapBG";
        public override Color? BackgroundColor => base.BackgroundColor;

        public static Asset<Texture2D> EventIcon;

        public override void Load()
        {
            EventIcon = Request<Texture2D>((GetType().Namespace + "." + Name).Replace('.', '/') + "_EventIcon");
        }

        public override void Unload()
        {
            EventIcon = null;
        }

        public override bool IsBiomeActive(Player player)
        {
            return (player.ZoneCrimson || player.ZoneCorrupt) && player.ZoneOverworldHeight && PolaritiesSystem.worldEvilInvasion;
        }

        public override void SpecialVisuals(Player player, bool isActive)
        {
            player.ManageSpecialBiomeVisuals("Polarities:WorldEvilInvasion", isActive);
        }

        public static bool ValidNPC(int npcType)
        {
            return PolaritiesNPC.customNPCCapSlot.ContainsKey(npcType) && (PolaritiesNPC.customNPCCapSlot[npcType] == NPCCapSlotID.WorldEvilInvasion || PolaritiesNPC.customNPCCapSlot[npcType] == NPCCapSlotID.WorldEvilInvasionWorm);
        }

        public static float GetSpawnChance(bool correctWorldEvil)
        {
            //gradually shift from just the 'correct' evil to a 50/50 split between the two
            if (correctWorldEvil) return 1f;
            return 1 - PolaritiesSystem.worldEvilInvasionSize / (float)PolaritiesSystem.worldEvilInvasionSizeStart;
        }

        public static void StartInvasion()
        {
            if (PolaritiesSystem.worldEvilInvasion) return;
            //Checks amount of players for scaling
            int numPlayers = 0;
            for (int i = 0; i < 255; i++)
            {
                if (Main.player[i].active)
                {
                    numPlayers++;
                }
            }

            PolaritiesSystem.worldEvilInvasion = true;
            PolaritiesSystem.worldEvilInvasionSize = 100 * numPlayers;
            PolaritiesSystem.worldEvilInvasionSizeStart = PolaritiesSystem.worldEvilInvasionSize;

            string text = Language.GetTextValue("Mods.Polarities.StatusMessage.SpawnWorldEvilInvasion");
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.NewText(text, 175, 75, 255);
                return;
            }
            if (Main.netMode == NetmodeID.Server)
            {
                //Sync with net
                NetMessage.SendData(MessageID.ChatText, -1, -1, NetworkText.FromLiteral(text), 255, 175f, 75f, 255f, 0, 0, 0);
            }
        }

        public static void UpdateInvasion()
        {
            if (PolaritiesSystem.worldEvilInvasion)
            {
                if (PolaritiesSystem.worldEvilInvasionSize <= 0)
                {
                    foreach (Player player in Main.player)
                    {
                        if (player.active && !player.dead)
                        {
                            PolaritiesSystem.worldEvilInvasion = false;

                            if (!PolaritiesSystem.downedWorldEvilInvasion)
                            {
                                PolaritiesSystem.downedWorldEvilInvasion = true;
                                if (Main.netMode == NetmodeID.Server)
                                {
                                    NetMessage.SendData(MessageID.WorldData);
                                }
                            }

                            PolaritiesSystem.esophageSpawnTimer++;
                            break;
                        }
                    }
                }
            }
        }

        public static void UpdateEsophageSpawning()
        {
            PolaritiesSystem.esophageSpawnTimer++;
            if (PolaritiesSystem.esophageSpawnTimer > 60 * 10)
            {
                foreach (Player player in Main.player)
                {
                    if (Main.netMode != 1 && player.active && !player.dead)
                    {
                        Esophage.SpawnOn(player);

                        PolaritiesSystem.esophageSpawnTimer = -10;

                        break;
                    }
                }
            }
        }

        public static void CheckInvasionProgress()
        {
            //idk
            if (Main.invasionProgressMode != 2)
            {
                Main.invasionProgressNearInvasion = false;
                return;
            }

            Player player = Main.LocalPlayer;
            bool flag = player.InModBiome(GetInstance<WorldEvilInvasion>());

            Main.invasionProgressNearInvasion = flag;
            int progressMax = 1;

            if (PolaritiesSystem.worldEvilInvasion)
            {
                progressMax = PolaritiesSystem.worldEvilInvasionSizeStart;
            }

            int icon = 0;

            //show the progress bar if in the world evil
            if (flag && PolaritiesSystem.worldEvilInvasion)
            {
                Main.ReportInvasionProgress(PolaritiesSystem.worldEvilInvasionSizeStart - PolaritiesSystem.worldEvilInvasionSize, progressMax, icon, 0);
            }

            //Syncing start of invasion
            foreach (Player p in Main.player)
            {
                NetMessage.SendData(MessageID.InvasionProgressReport, p.whoAmI, -1, null, PolaritiesSystem.worldEvilInvasionSizeStart - PolaritiesSystem.worldEvilInvasionSize, PolaritiesSystem.worldEvilInvasionSizeStart, Main.invasionType, 0f, 0, 0, 0);
            }
        }
    }

    public class EsophageSummonItemDropCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            return PolaritiesSystem.worldEvilInvasion && PolaritiesSystem.worldEvilInvasionSize <= 0;
        }

        public bool CanShowItemDropInUI()
        {
            return true;
        }

        public string GetConditionDescription()
        {
            return Language.GetTextValue("Mods.Polarities.DropConditions.EsophageSummonItem");
        }
    }

    //TODO: Needs to take into account variable screen sizes better
    //TODO: Needs to take into account pausing better
    public class WorldEvilInvasionSky : CustomSky, ILoadable
    {
        private bool isActive;
        private float fadeOpacity;
        private WorldEvilInvasionParticle[] particles;
        private WorldEvilInvasionPlume[] plumes;

        private struct WorldEvilInvasionPlume
        {
            public Vector2 Position;
            public float Depth;
            public float Bias; //0 is fully corrupt, 1 is fully crimson
        }

        private struct WorldEvilInvasionParticle
        {
            public Vector2 Position;
            public float Rotation;
            public Vector2 Velocity;
            public Color Color;
            public float Depth;
            public int LifeTime;
            public bool Activated;
        }

        public static Asset<Texture2D> BGDust;

        public void Load(Mod mod)
        {
            Filters.Scene["Polarities:WorldEvilInvasion"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(1f, 1f, 1f).UseOpacity(0f), EffectPriority.VeryLow);
            SkyManager.Instance["Polarities:WorldEvilInvasion"] = new WorldEvilInvasionSky();

            BGDust = Request<Texture2D>("Polarities/Biomes/WorldEvilInvasionSky_BGDust");
        }

        public void Unload()
        {
            BGDust = null;
        }

        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
            fadeOpacity = 0.002f;
            plumes = new WorldEvilInvasionPlume[16];
            for (int i = 0; i < plumes.Length; i++)
            {
                InitializePlume(i);
            }
            particles = new WorldEvilInvasionParticle[1024];
        }

        private void InitializePlume(int i)
        {
            plumes[i].Depth = Main.rand.NextFloat() * 7f + 1f;
            plumes[i].Position = Main.LocalPlayer.Center + new Vector2(Main.rand.NextFloat(-1f, 1f) * Main.screenWidth * plumes[i].Depth, Main.screenHeight * plumes[i].Depth);
            plumes[i].Bias = Main.rand.NextFloat(1 - PolaritiesSystem.worldEvilInvasionSize / (float)PolaritiesSystem.worldEvilInvasionSizeStart);

            if (Main.LocalPlayer.ZoneCrimson) plumes[i].Bias = 1 - plumes[i].Bias;
        }

        private void InitializeParticle(int i)
        {
            int plume = Main.rand.Next(plumes.Length);

            particles[i].Activated = true;
            particles[i].Depth = plumes[plume].Depth;
            particles[i].Position = plumes[plume].Position;
            particles[i].Velocity = new Vector2(0, -10);
            particles[i].LifeTime = (int)(16 * 60 * particles[i].Depth);
            particles[i].Color = Main.rand.NextFloat() >= plumes[plume].Bias ? Color.Purple : Color.Red;
        }

        public override void Deactivate(params object[] args)
        {
            isActive = false;
        }

        public override bool IsActive()
        {
            if (!isActive)
            {
                return fadeOpacity > 0.001f;
            }
            return true;
        }

        public override void Reset()
        {
            isActive = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (isActive)
            {
                fadeOpacity = Math.Min(1f, 0.01f + fadeOpacity);
            }
            else
            {
                fadeOpacity = Math.Max(0f, fadeOpacity - 0.01f);
            }

            for (int i = 0; i < plumes.Length; i++)
            {
                if (Main.rand.NextBool(16 * 60))
                {
                    InitializePlume(i);
                }
            }
            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i].Activated)
                {
                    particles[i].Velocity += new Vector2(Main.rand.NextFloat(1f), 0).RotatedByRandom(MathHelper.TwoPi);
                    Vector2 goalVelocity = new Vector2(Main.windSpeedCurrent * 3, -3);
                    particles[i].Velocity += (goalVelocity - particles[i].Velocity) / 20f;

                    particles[i].Position += particles[i].Velocity;
                    particles[i].Rotation += particles[i].Velocity.X * 0.1f;

                    particles[i].LifeTime--;
                    if (particles[i].LifeTime <= 0)
                    {
                        InitializeParticle(i);
                    }
                }
                else
                {
                    if (Main.rand.NextBool(16 * 60))
                    {
                        InitializeParticle(i);
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i].Activated)
                {
                    if (particles[i].Depth >= minDepth && particles[i].Depth < maxDepth)
                    {
                        float particleScaleMult = 2 * (10f - particles[i].Depth) / 12f;

                        Color drawColor = particles[i].Color.MultiplyRGB(Main.ColorOfTheSkies) * fadeOpacity;
                        if (particles[i].LifeTime < 60)
                        {
                            drawColor *= particles[i].LifeTime / 60f;
                        }

                        Vector2 position = (particles[i].Position - Main.screenPosition - Main.ScreenSize.ToVector2() / 2) / particles[i].Depth + Main.ScreenSize.ToVector2() / 2;
                        position.X = (float)Math.IEEERemainder(position.X - Main.screenWidth / 2, Main.screenWidth * 2) + Main.screenWidth / 2;

                        if (position.Y <= -Main.screenHeight / 2)
                        {
                            InitializeParticle(i);
                        }
                        else
                        {
                            Rectangle frame = BGDust.Frame(1, 3, 0, i % 3);

                            spriteBatch.Draw(BGDust.Value, position, frame, drawColor, particles[i].Rotation, frame.Size() / 2, particleScaleMult, SpriteEffects.None, 0f);
                        }
                    }
                }
            }
        }
    }
}