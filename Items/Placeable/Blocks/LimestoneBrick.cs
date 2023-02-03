using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Placeable.Blocks
{
	public class LimestoneBrick : ModItem
	{
		public override void SetStaticDefaults()
		{
			ItemID.Sets.SortingPriorityMaterials[Item.type] = ItemID.Sets.SortingPriorityMaterials[ItemID.MarbleBlock];

			SacrificeTotal = (100);
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(TileType<LimestoneBrickTile>());
		}

		public override void AddRecipes()
		{
			CreateRecipe(2)
				.AddIngredient(ItemType<Limestone>())
				.AddIngredient(ItemID.StoneBlock)
				.AddTile(TileID.Furnaces)
				.Register();

			CreateRecipe()
				.AddIngredient(ItemType<Furniture.Limestone.LimestonePlatform>(), 2)
				.Register();

			CreateRecipe()
				.AddIngredient(ItemType<Walls.LimestoneBrickWall>(), 4)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}

	public class LimestoneBrickTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileBlendAll[Type] = true;
			TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
			AddMapEntry(new Color(61, 76, 61));

			DustType = DustType<Dusts.LimestoneDust>();
			ItemDrop = ItemType<LimestoneBrick>();

			HitSound = SoundID.Tink;

			MinPick = 0;
		}
	}
}