﻿using Terraria;
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
using Polarities.Items.Weapons.Ranged.Atlatls;
using Terraria.GameContent.ItemDropRules;

namespace Polarities.Items
{
	public class PolaritiesItem : GlobalItem
	{
        public override bool InstancePerEntity => true;

        public override void Load()
        {
            //custom biome mimic summons
            Terraria.On_NPC.BigMimicSummonCheck += NPC_BigMimicSummonCheck;

            Terraria.UI.On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color;
        }

        private void ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color(Terraria.UI.On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig, SpriteBatch spriteBatch, Item[] inv, int context, int slot, Vector2 position, Color lightColor)
        {
            bool doDraw = true;
            IInventoryDrawItem inventoryDrawItem = null;
            if (inv[slot] != null && inv[slot].ModItem != null && inv[slot].ModItem is IInventoryDrawItem)
            {
                inventoryDrawItem = inv[slot].ModItem as IInventoryDrawItem;
                doDraw = inventoryDrawItem.PreInventoryDraw(spriteBatch, inv, context, slot, position, lightColor);
            }
            if (doDraw)
            {
                orig(spriteBatch, inv, context, slot, position, lightColor);

                if (inventoryDrawItem != null)
                {
                    inventoryDrawItem.PostInventoryDraw(spriteBatch, inv, context, slot, position, lightColor);
                }
            }
        }

        private bool NPC_BigMimicSummonCheck(Terraria.On_NPC.orig_BigMimicSummonCheck orig, int x, int y, Player user)
        {
            //adapted from vanilla
			if (Main.netMode == NetmodeID.MultiplayerClient || !Main.hardMode)
			{
				return false;
			}
			int chestIndex = Chest.FindChest(x, y);
			if (chestIndex < 0)
			{
				return false;
			}
			bool doSummon = false;
			int numItems = 0;
			int summonType = 0;
			for (int i = 0; i < 40; i++)
			{
				ushort num5 = Main.tile[Main.chest[chestIndex].x, Main.chest[chestIndex].y].TileType;
				int num6 = Main.tile[Main.chest[chestIndex].x, Main.chest[chestIndex].y].TileFrameX / 36;
				if (TileID.Sets.BasicChest[num5] && (num5 != 21 || num6 < 5 || num6 > 6) && Main.chest[chestIndex].item[i] != null && Main.chest[chestIndex].item[i].type > ItemID.None)
				{
					if (Main.chest[chestIndex].item[i].ModItem != null && Main.chest[chestIndex].item[i].ModItem is IBiomeMimicSummon biomeMimicSummon)
					{
						summonType = biomeMimicSummon.SpawnMimicType;
                        doSummon = true;
					}
					numItems += Main.chest[chestIndex].item[i].stack;

                    if (numItems > 1)
                    {
                        doSummon = false;
                        break;
                    }
                }
            }
			if (doSummon && summonType != 0)
			{
				_ = 1;
				if (TileID.Sets.BasicChest[Main.tile[x, y].TileType])
				{
					if (Main.tile[x, y].TileFrameX % 36 != 0)
					{
						x--;
					}
					if (Main.tile[x, y].TileFrameY % 36 != 0)
					{
						y--;
					}
					int number = Chest.FindChest(x, y);
					for (int j = 0; j < 40; j++)
					{
						Main.chest[chestIndex].item[j] = new Item();
					}
					Chest.DestroyChest(x, y);
					for (int k = x; k <= x + 1; k++)
					{
						for (int l = y; l <= y + 1; l++)
						{
							if (TileID.Sets.BasicChest[Main.tile[k, l].TileType])
							{
								Main.tile[k, l].ClearTile();
							}
						}
					}
					int number2 = 1;
					if (Main.tile[x, y].TileType == 467)
					{
						number2 = 5;
					}
					if (Main.tile[x, y].TileType >= 625)
					{
						number2 = 101;
					}
					NetMessage.SendData(MessageID.ChestUpdates, -1, -1, null, number2, x, y, 0f, number, Main.tile[x, y].TileType);
					NetMessage.SendTileSquare(-1, x, y, 3);
				}
				int num8 = NPC.NewNPC(user.GetSource_TileInteraction(x, y), x * 16 + 16, y * 16 + 32, summonType);
				Main.npc[num8].whoAmI = num8;
				NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, num8);
				Main.npc[num8].BigMimicSpawnSmoke();
                return false;
			}

			return orig(x, y, user);
		}

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
                if (Main.tooltipPrefixComparisonItem != null && item.autoReuse ^ Main.tooltipPrefixComparisonItem.autoReuse) //TODO: This doesn't seem to work?
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
                if (manaVal > 0)
                {
                    player.statMana += manaVal;
                    player.ManaEffect(manaVal);
                }
            }
            return true;
        }

        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            switch (item.type)
            {
                case ItemID.WormScarf:
                    if (player.GetModPlayer<PolaritiesPlayer>().wormScarf)
                    {
                        player.endurance -= 0.12f;
                    }
                    player.GetModPlayer<PolaritiesPlayer>().wormScarf = true;
                    break;
            }
        }

        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            switch (item.type) {
                case ItemID.PlanteraBossBag:
                    {
                        itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<JunglesRage>(), chanceDenominator: 4));
                    }

                    break;
            }
        }
    }
}

