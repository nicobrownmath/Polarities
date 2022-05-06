using Polarities.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria.DataStructures;

namespace Polarities.Items.Materials
{
	public class LimestoneCarapace : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(25);
		}

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.maxStack = 999;
			Item.value = Item.sellPrice(silver: 15);
			Item.rare = ItemRarityID.Orange;
		}
	}
}