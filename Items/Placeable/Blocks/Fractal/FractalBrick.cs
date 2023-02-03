using Microsoft.Xna.Framework;
using Polarities.Items.Placeable.Furniture.Fractal;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Placeable.Blocks.Fractal
{
    public class FractalBrick : ModItem
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
            Item.createTile = ModContent.TileType<FractalBrickTile>();
            Item.rare = ItemRarityID.White;
            Item.width = 16;
            Item.height = 16;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<FractalMatter>()
                .AddIngredient<FractalStrands>()
                .AddTile(TileID.Furnaces)
                .Register();
            CreateRecipe()
                .AddIngredient<FractalBrickWall>(4)
                .AddTile(TileID.WorkBenches)
                .Register();
            CreateRecipe()
                .AddIngredient<FractalPlatform>(2)
                .Register();
        }
    }

    public class FractalBrickWall : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fractal Brick Wall");
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 9999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 7;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createWall = ModContent.WallType<FractalBrickWallPlaced>();
        }

        public override void AddRecipes()
        {
            CreateRecipe(4)
                .AddIngredient<FractalBrick>()
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    [LegacyName("FractalBrick")]
    public class FractalBrickTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileBrick[Type] = true;

            AddMapEntry(new Color(73, 143, 170));

            DustType = 116;
            ItemDrop = ModContent.ItemType<FractalBrick>();
            HitSound = SoundID.Tink;
            MineResist = 3f;
            MinPick = 100;
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            int x = 0;
            int y = 0;
            for (int a = 0; a < 32; a++)
            {
                x = x ^ i >> a & 1;
                y = y ^ j >> a & 1;
            }

            frameYOffset = x * 90 + y * 180;
        }
        public override bool CanExplode(int i, int j)
        {
            return false;
        }
    }

    [LegacyName("CrackedFractalBrick")]
    public class FractalBrickTileCracked : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileBlendAll[Type] = true;

            AddMapEntry(new Color(73, 143, 170));

            DustType = 116;
            HitSound = SoundID.Tink;
            MineResist = 3f;
            MinPick = 65;
        }

        public override bool CanExplode(int i, int j)
        {
            return false;
        }
    }

    [LegacyName("FractaBrickWall")]
    public class FractalBrickWallPlaced : ModWall
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;
            AddMapEntry(new Color(43, 86, 102));
            ItemDrop = ModContent.ItemType<FractalBrickWall>();
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        /*public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
            Color drawColor = Lighting.GetColor(i,j);
            if (drawColor.R == 0 && drawColor.G == 0  && drawColor.B == 0) {
                return;
            }

            int x = 0;
            int y = 0;
            for(int a=0; a<8; a++) {
                x = x ^ ((i>>a) & 1);
                y = y ^ ((j>>a) & 1);
            }

			int yOffset = x * 16 + y * 32;

            Color newDrawColor = new Color(Math.Max(0,Math.Min((int)drawColor.R,255)),Math.Max(0,Math.Min((int)drawColor.G,255)),Math.Max(0,Math.Min((int)drawColor.B,255)));

            Texture2D texture = mod.GetTexture("Walls/FractalWallCenter");
            spriteBatch.Draw(texture,new Vector2(i*16 - Main.screenPosition.X + 200, j*16 - Main.screenPosition.Y + 200),
                new Rectangle(0, yOffset, 16, 16), newDrawColor, 0f, 
                new Vector2(16 * 0.5f, 16 * 0.5f), 1f, SpriteEffects.None, 0f
            );
        }*/

        public override bool CanExplode(int i, int j)
        {
            return true;
        }
    }
}