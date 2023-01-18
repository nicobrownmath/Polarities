using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Placeable.Walls
{
	public class LimestoneBrickWall : ModItem
	{
        public override void SetStaticDefaults()
		{
			SacrificeTotal = (400);
		}

        public override void SetDefaults()
		{
			Item.DefaultToPlacableWall((ushort)WallType<LimestoneBrickWallPlaced>());
		}

		public override void AddRecipes()
		{
			CreateRecipe(4)
				.AddIngredient(ItemType<Blocks.LimestoneBrick>())
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}

	public class LimestoneBrickWallPlaced : ModWall
	{
		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = true;
			ItemDrop = ItemType<LimestoneWall>();
			AddMapEntry(new Color(34, 50, 30));

			DustType = DustType<Dusts.LimestoneDust>();
		}

		public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = fail ? 1 : 3;
		}
	}
}