using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.NPCs.Esophage;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Hooks
{
    public class PhagefootHook : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Phagefoot Hook");

            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.noUseGraphic = true;
            Item.damage = 0;
            Item.knockBack = 7f;
            Item.useStyle = 5;
            Item.shootSpeed = 16f;
            Item.shoot = ProjectileType<PhagefootProjectile>();
            Item.width = 44;
            Item.height = 44;
            Item.UseSound = SoundID.Item1;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.rare = ItemRarityID.Pink;
            Item.noMelee = true;
            Item.value = 10000 * 5;
        }
    }

    public class PhagefootProjectile : ModProjectile
    {
        public override string Texture => "Polarities/NPCs/Esophage/EsophageClaw";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Phagefoot");
        }

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 18;
            Projectile.height = 18;
            DrawOffsetX = -11;

            Projectile.aiStyle = 7;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft *= 10;

            DrawOriginOffsetY = 180;
        }

        public override bool PreAI()
        {
            if (Projectile.ai[1] == 0)
            {
                Projectile.ai[1] = 1;
                Projectile.position.Y -= 180;
            }
            if (Projectile.ai[0] == 0)
            {
                Projectile.velocity.Y += 0.3f;
            }

            if (Main.player[Projectile.owner].dead || Main.player[Projectile.owner].stoned || Main.player[Projectile.owner].webbed || Main.player[Projectile.owner].frozen)
            {
                Projectile.Kill();
                return false;
            }
            Vector2 mountedCenter3 = Main.player[Projectile.owner].MountedCenter;
            Vector2 vector131 = new Vector2(Projectile.position.X + Projectile.width * 0.5f, Projectile.position.Y + Projectile.height * 0.5f);
            float num2510 = mountedCenter3.X - vector131.X;
            float num2509 = mountedCenter3.Y - vector131.Y;
            float num2508 = (float)Math.Sqrt(num2510 * num2510 + num2509 * num2509);
            Projectile.rotation = (float)Math.Atan2(num2509, num2510) - 1.57f;

            if (Projectile.ai[0] == 0f)
            {
                if (num2508 > GrappleRange())
                {
                    Projectile.ai[0] = 1f;
                }

                Vector2 value170 = Projectile.Center - new Vector2(5f, -180f);
                Vector2 value169 = Projectile.Center + new Vector2(5f, 180f);
                Point point17 = (value170 - new Vector2(16f)).ToTileCoordinates();
                Point point16 = (value169 + new Vector2(32f)).ToTileCoordinates();
                int num2506 = point17.X;
                int num2505 = point16.X;
                int num2504 = point17.Y;
                int num2503 = point16.Y;
                if (num2506 < 0)
                {
                    num2506 = 0;
                }
                if (num2505 > Main.maxTilesX)
                {
                    num2505 = Main.maxTilesX;
                }
                if (num2504 < 0)
                {
                    num2504 = 0;
                }
                if (num2503 > Main.maxTilesY)
                {
                    num2503 = Main.maxTilesY;
                }
                Vector2 vector133 = default(Vector2);
                for (int num2502 = num2506; num2502 < num2505; num2502++)
                {
                    for (int num2501 = num2504; num2501 < num2503; num2501++)
                    {
                        vector133.X = num2502 * 16;
                        vector133.Y = num2501 * 16;
                        if (!(value170.X + 10f > vector133.X) || !(value170.X < vector133.X + 16f) || !(value170.Y + 10f > vector133.Y) || !(value170.Y < vector133.Y + 16f) || !Main.tile[num2502, num2501].HasUnactuatedTile || (!Main.tileSolid[Main.tile[num2502, num2501].TileType] && Main.tile[num2502, num2501].TileType != 314))
                        {
                            continue;
                        }
                        if (Main.player[Projectile.owner].grapCount < 10)
                        {
                            Main.player[Projectile.owner].grappling[Main.player[Projectile.owner].grapCount] = Projectile.whoAmI;
                            Player player25 = Main.player[Projectile.owner];
                            Player player26 = player25;
                            player26.grapCount++;
                        }
                        if (Main.myPlayer == Projectile.owner)
                        {
                            int num2500 = 0;
                            int num2499 = -1;
                            int num2498 = 100000;
                            int num2497 = 3;

                            ProjectileLoader.NumGrappleHooks(Projectile, Main.player[Projectile.owner], ref num2497);

                            for (int num2496 = 0; num2496 < 1000; num2496++)
                            {
                                if (Main.projectile[num2496].active && Main.projectile[num2496].owner == Projectile.owner && Main.projectile[num2496].aiStyle == 7)
                                {
                                    if (Main.projectile[num2496].timeLeft < num2498)
                                    {
                                        num2499 = num2496;
                                        num2498 = Main.projectile[num2496].timeLeft;
                                    }
                                    num2500++;
                                }
                            }
                            if (num2500 > num2497)
                            {
                                Main.projectile[num2499].Kill();
                            }
                        }
                        WorldGen.KillTile(num2502, num2501, fail: true, effectOnly: true);
                        SoundEngine.PlaySound(SoundID.Dig, new Vector2(num2502, num2501) * 16);
                        Projectile.velocity.X = 0f;
                        Projectile.velocity.Y = 0f;
                        Projectile.ai[0] = 2f;
                        Projectile.position.X = num2502 * 16 + 8 - Projectile.width / 2;
                        Projectile.position.Y = num2501 * 16 + 8 - Projectile.height / 2 - 180;
                        Projectile.damage = 0;
                        Projectile.netUpdate = true;
                        if (Main.myPlayer == Projectile.owner)
                        {
                            NetMessage.SendData(13, -1, -1, null, Projectile.owner);
                        }
                        break;
                    }
                    if (Projectile.ai[0] == 2f)
                    {
                        break;
                    }
                }
            }
            else if (Projectile.ai[0] == 1f)
            {
                float num2494 = 11f;
                ProjectileLoader.GrappleRetreatSpeed(Projectile, Main.player[Projectile.owner], ref num2494);
                if (num2508 < 24f)
                {
                    Projectile.Kill();
                }
                num2508 = num2494 / num2508;
                num2510 *= num2508;
                num2509 *= num2508;
                Projectile.velocity.X = num2510;
                Projectile.velocity.Y = num2509;
            }
            else
            {
                if (Projectile.ai[0] != 2f)
                {
                    return false;
                }
                int num2490 = (int)(Projectile.position.X / 16f) - 1;
                int num2489 = (int)((Projectile.position.X + Projectile.width) / 16f) + 2;
                int num2488 = (int)((Projectile.position.Y + 180) / 16f) - 1;
                int num2487 = (int)(((Projectile.position.Y + 180) + Projectile.height) / 16f) + 2;
                if (num2490 < 0)
                {
                    num2490 = 0;
                }
                if (num2489 > Main.maxTilesX)
                {
                    num2489 = Main.maxTilesX;
                }
                if (num2488 < 0)
                {
                    num2488 = 0;
                }
                if (num2487 > Main.maxTilesY)
                {
                    num2487 = Main.maxTilesY;
                }
                bool flag149 = true;
                Vector2 vector134 = default(Vector2);
                for (int num2486 = num2490; num2486 < num2489; num2486++)
                {
                    for (int num2485 = num2488; num2485 < num2487; num2485++)
                    {
                        vector134.X = num2486 * 16;
                        vector134.Y = num2485 * 16;
                        if (Projectile.position.X + Projectile.width / 2 > vector134.X && Projectile.position.X + Projectile.width / 2 < vector134.X + 16f && (Projectile.position.Y + 180) + Projectile.height / 2 > vector134.Y && (Projectile.position.Y + 180) + Projectile.height / 2 < vector134.Y + 16f && Main.tile[num2486, num2485].HasUnactuatedTile && (Main.tileSolid[Main.tile[num2486, num2485].TileType] || Main.tile[num2486, num2485].TileType == 314 || Main.tile[num2486, num2485].TileType == 5))
                        {
                            flag149 = false;
                        }
                    }
                }
                if (flag149)
                {
                    Projectile.ai[0] = 1f;
                }
                else if (Main.player[Projectile.owner].grapCount < 10)
                {
                    Main.player[Projectile.owner].grappling[Main.player[Projectile.owner].grapCount] = Projectile.whoAmI;
                    Player player24 = Main.player[Projectile.owner];
                    Player player26 = player24;
                    player26.grapCount++;
                }
            }
            return false;
        }

        public override void PostAI()
        {
            Projectile.rotation = 0;
        }

        // Amethyst Hook is 300, Static Hook is 600
        public override float GrappleRange()
        {
            return 800f;
        }

        public override void NumGrappleHooks(Player player, ref int numHooks)
        {
            numHooks = 4;
        }

        // default is 11, Lunar is 24
        public override void GrappleRetreatSpeed(Player player, ref float speed)
        {
            speed = 14f;
        }

        public override void GrapplePullSpeed(Player player, ref float speed)
        {
            speed = 11;
        }

        public override bool PreDraw(ref Color drawColor)
        {
            Vector2 center = Projectile.Center + new Vector2(0, 172);
            Vector2 ownerCenter = Main.player[Projectile.owner].Center;

            Vector2[] points = { center, center + (ownerCenter - center) * 0.34f - new Vector2(0, 200), center + (ownerCenter - center) * 0.66f, ownerCenter };

            for (int i = 0; i < points.Length - 1; i++)
            {
                DrawChain(points[i], points[i + 1], ref drawColor);
            }

            return true;
        }

        public void DrawChain(Vector2 startPoint, Vector2 endPoint, ref Color drawColor)
        {
            Texture2D chainTexture = EsophageClaw.ChainTexture.Value;
            Rectangle chainFrame = EsophageClaw.ChainTexture.Frame();
            Vector2 chainOrigin = chainFrame.Size() / 2;

            int stepSize = chainFrame.Width;

            int parity = 1;

            for (int i = 0; i < (endPoint - startPoint).Length() / stepSize; i++)
            {
                parity *= -1;

                Vector2 drawingPos = startPoint + (endPoint - startPoint).SafeNormalize(Vector2.Zero) * i * stepSize;
                Main.EntitySpriteDraw(chainTexture, drawingPos - Main.screenPosition, chainFrame, Lighting.GetColor(drawingPos.ToTileCoordinates()), (endPoint - startPoint).ToRotation(), chainOrigin, Projectile.scale, Projectile.spriteDirection * parity == -1 ? SpriteEffects.None : SpriteEffects.FlipVertically, 0);
            }
        }

        public override bool PreDrawExtras()
        {
            return false;
        }
    }
}
