using Polarities.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace Polarities.Items.Materials
{
	public class VenomGland : ModItem
	{
		public override void SetStaticDefaults()
		{
			SacrificeTotal = (25);
		}

		public override void SetDefaults()
		{
			Item.width = 26;
			Item.height = 30;
			Item.maxStack = 9999;
			Item.value = Item.sellPrice(silver: 20);
			Item.rare = ItemRarityID.LightRed;
		}
	}
}