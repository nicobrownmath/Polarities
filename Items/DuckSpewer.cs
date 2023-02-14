using Microsoft.Xna.Framework;
using Polarities.Items.Weapons.Magic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items
{
    public class DuckSpewer : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 96;
            Item.height = 38;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = 5;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.knockBack = 1;
            Item.value = 80000;
            Item.rare = 8;
            Item.shoot = ProjectileType<WormSpewerProjectile>();
            Item.shootSpeed = 8f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundEngine.PlaySound(SoundID.Duck, player.position);
            int duck = NPC.NewNPC(source, (int)position.X, (int)position.Y, Main.rand.NextBool() ? NPCID.Duck2 : NPCID.DuckWhite2);
            Main.npc[duck].velocity = velocity;
            Main.npc[duck].SpawnedFromStatue = true;
            Main.npc[duck].direction = -player.direction;
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-20, 4);
        }

        //TODO: Require a convective duck and have a small chance to shoot them when they're added
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<WormSpewer>())
                .AddRecipeGroup("Duck", 20)
                .AddTile(TileID.DemonAltar)
                .Register();
        }
    }
}