using Terraria;
using Terraria.ModLoader;

namespace Polarities.Items.Accessories
{
    public class SolarEnergizer : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 28;
            Item.accessory = true;
            Item.value = 10000;
            Item.rare = 6;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PolaritiesPlayer>().solarEnergizer = true;
            player.manaCost -= 0.1f;
        }
    }
}