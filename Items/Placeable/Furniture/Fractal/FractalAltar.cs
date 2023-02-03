using Microsoft.Xna.Framework;
using Polarities.Items.Placeable.Blocks.Fractal;
using SubworldLibrary;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Polarities.Items.Placeable.Furniture.Fractal
{
    [LegacyName("FractalAltarItem")]
    public class FractalAltar : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<FractalAltarTile>());
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.buyPrice(silver: 10);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<FractalMatter>(20)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    [LegacyName("FractalAltar")]
    public class FractalAltarTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;
            Main.tileLighted[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Width = 4;
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;

            TileObjectData.newTile.Origin = new Point16(2, 3);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(33, 88, 106), CreateMapEntryName("FractalAltar"));
            AnimationFrameHeight = 72;

            MinPick = 110;
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            int uniqueAnimationFrame = Main.tileFrame[Type];

            frameYOffset = uniqueAnimationFrame * AnimationFrameHeight;
        }

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            // Spend 5 ticks on each of 4 frames, looping
            if (++frameCounter >= 5)
            {
                frameCounter = 0;
                frame = ++frame % 4;
            }
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 33 / 255f;
            g = 88 / 255f;
            b = 106 / 255f;
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
        {
            return true;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 48, ModContent.ItemType<FractalAltar>());
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<FractalAltar>();
        }

        public override bool RightClick(int i, int j)
        {
            if (FractalSubworld.Active)
            {
                FractalSubworld.DoExit();
            }
            else
            {
                FractalSubworld.DoEnter();
            }
            return true;
        }
    }
}