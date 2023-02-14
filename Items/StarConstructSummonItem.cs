using Polarities.NPCs.StarConstruct;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items
{
    public class StarConstructSummonItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;

            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.rare = ItemRarityID.Blue;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = 4;
        }

        public override bool CanUseItem(Player player)
        {
            return !Main.dayTime && !NPC.AnyNPCs(NPCType<StarConstruct>()) && !NPC.AnyNPCs(NPCType<DormantConstruct>());
        }

        public override bool? UseItem(Player player)
        {
            //summon dormant construct
            Main.npc[NPC.NewNPC(new EntitySource_BossSpawn(player), (int)player.Center.X + Main.rand.Next(-500, 500), (int)player.position.Y - 1000, NPCType<NPCs.StarConstruct.DormantConstruct>())].netUpdate = true;
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup(RecipeGroupID.IronBar, 20)
                .AddIngredient(ItemID.FallenStar, 20)
                .AddTile(TileID.DemonAltar)
                .Register();
        }
    }
}