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
	public class Tentacle : ModItem
	{
        public override void SetStaticDefaults()
        {
			this.SetResearch(25);
        }

        public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 32;
			Item.maxStack = 999;
			Item.value = 1000;
			Item.rare = ItemRarityID.LightRed;
		}
	}
}