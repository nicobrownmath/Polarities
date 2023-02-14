using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Materials
{
    public class EvilDNA : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (5);
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 10);
            Item.rare = ItemRarityID.LightPurple;
        }
    }
}

