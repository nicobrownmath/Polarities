using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Biomes
{
    //TODO: Background should remove all rocky hell BG elements, add flocks of lavafowl
    public class LavaOcean : ModBiome
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.None;

        public override int Music => -1;

        public override string BestiaryIcon => (GetType().Namespace + "." + Name).Replace('.', '/') + "_BestiaryIcon";
        public override string BackgroundPath => base.BackgroundPath;
        public override Color? BackgroundColor => base.BackgroundColor;

        public override bool IsBiomeActive(Player player)
        {
            return player.ZoneUnderworldHeight && (Main.drunkWorld ? (Math.Abs(player.Center.X / 16f - Main.maxTilesX / 2) < Main.maxTilesX * 0.23f) : (((Main.dungeonX < Main.maxTilesX / 2) ? player.Center.X / 16f : Main.maxTilesX - 1 - player.Center.X / 16f) < Main.maxTilesX * 0.23f));
        }

        public static bool SpawnValid(NPCSpawnInfo spawnInfo, bool requireLava = false)
        {
            return spawnInfo.Player.ZoneUnderworldHeight &&
                (Main.drunkWorld ? (Math.Abs(spawnInfo.SpawnTileX - Main.maxTilesX / 2) < Main.maxTilesX * 0.23f) : ((Main.dungeonX < Main.maxTilesX / 2 ? spawnInfo.SpawnTileX : Main.maxTilesX - 1 - spawnInfo.SpawnTileX) < Main.maxTilesX * 0.23f)) &&
                (!requireLava || (spawnInfo.SpawnTileY > Main.maxTilesY - 100 && Framing.GetTileSafely(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY - 1).LiquidAmount == 255 && Framing.GetTileSafely(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY - 1).LiquidType == LiquidID.Lava));
        }
    }
}