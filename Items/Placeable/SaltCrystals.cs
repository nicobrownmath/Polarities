using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Placeable
{
	public class SaltCrystals : ModItem
	{
		public override void SetStaticDefaults()
		{
			SacrificeTotal = (25);
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(TileType<SaltCrystalsTile>());

			Item.width = 16;
			Item.height = 18;

			Item.value = Item.sellPrice(silver: 1);
			Item.rare = ItemRarityID.White;
		}
	}

	public class SaltCrystalsTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileShine[Type] = 1100;
			Main.tileSolid[Type] = false;
			Main.tileSolidTop[Type] = false;
			Main.tileFrameImportant[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.WaterDeath = false;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.newTile.RandomStyleRange = 7;
			TileObjectData.addTile(Type);

			ItemDrop = ItemType<SaltCrystals>();

			HitSound = SoundID.Shatter;

			AddMapEntry(new Color(255, 240, 240), CreateMapEntryName());
		}
	}
}