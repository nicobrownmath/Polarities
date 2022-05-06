using Microsoft.Xna.Framework;
using Polarities.Items.Weapons.Ranged;
using Polarities.Items.Weapons.Ranged.Ammo;
using Polarities.Items.Weapons.Summon.Orbs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using static Terraria.ModLoader.ModContent;

namespace Polarities.NPCs
{
    public class VanillaNPCShop : GlobalNPC
    {
        public override void SetupShop(int type, Chest shop, ref int nextSlot)
        {
            switch (type)
			{
				case NPCID.WitchDoctor:
					/*TODO: if (NPC.downedPlantBoss && Main.player[Main.myPlayer].ZoneJungle)
					{
						shop.item[nextSlot].SetDefaults(ItemType<SymbioticSapling>());
						nextSlot++;
					}*/
					break;
				case NPCID.Painter:
					/*TODO: if (Main.player[Main.myPlayer].GetModPlayer<PolaritiesPlayer>().hasBeenInFractalSubworld)
					{
						shop.item[nextSlot].SetDefaults(ItemType<WarpedLandscape>());
						nextSlot++;
					}*/
					break;
				case NPCID.Demolitionist:
					if (Main.player[Main.myPlayer].HasItem(ItemType<Flarecaller>()))
					{
						shop.item[nextSlot].SetDefaults(ItemType<Flarecaller>());
						nextSlot++;
					}
					break;
				case NPCID.Dryad:
                    if (Main.player[Main.myPlayer].HasItem(ItemType<BatArrow>()))
                    {
                        shop.item[nextSlot].SetDefaults(ItemType<BatArrow>());
                        nextSlot++;
                    }
                    break;
                case NPCID.PartyGirl:
                    shop.item[nextSlot].SetDefaults(ItemType<Discorb>());
                    nextSlot++;
                    break;
            }
        }

		public override void GetChat(NPC npc, ref string chat)
		{
			if (!Main.rand.NextBool(10)) return;

			WeightedRandom<string> chatPool = new WeightedRandom<string>();

			switch (npc.type)
			{
				//TODO: LOCALIZE THIS (can use Language.GetTextValueWith)
				case NPCID.Guide:
					//TODO: Advice quotes?
					if (PolaritiesSystem.downedRiftDenizen)
					{
						chatPool.Add("That strange rift that showed up... Yeah I got nothing.");
					}
					break;
				case NPCID.Dryad:
					if (PolaritiesSystem.downedRiftDenizen)
					{
						chatPool.Add("That fractal stuff doesn't seem to be spreading, at least.");
					}
					if (PolaritiesSystem.worldEvilInvasion)
					{
						chatPool.Add("I sense something, a presence I've not felt since...");
						if (WorldGen.crimson)
							chatPool.Add("The crimson is particulary hostile today.");
						else
							chatPool.Add("The corruption is particulary eerie today.");
					}
					if (PolaritiesSystem.hallowInvasion)
					{
						chatPool.Add("The hallow is particularly luminous today.");
					}
					break;
				case NPCID.DD2Bartender:
					if (PolaritiesSystem.downedRiftDenizen)
					{
						chatPool.Add("Visitors from other worlds, huh? You don't say.");
					}
					break;
				case NPCID.Clothier:
					if (NPC.FindFirstNPC(NPCType<NPCs.TownNPCs.Ghostwriter>()) >= 0)
					{
						chatPool.Add("I can't help but feel like I recognize " + Main.npc[NPC.FindFirstNPC(NPCType<NPCs.TownNPCs.Ghostwriter>())].GivenName + " from somewhere.");
					}
					break;
				case NPCID.Mechanic:
					if (NPC.downedGolemBoss && !PolaritiesSystem.downedPolarities)
					{
						chatPool.Add("I've had a terribly irresponsible idea for a contraption!");
					}
					if (PolaritiesSystem.downedPolarities)
					{
						chatPool.Add("Those spinny magnet things do not obey the laws of physics at all.");
					}
					break;
				case NPCID.Angler:
					if (Main.anglerQuestFinished)
					{
						if (Main.raining)
						{
							chatPool.Add("You know, the little cloudfish outside are just small fry!");
						}
						chatPool.Add("I once hooked a massive cloudfish while fishing with a balloon, you know! Biggest one I ever saw. Carried me for miles.");
						if (Main.hardMode)
						{
							chatPool.Add("I heard there are these rare mushroom worms that live underground, and fish love them! You should use one as bait and see what bites!");
						}
						if (NPC.FindFirstNPC(NPCType<NPCs.TownNPCs.Ghostwriter>()) >= 0)
						{
							chatPool.Add("I tried to stick a fish in " + Main.npc[NPC.FindFirstNPC(NPCType<NPCs.TownNPCs.Ghostwriter>())].GivenName + "'s hair, but it just fell through. Disappointing.");
						}
					}
					else
					{
						if (Main.raining)
						{
							chatPool.Add("All those flying fish remind me of something... oh right, I need you to go get a fish for me!");
						}
					}
					break;
			}

			if (chatPool.elements.Count == 0) return;
			chat = chatPool;
		}
	}
}