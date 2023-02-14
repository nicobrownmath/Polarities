using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Placeable.Blocks
{
    public class GlowingLimestoneBrick : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityMaterials[Item.type] = ItemID.Sets.SortingPriorityMaterials[ItemID.MarbleBlock];

            SacrificeTotal = (100);
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(TileType<GlowingLimestoneBrickTile>());
        }

        public override void AddRecipes()
        {
            CreateRecipe(4)
                .AddIngredient(ItemType<LimestoneBrick>())
                .AddIngredient(ItemType<Materials.AlkalineFluid>())
                .AddTile(TileID.Furnaces)
                .Register();
        }
    }

    public class GlowingLimestoneBrickTile : ModTile
    {
        private Asset<Texture2D> GlowTexture;

        public override void Load()
        {
            GlowTexture = Request<Texture2D>(Texture + "_Mask");
        }

        public override void Unload()
        {
            GlowTexture = null;
        }

        public override void SetStaticDefaults()
        {
            //Polarities.customTileGlowMasks.Add(TileType<GlowingLimestoneBrickTile>(), Request<Texture2D>(Texture + "_Mask"));

            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileBlendAll[Type] = true;
            TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
            AddMapEntry(new Color(179, 191, 177));

            DustType = DustType<Dusts.LimestoneDust>();
            ItemDrop = ItemType<LimestoneBrick>();

            HitSound = SoundID.Tink;

            MinPick = 0;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.01f;
            g = 0.01f;
            b = 0.01f;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            if (tile.Slope == 0 && !tile.IsHalfBlock)
            {
                Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
                if (Main.drawToScreen)
                {
                    zero = Vector2.Zero;
                }
                Main.spriteBatch.Draw(GlowTexture.Value, new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero, new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
        }
    }
}