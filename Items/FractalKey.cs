using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items
{
    public class FractalKey : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.GoldenKey);
        }
    }
}