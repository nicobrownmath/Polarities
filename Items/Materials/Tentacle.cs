using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Materials
{
    public class Tentacle : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (25);
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 32;
            Item.maxStack = 9999;
            Item.value = 1000;
            Item.rare = ItemRarityID.LightRed;
        }
    }
}