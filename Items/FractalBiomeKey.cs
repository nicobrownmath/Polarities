using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items
{
    public class FractalBiomeKey : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
            ItemID.Sets.UsesCursedByPlanteraTooltip[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.HallowedKey);
        }
    }
}
