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
}

