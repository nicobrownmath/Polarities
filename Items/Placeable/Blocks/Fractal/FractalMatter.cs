using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Placeable.Blocks.Fractal
{
    public class FractalMatter : ModItem
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
            Item.createTile = ModContent.TileType<FractalMatterTile>();
            Item.rare = ItemRarityID.White;
            Item.width = 16;
            Item.height = 16;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<FractalMatterWall>(4)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    public class FractalMatterWall : ModItem
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
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createWall = ModContent.WallType<FractalMatterWallPlaced>();
        }

        public override void AddRecipes()
        {
            CreateRecipe(4)
                .AddIngredient<FractalMatter>()
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    [LegacyName("FractalMatter")]
    public class FractalMatterTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileStone[Type] = true;

            AddMapEntry(new Color(44, 124, 154));

            DustType = 116;
            ItemDrop = ModContent.ItemType<FractalMatter>();
            HitSound = SoundID.Tink;

            MineResist = 3f;
            MinPick = 100;
        }

        public override bool CanExplode(int i, int j)
        {
            return true;
        }
    }

    [LegacyName("FractalMatterWall")]
    public class FractalMatterWallPlaced : ModWall
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;
            ItemDrop = ModContent.ItemType<FractalMatterWall>();
            AddMapEntry(new Color(31, 68, 84));
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }

    public class FractalMatterWallNatural : ModWall
    {
        public override string Texture => ModContent.GetInstance<FractalMatterWallPlaced>().Texture;

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