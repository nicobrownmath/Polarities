using System;
using Microsoft.Xna.Framework;
using Polarities.Biomes;
using Polarities.NPCs.Esophage;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items
{
    public class WorldEvilInvasionSummonItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityBossSpawns[Item.type] = 12;

            this.SetResearch(3);
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 44;
            Item.maxStack = 20;
            Item.rare = 1;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = 4;
            Item.consumable = true;
        }

        public override bool CanUseItem(Player player)
        {
            return (player.ZoneCorrupt || player.ZoneCrimson) && player.ZoneOverworldHeight && !PolaritiesSystem.worldEvilInvasion && PolaritiesSystem.esophageSpawnTimer == 0 && !NPC.AnyNPCs(NPCType<Esophage>());
        }

        public override bool? UseItem(Player player)
        {
            SoundEngine.PlaySound(SoundID.Roar, player.position);
            WorldEvilInvasion.StartInvasion();
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup("RottenChunk", 10)
                .AddIngredient(ItemID.SoulofMight, 1)
                .AddIngredient(ItemID.SoulofSight, 1)
                .AddIngredient(ItemID.SoulofFright, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}