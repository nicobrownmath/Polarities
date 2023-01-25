using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using System.Collections.Generic;
using System;
using Polarities.Buffs;

namespace Polarities.Items.Accessories
{
    public class HydraHide : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 20;
            Item.accessory = true;

            Item.rare = ItemRarityID.Pink;
            Item.value = Item.buyPrice(gold: 1);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PolaritiesPlayer>().hydraHide = true;
        }
    }
}