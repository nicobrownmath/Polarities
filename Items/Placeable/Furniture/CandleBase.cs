using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Placeable.Furniture
{
	public abstract class CandleBase : ModItem
	{
		public abstract int PlaceTile { get; }

		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(PlaceTile, 1);
		}
	}

	public abstract class CandleTileBase : ModTile
	{
		public abstract int MyDustType { get; }
		public abstract int DropItem { get; }
		public virtual bool DieInWater => true;
		public virtual bool DieInLava => true;
		public virtual Color MapColor => new Color(253, 221, 3);
		public virtual Color LightColor => Color.White;

		public override void SetStaticDefaults()
		{
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileWaterDeath[Type] = DieInWater;
			Main.tileLavaDeath[Type] = DieInLava;
			TileID.Sets.DisableSmartCursor[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			if (DieInWater)
				TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
			if (DieInLava)
				TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
			TileObjectData.newTile.StyleHorizontal = true;

			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidWithTop | AnchorType.Table, TileObjectData.newTile.Width, 0);

			TileObjectData.addTile(Type);
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);

			DustType = MyDustType;
			ItemDrop = DropItem;

			AddMapEntry(MapColor, Lang.GetItemName(ItemID.Candle));
		}

		public override void HitWire(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			int topX = i - tile.TileFrameX / 18 % 1;
			int topY = j - tile.TileFrameY / 18 % 1;
			short frameAdjustment = (short)(tile.TileFrameX >= 18 ? -18 : 18);
			Main.tile[topX, topY].TileFrameX += frameAdjustment;
			Wiring.SkipWire(topX, topY);
			NetMessage.SendTileSquare(-1, i, topY + 1, 3, TileChangeType.None);
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameX >= 18)
			{
				r = LightColor.R / 255f;
				g = LightColor.G / 255f;
				b = LightColor.B / 255f;
			}
		}
	}
}