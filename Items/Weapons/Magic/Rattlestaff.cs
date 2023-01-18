using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Projectiles;
using System;
using Polarities.Buffs;
using Terraria.DataStructures;
using Terraria.Audio;

namespace Polarities.Items.Weapons.Magic
{
    public class Rattlestaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);

            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(15, 0, 0);
            Item.DamageType = DamageClass.Magic;
            Item.mana = 10;

            Item.width = 40;
            Item.height = 40;

            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.noMelee = true;
            Item.useStyle = 5;
            Item.UseSound = Sounds.Rattle;

            Item.shoot = ProjectileType<RattlestaffProjectile>();
            Item.shootSpeed = 2f;

            Item.value = 10000;
            Item.rare = ItemRarityID.Green;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 1; i < 6; i++)
            {
                Projectile.NewProjectile(source, position, i * velocity, type, damage, knockback, player.whoAmI);
            }
            return false;
        }

        public override Vector2? HoldoutOrigin()
        {
            return new Vector2(10, 10);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Items.Materials.Rattle>())
                .AddRecipeGroup(RecipeGroupID.Wood, 12)
                .AddIngredient(ItemID.SandBlock, 6)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    public class RattlestaffProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 4;
            if (Projectile.timeLeft == 1)
            {
                for (int i = 0; i < 5; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, Projectile.velocity.X, Projectile.velocity.Y, Scale: 1f);
                }
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, Projectile.velocity.X, Projectile.velocity.Y, Scale: 1f);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, Projectile.velocity.X, Projectile.velocity.Y, Scale: 1f);
            }
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            return true;
        }
    }
}