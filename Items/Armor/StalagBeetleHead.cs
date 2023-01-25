using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Items;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Polarities.Items.Placeable.Bars;
using Terraria.Localization;
using System;

namespace Polarities.Items.Armor
{
	[AutoloadEquip(EquipType.Head)]
	public class StalagBeetleHead : ModItem
	{
		public override void SetStaticDefaults()
		{
			SacrificeTotal = (1);
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 24;
			Item.rare = 1;
			Item.defense = 10;
			Item.value = 2000;
		}

		public override void UpdateEquip(Player player)
		{
			player.AddBuff(BuffID.Darkness, 2);
		}
	}
}

