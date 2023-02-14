using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Materials
{
    public class FractalResidue : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 25;
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 34;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 12);
            Item.rare = ItemRarityID.Pink;
        }
    }
}
