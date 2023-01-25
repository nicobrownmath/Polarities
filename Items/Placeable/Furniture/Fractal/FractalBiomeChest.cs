using Polarities.Dusts;
using Terraria;
using Terraria.ModLoader;

namespace Polarities.Items.Placeable.Furniture.Fractal
{
    [LegacyName("FractalBiomeChestItem")]
    public class FractalBiomeChest : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<FractalBiomeChestTile>());
            Item.value = Item.buyPrice(silver: 5);
        }
    }

    [LegacyName("FractalBiomeChest")]
    public class FractalBiomeChestTile : ChestTileBase
    {
        public override int MyDustType => ModContent.DustType<FractalMatterDust>();
        public override int DropItem => ModContent.ItemType<FractalBiomeChest>();
    }
}