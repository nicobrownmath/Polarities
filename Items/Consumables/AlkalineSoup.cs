using Microsoft.Xna.Framework;
using Polarities.Buffs;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Consumables
{
    public class AlkalineSoup : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (10);

            DrawAnimationVertical animation = new DrawAnimationVertical(1, 3)
            {
                NotActuallyAnimating = true
            };
            Main.RegisterItemAnimation(Type, animation);

            ItemID.Sets.DrinkParticleColors[Type] = new Color[] { Color.Lime, Color.LimeGreen };
            ItemID.Sets.IsFood[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 24;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 5);
            Item.rare = ItemRarityID.Blue;
            Item.consumable = true;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.UseSound = SoundID.Item3;
            Item.buffType = BuffID.WellFed;
            Item.buffTime = 10800;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Materials.AlkalineFluid>(), 10)
                .AddIngredient(ItemType<Placeable.Blocks.Limestone>(), 4)
                .AddTile(TileID.CookingPots)
                .Register();
        }

        public override void OnConsumeItem(Player player)
        {
            player.AddBuff(BuffType<Corroding>(), 5 * 60 * (Main.expertMode ? 1 : 2));
        }
    }
}