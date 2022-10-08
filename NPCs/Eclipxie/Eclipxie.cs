using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Polarities.Buffs;
using Polarities.Items;
using Polarities.Items.Placeable;
using Polarities.Items.Weapons;
using Polarities.Items.Armor;
using System.Collections.Generic;
using Terraria.Utilities;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.DataStructures;
using ReLogic.Content;
using Polarities;
using Terraria.Localization;
using Polarities.Effects;
using Polarities.Projectiles;
using Polarities.Items.Weapons.Ranged;
using Terraria.Graphics.Shaders;
using Terraria.Graphics.Effects;
using Filters = Terraria.Graphics.Effects.Filters;
using static tModPorter.ProgressUpdate;

namespace Polarities.NPCs.Eclipxie
{
    [AutoloadBossHead]
    public class Eclipxie : ModNPC
    {
        public static Asset<Texture2D> MoonTexture;
        public static Asset<Texture2D> FireGradient;

        public override void Load()
        {
            MoonTexture = Request<Texture2D>(Texture + "_Moon");
            FireGradient = Request<Texture2D>(Texture + "_FireGradient");

            /*IL.Terraria.Main.UpdateMenu += Main_UpdateMenu;
		}

        private void Main_UpdateMenu(MonoMod.Cil.ILContext il)
        {
            MonoMod.Cil.ILCursor c = new MonoMod.Cil.ILCursor(il);

			c.EmitDelegate<Action>(() =>
			{
				if (!(bool)(typeof(ModLoader).GetField("isLoading", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null)))
				{
					String filePath = Main.SavePath + Path.DirectorySeparatorChar + "ModSources/Polarities/NPCs/Eclipxie/Eclipxie_FireGradient.png";

					if (!System.IO.File.Exists(filePath))
					{
						const int textureSize = 64;

						Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, textureSize, 1, false, SurfaceFormat.Color);
						System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
						for (int i = 0; i < texture.Width; i++)
						{
							float x = i / (float)(texture.Width - 1);

                            Color baseColor = ModUtils.ConvectiveFlameColor((1 - x) * 0.5f);
                            float baseAlpha = 1 - (float)Math.Pow(x, 8);

							list.Add(baseColor * baseAlpha);
						}
						texture.SetData(list.ToArray());
						texture.SaveAsPng(new System.IO.FileStream(filePath, System.IO.FileMode.Create), texture.Width, texture.Height);
					}
				}
			});*/
        }

        public override void Unload()
        {
            MoonTexture = null;
            FireGradient = null;
        }

        public override void SetStaticDefaults()
        {
            //group with other bosses
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
            {
                SpecificallyImmuneTo = new int[] {
                    BuffID.Frostburn,
                    BuffID.OnFire,
                    BuffID.Confused
                }
            };
            NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);

            Main.npcFrameCount[NPC.type] = 8;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                //spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.Eclipse,
				//flavor text
				this.TranslatedBestiaryEntry()
            });
        }

        public const int ProjectileDamage = 35;

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 64;
            NPC.height = 64;

            NPC.defense = 45;
            NPC.damage = 70;
            NPC.lifeMax = Main.masterMode ? 1000000 / 3 : Main.expertMode ? 864000 / 2 : 600000;

            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(gold: 15);
            NPC.npcSlots = 15f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit5;

            NPC.hide = true;

            Music = MusicID.EmpressOfLight;
            //TODO: Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Eclipxie");
        }
        public static void SpawnOn(Player player)
        {
            NPC pixie = Main.npc[NPC.NewNPC(NPC.GetBossSpawnSource(player.whoAmI), (int)player.Center.X, (int)player.Center.Y - 300, ModContent.NPCType<Eclipxie>())];
            Main.NewText(Language.GetTextValue("Announcement.HasAwoken", pixie.TypeName), 171, 64, 255);
            SoundEngine.PlaySound(SoundID.Item29, player.position);
        }

        public override void AI()
        {
            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
                if (player.dead)
                {
                    if (NPC.timeLeft > 10)
                    {
                        NPC.timeLeft = 10;
                    }
                    //TODO: Despawning
                }
            }

            NPC.dontTakeDamage = false;

            bool gotoNextAttack = false;

            switch (NPC.ai[0])
            {
                #region Spawn Animation
                case 0:
                    {
                        NPC.dontTakeDamage = true;

                        if (NPC.ai[1] == 0)
                        {
                            ParticleLayer.WarpParticles.Add(Particle.NewParticle<WarpZoomPulseParticle>(NPC.Center, Vector2.Zero, MathHelper.Pi, 0, Scale: 0f));
                            ParticleLayer.WarpParticles.Add(Particle.NewParticle<WarpZoomPulseParticle>(NPC.Center, Vector2.Zero, MathHelper.Pi, 0, Scale: 0f, TimeLeft: 120));
                        }

                        if (NPC.ai[1] == 110f)
                        {
                            for (int i = 1; i <= 3; i++)
                            {
                                WarpZoomWaveParticle particle = Particle.NewParticle<WarpZoomWaveParticle>(NPC.Center, Vector2.Zero, MathHelper.Pi, 0, Scale: 0f, TimeLeft: 240 / i);
                                particle.AlphaMultiplier *= (float)Math.Pow((NPC.ai[1] + 1) / 100f, 4);
                                ParticleLayer.WarpParticles.Add(particle);
                            }
                        }

                        NPC.ai[1]++;
                        if (NPC.ai[1] == 120)
                        {
                            gotoNextAttack = true;
                        }
                        break;
                    }
                #endregion
                #region Phase 1 Dash (360, both?)
                case 1:
                    {
                        //TODO: Dashes need telegraphing and visuals and also it looks a bit weird with how the wings work as is and also it should have a trail or something
                        //Telegraph/trail should be silver for predictive and gold for standard
                        const float dashSpeed = 24f;
                        const float distanceFromPlayer = 400f;
                        const int setupTime = 45;
                        const int period = 90;
                        const int iterations = 2;
                        if (NPC.ai[1] % period < setupTime)
                        {
                            //dash setup
                            Vector2 goalPosition = (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * distanceFromPlayer + player.Center;
                            Vector2 goalVelocity = (goalPosition - NPC.Center) / (setupTime - NPC.ai[1] % period) * 3;
                            NPC.velocity += (goalVelocity - NPC.velocity) / Math.Max(1, setupTime - NPC.ai[1] % period - setupTime / 2);
                        }
                        else if (NPC.ai[1] % period == setupTime)
                        {
                            if (NPC.ai[1] % (period * 2) == setupTime)
                            {
                                //normal dash
                                NPC.velocity = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * dashSpeed * 1.5f;
                            }
                            else
                            {
                                //default to velocity-matching if a predictive dash doesn't work
                                NPC.velocity = player.velocity.SafeNormalize(Vector2.Zero) * dashSpeed;

                                //predictive dash
                                Vector2 playerOffset = player.Center - NPC.Center;
                                float a = player.velocity.LengthSquared() - dashSpeed * dashSpeed;
                                float b = 2 * Vector2.Dot(playerOffset, player.velocity);
                                float c = playerOffset.LengthSquared();
                                float det = b * b - 4 * a * c;
                                if (det >= 0)
                                {
                                    float t = (-b + (a > 0 ? 1 : -1) * (float)Math.Sqrt(det)) / (2 * a);
                                    if (t > 0)
                                        NPC.velocity = (playerOffset + player.velocity * t).SafeNormalize(Vector2.Zero) * dashSpeed;
                                }
                            }
                            WarpZoomWaveParticle particle = Particle.NewParticle<WarpZoomWaveParticle>(NPC.Center, Vector2.Zero, MathHelper.Pi, 0, Scale: 0f, TimeLeft: 180);
                            ParticleLayer.WarpParticles.Add(particle);

                            //TODO: Produce particles to make it look like a jet is propelling the boss?
                        }
                        else
                        {
                            WarpZoomWaveParticle particle = Particle.NewParticle<WarpZoomWaveParticle>(NPC.Center, Vector2.Zero, MathHelper.Pi, 0, Scale: 0f, TimeLeft: 120);
                            particle.AlphaMultiplier = NPC.velocity.Length() / 480f;
                            particle.ScaleIncrement = 1200f;
                            ParticleLayer.WarpParticles.Add(particle);
                        }

                        NPC.ai[1]++;
                        if (NPC.ai[1] == period * iterations * 2)
                        {
                            gotoNextAttack = true;
                        }
                        break;
                    }
                #endregion
                #region Deathray rows (300, both?)
                case 2:
                    {
                        const float distanceFromPlayer = 400f;
                        const int setupTime = 60;
                        const int attackTime = 240;
                        const int attackPeriod = 60;

                        //TODO: visual indicator (either silver or gold) corresponding to direction of rotation
                        //TODO: Needs short wind-down
                        if (NPC.ai[1] < setupTime)
                        {
                            if (NPC.ai[1] == 0) NPC.ai[3] = Main.rand.NextBool() ? -1 : 1;

                            //get into position
                            //TODO: Pre-rotation here is a bit weird
                            Vector2 goalPosition = (NPC.Center - player.Center).SafeNormalize(Vector2.Zero).RotatedBy(-NPC.ai[3] * MathHelper.PiOver2 / attackTime * (setupTime - NPC.ai[1]) / 3) * distanceFromPlayer + player.Center;
                            Vector2 goalVelocity = (goalPosition - NPC.Center) / (setupTime - NPC.ai[1]) * 3;
                            NPC.velocity += (goalVelocity - NPC.velocity) / Math.Max(1, setupTime - NPC.ai[1] - setupTime / 2);

                            NPC.ai[2] = (NPC.Center - player.Center).ToRotation();
                        }
                        else
                        {
                            float progress = (NPC.ai[1] - setupTime) / attackTime;
                            Vector2 goalPosition = player.Center + new Vector2(distanceFromPlayer, 0).RotatedBy(NPC.ai[2] + (progress + 9f / attackTime) * MathHelper.PiOver2 * NPC.ai[3]);
                            NPC.velocity = (goalPosition - NPC.Center) / 10f;

                            //produce rays
                            if ((NPC.ai[1] - setupTime) % attackPeriod == 0)
                            {
                                Vector2 projectileOffset = new Vector2(0, 230).RotatedBy(NPC.ai[2] + progress * MathHelper.PiOver2 * NPC.ai[3]);
                                Vector2 projectileVelocity = new Vector2(1, 0).RotatedBy(NPC.ai[2] + progress * MathHelper.PiOver2 * NPC.ai[3]);
                                int parity = (int)((NPC.ai[1] - setupTime) % (attackPeriod * 2) / attackPeriod);
                                for (int i = -10; i <= 10; i++)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + projectileOffset * i, projectileVelocity, ProjectileType<EclipxieRay>(), ProjectileDamage, 0f, Main.myPlayer, ai0: i, ai1: parity);
                                }
                            }
                        }

                        NPC.ai[1]++;
                        if (NPC.ai[1] == setupTime + attackTime)
                        {
                            gotoNextAttack = true;
                        }
                        break;
                    }
                #endregion
                #region Planet Blender (450, both)
                case 3:
                    {
                        //TODO: This attack needs a longer/clearer telegraph to allow the player to slow down and reposition
                        const int setupTime = 60;
                        const int attackTime = 390;
                        const int distanceFromPlayer = 400;

                        if (NPC.ai[1] < setupTime)
                        {
                            if (NPC.ai[1] == 0) NPC.ai[3] = Main.rand.NextBool() ? 1 : -1;

                            Vector2 goalPosition = (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * distanceFromPlayer + player.Center;
                            Vector2 goalVelocity = (goalPosition - NPC.Center) / Math.Max(setupTime / 2 - NPC.ai[1], NPC.ai[1] - setupTime / 2 + 1);
                            NPC.velocity += (goalVelocity - NPC.velocity) / Math.Max(setupTime / 4 - NPC.ai[1], 1);
                        }
                        else
                        {
                            NPC.velocity = Vector2.Zero;

                            if (NPC.ai[1] == setupTime)
                            {
                                //produce planets
                                for (int i = 0; i < 300; i++)
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<EclipxieOrbiter>(), ProjectileDamage, 0, Main.myPlayer, ai0: NPC.whoAmI, ai1: i);
                                //produce deathrays
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (player.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.Pi / 6), ProjectileType<EclipxieRaysBig>(), ProjectileDamage, 0f, Main.myPlayer, ai0: NPC.whoAmI, ai1: 0);

                                ParticleLayer.WarpParticles.Add(Particle.NewParticle<WarpZoomPulseParticle>(NPC.Center, Vector2.Zero, MathHelper.Pi, 0, Scale: 0f));
                                WarpZoomWaveParticle particle = Particle.NewParticle<WarpZoomWaveParticle>(NPC.Center, Vector2.Zero, MathHelper.Pi, 0, Scale: 0f, TimeLeft: 180);
                                ParticleLayer.WarpParticles.Add(particle);
                            }
                        }

                        NPC.ai[1]++;
                        if (NPC.ai[1] == setupTime + attackTime)
                        {
                            gotoNextAttack = true;
                        }
                        break;
                    }
                #endregion
                #region Circle player while bombarding with projectiles (390, either lunar or solar)
                case 4:
                    {
                        const float distanceFromPlayer = 600f;
                        const int setupTime = 60;
                        const int attackTime = 240;
                        const int windDownTime = 90;
                        const float totalAngle = MathHelper.TwoPi;
                        
                        if (NPC.ai[1] < setupTime)
                        {
                            if (NPC.ai[1] == 0)
                            {
                                NPC.ai[3] = Main.rand.NextBool() ? -1 : 1;
                                NPC.localAI[0] = Main.rand.Next(2);

                                //if in phase 3
                                //NPC.localAI[0] = 2;
                            }

                            //TODO: Telegraph with color/shape indicator

                            //get into position
                            Vector2 goalPosition = (NPC.Center - player.Center).SafeNormalize(Vector2.Zero).RotatedBy(NPC.ai[3] * totalAngle / attackTime * (setupTime - NPC.ai[1]) / 3) * distanceFromPlayer + player.Center;
                            Vector2 goalVelocity = (goalPosition - NPC.Center) / (setupTime - NPC.ai[1]) * 3;
                            NPC.velocity += (goalVelocity - NPC.velocity) / Math.Max(1, setupTime - NPC.ai[1] - setupTime / 2);

                            NPC.ai[2] = (NPC.Center - player.Center).ToRotation();
                        }
                        else if (NPC.ai[1] < setupTime + attackTime)
                        {
                            float progress = (NPC.ai[1] - setupTime) / attackTime;
                            Vector2 goalPosition = player.Center + new Vector2(distanceFromPlayer, 0).RotatedBy(NPC.ai[2] + (progress + 9f / attackTime) * totalAngle * NPC.ai[3]);
                            NPC.velocity = (goalPosition - NPC.Center) / 10f;

                            const int attackPeriod = 12;
                            int attackPoint = attackPeriod - (int)((((NPC.ai[2] * NPC.ai[3] + MathHelper.TwoPi) % MathHelper.TwoPi) % (attackPeriod * totalAngle / attackTime)) / (totalAngle / attackTime));
                            attackPoint %= attackPeriod;
                            if (NPC.ai[1] % attackPeriod == attackPoint)
                            {
                                if (NPC.localAI[0] != 1)
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(1, 0).RotatedBy((player.Center - NPC.Center).ToRotation()), ProjectileType<EclipxieRayStar>(), ProjectileDamage, 0f, Main.myPlayer, ai1: 0);
                                if (NPC.localAI[0] != 0)
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(1, 0), ProjectileType<EclipxieRayStar>(), ProjectileDamage, 0f, Main.myPlayer, ai1: 1);
                            }
                        }
                        else
                        {
                            Vector2 goalPosition = player.Center + new Vector2(distanceFromPlayer, 0).RotatedBy(NPC.ai[2] + (1 + 9f / attackTime) * totalAngle * NPC.ai[3]);
                            NPC.velocity = (goalPosition - NPC.Center) / 10f;
                        }

                        NPC.ai[1]++;
                        if (NPC.ai[1] == setupTime + attackTime + windDownTime)
                        {
                            gotoNextAttack = true;
                        }
                        break;
                    }
                #endregion
                #region Starburst (480, both)
                case 5:
                    {
                        const int setupTime = 60;
                        const int attackTime = 420;
                        const float distanceFromPlayer = 400f;

                        if (NPC.ai[1] < setupTime)
                        {
                            if (NPC.ai[1] == 0) NPC.ai[3] = Main.rand.NextBool() ? 1 : -1;

                            Vector2 goalPosition = (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * distanceFromPlayer + player.Center;
                            Vector2 goalVelocity = (goalPosition - NPC.Center) / Math.Max(setupTime / 2 - NPC.ai[1], NPC.ai[1] - setupTime / 2 + 1);
                            NPC.velocity += (goalVelocity - NPC.velocity) / Math.Max(setupTime / 4 - NPC.ai[1], 1);
                        }
                        else
                        {
                            Vector2 goalPosition = (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * distanceFromPlayer + player.Center;
                            NPC.velocity = (goalPosition - NPC.Center) / 10f;

                            if ((NPC.ai[1] - setupTime) % 180 == 0 && NPC.ai[1] - setupTime < 360)
                            {
                                ParticleLayer.WarpParticles.Add(Particle.NewParticle<WarpZoomPulseParticle>(NPC.Center, Vector2.Zero, MathHelper.Pi, 0, Scale: 0f));
                                WarpZoomWaveParticle particle = Particle.NewParticle<WarpZoomWaveParticle>(NPC.Center, Vector2.Zero, MathHelper.Pi, 0, Scale: 0f, TimeLeft: 180);
                                ParticleLayer.WarpParticles.Add(particle);

                                bool parity = Vector2.Dot(player.velocity.RotatedBy(MathHelper.PiOver2), NPC.Center - player.Center) < 0;
                                for (int i = 0; i < 10; i++)
                                {
                                    float speedMult = ((i % 2 == 0) ^ parity) ? (3 + (float)Math.Sqrt(5)) / 2 : 1;
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(32 * speedMult, 0).RotatedBy((player.Center - NPC.Center).ToRotation() + (i + 0.5f) * MathHelper.TwoPi / 10), ProjectileType<EclipxieRayStar>(), ProjectileDamage, 0f, Main.myPlayer, ai0: i == 0 ? 1.5f : 1, ai1: (i + (parity ? 1 : 0)) % 2);
                                }
                            }
                        }

                        NPC.ai[1]++;
                        if (NPC.ai[1] == setupTime + attackTime)
                        {
                            gotoNextAttack = true;
                        }
                        break;
                    }
                #endregion
                #region Meteor Shower (690, both? currently, should maybe be lunar)
                case 6:
                    {
                        const int setupTimePart1 = 60;
                        const int setupTime = 90;
                        const int attackTime = 510;
                        const int windDownTime = 90;
                        const float distanceFromPlayer = 400f;

                        if (NPC.ai[1] < setupTimePart1)
                        {
                            float progress = NPC.ai[1] / setupTimePart1;
                            Vector2 goalPosition = player.Center + (new Vector2(0, -progress) + (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * (1 - progress)).SafeNormalize(Vector2.Zero) * distanceFromPlayer;
                            Vector2 goalVelocity = (goalPosition - NPC.Center) / Math.Max(3 * setupTimePart1 / 4 - NPC.ai[1], 1);
                            NPC.velocity += (goalVelocity - NPC.velocity) / Math.Max(setupTimePart1 / 4 - NPC.ai[1], 1);
                        }
                        else if (NPC.ai[1] < setupTime)
                        {
                            //TODO: This attack needs an indicator and probably some more oomph
                            float progress = (NPC.ai[1] - setupTimePart1) / (setupTime - setupTimePart1);
                            NPC.velocity = (player.Center + new Vector2(0, -distanceFromPlayer) - NPC.Center) / Vector2.Lerp(Vector2.One, new Vector2(60, 20), progress);
                        }
                        else if (NPC.ai[1] < setupTime + attackTime)
                        {
                            if (NPC.ai[1] == setupTime)
                            {
                                NPC.ai[3] = player.velocity.X > 0 ? 1 : (player.velocity.X < 0 ? -1 : (Main.rand.NextBool() ? 1 : -1));
                            }

                            NPC.velocity.Y = (player.Center.Y - distanceFromPlayer - NPC.Center.Y) / 20f;
                            float pullMultiplier = 24000f / Math.Abs(player.Center.X - NPC.Center.X);
                            NPC.velocity.X = (NPC.ai[3] * 12f + (player.Center.X - NPC.Center.X) / pullMultiplier) * (NPC.ai[1] - setupTime) / attackTime;

                            if (NPC.ai[1] < setupTime + attackTime)
                                for (int side = -1; side <= 1; side += 2)
                                {
                                    Vector2 shotVelocity = new Vector2(NPC.velocity.X, 48);
                                    Vector2 shotPosition = NPC.Center + new Vector2(side * Main.rand.NextFloat(240, 1200), -shotVelocity.Y * 30);
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), shotPosition, shotVelocity, ProjectileType<EclipxieMeteor>(), ProjectileDamage, 0f, Main.myPlayer, ai1: 1);
                                }
                            if ((NPC.ai[1] - 20 - setupTime) % 60 == 0 && NPC.ai[1] < setupTime + attackTime - 120)
                            {
                                float rotOffset = (NPC.Center - player.Center).ToRotation();
                                float projSpeed = 24 * distanceFromPlayer / (player.Center - NPC.Center).Length();
                                for (int i = 0; i < 3; i++)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(projSpeed, 0).RotatedBy(i * MathHelper.TwoPi / 3 + rotOffset), ProjectileType<EclipxieMeteor>(), ProjectileDamage, 0f, Main.myPlayer, ai0: 3, ai1: 0);
                                }
                            }
                        }
                        else
                        {
                            NPC.velocity.Y = (player.Center.Y - distanceFromPlayer - NPC.Center.Y) / 20f;
                            float pullMultiplier = 24000f / Math.Abs(player.Center.X - NPC.Center.X);
                            NPC.velocity.X = (NPC.ai[3] * 12f + (player.Center.X - NPC.Center.X) / pullMultiplier) * (setupTime + attackTime + windDownTime - NPC.ai[1]) / windDownTime;
                        }

                        NPC.ai[1]++;
                        if (NPC.ai[1] == setupTime + attackTime + windDownTime)
                        {
                            gotoNextAttack = true;
                        }
                        break;
                    }
                #endregion
                #region Star Pursuit (480, both?)
                case 7:
                    {
                        const int setupTime = 60;
                        const int attackPart1Time = 240;
                        const int attackPart2Time = 180;
                        const float distanceFromPlayer = 600f;

                        if (NPC.ai[1] < setupTime)
                        {
                            Vector2 goalPosition = (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * distanceFromPlayer + player.Center;
                            Vector2 goalVelocity = (goalPosition - NPC.Center) / (setupTime - NPC.ai[1]) * 3;
                            NPC.velocity += (goalVelocity - NPC.velocity) / Math.Max(1, setupTime - NPC.ai[1] - setupTime / 2);

                            NPC.ai[2] = 0;
                        }
                        else if (NPC.ai[1] < setupTime + attackPart1Time)
                        {
                            NPC.velocity += (player.Center - NPC.Center) / 1000f;
                            if (Vector2.Dot(NPC.velocity, player.Center - NPC.Center) < 0) NPC.velocity *= 0.9f;
                            else if (Vector2.Dot(NPC.velocity - player.velocity, player.Center - NPC.Center) < 0) NPC.velocity = (NPC.velocity - player.velocity) * 0.95f + player.velocity;

                            NPC.ai[2] += NPC.velocity.Length() / 24f;

                            if (NPC.ai[2] >= 1)
                            {
                                WarpZoomWaveParticle particle = Particle.NewParticle<WarpZoomWaveParticle>(NPC.Center, Vector2.Zero, MathHelper.Pi, 0, Scale: 0f, TimeLeft: 120);
                                particle.AlphaMultiplier = NPC.velocity.Length() / 480f;
                                particle.ScaleIncrement = 2400f;
                                ParticleLayer.WarpParticles.Add(particle);
                            }

                            while (NPC.ai[2] >= 1)
                            {
                                NPC.ai[2]--;
                                for (int i = 0; i < 2; i++)
                                {
                                    int projIndex = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(Main.rand.NextFloat(10), 0).RotatedByRandom(MathHelper.TwoPi), ProjectileType<EclipxieMeteor>(), ProjectileDamage, 0f, Main.myPlayer, ai0: 1, ai1: i);
                                    Main.projectile[projIndex].timeLeft = attackPart2Time + (attackPart1Time + setupTime - (int)NPC.ai[1]) * 5 / 6;
                                }
                            }
                        }
                        else
                        {
                            NPC.velocity += (player.Center - NPC.Center) * 0.001f;
                            NPC.velocity *= 0.95f;
                        }

                        NPC.ai[1]++;
                        if (NPC.ai[1] == setupTime + attackPart1Time + attackPart2Time)
                        {
                            gotoNextAttack = true;
                        }
                        break;
                    }
                #endregion
                #region Deathray Sweep (285, both)
                case 8:
                    {
                        const int setupTime = 60;
                        const int hoverTime = 45;
                        const int attackTime = 120;
                        const int windDownTime = 60;

                        if (NPC.ai[1] < setupTime)
                        {
                            if (NPC.ai[1] == 0) NPC.ai[2] = NPC.Center.X < player.Center.X ? -1 : 1;

                            float progress = NPC.ai[1] / setupTime;
                            Vector2 finalGoalPosition = player.Center + new Vector2(NPC.ai[2] * 600, -400);
                            Vector2 goalPosition = player.Center + ((finalGoalPosition - player.Center).SafeNormalize(Vector2.Zero) * progress + (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * (1 - progress)).SafeNormalize(Vector2.Zero) * (finalGoalPosition - player.Center).Length();
                            Vector2 goalVelocity = (goalPosition - NPC.Center) / Math.Max(3 * setupTime / 4 - NPC.ai[1], 1);
                            NPC.velocity += (goalVelocity - NPC.velocity) / Math.Max(setupTime / 4 - NPC.ai[1], 1);
                        }
                        else if (NPC.ai[1] < setupTime + hoverTime)
                        {
                            if (NPC.ai[1] == setupTime)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 1), ProjectileType<EclipxieRaysBig>(), ProjectileDamage, 0f, Main.myPlayer, ai0: NPC.whoAmI, ai1: 2);
                            }

                            Vector2 goalPosition = player.Center + new Vector2(NPC.ai[2] * 600, -400);
                            NPC.velocity = (goalPosition - NPC.Center) / 20f;
                        }
                        else if (NPC.ai[1] < setupTime + hoverTime + attackTime)
                        {
                            float progress = (NPC.ai[1] - setupTime - hoverTime) / attackTime;
                            NPC.velocity = new Vector2(-NPC.ai[2] * progress * 64, 0);
                        }
                        else
                        {
                            //wind down
                            //TODO: Use sun pixie's arcing motions for this and probably some other stuff too
                            float progress = (NPC.ai[1] - setupTime - hoverTime - attackTime) / windDownTime;
                            Vector2 finalGoalPosition = player.Center + new Vector2(-NPC.ai[2] * 600, -400);
                            Vector2 goalPosition = player.Center + ((finalGoalPosition - player.Center).SafeNormalize(Vector2.Zero) * progress + (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * (1 - progress)).SafeNormalize(Vector2.Zero) * (finalGoalPosition - player.Center).Length();
                            Vector2 goalVelocity = (goalPosition - NPC.Center) / Math.Max(3 * windDownTime / 4 - (NPC.ai[1] - setupTime - hoverTime - attackTime), 1);
                            NPC.velocity += (goalVelocity - NPC.velocity) / Math.Max(windDownTime / 4 - (NPC.ai[1] - setupTime - hoverTime - attackTime), 1);
                        }

                        NPC.ai[1]++;
                        if (NPC.ai[1] == setupTime + hoverTime + attackTime + windDownTime)
                        {
                            gotoNextAttack = true;
                        }
                        break;
                    }
                #endregion
                #region Converging star rays (300, solar?)
                case 9:
                    {
                        //Ideas for star ray type attacks:
                        //  Summons n stars around it which produce deathrays outwards and converge towards each other
                        //    These could either converge fully, requiring the player to go between them, or be merciful and stop before convergence
                        //  Summons a circlesgridthing of stars around it, then produces a shadery pulse which causes each star to produce a ray outwards in a pulse
                        //    This could also work as a followup to the planet blender?

                        const int setupTime = 60;
                        const int telegraphTime = 60;
                        const int attackTime = 180;
                        const float distanceFromPlayer = 600f;

                        if (NPC.ai[1] < setupTime)
                        {
                            Vector2 goalPosition = (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * distanceFromPlayer + player.Center;
                            Vector2 goalVelocity = (goalPosition - NPC.Center) / Math.Max(setupTime / 2 - NPC.ai[1], 1);
                            NPC.velocity += (goalVelocity - NPC.velocity) / Math.Max(setupTime / 4 - NPC.ai[1], 1);
                        }
                        else
                        {
                            //TODO: Create burst on ray activation
                            //TODO: Some sort of visual connection to the stars (wavy lines with a circle around them?)
                            //TODO: Maybe do something when the stars converge?
                            if (NPC.ai[1] == setupTime)
                            {
                                int sweepDirection = Main.rand.Next(-2, 3);
                                NPC.ai[2] = sweepDirection * MathHelper.Pi / 3;
                                for (int i = -3; i < 3; i++)
                                {
                                    float rotation = (player.Center - NPC.Center).ToRotation() + (i + sweepDirection + 0.5f) * MathHelper.TwoPi / 6;
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(240, 0).RotatedBy(rotation), new Vector2(-(i + 0.5f) * MathHelper.TwoPi / 6, 0), ProjectileType<EclipxieRayStar>(), ProjectileDamage, 0f, Main.myPlayer, ai0: 2, ai1: NPC.whoAmI);
                                }
                            }
                            if (NPC.ai[1] < setupTime + telegraphTime)
                            {
                                Vector2 goalPosition = (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * distanceFromPlayer + player.Center;
                                NPC.velocity = (goalPosition - NPC.Center) * ((setupTime + telegraphTime - NPC.ai[1]) / telegraphTime);
                            }
                            else
                            {
                                if (NPC.ai[1] == setupTime + telegraphTime) NPC.ai[2] += (NPC.Center - player.Center).ToRotation();

                                float progress = (NPC.ai[1] - setupTime - telegraphTime) / (attackTime - telegraphTime);
                                NPC.velocity = new Vector2(progress * (1 - progress) * 20f, 0).RotatedBy(NPC.ai[2]);
                            }
                        }

                        NPC.ai[1]++;
                        if (NPC.ai[1] == setupTime + attackTime)
                        {
                            gotoNextAttack = true;
                        }
                        break;
                    }
                #endregion
                #region Random rays (300, both)
                case 10:
                    {
                        const int setupTime = 60;
                        const int attackTime = 240;
                        const float distanceFromPlayer = 400f;

                        if (NPC.ai[1] < setupTime)
                        {
                            Vector2 goalPosition = (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * distanceFromPlayer + player.Center;
                            Vector2 goalVelocity = (goalPosition - NPC.Center) / Math.Max(setupTime / 2 - NPC.ai[1], NPC.ai[1] - setupTime / 2 + 1);
                            NPC.velocity += (goalVelocity - NPC.velocity) / Math.Max(setupTime / 4 - NPC.ai[1], 1);
                        }
                        else
                        {
                            NPC.velocity = (player.Center - NPC.Center) / 120;

                            //TODO: Maybe start off slow and speed up?
                            if ((NPC.ai[1] - setupTime) % 20 == 0 && NPC.ai[1] + 60 < setupTime + attackTime)
                            {
                                int typeModifier = (int)(NPC.ai[1] - setupTime) % 40 / 20; //could also maybe alternate
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(1, 0).RotatedByRandom(MathHelper.TwoPi), ProjectileType<EclipxieRaysBig>(), ProjectileDamage, 0f, Main.myPlayer, ai0: NPC.whoAmI, ai1: 4 + typeModifier);
                            }
                        }

                        NPC.ai[1]++;
                        if (NPC.ai[1] == setupTime + attackTime)
                        {
                            gotoNextAttack = true;
                        }
                        break;
                    }
                    #endregion
            }

            if (gotoNextAttack)
            {
                //TODO: p1 should alternate (solar or lunar), (lunar or solar), both?
                NPC.ai[0] = Main.rand.Next(1, 11);
                NPC.ai[1] = 0;
            }
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter = (NPC.frameCounter + 1) % 5;
            if (NPC.frameCounter == 0)
            {
                NPC.frame.Y = (NPC.frame.Y + frameHeight) % (8 * frameHeight);
            }
        }

        public override void DrawBehind(int index)
        {
            RenderTargetLayer.AddNPC<EclipxieTarget>(index);
            DrawLayer.AddNPC<DrawLayerBeforeScreenObstruction>(index);
            DrawLayer.AddNPC<DrawLayerAfterAdditiveBeforeScreenObstruction>(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            drawColor = NPC.GetNPCColorTintedByBuffs(Color.White);

            Vector2 halfRenderTargetSize = RenderTargetLayer.GetRenderTargetLayer<EclipxieTarget>().Center;

            if (RenderTargetLayer.IsActive<EclipxieTarget>())
            {
                Texture2D texture = TextureAssets.Npc[Type].Value;

                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f / Main.npcFrameCount[Type] - 16);
                Vector2 drawPos = halfRenderTargetSize;
                spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, 0.5f, SpriteEffects.None, 0f);

                //glow
                spriteBatch.Draw(Textures.Glow256.Value, drawPos, Textures.Glow256.Frame(), new Color(255, 224, 192) * 0.1f, 0f, Textures.Glow256.Size() / 2, 0.3f, SpriteEffects.None, 0f);

                spriteBatch.End();
                spriteBatch.Begin((SpriteSortMode)1, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Matrix.Identity);

                Texture2D coronaTexture = FireGradient.Value;

                GameShaders.Misc["Polarities:EclipxieSun"].UseImage1(Textures.Perlin256).UseShaderSpecificData(new Vector4((PolaritiesSystem.timer / 120f) % 1, 0.75f, 0.45f, 0)).Apply();

                Vector2 coronaScaling = new Vector2(1f / coronaTexture.Width, 1) * 86 * 0.5f;
                spriteBatch.Draw(coronaTexture, drawPos, coronaTexture.Frame(), Color.White, 0f, coronaTexture.Size() / 2, coronaScaling, SpriteEffects.None, 0);

                spriteBatch.End();
                spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, (Effect)null, Matrix.Identity);

                Vector2 newDrawOrigin = new Vector2(MoonTexture.Width() / 2, MoonTexture.Height() / 2);
                Vector2 moonOffset = (Main.LocalPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 1.5f; //TODO: Move eye nontrivially
                spriteBatch.Draw(MoonTexture.Value, drawPos + moonOffset, new Rectangle(0, 0, MoonTexture.Width(), MoonTexture.Height()), drawColor, NPC.rotation, newDrawOrigin, 0.5f, SpriteEffects.None, 0f);

                return false;
            }
            if (DrawLayer.IsActive<DrawLayerAfterAdditiveBeforeScreenObstruction>())
            {
                //draw an extra moon over stuff sometimes
                //TODO: The positioning isn't the same as normal and the darkness can be weird
                if (NPC.ai[0] == 3 && NPC.ai[1] >= 60 || NPC.ai[0] == 8 && NPC.ai[1] >= 60 || NPC.ai[0] == 10 && NPC.ai[1] >= 60)
                {
                    Vector2 moonOffset = (Main.LocalPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 1.5f; //TODO: Move eye nontrivially
                    spriteBatch.Draw(MoonTexture.Value, NPC.Center + moonOffset - screenPos, MoonTexture.Frame(), Color.Black, 0f, MoonTexture.Size() / 2, NPC.scale, SpriteEffects.None, 0f);
                }

                return false;
            }

            if (NPC.ai[0] == 0)
            {
                const float spawnAnimTime = 120f;
                float progress = NPC.ai[1] / spawnAnimTime;

                //spawn animation
                float glowAlpha = 8f * (1 - progress);
                float glowScale = (float)Math.Exp(progress * 16f) - 1;
                //TODO: Radiating lines moving outwards rapidly
                //TODO: Possibly also warp space and screenshake
                spriteBatch.Draw(Textures.Glow256.Value, NPC.Center - screenPos, Textures.Glow256.Frame(), Color.White * glowAlpha, 0f, Textures.Glow256.Size() / 2, glowScale, SpriteEffects.None, 0f);

                Color modifiedColor = new Color(new Vector3(1 - 8f * (1 - progress))) * ((progress - 0.1f) / 0.9f);

                //TODO: maybe show up in a more interesting way
                if (RenderTargetLayer.GetRenderTargetLayer<EclipxieTarget>().HasContent())
                    RenderTargetLayer.GetRenderTargetLayer<EclipxieTarget>().Draw(spriteBatch, NPC.Center - screenPos, modifiedColor, halfRenderTargetSize);
            }
            else
            {
                if (NPC.ai[0] == 1 && NPC.ai[1] % 90 < 45)
                {
                    //phase 1 dash telegraph
                    //TODO: This currently feels a bit dull
                    float telegraphProgress = (NPC.ai[1] % 90) / 45f;
                    Color telegraphColor = ((int)NPC.ai[1] % 180) / 90 == 0 ? new Color(255, 224, 192) : new Color(192, 224, 255);
                    spriteBatch.Draw(Textures.Glow256.Value, NPC.Center - screenPos, Textures.Glow256.Frame(), telegraphColor * telegraphProgress, 0f, Textures.Glow256.Size() / 2, (float)Math.Sqrt(1 - telegraphProgress) * 2f + 0.25f, SpriteEffects.None, 0f);
                }

                if (RenderTargetLayer.GetRenderTargetLayer<EclipxieTarget>().HasContent())
                    RenderTargetLayer.GetRenderTargetLayer<EclipxieTarget>().Draw(spriteBatch, NPC.Center - screenPos, Color.White, halfRenderTargetSize);
            }

            return false;
        }
    }
    public class EclipxieTarget : RenderTargetLayer
    {
        public override int Width => 600;
        public override int Height => 600;
        public override bool useIdentityMatrix => true;

        public override void Load(Mod mod)
        {
            base.Load(mod);

            targetScale = 0.5f;
        }
    }
    public class EclipxieSky : CustomSky, ILoadable
    {
        bool isActive;
        float fadeOpacity;

        public void Load(Mod mod)
        {
            Filters.Scene["Polarities:EclipxieSky"] = new Filter(new ScreenShaderData("FilterMiniTower").UseColor(1f, 1f, 1f).UseOpacity(0f), EffectPriority.VeryLow);
            SkyManager.Instance["Polarities:EclipxieSky"] = new EclipxieSky();
        }

        public void Unload()
        {
            if (skyTarget != null)
            {
                skyTarget.Dispose();
                skyTarget = null;
            }
        }

        static RenderTarget2D skyTarget;
        static int numStars = 0;
        static float textureGenProgress = 0f;

        //TODO: This approach to star drawing is almost certainly horribly inefficient
        //Generates stars gradually over the course of the spawn anim to avoid freezing up
        public static void UpdateSky(float progress)
        {
            int targetSize = (int)Math.Ceiling(2 * Math.Sqrt(Main.screenWidth * Main.screenWidth / 4f + Main.screenHeight * Main.screenHeight));
            int maxStars = (int)((targetSize * targetSize) / 10 * progress);
            if (skyTarget == null || skyTarget.Width != targetSize || numStars < maxStars || progress < textureGenProgress)
            {
                bool resetTarget = skyTarget == null || skyTarget.Width != targetSize || progress < textureGenProgress;

                if (resetTarget)
                {
                    skyTarget = new RenderTarget2D(Main.spriteBatch.GraphicsDevice, targetSize, targetSize, false, Main.spriteBatch.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
                    numStars = 0;
                    textureGenProgress = progress;
                }

                RenderTarget2D skyTargetBackup = new RenderTarget2D(Main.spriteBatch.GraphicsDevice, targetSize, targetSize, false, Main.spriteBatch.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

                Main.spriteBatch.GraphicsDevice.SetRenderTarget(skyTargetBackup);
                Main.spriteBatch.GraphicsDevice.Clear(new Color(0, 0, 2));
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Matrix.Identity);

                Main.spriteBatch.Draw(skyTarget, skyTarget.Frame(), Color.White);

                Main.spriteBatch.End();

                Main.spriteBatch.GraphicsDevice.SetRenderTarget(skyTarget);
                Main.spriteBatch.GraphicsDevice.Clear(new Color(0, 0, 2));
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Matrix.Identity);

                if (skyTargetBackup != null) Main.spriteBatch.Draw(skyTargetBackup, skyTarget.Frame(), Color.White);

                if (resetTarget)
                {
                    //TODO: Milky way could use some sort of core

                    //milky way
                    const int numDraws = 60;
                    for (int i = 0; i < numDraws; i++)
                    {
                        //TODO: Some positional adjustments so it doesn't look basically symmetric
                        Texture2D texture = Textures.Glow256.Value;
                        Texture2D fuzzTexture = TextureAssets.Projectile[ProjectileType<ContagunProjectile>()].Value;
                        Vector2 drawPos = new Vector2(i * targetSize / (float)numDraws, targetSize / 4f);
                        float scaleMult = 512 * (float)(Math.Pow(i * (numDraws - i) / (float)(numDraws * numDraws) * 4, 2) + 2) / 3 * Main.rand.NextFloat(0.25f, 1f);
                        Color drawColor = Color.Lerp(new Color(192, 224, 255), new Color(255, 224, 192), i * (numDraws - i) / (float)(numDraws * numDraws) * 4 + Main.rand.NextFloat(-0.2f, 0.2f)) * 0.25f;
                        Main.spriteBatch.Draw(texture, drawPos, texture.Frame(), drawColor, Main.rand.NextFloat(MathHelper.TwoPi), texture.Size() / 2, scaleMult / texture.Width, (SpriteEffects)Main.rand.Next(2), 0f);
                        Main.spriteBatch.Draw(fuzzTexture, drawPos, fuzzTexture.Frame(), drawColor, Main.rand.NextFloat(MathHelper.TwoPi), fuzzTexture.Size() / 2, scaleMult / fuzzTexture.Width * 1.2f, (SpriteEffects)Main.rand.Next(2), 0f);
                    }
                    for (int i = 0; i < numDraws; i++)
                    {
                        Texture2D texture = TextureAssets.Projectile[ProjectileType<ContagunProjectile>()].Value;
                        Vector2 drawPos = new Vector2(i * targetSize / (float)numDraws, targetSize / 4f);
                        float scaleMult = 512 * (float)(Math.Pow(i * (numDraws - i) / (float)(numDraws * numDraws) * 4, 2) + 2) / 3 * Main.rand.NextFloat(0.25f, 1f);
                        Main.spriteBatch.Draw(texture, drawPos, texture.Frame(), new Color(16, 8, 0), Main.rand.NextFloat(MathHelper.TwoPi), texture.Size() / 2, scaleMult / texture.Width * 0.9f, (SpriteEffects)Main.rand.Next(2), 0f);
                        Main.spriteBatch.Draw(texture, drawPos, texture.Frame(), new Color(16, 8, 0), Main.rand.NextFloat(MathHelper.TwoPi), texture.Size() / 2, scaleMult / texture.Width * 1.1f, (SpriteEffects)Main.rand.Next(2), 0f);
                    }

                    //TODO: A few other galaxies/clusters
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Matrix.Identity);

                for (int i = numStars; i < maxStars; i++)
                {
                    Vector2 starPosition = new Vector2((float)Math.Sqrt(Main.rand.NextFloat(1)) * targetSize / 2f, 0).RotatedByRandom(MathHelper.TwoPi);
                    const float minDistance = 0.015f;
                    float starDistance = (float)Math.Pow(Main.rand.NextFloat(minDistance, 1f), 1 / 3f);
                    float starBrightness = Main.rand.NextFloat(1);
                    Color starColor = ModUtils.ConvectiveFlameColor(Main.rand.NextFloat(1));

                    Main.spriteBatch.Draw(Textures.Glow58.Value, new Vector2(targetSize / 2f) + starPosition, Textures.Glow58.Frame(), starColor * (0.25f / starDistance), 0f, Vector2.Zero, 0.03f / starDistance * starBrightness, SpriteEffects.None, 0f);
                }
                numStars = maxStars;

                Main.spriteBatch.End();
                Main.spriteBatch.GraphicsDevice.SetRenderTarget(null);

                skyTargetBackup.Dispose();

                /*if (progress == 1f)
                {
                    var stream = File.Create(Main.SavePath + Path.DirectorySeparatorChar + "ModSources/Polarities/NPCs/Eclipxie/EclipxieSky_Background.png");
                    skyTarget.SaveAsPng(stream, skyTarget.Width, skyTarget.Height);
                    stream.Dispose();
                }*/
            }
        }

        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
            fadeOpacity = 0.002f;
        }

        public override void Deactivate(params object[] args)
        {
            isActive = false;
        }

        public override bool IsActive()
        {
            if (!isActive)
            {
                if (fadeOpacity <= 0.001f)
                {
                    if (skyTarget != null)
                    {
                        skyTarget.Dispose();
                        skyTarget = null;
                    }
                    numStars = 0;
                    textureGenProgress = 0f;
                    return false;
                }
            }
            return true;
        }

        public override void Reset()
        {
            isActive = false;
            if (skyTarget != null)
            {
                skyTarget.Dispose();
                skyTarget = null;
            }
            numStars = 0;
            textureGenProgress = 0f;
        }

        //TODO: CustomSkies freeze when time is frozen thanks to Main.desiredWorldEventsUpdateRate, I should fix this
        public override void Update(GameTime gameTime)
        {
            if (isActive)
            {
                fadeOpacity = Math.Min(1f, 0.01f + fadeOpacity);
            }
            else
            {
                fadeOpacity = Math.Max(0f, fadeOpacity - 0.01f);
            }

            textureGenProgress = Math.Min(1f, textureGenProgress + 1 / 120f);
            UpdateSky(textureGenProgress);
        }

        public override Color OnTileColor(Color inColor)
        {
            PolaritiesSystem.modifyBackgroundColor = Color.Lerp(PolaritiesSystem.modifyBackgroundColor, Color.Black, fadeOpacity);
            return Color.Lerp(inColor, new Color(8, 8, 8), fadeOpacity);
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            Main.ColorOfTheSkies = Color.Lerp(Main.ColorOfTheSkies, Color.Black, fadeOpacity);

            const float depth = 10f;
            if (minDepth <= depth && maxDepth > depth)
            {
                //TODO: Maybe base on Eclipxie's health? Or at least choose values s.t. the milky way is always visible
                if (skyTarget != null) Main.spriteBatch.Draw(skyTarget, new Vector2(Main.screenWidth / 2f, Main.screenHeight), skyTarget.Frame(), Color.White * fadeOpacity, PolaritiesSystem.timer * MathHelper.TwoPi / 86400, skyTarget.Size() / 2, 1f, SpriteEffects.None, 0f);
            }
        }
    }
    //TODO: Remove any pixelation from this and maybe lasers
    public class WarpZoomPulseParticle : Particle
    {
        public override string Texture => "Polarities/Textures/WarpZoom256";

        public override void Initialize()
        {
            Color = Color.White;
            Glow = true;
            TimeLeft = 30;
        }

        public float ScaleIncrement = 30f;

        public override void AI()
        {
            Scale += ScaleIncrement / (float)MaxTimeLeft;
            Alpha = TimeLeft / (float)MaxTimeLeft;
        }
    }
    public class WarpZoomWaveParticle : Particle
    {
        public override string Texture => "Polarities/Textures/Pixel";

        public override void Initialize()
        {
            Color = Color.White;
            Glow = true;
            TimeLeft = 240;
        }

        public float ScaleIncrement = 7200f;
        public float AlphaMultiplier = 0.25f;

        public override void AI()
        {
            Scale += ScaleIncrement / (float)MaxTimeLeft;
            Alpha = TimeLeft / (float)MaxTimeLeft * AlphaMultiplier;
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            spritebatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);
            float width = Math.Min(256f, Scale / 2);
            GameShaders.Misc["Polarities:WarpZoomRipple"].UseShaderSpecificData(new Vector4(1 - width / Math.Max(Scale, 1 / width), width / 256 * Alpha, 0, 0)).Apply();

            Asset<Texture2D> particleTexture = particleTextures[Type];

            Vector2 drawPosition = Position - Main.screenPosition;

            spritebatch.Draw(particleTexture.Value, drawPosition, particleTexture.Frame(), Color.White, 0f, particleTexture.Size() / 2, Scale, SpriteEffects.None, 0f);

            spritebatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);
        }
    }
    public class EclipxieRay : ModProjectile
    {
        public static Asset<Texture2D> Distortion;
        public static Asset<Texture2D> Solar;

        public override void Load()
        {
            Distortion = Request<Texture2D>(Texture + "_Distortion");
            Solar = Request<Texture2D>(Texture + "_Solar");

            /*IL.Terraria.Main.UpdateMenu += Main_UpdateMenu;
		}

        private void Main_UpdateMenu(MonoMod.Cil.ILContext il)
        {
            MonoMod.Cil.ILCursor c = new MonoMod.Cil.ILCursor(il);

			c.EmitDelegate<Action>(() =>
			{
				if (!(bool)(typeof(ModLoader).GetField("isLoading", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null)))
				{
					String filePath = Main.SavePath + Path.DirectorySeparatorChar + "ModSources/Polarities/NPCs/Eclipxie/EclipxieRay_Solar.png";

					if (!System.IO.File.Exists(filePath))
					{
						Terraria.Utilities.UnifiedRandom rand = new Terraria.Utilities.UnifiedRandom(278539);
						const int textureSize = 64;

						float[,] fractalNoise = rand.FractalNoise(textureSize, 8);
                        float[,] processedNoise = new float[textureSize, textureSize];
                        for (int i = 0; i < textureSize; i++)
                        {
                            for (int j = 0; j < textureSize; j++)
                            {
                                processedNoise[i, j] = 0;
                                float expBase = -(1 + 16f / textureSize);
                                for (int k = 0; k < textureSize; k++)
                                    processedNoise[i, j] += fractalNoise[i, (j + k) % textureSize] / (float)Math.Pow(expBase, k) * (expBase - 1) / expBase;
                            }
                        }

						Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, textureSize, textureSize, false, SurfaceFormat.Color);
						System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
						for (int i = 0; i < texture.Width; i++)
						{
							for (int j = 0; j < texture.Height; j++)
							{
								float x = (2 * j / (float)(texture.Width - 1) - 1);
								float y = (2 * i / (float)(texture.Height - 1) - 1);

                                Color baseColor = Color.Black;
                                float baseAlpha = 0;

                                if (Math.Abs(y) < 0.5f)
                                {
                                    baseAlpha = (float)Math.Sqrt(1 - 4 * y * y) * (float)(1 + 0.5f * Math.Abs(fractalNoise[0, j]));
                                    baseAlpha *= 1 - (1 - baseAlpha) * (processedNoise[i, j] + 0.5f);
                                    baseAlpha = Math.Clamp(baseAlpha, 0, 1);

                                    //float baseAlpha = (float)Math.Pow(1 - y * y, 2);
                                    //baseAlpha *= 1 - 2 * (1 - baseAlpha) * (processedNoise[i, j] + 0.5f);

                                    baseColor = ModUtils.ConvectiveFlameColor((float)Math.Pow(baseAlpha, 3) * 0.5f);
                                    //baseColor = Color.Lerp(new Color(0, 128, 256), Color.White, (float)Math.Pow(baseAlpha, 2));
                                    baseAlpha = (float)Math.Sqrt(baseAlpha);
                                }

                                Color bloomColor = ModUtils.ConvectiveFlameColor((float)Math.Pow(1 - y * y, 2) * 0.5f);
                                //Color bloomColor = Color.Lerp(new Color(0, 128, 256), Color.White, 1 - y * y);
                                float bloomAlpha = 1 - y * y;
                                baseColor = new Color(baseColor.ToVector3() * baseAlpha + bloomColor.ToVector3() * (1 - baseAlpha));
                                baseAlpha += bloomAlpha * (1 - baseAlpha);

								list.Add(baseColor * baseAlpha);
							}
						}
						texture.SetData(list.ToArray());
						texture.SaveAsPng(new System.IO.FileStream(filePath, System.IO.FileMode.Create), texture.Width, texture.Height);
					}
				}
			});*/
        }

        public override void Unload()
        {
            Distortion = null;
            Solar = null;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 36;
            Projectile.height = 36;

            Projectile.timeLeft = 90;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.hide = true;

            Projectile.GetGlobalProjectile<PolaritiesProjectile>().ForceDraw = true;
            Projectile.GetGlobalProjectile<PolaritiesProjectile>().canLeaveWorld = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.timeLeft = 90 + (int)Math.Abs(Projectile.ai[0] * 8);
        }

        public override void AI()
        {
            Projectile.position += (Main.LocalPlayer.Center - Projectile.Center) / 300;
        }

        const float radius = 4000f;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center - new Vector2(radius, 0).RotatedBy(Projectile.rotation), Projectile.Center + new Vector2(radius, 0).RotatedBy(Projectile.rotation));
        }

        public override bool? CanDamage()
        {
            return Projectile.timeLeft < 60 ? null : false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            RenderTargetLayer.AddProjectile<ScreenWarpTarget>(index);
            DrawLayer.AddProjectile<DrawLayerAdditiveAfterNPCs>(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            //space warping
            if (RenderTargetLayer.IsActive<ScreenWarpTarget>())
            {
                if (Projectile.timeLeft < 60)
                {
                    Texture2D distortion = Distortion.Value;
                    const int numExtraDrawsOnEachSide = 18;
                    float segmentWidth = radius * 2 / (2 * numExtraDrawsOnEachSide - 1);
                    float heightMultiplier = 2 * Math.Min(1, Math.Min(Projectile.timeLeft / 5f, (60 - Projectile.timeLeft) / 5f));

                    Color offsetColor = new Color((float)Math.Cos(Projectile.rotation) * 0.5f * heightMultiplier / 2 + 0.5f, (float)Math.Sin(Projectile.rotation) * heightMultiplier / 2 * 0.5f + 0.5f, 0.5f) * (heightMultiplier / 4);

                    Vector2 scale = new Vector2(segmentWidth / (float)TextureAssets.Projectile[Type].Width(), heightMultiplier * Projectile.height / (float)TextureAssets.Projectile[Type].Height());
                    Vector2 drawOffset = new Vector2((Projectile.timeLeft * 36) % segmentWidth, 0).RotatedBy(Projectile.rotation);
                    Vector2 drawOffsetPer = new Vector2(segmentWidth, 0).RotatedBy(Projectile.rotation);
                    for (int i = -numExtraDrawsOnEachSide; i <= numExtraDrawsOnEachSide; i++)
                    {
                        Main.spriteBatch.Draw(distortion, Projectile.Center - Main.screenPosition + drawOffset + drawOffsetPer * i, TextureAssets.Projectile[Type].Frame(), offsetColor, Projectile.rotation, TextureAssets.Projectile[Type].Size() / 2, scale, SpriteEffects.None, 0);
                    }
                }
                return false;
            }

            Color color = Color.White;
            Texture2D texture = (Projectile.ai[1] == 0) ? Solar.Value : TextureAssets.Projectile[Type].Value;
            if (Projectile.timeLeft < 60)
            {
                //fully active
                const int numExtraDrawsOnEachSide = 18;
                float segmentWidth = radius * 2 / (2 * numExtraDrawsOnEachSide - 1);
                float heightMultiplier = Math.Min(1, Math.Min(Projectile.timeLeft / 5f, (60 - Projectile.timeLeft) / 5f));
                Vector2 scale = new Vector2(segmentWidth / (float)TextureAssets.Projectile[Type].Width(), heightMultiplier * Projectile.height / (float)TextureAssets.Projectile[Type].Height());
                Vector2 drawOffset = new Vector2((Projectile.timeLeft * 18) % segmentWidth, 0).RotatedBy(Projectile.rotation);
                Vector2 drawOffsetPer = new Vector2(segmentWidth, 0).RotatedBy(Projectile.rotation);
                for (int i = -numExtraDrawsOnEachSide; i <= numExtraDrawsOnEachSide; i++)
                {
                    Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + drawOffset + drawOffsetPer * i, TextureAssets.Projectile[Type].Frame(), color, Projectile.rotation, TextureAssets.Projectile[Type].Size() / 2, scale, SpriteEffects.None, 0);
                }
            }
            else if (Projectile.timeLeft < 90)
            {
                //telegraphing
                Vector2 scale = new Vector2(radius * 2 / (float)TextureAssets.Projectile[Type].Width(), 2 / (float)TextureAssets.Projectile[Type].Height());
                color *= (Projectile.timeLeft - 60) * (90 - Projectile.timeLeft) / 225f;
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), color, Projectile.rotation, TextureAssets.Projectile[Type].Size() / 2, scale, SpriteEffects.None, 0);
            }

            return false;
        }

        public override bool ShouldUpdatePosition() => false;
    }
    public class EclipxieOrbiter : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Glow58";

        float Radius => (float)Math.Sqrt(Projectile.ai[1] + 1) * 120;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 16;
            Projectile.height = 16;

            Projectile.timeLeft = 360;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.hide = true;

            Projectile.GetGlobalProjectile<PolaritiesProjectile>().canLeaveWorld = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            NPC owner = Main.npc[(int)Projectile.ai[0]];

            Projectile.Center = owner.Center + new Vector2(Radius, 0).RotatedByRandom(MathHelper.TwoPi);
            Projectile.localAI[0] = (Projectile.Center - owner.Center).ToRotation();

            Projectile.timeLeft = 360 + (int)Radius / 64;

            Projectile.rotation = (owner.Center - Main.LocalPlayer.Center).ToRotation();

            Projectile.localAI[1] = Main.rand.Next(2);
        }

        public override void AI()
        {
            NPC owner = Main.npc[(int)Projectile.ai[0]];

            if (!owner.active)
            {
                Projectile.Kill();
                return;
            }

            int direction = (int)owner.ai[3];
            float attackProgress = (owner.ai[1] - 60);

            Projectile.rotation += direction * attackProgress / 30000f;

            Projectile.localAI[0] += direction * attackProgress / (float)Math.Pow(Radius, 1.5f);
            Projectile.velocity = owner.Center + new Vector2(Radius, 0).RotatedBy(Projectile.localAI[0]) - Projectile.Center;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return CustomCollision.CheckAABBvDisc(targetHitbox, new Circle(Projectile.Center, Projectile.width / 2));
        }

        public override bool? CanDamage()
        {
            return (Projectile.timeLeft < 320 && Projectile.timeLeft > 10) ? null : false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            DrawLayer.AddProjectile<DrawLayerAdditiveAfterNPCs>(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.timeLeft < 360)
            {
                Texture2D spikeTexture = TextureAssets.Projectile[644].Value;
                Texture2D texture = TextureAssets.Projectile[Type].Value;
                float scale = 1f;
                if (Projectile.timeLeft < 20)
                {
                    scale = 1 - (float)Math.Pow((1 - Projectile.timeLeft / 20f), 2);
                }
                else if (Projectile.timeLeft > 330)
                {
                    const float c = 0.125f;
                    float x = (360 - Projectile.timeLeft) / 30f;
                    scale = (x - c) * (float)Math.Pow(1 - x, 2) / c + 1;
                }

                Color starColor = (Projectile.localAI[1] == 0 ? new Color(255, 224, 192) : new Color(192, 224, 255));

                Vector2 scaleMult = new Vector2(1, 1 + 0.33f * (float)Math.Sin(0.33f * Projectile.timeLeft));
                for (int i = 0; i < 3; i++)
                {
                    Main.EntitySpriteDraw(spikeTexture, Projectile.Center - Main.screenPosition, spikeTexture.Frame(), starColor * 0.75f, Projectile.rotation + i * MathHelper.Pi / 3, spikeTexture.Size() / 2, scaleMult * scale, SpriteEffects.None, 0);
                }

                //draw trail
                for (int i = 1; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i - 1] != Vector2.Zero && Projectile.oldPos[i] != Vector2.Zero)
                    {
                        Vector2 trailScale = new Vector2((Projectile.oldPos[i] - Projectile.oldPos[i - 1]).Length() / texture.Width * 4, scale * 0.125f);
                        Main.EntitySpriteDraw(texture, Projectile.oldPos[i] + Projectile.Center - Projectile.position - Main.screenPosition, texture.Frame(), starColor * (1 - i / (float)Projectile.oldPos.Length), (Projectile.oldPos[i] - Projectile.oldPos[i - 1]).ToRotation(), texture.Size() / 2, trailScale, SpriteEffects.None, 0);
                    }
                }

                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, texture.Frame(), starColor, Projectile.rotation, texture.Size() / 2, scale * 0.5f, SpriteEffects.None, 0);
            }
            return false;
        }
    }
    public class EclipxieRayStar : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Glow58";

        const int numRays = 4;
        int projectileRays = numRays; //TODO: Draw cap if this is 1
        float starRotation;
        float[] extraAI;
        float telegraphTime = 30;
        float rayTime = 60;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 36;
            Projectile.height = 36;

            Projectile.timeLeft = 90;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.hide = true;

            Projectile.GetGlobalProjectile<PolaritiesProjectile>().ForceDraw = true;
            Projectile.GetGlobalProjectile<PolaritiesProjectile>().canLeaveWorld = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            starRotation = Projectile.rotation;
            switch ((int)Projectile.ai[0])
            {
                case 1:
                    Projectile.timeLeft = 248; //I don't know why this works the way it does
                    break;
                case 2:
                    telegraphTime = 30;
                    rayTime = 120;
                    Projectile.timeLeft = (int)(30 + telegraphTime + rayTime);
                    projectileRays = 1;
                    Projectile.localAI[1] = Projectile.ai[1];
                    Projectile.ai[1] = 0;
                    NPC owner = Main.npc[(int)Projectile.localAI[1]];
                    if (!owner.active)
                    {
                        Projectile.Kill();
                        return;
                    }
                    Projectile.rotation = (Projectile.Center - owner.Center).ToRotation();
                    float expectedRotation = (Main.LocalPlayer.Center - owner.Center).ToRotation() - Projectile.velocity.X;
                    starRotation = Projectile.rotation + MathHelper.PiOver4;
                    extraAI = new float[] { Projectile.rotation, (Projectile.Center - owner.Center).Length(), Projectile.rotation - expectedRotation };
                    break;
            }

            Projectile.localAI[0] = Projectile.timeLeft;
        }

        public override void AI()
        {
            switch ((int)Projectile.ai[0])
            {
                case 1:
                    if (Projectile.timeLeft >= 90)
                    {
                        Projectile.velocity += ((Main.LocalPlayer.Center - Projectile.Center) / 2f - Projectile.velocity) / 1200f * 2f;
                        Projectile.velocity *= (float)Math.Pow((Projectile.timeLeft - 90f) / (Projectile.timeLeft - 89f), 2);
                    }
                    else if (Projectile.timeLeft < 60) Projectile.rotation += MathHelper.PiOver4 / 600 * (Projectile.ai[1] == 0 ? 1 : -1);

                    if (Projectile.timeLeft == 60 && Projectile.ai[0] != 1)
                    {
                        ParticleLayer.WarpParticles.Add(Particle.NewParticle<WarpZoomPulseParticle>(Projectile.Center, Vector2.Zero, MathHelper.Pi, 0, Scale: 0f));
                        WarpZoomWaveParticle particle = Particle.NewParticle<WarpZoomWaveParticle>(Projectile.Center, Vector2.Zero, MathHelper.Pi, 0, Scale: 0f, TimeLeft: 180);
                        ParticleLayer.WarpParticles.Add(particle);
                    }
                    break;
                case 2:
                    NPC owner = Main.npc[(int)Projectile.localAI[1]];
                    if (!owner.active)
                    {
                        Projectile.Kill();
                        return;
                    }
                    if (Projectile.timeLeft >= rayTime)
                    {
                        extraAI[0] = (Main.LocalPlayer.Center - owner.Center).ToRotation() - Projectile.velocity.X + extraAI[2];
                        Projectile.Center = new Vector2(extraAI[1], 0).RotatedBy(extraAI[0]) + owner.Center;
                    }
                    else
                    {
                        float progress = Math.Max(0, (rayTime - Projectile.timeLeft) / rayTime);
                        Projectile.Center = new Vector2(extraAI[1], 0).RotatedBy(extraAI[0] + Projectile.velocity.X * progress * progress * (3 - 2 * progress) * 0.975f) + owner.Center;
                    }
                    Projectile.rotation = (Projectile.Center - owner.Center).ToRotation();
                    starRotation = Projectile.rotation + MathHelper.PiOver4;
                    break;
            }

            switch ((int)Projectile.ai[0])
            {
                case 0:
                case 1:
                    starRotation = Projectile.rotation;
                    break;
            }
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            RenderTargetLayer.AddProjectile<ScreenWarpTarget>(index);
            DrawLayer.AddProjectile<DrawLayerAdditiveAfterNPCs>(index);
        }

        const float radius = 4000f;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.timeLeft < rayTime)
                for (int i = 0; i < projectileRays; i++)
                    if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + new Vector2(radius, 0).RotatedBy(Projectile.rotation + i * MathHelper.TwoPi / projectileRays))) return true;
            return CustomCollision.CheckAABBvDisc(targetHitbox, new Circle(Projectile.Center, 8));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float starScale = 1f;
            if (Projectile.timeLeft < 5)
            {
                starScale = 1 - (float)Math.Pow((1 - Projectile.timeLeft / 5f), 2);
            }
            else if (Projectile.timeLeft > Projectile.localAI[0] - 30)
            {
                const float c = 0.125f;
                float x = (Projectile.localAI[0] - Projectile.timeLeft) / 30f;
                starScale = (x - c) * (float)Math.Pow(1 - x, 2) / c + 1;
            }

            bool drawCap = (projectileRays == 1);

            if (RenderTargetLayer.IsActive<ScreenWarpTarget>())
            {
                if (Projectile.timeLeft < rayTime)
                {
                    for (int laserIndex = 0; laserIndex < projectileRays; laserIndex++)
                    {
                        Texture2D distortion = EclipxieRay.Distortion.Value;
                        //fully active
                        const int numDraws = 18;
                        float segmentWidth = radius / numDraws;
                        float heightMultiplier = 2 * Math.Min(1, Math.Min(Projectile.timeLeft / 5f, (rayTime - Projectile.timeLeft) / 5f));

                        Color offsetColor = new Color(-(float)Math.Cos(Projectile.rotation + laserIndex * MathHelper.TwoPi / projectileRays) * 0.5f * heightMultiplier / 2 + 0.5f, -(float)Math.Sin(Projectile.rotation + laserIndex * MathHelper.TwoPi / projectileRays) * heightMultiplier / 2 * 0.5f + 0.5f, 0.5f) * (heightMultiplier / 4);

                        Vector2 scale = new Vector2(segmentWidth / (float)TextureAssets.Projectile[Type].Width(), heightMultiplier * Projectile.height / (float)TextureAssets.Projectile[Type].Height());
                        Vector2 drawOffset = new Vector2(((360 - Projectile.timeLeft) * 36) % segmentWidth, 0).RotatedBy(Projectile.rotation + laserIndex * MathHelper.TwoPi / projectileRays);
                        Vector2 drawOffsetPer = new Vector2(segmentWidth, 0).RotatedBy(Projectile.rotation + laserIndex * MathHelper.TwoPi / projectileRays);
                        Vector2 center = new Vector2(0, distortion.Height / 2);
                        for (int i = 0; i <= numDraws; i++)
                        {
                            Main.spriteBatch.Draw(distortion, Projectile.Center - Main.screenPosition + drawOffset + drawOffsetPer * i, TextureAssets.Projectile[Type].Frame(), offsetColor, Projectile.rotation + laserIndex * MathHelper.TwoPi / projectileRays, center, scale, SpriteEffects.None, 0);
                        }
                        //extra starting draw
                        float extra = ((360 - Projectile.timeLeft) * 36) % segmentWidth / scale.X;
                        Rectangle startFrame = new Rectangle(distortion.Width - (int)extra, 0, (int)extra, distortion.Height);
                        Vector2 startOrigin = new Vector2((int)extra - extra, distortion.Height / 2);
                        Main.spriteBatch.Draw(distortion, Projectile.Center - Main.screenPosition, startFrame, offsetColor, Projectile.rotation + laserIndex * MathHelper.TwoPi / projectileRays, startOrigin, scale, SpriteEffects.None, 0);

                        if (drawCap)
                        {
                            Texture2D capTexture = EclipxieRaysBig.CapDistortion.Value;
                            Vector2 extraOffset = new Vector2(extra - (int)extra, 0).RotatedBy(Projectile.rotation + laserIndex * MathHelper.TwoPi / (int)Projectile.localAI[0]) * scale.X;
                            Main.spriteBatch.Draw(capTexture, Projectile.Center + extraOffset - Main.screenPosition, capTexture.Frame(), offsetColor, Projectile.rotation + laserIndex * MathHelper.TwoPi / (int)Projectile.localAI[0], new Vector2(capTexture.Width, capTexture.Height / 2), scale.Y, SpriteEffects.None, 0);
                        }
                    }
                }

                //anti-distortion
                Main.spriteBatch.Draw(Textures.Glow256.Value, Projectile.Center - Main.screenPosition, Textures.Glow256.Frame(), new Color(128, 128, 128), 0f, Textures.Glow256.Size() / 2, starScale * 0.5f, SpriteEffects.None, 0f);
            }
            else
            {
                //draw lasers
                for (int laserIndex = 0; laserIndex < projectileRays; laserIndex++)
                {
                    Color color = Color.White;
                    Texture2D texture = (Projectile.ai[1] == 0) ? EclipxieRay.Solar.Value : TextureAssets.Projectile[ProjectileType<EclipxieRay>()].Value;
                    if (Projectile.timeLeft < rayTime)
                    {
                        //fully active
                        const int numDraws = 18;
                        float segmentWidth = radius / numDraws;
                        float heightMultiplier = Math.Min(1, Math.Min(Projectile.timeLeft / 5f, (rayTime - Projectile.timeLeft) / 5f));
                        Vector2 scale = new Vector2(segmentWidth / (float)TextureAssets.Projectile[Type].Width(), heightMultiplier * Projectile.height / (float)TextureAssets.Projectile[Type].Height());
                        Vector2 drawOffset = new Vector2(((360 - Projectile.timeLeft) * 18) % segmentWidth, 0).RotatedBy(Projectile.rotation + laserIndex * MathHelper.TwoPi / projectileRays);
                        Vector2 drawOffsetPer = new Vector2(segmentWidth, 0).RotatedBy(Projectile.rotation + laserIndex * MathHelper.TwoPi / projectileRays);
                        Vector2 center = new Vector2(0, texture.Height / 2);
                        for (int i = 0; i <= numDraws; i++)
                        {
                            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + drawOffset + drawOffsetPer * i, TextureAssets.Projectile[Type].Frame(), color, Projectile.rotation + laserIndex * MathHelper.TwoPi / projectileRays, center, scale, SpriteEffects.None, 0);
                        }
                        //extra starting draw
                        float extra = ((360 - Projectile.timeLeft) * 18) % segmentWidth / scale.X;
                        Rectangle startFrame = new Rectangle(texture.Width - (int)extra, 0, (int)extra, texture.Height);
                        Vector2 startOrigin = new Vector2((int)extra - extra, texture.Height / 2);
                        Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, startFrame, color, Projectile.rotation + laserIndex * MathHelper.TwoPi / projectileRays, startOrigin, scale, SpriteEffects.None, 0);
                        
                        if (drawCap)
                        {
                            Texture2D capTexture = EclipxieRaysBig.CapSolar.Value;
                            Vector2 extraOffset = new Vector2(extra - (int)extra, 0).RotatedBy(Projectile.rotation + laserIndex * MathHelper.TwoPi / (int)Projectile.localAI[0]) * scale.X;
                            Main.spriteBatch.Draw(capTexture, Projectile.Center + extraOffset - Main.screenPosition, capTexture.Frame(), color, Projectile.rotation + laserIndex * MathHelper.TwoPi / (int)Projectile.localAI[0], new Vector2(capTexture.Width, capTexture.Height / 2), scale.Y, SpriteEffects.None, 0);
                        }
                    }
                    else if (Projectile.timeLeft < rayTime + telegraphTime)
                    {
                        //telegraphing
                        Vector2 scale = new Vector2(radius / (float)TextureAssets.Projectile[Type].Width(), 2 / (float)TextureAssets.Projectile[Type].Height());
                        color *= (Projectile.timeLeft - rayTime) * (rayTime + telegraphTime - Projectile.timeLeft) * 4f / (telegraphTime * telegraphTime);
                        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), color, Projectile.rotation + laserIndex * MathHelper.TwoPi / projectileRays, new Vector2(0, TextureAssets.Projectile[Type].Height() / 2), scale, SpriteEffects.None, 0);
                    }
                }

                //draw star
                Color starColor = (Projectile.ai[1] == 0 ? new Color(255, 224, 192) : new Color(192, 224, 255));

                Texture2D spikeTexture = TextureAssets.Projectile[644].Value;
                Texture2D starTexture = TextureAssets.Projectile[Type].Value;

                Vector2 scaleMult = new Vector2(1, 1 + 0.33f * (float)Math.Sin(0.33f * Projectile.timeLeft));
                for (int i = 0; i < numRays / 2; i++)
                {
                    Main.EntitySpriteDraw(spikeTexture, Projectile.Center - Main.screenPosition, spikeTexture.Frame(), starColor * 0.75f, starRotation + i * MathHelper.Pi / (numRays / 2), spikeTexture.Size() / 2, scaleMult * starScale, SpriteEffects.None, 0);
                }

                //draw trail
                if ((int)Projectile.ai[0] == 1)
                {
                    for (int i = 1; i < Projectile.oldPos.Length; i++)
                    {
                        if (Projectile.oldPos[i - 1] != Vector2.Zero && Projectile.oldPos[i] != Vector2.Zero)
                        {
                            Vector2 trailScale = new Vector2((Projectile.oldPos[i] - Projectile.oldPos[i - 1]).Length() / starTexture.Width * 4, starScale * 0.125f);
                            Main.EntitySpriteDraw(starTexture, Projectile.oldPos[i] + Projectile.Center - Projectile.position - Main.screenPosition, starTexture.Frame(), starColor * (1 - i / (float)Projectile.oldPos.Length), (Projectile.oldPos[i] - Projectile.oldPos[i - 1]).ToRotation(), starTexture.Size() / 2, trailScale, SpriteEffects.None, 0);
                        }
                    }
                }

                Main.EntitySpriteDraw(starTexture, Projectile.Center - Main.screenPosition, starTexture.Frame(), starColor, Projectile.rotation, starTexture.Size() / 2, starScale * 0.5f, SpriteEffects.None, 0);
            }
            return false;
        }

        public override bool ShouldUpdatePosition() => (int)Projectile.ai[0] == 1;
    }
    public class EclipxieRaysBig : ModProjectile
    {
        public static Asset<Texture2D> CapDistortion;
        public static Asset<Texture2D> CapSolar;
        public override void Load()
        {
            CapDistortion = Request<Texture2D>(Texture + "_CapDistortion");
            CapSolar = Request<Texture2D>(Texture + "_CapSolar");

            /*IL.Terraria.Main.UpdateMenu += Main_UpdateMenu;
		}
        private void Main_UpdateMenu(MonoMod.Cil.ILContext il)
        {
            MonoMod.Cil.ILCursor c = new MonoMod.Cil.ILCursor(il);

			c.EmitDelegate<Action>(() =>
			{
				if (!(bool)(typeof(ModLoader).GetField("isLoading", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null)))
				{
					String filePath = Main.SavePath + Path.DirectorySeparatorChar + "ModSources/Polarities/NPCs/Eclipxie/EclipxieRay_CapDistortion.png";

					if (!System.IO.File.Exists(filePath))
					{
						Terraria.Utilities.UnifiedRandom rand = new Terraria.Utilities.UnifiedRandom(278539);
						const int textureSize = 64;

						float[,] fractalNoise = rand.FractalNoise(textureSize, 8);

						Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, textureSize / 2, textureSize, false, SurfaceFormat.Color);
						System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
						for (int i = 0; i < texture.Height; i++)
						{
							for (int j = 0; j < texture.Width; j++)
                            {
                                float x = (2 * i / (float)(texture.Height - 1) - 1);
                                float y = 1 - j / (float)(texture.Width - 1);
                                float r = (float)Math.Sqrt(x * x + y * y);

                                Color baseColor = Color.Black;
                                float baseAlpha = 0;

                                if (Math.Abs(r) < 1)//0.5f)
                                {
                                    //baseAlpha = (float)Math.Sqrt(1 - 4 * r * r);// * (float)(1 + 0.5f * Math.Abs(fractalNoise[0, j]));
                                    //baseAlpha *= 1 - (1 - baseAlpha) * (fractalNoise[i, j] + 0.5f);
                                    //baseAlpha = Math.Clamp(baseAlpha, 0, 1);

                                    baseAlpha = (float)Math.Pow(1 - r * r, 2);
                                    baseAlpha *= 1 - 2 * (1 - baseAlpha) * (fractalNoise[i, j] + 0.5f);
                                    baseColor = Color.White;

                                    //baseColor = ModUtils.ConvectiveFlameColor((float)Math.Pow(baseAlpha, 3) * 0.5f);
                                    //baseColor = Color.Lerp(new Color(0, 128, 256), Color.White, (float)Math.Pow(baseAlpha, 2));
                                    //baseAlpha = (float)Math.Sqrt(baseAlpha);
                                }

                                //Color bloomColor = ModUtils.ConvectiveFlameColor((float)Math.Pow(1 - r * r, 2) * 0.5f);
                                //Color bloomColor = Color.Lerp(new Color(0, 128, 256), Color.White, 1 - r * r);
                                //float bloomAlpha = 1 - r * r;
                                //baseColor = new Color(baseColor.ToVector3() * baseAlpha + bloomColor.ToVector3() * (1 - baseAlpha));
                                //baseAlpha += bloomAlpha * (1 - baseAlpha);

								list.Add(baseColor * baseAlpha);
							}
						}
						texture.SetData(list.ToArray());
						texture.SaveAsPng(new System.IO.FileStream(filePath, System.IO.FileMode.Create), texture.Width, texture.Height);
					}
				}
			});*/
        }
        public override void Unload()
        {
            CapDistortion = null;
            CapSolar = null;
        }

        public override string Texture => "Polarities/NPCs/Eclipxie/EclipxieRay";

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 96;
            Projectile.height = 96;

            Projectile.timeLeft = 360;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.hide = true;

            Projectile.GetGlobalProjectile<PolaritiesProjectile>().ForceDraw = true;
            Projectile.GetGlobalProjectile<PolaritiesProjectile>().canLeaveWorld = true;
        }

        float rayTime = 360;
        float expansionTime = 20f;

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            switch ((int)Projectile.ai[1] / 2)
            {
                case 0:
                    Projectile.localAI[0] = 6;
                    break;
                case 1:
                    Projectile.timeLeft = 165;
                    rayTime = 165;
                    Projectile.localAI[0] = 1;
                    break;
                case 2:
                    Projectile.timeLeft = 60;
                    rayTime = 20;
                    expansionTime = 5f;
                    Projectile.localAI[0] = 10;
                    break;
            }
            Projectile.localAI[1] = Projectile.timeLeft;
        }

        public override void AI()
        {
            NPC owner = Main.npc[(int)Projectile.ai[0]];

            if (!owner.active)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = owner.Center;

            switch ((int)Projectile.ai[1] / 2)
            {
                case 0:
                    int direction = (int)owner.ai[3];
                    float attackProgress = (owner.ai[1] - 60);

                    Projectile.rotation += direction * attackProgress / 30000f;
                    break;
                case 1:
                    if (Projectile.timeLeft < Projectile.localAI[1] - 45)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center + new Vector2((float)Math.Pow(Main.rand.NextFloat(), 2) * 2000, 0).RotatedBy(Projectile.rotation), owner.velocity + new Vector2(4, 0).RotatedBy(Projectile.rotation), ProjectileType<EclipxieMeteor>(), Eclipxie.ProjectileDamage, 0f, Projectile.owner, ai0: 2, ai1: Main.rand.Next(2));
                    }
                    break;
                case 2:
                    if (Projectile.timeLeft > rayTime)
                        Projectile.rotation += (Projectile.ai[1] % 2 * 2 - 1) * 0.0002f * (Projectile.timeLeft - rayTime);
                    else if (Projectile.timeLeft == rayTime)
                    {
                        WarpZoomWaveParticle particle = Particle.NewParticle<WarpZoomWaveParticle>(Projectile.Center, Vector2.Zero, MathHelper.Pi, 0, Scale: 0f, TimeLeft: 180);
                        ParticleLayer.WarpParticles.Add(particle);
                    }
                    break;
            }
        }

        const float radius = 4000f;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float widthMultiplier = Math.Min(1, Math.Min(Projectile.timeLeft / expansionTime, (Projectile.localAI[1] - Projectile.timeLeft) / expansionTime));
            switch ((int)Projectile.ai[1] / 2)
            {
                case 1:
                    widthMultiplier *= 1.5f;
                    break;
            }
            float collisionPoint = 0f;
            for (int i = 0; i < (int)Projectile.localAI[0]; i++)
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + new Vector2(radius, 0).RotatedBy(Projectile.rotation + i * MathHelper.TwoPi / (int)Projectile.localAI[0]), widthMultiplier * 16f, ref collisionPoint)) return true;
            return false;
        }

        public override bool? CanDamage()
        {
            return (Projectile.timeLeft > expansionTime / 2 && Projectile.timeLeft < rayTime - expansionTime / 2) ? null : false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            RenderTargetLayer.AddProjectile<ScreenWarpTarget>(index);
            DrawLayer.AddProjectile<DrawLayerAfterAdditiveBeforeScreenObstruction>(index);
            DrawLayer.AddProjectile<DrawLayerAdditiveBeforeScreenObstruction>(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float baseHeightMultiplier = Math.Min(1, Math.Min(Projectile.timeLeft / expansionTime, (rayTime - Projectile.timeLeft) / expansionTime));
            bool drawCap = false;
            bool drawObstruction = true;
            switch ((int)Projectile.ai[1] / 2)
            {
                case 1:
                    drawCap = true;
                    baseHeightMultiplier *= 1.5f;
                    break;
                case 2:
                    drawObstruction = false;
                    break;
            }

            if (RenderTargetLayer.IsActive<ScreenWarpTarget>())
            {
                if (Projectile.timeLeft < rayTime)
                {
                    for (int laserIndex = 0; laserIndex < (int)Projectile.localAI[0]; laserIndex++)
                    {
                        Texture2D distortion = EclipxieRay.Distortion.Value;
                        const int numDraws = 18;
                        float segmentWidth = radius / numDraws;
                        float heightMultiplier = 2 * baseHeightMultiplier;

                        Color offsetColor = new Color(-(float)Math.Cos(Projectile.rotation + laserIndex * MathHelper.TwoPi / (int)Projectile.localAI[0]) * 0.5f * heightMultiplier / 2 + 0.5f, -(float)Math.Sin(Projectile.rotation + laserIndex * MathHelper.TwoPi / (int)Projectile.localAI[0]) * heightMultiplier / 2 * 0.5f + 0.5f, 0.5f) * (heightMultiplier / 4);

                        Vector2 scale = new Vector2(segmentWidth / (float)TextureAssets.Projectile[Type].Width(), heightMultiplier * Projectile.height / (float)TextureAssets.Projectile[Type].Height());
                        Vector2 drawOffset = new Vector2(((360 - Projectile.timeLeft) * 36) % segmentWidth, 0).RotatedBy(Projectile.rotation + laserIndex * MathHelper.TwoPi / (int)Projectile.localAI[0]);
                        Vector2 drawOffsetPer = new Vector2(segmentWidth, 0).RotatedBy(Projectile.rotation + laserIndex * MathHelper.TwoPi / (int)Projectile.localAI[0]);
                        Vector2 center = new Vector2(0, distortion.Height / 2);
                        for (int i = 0; i <= numDraws; i++)
                        {
                            Main.spriteBatch.Draw(distortion, Projectile.Center - Main.screenPosition + drawOffset + drawOffsetPer * i, TextureAssets.Projectile[Type].Frame(), offsetColor, Projectile.rotation + laserIndex * MathHelper.TwoPi / (int)Projectile.localAI[0], center, scale, SpriteEffects.None, 0);
                        }
                        //extra starting draw
                        float extra = ((360 - Projectile.timeLeft) * 36) % segmentWidth / scale.X;
                        Rectangle startFrame = new Rectangle(distortion.Width - (int)extra, 0, (int)extra, distortion.Height);
                        Vector2 startOrigin = new Vector2((int)extra - extra, distortion.Height / 2);
                        Main.spriteBatch.Draw(distortion, Projectile.Center - Main.screenPosition, startFrame, offsetColor, Projectile.rotation + laserIndex * MathHelper.TwoPi / (int)Projectile.localAI[0], startOrigin, scale, SpriteEffects.None, 0);

                        if (drawCap)
                        {
                            Texture2D capTexture = CapDistortion.Value;
                            Vector2 extraOffset = new Vector2(extra - (int)extra, 0).RotatedBy(Projectile.rotation + laserIndex * MathHelper.TwoPi / (int)Projectile.localAI[0]) * scale.X;
                            Main.spriteBatch.Draw(capTexture, Projectile.Center + extraOffset - Main.screenPosition, capTexture.Frame(), offsetColor, Projectile.rotation + laserIndex * MathHelper.TwoPi / (int)Projectile.localAI[0], new Vector2(capTexture.Width, capTexture.Height / 2), scale.Y, SpriteEffects.None, 0);
                        }
                    }

                    //anti-distortion
                    Main.spriteBatch.Draw(Textures.Glow256.Value, Projectile.Center - Main.screenPosition, Textures.Glow256.Frame(), new Color(128, 128, 128), 0f, Textures.Glow256.Size() / 2, 1.8f * baseHeightMultiplier, SpriteEffects.None, 0f);
                }
            }
            else if (DrawLayer.IsActive<DrawLayerAdditiveBeforeScreenObstruction>())
            {
                //draw lasers
                for (int laserIndex = 0; laserIndex < (int)Projectile.localAI[0]; laserIndex++)
                {
                    Color color = Color.White;
                    Texture2D texture = (Projectile.ai[1] % 2 == 0) ? EclipxieRay.Solar.Value : TextureAssets.Projectile[ProjectileType<EclipxieRay>()].Value;
                    if (Projectile.timeLeft < rayTime)
                    {
                        const int numDraws = 18;
                        float segmentWidth = radius / numDraws;
                        float heightMultiplier = baseHeightMultiplier;
                        Vector2 scale = new Vector2(segmentWidth / (float)TextureAssets.Projectile[Type].Width(), heightMultiplier * Projectile.height / (float)TextureAssets.Projectile[Type].Height());
                        Vector2 drawOffset = new Vector2(((360 - Projectile.timeLeft) * 18) % segmentWidth, 0).RotatedBy(Projectile.rotation + laserIndex * MathHelper.TwoPi / (int)Projectile.localAI[0]);
                        Vector2 drawOffsetPer = new Vector2(segmentWidth, 0).RotatedBy(Projectile.rotation + laserIndex * MathHelper.TwoPi / (int)Projectile.localAI[0]);
                        Vector2 center = new Vector2(0, texture.Height / 2);
                        for (int i = 0; i <= numDraws; i++)
                        {
                            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + drawOffset + drawOffsetPer * i, TextureAssets.Projectile[Type].Frame(), color, Projectile.rotation + laserIndex * MathHelper.TwoPi / (int)Projectile.localAI[0], center, scale, SpriteEffects.None, 0);
                        }
                        //extra starting draw
                        float extra = ((360 - Projectile.timeLeft) * 18) % segmentWidth / scale.X;
                        Rectangle startFrame = new Rectangle(texture.Width - (int)extra, 0, (int)extra, texture.Height);
                        Vector2 startOrigin = new Vector2((int)extra - extra, texture.Height / 2);
                        Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, startFrame, color, Projectile.rotation + laserIndex * MathHelper.TwoPi / (int)Projectile.localAI[0], startOrigin, scale, SpriteEffects.None, 0);

                        if (drawCap)
                        {
                            Texture2D capTexture = CapSolar.Value;
                            Vector2 extraOffset = new Vector2(extra - (int)extra, 0).RotatedBy(Projectile.rotation + laserIndex * MathHelper.TwoPi / (int)Projectile.localAI[0]) * scale.X;
                            Main.spriteBatch.Draw(capTexture, Projectile.Center + extraOffset - Main.screenPosition, capTexture.Frame(), color, Projectile.rotation + laserIndex * MathHelper.TwoPi / (int)Projectile.localAI[0], new Vector2(capTexture.Width, capTexture.Height / 2), scale.Y, SpriteEffects.None, 0);
                        }
                    }
                    else
                    {
                        //telegraphing
                        Vector2 scale = new Vector2(radius / (float)TextureAssets.Projectile[Type].Width(), 4 / (float)TextureAssets.Projectile[Type].Height());
                        color *= (Projectile.timeLeft - rayTime) * (Projectile.localAI[1] - Projectile.timeLeft) * 4f / ((Projectile.localAI[1] - rayTime) * (Projectile.localAI[1] - rayTime));
                        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), color, Projectile.rotation + laserIndex * MathHelper.TwoPi / (int)Projectile.localAI[0], new Vector2(0, TextureAssets.Projectile[Type].Height() / 2), scale, SpriteEffects.None, 0);
                    }
                }
            }
            else
            {
                if (drawObstruction)
                {
                    float heightMultiplier = baseHeightMultiplier;
                    for (int laserIndex = 0; laserIndex < (int)Projectile.localAI[0]; laserIndex++)
                    {
                        Main.spriteBatch.Draw(Textures.PixelTexture.Value, Projectile.Center - Main.screenPosition, Textures.PixelTexture.Frame(), Color.Black, Projectile.rotation + laserIndex * MathHelper.TwoPi / (int)Projectile.localAI[0], new Vector2(0, 0.5f), new Vector2(radius, 0.3f * heightMultiplier * Projectile.height), SpriteEffects.None, 0);
                    }
                }
            }
            return false;
        }

        public override bool ShouldUpdatePosition() => false;
    }
    public class EclipxieMeteor : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Glow58";

        float Radius => (float)Math.Sqrt(Projectile.ai[1] + 1) * 120;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 16;
            Projectile.height = 16;

            Projectile.timeLeft = 180;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.hide = true;

            Projectile.GetGlobalProjectile<PolaritiesProjectile>().canLeaveWorld = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            switch (Projectile.ai[0])
            {
                case 1:
                    Projectile.timeLeft = 240;
                    Projectile.localAI[0] = Projectile.velocity.Length();
                    break;
                case 2:
                    Projectile.timeLeft = 240;
                    Projectile.localAI[0] = Projectile.velocity.Length();
                    break;
                case 3:
                    Projectile.timeLeft = 243;
                    break;
            }
        }

        public override void AI()
        {
            switch (Projectile.ai[0])
            {
                case 1:
                    if (Projectile.localAI[1] == 0) Projectile.localAI[1] = Projectile.timeLeft - 60;
                    if (Projectile.timeLeft >= Projectile.localAI[1])
                    {
                        Projectile.velocity *= (Projectile.timeLeft - Projectile.localAI[1]) / (Projectile.timeLeft - Projectile.localAI[1] + 1f);
                    }
                    else if (Projectile.timeLeft < 120)
                    {
                        Projectile.velocity += (Main.LocalPlayer.Center - Projectile.Center) / 80000f * Projectile.timeLeft;
                        Projectile.velocity *= (Projectile.timeLeft) / (Projectile.timeLeft + 1f);
                    }
                    break;
                case 2:
                    Projectile.velocity *= 0.99f;
                    Projectile.velocity.Y -= 0.15f;
                    break;
                case 3:
                    Projectile.velocity += (Main.LocalPlayer.Center - Projectile.Center) / 1200f;
                    Projectile.velocity *= (Projectile.timeLeft) / (Projectile.timeLeft + 1f);
                    break;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return CustomCollision.CheckAABBvDisc(targetHitbox, new Circle(Projectile.Center, Projectile.width / 2));
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            DrawLayer.AddProjectile<DrawLayerAdditiveAfterNPCs>(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D spikeTexture = TextureAssets.Projectile[644].Value;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            float scale = 1f;
            if (Projectile.timeLeft < 20)
            {
                scale = 1 - (float)Math.Pow((1 - Projectile.timeLeft / 20f), 2);
            }

            //TODO: Gold stars should be gold
            Color starColor = (Projectile.ai[1] == 0 ? new Color(255, 224, 192) : new Color(192, 224, 255));

            Vector2 scaleMult = new Vector2(1, 1 + 0.33f * (float)Math.Sin(0.33f * Projectile.timeLeft));
            for (int i = 0; i < 3; i++)
            {
                Main.EntitySpriteDraw(spikeTexture, Projectile.Center - Main.screenPosition, spikeTexture.Frame(), starColor * 0.75f, Projectile.rotation + i * MathHelper.Pi / 3, spikeTexture.Size() / 2, scaleMult * scale, SpriteEffects.None, 0);
            }

            //draw trail
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i - 1] != Vector2.Zero && Projectile.oldPos[i] != Vector2.Zero)
                {
                    Vector2 trailScale = new Vector2((Projectile.oldPos[i] - Projectile.oldPos[i - 1]).Length() / texture.Width * 4, scale * 0.125f);
                    Main.EntitySpriteDraw(texture, Projectile.oldPos[i] + Projectile.Center - Projectile.position - Main.screenPosition, texture.Frame(), starColor * (1 - i / (float)Projectile.oldPos.Length), (Projectile.oldPos[i] - Projectile.oldPos[i - 1]).ToRotation(), texture.Size() / 2, trailScale, SpriteEffects.None, 0);
                }
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, texture.Frame(), starColor, Projectile.rotation, texture.Size() / 2, scale * 0.5f, SpriteEffects.None, 0);
            return false;
        }
    }
}
