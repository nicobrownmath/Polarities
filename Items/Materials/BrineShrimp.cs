using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using System;
using Terraria.DataStructures;

namespace Polarities.Items.Materials
{
	public class BrineShrimp : ModItem
	{
		public override void SetStaticDefaults()
		{
			SacrificeTotal = (3);
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 30;
			Item.maxStack = 9999;
			Item.value = 500;
			Item.rare = ItemRarityID.White;
		}

        public override void AddRecipes()
        {
			Recipe.Create(ItemID.CookedShrimp)
				.AddIngredient(Type)
				.AddTile(TileID.CookingPots)
				.Register();
        }
    }
}

