using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Polarities.Projectiles;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.DataStructures;
using ReLogic.Content;
using Microsoft.CodeAnalysis;
using static Terraria.ModLoader.PlayerDrawLayer;
using Terraria.Audio;
using static Humanizer.In;

namespace Polarities.Items.Weapons.Melee
{
    public class NeuralBasher : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(5, 8));
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(14, 5.5f, 0);
            Item.DamageType = DamageClass.Melee;

            Item.width = 46;
            Item.height = 46;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = 5;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.channel = true;
            Item.shoot = ProjectileType<NeuralBasherProjectile>();
            Item.shootSpeed = 16f;

            Item.value = Item.sellPrice(gold: 2);
            Item.rare = RarityType<BrainOfCthulhuFlawlessRarity>();
            Item.GetGlobalItem<PolaritiesItem>().flawless = true;
        }
    }

    public class NeuralBasherProjectile : ModProjectile
    {
        private int numChildren = 0;
        private int parent = -1;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead || player.noItems || player.CCed || Vector2.Distance(Projectile.Center, player.Center) > 900f)
            {
                Projectile.Kill();
                return;
            }
            if (Main.myPlayer == Projectile.owner && Main.mapFullscreen)
            {
                Projectile.Kill();
                return;
            }
            Vector2 mountedCenter = player.MountedCenter;
            if (parent != -1)
            {
                if (Main.projectile[parent].active)
                {
                    mountedCenter = Main.projectile[parent].Center;
                }
                else
                {
                    parent = -1;
                }
            }
            bool attachToPlayer = parent == -1;

            bool doFastThrowDust = false;
            bool flag = true;
            bool flag2 = false;
            int num = 10;
            float num12 = 24f;
            float num15 = 800f;
            float num16 = 3f;
            float num17 = 16f;
            float num18 = 6f;
            float num19 = 48f;
            float num20 = 1f;
            float num21 = 14f;
            int num2 = 60;
            int num3 = 10;
            int num4 = 20;
            int num5 = 10;
            int num6 = num + 5;
            float num7 = 1f / player.GetTotalAttackSpeed(DamageClass.Melee);
            num12 *= num7;
            num20 *= num7;
            num21 *= num7;
            num16 *= num7;
            num17 *= num7;
            num18 *= num7;
            num19 *= num7;
            float num8 = num12 * (float)num;
            float num9 = num8 + 160f;
            Projectile.localNPCHitCooldown = num3;
            switch ((int)Projectile.ai[0])
            {
                case 0:
                    {
                        flag2 = true;
                        if (Projectile.owner == Main.myPlayer)
                        {
                            Vector2 mouseWorld = Main.MouseWorld;
                            Vector2 value3 = mountedCenter.DirectionTo(mouseWorld).SafeNormalize(Vector2.UnitX * (float)player.direction);
                            player.ChangeDir((value3.X > 0f) ? 1 : (-1));
                            if (!player.channel)
                            {
                                Projectile.ai[0] = 1f;
                                Projectile.ai[1] = 0f;
                                Projectile.velocity = value3 * num12 + player.velocity;
                                Projectile.Center = mountedCenter;
                                Projectile.netUpdate = true;
                                Projectile.ResetLocalNPCHitImmunity();
                                Projectile.localNPCHitCooldown = num5;
                                break;
                            }
                        }
                        Projectile.localAI[1] += 1f;
                        Vector2 value4 = Utils.RotatedBy(new Vector2((float)player.direction), (float)Math.PI * 10f * (Projectile.localAI[1] / 60f) * (float)player.direction);
                        value4.Y *= 0.8f;
                        if (value4.Y * player.gravDir > 0f)
                        {
                            value4.Y *= 0.5f;
                        }
                        Projectile.Center = mountedCenter + value4 * 30f;
                        Projectile.velocity = Vector2.Zero;
                        Projectile.localNPCHitCooldown = num4;
                        break;
                    }
                case 1:
                    {
                        doFastThrowDust = true;
                        bool flag4 = Projectile.ai[1]++ >= (float)num;
                        flag4 |= Projectile.Distance(mountedCenter) >= num15;
                        if (player.controlUseItem && attachToPlayer)
                        {
                            Projectile.ai[0] = 6f;
                            Projectile.ai[1] = 0f;
                            Projectile.netUpdate = true;
                            Projectile.velocity *= 0.2f;
                            break;
                        }
                        if (flag4)
                        {
                            Projectile.ai[0] = 2f;
                            Projectile.ai[1] = 0f;
                            Projectile.netUpdate = true;
                            Projectile.velocity *= 0.3f;
                        }
                        player.ChangeDir((player.Center.X < Projectile.Center.X) ? 1 : (-1));
                        Projectile.localNPCHitCooldown = num5;
                        break;
                    }
                case 2:
                    {
                        Vector2 value2 = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
                        if (Projectile.Distance(mountedCenter) <= num17)
                        {
                            Projectile.Kill();
                            return;
                        }
                        if (player.controlUseItem && attachToPlayer)
                        {
                            Projectile.ai[0] = 6f;
                            Projectile.ai[1] = 0f;
                            Projectile.netUpdate = true;
                            Projectile.velocity *= 0.2f;
                        }
                        else
                        {
                            Projectile.velocity *= 0.98f;
                            Projectile.velocity = Projectile.velocity.MoveTowards(value2 * num17, num16);
                            player.ChangeDir((player.Center.X < Projectile.Center.X) ? 1 : (-1));
                        }
                        break;
                    }
                case 3:
                    {
                        if (!player.controlUseItem && attachToPlayer)
                        {
                            Projectile.ai[0] = 4f;
                            Projectile.ai[1] = 0f;
                            Projectile.netUpdate = true;
                            break;
                        }
                        float num10 = Projectile.Distance(mountedCenter);
                        Projectile.tileCollide = Projectile.ai[1] == 1f;
                        bool flag3 = num10 <= num8;
                        if (flag3 != Projectile.tileCollide)
                        {
                            Projectile.tileCollide = flag3;
                            Projectile.ai[1] = (Projectile.tileCollide ? 1 : 0);
                            Projectile.netUpdate = true;
                        }
                        if (num10 > (float)num2)
                        {
                            if (num10 >= num8)
                            {
                                Projectile.velocity *= 0.5f;
                                Projectile.velocity = Projectile.velocity.MoveTowards(Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero) * num21, num21);
                            }
                            Projectile.velocity *= 0.98f;
                            Projectile.velocity = Projectile.velocity.MoveTowards(Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero) * num21, num20);
                        }
                        else
                        {
                            if (Projectile.velocity.Length() < 6f)
                            {
                                Projectile.velocity.X *= 0.96f;
                                Projectile.velocity.Y += 0.2f;
                            }
                            if (player.velocity.X == 0f)
                            {
                                Projectile.velocity.X *= 0.96f;
                            }
                        }
                        player.ChangeDir((player.Center.X < Projectile.Center.X) ? 1 : (-1));
                        break;
                    }
                case 4:
                    {
                        Projectile.tileCollide = false;
                        Vector2 vector = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
                        if (Projectile.Distance(mountedCenter) <= num19)
                        {
                            Projectile.Kill();
                            return;
                        }
                        Projectile.velocity *= 0.98f;
                        Projectile.velocity = Projectile.velocity.MoveTowards(vector * num19, num18);
                        Vector2 target = Projectile.Center + Projectile.velocity;
                        Vector2 value = mountedCenter.DirectionFrom(target).SafeNormalize(Vector2.Zero);
                        if (Vector2.Dot(vector, value) < 0f)
                        {
                            Projectile.Kill();
                            return;
                        }
                        player.ChangeDir((player.Center.X < Projectile.Center.X) ? 1 : (-1));
                        break;
                    }
                case 5:
                    if (Projectile.ai[1]++ >= (float)num6 && attachToPlayer)
                    {
                        Projectile.ai[0] = 6f;
                        Projectile.ai[1] = 0f;
                        Projectile.netUpdate = true;
                    }
                    else
                    {
                        Projectile.localNPCHitCooldown = num5;
                        Projectile.velocity.Y += 0.6f;
                        Projectile.velocity.X *= 0.95f;
                        player.ChangeDir((player.Center.X < Projectile.Center.X) ? 1 : (-1));
                    }
                    break;
                case 6:
                    if ((!player.controlUseItem || Projectile.Distance(mountedCenter) > num9) && attachToPlayer)
                    {
                        Projectile.ai[0] = 4f;
                        Projectile.ai[1] = 0f;
                        Projectile.netUpdate = true;
                    }
                    else
                    {
                        Projectile.velocity.Y += 0.8f;
                        Projectile.velocity.X *= 0.95f;
                        player.ChangeDir((player.Center.X < Projectile.Center.X) ? 1 : (-1));
                    }
                    break;
            }
            Projectile.direction = ((Projectile.velocity.X > 0f) ? 1 : (-1));
            Projectile.spriteDirection = Projectile.direction;
            Projectile.ownerHitCheck = flag2;
            if (flag)
            {
                if (Projectile.velocity.Length() > 1f)
                {
                    Projectile.rotation = Projectile.velocity.ToRotation() + Projectile.velocity.X * 0.1f;
                }
                else
                {
                    Projectile.rotation += Projectile.velocity.X * 0.1f;
                }
            }

            Projectile.timeLeft = Math.Max(2, Projectile.timeLeft);
            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);
            player.itemRotation = Projectile.DirectionFrom(mountedCenter).ToRotation();
            if (Projectile.Center.X < mountedCenter.X)
            {
                player.itemRotation += (float)Math.PI;
            }
            player.itemRotation = MathHelper.WrapAngle(player.itemRotation);

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 4;
            }
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = 24;
            height = 24;
            return Projectile.ai[0] != 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            parent = (int)Projectile.ai[1] - 1;
            Projectile.ai[1] = 0;

            if (parent != -1) Projectile.localAI[1] = Main.rand.NextFloat(60f);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (numChildren < 3 && Main.player[Projectile.owner].ownedProjectileCounts[Projectile.type] < 13)
            {
                Projectile.NewProjectile(Projectile.GetSource_OnHit(target), Projectile.Center, (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero) * 16, Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner, ai1: Projectile.whoAmI + 1);
                numChildren++;
            }
            Projectile.timeLeft = 240;
        }

        public override void Kill(int timeLeft)
        {
            if (parent != -1 && Main.projectile[parent].active && Main.projectile[parent].type == Projectile.type)
            {
                (Main.projectile[parent].ModProjectile as NeuralBasherProjectile).numChildren--;
            }

            for (int i = 0; i < 16; i++)
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.t_Flesh, 0, 0, 64, Scale: 1)].noGravity = true;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            int num = 10;
            int num2 = 0;
            Vector2 velocity = Projectile.velocity;
            float num3 = 0.2f;
            if (Projectile.ai[0] == 1f || Projectile.ai[0] == 5f)
            {
                num3 = 0.4f;
            }
            if (Projectile.ai[0] == 6f)
            {
                num3 = 0f;
            }
            if (oldVelocity.X != Projectile.velocity.X)
            {
                if (Math.Abs(oldVelocity.X) > 4f)
                {
                    num2 = 1;
                }
                Projectile.velocity.X = (0f - oldVelocity.X) * num3;
                Projectile.localAI[0] += 1f;
            }
            if (oldVelocity.Y != Projectile.velocity.Y)
            {
                if (Math.Abs(oldVelocity.Y) > 4f)
                {
                    num2 = 1;
                }
                Projectile.velocity.Y = (0f - oldVelocity.Y) * num3;
                Projectile.localAI[0] += 1f;
            }
            if (Projectile.ai[0] == 1f && parent == -1)
            {
                Projectile.ai[0] = 5f;
                Projectile.localNPCHitCooldown = num;
                Projectile.netUpdate = true;
                Point scanAreaStart = Projectile.TopLeft.ToTileCoordinates();
                Point scanAreaEnd = Projectile.BottomRight.ToTileCoordinates();
                num2 = 2;
                Projectile.CreateImpactExplosion(2, Projectile.Center, ref scanAreaStart, ref scanAreaEnd, Projectile.width, out var causedShockwaves);
                Projectile.CreateImpactExplosion2_FlailTileCollision(Projectile.Center, causedShockwaves, velocity);
                Projectile.position -= velocity;
            }
            if (num2 > 0)
            {
                Projectile.netUpdate = true;
                for (int i = 0; i < num2; i++)
                {
                    Collision.HitTiles(Projectile.position, velocity, Projectile.width, Projectile.height);
                }
                SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            }
            if (Projectile.ai[0] != 3f && Projectile.ai[0] != 0f && Projectile.ai[0] != 5f && Projectile.ai[0] != 6f && Projectile.localAI[0] >= 10f && parent == -1)
            {
                Projectile.ai[0] = 4f;
                Projectile.netUpdate = true;
            }
            return false;
        }

        public static Asset<Texture2D> ChainTexture;

        public override void Load()
        {
            ChainTexture = Request<Texture2D>(Texture + "_Chain");
        }

        public override void Unload()
        {
            ChainTexture = null;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 playerCenter = Main.player[Projectile.owner].MountedCenter;
            if (parent != -1)
            {
                playerCenter = Main.projectile[parent].Center;
            }

            Vector2 center = Projectile.Center;
            Vector2 distToProj = playerCenter - Projectile.Center;
            float projRotation = distToProj.ToRotation();
            float distance = distToProj.Length();
            while (distance > 16f && !float.IsNaN(distance))
            {
                distToProj.Normalize();                 //get unit vector
                distToProj *= 16f;                      //speed = 24
                center += distToProj;                   //update draw position
                distToProj = playerCenter - center;    //update distance
                distance = distToProj.Length();
                Color drawColor = lightColor;

                //Draw chain
                Main.EntitySpriteDraw(ChainTexture.Value, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                    new Rectangle(0, 20 * 3, 14, 20), drawColor, projRotation + MathHelper.PiOver2,
                    new Vector2(14 * 0.5f, 20 * 0.5f), 1f, SpriteEffects.None, 0);
            }
            return true;
        }
    }
}