﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Items.Placeable.Banners;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Polarities.Items.Weapons.Ranged;
using Polarities.NPCs.Esophage;
using Terraria.GameContent;

namespace Polarities.NPCs.Enemies.WorldEvilInvasion
{
	public class Uraraneid : ModNPC
	{
		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 6;

			PolaritiesNPC.npcTypeCap[Type] = 1;
			PolaritiesNPC.customNPCCapSlot[Type] = NPCCapSlotID.WorldEvilInvasion;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                //spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson,
				//flavor text
				this.TranslatedBestiaryEntry()
			});
		}

		public override void SetDefaults()
		{
			NPC.aiStyle = -1;
			NPC.width = 60;
			NPC.height = 60;
			DrawOffsetY = 25;

			NPC.defense = 28;
			NPC.damage = 50;
			NPC.lifeMax = 4200;
			NPC.knockBackResist = 0f;
			NPC.npcSlots = 1f;
			NPC.noGravity = false;
			NPC.noTileCollide = true;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = Item.buyPrice(silver: 25);

			Music = GetInstance<Biomes.WorldEvilInvasion>().Music;
			SceneEffectPriority = SceneEffectPriority.Event;

			Banner = Type;
			BannerItem = ItemType<UraraneidBanner>();

			SpawnModBiomes = new int[1] { GetInstance<Biomes.WorldEvilInvasion>().Type };
		}

		public override void AI()
		{
			NPC.noGravity = true;

			NPC.TargetClosest(true);
			Player player = Main.player[NPC.target];

			if (!PolaritiesSystem.worldEvilInvasion)
			{
				//flee if not in the invasion

				NPC.frame.X = NPC.frame.Width;
				NPC.frame.Y = NPC.frame.Height * 4;

				NPC.velocity *= 0.99f;

				NPC.velocity.Y += 0.3f;

				return;
			}

			bool groundFlag = false;

			if (NPC.velocity.Y == 0)
			{
				groundFlag = true;

				switch (NPC.ai[0])
				{
					case 0:
                        //big jump at player once
                        if (NPC.ai[1] == 20)
                        {
                            NPC.frame.X = NPC.frame.Width;
                            NPC.frame.Y = 0;
                        }

                        NPC.ai[1]++;
						if (NPC.ai[1] == 30)
						{
							NPC.velocity = new Vector2(9 * NPC.direction, -18);

							if (Main.netMode != 1)
							{
								NPC.ai[0] = Main.rand.Next(2, 4);
							}
							NPC.netUpdate = true;
							NPC.ai[1] = 0;
						}
						break;
					case 1:
                        //small jumps at player repeatedly
                        if (NPC.ai[1] % 30 == 20)
                        {
                            NPC.frame.X = NPC.frame.Width;
                            NPC.frame.Y = 0;
                        }

                        NPC.ai[1]++;
						if (NPC.ai[1] % 30 == 0)
						{
							NPC.velocity = new Vector2(7 * NPC.direction, -10);
						}
						if (NPC.ai[1] == 90)
						{
							if (Main.netMode != 1)
							{
								if (Main.rand.NextBool())
								{
									NPC.ai[0] = 0;
								}
								else
								{
									NPC.ai[0] = Main.rand.Next(2, 4);
								}
							}
							NPC.netUpdate = true;
							NPC.ai[1] = 0;
						}
						break;
					case 2:
						//shoot ichor upwards similar to esophage
						int ichorSprayPeriod = 12;
						float ichorSprayVelocity = 12;

						if (NPC.ai[1] == 30)
						{
							NPC.ai[2] = NPC.direction;
                        }
						if (NPC.ai[1] == 60)
                        {
                            NPC.frame.X = NPC.frame.Width * 2;
                            NPC.frame.Y = 0;
                        }
						if (Main.netMode != 1)
						{
							if (NPC.ai[1] >= 60)
							{
								float angle = NPC.ai[2] * 0.25f * (1 - (NPC.ai[1] - 30) / 150f);
								if (NPC.ai[1] % ichorSprayPeriod == ichorSprayPeriod - 1)
								{
									Vector2 speed = new Vector2(0, -ichorSprayVelocity).RotatedBy(angle);
									int shot = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, -NPC.height / 2), speed, ProjectileType<EsophageIchorSpray>(), 14, 3, Main.myPlayer);
									Main.projectile[shot].tileCollide = false;
								}
							}
						}

						NPC.ai[1]++;
						if (NPC.ai[1] == 210)
						{
							if (Main.netMode != 1)
							{
								NPC.ai[0] = (2 + Main.rand.Next(1, 4)) % 4;
							}
							NPC.netUpdate = true;

							NPC.ai[1] = 0;
						}
						break;
					case 3:
						//shoot ichor upwards in clump that lands on the player

						if (NPC.ai[1] == 30)
                        {
                            NPC.frame.X = 0;
                            NPC.frame.Y = 0;
                        }
						if (NPC.ai[1] == 50)
                        {
                            NPC.frame.X = NPC.frame.Width * 2;
                            NPC.frame.Y = 0;
                        }

                        NPC.ai[1]++;

                        if (NPC.ai[1] == 60)
						{
							if (Main.netMode != 1)
							{
								for (int i = 0; i < 24; i++)
								{
									Vector2 aimPosition = player.Center + new Vector2(Main.rand.NextFloat(48)).RotatedByRandom(MathHelper.TwoPi);

									NPC.direction = aimPosition.X > NPC.Center.X ? 1 : -1;

									float a = 0.0375f;
									float v = Main.rand.NextFloat(11.9f, 12.1f);
									float x = aimPosition.X - NPC.Center.X;
									float y = aimPosition.Y - NPC.position.Y;

									double theta = (new Vector2(x, y)).ToRotation();
									theta += Math.PI / 2;
									if (theta > Math.PI) { theta -= Math.PI * 2; }
									theta *= 0.5;
									theta -= Math.PI / 2;

									double num0 = -4 * Math.Pow(a, 2) * Math.Pow(x, 4) + 4 * a * Math.Pow(v, 2) * Math.Pow(x, 2) * y + Math.Pow(v, 4) * Math.Pow(x, 2);
									if (num0 > 0)
									{
										num0 = NPC.direction * Math.Sqrt(num0);
										double num1 = a * x * x - v * v * y;

										theta = -2 * Math.Atan(
											num0 / (2 * num1) +
											0.5 * Math.Sqrt(Math.Max(0,
												-(
													(num1 * (-16 * Math.Pow(v, 2) * x * (a * x * x + v * v * y) / Math.Pow(num1, 2) - 16 * Math.Pow(v, 2) * x / num1 + 8 * Math.Pow(v, 6) * Math.Pow(x, 3) / Math.Pow(num1, 3))) /
													(4 * num0)
												)
												- 2 * (a * x * x + v * v * y) / num1 + 2 + 2 * Math.Pow(v, 4) * Math.Pow(x, 2) / (num1 * num1)
											)) -
											Math.Pow(v, 2) * x / (2 * num1)
										); //some magic thingy idk

										int thetaDir = Math.Cos(theta) < 0 ? -1 : 1;
										if (thetaDir != NPC.direction) { theta -= Math.PI; }
									}

									int shot = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, -NPC.height / 2), new Vector2(v, 0).RotatedBy(theta), ProjectileType<EsophageIchorSpray>(), 14, 3, Main.myPlayer);
									Main.projectile[shot].tileCollide = false;
								}
							}
							SoundEngine.PlaySound(SoundID.NPCDeath13, NPC.Center);
						}

						if (NPC.ai[1] == 70)
                        {
                            if (Main.netMode != 1)
                            {
                                NPC.ai[0] = (3 + Main.rand.Next(1, 4)) % 4;
                            }
                            NPC.netUpdate = true;

                            NPC.ai[1] = 0;
                        }
						break;
				}
			}
			else
			{
				//while in the air
				NPC.frame.X = NPC.frame.Width;

				if (NPC.velocity.Y > 1f)
                {
					NPC.frame.Y = NPC.frame.Height * 4;
                }
                else if (NPC.velocity.Y > 0f)
                {
                    NPC.frame.Y = NPC.frame.Height * 3;
                }
                else if (NPC.velocity.Y > -1f)
                {
                    NPC.frame.Y = NPC.frame.Height * 2;
                }
				else
                {
					NPC.frame.Y = NPC.frame.Height;
                }

				NPC.rotation = -(float)Math.Atan(0.05f * NPC.velocity.Y * NPC.velocity.X) / MathHelper.TwoPi;

                NPC.velocity *= 0.99f;
			}

			NPC.velocity.Y += 0.3f;

			if (NPC.velocity.Y > 0 && Collision.TileCollision(NPC.position + new Vector2(0, 25), new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height) != new Vector2(0, NPC.velocity.Y))
			{
				NPC.velocity = Collision.TileCollision(NPC.position + new Vector2(0, 25), new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height);
				NPC.position.Y += NPC.velocity.Y;
				NPC.velocity.Y = 0;

				if (!groundFlag)
				{
					MakeDusts();
                    NPC.frame.Y = NPC.frame.Height * 5;
					NPC.rotation = 0f;
                }
			}
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Width = TextureAssets.Npc[Type].Width() / 3;

			if (NPC.frame.X == 0)
            {
				NPC.frameCounter++;
				if (NPC.frameCounter >= 5)
                {
					NPC.frameCounter = 0;
					NPC.frame.Y += NPC.frame.Height;
					if (NPC.frame.Y >= NPC.frame.Height * 6)
                    {
						NPC.frame.Y = 0;
                    }
                }

				DrawOffsetY = 25;
            }
			else if (NPC.frame.X == NPC.frame.Width)
            {
				if (NPC.frame.Y == 0 || NPC.frame.Y == 5 * NPC.frame.Height)
                {
                    DrawOffsetY = 25;
                }
				else
                {
                    DrawOffsetY = 73;
                }

				if (NPC.frame.Y == 5 * NPC.frame.Height)
				{
					NPC.frameCounter++;
					if (NPC.frameCounter >= 10)
                    {
                        NPC.frameCounter = 0;
						NPC.frame.X = 0;
						NPC.frame.Y = 0;
                    }
				}
				else
				{
					NPC.frameCounter = 0;
				}
            }
            else if (NPC.frame.X == 2 * NPC.frame.Width)
            {
                NPC.frameCounter++;
                if (NPC.frameCounter >= 5)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += NPC.frame.Height;
                    if (NPC.frame.Y >= NPC.frame.Height * 4)
                    {
                        NPC.frame.Y = 0;
                    }
                }

                DrawOffsetY = 25;
            }
        }

        private void MakeDusts()
		{
			SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/NPC_Killed_14")
			{
				Volume = 0.75f,
			}, NPC.Center);

			for (int num231 = 0; num231 < 20; num231++)
			{
				int num217 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y + NPC.height * 7 / 8), NPC.width, NPC.height / 8, DustID.Smoke, 0f, 0f, 100, default(Color), 1.5f);
				Dust dust71 = Main.dust[num217];
				Dust dust362 = dust71;
				dust362.velocity *= 1.4f;
			}
			Vector2 position67 = new Vector2(NPC.position.X + Main.rand.Next(0, NPC.width), NPC.position.Y + NPC.height);
			Vector2 val = default(Vector2);
			int num229 = Gore.NewGore(NPC.GetSource_FromAI(), position67, val, Main.rand.Next(61, 64));
			Gore gore20 = Main.gore[num229];
			Gore gore76 = gore20;
			gore76.velocity *= 0.4f;
			Main.gore[num229].velocity.X += 1f;
			Main.gore[num229].velocity.Y += 1f;
			Vector2 position68 = new Vector2(NPC.position.X + Main.rand.Next(0, NPC.width), NPC.position.Y + NPC.height);
			val = default(Vector2);
			num229 = Gore.NewGore(NPC.GetSource_FromAI(), position68, val, Main.rand.Next(61, 64));
			gore20 = Main.gore[num229];
			gore76 = gore20;
			gore76.velocity *= 0.4f;
			Main.gore[num229].velocity.X -= 1f;
			Main.gore[num229].velocity.Y += 1f;
			Vector2 position69 = new Vector2(NPC.position.X + Main.rand.Next(0, NPC.width), NPC.position.Y + NPC.height);
			val = default(Vector2);
			num229 = Gore.NewGore(NPC.GetSource_FromAI(), position69, val, Main.rand.Next(61, 64));
			gore20 = Main.gore[num229];
			gore76 = gore20;
			gore76.velocity *= 0.4f;
			Main.gore[num229].velocity.X += 1f;
			Main.gore[num229].velocity.Y -= 1f;
			Vector2 position70 = new Vector2(NPC.position.X + Main.rand.Next(0, NPC.width), NPC.position.Y + NPC.height);
			val = default(Vector2);
			num229 = Gore.NewGore(NPC.GetSource_FromAI(), position70, val, Main.rand.Next(61, 64));
			gore20 = Main.gore[num229];
			gore76 = gore20;
			gore76.velocity *= 0.4f;
			Main.gore[num229].velocity.X -= 1f;
			Main.gore[num229].velocity.Y -= 1f;
		}

		public override bool CheckDead()
		{
			for (int i = 1; i <= 4; i++)
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("UraraneidGore" + i).Type);

			for (int i = 0; i < 64; i++)
			{
				Vector2 dustPos = NPC.Center + new Vector2(Main.rand.NextFloat(28), 0).RotatedByRandom(MathHelper.TwoPi);
				Dust dust = Dust.NewDustPerfect(dustPos, DustID.Blood, Velocity: (dustPos - NPC.Center) / 12, Scale: 2f);
				dust.noGravity = true;
			}

			if (PolaritiesSystem.worldEvilInvasion)
			{
				//counts for 7 points
				PolaritiesSystem.worldEvilInvasionSize -= 7;
			}

			return true;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			//only spawns during the evil event
			if (spawnInfo.Player.InModBiome(GetInstance<Biomes.WorldEvilInvasion>()))
			{
				return Biomes.WorldEvilInvasion.GetSpawnChance(spawnInfo.Player.ZoneCrimson);
			}
			return 0f;
		}

        public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ItemType<Splattergun>(), 8));
			npcLoot.Add(ItemDropRule.Common(ItemID.Ichor, 1, 2, 5));
		}

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            //adapted from vanilla npc drawcode
            drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
            Vector2 halfSize = NPC.frame.Size() / 2;
            SpriteEffects spriteEffects = (SpriteEffects)0;
            if (NPC.spriteDirection == 1)
            {
                spriteEffects = (SpriteEffects)1;
            }

            Texture2D npcTexture = TextureAssets.Npc[Type].Value;

            int numHorizontalFrames = 3;

            Main.spriteBatch.Draw(npcTexture, new Vector2(NPC.Center.X - screenPos.X - npcTexture.Width * NPC.scale / 2f / numHorizontalFrames + halfSize.X * NPC.scale, NPC.position.Y - screenPos.Y + NPC.height - npcTexture.Height * NPC.scale / Main.npcFrameCount[Type] + 4f + halfSize.Y * NPC.scale + Main.NPCAddHeight(NPC) + NPC.gfxOffY), NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, halfSize, NPC.scale, spriteEffects, 0f);
            if (NPC.confused)
            {
                Main.spriteBatch.Draw(TextureAssets.Confuse.Value, new Vector2(NPC.position.X - screenPos.X + (float)(NPC.width / 2) - (float)TextureAssets.Npc[Type].Width() * NPC.scale / 2f / numHorizontalFrames + halfSize.X * NPC.scale, NPC.position.Y - screenPos.Y + (float)NPC.height - (float)TextureAssets.Npc[Type].Height() * NPC.scale / (float)Main.npcFrameCount[Type] + 4f + halfSize.Y * NPC.scale + Main.NPCAddHeight(NPC) - (float)TextureAssets.Confuse.Height() - 20f), (Rectangle?)new Rectangle(0, 0, TextureAssets.Confuse.Width(), TextureAssets.Confuse.Height()), new Color(250, 250, 250, 70), NPC.velocity.X * -0.05f, new Vector2((float)(TextureAssets.Confuse.Width() / 2), (float)(TextureAssets.Confuse.Height() / 2)), Main.essScale + 0.2f, (SpriteEffects)0, 0f);
            }

            return false;
        }
    }
}
