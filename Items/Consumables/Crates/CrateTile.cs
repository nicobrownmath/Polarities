using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Consumables.Crates
{
    public class CrateTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileSolidTop[Type] = true;
            Main.tileTable[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(160, 120, 92), CreateMapEntryName());
            AnimationFrameHeight = 56;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            int style = frameX / 36;
            int itemType = CrateBase.crateIndexToItemType[style];
            if (itemType != 0)
            {
                Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, itemType);
            }
        }
    }

    public abstract class CrateBase : ModItem
    {
        public abstract int CrateIndex { get; }
        public virtual bool HardmodeCrate => false;

        public static Dictionary<int, int> crateIndexToItemType = new Dictionary<int, int>();

        public override void Unload()
        {
            crateIndexToItemType = null;
        }

        public override void SetStaticDefaults()
        {
            crateIndexToItemType.Add(CrateIndex, Type);

            Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}");

            SacrificeTotal = (5);

            ItemID.Sets.IsFishingCrate[Type] = true;
            ItemID.Sets.IsFishingCrate[Type] = HardmodeCrate;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(TileType<CrateTile>(), CrateIndex);

            Item.width = 34;
            Item.height = 34;
            Item.maxStack = 99;
            Item.value = 10000;
            Item.rare = ItemRarityID.Green;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.Crates;
        }

        public override bool CanRightClick()
        {
            return true;
        }
    }
}

