using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.NPCs;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.GameContent;
using Terraria.Localization;

namespace Polarities.Items
{
	public class PolaritiesItem : GlobalItem
	{
        public override bool InstancePerEntity => true;

        public override GlobalItem Clone(Item item, Item itemClone)
        {
            PolaritiesItem myClone = (PolaritiesItem)base.Clone(item, itemClone);
            myClone.flawless = flawless;
            return myClone;
        }

        public bool flawless;

        public override void SetDefaults(Item item)
        {
            switch(item.type)
            {
                case ItemID.EmpressBlade:
                    flawless = true;
                    item.rare = RarityType<EmpressFlawlessRarity>();
                    break;
            }
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            int i;
            for (i = 0; i < tooltips.Count; i++)
            {
                if (tooltips[i].Name == "JourneyResearch" || tooltips[i].Name == "BestiaryNotes")
                {
                    break;
                }
            }

            if (!item.social && item.prefix > 0)
            {
                if (item.autoReuse ^ Main.tooltipPrefixComparisonItem.autoReuse)
                {
                    if (item.autoReuse)
                    {
                        TooltipLine line = new TooltipLine(Mod, "PrefixAutomated", Language.GetTextValue("Mods.Polarities.ItemTooltip.AutomaticPrefix"))
                        {
                            IsModifier = true
                        };
                        tooltips.Insert(i, line);
                        i++;
                    }
                    else
                    {
                        TooltipLine line = new TooltipLine(Mod, "PrefixAutomated", Language.GetTextValue("Mods.Polarities.ItemTooltip.ManualPrefix"))
                        {
                            IsModifier = true,
                            IsModifierBad = true
                        };
                        tooltips.Insert(i, line);
                        i++;
                    }
                }
            }

            if (flawless)
            {
                tooltips.Insert(i, new TooltipLine(Mod, "Flawless", Language.GetTextValue("Mods.Polarities.ItemTooltip.TooltipFlawless")));
                i++;
            }
        }

        public override bool OnPickup(Item item, Player player)
        {
            if (item.type == ItemID.Star || item.type == ItemID.SoulCake || item.type == ItemID.SugarPlum && player.GetModPlayer<PolaritiesPlayer>().manaStarMultiplier > 1f)
            {
                int manaVal = (int)(100 * (player.GetModPlayer<PolaritiesPlayer>().manaStarMultiplier - 1f));
                player.statMana += manaVal;
                player.ManaEffect(manaVal);
            }
            return true;
        }
    }
}

