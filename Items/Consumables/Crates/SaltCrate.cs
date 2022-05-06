using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.ID;
using System.Collections.Generic;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Enums;
using Microsoft.Xna.Framework;
using Polarities.NPCs;
using Terraria.ModLoader.IO;
using Terraria.Localization;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Items.Placeable.Blocks;
using Polarities.Items.Placeable;
using Polarities.Items.Weapons.Melee;
using Polarities.Items.Weapons.Summon.Sentries;
using Polarities.Items.Accessories;

namespace Polarities.Items.Consumables.Crates
{
	public class SaltCrate : CrateBase
	{
        public override int CrateIndex => 0;

		public override void RightClick(Player player)
		{
			IEntitySource source = player.GetSource_OpenItem(Type);

			switch (Main.rand.Next(4))
			{
				case 0:
					player.QuickSpawnItem(source, ItemType<HopperCrystal>());
					break;
				case 1:
					player.QuickSpawnItem(source, ItemType<MolluscStaff>());
					break;
				case 2:
					player.QuickSpawnItem(source, ItemType<SaltKnife>(), Main.rand.Next(1, 4));
					break;
				case 3:
					player.QuickSpawnItem(source, ItemType<TolerancePotion>(), Main.rand.Next(5, 11));
					break;
			}
			if (Main.rand.NextBool())
			{
				player.QuickSpawnItem(source, ItemType<SaltCrystals>(), Main.rand.Next(6, 11));
			}
			if (Main.rand.NextBool())
			{
				player.QuickSpawnItem(source, ItemType<Salt>(), Main.rand.Next(30, 40));
			}

			//standard prehardmode biome crate loot
			if (Main.rand.NextBool(7))
			{
				int itemType = 0;
				switch (Main.rand.Next(8))
				{
					case 0:
						itemType = ItemID.CopperOre;
						break;
					case 1:
						itemType = ItemID.TinOre;
						break;
					case 2:
						itemType = ItemID.IronOre;
						break;
					case 3:
						itemType = ItemID.LeadOre;
						break;
					case 4:
						itemType = ItemID.SilverOre;
						break;
					case 5:
						itemType = ItemID.TungstenOre;
						break;
					case 6:
						itemType = ItemID.GoldOre;
						break;
					case 7:
						itemType = ItemID.PlatinumOre;
						break;
				}
				player.QuickSpawnItem(source, itemType, Main.rand.Next(30, 50));
			}
			if (Main.rand.NextBool(4))
			{
				int itemType = 0;
				switch (Main.rand.Next(6))
				{
					case 0:
						itemType = ItemID.IronBar;
						break;
					case 1:
						itemType = ItemID.LeadBar;
						break;
					case 2:
						itemType = ItemID.SilverBar;
						break;
					case 3:
						itemType = ItemID.TungstenBar;
						break;
					case 4:
						itemType = ItemID.GoldBar;
						break;
					case 5:
						itemType = ItemID.PlatinumBar;
						break;
				}
				player.QuickSpawnItem(source, itemType, Main.rand.Next(10, 21));
			}
			if (Main.rand.NextBool(4))
			{
				int itemType = 0;
				switch (Main.rand.Next(6))
				{
					case 0:
						itemType = ItemID.ObsidianSkinPotion;
						break;
					case 1:
						itemType = ItemID.SpelunkerPotion;
						break;
					case 2:
						itemType = ItemID.HunterPotion;
						break;
					case 3:
						itemType = ItemID.GravitationPotion;
						break;
					case 4:
						itemType = ItemID.MiningPotion;
						break;
					case 5:
						itemType = ItemID.HeartreachPotion;
						break;
				}
				player.QuickSpawnItem(source, itemType, Main.rand.Next(2, 5));
			}
			if (Main.rand.NextBool(2))
			{
				int itemType = 0;
				switch (Main.rand.Next(2))
				{
					case 0:
						itemType = ItemID.HealingPotion;
						break;
					case 1:
						itemType = ItemID.ManaPotion;
						break;
				}
				player.QuickSpawnItem(source, itemType, Main.rand.Next(5, 18));
			}
			if (Main.rand.NextBool(2))
			{
				int itemType = 0;
				switch (Main.rand.Next(2))
				{
					case 0:
						itemType = ItemID.JourneymanBait;
						break;
					case 1:
						itemType = ItemID.MasterBait;
						break;
				}
				player.QuickSpawnItem(source, itemType, Main.rand.Next(2, 7));
			}
			if (Main.rand.NextBool(4))
			{
				player.QuickSpawnItem(source, ItemID.GoldCoin, Main.rand.Next(5, 13));
			}
		}
	}
}

