using Polarities.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Polarities.NPCs.SunPixie;
using Terraria.GameContent;
using Terraria.Localization;
using Polarities.NPCs.Esophage;

namespace Polarities.Items
{
	//TODO: Inventory blood drip effect thing during the eclipse
	public class EsophageSummonItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			SacrificeTotal = (1);

			ItemID.Sets.SortingPriorityBossSpawns[Item.type] = 12;
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 32;
			Item.maxStack = 1;
			Item.rare = 1;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useStyle = 4;
			Item.consumable = false;
		}

		public override bool CanUseItem(Player player)
		{
			return !NPC.AnyNPCs(NPCType<Esophage>()) && PolaritiesSystem.esophageSpawnTimer == 0 /*&& !NPC.AnyNPCs(NPCType<Hemorrphage>())*/;
		}

		public override void UseAnimation(Player player)
		{
			base.UseAnimation(player);
		}

		public override bool? UseItem(Player player)
		{
			/*if (!(NPC.AnyNPCs(NPCType<NPCs.Hemorrphage.Hemorrphage>())) && Main.bloodMoon)
			{
				float r = (float)Main.rand.NextDouble();
				float theta = (float)(Math.PI * Main.rand.NextDouble());

				if (Main.netMode != 1)
				{
					int boss = NPC.NewNPC((int)(player.Center.X + (500 * r + 1000) * (float)Math.Cos(theta)), (int)(player.Center.Y - (500 * r + 1000) * (float)Math.Sin(theta)), NPCType<NPCs.Hemorrphage.Hemorrphage>());

					Main.npc[boss].netUpdate = true;

					Main.NewText("Hemorrphage has awoken!", 171, 64, 255);
				}
				Main.PlaySound(SoundID.NPCKilled, (int)player.position.X, (int)player.position.Y, 10, volumeScale: 1.2f, pitchOffset: -0.5f);
				Main.PlaySound(SoundID.Roar, (int)player.position.X, (int)player.position.Y, 0, volumeScale: 1.2f, pitchOffset: -0.5f);
				return true;
			}
			else*/
			{
				Esophage.SpawnOn(player);
			}
			return true;
		}

		public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
		{
			if (Main.bloodMoon)
			{
				TooltipLine line = new TooltipLine(Mod, "Tooltip1", Language.GetTextValue("Mods.Polarities.ItemTooltip.EsophageSummonItemExtra"));
				tooltips.Add(line);
			}
		}
	}
}