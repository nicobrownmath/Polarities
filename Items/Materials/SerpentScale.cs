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
	public class SerpentScale : ModItem
	{
        public override void SetStaticDefaults()
        {
			this.SetResearch(25);
        }

        public override void SetDefaults()
		{
			Item.width = 26;
			Item.height = 36;
			Item.maxStack = 999;
			Item.value = 100;
			Item.rare = ItemRarityID.LightRed;
		}
	}
}