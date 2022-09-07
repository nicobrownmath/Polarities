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
using static tModPorter.ProgressUpdate;
using IL.Terraria.GameContent.NetModules;
using Polarities.Items.Weapons.Ranged;
using Terraria.Graphics.Shaders;

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

        const int ProjectileDamage = 35;

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
                            NPC.ai[1] = 0;
                            NPC.ai[0] = 1;
                        }
                        break;
                    }
                #endregion
                #region Phase 1 Dash (A generic attack for when I start creating fight structure)
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
                            NPC.ai[1] = 0;
                            NPC.ai[0] = 2;
                        }
                        break;
                    }
                #endregion
                #region Angling deathray rows (Probably generic)
                case 2:
                    {
                        const float distanceFromPlayer = 400f;
                        const int setupTime = 60;
                        const int attackTime = 240;
                        const int attackPeriod = 60;

                        //TODO: visual indicator (either silver or gold) corresponding to direction of rotation
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
                                Vector2 projectileOffset = new Vector2(0, 200).RotatedBy(NPC.ai[2] + progress * MathHelper.PiOver2 * NPC.ai[3]);
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
                            NPC.ai[1] = 0;
                            NPC.ai[0] = 3;
                        }
                        break;
                    }
                #endregion
                #region Planet Blender
                case 3:
                    {
                        //TODO: Rays should look like they're actually coming from the boss a la sun pixie's flares? Possibly should all be sunny
                        //TODO: This attack needs more windup telegraph so the player can get away from any tiles
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
                                //for (int i = 0; i < 6; i++)
                                //Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (player.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(i * MathHelper.TwoPi / 6 + MathHelper.Pi / 6), ProjectileType<EclipxieRay2>(), ProjectileDamage, 0f, Main.myPlayer, ai0: NPC.whoAmI, ai1: i);
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (player.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.Pi / 6), ProjectileType<EclipxieRaysBig>(), ProjectileDamage, 0f, Main.myPlayer, ai0: NPC.whoAmI, ai1: 0);

                                ParticleLayer.WarpParticles.Add(Particle.NewParticle<WarpZoomPulseParticle>(NPC.Center, Vector2.Zero, MathHelper.Pi, 0, Scale: 0f));
                                WarpZoomWaveParticle particle = Particle.NewParticle<WarpZoomWaveParticle>(NPC.Center, Vector2.Zero, MathHelper.Pi, 0, Scale: 0f, TimeLeft: 180);
                                ParticleLayer.WarpParticles.Add(particle);
                            }
                        }

                        NPC.ai[1]++;
                        if (NPC.ai[1] == setupTime + attackTime)
                        {
                            NPC.ai[1] = 0;
                            NPC.ai[0] = 4;
                        }
                        break;
                    }
                #endregion
                #region Circle player while bombarding with projectiles
                case 4:
                    {
                        const float distanceFromPlayer = 600f;
                        const int setupTime = 60;
                        const int attackTime = 480;
                        const int windDownTime = 90;
                        const float totalAngle = 2 * MathHelper.TwoPi;
                        
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
                            NPC.ai[1] = 0;
                            NPC.ai[0] = 5;
                        }
                        break;
                    }
                #endregion
                #region Starburst
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
                            NPC.ai[1] = 0;
                            NPC.ai[0] = 6;
                        }
                        break;
                    }
                #endregion
                #region Meteor Shower
                case 6:
                    {
                        const int setupTimePart1 = 60;
                        const int setupTime = 120;
                        const int attackTime = 510;
                        const float distanceFromPlayer = 400f;

                        //TODO: Make this move to above the player without hitting them
                        if (NPC.ai[1] < setupTimePart1)
                        {
                            float progress = NPC.ai[1] / setupTimePart1;
                            Vector2 goalPosition = player.Center + (new Vector2(0, -progress) + (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * (1 - progress)).SafeNormalize(Vector2.Zero) * distanceFromPlayer;
                            Vector2 goalVelocity = (goalPosition - NPC.Center) / Math.Max(3 * setupTimePart1 / 4 - NPC.ai[1], 1);
                            NPC.velocity += (goalVelocity - NPC.velocity) / Math.Max(setupTimePart1 / 4 - NPC.ai[1], 1);
                        }
                        else if (NPC.ai[1] < setupTimePart1)
                        {
                            //TODO: This attack needs an indicator and probably some more oomph
                            float progress = (NPC.ai[1] - setupTimePart1) / (setupTime - setupTimePart1);
                            NPC.velocity = (player.Center + new Vector2(0, -distanceFromPlayer) - NPC.Center) / Vector2.Lerp(Vector2.One, new Vector2(60, 20), progress);
                        }
                        else
                        {
                            if (NPC.ai[1] == setupTime)
                            {
                                NPC.ai[3] = player.velocity.X > 0 ? -1 : (player.velocity.X < 0 ? -1 : (Main.rand.NextBool() ? 1 : -1));
                            }

                            NPC.velocity.Y = (player.Center.Y - distanceFromPlayer - NPC.Center.Y) / 20f;
                            NPC.velocity.X = (NPC.ai[3] * 18f + (player.Center.X - NPC.Center.X) / 20f) * (NPC.ai[1] - setupTime) / attackTime;

                            if (NPC.ai[1] < setupTime + attackTime)
                                for (int side = -1; side <= 1; side += 2)
                                {
                                    Vector2 shotVelocity = new Vector2(NPC.velocity.X, 48);
                                    Vector2 shotPosition = NPC.Center + new Vector2(side * Main.rand.NextFloat(240, 1200), -shotVelocity.Y * 30);
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), shotPosition, shotVelocity, ProjectileType<EclipxieMeteor>(), ProjectileDamage, 0f, Main.myPlayer, ai1: 1);
                                }
                            if ((NPC.ai[1] - 20) % 30 == 0 && (NPC.ai[1] - 20) >= 0)
                            {
                                for (int i = 0; i < 6; i++)
                                {
                                    float shotAngle = (i + ((NPC.ai[1] - 20) - setupTime) / 60) * MathHelper.TwoPi / 6;
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(12, 0).RotatedBy(shotAngle), ProjectileType<EclipxieMeteor>(), ProjectileDamage, 0f, Main.myPlayer, ai1: 0);
                                }
                            }
                        }

                        NPC.ai[1]++;
                        if (NPC.ai[1] == setupTime + attackTime)
                        {
                            NPC.ai[1] = 0;
                            NPC.ai[0] = 7;
                        }
                        break;
                    }
                #endregion
                #region Star Pursuit
                case 7:
                    {
                        const int setupTime = 60;
                        const int attackTime = 480;
                        const float distanceFromPlayer = 600f;

                        //TODO: This attack feels boring
                        if (NPC.ai[1] < setupTime)
                        {
                            Vector2 goalPosition = (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * distanceFromPlayer + player.Center;
                            Vector2 goalVelocity = (goalPosition - NPC.Center) / (setupTime - NPC.ai[1]) * 3;
                            NPC.velocity += (goalVelocity - NPC.velocity) / Math.Max(1, setupTime - NPC.ai[1] - setupTime / 2);

                            NPC.ai[2] = 0;
                        }
                        else
                        {
                            NPC.velocity += (player.Center - NPC.Center) / 900f;
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
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(Main.rand.NextFloat(10), 0).RotatedByRandom(MathHelper.TwoPi), ProjectileType<EclipxieMeteor>(), ProjectileDamage, 0f, Main.myPlayer, ai0: 1, ai1: i);
                                }
                            }
                        }

                        NPC.ai[1]++;
                        if (NPC.ai[1] == setupTime + attackTime)
                        {
                            NPC.ai[1] = 0;
                            NPC.ai[0] = 1;
                        }
                        break;
                    }
                #endregion
                #region Deathray Sweep
                case 8:
                    {

                        break;
                    }
                #endregion
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
                if (NPC.ai[0] == 3 && NPC.ai[1] >= 60)
                {
                    spriteBatch.Draw(MoonTexture.Value, NPC.Center - screenPos, MoonTexture.Frame(), Color.Black, 0f, MoonTexture.Size() / 2, NPC.scale, SpriteEffects.None, 0f);
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

    //TODO: Cool cosmic background

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
            if (Projectile.timeLeft < 60)
            {
                Projectile.rotation += (float)Math.Atan(Projectile.ai[0] / 7.5f) / 60;
            }
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

                Vector2 scaleMult = new Vector2(1, 1 + 0.33f * (float)Math.Sin(0.33f * Projectile.timeLeft));
                for (int i = 0; i < 3; i++)
                {
                    Main.EntitySpriteDraw(spikeTexture, Projectile.Center - Main.screenPosition, spikeTexture.Frame(), new Color(224, 248, 255) * 0.75f, Projectile.rotation + i * MathHelper.Pi / 3, spikeTexture.Size() / 2, scaleMult * scale, SpriteEffects.None, 0);
                }

                //draw trail
                for (int i = 1; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i - 1] != Vector2.Zero && Projectile.oldPos[i] != Vector2.Zero)
                    {
                        Vector2 trailScale = new Vector2((Projectile.oldPos[i] - Projectile.oldPos[i - 1]).Length() / texture.Width * 4, scale * 0.125f);
                        Main.EntitySpriteDraw(texture, Projectile.oldPos[i] + Projectile.Center - Projectile.position - Main.screenPosition, texture.Frame(), new Color(224, 248, 255) * (1 - i / (float)Projectile.oldPos.Length), (Projectile.oldPos[i] - Projectile.oldPos[i - 1]).ToRotation(), texture.Size() / 2, trailScale, SpriteEffects.None, 0);
                    }
                }

                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, texture.Frame(), new Color(224, 248, 255), Projectile.rotation, texture.Size() / 2, scale * 0.5f, SpriteEffects.None, 0);
            }
            return false;
        }
    }

    public class EclipxieRayStar : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Glow58";

        const int numRays = 4;

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
            switch ((int)Projectile.ai[0])
            {
                case 1:
                    Projectile.timeLeft = 248; //I don't know why this works the way it does
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
            if (Projectile.timeLeft < 60)
                for (int i = 0; i < numRays; i++)
                    if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + new Vector2(radius, 0).RotatedBy(Projectile.rotation + i * MathHelper.TwoPi / numRays))) return true;
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

            if (RenderTargetLayer.IsActive<ScreenWarpTarget>())
            {
                if (Projectile.timeLeft < 60)
                {
                    for (int laserIndex = 0; laserIndex < numRays; laserIndex++)
                    {
                        Texture2D distortion = EclipxieRay.Distortion.Value;
                        //fully active
                        const int numDraws = 18;
                        float segmentWidth = radius / numDraws;
                        float heightMultiplier = 2 * Math.Min(1, Math.Min(Projectile.timeLeft / 5f, (60 - Projectile.timeLeft) / 5f));

                        Color offsetColor = new Color(-(float)Math.Cos(Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays) * 0.5f * heightMultiplier / 2 + 0.5f, -(float)Math.Sin(Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays) * heightMultiplier / 2 * 0.5f + 0.5f, 0.5f) * (heightMultiplier / 4);

                        Vector2 scale = new Vector2(segmentWidth / (float)TextureAssets.Projectile[Type].Width(), heightMultiplier * Projectile.height / (float)TextureAssets.Projectile[Type].Height());
                        Vector2 drawOffset = new Vector2(((360 - Projectile.timeLeft) * 36) % segmentWidth, 0).RotatedBy(Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays);
                        Vector2 drawOffsetPer = new Vector2(segmentWidth, 0).RotatedBy(Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays);
                        Vector2 center = new Vector2(0, distortion.Height / 2);
                        for (int i = 0; i <= numDraws; i++)
                        {
                            Main.spriteBatch.Draw(distortion, Projectile.Center - Main.screenPosition + drawOffset + drawOffsetPer * i, TextureAssets.Projectile[Type].Frame(), offsetColor, Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays, center, scale, SpriteEffects.None, 0);
                        }
                        //extra starting draw
                        float extra = ((360 - Projectile.timeLeft) * 36) % segmentWidth / scale.X;
                        Rectangle startFrame = new Rectangle(distortion.Width - (int)extra, 0, (int)extra, distortion.Height);
                        Vector2 startOrigin = new Vector2((int)extra - extra, distortion.Height / 2);
                        Main.spriteBatch.Draw(distortion, Projectile.Center - Main.screenPosition, startFrame, offsetColor, Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays, startOrigin, scale, SpriteEffects.None, 0);
                    }
                }

                //anti-distortion
                Main.spriteBatch.Draw(Textures.Glow256.Value, Projectile.Center - Main.screenPosition, Textures.Glow256.Frame(), new Color(128, 128, 128), 0f, Textures.Glow256.Size() / 2, starScale * 0.5f, SpriteEffects.None, 0f);
            }
            else
            {
                //draw lasers
                for (int laserIndex = 0; laserIndex < numRays; laserIndex++)
                {
                    Color color = Color.White;
                    Texture2D texture = (Projectile.ai[1] == 0) ? EclipxieRay.Solar.Value : TextureAssets.Projectile[ProjectileType<EclipxieRay>()].Value;
                    if (Projectile.timeLeft < 60)
                    {
                        //fully active
                        const int numDraws = 18;
                        float segmentWidth = radius / numDraws;
                        float heightMultiplier = Math.Min(1, Math.Min(Projectile.timeLeft / 5f, (60 - Projectile.timeLeft) / 5f));
                        Vector2 scale = new Vector2(segmentWidth / (float)TextureAssets.Projectile[Type].Width(), heightMultiplier * Projectile.height / (float)TextureAssets.Projectile[Type].Height());
                        Vector2 drawOffset = new Vector2(((360 - Projectile.timeLeft) * 18) % segmentWidth, 0).RotatedBy(Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays);
                        Vector2 drawOffsetPer = new Vector2(segmentWidth, 0).RotatedBy(Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays);
                        Vector2 center = new Vector2(0, texture.Height / 2);
                        for (int i = 0; i <= numDraws; i++)
                        {
                            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + drawOffset + drawOffsetPer * i, TextureAssets.Projectile[Type].Frame(), color, Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays, center, scale, SpriteEffects.None, 0);
                        }
                        //extra starting draw
                        float extra = ((360 - Projectile.timeLeft) * 18) % segmentWidth / scale.X;
                        Rectangle startFrame = new Rectangle(texture.Width - (int)extra, 0, (int)extra, texture.Height);
                        Vector2 startOrigin = new Vector2((int)extra - extra, texture.Height / 2);
                        Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, startFrame, color, Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays, startOrigin, scale, SpriteEffects.None, 0);
                    }
                    else if (Projectile.timeLeft < 90)
                    {
                        //telegraphing
                        Vector2 scale = new Vector2(radius / (float)TextureAssets.Projectile[Type].Width(), 2 / (float)TextureAssets.Projectile[Type].Height());
                        color *= (Projectile.timeLeft - 60) * (90 - Projectile.timeLeft) / 225f;
                        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), color, Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays, new Vector2(0, TextureAssets.Projectile[Type].Height() / 2), scale, SpriteEffects.None, 0);
                    }
                }

                //draw star
                Color starColor = (Projectile.ai[1] == 0 ? new Color(255, 248, 224) : new Color(224, 248, 255));

                Texture2D spikeTexture = TextureAssets.Projectile[644].Value;
                Texture2D starTexture = TextureAssets.Projectile[Type].Value;

                Vector2 scaleMult = new Vector2(1, 1 + 0.33f * (float)Math.Sin(0.33f * Projectile.timeLeft));
                for (int i = 0; i < numRays / 2; i++)
                {
                    Main.EntitySpriteDraw(spikeTexture, Projectile.Center - Main.screenPosition, spikeTexture.Frame(), starColor * 0.75f, Projectile.rotation + i * MathHelper.Pi / (numRays / 2), spikeTexture.Size() / 2, scaleMult * starScale, SpriteEffects.None, 0);
                }

                //draw trail
                if (Projectile.ai[0] != 0)
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

        public override bool ShouldUpdatePosition() => Projectile.ai[0] != 0;
    }

    public class EclipxieRaysBig : ModProjectile
    {
        public override string Texture => "Polarities/NPCs/Eclipxie/EclipxieRay";

        const int numRays = 6;

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

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
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
        }

        const float radius = 4000f;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (int i = 0; i < numRays; i++)
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + new Vector2(radius, 0).RotatedBy(Projectile.rotation + i * MathHelper.TwoPi / numRays))) return true;
            return false;
        }

        public override bool? CanDamage()
        {
            return (Projectile.timeLeft > 10 && Projectile.timeLeft < 350) ? null : false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            RenderTargetLayer.AddProjectile<ScreenWarpTarget>(index);
            DrawLayer.AddProjectile<DrawLayerAfterAdditiveBeforeScreenObstruction>(index);
            DrawLayer.AddProjectile<DrawLayerAdditiveBeforeScreenObstruction>(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (RenderTargetLayer.IsActive<ScreenWarpTarget>())
            {
                for (int laserIndex = 0; laserIndex < numRays; laserIndex++)
                {
                    Texture2D distortion = EclipxieRay.Distortion.Value;
                    const int numDraws = 18;
                    float segmentWidth = radius / numDraws;
                    float heightMultiplier = 2 * Math.Min(1, Math.Min(Projectile.timeLeft / 20f, (360 - Projectile.timeLeft) / 20f));

                    Color offsetColor = new Color(-(float)Math.Cos(Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays) * 0.5f * heightMultiplier / 2 + 0.5f, -(float)Math.Sin(Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays) * heightMultiplier / 2 * 0.5f + 0.5f, 0.5f) * (heightMultiplier / 4);

                    Vector2 scale = new Vector2(segmentWidth / (float)TextureAssets.Projectile[Type].Width(), heightMultiplier * Projectile.height / (float)TextureAssets.Projectile[Type].Height());
                    Vector2 drawOffset = new Vector2(((360 - Projectile.timeLeft) * 36) % segmentWidth, 0).RotatedBy(Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays);
                    Vector2 drawOffsetPer = new Vector2(segmentWidth, 0).RotatedBy(Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays);
                    Vector2 center = new Vector2(0, distortion.Height / 2);
                    for (int i = 0; i <= numDraws; i++)
                    {
                        Main.spriteBatch.Draw(distortion, Projectile.Center - Main.screenPosition + drawOffset + drawOffsetPer * i, TextureAssets.Projectile[Type].Frame(), offsetColor, Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays, center, scale, SpriteEffects.None, 0);
                    }
                    //extra starting draw
                    float extra = ((360 - Projectile.timeLeft) * 36) % segmentWidth / scale.X;
                    Rectangle startFrame = new Rectangle(distortion.Width - (int)extra, 0, (int)extra, distortion.Height);
                    Vector2 startOrigin = new Vector2((int)extra - extra, distortion.Height / 2);
                    Main.spriteBatch.Draw(distortion, Projectile.Center - Main.screenPosition, startFrame, offsetColor, Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays, startOrigin, scale, SpriteEffects.None, 0);
                }

                //anti-distortion
                Main.spriteBatch.Draw(Textures.Glow256.Value, Projectile.Center - Main.screenPosition, Textures.Glow256.Frame(), new Color(128, 128, 128), 0f, Textures.Glow256.Size() / 2, 1.8f, SpriteEffects.None, 0f);
            }
            else if (DrawLayer.IsActive<DrawLayerAdditiveBeforeScreenObstruction>())
            {
                //draw lasers
                for (int laserIndex = 0; laserIndex < numRays; laserIndex++)
                {
                    Color color = Color.White;
                    Texture2D texture = (Projectile.ai[1] == 0) ? EclipxieRay.Solar.Value : TextureAssets.Projectile[ProjectileType<EclipxieRay>()].Value;
                    const int numDraws = 18;
                    float segmentWidth = radius / numDraws;
                    float heightMultiplier = Math.Min(1, Math.Min(Projectile.timeLeft / 20f, (360 - Projectile.timeLeft) / 20f));
                    Vector2 scale = new Vector2(segmentWidth / (float)TextureAssets.Projectile[Type].Width(), heightMultiplier * Projectile.height / (float)TextureAssets.Projectile[Type].Height());
                    Vector2 drawOffset = new Vector2(((360 - Projectile.timeLeft) * 18) % segmentWidth, 0).RotatedBy(Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays);
                    Vector2 drawOffsetPer = new Vector2(segmentWidth, 0).RotatedBy(Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays);
                    Vector2 center = new Vector2(0, texture.Height / 2);
                    for (int i = 0; i <= numDraws; i++)
                    {
                        Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + drawOffset + drawOffsetPer * i, TextureAssets.Projectile[Type].Frame(), color, Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays, center, scale, SpriteEffects.None, 0);
                    }
                    //extra starting draw
                    float extra = ((360 - Projectile.timeLeft) * 18) % segmentWidth / scale.X;
                    Rectangle startFrame = new Rectangle(texture.Width - (int)extra, 0, (int)extra, texture.Height);
                    Vector2 startOrigin = new Vector2((int)extra - extra, texture.Height / 2);
                    Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, startFrame, color, Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays, startOrigin, scale, SpriteEffects.None, 0);
                }
            }
            else
            {
                float heightMultiplier = Math.Min(1, Math.Min(Projectile.timeLeft / 20f, (360 - Projectile.timeLeft) / 20f));
                for (int laserIndex = 0; laserIndex < numRays; laserIndex++)
                {
                    Main.spriteBatch.Draw(Textures.PixelTexture.Value, Projectile.Center - Main.screenPosition, Textures.PixelTexture.Frame(), Color.Black, Projectile.rotation + laserIndex * MathHelper.TwoPi / numRays, new Vector2(0, 0.5f), new Vector2(radius, 0.3f * heightMultiplier * Projectile.height), SpriteEffects.None, 0);
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
            }
        }

        public override void AI()
        {
            switch (Projectile.ai[0])
            {
                case 1:
                    if (Projectile.timeLeft >= 180)
                    {
                        Projectile.velocity *= (Projectile.timeLeft - 180) / (Projectile.timeLeft - 179f);
                    }
                    else
                    {
                        //if (Projectile.ai[1] == 1) Projectile.velocity += (Main.LocalPlayer.Center - Projectile.Center) / 28800f * Projectile.localAI[0];
                    }
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
            /*else if (Projectile.timeLeft > 330)
            {
                const float c = 0.125f;
                float x = (360 - Projectile.timeLeft) / 30f;
                scale = (x - c) * (float)Math.Pow(1 - x, 2) / c + 1;
            }*/

            Color starColor = (Projectile.ai[1] == 0 ? new Color(255, 248, 224) : new Color(224, 248, 255));

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
