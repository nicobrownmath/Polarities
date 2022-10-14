using Microsoft.Xna.Framework;
using Polarities.Dusts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Placeable.Blocks
{
	public class MantellarOre : ModItem
	{
		public override void SetStaticDefaults()
		{
			ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;

			this.SetResearch(100);
		}

		public override void SetDefaults()
		{
			Item.useStyle = 1;
			Item.useTurn = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.autoReuse = true;
			Item.maxStack = 999;
			Item.consumable = true;
			Item.createTile = TileType<MantellarOreTile>();
			Item.rare = ItemRarityID.Yellow;
			Item.width = 16;
			Item.height = 16;
			Item.value = 2500;
		}
	}

	public class MantellarOreTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			TileID.Sets.Ore[Type] = true;
			Main.tileSpelunker[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileOreFinderPriority[Type] = 710;
			Main.tileShine2[Type] = true;
			Main.tileShine[Type] = 975;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;

			ModTranslation name = CreateMapEntryName();
			name.SetDefault("{$Mods.Polarities.ItemName.MantellarOre}");
			AddMapEntry(new Color(255, 200, 0), name);

			DustType = DustType<MantellarDust>();
			ItemDrop = ItemType<MantellarOre>();

			HitSound = SoundID.Tink;

			MineResist = 8f;
			MinPick = 200;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.5f;
			g = 0.4f;
			b = 0.3f;
		}
		public override bool CanExplode(int i, int j)
		{
			return false;
		}
	}
}