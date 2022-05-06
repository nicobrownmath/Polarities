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
	public abstract class CandelabraBase : ModItem
	{
		public abstract int PlaceTile { get; }

		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(PlaceTile, 1);
			Item.value = Item.sellPrice(silver: 3);
		}
	}

	public abstract class CandelabraTileBase : ModTile
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

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			if (DieInWater)
				TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
			if (DieInLava)
				TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
			TileObjectData.newTile.StyleHorizontal = true;

			TileObjectData.addTile(Type);
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);

			DustType = MyDustType;

			AddMapEntry(MapColor, Lang.GetItemName(ItemID.Candelabra));
		}

		public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = fail ? 1 : 3;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, DropItem);
		}

		public override void HitWire(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			int topX = i - tile.TileFrameX / 18 % 2;
			int topY = j - tile.TileFrameY / 18 % 2;
			short frameAdjustment = (short)(tile.TileFrameX >= 36 ? -36 : 36);
			Main.tile[topX, topY].TileFrameX += frameAdjustment;
			Main.tile[topX, topY + 1].TileFrameX += frameAdjustment;
			Main.tile[topX + 1, topY].TileFrameX += frameAdjustment;
			Main.tile[topX + 1, topY + 1].TileFrameX += frameAdjustment;
			Wiring.SkipWire(topX, topY);
			Wiring.SkipWire(topX, topY + 1);
			Wiring.SkipWire(topX + 1, topY);
			Wiring.SkipWire(topX + 1, topY + 1);
			NetMessage.SendTileSquare(-1, i, topY + 1, 3, TileChangeType.None);
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameX >= 36)
			{
				r = LightColor.R / 255f;
				g = LightColor.G / 255f;
				b = LightColor.B / 255f;
			}
		}
	}
}