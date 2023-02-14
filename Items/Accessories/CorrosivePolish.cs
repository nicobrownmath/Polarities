using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Accessories
{
    public class CorrosivePolish : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 28;
            Item.accessory = true;
            Item.value = 2500;
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PolaritiesPlayer>().warhammerDefenseBoost += 10;
            player.GetModPlayer<PolaritiesPlayer>().warhammerTimeBoost += 10 * 60;
        }
    }
}
