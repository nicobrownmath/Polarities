﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.NPCs;
using Polarities.NPCs.SunPixie;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Biomes
{
	public class HallowInvasion : ModBiome
	{
		public override SceneEffectPriority Priority => SceneEffectPriority.Event;

		public override int Music => MusicID.TheTowers;

		public override string BestiaryIcon => "Biomes/" + Name + "_BestiaryIcon";
        public override string BackgroundPath => "Terraria/Images\\MapBG8";
		public override Color? BackgroundColor => base.BackgroundColor;

        public static Asset<Texture2D> EventIcon;

        public override void Load()
        {
            EventIcon = Request<Texture2D>("Polarities/Biomes/" + Name + "_EventIcon");

            //allow the mod's map/bestiary bgs to use vanilla textures
            On.Terraria.GameContent.Bestiary.ModBiomeBestiaryInfoElement.GetBackgroundImage += ModBiomeBestiaryInfoElement_GetBackgroundImage;
        }

        private Asset<Texture2D> ModBiomeBestiaryInfoElement_GetBackgroundImage(On.Terraria.GameContent.Bestiary.ModBiomeBestiaryInfoElement.orig_GetBackgroundImage orig, Terraria.GameContent.Bestiary.ModBiomeBestiaryInfoElement self)
        {
            if (typeof(Terraria.GameContent.Bestiary.ModBestiaryInfoElement).GetField("_mod", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self) is Polarities)
            {
                string basePath = (string)(typeof(Terraria.GameContent.Bestiary.ModBestiaryInfoElement).GetField("_backgroundPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));

                if (basePath != null)
                {
                    if (basePath.StartsWith("Terraria/"))
                    {
                        if (basePath.Length == 0)
                        {
                            return null;
                        }
                        Asset<Texture2D> asset = Request<Texture2D>(basePath, AssetRequestMode.AsyncLoad);
                        if (asset.Size() == new Vector2(115f, 65f))
                        {
                            return asset;
                        }
                        Mod.Logger.Info((object)(basePath + " needs to be 115x65 pixels."));
                        return null;
                    }
                }
            }

            return orig(self);
        }

        public override void Unload()
        {
            EventIcon = null;
        }

        public override bool IsBiomeActive(Player player)
		{
			return player.ZoneHallow && player.ZoneOverworldHeight && PolaritiesSystem.hallowInvasion;
		}

        public override void SpecialVisuals(Player player)
        {
            player.ManageSpecialBiomeVisuals("Polarities:HallowInvasion", IsBiomeActive(player));
        }

        public override void OnLeave(Player player)
        {
            player.ManageSpecialBiomeVisuals("Polarities:HallowInvasion", false);
        }

        public static bool ValidNPC(int npcType)
        {
            return PolaritiesNPC.customNPCCapSlot.ContainsKey(npcType) && PolaritiesNPC.customNPCCapSlot[npcType] == NPCCapSlotID.HallowInvasion;
        }

        public static float GetSpawnChance(int tier)
        {
            float invasionLeft = PolaritiesSystem.hallowInvasionSize / (float)PolaritiesSystem.hallowInvasionSizeStart;

            const int maxTier = 6;
            return (float)(ModUtils.BinomialCoefficient(maxTier, tier) * Math.Pow(1 - invasionLeft, tier) * Math.Pow(invasionLeft, maxTier - tier));
        }

        public static void StartInvasion()
        {
            if (PolaritiesSystem.hallowInvasion) return;
            //Checks amount of players for scaling
            int numPlayers = 0;
            for (int i = 0; i < 255; i++)
            {
                if (Main.player[i].active)
                {
                    numPlayers++;
                }
            }

            PolaritiesSystem.hallowInvasion = true;
            PolaritiesSystem.hallowInvasionSize = 100 * numPlayers;
            PolaritiesSystem.hallowInvasionSizeStart = PolaritiesSystem.hallowInvasionSize;

            String text = Language.GetTextValue("Mods.Polarities.StatusMessage.SpawnHallowInvasion");
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
            if (PolaritiesSystem.hallowInvasion)
            {
                if (PolaritiesSystem.hallowInvasionSize <= 0)
                {
                    foreach (Player player in Main.player)
                    {
                        if (player.active && !player.dead)
                        {
                            PolaritiesSystem.hallowInvasion = false;

                            if (!PolaritiesSystem.downedHallowInvasion)
                            {
                                PolaritiesSystem.downedHallowInvasion = true;
                                if (Main.netMode == NetmodeID.Server)
                                {
                                    NetMessage.SendData(MessageID.WorldData);
                                }
                            }

                            PolaritiesSystem.sunPixieSpawnTimer++;
                            break;
                        }
                    }
                }
            }
        }

        public static void UpdateSunPixieSpawning()
        {
            PolaritiesSystem.sunPixieSpawnTimer++;
            if (PolaritiesSystem.sunPixieSpawnTimer > 60 * 10)
            {
                foreach (Player player in Main.player)
                {
                    if (Main.netMode != 1 && player.active && !player.dead)
                    {
                        SunPixie.SpawnOn(player);

                        PolaritiesSystem.sunPixieSpawnTimer = -10;

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
            bool flag = player.InModBiome(GetInstance<HallowInvasion>());

            Main.invasionProgressNearInvasion = flag;
            int progressMax = 1;

            if (PolaritiesSystem.hallowInvasion)
            {
                progressMax = PolaritiesSystem.hallowInvasionSizeStart;
            }

            int icon = 0;

            //show the progress bar if in the hallow
            if (flag && PolaritiesSystem.hallowInvasion)
            {
                Main.ReportInvasionProgress(PolaritiesSystem.hallowInvasionSizeStart - PolaritiesSystem.hallowInvasionSize, progressMax, icon, 0);
            }

            //Syncing start of invasion
            foreach (Player p in Main.player)
            {
                NetMessage.SendData(MessageID.InvasionProgressReport, p.whoAmI, -1, null, PolaritiesSystem.hallowInvasionSizeStart - PolaritiesSystem.hallowInvasionSize, (float)PolaritiesSystem.hallowInvasionSizeStart, (float)Main.invasionType, 0f, 0, 0, 0);
            }
        }
    }

    public class SunPixieSummonItemDropCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            return PolaritiesSystem.hallowInvasion && PolaritiesSystem.hallowInvasionSize <= 0;
        }

        public bool CanShowItemDropInUI()
        {
            return true;
        }

        public string GetConditionDescription()
        {
            return Language.GetTextValue("Mods.Polarities.DropConditions.SunPixieSummonItem");
        }
    }

    //TODO: Needs to take into account variable screen sizes better
    public class HallowInvasionSky : CustomSky, ILoadable
    {
        bool isActive;
        float fadeOpacity;
        HallowInvasionLightPillar[] pillars;

        private struct HallowInvasionLightPillar
        {
            public float XPosition;
            public float Depth;
            public float Phase;
            public float Angle;
            public bool Draw;
        }

        public void Load(Mod mod)
        {
            Filters.Scene["Polarities:HallowInvasion"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(1f, 1f, 1f).UseOpacity(0f), EffectPriority.VeryLow);
            SkyManager.Instance["Polarities:HallowInvasion"] = new HallowInvasionSky();
        }

        public void Unload() { }

        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
            fadeOpacity = 0.002f;
            pillars = new HallowInvasionLightPillar[30];
            for (int i = 0; i < pillars.Length; i++)
            {
                pillars[i].Depth = Main.rand.NextFloat() * 7f + 1f;
                pillars[i].Angle = Main.rand.NextFloat(-0.1f, 0.1f);
                pillars[i].Phase = MathHelper.TwoPi * i / pillars.Length;
            }
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
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            for (int i = 0; i < pillars.Length; i++)
            {
                if (pillars[i].Depth >= minDepth && pillars[i].Depth < maxDepth)
                {
                    float pillarPhaseScaleMult = (float)Math.Sin(pillars[i].Phase + Main.GlobalTimeWrappedHourly / 2f) * 2 - 1;
                    if (pillarPhaseScaleMult > 0)
                    {
                        if (!pillars[i].Draw)
                        {
                            pillars[i].Draw = true;
                            pillars[i].XPosition = Main.rand.NextFloat(0f, 1f);
                        }

                        float pillarDistScaleMult = (10f - pillars[i].Depth) / 12f;

                        //draw pillar
                        Color drawColor = Color.White * fadeOpacity;

                        float xPosition = (Main.screenWidth * 2 * pillars[i].Depth * pillars[i].XPosition - Main.screenPosition.X) / pillars[i].Depth;
                        xPosition = (float)Math.IEEERemainder(xPosition, Main.screenWidth * 2) + Main.screenWidth / 2;

                        Vector2 position = new Vector2(xPosition, 0);

                        Asset<Texture2D> pillarTexture = TextureAssets.Projectile[ProjectileType<NPCs.Enemies.HallowInvasion.SunServitorBeam>()];

                        spriteBatch.Draw(pillarTexture.Value, position, pillarTexture.Frame(), drawColor, pillars[i].Angle, new Vector2(pillarTexture.Size().X / 2, 0), new Vector2(pillarDistScaleMult * pillarPhaseScaleMult, Main.screenHeight / (float)Math.Cos(pillars[i].Angle)), SpriteEffects.None, 0f);
                    }
                    else
                    {
                        pillars[i].Draw = false;
                    }
                }
            }
        }
    }
}