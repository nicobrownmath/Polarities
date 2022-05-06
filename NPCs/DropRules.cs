using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using static Terraria.ModLoader.ModContent;
using Terraria.GameInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using Polarities.Items;
using Polarities.NPCs;
using MonoMod.Cil;
using Terraria.ModLoader.IO;
using Terraria.Enums;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;
using static Terraria.GameContent.ItemDropRules.Chains;

namespace Polarities.NPCs
{
    public class FlawlessDropCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            return info.npc.GetGlobalNPC<PolaritiesNPC>().flawless;
        }

        public bool CanShowItemDropInUI()
        {
            return true;
        }

        public string GetConditionDescription()
        {
            return Language.GetTextValue("Mods.Polarities.DropConditions.Flawless");
        }
    }

    public class FlawlessOrRandomDropRule : CommonDrop
    {
        IItemDropRuleCondition condition;

        public FlawlessOrRandomDropRule(int itemId, int chanceDenominator, int amountDroppedMinimum = 1, int amountDroppedMaximum = 1, int chanceNumerator = 1, IItemDropRuleCondition condition = null) : base(itemId, chanceDenominator, amountDroppedMinimum, amountDroppedMaximum, chanceNumerator)
        {
            this.condition = condition;
        }

        public override bool CanDrop(DropAttemptInfo info)
        {
            if (condition == null) return true;
            return condition.CanDrop(info);
        }

        public override ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
        {
            ItemDropAttemptResult result;
            if (info.npc.GetGlobalNPC<PolaritiesNPC>().flawless || info.player.RollLuck(chanceDenominator) < chanceNumerator)
            {
                CommonCode.DropItemFromNPC(info.npc, itemId, info.rng.Next(amountDroppedMinimum, amountDroppedMaximum + 1));
                result = default(ItemDropAttemptResult);
                result.State = ItemDropAttemptResultState.Success;
                return result;
            }
            result = default(ItemDropAttemptResult);
            result.State = ItemDropAttemptResultState.FailedRandomRoll;
            return result;
        }
    }

    public class OneFromOptionsWithCountsNotScaledWithLuckDropRule : IItemDropRule
    {
        public List<IItemDropRuleChainAttempt> ChainedRules { get; }

        int chanceDenominator;
        int chanceNumerator;
        (int itemID, int minAmount, int maxAmount)[] itemData;

        public OneFromOptionsWithCountsNotScaledWithLuckDropRule(int chanceDenominator, int chanceNumerator = 1, params (int itemID, int minAmount, int maxAmount)[] itemData)
        {
            this.chanceDenominator = chanceDenominator;
            this.chanceNumerator = chanceNumerator;
            this.itemData = itemData;

            ChainedRules = new List<IItemDropRuleChainAttempt>();
        }

        public bool CanDrop(DropAttemptInfo info)
        {
            return true;
        }

        public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
        {
            float num = (float)chanceNumerator / (float)chanceDenominator;
            float num2 = num * ratesInfo.parentDroprateChance;
            float dropRate = 1f / (float)itemData.Length * num2;
            for (int i = 0; i < itemData.Length; i++)
            {
                drops.Add(new DropRateInfo(itemData[i].itemID, itemData[i].minAmount, itemData[i].maxAmount, dropRate, ratesInfo.conditions));
            }
            Chains.ReportDroprates(ChainedRules, num, drops, ratesInfo);
        }

        public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
        {
            ItemDropAttemptResult result;
            if (info.rng.Next(chanceDenominator) < chanceNumerator)
            {
                int i = info.rng.Next(itemData.Length);
                CommonCode.DropItemFromNPC(info.npc, itemData[i].itemID, Main.rand.Next(itemData[i].minAmount, itemData[i].maxAmount + 1));
                result = default(ItemDropAttemptResult);
                result.State = ItemDropAttemptResultState.Success;
                return result;
            }
            result = default(ItemDropAttemptResult);
            result.State = ItemDropAttemptResultState.FailedRandomRoll;
            return result;
        }
    }

    public class PerSegmentDropCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
        {
            return info.npc.GetGlobalNPC<MultiHitboxNPC>().useMultipleHitboxes;
        }

        public bool CanShowItemDropInUI()
        {
            return true;
        }

        public string GetConditionDescription()
        {
            return Language.GetTextValue("Mods.Polarities.DropConditions.MultiHitbox");
        }
    }

    public class MultiHitboxDropPerSegment : IItemDropRule
    {
        public List<IItemDropRuleChainAttempt> ChainedRules { get; }

        int chanceDenominator;
        int chanceNumerator;
        int minAmount;
        int maxAmount;
        int itemId;

        public MultiHitboxDropPerSegment(int itemId, int chanceDenominator = 1, int chanceNumerator = 1, int minAmount = 1, int maxAmount = 1)
        {
            this.itemId = itemId;
            this.chanceDenominator = chanceDenominator;
            this.chanceNumerator = chanceNumerator;
            this.minAmount = minAmount;
            this.maxAmount = maxAmount;

            ChainedRules = new List<IItemDropRuleChainAttempt>();
        }

        public bool CanDrop(DropAttemptInfo info)
        {
            return info.npc.GetGlobalNPC<MultiHitboxNPC>().useMultipleHitboxes;
        }

        public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
        {
            DropRateInfoChainFeed ratesInfo2 = ratesInfo.With(1f);
            ratesInfo2.AddCondition(new PerSegmentDropCondition());

            float num = (float)chanceNumerator / (float)chanceDenominator;
            float dropRate = num * ratesInfo2.parentDroprateChance;
            drops.Add(new DropRateInfo(itemId, minAmount, maxAmount, dropRate, ratesInfo2.conditions));
            Chains.ReportDroprates(ChainedRules, num, drops, ratesInfo2);
        }

        public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
        {
            ItemDropAttemptResult result;

            bool success = false;
            foreach (Rectangle hitbox in info.npc.GetGlobalNPC<MultiHitboxNPC>().hitboxes)
            {
                if (info.rng.Next(chanceDenominator) < chanceNumerator)
                {
                    success = true;
                    if (itemId > 0 && itemId < ItemLoader.ItemCount)
                    {
                        int itemIndex = Item.NewItem(info.npc.GetSource_Loot(), hitbox.Center.X, hitbox.Center.Y, 0, 0, itemId, Main.rand.Next(minAmount, maxAmount + 1), noBroadcast: false, -1);
                        CommonCode.ModifyItemDropFromNPC(info.npc, itemIndex);
                    }
                }
            }

            if (success)
            {
                result = default(ItemDropAttemptResult);
                result.State = ItemDropAttemptResultState.Success;
                return result;
            }
            result = default(ItemDropAttemptResult);
            result.State = ItemDropAttemptResultState.FailedRandomRoll;
            return result;
        }
    }
}

