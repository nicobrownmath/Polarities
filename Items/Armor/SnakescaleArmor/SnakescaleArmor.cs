using Polarities.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Armor.SnakescaleArmor
{
    [AutoloadEquip(EquipType.Body)]
    public class SnakescaleArmor : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 18;
            Item.value = 50000;
            Item.defense = 16;
            Item.rare = ItemRarityID.LightPurple;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.05f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<SerpentScale>(), 24)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class SnakescaleGreaves : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 12;
            Item.value = 50000;
            Item.defense = 12;
            Item.rare = ItemRarityID.LightPurple;
        }

        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.1f;
            player.GetModPlayer<PolaritiesPlayer>().runSpeedBoost += 0.1f;
            player.accFlipper = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<SerpentScale>(), 18)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    [AutoloadEquip(EquipType.Head)]
    public class SnakescaleMask : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 18;
            Item.value = 50000;
            Item.defense = 12;
            Item.rare = ItemRarityID.LightPurple;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetCritChance(DamageClass.Generic) += 8;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ItemType<SnakescaleArmor>() && legs.type == ItemType<SnakescaleGreaves>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = Language.GetTextValue("Mods.Polarities.ArmorSetBonus." + Name);

            player.ignoreWater = true;
            player.GetModPlayer<PolaritiesPlayer>().snakescaleSetBonus = true;
            player.GetModPlayer<PolaritiesPlayer>().critDamageBoostMultiplier *= 1.5f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<SerpentScale>(), 12)
                .AddIngredient(ItemType<VenomGland>(), 6)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}