using Terraria;
using Terraria.ModLoader;

namespace Polarities.Biomes.Fractal
{
    public class FractalUGBiome : ModBiome
    {
        public override bool IsBiomeActive(Player player)
        {
            return false;
        }

        public override float GetWeight(Player player)
        {
            return 0.7f;
        }

        public override string MapBackground => "Polarities/Biomes/Fractal/FractalMapBackground";
        public override string BackgroundPath => MapBackground;
        public override string BestiaryIcon => "Polarities/Biomes/Fractal/FractalBestiaryIcon";
    }
}