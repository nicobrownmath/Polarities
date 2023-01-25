using Microsoft.Xna.Framework;
using Polarities.Dusts;
using Polarities.Items.Placeable.Blocks.Fractal;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Placeable.Furniture.Fractal
{
    public class FractalCandle : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Candle);
            Item.createTile = ModContent.TileType<FractalCandleTile>();
            Item.placeStyle = 1;
            Item.flame = false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<FractalBrick>(), 5)
                .AddIngredient(ItemID.Torch)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class FractalCandleTile : CandleTileBase
    {
        public override int MyDustType => ModContent.DustType<FractalMatterDust>();
        public override int DropItem => ModContent.ItemType<FractalCandle>();
        public override Color LightColor => new Color(0.75f, 0.75f, 1f);
    }
}
