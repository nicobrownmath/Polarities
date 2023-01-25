using Microsoft.Xna.Framework;
using Polarities.Dusts;
using Polarities.Items.Placeable.Blocks.Fractal;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Placeable.Furniture.Fractal
{
    public class FractalLamp : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.TikiTorch);
            Item.createTile = ModContent.TileType<FractalLampTile>();
            Item.placeStyle = 1;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<FractalBrick>(), 3)
                .AddIngredient(ItemID.Torch)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class FractalLampTile : LampTileBase
    {
        public override int MyDustType => ModContent.DustType<FractalMatterDust>();
        public override int DropItem => ModContent.ItemType<FractalLamp>();
        public override Color LightColor => new Color(0.75f, 0.75f, 1f);
    }
}