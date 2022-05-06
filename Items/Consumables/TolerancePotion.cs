using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using System;
using Terraria.DataStructures;

namespace Polarities.Items.Consumables
{
    //TODO: Draw with an x over it in the hotbar if on cooldown
    public class TolerancePotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            this.SetResearch(20);

            ItemID.Sets.DrinkParticleColors[Type] = new Color[] { Color.White, Color.LightPink };
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 34;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 30;
            Item.consumable = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 2);
            Item.buffType = BuffType<TolerancePotionCooldownBuff>();
            Item.buffTime = 3600;
        }

        public override bool CanUseItem(Player player)
        {
            return !player.HasBuff(BuffType<TolerancePotionCooldownBuff>()) && player.HasBuff(BuffID.PotionSickness);
        }

        public override bool ConsumeItem(Player player)
        {
            player.buffTime[player.FindBuffIndex(BuffID.PotionSickness)] /= 2;
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BottledWater)
                .AddIngredient(ItemType<Placeable.SaltCrystals>())
                .AddIngredient(ItemType<Materials.BrineShrimp>())
                .AddTile(TileID.Bottles)
                .Register();
        }
    }

    public class TolerancePotionCooldownBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
        }
    }
}

