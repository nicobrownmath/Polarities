using Polarities.Buffs;
using Polarities.Items.Placeable;
using Polarities.Items.Placeable.Blocks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Armor.HaliteArmor
{
    [AutoloadEquip(EquipType.Body)]
    public class HaliteArmor : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 18;
            Item.value = 5000;
            Item.defense = 5;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<SaltCrystals>(), 18)
                .AddIngredient(ItemType<RockSalt>(), 6)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class HaliteGreaves : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 18;
            Item.value = 5000;
            Item.defense = 5;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<SaltCrystals>(), 15)
                .AddIngredient(ItemType<RockSalt>(), 6)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    [AutoloadEquip(EquipType.Head)]
    public class HaliteHelmet : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 24;
            Item.value = 5000;
            Item.defense = 4;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ItemType<HaliteArmor>() && legs.type == ItemType<HaliteGreaves>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = Language.GetTextValue("Mods.Polarities.ArmorSetBonus." + Name);

            player.buffImmune[BuffType<Desiccating>()] = true;
            player.ignoreWater = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<SaltCrystals>(), 12)
                .AddIngredient(ItemType<RockSalt>(), 6)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}