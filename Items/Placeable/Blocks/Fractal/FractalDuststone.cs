using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Placeable.Blocks.Fractal
{
    public class FractalDuststone : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 100;
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<FractalDuststoneTile>();
            Item.rare = ItemRarityID.White;
            Item.width = 20;
            Item.height = 18;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<FractalDuststoneWall>(4)
                .AddTile(TileID.WorkBenches)
                .Register();
            CreateRecipe()
                .AddIngredient<FractalDust>(2)
                .AddTile(TileID.Furnaces)
                .Register();
        }
    }

    public class FractalDuststoneWall : ModItem
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
            Item.createWall = ModContent.WallType<FractalDuststoneWallPlaced>();
        }

        public override void AddRecipes()
        {
            CreateRecipe(4)
                .AddIngredient<FractalDuststone>()
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    [LegacyName("FractalDuststone")]
    public class FractalDuststoneTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileStone[Type] = true;

            AddMapEntry(new Color(103, 92, 125));

            DustType = 37;
            ItemDrop = ModContent.ItemType<FractalDuststone>();
            HitSound = SoundID.Tink;

            MineResist = 3f;
            MinPick = 100;
        }

        public override bool CanExplode(int i, int j)
        {
            return true;
        }
    }

    [LegacyName("FractalDuststoneWall")]
    public class FractalDuststoneWallPlaced : ModWall
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;
            ItemDrop = ModContent.ItemType<FractalDuststoneWall>();
            AddMapEntry(new Color(31, 68, 84));
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }

    public class FractalDuststoneWallNatural : ModWall
    {
        public override string Texture => ModContent.GetInstance<FractalDuststoneWallPlaced>().Texture;

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