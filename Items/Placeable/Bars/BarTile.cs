using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Polarities.Items;
using Terraria.DataStructures;
using System.Collections.Generic;
using Terraria.ID;
using Polarities.Dusts;

namespace Polarities.Items.Placeable.Bars
{
	public class BarTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileShine[Type] = 1100;
			Main.tileSolid[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileFrameImportant[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(224, 194, 101), Language.GetText("MapObject.MetalBar"));
		}

		public override bool Drop(int i, int j)
		{
			Tile t = Main.tile[i, j];
			int style = t.TileFrameX / 18;
			int itemType = BarBase.barIndexToItemType[style];
			if (itemType != 0)
			{
				Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, itemType);
			}
			return base.Drop(i, j);
		}

		public override bool CreateDust(int i, int j, ref int type)
		{
			Tile t = Main.tile[i, j];
			int style = t.TileFrameX / 18;
			type = BarBase.barIndexToDustIndex[style] ?? type;
			return true;
		}
	}

	public abstract class BarBase : ModItem
	{
		public abstract int BarIndex { get; }
		public virtual int? DustIndex => null;

		public static Dictionary<int, int> barIndexToItemType = new Dictionary<int, int>();
		public static Dictionary<int, int?> barIndexToDustIndex = new Dictionary<int, int?>();

		public override void Unload()
		{
			barIndexToItemType = null;
			barIndexToDustIndex = null;
		}

		public override void SetStaticDefaults()
		{
			barIndexToItemType.Add(BarIndex, Type);
			barIndexToDustIndex.Add(BarIndex, DustIndex);

			this.SetResearch(25);
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(TileType<BarTile>(), BarIndex);

			Item.width = 30;
			Item.height = 24;
			Item.maxStack = 99;
		}
	}

	public class SunplateBar : BarBase
    {
        public override int BarIndex => 2;
		//TODO: Dust

        public override void SetDefaults()
        {
            base.SetDefaults();

			Item.width = 28;
			Item.height = 20;

			Item.rare = ItemRarityID.Blue;
			Item.value = 4000;
        }
	}

	public class ConvectiveBar : BarBase
	{
		public override int BarIndex => 1;
		public override int? DustIndex => DustType<ConvectiveDust>();

		public override void SetDefaults()
		{
			base.SetDefaults();

			Item.width = 30;
			Item.height = 36;

			Item.rare = ItemRarityID.Yellow;
			Item.value = 10000;
		}
	}
}