using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Accessories
{
    public class SunlightEmblem : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Magic) += 0.15f;
            if (player.statMana < player.statManaMax2)
            {
                player.GetModPlayer<PolaritiesPlayer>().nonMagicDamage += 0.1f;
            }
            player.GetModPlayer<PolaritiesPlayer>().solarEnergizer = true;
            player.manaCost -= 0.1f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SorcererEmblem)
                .AddIngredient(ItemType<SolarEnergizer>())
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}
