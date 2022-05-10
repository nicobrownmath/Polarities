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
	public class RotKnuckles : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;

			Item.defense = 8;

			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ItemRarityID.LightPurple;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetDamage(DamageClass.Generic) += 0.05f;
			player.GetCritChance(DamageClass.Generic) += 5;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.PutridScent)
				.AddIngredient(ItemID.FleshKnuckles)
				.AddIngredient(ItemType<EvilDNA>())
				.AddTile(TileID.DemonAltar)
				.Register();
		}
	}
}