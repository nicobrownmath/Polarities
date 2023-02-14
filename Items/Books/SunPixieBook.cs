using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Effects;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Books
{
    public class SunPixieBook : BookBase
    {
        public override int BuffType => BuffType<SunPixieBookBuff>();
        public override int BookIndex => 18;
    }

    public class SunPixieBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<SunPixieBook>();

        private static int trailTimer;

        public override void Update(Player player, ref int buffIndex)
        {
            trailTimer++;
            if (trailTimer == 40)
            {
                trailTimer = 0;
                //summon projectile
                Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.Center, new Vector2(0.25f, 0).RotatedByRandom(Math.PI * 2), ProjectileType<SunPixieBookTrail>(), 30, 0, player.whoAmI);
            }

            base.Update(player, ref buffIndex);
        }
    }

    public class SunPixieBookTrail : ModProjectile
    {
        public override string Texture => "Polarities/NPCs/SunPixie/SunPixieArena";

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.alpha = 0;
            Projectile.timeLeft = 256;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Projectile.frameCounter++;

            if (Projectile.timeLeft < 30)
            {
                Projectile.friendly = false;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockBack, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 450, true);
            Projectile.penetrate -= 1;
            if (Projectile.penetrate == 0) { Projectile.Kill(); }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return CustomCollision.CheckAABBvDisc(targetHitbox, new Circle(Projectile.Center, Projectile.width / 2));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float alphaMult = 0.8f;

            Color ftwColor = Color.White;
            if (Main.getGoodWorld)
            {
                ftwColor = new Color(255, 96, 96);
            }

            int numDraws = 12;
            for (int i = 0; i < numDraws; i++)
            {
                float scale = (1 + (float)Math.Sin(Projectile.frameCounter * 0.1f + (MathHelper.TwoPi * i) / numDraws)) / 2f;
                Color color = new Color(255, (int)(195 * scale + 512 * (1 - scale)), (int)(32 * scale + 512 * (1 - scale))).MultiplyRGB(ftwColor);
                float alpha = 0.2f * alphaMult;
                float rotation = Projectile.frameCounter * 0.2f;

                if (Projectile.timeLeft < 30)
                {
                    scale *= Projectile.timeLeft / 30f;
                }

                Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 64), color * alpha, rotation, new Vector2(32, 32), Projectile.scale * scale, SpriteEffects.None, 0);
            }
            for (int i = 0; i < numDraws; i++)
            {
                float scale = (1 + (float)Math.Sin(Projectile.frameCounter * 0.1f + (MathHelper.TwoPi * i) / numDraws)) / 2f;
                scale *= 0.75f;
                Color color = new Color(255, (int)(195 * scale + 512 * (1 - scale)), (int)(32 * scale + 512 * (1 - scale))).MultiplyRGB(ftwColor);
                float alpha = 0.2f * alphaMult;
                float rotation = Projectile.frameCounter * 0.3f;

                if (Projectile.timeLeft < 45)
                {
                    scale *= (Projectile.timeLeft - 15) / 30f;
                }
                if (Projectile.timeLeft > 15)
                {
                    Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 64), color * alpha, rotation, new Vector2(32, 32), Projectile.scale * scale, SpriteEffects.FlipHorizontally, 0);
                }
            }

            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
            DrawLayer.AddProjectile<DrawLayerAdditiveBeforeNPCs>(index);
        }

        public override bool? CanCutTiles()
        {
            return false;
        }
    }
}