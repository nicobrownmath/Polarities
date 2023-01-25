using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.NPCs;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.GameContent;
using Terraria.Audio;

namespace Polarities.Items.Weapons.Melee
{
    public class Stardance : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true;

            SacrificeTotal = (1);
            PolaritiesItem.IsFlawless.Add(Type);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(23, 3.1f, 0);
            Item.DamageType = DamageClass.Melee;

            Item.width = 42;
            Item.height = 46;

            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.noMelee = false;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;

            Item.shoot = ProjectileType<StardanceProjectile>();
            Item.shootSpeed = 4f;

            Item.value = Item.sellPrice(gold: 2);
            Item.rare = RarityType<StarConstructFlawlessRarity>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundEngine.PlaySound(SoundID.Item8, player.Center);
            for (int i = 1; i < 6; i++)
            {
                Projectile.NewProjectile(source, position, i * velocity, type, damage, knockback, player.whoAmI);
            }
            return false;
        }

        public override Vector2? HoldoutOrigin()
        {
            return new Vector2(5, 5);
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            noHitbox = true;
            double rotation = player.itemRotation;
            if (player.direction == -1)
            {
                rotation += MathHelper.Pi;
            }
            Vector2 unitVector;
            if (player.direction == -1) { unitVector = new Vector2(-(float)Math.Cos(rotation - Math.PI), -(float)Math.Sin(rotation - Math.PI)); }
            else { unitVector = new Vector2(-(float)Math.Cos(-rotation - Math.PI), (float)Math.Sin(-rotation - Math.PI)); }
            for (int i = 0; i < Main.npc.Length; i++)
            {
                NPC target = Main.npc[i];
                if (target.active)
                {
                    float point = 0f;
                    if (player.whoAmI == Main.myPlayer && Collision.CheckAABBvLineCollision(target.Hitbox.TopLeft(), target.Hitbox.Size(), player.itemLocation, player.itemLocation + unitVector * Item.width * Item.scale * (float)Math.Sqrt(2), 7, ref point))
                    {
                        if (!target.dontTakeDamage && target.immune[player.whoAmI] <= 0)
                        {
                            if (noHitbox || (target.Center - player.itemLocation).Length() < ((new Vector2(hitbox.Center.X, hitbox.Center.Y)) - player.itemLocation).Length())
                            {
                                hitbox = target.Hitbox;
                                noHitbox = false;
                            }
                        }
                    }
                }
            }
        }
    }

    public class StardanceProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Star Lance");
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            DrawOffsetX = -28;
            DrawOriginOffsetX = 14;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 2;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.light = 1;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (Projectile.timeLeft % 2 == 0)
            {
                Dust.NewDustPerfect(Projectile.Center, DustID.Electric, Velocity: 0.25f * Projectile.velocity.RotatedBy(MathHelper.PiOver2), newColor: Color.LightBlue, Scale: 0.75f).noGravity = true;
                Dust.NewDustPerfect(Projectile.Center, DustID.Electric, Velocity: 0.25f * Projectile.velocity.RotatedBy(-MathHelper.PiOver2), newColor: Color.LightBlue, Scale: 0.75f).noGravity = true;
                Dust.NewDustPerfect(Projectile.Center, DustID.Electric, Velocity: Vector2.Zero, newColor: Color.LightBlue, Scale: 1f).noGravity = true;
            }

            NPC target = Projectile.FindTargetWithinRange(2000f);

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            if (Projectile.timeLeft > 540)
            {
                Projectile.velocity *= 0.95f;
            }
            else
            {
                if (target != null)
                {
                    Vector2 a = target.Center - Projectile.Center;
                    if (a.Length() > 1) { a.Normalize(); }
                    a *= 0.2f;
                    Projectile.velocity += a;
                }
                else
                {
                    Vector2 a = Projectile.velocity;
                    if (a.Length() > 1) { a.Normalize(); }
                    a *= 0.2f;
                    Projectile.velocity += a;
                }

                Projectile.velocity *= 1.005f;
            }

            if (Projectile.velocity.Length() > 24)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= 24;
            }
        }
    }
}

