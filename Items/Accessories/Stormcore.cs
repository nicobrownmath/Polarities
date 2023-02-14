using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Accessories
{
    public class Stormcore : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Summon;
            Item.damage = 1;
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.value = 2500;
            Item.rare = ItemRarityID.Green;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (player.ZoneOverworldHeight || player.ZoneSkyHeight)
            {
                player.GetModPlayer<PolaritiesPlayer>().stormcore = true;
            }
        }
    }

    public class StormcoreMinion : ModProjectile
    {
        private bool leaving;
        private int atkCooldown;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            DrawOffsetX = -18;
            DrawOriginOffsetX = 9;
            Projectile.penetrate = -1;
            Projectile.minion = true;
            Projectile.minionSlots = 0.2f;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            if (Projectile.velocity.X > 0)
            {
                Projectile.rotation = (float)Math.Atan(Projectile.velocity.Y / Projectile.velocity.X);
            }
            else
            {
                Projectile.rotation = (float)(Math.PI + Math.Atan(Projectile.velocity.Y / Projectile.velocity.X));
            }

            atkCooldown -= 1;


            Player player = Main.player[Projectile.owner];
            if (!player.active)
            {
                Projectile.active = false;
                return;
            }
            if (player.dead)
            {
                player.GetModPlayer<PolaritiesPlayer>().stormcore = false;
            }
            if (player.GetModPlayer<PolaritiesPlayer>().stormcore)
            {
                leaving = false;
                Projectile.timeLeft = 120;
                Projectile.tileCollide = true;
            }
            else if ((Main.rand.NextBool(300) || leaving) && Main.myPlayer == Projectile.owner)
            {
                leaving = true;
                Projectile.tileCollide = false;
                Projectile.velocity.Y -= 0.2f;

                Projectile.netUpdate = true;

                return;
            }
            else if (!leaving)
            {
                Projectile.timeLeft = 120;
            }

            if (Math.Sqrt((Projectile.Center.X - player.Center.X) * (Projectile.Center.X - player.Center.X) + (Projectile.Center.Y - player.Center.Y) * (Projectile.Center.Y - player.Center.Y)) > 1000)
            {
                Projectile.active = false;
            }

            int targetID = -1;
            Projectile.Minion_FindTargetInRange(750, ref targetID, true);
            NPC target = null;
            if (targetID != -1)
            {
                target = Main.npc[targetID];
            }

            float targetX = player.Center.X;
            float targetY = player.Center.Y - 100;
            float maxSpeed = 8;
            float maxAcc = 0.4f;
            Projectile.friendly = target != null;

            if (target != null)
            {
                targetX = target.Center.X;
                targetY = target.Center.Y;
                maxSpeed = 12;
                maxAcc = 0.8f;
            }

            if (((Projectile.Center.X - targetX) * (Projectile.Center.X - targetX)) + ((Projectile.Center.Y - targetY) * (Projectile.Center.Y - targetY)) > 1000)
            {
                float aX = (targetX - Projectile.Center.X) - Projectile.velocity.X;
                float aY = (targetY - Projectile.Center.Y) - Projectile.velocity.Y;

                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.netUpdate = true;

                    Projectile.velocity.X += (float)Main.rand.NextDouble() * maxAcc * aX / (float)Math.Sqrt(aX * aX + aY * aY);
                    Projectile.velocity.Y += (float)Main.rand.NextDouble() * maxAcc * aY / (float)Math.Sqrt(aX * aX + aY * aY);
                }
            }

            if (Projectile.velocity.X * Projectile.velocity.X + Projectile.velocity.Y * Projectile.velocity.Y > maxSpeed * maxSpeed)
            {
                float newVX = Projectile.velocity.X / (float)Math.Sqrt(Projectile.velocity.X * Projectile.velocity.X + Projectile.velocity.Y * Projectile.velocity.Y);
                float newVY = Projectile.velocity.Y / (float)Math.Sqrt(Projectile.velocity.X * Projectile.velocity.X + Projectile.velocity.Y * Projectile.velocity.Y);

                Projectile.velocity.X = maxSpeed * newVX;
                Projectile.velocity.Y = maxSpeed * newVY;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.immune[Projectile.owner] = 0;
            atkCooldown = 6;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool MinionContactDamage()
        {
            return atkCooldown <= 0;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = true;
            return true;
        }
    }
}

