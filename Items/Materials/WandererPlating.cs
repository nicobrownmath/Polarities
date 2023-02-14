using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Materials
{
    public class WandererPlating : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (5);
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ItemRarityID.Yellow;
        }
    }
}