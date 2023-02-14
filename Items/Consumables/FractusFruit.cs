using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Consumables
{
    public class FractusFruit : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 26;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 5);
            Item.rare = ItemRarityID.White;
            Item.consumable = true;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useTurn = true;
            Item.useStyle = 2;
            Item.UseSound = SoundID.Item2;
            Item.buffType = BuffID.WellFed;
            Item.buffTime = 7200;
        }

        public override bool ConsumeItem(Player player)
        {
            //if (FractalSubworld.Active)
            //{
            //    player.AddBuff(BuffType<FractalSubworldDebuff>(), 10 * 60);
            //    player.GetModPlayer<PolaritiesPlayer>().suddenFractalizationChange = true;
            //}

            return true;
        }
    }

    public class FractusJuice : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 30;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 5);
            Item.rare = ItemRarityID.Blue;
            Item.consumable = true;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.UseSound = SoundID.Item3;
            Item.buffType = BuffID.WellFed2;
            Item.buffTime = 7200;
        }

        public override bool ConsumeItem(Player player)
        {
            //if (FractalSubworld.Active)
            //{
            //    player.AddBuff(BuffType<FractalSubworldDebuff>(), 20 * 60);
            //    player.GetModPlayer<PolaritiesPlayer>().suddenFractalizationChange = true;
            //}

            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Bottle)
                .AddIngredient<FractusFruit>()
                .AddTile(TileID.CookingPots)
                .Register();
        }
    }
}