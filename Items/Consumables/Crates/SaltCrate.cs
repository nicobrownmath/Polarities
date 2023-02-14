using Polarities.Items.Accessories;
using Polarities.Items.Placeable;
using Polarities.Items.Weapons.Melee;
using Polarities.Items.Weapons.Summon.Sentries;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Consumables.Crates
{
    public class SaltCrate : CrateBase
    {
        public override int CrateIndex => 0;

        public override void ModifyItemLoot(ItemLoot itemLoot)
        {
            // Drop pre-hm ores
            IItemDropRule[] saltGear = new IItemDropRule[] {
                ItemDropRule.Common(ItemType<HopperCrystal>(), 1),
                ItemDropRule.Common(ItemType<MolluscStaff>(), 1),
                ItemDropRule.Common(ItemType<SaltKnife>(), 1, 1, 3),
                ItemDropRule.Common(ItemType<TolerancePotion>(), 1, 5, 10),
            };
            itemLoot.Add(new OneFromRulesRule(1, saltGear));

            //salt and crystals
            itemLoot.Add(ItemDropRule.Common(ItemType<SaltCrystals>(), 2, 6, 10));
            itemLoot.Add(ItemDropRule.Common(ItemType<SaltCrystals>(), 2, 30, 40));

            // Drop coins
            itemLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 4, 5, 13));

            // Drop pre-hm ores
            IItemDropRule[] oreTypes = new IItemDropRule[] {
                ItemDropRule.Common(ItemID.CopperOre, 1, 30, 50),
                ItemDropRule.Common(ItemID.TinOre, 1, 30, 50),
                ItemDropRule.Common(ItemID.IronOre, 1, 30, 50),
                ItemDropRule.Common(ItemID.LeadOre, 1, 30, 50),
                ItemDropRule.Common(ItemID.SilverOre, 1, 30, 50),
                ItemDropRule.Common(ItemID.TungstenOre, 1, 30, 50),
                ItemDropRule.Common(ItemID.GoldOre, 1, 30, 50),
                ItemDropRule.Common(ItemID.PlatinumOre, 1, 30, 50),
            };
            itemLoot.Add(new OneFromRulesRule(7, oreTypes));

            // Drop pre-hm bars (except copper/tin)
            IItemDropRule[] oreBars = new IItemDropRule[] {
                ItemDropRule.Common(ItemID.IronBar, 1, 10, 21),
                ItemDropRule.Common(ItemID.LeadBar, 1, 10, 21),
                ItemDropRule.Common(ItemID.SilverBar, 1, 10, 21),
                ItemDropRule.Common(ItemID.TungstenBar, 1, 10, 21),
                ItemDropRule.Common(ItemID.GoldBar, 1, 10, 21),
                ItemDropRule.Common(ItemID.PlatinumBar, 1, 10, 21),
            };
            itemLoot.Add(new OneFromRulesRule(4, oreBars));

            // Drop an "exploration utility" potion
            IItemDropRule[] explorationPotions = new IItemDropRule[] {
                ItemDropRule.Common(ItemID.ObsidianSkinPotion, 1, 2, 5),
                ItemDropRule.Common(ItemID.SpelunkerPotion, 1, 2, 5),
                ItemDropRule.Common(ItemID.HunterPotion, 1, 2, 5),
                ItemDropRule.Common(ItemID.GravitationPotion, 1, 2, 5),
                ItemDropRule.Common(ItemID.MiningPotion, 1, 2, 5),
                ItemDropRule.Common(ItemID.HeartreachPotion, 1, 2, 5),
            };
            itemLoot.Add(new OneFromRulesRule(4, explorationPotions));

            // Drop (pre-hm) resource potion
            IItemDropRule[] resourcePotions = new IItemDropRule[] {
                ItemDropRule.Common(ItemID.HealingPotion, 1, 5, 18),
                ItemDropRule.Common(ItemID.ManaPotion, 1, 5, 18),
            };
            itemLoot.Add(new OneFromRulesRule(2, resourcePotions));

            // Drop (high-end) bait
            IItemDropRule[] highendBait = new IItemDropRule[] {
                ItemDropRule.Common(ItemID.JourneymanBait, 1, 2, 7),
                ItemDropRule.Common(ItemID.MasterBait, 1, 2, 7),
            };
            itemLoot.Add(new OneFromRulesRule(2, highendBait));
        }
    }
}

