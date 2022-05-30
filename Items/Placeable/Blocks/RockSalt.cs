using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using System;
using Terraria.DataStructures;

namespace Polarities.Items.Placeable.Blocks
{
	public class RockSalt : ModItem
	{
		public override void SetStaticDefaults()
		{
			ItemID.Sets.SortingPriorityMaterials[Item.type] = ItemID.Sets.SortingPriorityMaterials[ItemID.StoneBlock];

			this.SetResearch(100);
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(TileType<RockSaltTile>());
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemType<Salt>(), 2)
				.AddTile(TileID.WorkBenches)
				.Register();

			CreateRecipe()
				.AddIngredient(ItemType<Items.Placeable.Walls.RockSaltWall>(), 4)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}

	public class RockSaltTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileBlendAll[Type] = true;
			TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = false;

			AddMapEntry(new Color(255, 200, 200));

			DustType = DustType<Dusts.SaltDust>();
			ItemDrop = ItemType<RockSalt>();

			HitSound = SoundID.Dig;
		}

		public override void RandomUpdate(int i, int j)
		{
			if (j > Main.rockLayer && !Main.tile[i, j - 1].HasTile && Main.tile[i, j - 1].LiquidType == 0 && Main.tile[i, j - 1].LiquidAmount == 255 && Main.tile[i, j].Slope == 0 && !Main.tile[i, j].IsHalfBlock)
			{
				//count nearby salt crystals
				int crystalCount = 0;
				for (int k = -5; k < 6; k++)
				{
					for (int l = -5; l < 6; l++)
					{
						if (Main.tile[i + k, j + l].HasTile && Main.tile[i + k, j + l].TileType == TileType<SaltCrystalsTile>()) { crystalCount++; }
					}
				}
				//grow salt crystal
				if (crystalCount < 3)
				{
					WorldGen.PlaceTile(i, j - 1, TileType<SaltCrystalsTile>(), mute: true, style: Main.rand.Next(7));
				}
			}
		}
	}
}
