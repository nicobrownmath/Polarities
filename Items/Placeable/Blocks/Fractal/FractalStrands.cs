using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Placeable.Blocks.Fractal
{
    public class FractalStrands : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 100;
        }

        public override void SetDefaults()
        {
            Item.useStyle = 1;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<FractalStrandsTile>();
            Item.rare = ItemRarityID.White;
            Item.width = 20;
            Item.height = 18;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<FractalStrandsWall>(4)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    public class FractalStrandsWall : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 400;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 9999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 7;
            Item.useStyle = 1;
            Item.consumable = true;
            Item.createWall = ModContent.WallType<FractalStrandsWallPlaced>();
        }

        public override void AddRecipes()
        {
            CreateRecipe(4)
                .AddIngredient<FractalStrands>()
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    [LegacyName("FractalStrands")]
    public class FractalStrandsTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileStone[Type] = true;

            AddMapEntry(new Color(60, 161, 199));

            DustType = 116;
            ItemDrop = ModContent.ItemType<FractalStrands>();
            HitSound = SoundID.Dig;

            MineResist = 2f;
            MinPick = 100;
            //SetModTree(new FractalTree());
        }
        public override bool CanExplode(int i, int j)
        {
            return true;
        }

        //public override int SaplingGrowthType(ref int style)
        //{
        //    style = 0;
        //    return TileType<FractalSapling>();
        //}
    }

    [LegacyName("FractalStrandsWall")]
    public class FractalStrandsWallPlaced : ModWall
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;
            ItemDrop = ModContent.ItemType<FractalStrandsWall>();
            AddMapEntry(new Color(31, 68, 84));
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }

    public class FractalStrandsWallNatural : ModWall
    {
        public override string Texture => ModContent.GetInstance<FractalStrandsWallPlaced>().Texture;

        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = false;
            AddMapEntry(new Color(31, 68, 84));
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}