using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Hooks
{
    internal class Skyhook : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.noUseGraphic = true;
            Item.damage = 0;
            Item.knockBack = 7f;
            Item.useStyle = 5;
            Item.shootSpeed = 16f;
            Item.shoot = ProjectileType<SkyhookProjectile>();
            Item.width = 36;
            Item.height = 34;
            Item.UseSound = SoundID.Item1;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.rare = 1;
            Item.noMelee = true;
            Item.value = 10000;
        }
    }

    internal class SkyhookProjectile : ModProjectile
    {
        //texture cacheing
        public static Asset<Texture2D> ChainTexture;

        public override void Load()
        {
            ChainTexture = Request<Texture2D>(Texture + "_Chain");
        }

        public override void Unload()
        {
            ChainTexture = null;

        }

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.aiStyle = 7;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.timeLeft *= 10;
        }

        public override bool PreAI()
        {
            return false;
        }

        public override void PostAI()
        {
            if (Main.player[Projectile.owner].dead || Main.player[Projectile.owner].stoned || Main.player[Projectile.owner].webbed || Main.player[Projectile.owner].frozen)
            {
                Projectile.Kill();
                return;
            }
            float dToPlayer = (Projectile.Center - Main.player[Projectile.owner].MountedCenter).Length();
            Projectile.rotation = (Projectile.Center - Main.player[Projectile.owner].MountedCenter).ToRotation() + (float)Math.PI / 2;

            if (Projectile.ai[0] == 0f)
            {
                if (dToPlayer > GrappleRange())
                {
                    Projectile.ai[0] = 2f;
                }
                if (Main.player[Projectile.owner].grapCount < 10)
                    if (Main.myPlayer == Projectile.owner)
                    {
                        int num82 = 0;
                        int num83 = -1;
                        int num84 = 100000;
                        int numHooks = 2;
                        ProjectileLoader.NumGrappleHooks(Projectile, Main.player[Projectile.owner], ref numHooks);
                        for (int num86 = 0; num86 < 1000; num86++)
                        {
                            if (Main.projectile[num86].active && Main.projectile[num86].owner == Projectile.owner && Main.projectile[num86].aiStyle == 7)
                            {
                                if (Main.projectile[num86].timeLeft < num84)
                                {
                                    num83 = num86;
                                    num84 = Main.projectile[num86].timeLeft;
                                }
                                num82++;
                            }
                        }
                        if (num82 > numHooks)
                        {
                            Main.projectile[num83].Kill();
                        }
                    }
            }
            else if (Projectile.ai[0] == 1f)
            {
                float speed = 0;
                GrappleRetreatSpeed(Main.player[Projectile.owner], ref speed);
                if (dToPlayer < 24f)
                {
                    Projectile.Kill();
                }
                Projectile.velocity = Main.player[Projectile.owner].MountedCenter - Projectile.Center;
                Projectile.velocity.Normalize();
                Projectile.velocity *= speed;
            }
            else
            {
                if (Projectile.ai[0] == 2 && Main.player[Projectile.owner].grapCount < 10)
                {
                    Projectile.velocity = Vector2.Zero;

                    Main.player[Projectile.owner].grappling[Main.player[Projectile.owner].grapCount] = Projectile.whoAmI;
                    Player player = Main.player[Projectile.owner];
                    player.grapCount++;
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.ai[0] = 1f;
            Projectile.tileCollide = false;
            return false;
        }

        // Amethyst Hook is 300, Static Hook is 600
        public override float GrappleRange()
        {
            return 400f;
        }

        public override void NumGrappleHooks(Player player, ref int numHooks)
        {
            numHooks = 1;
        }

        // default is 11, Lunar is 24
        public override void GrappleRetreatSpeed(Player player, ref float speed)
        {
            speed = 16f;
        }

        public override void GrapplePullSpeed(Player player, ref float speed)
        {
            speed = 4;
        }

        public override bool PreDrawExtras()
        {
            Vector2 playerCenter = Main.player[Projectile.owner].MountedCenter;
            Vector2 center = Projectile.Center;
            Vector2 distToProj = playerCenter - Projectile.Center;
            float projRotation = distToProj.ToRotation() - 1.57f;
            float distance = distToProj.Length();

            center += distToProj.SafeNormalize(Vector2.Zero) * 3f;

            while (distance > 8f && !float.IsNaN(distance))
            {
                distToProj.Normalize();                 //get unit vector
                distToProj *= 8f;                      //speed = 24
                center += distToProj;                   //update draw position
                distToProj = playerCenter - center;    //update distance
                distance = distToProj.Length();
                Color drawColor = Lighting.GetColor(center.ToTileCoordinates());

                //Draw chain
                Main.EntitySpriteDraw(ChainTexture.Value, center - Main.screenPosition,
                    new Rectangle(0, 0, 14, 8), drawColor, projRotation,
                    new Vector2(14 * 0.5f, 8 * 0.5f), 1f, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
