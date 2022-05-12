using System;
using System.Reflection;
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
    public class LavaOcean : ModBiome
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.None;

        public override int Music => 0;

        public override string BestiaryIcon => "Biomes/" + Name + "_BestiaryIcon";
        public override string BackgroundPath => base.BackgroundPath;
        public override Color? BackgroundColor => base.BackgroundColor;

        public override bool IsBiomeActive(Player player)
        {
            return player.ZoneUnderworldHeight && ((WorldGen.dungeonX < Main.maxTilesX / 2) ? player.Center.X / 16f : Main.maxTilesX - 1 - player.Center.X / 16f) < Main.maxTilesX * 0.23f;
        }

        public static bool SpawnValid(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.Player.ZoneUnderworldHeight && (WorldGen.dungeonX < Main.maxTilesX / 2 ? spawnInfo.SpawnTileX : Main.maxTilesX - 1 - spawnInfo.SpawnTileX) < Main.maxTilesX * 0.23f;
        }
    }
}