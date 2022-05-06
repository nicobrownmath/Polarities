using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;
using System;
using Polarities.Buffs;

namespace Polarities.Items.Accessories
{
	public class StarBadge : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 26;
			Item.accessory = true;
			Item.value = 5000;
			Item.rare = ItemRarityID.Green;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<PolaritiesPlayer>().manaStarMultiplier *= 2;
		}
	}
}

