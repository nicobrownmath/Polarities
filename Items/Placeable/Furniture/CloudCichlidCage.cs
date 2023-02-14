using Microsoft.Xna.Framework;
using Polarities.NPCs.Critters;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Placeable.Furniture
{
    internal class CloudCichlidCageTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style6x3);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 18 };
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(122, 217, 232), CreateMapEntryName());
            AnimationFrameHeight = 56;
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            int uniqueAnimationFrame = Main.tileFrame[Type];
            //uniqueAnimationFrame = uniqueAnimationFrame % (69*3);

            frameYOffset = uniqueAnimationFrame * AnimationFrameHeight;
        }

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            // Spend 6 ticks on each of 69 frames, looping
            if (++frameCounter >= 6)
            {
                frameCounter = 0;
                frame = ++frame % 69;
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 32, ItemType<CloudCichlidCage>());
        }
    }

    internal class CloudCichlidCage : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.BunnyCage);
            Item.createTile = TileType<CloudCichlidCageTile>();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Terrarium)
                .AddIngredient(ItemType<CloudCichlidItem>())
                .Register();
        }
    }

    internal class StormcloudCichlidCageTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style6x3);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 18 };
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(122, 217, 232), CreateMapEntryName());
            AnimationFrameHeight = 56;
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            int uniqueAnimationFrame = Main.tileFrame[Type];
            //uniqueAnimationFrame = uniqueAnimationFrame % (69*3);

            frameYOffset = uniqueAnimationFrame * AnimationFrameHeight;
        }

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            // Spend 6 ticks on each of 69 frames, looping
            if (++frameCounter >= 6)
            {
                frameCounter = 0;
                frame = ++frame % 69;
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 32, ItemType<StormcloudCichlidCage>());
        }
    }

    internal class StormcloudCichlidCage : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.BunnyCage);
            Item.createTile = TileType<StormcloudCichlidCageTile>();
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Terrarium)
                .AddIngredient(ItemType<StormcloudCichlidItem>())
                .Register();
        }
    }
}
