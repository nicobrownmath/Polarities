using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Polarities.Projectiles;
using System;
using Terraria.DataStructures;
using Terraria.Audio;

namespace Polarities.Items.Weapons.Magic
{
    public class WormSpewer : ModItem
    {
        private Projectile priorSegment;
        private int timer;

        public override void SetStaticDefaults()
        {
            this.SetResearch(1);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(56, 1, 0);
            Item.DamageType = DamageClass.Magic;
            Item.mana = 2;

            Item.width = 92;
            Item.height = 38;

            Item.useTime = 3;
            Item.useAnimation = 3;
            Item.useStyle = 5;
            Item.noMelee = true;

            Item.value = 80000;
            Item.rare = 8;

            Item.autoReuse = true;
            Item.shoot = ProjectileType<WormSpewerProjectile>();
            Item.shootSpeed = 12f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 1; i++)
            {
                int segment = Projectile.NewProjectile(source, new Vector2(position.X + player.direction * Item.width * 0.6f * (float)Math.Cos(player.itemRotation), position.Y + player.direction * Item.width * 0.75f * (float)Math.Sin(player.itemRotation)), velocity, type, damage, knockback, player.whoAmI, 0, 0);
                timer++;
                if (priorSegment != null && priorSegment.active)
                {
                    if (priorSegment.timeLeft > 750 - 9)
                    {
                        Main.projectile[segment].ai[0] = priorSegment.whoAmI + 1;
                        priorSegment.ai[1] = segment + 1;
                    }
                    else
                    {
                        if (timer > 20) { timer = 0; }
                    }
                    if (timer % 30 == 0)
                    {
                        timer = 0;
                        SoundEngine.PlaySound(SoundID.NPCDeath13, player.Center);
                    }
                }
                priorSegment = Main.projectile[segment];
            }

            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-20, 4);
        }
    }

    public class WormSpewerProjectile : ModProjectile
    {
        public bool head = false;
        private bool tail = false;
        private int following
        {
            get => (int)Projectile.ai[0] - 1;
            set => Projectile.ai[0] = value + 1;
        }
        private int follower
        {
            get => (int)Projectile.ai[1] - 1;
            set => Projectile.ai[1] = value + 1;
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 14;
            Projectile.height = 14;
            DrawOffsetX = -14;
            Projectile.timeLeft = 750;
            Projectile.penetrate = 1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            head = head || !(following >= 0 && Main.projectile[following].active);
            tail = tail || (!(follower >= 0 && Main.projectile[follower].active) && Projectile.timeLeft < 750 - 9);

            if (head && tail)
            {
                Projectile.Kill();
            }

            if (head)
            {
                Projectile.frame = 0;
            }
            else if (tail)
            {
                Projectile.frame = 2;
            }
            else
            {
                Projectile.frame = 1;
            }

            if (head)
            {
                NPC target = Projectile.FindTargetWithinRange(400000f);

                if (target != null)
                {
                    Vector2 targetPoint = target.Center;
                    Vector2 velocityGoal = 18 * (targetPoint - Projectile.Center) / (targetPoint - Projectile.Center).Length();
                    Projectile.velocity += (velocityGoal - Projectile.velocity) / 60;
                }

                Projectile.rotation = Projectile.velocity.ToRotation();

                if (Projectile.velocity.Length() < 2 && Projectile.velocity.Length() > 0.01f)
                {
                    Projectile.velocity = 2 * Projectile.velocity / Projectile.velocity.Length();
                }
            }
            ChainUpdate();
        }

        public void ChainUpdate()
        {
            if (!head)
            {
                Projectile priorSegment = Main.projectile[following];
                Vector2 target = priorSegment.Center - 2 * priorSegment.width * (new Vector2(1, 0)).RotatedBy(priorSegment.rotation) / 2;
                Projectile.rotation = (target - Projectile.Center).ToRotation();
                Vector2 targetVelocity = (target - (Projectile.Center - Projectile.position) - 14f * (target - Projectile.Center) / (target - Projectile.Center).Length() - Projectile.position);
                if (targetVelocity.Length() > priorSegment.velocity.Length())
                {
                    Projectile.velocity = priorSegment.velocity.Length() * targetVelocity / targetVelocity.Length();
                }
                else
                {
                    Projectile.velocity = targetVelocity;
                }
            }
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (Projectile.timeLeft < 750 - 9)
            {
                return null;
            }
            else
            {
                return false;
            }
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (!tail) { (Main.projectile[follower].ModProjectile as WormSpewerProjectile).head = true; }
            if (!head) { (Main.projectile[following].ModProjectile as WormSpewerProjectile).tail = true; }
        }
    }
}