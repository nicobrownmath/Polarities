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
using Terraria.Audio;
using MultiHitboxNPCLibrary;

namespace Polarities.Items.Weapons.Melee.Warhammers
{
    public class BonyBackhand : WarhammerBase
    {
        public override int HammerLength => 35;
        public override int HammerHeadSize => 14;
        public override int DefenseLoss => 16;
        public override int DebuffTime => 1200;
        public override bool CollideWithTiles => false;
        public override float MainSwingAmount => MathHelper.TwoPi * 2;
        public override float SwingTime => 20f;
        public override float SwingTilt => 0.1f;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            PolaritiesItem.IsFlawless.Add(Type);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(50, 18, 0);
            Item.DamageType = DamageClass.Melee;

            Item.width = 44;
            Item.height = 46;

            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = WarhammerUseStyle;
            Item.autoReuse = true;

            Item.value = Item.sellPrice(gold: 4);
            Item.rare = RarityType<SkeletronFlawlessRarity>();
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            base.OnHitNPC(player, target, damage, knockBack, crit);

            for (int i = 0; i < 2; i++)
            {
                Vector2 hitboxDisplacement = GetHitboxCenter(player);
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), hitboxDisplacement, (Main.MouseWorld - hitboxDisplacement).SafeNormalize(Vector2.Zero).RotatedByRandom(MathHelper.Pi / 16) * 16, ProjectileType<BonyBackhandShard>(), Item.damage / 3, 0f, player.whoAmI);
            }
        }
    }

    public class BonyBackhandShard : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 10;
            Projectile.height = 10;
            DrawOffsetX = -6;
            DrawOriginOffsetX = 3;
            Projectile.alpha = 0;
            Projectile.timeLeft = 600;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;

            Projectile.GetGlobalProjectile<MultiHitboxNPCLibraryProjectile>().badCollision = true;
            Projectile.GetGlobalProjectile<MultiHitboxNPCLibraryProjectile>().javelinSticking = true;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (targetHitbox.Width > 8 && targetHitbox.Height > 8)
            {
                targetHitbox.Inflate(-targetHitbox.Width / 8, -targetHitbox.Height / 8);
            }
            return projHitbox.Intersects(targetHitbox);
        }

        public bool IsStickingToTarget
        {
            get => Projectile.ai[0] == 1f;
            set => Projectile.ai[0] = value ? 1f : 0f;
        }

        // Index of the current target
        public int TargetWhoAmI
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private const int MAX_STICKY_JAVELINS = 32; // This is the max. amount of javelins being able to attach
        private readonly Point[] _stickingJavelins = new Point[MAX_STICKY_JAVELINS]; // The point array holding for sticking javelins

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            IsStickingToTarget = true;
            TargetWhoAmI = target.whoAmI;
            Projectile.velocity =
                (target.Center - Projectile.Center) *
                0.75f;
            Projectile.netUpdate = true;

            Projectile.damage = 0;

            UpdateStickyJavelins(target);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Projectile.penetrate++;
            target.immune[Projectile.owner] = 0;
        }

        private void UpdateStickyJavelins(NPC target)
        {
            int currentJavelinIndex = 0;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile currentProjectile = Main.projectile[i];
                if (i != Projectile.whoAmI
                    && currentProjectile.active
                    && currentProjectile.owner == Main.myPlayer
                    && currentProjectile.type == Projectile.type
                    && currentProjectile.ModProjectile is BonyBackhandShard javelinProjectile
                    && javelinProjectile.IsStickingToTarget
                    && javelinProjectile.TargetWhoAmI == target.whoAmI)
                {
                    _stickingJavelins[currentJavelinIndex++] = new Point(i, currentProjectile.timeLeft);
                    if (currentJavelinIndex >= _stickingJavelins.Length)
                        break;
                }
            }

            if (currentJavelinIndex >= MAX_STICKY_JAVELINS)
            {
                int oldJavelinIndex = 0;
                for (int i = 1; i < MAX_STICKY_JAVELINS; i++)
                {
                    if (_stickingJavelins[i].Y < _stickingJavelins[oldJavelinIndex].Y)
                    {
                        oldJavelinIndex = i;
                    }
                }
                Main.projectile[_stickingJavelins[oldJavelinIndex].X].Kill();
            }
        }
        public override void AI()
        {
            if (IsStickingToTarget) StickyAI();
            else NormalAI();
        }

        private void NormalAI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity.Y += 0.3f;
        }

        private void StickyAI()
        {
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            const int aiFactor = 15;
            Projectile.localAI[0] += 1f;

            bool hitEffect = Projectile.localAI[0] % 30f == 0f;
            int projTargetIndex = (int)TargetWhoAmI;
            if (Projectile.localAI[0] >= 60 * aiFactor || projTargetIndex < 0 || projTargetIndex >= 200)
            {
                Projectile.Kill();
            }
            else if (Main.npc[projTargetIndex].active && !Main.npc[projTargetIndex].dontTakeDamage)
            {
                Main.npc[projTargetIndex].GetGlobalNPC<PolaritiesNPC>().boneShards++;
                Projectile.Center = Main.npc[projTargetIndex].Center - Projectile.velocity * 2f;
                Projectile.gfxOffY = Main.npc[projTargetIndex].gfxOffY;
                if (hitEffect)
                {
                    Main.npc[projTargetIndex].HitEffect(0, 1.0);
                }
            }
            else
            {
                Projectile.Kill();
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            return true;
        }
    }
}

