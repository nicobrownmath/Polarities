using System;
using Microsoft.Xna.Framework;
using Polarities.Biomes;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items
{
    public class HallowInvasionSummonItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityBossSpawns[Item.type] = 12;

            this.SetResearch(3);
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 36;
            Item.maxStack = 20;
            Item.rare = 1;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = 4;
            Item.consumable = true;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ZoneHallow && player.ZoneOverworldHeight && !PolaritiesSystem.hallowInvasion;
        }

        public override bool? UseItem(Player player)
        {
            SoundEngine.PlaySound(SoundID.Item, player.position, 4);
            HallowInvasion.StartInvasion();
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.PixieDust, 6)
                .AddIngredient(ItemID.UnicornHorn, 1)
                .AddIngredient(ItemID.HallowedBar, 4)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}