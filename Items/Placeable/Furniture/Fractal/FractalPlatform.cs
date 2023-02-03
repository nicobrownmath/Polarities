using Microsoft.Xna.Framework;
using Polarities.Dusts;
using Polarities.Items.Placeable.Blocks.Fractal;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Placeable.Furniture.Fractal
{
    public class FractalPlatform : ModItem
    {

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 8;
            Item.maxStack = 9999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<FractalPlatformTile>();
        }

        public override void AddRecipes()
        {
            CreateRecipe(2)
                .AddIngredient<FractalBrick>()
                .Register();
        }
    }

    public class FractalPlatformTile : PlatformTileBase
    {
        public override Color MapColor => new Color(33, 88, 106);
        public override int MyDustType => ModContent.DustType<FractalMatterDust>();
        public override int DropItem => ModContent.ItemType<FractalPlatform>();
    }
}