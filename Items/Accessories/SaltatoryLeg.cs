using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Accessories
{
    [AutoloadEquip(EquipType.Shoes)]
    public class SaltatoryLeg : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 34;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 1, silver: 25);
            Item.rare = ItemRarityID.Green;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.jumpSpeedBoost += 3.6f;
            player.extraFall += 15;
            player.autoJump = true;
            player.GetModPlayer<PolaritiesPlayer>().hopperCrystal = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FrogLeg)
                .AddIngredient(ItemType<HopperCrystal>())
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}