using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using System;
using System.IO;
using Terraria.Localization;

namespace Polarities.Items
{
	public class StrangeSamples : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 40;
			Item.maxStack = 1;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.LightPurple;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useStyle = ItemUseStyleID.HoldUp;
		}

		public override bool? UseItem(Player player)
		{
			if (PolaritiesSystem.disabledEvilSpread)
			{
				PolaritiesSystem.disabledEvilSpread = false;

				Main.NewText(Language.GetTextValue("Mods.Polarities.StatusMessage.StartEvilSpread"), 175, 75, 255);
			}
			else
			{
				PolaritiesSystem.disabledEvilSpread = true;

				Main.NewText(Language.GetTextValue("Mods.Polarities.StatusMessage.StopEvilSpread"), 175, 75, 255);
			}
			return true;
		}
	}
}