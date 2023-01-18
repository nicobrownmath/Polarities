using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Placeable.Walls
{
	public class RockSaltWall : ModItem
	{
		public override void SetStaticDefaults()
		{
			SacrificeTotal = (400);
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlacableWall((ushort)WallType<RockSaltWallPlaced>());
		}

		public override void AddRecipes()
		{
			CreateRecipe(4)
				.AddIngredient(ItemType<Blocks.RockSalt>())
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}

	public class RockSaltWallPlaced : ModWall
	{
		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = true;
			ItemDrop = ItemType<RockSaltWall>();
			AddMapEntry(new Color(127, 100, 100));

			DustType = DustType<Dusts.SaltDust>();
		}

		public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = fail ? 1 : 3;
		}
	}

	public class RockSaltWallNatural : ModWall
	{
		public override string Texture => "Polarities/Items/Placeable/Walls/RockSaltWallPlaced";

		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = false;
			AddMapEntry(new Color(127, 100, 100));

			DustType = DustType<Dusts.SaltDust>();
		}

		public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = fail ? 1 : 3;
		}
	}
}