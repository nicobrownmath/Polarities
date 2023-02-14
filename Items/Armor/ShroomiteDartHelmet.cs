using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Polarities.Items.Armor
{
    [AutoloadEquip(EquipType.Head)]
    public class ShroomiteDartHelmet : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.ShroomiteHelmet);
            Item.headSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
        }

        public override void UpdateEquip(Player player)
        {
            player.GetCritChance(DamageClass.Ranged) += 5f;
            player.GetModPlayer<PolaritiesPlayer>().dartDamage += 0.15f;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ItemID.ShroomiteBreastplate && legs.type == ItemID.ShroomiteLeggings;
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = Language.GetTextValue("ArmorSetBonus.Shroomite");
            player.shroomiteStealth = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ShroomiteBar, 12)
                .AddTile(TileID.MythrilAnvil)
                .SortAfterFirstRecipesOf(ItemID.ShroomiteHelmet)
                .Register();
        }
    }
}

