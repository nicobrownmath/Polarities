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
using Polarities.NPCs.ConvectiveWanderer;
using Polarities.Biomes;

namespace Polarities.Items
{
	//TODO: Make this obtainable
	//TODO: Sound like whatever the death sound of the baby wanderer is
	public class ConvectiveWandererSummonItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);

			ItemID.Sets.SortingPriorityBossSpawns[Item.type] = 12;
		}

		public override void SetDefaults()
		{
			Item.width = 38;
			Item.height = 34;
			Item.maxStack = 1;
			Item.rare = ItemRarityID.Blue;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.consumable = false;
		}

		public override bool CanUseItem(Player player)
		{
			return !NPC.AnyNPCs(NPCType<ConvectiveWanderer>()) && player.InModBiome(GetInstance<LavaOcean>());
		}

		public override bool? UseItem(Player player)
		{
			ConvectiveWanderer.SpawnOn(player);
			return true;
		}
	}
}