using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Summon.Minions
{
    public class ZombatStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(8, 1, 0);
            Item.DamageType = DamageClass.Summon;
            Item.mana = 10;

            Item.width = 34;
            Item.height = 34;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = true;

            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ItemRarityID.Blue;

            Item.buffType = BuffType<ZombatMinionBuff>();
            Item.shoot = ProjectileType<ZombatMinion>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 18000, true);
            player.SpawnMinionOnCursor(source, player.whoAmI, type, damage, knockback, offsetFromCursor: Vector2.UnitX);
            player.SpawnMinionOnCursor(source, player.whoAmI, type, damage, knockback, offsetFromCursor: -Vector2.UnitX);
            return false;
        }
    }

    public class ZombatMinionBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ProjectileType<ZombatMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }

    public class ZombatMinion : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 12;
            DrawOffsetX = -14;
            DrawOriginOffsetY = -6;
            DrawOriginOffsetX = 7;

            Projectile.penetrate = -1;
            Projectile.minion = true;
            Projectile.minionSlots = 0.5f;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 3600;
            Projectile.DamageType = DamageClass.Summon;

            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active)
            {
                Projectile.active = false;
                return;
            }
            if (player.dead)
            {
                player.ClearBuff(BuffType<ZombatMinionBuff>());
            }
            if (player.HasBuff(BuffType<ZombatMinionBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            int targetID = -1;
            Projectile.Minion_FindTargetInRange(750, ref targetID, true);

            Vector2 targetPosition;
            Vector2 targetVelocity;

            if (targetID != -1)
            {
                targetPosition = Main.npc[targetID].Center;
                targetVelocity = Main.npc[targetID].velocity;
            }
            else
            {
                targetPosition = player.Center + new Vector2(-player.direction * 64, -64);
                targetVelocity = player.velocity;
            }

            Boids(targetPosition, targetVelocity);

            Projectile.rotation = (float)(0.5 * Math.Atan(Projectile.velocity.X));
            Projectile.spriteDirection = Projectile.velocity.X > 0 ? -1 : 1;

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 4;
            }

            if (Projectile.Distance(player.MountedCenter) > 1000)
            {
                Projectile.Center = player.MountedCenter;
            }
        }

        private void Boids(Vector2 targetPosition, Vector2 targetVelocity)
        {
            //boids
            Vector2 separation = Vector2.Zero;
            Vector2 alignment = Vector2.Zero;
            Vector2 cohesion = Vector2.Zero;
            Vector2 targeting = 1 / 32f * (targetPosition - Projectile.Center) + (targetVelocity - Projectile.velocity);
            int count = 0;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile otherProjectile = Main.projectile[i];

                if (i != Projectile.whoAmI && otherProjectile.type == Projectile.type && otherProjectile.active && (Projectile.Center - otherProjectile.Center).Length() < 128)
                {
                    count++;

                    //separation component
                    separation += 32f * (Projectile.Center - otherProjectile.Center).SafeNormalize(Vector2.Zero) / (Projectile.Center - otherProjectile.Center).Length();

                    //alignment component
                    alignment += 1 / 64f * (otherProjectile.velocity - Projectile.velocity);

                    //cohesion component
                    cohesion += 1 / 128f * (otherProjectile.Center - Projectile.Center);
                }

            }

            if (count > 0)
            {
                alignment /= count;
                cohesion /= count;
            }

            Vector2 goalVelocity = Projectile.velocity + targeting + separation + alignment + cohesion;
            if (goalVelocity.Length() > 8)
            {
                goalVelocity.Normalize();
                goalVelocity *= 8;
            }
            Projectile.velocity += (goalVelocity - Projectile.velocity) / 40;
        }

        public override bool? CanCutTiles()
        {
            return false;
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

        public override bool MinionContactDamage()
        {
            return true;
        }
    }
}