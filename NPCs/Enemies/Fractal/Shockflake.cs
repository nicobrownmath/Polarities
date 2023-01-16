using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.NPCs.Enemies.Fractal
{
    public class Shockflake : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;

            NPCID.Sets.TrailCacheLength[NPC.type] = 5;
            NPCID.Sets.TrailingMode[NPC.type] = 0;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 62;
            NPC.height = 62;
            DrawOffsetY = 5;

            NPC.defense = 0;
            NPC.damage = 50;
            NPC.lifeMax = 400;
            NPC.knockBackResist = 0.01f;
            NPC.npcSlots = 1f;
            NPC.HitSound = SoundID.NPCHit36;
            NPC.DeathSound = SoundID.NPCDeath39;
            NPC.value = 750;
            NPC.noTileCollide = true;
            NPC.noGravity = true;

            //Banner = NPC.type;
            //BannerItem = ItemType<ShockflakeBanner>();
        }

        public override void AI()
        {
            switch (NPC.ai[0])
            {
                case 0:
                    //spin around while chasing the player

                    NPC.TargetClosest(true);

                    Player player = Main.player[NPC.target];

                    Vector2 goalVelocity = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 20;
                    NPC.velocity += (goalVelocity - NPC.velocity) / 90;

                    NPC.rotation += NPC.velocity.X * 0.05f;

                    NPC.ai[1]++;
                    if (NPC.ai[1] == 120)
                    {
                        if (Main.netMode != 1)
                        {
                            NPC.ai[0] = Main.rand.Next(new int[] { 0, 1, 2, 3 });
                        }
                        NPC.netUpdate = true;
                        NPC.ai[1] = 0;
                    }
                    break;
                case 1:
                    //spin in place, accelerating spinning before suddenly dashing at the player

                    if (NPC.ai[1] < 90)
                    {
                        if (NPC.ai[1] == 0)
                        {
                            NPC.TargetClosest(true);
                        }

                        NPC.rotation += NPC.direction * NPC.ai[1] * 0.005f;
                        NPC.velocity *= 0.93f;
                    }
                    else if (NPC.ai[1] == 90)
                    {
                        player = Main.player[NPC.target];

                        NPC.velocity = NPC.DirectionTo(player.Center) * 16f;

                        for (int i = 0; i < 6; i++)
                        {
                            for (int j = -5; j < 5; j++)
                            {
                                Dust.NewDustPerfect(NPC.Center + new Vector2((float)Math.Sqrt(3) / 2, j / 10f).RotatedBy(NPC.rotation + MathHelper.TwoPi * i / 6f) * 24f, DustID.Electric, Velocity: new Vector2((float)Math.Sqrt(3) / 2, j / 10f).RotatedBy(NPC.rotation + MathHelper.TwoPi * i / 6f) * 1f, Scale: 1f).noGravity = true;
                            }
                        }
                    }

                    NPC.ai[1]++;
                    if (NPC.ai[1] == 120)
                    {
                        if (Main.netMode != 1)
                        {
                            NPC.ai[0] = Main.rand.Next(new int[] { 0, 1, 2, 3 });
                        }
                        NPC.netUpdate = true;
                        NPC.ai[1] = 0;
                    }
                    break;
                case 2:
                    //spin around the player before stopping, waiting a very short time, and releasing six bolts

                    if (NPC.ai[1] < 180)
                    {
                        if (NPC.ai[1] == 0)
                        {
                            NPC.TargetClosest(true);
                        }

                        player = Main.player[NPC.target];

                        goalVelocity = NPC.DirectionTo(player.Center).RotatedBy(NPC.direction * MathHelper.Pi / 3) * 20f;
                        NPC.velocity += (goalVelocity - NPC.velocity) / 10;

                        NPC.rotation += NPC.direction * NPC.ai[1] * 0.005f;
                    }
                    else
                    {
                        NPC.velocity *= 0.93f;
                    }
                    if (NPC.ai[1] == 210)
                    {
                        SoundEngine.PlaySound(SoundID.Item33, NPC.Center);
                        if (Main.netMode != 1)
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 12).RotatedBy(NPC.rotation + i * MathHelper.TwoPi / 6f), ProjectileType<ShockflakeBolt>(), 25, 3f, Main.myPlayer);
                            }
                        }
                    }

                    NPC.ai[1]++;
                    if (NPC.ai[1] == 240)
                    {
                        if (Main.netMode != 1)
                        {
                            NPC.ai[0] = Main.rand.Next(new int[] { 0, 1 });//, 2, 3 });
                        }
                        NPC.netUpdate = true;
                        NPC.ai[1] = 0;
                    }
                    break;
                case 3:
                    //spin in place, releasing three waves of six bolts
                    if (NPC.ai[1] < 210)
                    {
                        NPC.velocity *= 0.93f;

                        NPC.rotation += 25 * MathHelper.TwoPi / 12f / 60f;

                        if (NPC.ai[1] > 0 && NPC.ai[1] % 60 == 0)
                        {
                            SoundEngine.PlaySound(SoundID.Item33, NPC.Center);
                            if (Main.netMode != 1)
                            {
                                for (int i = 0; i < 6; i++)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 12).RotatedBy(NPC.rotation + i * MathHelper.TwoPi / 6f), ProjectileType<ShockflakeBolt>(), 20, 3f, Main.myPlayer);
                                }
                            }
                        }
                    }


                    NPC.ai[1]++;
                    if (NPC.ai[1] == 210)
                    {
                        if (Main.netMode != 1)
                        {
                            NPC.ai[0] = Main.rand.Next(new int[] { 0, 1 });//, 2, 3 });
                        }
                        NPC.netUpdate = true;
                        NPC.ai[1] = 0;
                    }
                    break;
            }
        }

        public override bool CheckDead()
        {
            for (int i = 0; i < 6; i++)
            {
                for (int j = -5; j < 5; j++)
                {
                    Dust.NewDustPerfect(NPC.Center + new Vector2((float)Math.Sqrt(3) / 2, j / 10f).RotatedBy(NPC.rotation + MathHelper.TwoPi * i / 6f) * 24f, DustID.Electric, Velocity: new Vector2((float)Math.Sqrt(3) / 2, j / 10f).RotatedBy(NPC.rotation + MathHelper.TwoPi * i / 6f) * 1f, Scale: 1f).noGravity = true;
                }
            }

            return true;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            //if (Subworld.IsActive<FractalSubworld>())
            //{
            //    return 0.3f * FractalSubworld.SpawnConditionFractalSky(spawnInfo);
            //}
            return 0f;
        }

        public override void OnKill()
        {
            //if (Main.rand.NextBool(4))
            //{
            //    Item.NewItem(NPC.Hitbox, ItemType<FractalResidue>());
            //}
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Vector2 drawOrigin = new Vector2(TextureAssets.Npc[NPC.type].Value.Width * 0.5f, TextureAssets.Npc[NPC.type].Value.Height * 0.5f);
            Vector2 drawPos = NPC.Center - Main.screenPosition;
            Color color = Color.White;

            for (int k = 0; k < NPC.oldPos.Length; k++)
            {
                drawPos = NPC.Center - NPC.position + NPC.oldPos[k] - Main.screenPosition;
                color = Color.White * ((float)(NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                if (NPC.spriteDirection == -1)
                {
                    spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
                }
                else
                {
                    spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.FlipHorizontally, 0f);
                }
            }

            drawPos = NPC.Center - Main.screenPosition;
            color = Color.White;
            if (NPC.spriteDirection == -1)
            {
                spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
            }
            else
            {
                spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.FlipHorizontally, 0f);
            }
            return false;
        }
    }

    public class ShockflakeBolt : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.light = 1;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, newColor: Color.LightBlue, Scale: 1f)].noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Color mainColor = Color.White;

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), mainColor, Projectile.rotation, new Vector2(5, 5), Projectile.scale, SpriteEffects.None, 0f);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                float scale = Projectile.scale * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

                float rotation;
                if (k + 1 >= Projectile.oldPos.Length)
                {
                    rotation = (Projectile.oldPos[k] - Projectile.position).ToRotation() + MathHelper.PiOver2;
                }
                else
                {
                    rotation = (Projectile.oldPos[k] - Projectile.oldPos[k + 1]).ToRotation() + MathHelper.PiOver2;
                }

                Main.spriteBatch.Draw(texture, Projectile.Center - Projectile.position + (Projectile.oldPos[k] + Projectile.position) / 2 - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), color, rotation, new Vector2(5, 5), scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}