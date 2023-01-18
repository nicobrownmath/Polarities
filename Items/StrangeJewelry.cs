using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using System;
using System.IO;
using Terraria.Localization;

namespace Polarities.Items
{
	public class StrangeJewelry : ModItem
	{
		public override void SetStaticDefaults()
		{
			SacrificeTotal = (1);
		}

		public override void SetDefaults()
		{
			Item.width = 42;
			Item.height = 26;
			Item.maxStack = 1;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.LightPurple;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useStyle = ItemUseStyleID.HoldUp;
		}

		public override bool? UseItem(Player player)
		{
			if (PolaritiesSystem.disabledHallowSpread)
			{
				PolaritiesSystem.disabledHallowSpread = false;

				Main.NewText(Language.GetTextValue("Mods.Polarities.StatusMessage.StartHallowSpread"), 175, 75, 255);
			}
			else
			{
				PolaritiesSystem.disabledHallowSpread = true;

				Main.NewText(Language.GetTextValue("Mods.Polarities.StatusMessage.StopHallowSpread"), 175, 75, 255);
			}
			return true;
		}
	}
}