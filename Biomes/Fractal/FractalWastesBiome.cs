using Terraria;
using Terraria.ModLoader;

namespace Polarities.Biomes.Fractal
{
    public class FractalWastesBiome : ModBiome
    {
        public static int TileCount;

        public override bool IsBiomeActive(Player player)
        {
            return TileCount > 150;
        }

        public override float GetWeight(Player player)
        {
            return 0.75f;
        }

        public override string MapBackground => "Polarities/Biomes/Fractal/FractalMapBackground";
        public override string BackgroundPath => MapBackground;
        public override string BestiaryIcon => "Polarities/Biomes/Fractal/FractalWastesBestiaryIcon";
    }
}