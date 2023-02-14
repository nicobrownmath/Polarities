using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items
{
    public class Everlight : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.value = 10000;
            Item.rare = ItemRarityID.Expert;
            Item.expert = true;
            Item.holdStyle = 1;
        }

        public override void HoldItem(Player player)
        {
            if (Lighting.Mode == LightMode.White || Lighting.Mode == LightMode.Color)
                Lighting.AddLight(Main.MouseWorld, new Vector3(10f, 10f, 10f));
            else
                Lighting.AddLight(Main.MouseWorld, new Vector3(2f, 2f, 2f));
            player.scope = true;
        }

        public override void HoldStyle(Player player, Rectangle heldItemFrame)
        {
            player.itemLocation = player.Center + new Vector2(0, Item.width / 2);
        }
    }
}