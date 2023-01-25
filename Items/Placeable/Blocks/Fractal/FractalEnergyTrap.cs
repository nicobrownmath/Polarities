using Terraria.ModLoader;

namespace Polarities.Items.Placeable.Blocks.Fractal
{
    public class FractalEnergyTrap : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 5;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<FractalTrap>(), 0);
        }
    }
}