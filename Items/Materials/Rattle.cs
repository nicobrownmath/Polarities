using Polarities.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria.Audio;

namespace Polarities.Items.Materials
{
	public class Rattle : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(5);
		}

		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 28;
			Item.maxStack = 999;
			Item.value = 100;
			Item.rare = ItemRarityID.White;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.consumable = false;
		}

		public override bool? UseItem(Player player)
		{
			SoundEngine.PlaySound(Sounds.Rattle, player.position);
			return true;
		}
	}
}