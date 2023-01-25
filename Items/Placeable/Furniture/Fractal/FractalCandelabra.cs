using Microsoft.Xna.Framework;
using Polarities.Dusts;
using Polarities.Items.Placeable.Blocks.Fractal;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Placeable.Furniture.Fractal
{
    public class FractalCandelabra : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Candelabra);
            Item.createTile = ModContent.TileType<FractalCandelabraTile>();
            Item.placeStyle = 1;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<FractalBrick>(), 5)
                .AddIngredient(ItemID.Torch, 3)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class FractalCandelabraTile : CandelabraTileBase
    {
        public override int MyDustType => ModContent.DustType<FractalMatterDust>();
        public override int DropItem => ModContent.ItemType<FractalCandelabra>();
        public override Color LightColor => new Color(0.75f, 0.75f, 1f);
    }
}