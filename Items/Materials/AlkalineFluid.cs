using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Materials
{
    public class AlkalineFluid : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (25);
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 16;
            Item.maxStack = 9999;
            Item.value = 10;
            Item.rare = ItemRarityID.White;
        }

        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 198 / 512f, 239 / 512f, 159 / 512f);
        }
    }
}