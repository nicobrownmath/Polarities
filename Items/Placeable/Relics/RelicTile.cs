using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Polarities.Items.Placeable.Relics
{
	public class RelicTile : ModTile
	{
		public const int FrameWidth = 18 * 3;
		public const int FrameHeight = 18 * 4;

		public Asset<Texture2D> RelicTexture;
		public virtual string RelicTextureName => "Polarities/Items/Placeable/Relics/RelicTileTopper";

		public override void Load()
		{
			if (!Main.dedServ)
			{
				// Cache the extra texture displayed on the pedestal
				RelicTexture = ModContent.Request<Texture2D>(RelicTextureName);
			}
		}

		public override void Unload()
		{
			RelicTexture = null;
		}

		public override void SetStaticDefaults()
		{
			Main.tileShine[Type] = 400;
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.InteractibleByNPCs[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
			TileObjectData.newTile.StyleHorizontal = false;
			TileObjectData.newTile.StyleWrapLimitVisualOverride = 2;
			TileObjectData.newTile.StyleMultiplier = 2;
			TileObjectData.newTile.StyleWrapLimit = 2;
			TileObjectData.newTile.styleLineSkipVisualOverride = 0;
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile); // Copy everything from above, saves us some code
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight; // Player faces to the right
			TileObjectData.addAlternate(1);
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(233, 207, 94), Language.GetText("MapObject.Relic"));
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			int style = frameX / FrameWidth;
			int itemType = RelicBase.relicIndexToItemType[style];
			if (itemType != 0)
			{
				Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 48, 64, itemType);
			}
		}

		public override bool CreateDust(int i, int j, ref int type)
		{
			return false;
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
		{
			tileFrameX %= FrameWidth;
			tileFrameY %= FrameHeight * 2;
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
		{
			//registers top left of tile for specialPoint
			if (drawData.tileFrameX % FrameWidth == 0 && drawData.tileFrameY % FrameHeight == 0)
			{
				Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
			}
		}

		//TODO: Relics are currently fuzzy, change this to use the same system as vanilla master mode relics (I believe nalyddd sent a message with a way to do this at some point)
		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			// This is lighting-mode specific, always include this if you draw tiles manually
			Vector2 offScreen = new Vector2(Main.offScreenRange);
			if (Main.drawToScreen)
			{
				offScreen = Vector2.Zero;
			}

			Point p = new Point(i, j);
			Tile tile = Main.tile[p.X, p.Y];
			if (tile == null || !tile.HasTile)
			{
				return;
			}

			// Get the initial draw parameters
			Texture2D texture = RelicTexture.Value;

			int frameX = tile.TileFrameX / FrameWidth;
			Rectangle frame = texture.Frame(texture.Width / texture.Height, 1, frameX, 0);

			Vector2 origin = frame.Size() / 2f;
			Vector2 worldPos = p.ToWorldCoordinates(24f, 64f);

			Color color = Lighting.GetColor(p.X, p.Y);

			bool direction = tile.TileFrameY / FrameHeight != 0;
			SpriteEffects effects = direction ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			// Some math magic to make it smoothly move up and down over time
			const float TwoPi = (float)Math.PI * 2f;
			float offset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * TwoPi / 5f);
			Vector2 drawPos = worldPos + offScreen - Main.screenPosition + new Vector2(0f, -40f) + new Vector2(0f, offset * 4f);

			// Draw the main texture
			spriteBatch.Draw(texture, drawPos, frame, color, 0f, origin, 1f, effects, 0f);

			// Draw the periodic glow effect
			float scale = (float)Math.Sin(Main.GlobalTimeWrappedHourly * TwoPi / 2f) * 0.3f + 0.7f;
			Color effectColor = color;
			effectColor.A = 0;
			effectColor = effectColor * 0.1f * scale;
			for (float num5 = 0f; num5 < 1f; num5 += 355f / (678f * (float)Math.PI))
			{
				spriteBatch.Draw(texture, drawPos + (TwoPi * num5).ToRotationVector2() * (6f + offset * 2f), frame, effectColor, 0f, origin, 1f, effects, 0f);
			}
		}
	}

	public abstract class RelicBase : ModItem
	{
		public abstract int RelicIndex { get; }

		public static Dictionary<int, int> relicIndexToItemType = new Dictionary<int, int>();

		public override void Unload()
		{
			relicIndexToItemType = null;
		}

		public override void SetStaticDefaults()
		{
			relicIndexToItemType.Add(RelicIndex, Type);

			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<RelicTile>(), RelicIndex);

			Item.width = 30;
			Item.height = 40;
			Item.maxStack = 99;
			Item.rare = ItemRarityID.Master;
			Item.master = true;
			Item.value = Item.buyPrice(gold: 5);
		}
	}

	public class StarConstructRelic : RelicBase { public override int RelicIndex => 0; }
	public class GigabatRelic : RelicBase { public override int RelicIndex => 1; }
	public class SunPixieRelic : RelicBase { public override int RelicIndex => 2; }
	public class EsophageRelic : RelicBase { public override int RelicIndex => 3; }
	public class ConvectiveWandererRelic : RelicBase { public override int RelicIndex => 4; }
    public class StormCloudfishRelic : RelicBase { public override int RelicIndex => 5; }
}
