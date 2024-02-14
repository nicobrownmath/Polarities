﻿using Polarities.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Polarities.Buffs;
using Terraria.DataStructures;

namespace Polarities.Items.Consumables
{
	public class Pretzel : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(10);

			// Tooltip.SetDefault(Lang.GetTooltip(ItemID.GoldenDelight).GetLine(0));

			DrawAnimationVertical animation = new DrawAnimationVertical(1, 3);
			animation.NotActuallyAnimating = true;
			Main.RegisterItemAnimation(Type, animation);

			ItemID.Sets.FoodParticleColors[Type] = new Color[] { Color.Brown, Color.Brown, Color.White };
			ItemID.Sets.IsFood[Type] = true;
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 30;
			Item.maxStack = 999;
			Item.value = Item.sellPrice(silver: 40);
			Item.rare = ItemRarityID.Orange;
			Item.consumable = true;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.useTurn = true;
			Item.useStyle = ItemUseStyleID.EatFood;
			Item.UseSound = SoundID.Item2;
			Item.buffType = BuffID.WellFed3;
			Item.buffTime = 28800;
		}
	}
}