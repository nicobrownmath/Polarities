﻿using Terraria.ModLoader;
using Terraria.ID;
using Polarities.NPCs;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria;

namespace Polarities.Items.Armor.Vanity
{
	[AutoloadEquip(EquipType.Head)]
	public class SnekHat : ModItem
	{
        public override void SetStaticDefaults()
        {
			this.SetResearch(1);

			int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
			ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = true;
			ArmorIDs.Head.Sets.DrawsBackHairWithoutHeadgear[equipSlotHead] = true;
			ArmorIDs.Head.Sets.DrawHatHair[equipSlotHead] = true;
			ArmorIDs.Head.Sets.DrawFullHair[equipSlotHead] = false;
		}

        public override void SetDefaults()
		{
			Item.width = 22;
			Item.height = 18;
			Item.rare = ItemRarityID.Blue;
			Item.vanity = true;
		}
	}
}
