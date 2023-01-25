using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;
using System;
using Polarities.Buffs;

namespace Polarities.Items.Accessories
{
	public class BatSigil : ModItem
	{
		public override void SetStaticDefaults()
		{
			SacrificeTotal = (1);
		}

		public override void SetDefaults()
		{
			Item.width = 34;
			Item.height = 34;
			Item.accessory = true;
			Item.value = 10000;
			Item.rare = ItemRarityID.Green;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.maxMinions += (int)(4 - 4 * (float)player.statLife / (float)player.statLifeMax2);
		}
	}
}