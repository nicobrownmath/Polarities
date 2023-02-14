using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Books
{
    public class EyeOfCthulhuBook : BookBase
    {
        public override int BuffType => BuffType<EyeOfCthulhuBookBuff>();
        public override int BookIndex => 4;
    }

    public class EyeOfCthulhuBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<EyeOfCthulhuBook>();
    }

    public class EyeOfCthulhuBookEye : ModProjectile
    {
        public override string Texture => "Terraria/Images/NPC_5";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("{$Mods.Polarities.ProjectileName.EyeruptionProjectile}");

            Main.projFrames[Projectile.type] = 2;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            DrawOriginOffsetY = -10;
            Projectile.penetrate = 6;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 3600;
        }

        public override void AI()
        {
            int targetID = Projectile.FindTargetWithLineOfSight();
            NPC target = null;
            if (targetID != -1)
            {
                target = Main.npc[targetID];
            }

            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            if (target != null)
            {
                float rotOffset = (target.Center - Projectile.Center).ToRotation() - Projectile.velocity.ToRotation();
                if (rotOffset > Math.PI)
                {
                    rotOffset -= (float)Math.PI * 2;
                }
                else if (rotOffset < -Math.PI)
                {
                    rotOffset += (float)Math.PI * 2;
                }
                rotOffset = Math.Min(0.025f, Math.Max(-0.02f, rotOffset));
                Projectile.velocity = Projectile.velocity.RotatedBy(rotOffset);
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 5)
            {
                Projectile.frame = (Projectile.frame + 1) % 2;
                Projectile.frameCounter = 0;
            }
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
    }
}