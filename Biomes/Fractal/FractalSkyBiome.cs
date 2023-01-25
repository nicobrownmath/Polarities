using Terraria;
using Terraria.ModLoader;

namespace Polarities.Biomes.Fractal
{
    public class FractalSkyBiome : ModBiome
    {
        public override bool IsBiomeActive(Player player)
        {
            return player.InModBiome<FractalBiome>() && player.Center.ToTileCoordinates().Y < FractalSubworld.skyHeight;
        }

        public override float GetWeight(Player player)
        {
            return 0.6f;
        }

        public override string MapBackground => "Polarities/Biomes/Fractal/FractalMapBackground";
        public override string BackgroundPath => MapBackground;
        public override string BestiaryIcon => "Polarities/Biomes/Fractal/FractalSkyBestiaryIcon";
    }
}