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
	public abstract class ChandelierBase : ModItem
	{
		public abstract int PlaceTile { get; }

		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(PlaceTile, 1);
			Item.value = Item.sellPrice(silver: 6);
		}
	}

	public abstract class ChandelierTileBase : ModTile
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

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
			if (DieInWater)
			{
				TileObjectData.newTile.WaterDeath = true;
				TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
			}
			if (DieInLava)
				TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
			TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 1, 1);
			TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
			TileObjectData.newTile.Origin = new Point16(1, 0);
			TileObjectData.newTile.StyleHorizontal = true;

			TileObjectData.addTile(Type);
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);

			AddMapEntry(MapColor, Language.GetText("MapObject.Chandelier"));

			DustType = MyDustType;
		}

		public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = fail ? 1 : 3;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 48, 48, DropItem);
		}

		public override void HitWire(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			int topX = i - tile.TileFrameX / 18 % 3;
			int topY = j - tile.TileFrameY / 18 % 3;
			short frameAdjustment = (short)(tile.TileFrameX >= 54 ? -54 : 54);
			Main.tile[topX, topY].TileFrameX += frameAdjustment;
			Main.tile[topX, topY + 1].TileFrameX += frameAdjustment;
			Main.tile[topX, topY + 2].TileFrameX += frameAdjustment;
			Main.tile[topX + 1, topY].TileFrameX += frameAdjustment;
			Main.tile[topX + 1, topY + 1].TileFrameX += frameAdjustment;
			Main.tile[topX + 1, topY + 2].TileFrameX += frameAdjustment;
			Main.tile[topX + 2, topY].TileFrameX += frameAdjustment;
			Main.tile[topX + 2, topY + 1].TileFrameX += frameAdjustment;
			Main.tile[topX + 2, topY + 2].TileFrameX += frameAdjustment;
			Wiring.SkipWire(topX, topY);
			Wiring.SkipWire(topX, topY + 1);
			Wiring.SkipWire(topX, topY + 2);
			Wiring.SkipWire(topX + 1, topY);
			Wiring.SkipWire(topX + 1, topY + 1);
			Wiring.SkipWire(topX + 1, topY + 2);
			Wiring.SkipWire(topX + 2, topY);
			Wiring.SkipWire(topX + 2, topY + 1);
			Wiring.SkipWire(topX + 2, topY + 2);
			NetMessage.SendTileSquare(-1, i, topY + 1, 3, TileChangeType.None);
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameX >= 54)
			{
				r = LightColor.R / 255f;
				g = LightColor.G / 255f;
				b = LightColor.B / 255f;
			}
		}
	}
}