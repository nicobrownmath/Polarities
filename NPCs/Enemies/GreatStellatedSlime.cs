using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Polarities.Projectiles;
using Polarities.Buffs;
using Polarities.Items;
using Polarities.Items.Placeable;
using Polarities.Items.Weapons;
using Polarities.Items.Armor;
using Polarities.Items.Placeable.Banners;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.Audio;

namespace Polarities.NPCs.Enemies
{
    public class GreatStellatedSlime : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 2;

            PolaritiesNPC.customSlimes.Add(Type);
        }

        private bool hasUsedShotgun;
        private int shotTimer;

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                //spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				//flavor text
				this.TranslatedBestiaryEntry()
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = 1;
            NPC.width = 64;
            NPC.height = 48;
            NPC.defense = 0;
            NPC.damage = 70;
            NPC.lifeMax = 1000;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 1f;
            NPC.noGravity = false;
            NPC.HitSound = SoundID.Item9;
            NPC.DeathSound = SoundID.Item10;
            NPC.value = Item.buyPrice(0, 5, 0, 0);

            NPC.rarity = 1;

            NPC.alpha = 96;

            Banner = NPCType<StellatedSlime>();
            BannerItem = ItemType<StellatedSlimeBanner>();
        }

        public override bool PreAI()
        {
            NPC.noGravity = true;
            if (NPC.ai[2] > 1f)
            {
                NPC.ai[2] -= 1f;
            }
            NPC.aiAction = 0;
            if (NPC.ai[2] == 0f)
            {
                NPC.ai[0] = -100f;
                NPC.ai[2] = 1f;
                NPC.TargetClosest();
            }
            int midjumpState = 0;
            if (NPC.ai[0] < 0f && NPC.ai[0] > -500f)
            {
                midjumpState = 3;
            }
            if (NPC.ai[0] < -1000f && NPC.ai[0] > -1500f)
            {
                midjumpState = 1;
            }
            if (NPC.ai[0] < -2000f && NPC.ai[0] > -2500f)
            {
                midjumpState = 2;
            }

            Player player = Main.player[NPC.target];

            if (player.active)
            {
                if (midjumpState == 1 || midjumpState == 2)
                {
                    if (NPC.velocity.Y < 0)
                    {
                        shotTimer++;
                        if (shotTimer == 5)
                        {
                            if (!player.npcTypeNoAggro[Type]) Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 16, ProjectileType<GreatStellatedSlimeProjectile>(), 25, 0f, Main.myPlayer);
                            shotTimer = 0;
                        }
                    }
                }
                else if (midjumpState == 3)
                {
                    if (!hasUsedShotgun)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Vector2 value34 = new Vector2((float)Main.screenWidth, (float)Main.screenHeight);
                            if (NPC.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + value34 / 2f, value34 + new Vector2(400f))) && Main.rand.Next(6) == 0)
                            {
                                int[] array6 = new int[4] { 16, 17, 17, 17 };
                                int num855 = Utils.SelectRandom(Main.rand, array6);
                                if (Main.tenthAnniversaryWorld)
                                {
                                    int[] array7 = new int[4] { 16, 16, 16, 17 };
                                    num855 = Utils.SelectRandom(Main.rand, array7);
                                }
                                Gore.NewGore(NPC.GetSource_FromAI(), NPC.position, NPC.velocity * 0.2f, num855);
                            }
                            if (Main.rand.Next(20) == 0 || (Main.tenthAnniversaryWorld && Main.rand.Next(15) == 0))
                            {
                                Dust.NewDust(NPC.position, NPC.width, NPC.height, 58, NPC.velocity.X * 0.5f, NPC.velocity.Y * 0.5f, 150, default(Color), 1.2f);
                            }
                        }
                    }
                    if (NPC.velocity.Y > 0 && !hasUsedShotgun)
                    {
                        hasUsedShotgun = true;
                        if (!player.npcTypeNoAggro[Type])
                        {
                            if (Main.netMode != 1)
                            {
                                for (int i = 0; i < 8; i++)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (player.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedByRandom(0.25f) * Main.rand.NextFloat(12f, 20f), ProjectileType<GreatStellatedSlimeProjectile>(), 25, 0f, Main.myPlayer);
                                }
                            }
                        }
                    }
                }
            }

            if (NPC.velocity.Y == 0f)
            {
                if (NPC.collideY && NPC.oldVelocity.Y != 0f && Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                {
                    NPC.position.X -= NPC.velocity.X + (float)NPC.direction;
                }
                if (NPC.ai[3] == NPC.position.X)
                {
                    NPC.direction *= -1;
                    NPC.ai[2] = 200f;
                }
                NPC.ai[3] = 0f;
                NPC.velocity.X *= 0.8f;
                if ((double)NPC.velocity.X > -0.1 && (double)NPC.velocity.X < 0.1)
                {
                    NPC.velocity.X = 0f;
                }
                NPC.ai[0] += 2f;
                int jumpState = 0;
                if (NPC.ai[0] >= 0f)
                {
                    jumpState = 1;
                }
                if (NPC.ai[0] >= -1000f && NPC.ai[0] <= -500f)
                {
                    jumpState = 2;
                }
                if (NPC.ai[0] >= -2000f && NPC.ai[0] <= -1500f)
                {
                    jumpState = 3;
                }
                if (jumpState > 0)
                {
                    NPC.netUpdate = true;
                    if (NPC.ai[2] == 1f)
                    {
                        NPC.TargetClosest();
                    }
                    if (jumpState == 3)
                    {
                        hasUsedShotgun = false;

                        NPC.velocity.Y = -32f;
                        NPC.velocity.X += (float)(3 * NPC.direction);
                        NPC.ai[0] = -200f;
                        NPC.ai[3] = NPC.position.X;
                    }
                    else
                    {
                        shotTimer = 0;

                        NPC.velocity.Y = -24f;
                        NPC.velocity.X += (float)(3 * NPC.direction);
                        NPC.ai[0] = -120f;
                        if (jumpState == 1)
                        {
                            NPC.ai[0] -= 1000f;
                        }
                        else
                        {
                            NPC.ai[0] -= 2000f;
                        }
                    }
                }
                else if (NPC.ai[0] >= -30f)
                {
                    NPC.aiAction = 1;
                }
            }
            else if (NPC.target < 255 && ((NPC.direction == 1 && NPC.velocity.X < 3f) || (NPC.direction == -1 && NPC.velocity.X > -3f)))
            {
                if (NPC.collideX && Math.Abs(NPC.velocity.X) == 0.2f)
                {
                    NPC.position.X -= 1.4f * (float)NPC.direction;
                }
                if (NPC.collideY && NPC.oldVelocity.Y != 0f && Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                {
                    NPC.position.X -= NPC.velocity.X + (float)NPC.direction;
                }
                if ((NPC.direction == -1 && (double)NPC.velocity.X < 0.01) || (NPC.direction == 1 && (double)NPC.velocity.X > -0.01))
                {
                    NPC.velocity.X += 0.2f * (float)NPC.direction;
                }
                else
                {
                    NPC.velocity.X *= 0.93f;
                }
            }

            NPC.velocity.Y += 1f;

            if (Main.dayTime)
            {
                //spawn dusts and despawn
                for (int i = 0; i < 30; i++)
                {
                    Vector2 position30 = NPC.position;
                    int width27 = NPC.width;
                    int height27 = NPC.height;
                    float speedX13 = NPC.velocity.X * 0.5f;
                    float speedY13 = NPC.velocity.Y * 0.5f;
                    Color newColor = default(Color);
                    Dust.NewDust(position30, width27, height27, 58, speedX13, speedY13, 150, newColor, 1.2f);
                }
                for (int i = 0; i < 15; i++)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center + new Vector2(-8, NPC.height / 2), new Vector2(NPC.velocity.X * 0.2f, NPC.velocity.Y * 0.2f), Main.rand.Next(16, 18));
                }

                NPC.active = false;
            }

            if (NPC.velocity.Y <= 0 && Collision.TileCollision(NPC.position, new Vector2(0, -16), NPC.width, NPC.height) != new Vector2(0, -16))
            {
                NPC.velocity.Y = Collision.TileCollision(NPC.position, new Vector2(0, -16), NPC.width, NPC.height).Y;
            }
            else if (NPC.velocity.Y <= -16 && Collision.TileCollision(NPC.position + new Vector2(0, -16), new Vector2(0, -16), NPC.width, NPC.height + 16) != new Vector2(0, -16))
            {
                NPC.velocity.Y = Collision.TileCollision(NPC.position + new Vector2(0, -16), new Vector2(0, -16), NPC.width, NPC.height + 16).Y - 16;
            }
            else if (NPC.velocity.Y >= 0 && Collision.TileCollision(NPC.position, new Vector2(0, 16), NPC.width, NPC.height) != new Vector2(0, 16))
            {
                NPC.velocity.Y = Collision.TileCollision(NPC.position, new Vector2(0, 16), NPC.width, NPC.height).Y;
            }
            else if (NPC.velocity.Y >= 16 && Collision.TileCollision(NPC.position, new Vector2(0, 16), NPC.width, NPC.height + 16) != new Vector2(0, 16))
            {
                NPC.velocity.Y = Collision.TileCollision(NPC.position, new Vector2(0, 16), NPC.width, NPC.height + 16).Y + 16;
            }

            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            int num2 = 0;
            if (NPC.aiAction == 0)
            {
                num2 = ((NPC.velocity.Y < 0f) ? 2 : ((NPC.velocity.Y > 0f) ? 3 : ((NPC.velocity.X != 0f) ? 1 : 0)));
            }
            else if (NPC.aiAction == 1)
            {
                num2 = 4;
            }
            NPC.frameCounter++;
            if (num2 > 0)
            {
                NPC.frameCounter++;
            }
            if (num2 == 4)
            {
                NPC.frameCounter++;
            }
            if (NPC.frameCounter >= 8.0)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0.0;
            }
            if (NPC.frame.Y >= frameHeight * Main.npcFrameCount[NPC.type])
            {
                NPC.frame.Y = 0;
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (PolaritiesSystem.downedStarConstruct)
            {
                return Terraria.ModLoader.Utilities.SpawnCondition.OverworldNightMonster.Chance * 0.01f;
            }
            else
            {
                return 0f;
            }
        }

        public override bool CheckDead()
        {
            for (int i = 0; i < 3; i++)
            {
                int slime = NPC.NewNPC(NPC.GetSource_Death(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<StellatedSlime>());
                Main.npc[slime].velocity = new Vector2(0, -8).RotatedByRandom(MathHelper.PiOver2);
            }

            return true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.NormalvsExpert(ItemType<Items.Accessories.StargelAmulet>(), 2, 1));

            //normal mode exclusive
            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.FallenStar, 3));
            npcLoot.Add(notExpertRule);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            int midjumpState = 0;
            if (NPC.ai[0] < 0f && NPC.ai[0] > -500f)
            {
                midjumpState = 3;
            }
            if (NPC.ai[0] < -1000f && NPC.ai[0] > -1500f)
            {
                midjumpState = 1;
            }
            if (NPC.ai[0] < -2000f && NPC.ai[0] > -2500f)
            {
                midjumpState = 2;
            }

            if (midjumpState == 3 && !hasUsedShotgun)
            {
                float scaleMultiplier = 1.5f;

                Texture2D value175 = TextureAssets.Extra[91].Value;
                Rectangle value176 = value175.Frame();
                Vector2 origin10 = new Vector2((float)value176.Width / 2f, 10f);
                Vector2 spinningpoint = new Vector2(0f, -10f);
                float num184 = (float)Main.timeForVisualEffects / 60f;
                Vector2 value178 = NPC.Center + NPC.velocity;
                Color color42 = Color.Blue * 0.2f;
                Color value179 = Color.White * 0.5f;
                value179.A = 0;
                float num185 = 0f;
                if (Main.tenthAnniversaryWorld)
                {
                    color42 = Color.HotPink * 0.3f;
                    value179 = Color.White * 0.75f;
                    value179.A = 0;
                    num185 = -0.1f;
                }
                Color color43 = color42;
                color43.A = 0;
                Color color44 = color42;
                color44.A = 0;
                Color color45 = color42;
                color45.A = 0;
                Vector2 val8 = value178 - screenPos;
                Vector2 spinningpoint17 = spinningpoint;
                double radians6 = (float)Math.PI * 2f * num184;
                Vector2 val2 = default(Vector2);
                Main.EntitySpriteDraw(value175, val8 + spinningpoint17.RotatedBy(radians6, val2), value176, color43, NPC.velocity.ToRotation() + (float)Math.PI / 2f, origin10, (1.5f + num185) * scaleMultiplier, (SpriteEffects)0, 0);
                Vector2 val9 = value178 - Main.screenPosition;
                Vector2 spinningpoint18 = spinningpoint;
                double radians7 = (float)Math.PI * 2f * num184 + (float)Math.PI * 2f / 3f;
                val2 = default(Vector2);
                Main.EntitySpriteDraw(value175, val9 + spinningpoint18.RotatedBy(radians7, val2), value176, color44, NPC.velocity.ToRotation() + (float)Math.PI / 2f, origin10, (1.1f + num185) * scaleMultiplier, (SpriteEffects)0, 0);
                Vector2 val10 = value178 - Main.screenPosition;
                Vector2 spinningpoint19 = spinningpoint;
                double radians8 = (float)Math.PI * 2f * num184 + 4.1887903f;
                val2 = default(Vector2);
                Main.EntitySpriteDraw(value175, val10 + spinningpoint19.RotatedBy(radians8, val2), value176, color45, NPC.velocity.ToRotation() + (float)Math.PI / 2f, origin10, (1.3f + num185) * scaleMultiplier, (SpriteEffects)0, 0);
                Vector2 value180 = NPC.Center - NPC.velocity * 0.5f;
                for (float num186 = 0f; num186 < 1f; num186 += 0.5f)
                {
                    float num187 = num184 % 0.5f / 0.5f;
                    num187 = (num187 + num186) % 1f;
                    float num188 = num187 * 2f;
                    if (num188 > 1f)
                    {
                        num188 = 2f - num188;
                    }
                    Main.EntitySpriteDraw(value175, value180 - screenPos, value176, value179 * num188, NPC.velocity.ToRotation() + (float)Math.PI / 2f, origin10, (0.3f + num187 * 0.5f) * scaleMultiplier, (SpriteEffects)0, 0);
                }
            }

            Texture2D star = TextureAssets.Item[ItemID.FallenStar].Value;
            Rectangle frame = star.Frame(1, 8, 0, (int)(Main.timeForVisualEffects / Main.itemAnimations[ItemID.FallenStar].TicksPerFrame) % 8);
            Vector2 drawOrigin = frame.Size() / 2;
            float scale = NPC.scale * 0.75f;

            for (int i = 0; i < 3; i++)
            {
                Vector2 drawPos = NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY) + new Vector2(0, 4) + new Vector2(0, -6).RotatedBy(i * MathHelper.TwoPi / 3);

                float num10 = (float)Main.timeForVisualEffects / 240f + Main.GlobalTimeWrappedHourly * 0.04f;
                float globalTimeWrappedHourly2 = Main.GlobalTimeWrappedHourly;
                globalTimeWrappedHourly2 %= 5f;
                globalTimeWrappedHourly2 /= 2.5f;
                if (globalTimeWrappedHourly2 >= 1f)
                {
                    globalTimeWrappedHourly2 = 2f - globalTimeWrappedHourly2;
                }
                globalTimeWrappedHourly2 = globalTimeWrappedHourly2 * 0.5f + 0.5f;
                for (float num11 = 0f; num11 < 1f; num11 += 0.25f)
                {
                    spriteBatch.Draw(star, drawPos + Utils.RotatedBy(new Vector2(0f, 8f), (num11 + num10) * ((float)Math.PI * 2f)) * scale * globalTimeWrappedHourly2, frame, new Color(50, 50, 255, 50), 0f, drawOrigin, scale, (SpriteEffects)0, 0f);
                }
                for (float num12 = 0f; num12 < 1f; num12 += 0.34f)
                {
                    spriteBatch.Draw(star, drawPos + Utils.RotatedBy(new Vector2(0f, 4f), (num12 + num10) * ((float)Math.PI * 2f)) * scale * globalTimeWrappedHourly2, frame, new Color(120, 120, 255, 127), 0f, drawOrigin, scale, (SpriteEffects)0, 0f);
                }

                spriteBatch.Draw(star, drawPos, frame, Color.White, NPC.rotation, drawOrigin, NPC.scale * 0.5f, SpriteEffects.None, 0f);
            }

            return true;
        }
    }

    public class GreatStellatedSlimeProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.FallingStar;

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 3600;
            Projectile.tileCollide = true;
            Projectile.light = 0.9f;
            Projectile.scale = 1.2f;
        }

        public override void AI()
        {
            if (Projectile.ai[1] == 0f && !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
            {
                Projectile.ai[1] = 1f;
            }
            if (Projectile.ai[1] != 0f)
            {
                Projectile.tileCollide = true;
            }
            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = 20 + Main.rand.Next(40);
                SoundEngine.PlaySound(SoundID.Item9, Projectile.position);
            }
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
            }
            Projectile.alpha += (int)(25f * Projectile.localAI[0]);
            if (Projectile.alpha > 200)
            {
                Projectile.alpha = 200;
                Projectile.localAI[0] = -1f;
            }
            if (Projectile.alpha < 0)
            {
                Projectile.alpha = 0;
                Projectile.localAI[0] = 1f;
            }
            Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.01f * (float)Projectile.direction;

            Vector2 value34 = new Vector2((float)Main.screenWidth, (float)Main.screenHeight);
            if (Projectile.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + value34 / 2f, value34 + new Vector2(400f))) && Main.rand.NextBool(6))
            {
                int[] array6 = new int[4] { 16, 17, 17, 17 };
                int num855 = Utils.SelectRandom(Main.rand, array6);
                if (Main.tenthAnniversaryWorld)
                {
                    int[] array7 = new int[4] { 16, 16, 16, 17 };
                    num855 = Utils.SelectRandom(Main.rand, array7);
                }
                Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.position, Projectile.velocity * 0.2f, num855);
            }
            Projectile.light = 0.9f;
            if (Main.rand.NextBool(20)|| (Main.tenthAnniversaryWorld && Main.rand.NextBool(15)))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Enchanted_Pink, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 150, default(Color), 1.2f);
            }
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            Color newColor7 = Color.CornflowerBlue;
            if (Main.tenthAnniversaryWorld)
            {
                newColor7 = Color.HotPink;
                newColor7.A = (byte)(newColor7.A / 2);
            }
            for (int num573 = 0; num573 < 7; num573++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Enchanted_Pink, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 150, default(Color), 0.8f);
            }
            for (float num574 = 0f; num574 < 1f; num574 += 0.125f)
            {
                Vector2 center25 = Projectile.Center;
                Vector2 unitY11 = Vector2.UnitY;
                double radians36 = num574 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f;
                Vector2 center2 = default(Vector2);
                Dust.NewDustPerfect(center25, 278, unitY11.RotatedBy(radians36, center2) * (4f + Main.rand.NextFloat() * 4f), 150, newColor7).noGravity = true;
            }
            for (float num575 = 0f; num575 < 1f; num575 += 0.25f)
            {
                Vector2 center26 = Projectile.Center;
                Vector2 unitY12 = Vector2.UnitY;
                double radians37 = num575 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f;
                Vector2 center2 = default(Vector2);
                Dust.NewDustPerfect(center26, 278, unitY12.RotatedBy(radians37, center2) * (2f + Main.rand.NextFloat() * 3f), 150, Color.Gold).noGravity = true;
            }
            Vector2 value15 = new Vector2((float)Main.screenWidth, (float)Main.screenHeight);
            if (Projectile.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + value15 / 2f, value15 + new Vector2(400f))))
            {
                for (int num576 = 0; num576 < 7; num576++)
                {
                    Vector2 val29 = Projectile.position;
                    Vector2 val30 = Main.rand.NextVector2CircularEdge(0.5f, 0.5f) * Projectile.velocity.Length();
                    int[] array18 = new int[8] { 16, 17, 17, 17, 17, 17, 17, 17 };
                    Gore.NewGore(Projectile.GetSource_Death(), val29, val30, Utils.SelectRandom(Main.rand, array18));
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            lightColor = Color.White;

            SpriteEffects spriteEffects = (SpriteEffects)0;
            if (Projectile.spriteDirection == -1)
            {
                spriteEffects = (SpriteEffects)1;
            }

            Texture2D value174 = TextureAssets.Projectile[Type].Value;
            Rectangle rectangle24 = value174.Frame();
            Vector2 origin33 = rectangle24.Size() / 2f;
            Color alpha13 = Projectile.GetAlpha(lightColor);
            Texture2D value175 = TextureAssets.Extra[91].Value;
            Rectangle value176 = value175.Frame();
            Vector2 origin10 = new Vector2((float)value176.Width / 2f, 10f);
            Vector2 value177 = new Vector2(0f, Projectile.gfxOffY);
            Vector2 spinningpoint = new Vector2(0f, -10f);
            float num184 = (float)Main.timeForVisualEffects / 60f;
            Vector2 value178 = Projectile.Center + Projectile.velocity;
            Color color42 = Color.Blue * 0.2f;
            Color value179 = Color.White * 0.5f;
            value179.A = 0;
            float num185 = 0f;
            if (Main.tenthAnniversaryWorld)
            {
                color42 = Color.HotPink * 0.3f;
                value179 = Color.White * 0.75f;
                value179.A = 0;
                num185 = -0.1f;
            }
            Color color43 = color42;
            color43.A = 0;
            Color color44 = color42;
            color44.A = 0;
            Color color45 = color42;
            color45.A = 0;
            Vector2 val8 = value178 - Main.screenPosition + value177;
            Vector2 spinningpoint17 = spinningpoint;
            double radians6 = (float)Math.PI * 2f * num184;
            Vector2 val2 = default(Vector2);
            Main.EntitySpriteDraw(value175, val8 + spinningpoint17.RotatedBy(radians6, val2), value176, color43, Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin10, 1.5f + num185, (SpriteEffects)0, 0);
            Vector2 val9 = value178 - Main.screenPosition + value177;
            Vector2 spinningpoint18 = spinningpoint;
            double radians7 = (float)Math.PI * 2f * num184 + (float)Math.PI * 2f / 3f;
            val2 = default(Vector2);
            Main.EntitySpriteDraw(value175, val9 + spinningpoint18.RotatedBy(radians7, val2), value176, color44, Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin10, 1.1f + num185, (SpriteEffects)0, 0);
            Vector2 val10 = value178 - Main.screenPosition + value177;
            Vector2 spinningpoint19 = spinningpoint;
            double radians8 = (float)Math.PI * 2f * num184 + 4.1887903f;
            val2 = default(Vector2);
            Main.EntitySpriteDraw(value175, val10 + spinningpoint19.RotatedBy(radians8, val2), value176, color45, Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin10, 1.3f + num185, (SpriteEffects)0, 0);
            Vector2 value180 = Projectile.Center - Projectile.velocity * 0.5f;
            for (float num186 = 0f; num186 < 1f; num186 += 0.5f)
            {
                float num187 = num184 % 0.5f / 0.5f;
                num187 = (num187 + num186) % 1f;
                float num188 = num187 * 2f;
                if (num188 > 1f)
                {
                    num188 = 2f - num188;
                }
                Main.EntitySpriteDraw(value175, value180 - Main.screenPosition + value177, value176, value179 * num188, Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin10, 0.3f + num187 * 0.5f, (SpriteEffects)0, 0);
            }
            Main.EntitySpriteDraw(value174, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), rectangle24, alpha13, Projectile.rotation, origin33, Projectile.scale + 0.1f, spriteEffects, 0);

            return false;
        }
    }
}

