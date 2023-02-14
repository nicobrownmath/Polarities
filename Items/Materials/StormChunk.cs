using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Materials
{
    public class StormChunk : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (25);
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 8));
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.maxStack = 9999;
            Item.value = 50;
            Item.rare = 1;
        }

        public override void AddRecipes()
        {
            CreateRecipe(5)
                .AddIngredient(ItemType<NPCs.Critters.StormcloudCichlidItem>())
                .Register();
        }
    }
}