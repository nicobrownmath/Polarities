using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;

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
        private IItemDropRuleCondition condition;

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
                CommonCode.DropItem(info, itemId, info.rng.Next(amountDroppedMinimum, amountDroppedMaximum + 1));
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

        private int chanceDenominator;
        private int chanceNumerator;
        private (int itemID, int minAmount, int maxAmount)[] itemData;

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
            float num = chanceNumerator / (float)chanceDenominator;
            float num2 = num * ratesInfo.parentDroprateChance;
            float dropRate = 1f / itemData.Length * num2;
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
                CommonCode.DropItem(info, itemData[i].itemID, Main.rand.Next(itemData[i].minAmount, itemData[i].maxAmount + 1));
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

