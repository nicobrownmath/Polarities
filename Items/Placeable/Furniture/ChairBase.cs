using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Placeable.Furniture
{
	public abstract class ChairBase : ModItem
	{
		public abstract int PlaceTile { get; }

		public override void SetStaticDefaults()
		{
			SacrificeTotal = (1);
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(PlaceTile);
		}
	}

	public abstract class ChairTileBase : ModTile
	{
		public abstract int MyDustType { get; }
		public abstract int DropItem { get; }
		public virtual bool DieInLava => true;
		public virtual Color MapColor => new Color(191, 142, 111);

		public static bool[] IsChairTileBase;
		static bool StaticLoaded = false;

        public override void Load()
        {
            if (!StaticLoaded)
            {
				StaticLoaded = true;

                On.Terraria.GameContent.PlayerSittingHelper.GetSittingTargetInfo += PlayerSittingHelper_GetSittingTargetInfo;
			}
        }

        public override void Unload()
        {
			IsChairTileBase = null;
		}

        private bool PlayerSittingHelper_GetSittingTargetInfo(On.Terraria.GameContent.PlayerSittingHelper.orig_GetSittingTargetInfo orig, Player player, int x, int y, out int targetDirection, out Vector2 playerSittingPosition, out Vector2 seatDownOffset)
        {
			Tile tileSafely = Framing.GetTileSafely(x, y);
			if (IsChairTileBase[tileSafely.TileType])
			{
				if (!TileID.Sets.CanBeSatOnForPlayers[tileSafely.TileType] || !tileSafely.HasTile)
				{
					targetDirection = 1;
					seatDownOffset = Vector2.Zero;
					playerSittingPosition = default(Vector2);
					return false;
				}
				int num = x;
				int num2 = y;
				targetDirection = 1;
				seatDownOffset = Vector2.Zero;
				int num3 = 6;
				Vector2 zero = Vector2.Zero;

				seatDownOffset.Y = 0;//(tileSafely.TileFrameY / 40 == 27).ToInt() * 4; this is only used for the dynasty chair so ignore
				if (tileSafely.TileFrameY % 40 != 0)
				{
					num2--;
				}
				targetDirection = -1;
				if (tileSafely.TileFrameX != 0)
				{
					targetDirection = 1;
				}

				playerSittingPosition = Utils.ToWorldCoordinates(new Point(num, num2 + 1), 8f, 16f);
				playerSittingPosition.X += targetDirection * num3;
				playerSittingPosition += zero;
				return true;
			}
			else if (SofaTileBase.IsSofaTileBase[tileSafely.TileType])
            {
				if (!TileID.Sets.CanBeSatOnForPlayers[tileSafely.TileType] || !tileSafely.HasTile)
				{
					targetDirection = 1;
					seatDownOffset = Vector2.Zero;
					playerSittingPosition = default(Vector2);
					return false;
				}
				int num = x;
				int num2 = y;
				targetDirection = 1;
				seatDownOffset = Vector2.Zero;
				int num3 = 6;
				Vector2 zero = Vector2.Zero;

				targetDirection = player.direction;
				num3 = 0;
				Vector2 vector = new Vector2(-4f, 2f);
				Vector2 vector2 = new Vector2(4f, 2f);
				Vector2 vector3 = new Vector2(0f, 2f);
				Vector2 zero2 = Vector2.Zero;
				zero2.X = 1f;
				zero.X = -1f;

				(TileLoader.GetTile(tileSafely.TileType) as SofaTileBase).ModifySittingPosition(ref vector, ref vector2, ref vector3);

				if (tileSafely.TileFrameY % 40 != 0)
				{
					num2--;
				}
				if ((tileSafely.TileFrameX % 54 == 0 && targetDirection == -1) || (tileSafely.TileFrameX % 54 == 36 && targetDirection == 1))
				{
					seatDownOffset = vector;
				}
				else if ((tileSafely.TileFrameX % 54 == 0 && targetDirection == 1) || (tileSafely.TileFrameX % 54 == 36 && targetDirection == -1))
				{
					seatDownOffset = vector2;
				}
				else
				{
					seatDownOffset = vector3;
				}
				seatDownOffset += zero2;

				playerSittingPosition = Utils.ToWorldCoordinates(new Point(num, num2 + 1), 8f, 16f);
				playerSittingPosition.X += targetDirection * num3;
				playerSittingPosition += zero;
				return true;
			}
			else
			{
				return orig(player, x, y, out targetDirection, out playerSittingPosition, out seatDownOffset);
			}
        }

		protected virtual void AddMapEntry()
        {
			AddMapEntry(MapColor, Language.GetText("MapObject.Chair"));
		}

		public override void SetStaticDefaults()
		{
			if (IsChairTileBase == null)
            {
				IsChairTileBase = new bool[Main.tileFrameImportant.Length];
			}
			IsChairTileBase[Type] = true;

			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.CanBeSatOnForPlayers[Type] = true;
			TileID.Sets.CanBeSatOnForNPCs[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
			TileObjectData.newTile.StyleWrapLimit = 2;
			TileObjectData.newTile.StyleMultiplier = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
			TileObjectData.addAlternate(1);
			TileObjectData.addTile(Type);

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);

			AddMapEntry();

			DustType = MyDustType;

			AdjTiles = new int[] { TileID.Chairs };
		}

		public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = fail ? 1 : 3;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 32, DropItem);
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
		{
			return true;
		}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;
			if (player.IsWithinSnappngRangeToTile(i, j, 40))
			{
				player.noThrow = 2;
				player.cursorItemIconEnabled = true;
				player.cursorItemIconID = DropItem;
				player.cursorItemIconReversed = Main.tile[i, j].TileFrameX == 0;
			}
		}

        public override bool RightClick(int i, int j)
		{
			Player player = Main.LocalPlayer;
			if (player.IsWithinSnappngRangeToTile(i, j, 40))
            {
				player.GamepadEnableGrappleCooldown();
				player.sitting.SitDown(player, i, j);
				return true;
			}
			return false;
        }
    }
}