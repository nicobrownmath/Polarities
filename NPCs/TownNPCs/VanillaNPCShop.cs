using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Polarities.Items.Accessories;
using Polarities.Items.Weapons.Ranged;
using Polarities.Items.Weapons.Ranged.Ammo;
using Polarities.Items.Weapons.Summon.Orbs;
using Polarities.NPCs.TownNPCs;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using static Terraria.ModLoader.ModContent;

namespace Polarities.NPCs
{
    public class VanillaNPCShop : GlobalNPC
    {
        public override void Load()
        {
            Terraria.IL_Main.HelpText += Main_HelpText;
        }

        public override void ModifyShop(NPCShop shop)
        {
            if (shop.Name != "Shop")
            {
                return;
            }

            switch (shop.NpcType) 
            {
                case NPCID.WitchDoctor:
                    {
                        shop.Add(ModContent.ItemType<SymbioticSapling>(), Condition.DownedPlantera, Condition.InJungle);
                    }
                    break;
                case NPCID.Demolitionist:
                    {
                        shop.Add(ModContent.ItemType<Flarecaller>(), Condition.PlayerCarriesItem(ModContent.ItemType<Flarecaller>()));
                    }
                    break;
                case NPCID.Dryad:
                    {
                        shop.Add(ModContent.ItemType<BatArrow>(), Condition.PlayerCarriesItem(ModContent.ItemType<BatArrow>()));
                    }
                    break;
                case NPCID.PartyBunny:
                    {
                        shop.Add(ModContent.ItemType<Discorb>());
                    }
                    break;
                /*case NPCID.Painter:
                    {
                        shop.Add(ModContent.ItemType<WarpedLandscape>(), ...);
                    }
                    break;*/
            }

        }

        public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
		{
			switch (npc.type)
			{
				case NPCID.Dryad:
					//alt evil seeds in graveyard only unlock after esophage
					if (!PolaritiesSystem.downedEsophage)
					{
						for (int i = 0; i < items.Length; i++)
						{
							if (items[i].type == ItemID.CorruptSeeds && WorldGen.crimson)
							{
                                items[i] = new Item();
                                items[i].SetDefaults(ItemID.CrimsonSeeds);
								break;
							}
							else if (items[i].type == ItemID.CrimsonSeeds && !WorldGen.crimson)
							{
                                items[i] = new Item();
                                items[i].SetDefaults(ItemID.CorruptSeeds);
								break;
							}
						}
					}
					break;
				case NPCID.Steampunker:
					if (PolaritiesSystem.downedEsophage && Main.LocalPlayer.ZoneGraveyard)
					{
						for (int i = 0; i < items.Length; i++)
						{
							if (items[i].ammo == AmmoID.Solution)
							{
                                items[i] = new Item();
                                items[i].SetDefaults(WorldGen.crimson ? ItemID.PurpleSolution : ItemID.RedSolution);
								break;
							}
						}
					}
					break;
            }
        }

		public override void GetChat(NPC npc, ref string chat)
		{
			if (!Main.rand.NextBool(10)) return;

            WeightedRandom<(string, object)> chatPool = new WeightedRandom<(string, object)>();

            string baseDialogueString = "Mods.Polarities.TownNPCDialogue." + npc.TypeName + ".";

            switch (npc.type)
			{
				case NPCID.Guide:
					if (PolaritiesSystem.downedRiftDenizen)
					{
						chatPool.Add((baseDialogueString + "RiftDenizen", null));
					}
					break;
				case NPCID.Dryad:
					if (PolaritiesSystem.downedRiftDenizen)
					{
						chatPool.Add((baseDialogueString + "RiftDenizen", null));
					}
					if (PolaritiesSystem.worldEvilInvasion)
					{
						chatPool.Add((baseDialogueString + "WorldEvilInvasion", null));
						if (WorldGen.crimson)
							chatPool.Add((baseDialogueString + "CrimsonInvasion", null));
						else
							chatPool.Add((baseDialogueString + "CorruptInvasion", null));
					}
					if (PolaritiesSystem.hallowInvasion)
					{
						chatPool.Add((baseDialogueString + "HallowInvasion", null));
					}
					break;
				case NPCID.DD2Bartender:
					if (PolaritiesSystem.downedRiftDenizen)
					{
						chatPool.Add((baseDialogueString + "RiftDenizen", null));
					}
					break;
				case NPCID.Clothier:
					if (NPC.FindFirstNPC(NPCType<NPCs.TownNPCs.Ghostwriter>()) >= 0)
					{
						chatPool.Add((baseDialogueString + "Ghostwriter", new { NPCName = Main.npc[NPC.FindFirstNPC(NPCType<NPCs.TownNPCs.Ghostwriter>())].GivenName }));
					}
					break;
				case NPCID.Mechanic:
					if (PolaritiesSystem.downedConvectiveWanderer && PolaritiesSystem.downedSelfsimilarSentinel && !PolaritiesSystem.downedPolarities)
					{
						chatPool.Add((baseDialogueString + "PrePolarities", null));
					}
					if (PolaritiesSystem.downedPolarities)
					{
						chatPool.Add((baseDialogueString + "Polarities", null));
					}
					break;
				case NPCID.Angler:
					if (Main.anglerQuestFinished)
					{
						if (Main.raining)
						{
							chatPool.Add((baseDialogueString + "Rain", null));
						}
						chatPool.Add((baseDialogueString + "Generic", null));
						if (Main.hardMode)
						{
							chatPool.Add((baseDialogueString + "Hardmode", null));
						}
						if (NPC.FindFirstNPC(NPCType<NPCs.TownNPCs.Ghostwriter>()) >= 0)
						{
							chatPool.Add((baseDialogueString + "Ghostwriter", new { NPCName = Main.npc[NPC.FindFirstNPC(NPCType<NPCs.TownNPCs.Ghostwriter>())].GivenName }));
						}
					}
					else
					{
						if (Main.raining)
						{
							chatPool.Add((baseDialogueString + "RainQuest", null));
						}
					}
					break;
			}

            if (chatPool.elements.Count == 0) return;
            (string, object) output = chatPool;
            chat = Language.GetTextValueWith(output.Item1, output.Item2);
        }

		static bool isThereAGhostwriter; //for guide help dialogue efficiency
		static bool inventoryCloud; //for guide help dialogue efficiency
		static bool inventoryStar; //for guide help dialogue efficiency
		static bool inventoryGem; //for guide help dialogue efficiency

		//guide help text because anyone ever uses this
		private void Main_HelpText(ILContext il)
		{
			ILCursor c = new ILCursor(il);

			c.EmitDelegate<Action>(() =>
			{
				isThereAGhostwriter = NPC.AnyNPCs(NPCType<Ghostwriter>());

				inventoryCloud = false;
				inventoryStar = false;
				inventoryGem = false;
				for (int i = 0; i < 58; i++)
				{
					if (Main.LocalPlayer.inventory[i].type == ItemID.Cloud)
                    {
						inventoryCloud = true;
					}
					if (Main.LocalPlayer.inventory[i].type == ItemID.FallenStar)
					{
						inventoryStar = true;
					}
					if (Main.LocalPlayer.inventory[i].type == ItemID.Amethyst || Main.LocalPlayer.inventory[i].type == ItemID.Topaz || Main.LocalPlayer.inventory[i].type == ItemID.Sapphire || Main.LocalPlayer.inventory[i].type == ItemID.Emerald || Main.LocalPlayer.inventory[i].type == ItemID.Ruby || Main.LocalPlayer.inventory[i].type == ItemID.Diamond)
					{
						inventoryGem = true;
					}
				}
			});

			ILLabel label = null;

			if (!c.TryGotoNext(MoveType.Before,
				i => i.MatchLdsfld(typeof(Main).GetField("helpText", BindingFlags.Public | BindingFlags.Static)),
				i => i.MatchLdcI4(1200),
				i => i.MatchBle(out label)
				))
			{
				GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
				return;
			}

			//we skip one ahead and pop helpText to ensure we actually are reached
			c.Index++;
			c.Emit(OpCodes.Pop);

			//this breaks from the help text searching loop if true, and continues searching if false
			c.EmitDelegate<Func<bool>>(() =>
			{
				switch (Main.helpText)
                {
					case 1200:
						if (!isThereAGhostwriter)
						{
							Main.npcChatText = Language.GetTextValue("Mods.Polarities.GuideHelpText.Ghostwriter");
							return true;
						}
						break;
					case 1201:
						if (inventoryCloud && !PolaritiesSystem.downedStormCloudfish)
						{
							Main.npcChatText = Language.GetTextValue("Mods.Polarities.GuideHelpText.StormCloudfishSummonItem");
							return true;
						}
						break;
					case 1202:
						if (inventoryStar && !PolaritiesSystem.downedStarConstruct)
						{
							Main.npcChatText = Language.GetTextValue("Mods.Polarities.GuideHelpText.StarConstructSummonItem");
							return true;
						}
						break;
					case 1203:
						if (inventoryGem && !PolaritiesSystem.downedGigabat)
						{
							Main.npcChatText = Language.GetTextValue("Mods.Polarities.GuideHelpText.GigabatSummonItem");
							return true;
						}
						break;
					case 1204:
						if (!Main.hardMode)
                        {
							Main.npcChatText = Language.GetTextValue("Mods.Polarities.GuideHelpText.SaltCave");
							return true;
						}
						break;
					case 1205:
						if (!Main.hardMode)
						{
							Main.npcChatText = Language.GetTextValue("Mods.Polarities.GuideHelpText.LimestoneCave");
							return true;
						}
						break;
					case 1206:
						if (Main.LocalPlayer.statLifeMax >= 200 && !PolaritiesSystem.downedStormCloudfish)
						{
							Main.npcChatText = Language.GetTextValue("Mods.Polarities.GuideHelpText.StormCloudfish");
							return true;
						}
						break;
					//EoC is required for these because they all are helped so much by SoC
					case 1207:
						if (Main.LocalPlayer.statLifeMax >= 300 && NPC.downedBoss1 && !PolaritiesSystem.downedStarConstruct)
						{
							Main.npcChatText = Language.GetTextValue("Mods.Polarities.GuideHelpText.StarConstruct");
							return true;
						}
						break;
					case 1208:
						if (Main.LocalPlayer.statLifeMax >= 400 && NPC.downedBoss1 && !PolaritiesSystem.downedGigabat)
						{
							Main.npcChatText = Language.GetTextValue("Mods.Polarities.GuideHelpText.Gigabat");
							return true;
						}
						break;
					case 1209:
						if (Main.LocalPlayer.statLifeMax >= 400 && NPC.downedBoss1 && !PolaritiesSystem.downedRiftDenizen)
						{
							Main.npcChatText = Language.GetTextValue("Mods.Polarities.GuideHelpText.RiftDenizen");
							return true;
						}
						break;
					case 1210:
						if (PolaritiesSystem.downedRiftDenizen)
						{
							Main.npcChatText = Language.GetTextValue("Mods.Polarities.GuideHelpText.FractalDimension");
							return true;
						}
						break;
					case 1211:
						if (Main.hardMode && NPC.downedMechBossAny && !PolaritiesSystem.downedHallowInvasion)
						{
							Main.npcChatText = Language.GetTextValue("Mods.Polarities.GuideHelpText.HallowInvasion");
							return true;
						}
						break;
					case 1212:
						if (PolaritiesSystem.hallowInvasion && !PolaritiesSystem.downedSunPixie)
						{
							Main.npcChatText = Language.GetTextValue("Mods.Polarities.GuideHelpText.SunPixie");
							return true;
						}
						break;
					case 1213:
						if (Main.hardMode && NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3 && !PolaritiesSystem.downedWorldEvilInvasion)
						{
							Main.npcChatText = Language.GetTextValue("Mods.Polarities.GuideHelpText.WorldEvilInvasion");
							return true;
						}
						break;
					case 1214:
						if (PolaritiesSystem.worldEvilInvasion && !PolaritiesSystem.downedEsophage)
						{
							Main.npcChatText = Language.GetTextValue("Mods.Polarities.GuideHelpText.Esophage");
							return true;
						}
						break;
					case 1215:
						if (Main.hardMode && !PolaritiesSystem.downedConvectiveWanderer)
						{
							Main.npcChatText = Language.GetTextValue("Mods.Polarities.GuideHelpText.ConvectiveWanderer");
							return true;
						}
						break;
					case 1216:
						if (PolaritiesSystem.downedSunPixie && !PolaritiesSystem.downedEclipxie)
						{
							Main.npcChatText = Language.GetTextValue("Mods.Polarities.GuideHelpText.Eclipxie");
							return true;
						}
						break;
					case 1217:
						if (PolaritiesSystem.downedEsophage && !PolaritiesSystem.downedHemorrphage)
						{
							Main.npcChatText = Language.GetTextValue("Mods.Polarities.GuideHelpText.Hemorrphage");
							return true;
						}
						break;
					case 1218:
						if (PolaritiesSystem.downedConvectiveWanderer && PolaritiesSystem.downedSelfsimilarSentinel && Main.LocalPlayer.statLifeMax >= 500 && (PolaritiesSystem.downedEclipxie || PolaritiesSystem.downedHemorrphage || NPC.downedEmpressOfLight || NPC.downedFishron || NPC.downedAncientCultist) && !PolaritiesSystem.downedPolarities)
						{
							Main.npcChatText = Language.GetTextValue("Mods.Polarities.GuideHelpText.Polarities");
							return true;
						}
						break;
				}

				if (Main.helpText > 1218)
					Main.helpText = 0;
				return false;
			});
			c.Emit(OpCodes.Brfalse, label);
			c.Emit(OpCodes.Ret);

			//we re-add our helpText to make things not break
			c.Emit(OpCodes.Ldsfld, typeof(Main).GetField("helpText", BindingFlags.Public | BindingFlags.Static));
		}
	}
}
