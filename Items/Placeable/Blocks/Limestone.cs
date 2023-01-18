using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Placeable.Blocks
{
	public class Limestone : ModItem
	{
		public override void SetStaticDefaults()
		{
			ItemID.Sets.SortingPriorityMaterials[Item.type] = ItemID.Sets.SortingPriorityMaterials[ItemID.StoneBlock];

			SacrificeTotal = (100);
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(TileType<LimestoneTile>());
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemType<Walls.LimestoneWall>(), 4)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}

	public class LimestoneTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileBlendAll[Type] = true;
			TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = false;

			AddMapEntry(new Color(61, 76, 61));

			DustType = DustType<Dusts.LimestoneDust>();
			ItemDrop = ItemType<Limestone>();

			HitSound = SoundID.Tink;

			MinPick = 0;
		}
	}
}