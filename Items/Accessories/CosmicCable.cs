using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Accessories
{
    public class CosmicCable : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 26;
            Item.accessory = true;
            Item.value = 10000;
            Item.rare = ItemRarityID.Expert;
            Item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PolaritiesPlayer>().hookSpeedMult *= 1.5f;
        }
    }
}