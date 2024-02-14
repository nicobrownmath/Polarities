using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Placeable.Blocks
{
	public class HaliteBrick : ModItem
	{
		public override void SetStaticDefaults()
		{
			ItemID.Sets.SortingPriorityMaterials[Item.type] = ItemID.Sets.SortingPriorityMaterials[ItemID.MarbleBlock];

			this.SetResearch(100);
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(TileType<HaliteBrickTile>());
		}

		public override void AddRecipes()
		{
			CreateRecipe(2)
				.AddIngredient(ItemType<RockSalt>())
				.AddIngredient(ItemType<SaltCrystals>())
				.AddTile(TileID.Furnaces)
				.Register();
		}
	}

	public class HaliteBrickTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileBlendAll[Type] = true;
			TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
			AddMapEntry(new Color(173, 151, 172));

			DustType = DustType<Dusts.SaltDust>();

			HitSound = SoundID.Tink;

			MinPick = 0;
		}
	}
}
