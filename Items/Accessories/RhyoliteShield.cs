using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Accessories
{
    [AutoloadEquip(EquipType.Shield)]
    public class RhyoliteShield : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 2, silver: 50);
            Item.rare = ItemRarityID.Lime;
            Item.defense = 4;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PolaritiesPlayer>().limestoneShield = true;

            if (player.GetModPlayer<PolaritiesPlayer>().limestoneShieldCooldown == 0)
            {
                player.endurance = 1 - (1 - player.endurance) * 0.67f;
            }
            player.fireWalk = true;
            player.noKnockback = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<LimestoneShield>())
                .AddIngredient(ItemID.ObsidianShield)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}
