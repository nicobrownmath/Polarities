using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;
using System;
using Polarities.Buffs;
using MonoMod.Cil;
using System.IO;

namespace Polarities.Items.Accessories
{
	public class CosmicCable : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Cosmic Cable");
			Tooltip.SetDefault("Improves hook stats");
		}

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 26;
			Item.accessory = true;
			Item.value = 10000;
			Item.rare = ItemRarityID.Expert;
			Item.expert = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<PolaritiesPlayer>().hookSpeedMult *= 1.5f;
		}
	}
}