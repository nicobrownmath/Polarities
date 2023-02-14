using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Accessories
{
    [AutoloadEquip(EquipType.Shield)]
    public class LimestoneShield : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 24;
            Item.accessory = true;
            Item.value = 20000 * 5;
            Item.rare = ItemRarityID.Pink;
            Item.defense = 4;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PolaritiesPlayer>().limestoneShield = true;

            if (player.GetModPlayer<PolaritiesPlayer>().limestoneShieldCooldown == 0)
            {
                player.endurance = 1 - (1 - player.endurance) * 0.67f;
            }
        }
    }
}