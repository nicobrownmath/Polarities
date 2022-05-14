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
    //TODO: Background should remove all rocky hell BG elements, add flocks of lavafowl
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

        public static bool SpawnValid(NPCSpawnInfo spawnInfo, bool requireLava = false)
        {
            return spawnInfo.Player.ZoneUnderworldHeight &&
                (WorldGen.dungeonX < Main.maxTilesX / 2 ? spawnInfo.SpawnTileX : Main.maxTilesX - 1 - spawnInfo.SpawnTileX) < Main.maxTilesX * 0.23f &&
                (!requireLava || (spawnInfo.SpawnTileY > Main.maxTilesY - 100 && Framing.GetTileSafely(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY - 1).LiquidAmount == 255 && Framing.GetTileSafely(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY - 1).LiquidType == LiquidID.Lava));
        }
    }
}