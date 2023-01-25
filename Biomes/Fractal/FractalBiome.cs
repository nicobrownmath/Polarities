using SubworldLibrary;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace Polarities.Biomes.Fractal
{
    public class FractalBiome : ModBiome
    {
        public override bool IsBiomeActive(Player player)
        {
            return SubworldSystem.IsActive<FractalSubworld>();
        }

        public override float GetWeight(Player player)
        {
            return 0.6f;
        }

        public override string MapBackground => "Polarities/Biomes/Fractal/FractalMapBackground";
        public override string BackgroundPath => MapBackground;
        public override string BestiaryIcon => "Polarities/Biomes/Fractal/FractalBestiaryIcon";

        public override void Load()
        {
            if (!Main.dedServ)
            {
                SkyManager.Instance["Polarities:FractalDimension"] = new FractalSubworldSky();
                Filters.Scene["Polarities:FractalDimension"] = new Filter(new FractalScreenShaderData("FilterFractal"), EffectPriority.VeryLow);
                On.Terraria.Main.DrawBlack += Main_DrawBlack;
            }
        }

        private static void Main_DrawBlack(On.Terraria.Main.orig_DrawBlack orig, Main self, bool force)
        {
            if (SubworldSystem.IsActive<FractalSubworld>())
            {
                force = true;
            }
            orig(self, force);
        }

        public override void SpecialVisuals(Player player, bool isActive)
        {
            player.ManageSpecialBiomeVisuals("Polarities:FractalDimension", isActive);
        }
    }
}