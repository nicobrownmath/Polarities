using Polarities.Dusts;
using Polarities.Items.Placeable.Blocks.Fractal;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Placeable.Furniture.Fractal
{
    public class FractalPiano : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Piano);
            Item.createTile = ModContent.TileType<FractalPianoTile>();
            Item.placeStyle = 0;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<FractalBrick>(), 15)
                .AddIngredient(ItemID.Bone, 4)
                .AddIngredient(ItemID.Book)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class FractalPianoTile : PianoTileBase
    {
        public override int MyDustType => ModContent.DustType<FractalMatterDust>();
        public override int DropItem => ModContent.ItemType<FractalPiano>();
    }
}