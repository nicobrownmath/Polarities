using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Accessories
{
    [AutoloadEquip(EquipType.Face)]
    public class ReadingGlasses : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 8;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 1);
            Item.rare = ItemRarityID.Pink;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PolaritiesPlayer>().maxBookSlots = Math.Max(player.GetModPlayer<PolaritiesPlayer>().maxBookSlots, 2);
        }
    }

    [AutoloadEquip(EquipType.Face)]
    public class SpectralLenses : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 8;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 1);
            Item.rare = ItemRarityID.Yellow;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PolaritiesPlayer>().maxBookSlots = Math.Max(player.GetModPlayer<PolaritiesPlayer>().maxBookSlots, 3);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<ReadingGlasses>())
                .AddIngredient(ItemID.Ectoplasm, 20)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
