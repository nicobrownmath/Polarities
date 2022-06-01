using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Items.Placeable;
using System.Collections.Generic;
using System;

namespace Polarities.Items.Accessories
{
	public class CelestialAmulet : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 34;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 8, silver: 50);
			Item.rare = ItemRarityID.Yellow;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<PolaritiesPlayer>().stargelAmulet = true;
			player.skyStoneEffects = true;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.CelestialStone)
				.AddIngredient(ItemType<StargelAmulet>())
				.AddTile(TileID.TinkerersWorkbench)
				.Register();
		}
	}
}