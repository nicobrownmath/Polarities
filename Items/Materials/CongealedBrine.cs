using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Materials
{
    public class CongealedBrine : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (25);
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 20;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 15);
            Item.rare = ItemRarityID.LightRed;
        }
    }
}