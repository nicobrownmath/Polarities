using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Polarities.Items.Placeable.Furniture
{
    public abstract class WorkBenchBase : ModItem
    {
        public abstract int PlaceTile { get; }

        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(PlaceTile);
            Item.value = Item.sellPrice(copper: 30);
        }
    }

    public abstract class WorkBenchTileBase : ModTile
    {
        public abstract int MyDustType { get; }
        public abstract int DropItem { get; }
        public virtual bool DieInLava => true;
        public virtual Color MapColor => new Color(191, 142, 111);

        public override void SetStaticDefaults()
        {
            Main.tileSolidTop[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileTable[Type] = true;
            Main.tileLavaDeath[Type] = DieInLava;
            TileID.Sets.DisableSmartCursor[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.addTile(Type);
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);

            AddMapEntry(MapColor, Lang.GetItemName(ItemID.WorkBench));

            DustType = MyDustType;
            AdjTiles = new int[] { TileID.WorkBenches };
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 16, DropItem);
        }
    }
}