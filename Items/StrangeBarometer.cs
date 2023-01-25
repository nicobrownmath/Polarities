using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using System;
using System.IO;
using Terraria.Localization;

namespace Polarities.Items
{
	public class StrangeBarometer : ModItem
	{
		public override void SetStaticDefaults()
		{
			SacrificeTotal = (1);
		}

		public override void SetDefaults()
		{
			Item.width = 34;
			Item.height = 34;
			Item.maxStack = 1;
			Item.value = 10000;
			Item.rare = ItemRarityID.Green;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useStyle = ItemUseStyleID.HoldUp;
		}

        public override bool AltFunctionUse(Player player)
        {
			return true;
        }

        public override bool? UseItem(Player player)
		{
			if (player.altFunctionUse == 2)
			{
				if (Math.Abs(Main.windSpeedTarget) < 0.8f)
                {
					int direction = Main.MouseWorld.X > player.Center.X ? 1 : -1;
					Main.windSpeedTarget = 0.8f * direction;

					Main.NewText(Language.GetTextValue("Mods.Polarities.StatusMessage.StartWind"), 224, 224, 224);
				}
				else
                {
					Main.windSpeedTarget = 0f;

					Main.NewText(Language.GetTextValue("Mods.Polarities.StatusMessage.StopWind"), 192, 192, 192);
				}
			}
			else
			{
				if (Main.raining)
				{
					Main.StopRain();

					Main.NewText(Language.GetTextValue("Mods.Polarities.StatusMessage.StopRain"), 192, 192, 192);
				}
				else
				{
					Main.StartRain();

					Main.NewText(Language.GetTextValue("Mods.Polarities.StatusMessage.StartRain"), 96, 96, 96);
				}
			}
			return true;
		}
	}
}