using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;
using System;
using Polarities.Buffs;

namespace Polarities.Items.Accessories
{
	public class SunlightEmblem : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Sunlight Emblem");
			Tooltip.SetDefault("15% increased magic damage\n10% reduced mana usage\n10% increased non-magic damage while not at full mana\nHitting enemies with non-magic weapons restores your mana");

			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 28;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetDamage(DamageClass.Magic) += 0.15f;
			if (player.statMana < player.statManaMax2)
			{
				player.GetModPlayer<PolaritiesPlayer>().nonMagicDamage += 0.1f;
			}
			player.GetModPlayer<PolaritiesPlayer>().solarEnergizer = true;
			player.manaCost -= 0.1f;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.SorcererEmblem)
				.AddIngredient(ItemType<SolarEnergizer>())
				.AddTile(TileID.TinkerersWorkbench)
				.Register();
		}
	}
}
