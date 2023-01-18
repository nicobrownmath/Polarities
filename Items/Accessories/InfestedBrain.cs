using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Items.Placeable;
using System.Collections.Generic;
using System;
using Polarities.Items.Materials;

namespace Polarities.Items.Accessories
{
	public class InfestedBrain : ModItem
	{
		public override void SetStaticDefaults()
		{
			SacrificeTotal = (1);
		}

		public override void SetDefaults()
		{
			Item.width = 38;
			Item.height = 30;

			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ItemRarityID.Expert;
			Item.expert = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			//dodge change is 2/3 that of the brain of confusion
			if (!Main.rand.NextBool(3))
				player.brainOfConfusionItem = Item;

			if (!player.GetModPlayer<PolaritiesPlayer>().wormScarf)
			{
				player.endurance += 0.12f;
				player.GetModPlayer<PolaritiesPlayer>().wormScarf = true;
			}
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.WormScarf)
				.AddIngredient(ItemID.BrainOfConfusion)
				.AddIngredient(ItemType<EvilDNA>())
				.AddTile(TileID.DemonAltar)
				.Register();
		}
	}
}
