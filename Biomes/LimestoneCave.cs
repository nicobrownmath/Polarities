﻿using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Polarities.Biomes
{
	public class LimestoneCave : ModBiome
	{
		public override SceneEffectPriority Priority => SceneEffectPriority.None;

		public override int Music => 0;

		public override string BestiaryIcon => "Biomes/" + Name + "_BestiaryIcon";
		public override string BackgroundPath => base.BackgroundPath;
		public override Color? BackgroundColor => base.BackgroundColor;

		public override bool IsBiomeActive(Player player)
		{
			return ModContent.GetInstance<PolaritiesSystem>().limestoneBlockCount >= 500;
		}
	}
}