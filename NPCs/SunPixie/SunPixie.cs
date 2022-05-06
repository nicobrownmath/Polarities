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
using Polarities.Items.Accessories;
using Polarities.Items.Placeable.Trophies;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent;
using ReLogic.Content;
using Polarities.Effects;
using Terraria.Localization;
using Polarities.Items.Consumables.TreasureBags;
using Polarities.Items.Placeable.Relics;
using Polarities.Items.Armor.Vanity;
using Polarities.Items.Weapons.Magic;
using Polarities.Items.Pets;
using Polarities.Biomes;
using Polarities.Items.Weapons.Melee;
using Polarities.Items.Weapons.Summon.Minions;
using Polarities.Items.Weapons.Ranged;

namespace Polarities.NPCs.SunPixie
{
	[AutoloadBossHead]
	public class SunPixie : ModNPC
	{
		private const int AttackIDLaserShots = 0;
		private const int AttackIDBrachistochroneSwoop = 1;
		private const int AttackIDFlareCharge = 2;
		private const int AttackIDCircleArena = 3;
		private const int AttackIDLaserRain = 4;
		private const int AttackIDDeathraySweep = 6;
		private const int AttackIDDeathraySpam = 7;
		private const int AttackIDLaserStorm = 8;
		private const int AttackIDLaserShotsIntense = 9;

		private const int PhaseOneArenaCooldown = 2400;
		private const int PhaseTwoArenaCooldown = 1800;
		private const int PhaseThreeArenaCooldown = 1200;
		private int ArenaCooldown = 1200;

		public override void SetStaticDefaults()
		{
			//group with other bosses
			NPCID.Sets.BossBestiaryPriority.Add(Type);

			NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
			{
				SpecificallyImmuneTo = new int[] {
					BuffID.OnFire,
					BuffID.Confused
				}
			};
			NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);

			Main.npcFrameCount[NPC.type] = 6;
			NPCID.Sets.TrailCacheLength[NPC.type] = 5;
			NPCID.Sets.TrailingMode[NPC.type] = 0;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                //spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheHallow,
				//flavor text
				this.TranslatedBestiaryEntry()
			});
		}

		public override void SetDefaults()
		{
			NPC.aiStyle = -1;
			NPC.width = 64;
			NPC.height = 64;
			NPC.defense = 25;
			NPC.damage = 54;
			NPC.lifeMax = Main.masterMode ? 51000 / 3 : Main.expertMode ? 40000 / 2 : 30000;
			NPC.knockBackResist = 0f;
			NPC.value = Item.buyPrice(0, 10, 0, 0);
			NPC.npcSlots = 15f;
			NPC.boss = true;
			NPC.lavaImmune = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.HitSound = SoundID.NPCHit5;
			NPC.DeathSound = SoundID.NPCDeath7;

			Music = MusicID.Boss5;
			if (ModLoader.HasMod("PolaritiesMusic"))
			{
				Music = MusicLoader.GetMusicSlot(ModLoader.GetMod("PolaritiesMusic"), "Sounds/Music/SunPixie");
			}

			if (Main.getGoodWorld)
            {
				NPC.scale = 1.5f;
			}

			SpawnModBiomes = new int[1] { GetInstance<HallowInvasion>().Type };
		}

		public static void SpawnOn(Player player)
        {
			NPC pixie = Main.npc[NPC.NewNPC(NPC.GetBossSpawnSource(player.whoAmI), (int)player.Center.X, (int)player.Center.Y - 1400, ModContent.NPCType<SunPixie>())];
			Main.NewText(Language.GetTextValue("Announcement.HasAwoken", pixie.TypeName), 171, 64, 255);
			SoundEngine.PlaySound(SoundID.Item, (int)player.position.X, (int)player.position.Y, 29);
		}

		public Vector2 arenaCenter;
		public float arenaRadius;

		public override void AI()
		{
			if (Main.getGoodWorld)
			{
				Lighting.AddLight(NPC.Center, 2f, 1f, 1f);

				//for the worthy hallow spreading
				if (WorldGen.AllowedToSpreadInfections && !PolaritiesSystem.disabledHallowSpread)
				{
					Point p = (NPC.Center + new Vector2(Main.rand.NextFloat(800), 0).RotatedByRandom(MathHelper.TwoPi)).ToTileCoordinates();
					WorldGen.Convert(p.X, p.Y, 2, 0);
				}
			}
			else
			{
				Lighting.AddLight(NPC.Center, 2f, 2f, 2f);
			}

			ArenaCooldown++;

			//spawn animation
			if (NPC.localAI[0] < 120)
			{
				NPC.dontTakeDamage = true;

				if (NPC.localAI[0] == 0)
				{
					NPC.velocity = new Vector2(0, 18);

					NPC.ai[1] = -1;

					//scale control
					NPC.localAI[1] = 1;
				}
				else
				{
					NPC.velocity.Y -= 0.15f;
				}

				NPC.localAI[0]++;

				return;
			}
			if (NPC.ai[0] != -1) NPC.dontTakeDamage = false;

			Player player = Main.player[NPC.target];
			if (!player.active || player.dead)
			{
				NPC.TargetClosest(false);
				player = Main.player[NPC.target];
				if (player.dead)
				{
					if (NPC.timeLeft > 10 && player.dead)
					{
						NPC.timeLeft = 10;
					}
					NPC.velocity.Y -= 0.1f;
					return;
				}
			}

			//if the player leaves the arena, trigger the arena
			if (ArenaCooldown > 188 && ArenaCooldown <= PhaseThreeArenaCooldown && player.Distance(arenaCenter) > arenaRadius)
            {
				ArenaCooldown = PhaseOneArenaCooldown + 1;
			}
			if (ArenaCooldown > PhaseThreeArenaCooldown)
            {
				arenaCenter = player.Center;
			}

			if (NPC.ai[1] == -1)
			{
				//Pick an attack at random from the list of available attacks that isn't the current attack
				List<int> possibleAttacks;
				int ThisPhaseArenaCooldown;

				if (NPC.life > NPC.lifeMax * 0.65f)
				{
					ThisPhaseArenaCooldown = PhaseOneArenaCooldown;
					possibleAttacks = new List<int>(new int[] { AttackIDLaserShots, AttackIDBrachistochroneSwoop, AttackIDFlareCharge, AttackIDLaserRain });
				}
				else if (NPC.life > NPC.lifeMax * 0.2f || !Main.expertMode)
				{
					ThisPhaseArenaCooldown = PhaseTwoArenaCooldown;
					possibleAttacks = new List<int>(new int[] { AttackIDLaserShotsIntense, AttackIDBrachistochroneSwoop, AttackIDFlareCharge, AttackIDLaserRain, AttackIDDeathraySweep });
				}
				else
				{
					ThisPhaseArenaCooldown = PhaseThreeArenaCooldown;
					possibleAttacks = new List<int>(new int[] { AttackIDLaserStorm, AttackIDDeathraySpam });
				}
				if (possibleAttacks.Contains((int)NPC.ai[0]))
				{
					possibleAttacks.Remove((int)NPC.ai[0]);
				}
				if (possibleAttacks.Contains(AttackIDLaserShotsIntense) && NPC.ai[0] == AttackIDLaserShots)
				{
					possibleAttacks.Remove(AttackIDLaserShotsIntense);
				}

				if (ArenaCooldown > ThisPhaseArenaCooldown)
				{
					NPC.ai[0] = AttackIDCircleArena;
					ArenaCooldown = 0;
				}
				else
				{
					if (Main.netMode != 1)
					{
						NPC.ai[0] = Main.rand.Next(possibleAttacks);
					}
					NPC.netUpdate = true;
				}

				NPC.ai[1] = 0;
			}

			float timeLeft;
			Vector2 goalPosition;

			switch (NPC.ai[0])
			{
				case AttackIDLaserShots:
				case AttackIDLaserShotsIntense:
					int period = 60;

					if (NPC.ai[1] % period == 0)
					{
						if (Main.netMode != 1)
						{
							bool dir = NPC.ai[2] > 0;
							if (NPC.ai[1] == 0)
							{
								dir = Main.rand.NextBool();
							}

							Vector2 targetDisplacement = new Vector2(0, -Main.rand.Next(300, 500)).RotatedByRandom(MathHelper.PiOver2);
							NPC.ai[2] = (dir ? -1 : 1) * Math.Abs(targetDisplacement.X);
							NPC.ai[3] = targetDisplacement.Y;
						}
						NPC.netUpdate = true;
					}
					if (NPC.ai[1] % period < period - 20)
					{
						timeLeft = period - 20 - (NPC.ai[1] % period);

						goalPosition = player.Center + player.velocity / 2 * timeLeft + new Vector2(NPC.ai[2], NPC.ai[3]);
						GoTowardsRadial(goalPosition, player.Center, timeLeft);
					}
					else
					{
						NPC.velocity = Vector2.Zero;
					}
					if (NPC.ai[1] % period == period - 20)
					{
						int numLasers = (NPC.ai[0] == AttackIDLaserShots) ? 8 : 12;
						if (Main.getGoodWorld) numLasers = numLasers * 3 / 2;

						SoundEngine.PlaySound(SoundID.Item, NPC.position, 33);

						if (Main.netMode != 1)
						{
							float speed = 0.05f;
							float rotationOffset = Main.rand.NextBool() ? Main.rand.NextFloat(MathHelper.TwoPi) : 0;

							for (int i = 0; i < numLasers; i++)
							{
								Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center).RotatedBy(rotationOffset + i * MathHelper.TwoPi / numLasers) * speed, ProjectileType<SunPixieRay>(), 19, 3f, Main.myPlayer, 1.1f, 0);
							}

							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<SunPixieOrb>(), 19, 3f, Main.myPlayer);
						}
					}

					NPC.ai[1]++;
					if (NPC.ai[1] == period * 4)
					{
						NPC.ai[1] = -1;
					}
					break;
				case AttackIDBrachistochroneSwoop:
					if (NPC.ai[1] < 40)
					{
						if (NPC.ai[1] == 0)
						{
							NPC.ai[2] = NPC.life > NPC.lifeMax * 0.65f ? -400 : -500;
						}

						timeLeft = 40 - NPC.ai[1];

						goalPosition = player.Center + player.velocity / 2 * timeLeft + new Vector2(0, NPC.ai[2]);
						GoTowardsRadial(goalPosition, player.Center, timeLeft);
					}
					else if (NPC.ai[1] == 40)
					{
						NPC.velocity = Vector2.Zero;
					}
					else if (NPC.ai[1] >= 60 && NPC.ai[1] < (NPC.ai[2] == -400 ? 340 : 400))
					{
						//add aura projectiles or something else here

						float g = 0.3f;

						/*need to scale NPC.velocity so that 
						minY =  - NPC.velocity.Length() * NPC.velocity.Length() / (2 * g) + NPC.Center.Y;
						equals player.Center.Y + NPC.ai[2]*/

						if (-2 * g * (player.Center.Y - NPC.Center.Y + NPC.ai[2]) > NPC.velocity.LengthSquared())
						{
							NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * (float)Math.Sqrt(-2 * g * (player.Center.Y - NPC.Center.Y + NPC.ai[2]));
						}

						float minY = player.Center.Y + NPC.ai[2];

						Vector2 goalVelocity;

						if (player.Center.X == NPC.Center.X)
						{
							goalVelocity = NPC.DirectionTo(player.Center) * NPC.velocity.Length();
						}
						else
						{
							//it isn't quite a brachistochrone, but an ellipse with axes at a ratio of π/2 to 1 is close enough and way easier to work with
							float scaleFactor = MathHelper.PiOver2;

							float centerX = (NPC.Center.X + player.Center.X) / 2;

							if (NPC.Center.Y != player.Center.Y)
							{
								float midPointX = (NPC.Center.X + player.Center.X) / 2;
								float midPointY = (NPC.Center.Y + player.Center.Y) / 2;
								float connectingLineSlope = (player.Center.Y - NPC.Center.Y) / (player.Center.X - NPC.Center.X);
								float adjustedPerpendicularLineSlope = -1 / (scaleFactor * connectingLineSlope);
								float c = midPointY - midPointX * adjustedPerpendicularLineSlope;
								centerX = (minY - c) / adjustedPerpendicularLineSlope;
							}

							goalVelocity = new Vector2((scaleFactor * (minY - NPC.Center.Y)), -(centerX - NPC.Center.X)).SafeNormalize(Vector2.Zero) * NPC.velocity.Length();

							if (NPC.Center.X < player.Center.X)
							{
								goalVelocity *= -1;
							}
						}

						float maxTurn = NPC.ai[2] == -400 ? 0.05f : 0.056f;

						if ((goalVelocity - NPC.velocity).Length() < NPC.velocity.Length() * maxTurn)
						{
							NPC.velocity = goalVelocity;
						}
						else
						{
							if (goalVelocity.X * NPC.velocity.Y - NPC.velocity.X * goalVelocity.Y > 0)
							{
								NPC.velocity = NPC.velocity.RotatedBy(-maxTurn);
							}
							else
							{
								NPC.velocity = NPC.velocity.RotatedBy(maxTurn);
							}
						}

						NPC.velocity.Y += g;
					}
					else
					{
						timeLeft = (NPC.ai[2] == -400 ? 400 : 460) - NPC.ai[1];
						goalPosition = player.Center + player.velocity / 2 * timeLeft + new Vector2(0, NPC.ai[2]);

						GoTowardsRadial(goalPosition, player.Center, timeLeft);
					}

					NPC.ai[1]++;
					if (NPC.ai[1] == (NPC.ai[2] == -400 ? 400 : 460))
					{
						NPC.ai[1] = -1;
					}
					break;
				case AttackIDCircleArena:
					if (NPC.ai[1] < 40)
					{
						if (NPC.ai[1] == 0)
						{
							NPC.ai[2] = (NPC.life > NPC.lifeMax * 0.65f ? 600 : 500);
						}

						timeLeft = 40 - NPC.ai[1];

						goalPosition = player.Center + player.velocity / 2 * timeLeft + (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * NPC.ai[2];
						GoTowardsRadial(goalPosition, player.Center, timeLeft);
					}
					else
					{
						if (NPC.ai[1] == 40)
						{
							NPC.ai[3] = (NPC.Center - player.Center).ToRotation();
						}

						float attackTime = 148;

						timeLeft = (attackTime + 40) - NPC.ai[1];

						NPC.velocity = -NPC.Center + player.Center + new Vector2(NPC.ai[2], 0).RotatedBy(NPC.ai[3] + 2 * MathHelper.TwoPi * (NPC.ai[1] - 40) / attackTime);

						if (NPC.ai[1] % 4 == 0 && NPC.ai[1] > 40)
						{
							Projectile.NewProjectile(NPC.GetSource_FromAI(), player.Center + new Vector2(NPC.ai[2], 0).RotatedBy(NPC.ai[3] + 2 * MathHelper.TwoPi * (NPC.ai[1] - 40) / attackTime), Vector2.Zero, ProjectileType<SunPixieArena>(), 25, 0f, Main.myPlayer, NPC.target, timeLeft);
						}

						if (timeLeft >= 0)
                        {
							arenaCenter = player.Center;
							arenaRadius = NPC.ai[2];
						}
					}

					NPC.ai[1]++;
					if (NPC.ai[1] == 192)
					{
						NPC.ai[1] = -1;
					}
					break;
				case AttackIDFlareCharge:
					if (NPC.ai[1] % 120 < 90)
					{
						if (NPC.ai[1] == 0)
						{
							NPC.ai[2] = NPC.life > NPC.lifeMax * 0.65f ? 0 : 1;
						}

						double amtExtra = Main.getGoodWorld ? 1.5f : 1f;

						if (NPC.ai[2] == 0)
						{
							if (NPC.ai[1] == 0)
							{
								if (Main.netMode != 1)
								{
									for (int i = 0; i < 2 * amtExtra; i++)
									{
										Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(1, 0).RotatedBy(MathHelper.PiOver4 / amtExtra + i * MathHelper.Pi / 2f / amtExtra), ProjectileType<SunPixieFlare>(), 26, 0f, Main.myPlayer, NPC.whoAmI);
									}
								}
							}
							else if (NPC.ai[1] == 120)
							{
								if (Main.netMode != 1)
								{
									for (int i = 0; i < 2 * amtExtra; i++)
									{
										Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(1, 0).RotatedBy(i * MathHelper.Pi / 2f / amtExtra), ProjectileType<SunPixieFlare>(), 26, 0f, Main.myPlayer, NPC.whoAmI);
									}
								}
							}
							else if (NPC.ai[1] == 240)
							{
								if (Main.netMode != 1)
								{
									for (int i = 0; i < 4 * amtExtra; i++)
									{
										Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(1, 0).RotatedBy(i * MathHelper.Pi / 4f / amtExtra), ProjectileType<SunPixieFlare>(), 26, 0f, Main.myPlayer, NPC.whoAmI);
									}
								}
							}
						}
						else
						{
							if (NPC.ai[1] % 120 == 0)
							{
								if (Main.netMode != 1)
								{
									for (int i = 0; i < 4 * amtExtra; i++)
									{
										Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(1, 0).RotatedBy(i * MathHelper.Pi / 4f / amtExtra), ProjectileType<SunPixieFlare>(), 26, 0f, Main.myPlayer, NPC.whoAmI);
									}
								}
							}
						}
						NPC.velocity = (player.Center - NPC.Center) / (90f - (2f * (NPC.ai[1] % 120) / 3f));
					}
					else if (NPC.ai[1] % 120 < 120)
					{
						NPC.velocity *= 0.93f;
					}
					if (NPC.ai[1] % 120 == 90)
					{
						SoundEngine.PlaySound(SoundID.Item, NPC.Center, 29);
					}

					NPC.ai[1]++;
					if (NPC.ai[1] == 360)
					{
						NPC.ai[1] = -1;
					}
					break;
				case AttackIDLaserRain:
					if (NPC.ai[1] == 0)
					{
						NPC.ai[2] = NPC.life > NPC.lifeMax * 0.65f ? 6 : 4;

						if (Main.netMode != 1)
						{
							NPC.direction = Main.rand.NextBool() ? 1 : -1;
						}
						NPC.netUpdate = true;
					}

					if (NPC.ai[1] < 40)
					{
						timeLeft = 40 - NPC.ai[1];

						goalPosition = player.Center + player.velocity / 2 * timeLeft + new Vector2(-NPC.direction * 600, -400);
						GoTowardsRadial(goalPosition, player.Center, timeLeft, 48f);
					}
					else if (NPC.ai[1] < 160)
					{
						if (NPC.ai[1] == 40)
						{
							NPC.velocity = Vector2.Zero;

							if (Main.getGoodWorld)
								NPC.ai[3] = Main.rand.NextBool() ? 1 : 3;
						}
						else
						{
							NPC.velocity.X += NPC.direction * 0.34f;
						}

						if ((NPC.ai[1] - 40) % (int)NPC.ai[2] == 0)
						{
							SoundEngine.PlaySound(SoundID.Item, NPC.position, 33);
							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 0.5f), ProjectileType<SunPixieRay>(), 19, 3f, Main.myPlayer, 1.07f, 0);
						}
						else if (Main.getGoodWorld && (NPC.ai[1] - 40) % (int)(NPC.ai[2] * 2) == NPC.ai[3] * NPC.ai[2] / 2)
                        {
							if (Main.rand.NextBool())
							{
								SoundEngine.PlaySound(SoundID.Item, NPC.position, 33);
								Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 0.5f), ProjectileType<SunPixieRay>(), 19, 3f, Main.myPlayer, 1.07f, 0);
							}
							else
                            {
								NPC.ai[3] = 4 - NPC.ai[3];
							}
						}
					}
					else
					{
						timeLeft = 190 - NPC.ai[1];

						goalPosition = player.Center + player.velocity / 2 * timeLeft + new Vector2(NPC.direction * 600, -400);
						GoTowardsRadial(goalPosition, player.Center, timeLeft, 48f);
					}

					NPC.ai[1]++;
					if (NPC.ai[1] == 190)
					{
						NPC.ai[1] = -1;
					}
					break;
				case AttackIDDeathraySweep:
					if (NPC.ai[1] < 30)
					{
						timeLeft = 30 - NPC.ai[1];

						goalPosition = player.Center + player.velocity / 2 * timeLeft + new Vector2(0, -400);
						GoTowardsRadial(goalPosition, player.Center, timeLeft, 48f);
					}
					else if (NPC.ai[1] < 60)
					{
						NPC.Center = player.Center + new Vector2(0, -400);
					}
					else
					{
						if (NPC.ai[1] == 60)
						{
							NPC.velocity = Vector2.Zero;

							SoundEngine.PlaySound(SoundID.Item, NPC.Center, 29);

							if (Main.netMode != 1)
							{
								//create sweeping laser
								float direction = NPC.Center.X > arenaCenter.X ? -1 : (NPC.Center.X < arenaCenter.X ? 1 : (Main.rand.NextBool() ? -1 : 1));

								if (!Main.getGoodWorld)
                                {
									Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center).RotatedBy(direction * MathHelper.PiOver4), ProjectileType<SunPixieDeathray>(), 26, 4f, Main.myPlayer, -direction * MathHelper.PiOver4, NPC.whoAmI);
								}
								else
                                {
									for (int i = 0; i < 6; i++)
									{
										float rotAmount = i * MathHelper.Pi / 3 + MathHelper.Pi / 6;
										rotAmount = (rotAmount + MathHelper.Pi) % MathHelper.TwoPi - MathHelper.Pi;
										float sweepAmount = ((rotAmount + MathHelper.Pi / 2 + MathHelper.Pi) % MathHelper.TwoPi - MathHelper.Pi) / 3;
										Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center).RotatedBy(direction * rotAmount), ProjectileType<SunPixieDeathray>(), 26, 4f, Main.myPlayer, -direction * sweepAmount, NPC.whoAmI);
									}
                                }
							}
						}
					}

					NPC.ai[1]++;
					if (NPC.ai[1] == 180)
					{
						NPC.ai[1] = -1;
					}
					break;
				case AttackIDLaserStorm:
					if (NPC.ai[1] == 0)
					{
						if (Main.netMode != 1)
						{
							NPC.direction = Main.rand.NextBool() ? 1 : -1;
						}
						NPC.netUpdate = true;
					}

					if (NPC.ai[1] < 40)
					{
						timeLeft = 40 - NPC.ai[1];

						goalPosition = player.Center + player.velocity / 2 * timeLeft + new Vector2(-NPC.direction * 600, -400);
						GoTowardsRadial(goalPosition, player.Center, timeLeft, 48f);
					}
					else
					{
						float time = (NPC.ai[1] - 40);

						goalPosition = player.Center + new Vector2(-NPC.direction * 600 * (float)Math.Cos(time * 0.0476096f), -400);
						NPC.velocity = (goalPosition - NPC.Center) / 5;

						if (Main.getGoodWorld)
						{
							if ((NPC.ai[1] - 40) % 3 == 1)
							{
								NPC.ai[2] = Main.rand.Next(2);
							}
							if ((NPC.ai[1] - 40) % 3 != NPC.ai[2])
							{
								SoundEngine.PlaySound(SoundID.Item, NPC.position, 33);
								Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 0.5f), ProjectileType<SunPixieRay>(), 19, 3f, Main.myPlayer, 1.07f, 0);
							}
						}
						else if ((NPC.ai[1] - 40) % 3 == 2)
						{
							SoundEngine.PlaySound(SoundID.Item, NPC.position, 33);
							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 0.5f), ProjectileType<SunPixieRay>(), 19, 3f, Main.myPlayer, 1.07f, 0);
						}
					}

					NPC.ai[1]++;
					if (NPC.ai[1] == 240)
					{
						NPC.ai[1] = -1;
					}
					break;
				case AttackIDDeathraySpam:
					if (NPC.ai[1] < 30)
					{
						timeLeft = 30 - NPC.ai[1];

						Vector2 predictedCenter = player.Center + player.velocity * timeLeft;
						goalPosition = Vector2.Lerp(predictedCenter, (arenaCenter + (arenaCenter - predictedCenter).SafeNormalize(-Vector2.UnitY) * arenaRadius), 0.75f);
						GoTowardsRadial(goalPosition, player.Center, timeLeft, 48f);
					}
					else if (NPC.ai[1] < 60)
					{
						NPC.velocity = Vector2.Zero;
					}
					else
					{
						NPC.velocity = Vector2.Zero;
						if (NPC.ai[1] == 60)
						{
							if (Main.netMode != 1)
							{
								NPC.direction = NPC.Center.X > arenaCenter.X ? -1 : (NPC.Center.X < arenaCenter.X ? 1 : (Main.rand.NextBool() ? -1 : 1));
							}
							NPC.netUpdate = true;
						}

						if (NPC.ai[1] % 20 == 0 && NPC.ai[1] < 300)
						{
							SoundEngine.PlaySound(SoundID.Item, NPC.Center, 29);

							if (Main.netMode != 1)
							{
								if (Main.getGoodWorld)
								{
									Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center).RotatedBy(0.3f * NPC.direction), ProjectileType<SunPixieDeathray>(), 26, 4f, Main.myPlayer, -NPC.direction * 0.33f, NPC.whoAmI);
									Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center).RotatedBy(-0.2f * NPC.direction), ProjectileType<SunPixieDeathray>(), 26, 4f, Main.myPlayer, -NPC.direction * 0.33f, NPC.whoAmI);

									for (int i = 0; i < 1; i++)
									{
										//create sweeping laser
										float rotation = Main.rand.NextFloat(NPC.direction == 1 ? 0.3f : 0.2f, MathHelper.TwoPi - (NPC.direction == 1 ? 0.2f : 0.3f));
										Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center).RotatedBy(rotation), ProjectileType<SunPixieDeathray>(), 26, 4f, Main.myPlayer, -NPC.direction * 0.33f, NPC.whoAmI);
									}
								}
								else
								{
									for (int i = 0; i < 3; i++)
									{
										//create sweeping laser
										float rotation = Main.rand.NextFloat(NPC.direction == 1 ? 0.3f : 0.2f, MathHelper.TwoPi - (NPC.direction == 1 ? 0.2f : 0.3f));

										Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(player.Center).RotatedBy(rotation), ProjectileType<SunPixieDeathray>(), 26, 4f, Main.myPlayer, -NPC.direction * 0.33f, NPC.whoAmI);
									}
								}
							}
						}
					}

					NPC.ai[1]++;
					if (NPC.ai[1] == 420)
					{
						NPC.ai[1] = -1;
					}
					break;
				case -1:
					//death animation
					NPC.velocity += new Vector2(Main.rand.NextFloat(1f), 0).RotatedByRandom(MathHelper.TwoPi);
					NPC.velocity *= 0.9f;
					NPC.localAI[1] *= 1 + 1f / (180f - NPC.ai[1]);

					if (NPC.ai[1] == 60)
					{
						//TODO: Maybe make gores drift
						for (int i = 1; i <= 6; i++)
						{
							Vector2 gorePosition = NPC.Center;
							Vector2 goreVelocity = NPC.velocity;
							const float goreSpeed = 8;
							switch (i)
                            {
								case 1:
									gorePosition += new Vector2(-120, -86);
									goreVelocity += new Vector2(goreSpeed, 0).RotatedBy(4 * MathHelper.Pi / 3);
									break;
								case 2:
									gorePosition += new Vector2(-108, -30);
									goreVelocity += new Vector2(goreSpeed, 0).RotatedBy(3 * MathHelper.Pi / 3);
									break;
								case 3:
									gorePosition += new Vector2(-66, 22);
									goreVelocity += new Vector2(goreSpeed, 0).RotatedBy(2 * MathHelper.Pi / 3);
									break;
								case 4:
									gorePosition += new Vector2(36, -86);
									goreVelocity += new Vector2(goreSpeed, 0).RotatedBy(5 * MathHelper.Pi / 3);
									break;
								case 5:
									gorePosition += new Vector2(52, -30);
									goreVelocity += new Vector2(goreSpeed, 0).RotatedBy(0 * MathHelper.Pi / 3);
									break;
								case 6:
									gorePosition += new Vector2(18, 22);
									goreVelocity += new Vector2(goreSpeed, 0).RotatedBy(1 * MathHelper.Pi / 3);
									break;
							}

							Gore.NewGore(NPC.GetSource_Death(), gorePosition, goreVelocity, Mod.Find<ModGore>("SunPixieGore" + i).Type);
						}
					}

					NPC.ai[1]++;
					if (NPC.ai[1] == 180)
					{
						NPC.life = 0;
						NPC.checkDead();
					}
					break;
			}
		}

		private void GoTowardsRadial(Vector2 goalPosition, Vector2 orbitPoint, float timeLeft, float maxSpeed = 24f)
		{
			float dRadial = (goalPosition - orbitPoint).Length() - (NPC.Center - orbitPoint).Length();
			float dAngle = (goalPosition - orbitPoint).ToRotation() - (NPC.Center - orbitPoint).ToRotation();
			while (dAngle > MathHelper.Pi)
			{
				dAngle -= MathHelper.TwoPi;
			}
			while (dAngle < -MathHelper.Pi)
			{
				dAngle += MathHelper.TwoPi;
			}

			NPC.velocity = (new Vector2(dRadial, dAngle * (NPC.Center - orbitPoint).Length()).RotatedBy((NPC.Center - orbitPoint).ToRotation()) + (goalPosition - NPC.Center)) / 2 / timeLeft;

			if (NPC.velocity.Length() > maxSpeed)
			{
				NPC.velocity.Normalize();
				NPC.velocity *= maxSpeed;
			}
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter++;
			if (NPC.frameCounter >= 4)
			{
				NPC.frameCounter = 0;
				NPC.frame.Y = (NPC.frame.Y + frameHeight) % (6 * frameHeight);
			}

			if (NPC.IsABestiaryIconDummy) NPC.localAI[1] = 1;
		}

		public override bool CheckDead()
		{
			if (NPC.ai[0] != -1)
			{
				NPC.ai[0] = -1;
				NPC.ai[1] = 0;
				NPC.damage = 0;
				NPC.dontTakeDamage = true;
				NPC.life = NPC.lifeMax;

				//cause attached projectiles to end
				for (int i = 0; i < Main.maxProjectiles; i++)
                {
					if (Main.projectile[i].type == ProjectileType<SunPixieArena>())
                    {
						Main.projectile[i].timeLeft -= Math.Max(0, 1000 - ArenaCooldown);
					}
					else if (Main.projectile[i].type == ProjectileType<SunPixieDeathray>())
                    {
						Main.projectile[i].timeLeft = Math.Min(Main.projectile[i].timeLeft, 10);
					}
					else if (Main.projectile[i].type == ProjectileType<SunPixieFlare>())
					{
						Main.projectile[i].Kill();
					}
				}

				return false;
			}

			if (!PolaritiesSystem.downedSunPixie)
			{
				NPC.SetEventFlagCleared(ref PolaritiesSystem.downedSunPixie, -1);
			}

			return true;
		}

        public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(new FlawlessOrRandomDropRule(ItemType<SunPixieTrophy>(), 10));
			npcLoot.Add(ItemDropRule.BossBag(ItemType<SunPixieBag>()));
			npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ItemType<SunPixieRelic>()));
			npcLoot.Add(ModUtils.MasterModeDropOnAllPlayersOrFlawless(ItemType<SunPixiePetItem>(), 4));

			//normal mode loot
			LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
			notExpertRule.OnSuccess(ItemDropRule.Common(ItemType<SunPixieMask>(), 7));
			notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.SoulofLight, 1, 10, 15));
			notExpertRule.OnSuccess(ItemDropRule.Common(ItemType<StrangeJewelry>(), 2));
			notExpertRule.OnSuccess(ItemDropRule.Common(ItemType<SolarEnergizer>(), 5));
			notExpertRule.OnSuccess(new OneFromOptionsWithCountsNotScaledWithLuckDropRule(1, 1,
				(ItemType<LaserCutter>(), 1, 1),
				(ItemType<SolarScarabStaff>(), 1, 1),
				(ItemType<Flarecaller>(), 500, 999),
				(ItemType<BlazingIre>(), 1, 1)));
			npcLoot.Add(notExpertRule);

			npcLoot.Add(ItemDropRule.ByCondition(new FlawlessDropCondition(), ItemType<RayOfSunshine>()));
		}

		public override void BossLoot(ref string name, ref int potionType)
		{
			potionType = ItemID.GreaterHealingPotion;
		}

		public static Asset<Texture2D> WingsTexture;
		public static Asset<Texture2D> FlaresTexture;
		public static Asset<Texture2D> SunglassesTexture;

		public override void Load()
        {
			WingsTexture = Request<Texture2D>(Texture + "_Wings");
			FlaresTexture = Request<Texture2D>(Texture + "_Flares");
			SunglassesTexture = Request<Texture2D>(Texture + "_Sunglasses");
		}

        public override void Unload()
        {
			WingsTexture = null;
			FlaresTexture = null;
			SunglassesTexture = null;
		}

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
		{
			if (!NPC.IsABestiaryIconDummy)
			{
				MainDraw(spriteBatch, screenPos, lightColor, DrawLayer.IsActive<DrawLayerAdditiveAfterNPCs>(), DrawLayer.IsActive<DrawLayerAfterAdditiveAfterNPCs>());
			}
			else
			{
				MainDraw(spriteBatch, screenPos, lightColor, false, false);
				NPC.SetBlendState(spriteBatch, BlendState.Additive);
				MainDraw(spriteBatch, screenPos, lightColor, true, false);
				NPC.SetBlendState(spriteBatch, BlendState.AlphaBlend);
			}

			return false;
		}

		public void MainDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor, bool additiveLayer, bool sunglassesLayer)
        {
			Texture2D texture = TextureAssets.Npc[Type].Value;
			Texture2D wingTexture = WingsTexture.Value;
			Texture2D glowTexture = Textures.Glow256.Value;
			Vector2 drawOrigin = new Vector2(146, 108);

			Color ftwColor = Color.White;
			if (Main.getGoodWorld)
            {
				ftwColor = new Color(255, 96, 96);
			}
			//death animation adjustment
			if (NPC.ai[0] == -1)
            {
				Color deathAnimColor = Color.Red;
				if (Main.getGoodWorld)
                {
					deathAnimColor = new Color(96, 96, 255);
				}
				ftwColor = Color.Lerp(ftwColor, deathAnimColor, NPC.ai[1] / 180f);
			}

			int heightOffset = 0;
			switch (NPC.frame.Y / NPC.frame.Height)
			{
				case 0:
					heightOffset = -2;
					break;
				case 1:
					heightOffset = 0;
					break;
				case 2:
					heightOffset = 0;
					break;
				case 3:
					heightOffset = -2;
					break;
				case 4:
					heightOffset = -6;
					break;
				case 5:
					heightOffset = -4;
					break;
			}
			Vector2 drawPosition = NPC.Center + new Vector2(0, heightOffset) - screenPos;

			if (sunglassesLayer)
			{
				spriteBatch.Draw(SunglassesTexture.Value, drawPosition, SunglassesTexture.Frame(), Color.White, NPC.rotation, SunglassesTexture.Size() / 2, NPC.scale, SpriteEffects.None, 0f);
				return;
			}

			if (!additiveLayer && (NPC.ai[0] != -1 || NPC.ai[1] < 60)) spriteBatch.Draw(wingTexture, drawPosition, NPC.frame, Color.Lerp(Color.White, ftwColor, 0.5f), NPC.rotation, drawOrigin, 1f, SpriteEffects.None, 0f);

			float alphaMult = (additiveLayer ? 0.25f : 1f) / (float)Math.Sqrt(NPC.localAI[1]);

			spriteBatch.Draw(glowTexture, drawPosition, glowTexture.Frame(), new Color(255, 240, 168).MultiplyRGB(ftwColor) * alphaMult, NPC.rotation, glowTexture.Frame().Size() / 2, NPC.scale * 0.5f * NPC.localAI[1], SpriteEffects.None, 0f);

			spriteBatch.Draw(texture, drawPosition, NPC.frame, ftwColor * alphaMult, NPC.rotation, drawOrigin, NPC.scale * NPC.localAI[1], SpriteEffects.None, 0f);

			if (NPC.life <= NPC.lifeMax * 0.65f)
			{
				Texture2D flareTexture = FlaresTexture.Value;
				Rectangle frame = FlaresTexture.Frame(1, 12, 0, (int)(Main.GlobalTimeWrappedHourly * 12f) % 12);
				Vector2 flareDrawOrigin = new Vector2(36, 38);

				spriteBatch.Draw(flareTexture, drawPosition, frame, ftwColor * alphaMult, NPC.rotation, flareDrawOrigin, NPC.scale * NPC.localAI[1], SpriteEffects.None, 0f);
			}
		}

		/*public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
		{
			if (!File.Exists(Main.SavePath + Path.DirectorySeparatorChar + "SunPixie.png"))
			{
				spriteBatch.End();
				spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, (Effect)null, Main.Transform);

				var capture = new RenderTarget2D(spriteBatch.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, Main.spriteBatch.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

				spriteBatch.GraphicsDevice.SetRenderTarget(capture);
				spriteBatch.GraphicsDevice.Clear(Color.Transparent);

				MainDraw(spriteBatch, screenPos, lightColor, false, false);
				NPC.SetBlendState(spriteBatch, BlendState.Additive);
				MainDraw(spriteBatch, screenPos, lightColor, true, false);
				NPC.SetBlendState(spriteBatch, BlendState.AlphaBlend);
				MainDraw(spriteBatch, screenPos, lightColor, false, true);

				spriteBatch.End();
				spriteBatch.GraphicsDevice.SetRenderTarget(null);

				var stream = File.Create(Main.SavePath + Path.DirectorySeparatorChar + "SunPixie.png");
				capture.SaveAsPng(stream, capture.Width, capture.Height);
				stream.Dispose();
				capture.Dispose();

				spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, (Effect)null, Main.Transform);
			}
		}*/

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffID.OnFire, 300, true);
		}

        public override void DrawBehind(int index)
        {
			DrawLayer.AddNPC<DrawLayerAdditiveAfterNPCs>(index);

			if (Main.LocalPlayer.head == ArmorIDs.Head.Sunglasses)
			{
				DrawLayer.AddNPC<DrawLayerAfterAdditiveAfterNPCs>(index);
			}
        }
    }

	public class SunPixieArena : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 64;
			Projectile.height = 64;
			Projectile.alpha = 0;
			Projectile.timeLeft = 1200;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.hide = true;

			if (Main.getGoodWorld) Projectile.scale = 1.5f;
		}

		private Vector2 goalDisplacement;

		public override void AI()
		{
			Player player = Main.player[(int)Projectile.ai[0]];

			if (Projectile.localAI[0] == 0)
			{
				Projectile.localAI[0] = 1;

				goalDisplacement = Projectile.Center - player.Center;
			}

			if (Projectile.ai[1] >= 0)
			{
				Vector2 goalPosition = player.Center + player.velocity / 2 * (1 + Projectile.ai[1]) + goalDisplacement;
				GoTowardsRadial(goalPosition, player.Center, 1 + Projectile.ai[1]);
			}
			else if (Projectile.ai[1] == -1)
			{
				Projectile.Center = player.Center + goalDisplacement;
				Projectile.velocity = Vector2.Zero;
			}
			else
			{
				Vector2 arenaCenter = Projectile.Center - goalDisplacement;

				if ((player.Center - arenaCenter).Length() > goalDisplacement.Length() && Projectile.velocity == Vector2.Zero)
				{
					Projectile.velocity = goalDisplacement.SafeNormalize(Vector2.Zero) * 0.1f;
				}
				else if (Projectile.velocity != Vector2.Zero)
				{
					Projectile.velocity *= 1.05f;
				}
			}

			Projectile.ai[1]--;

			Projectile.frameCounter++;

			if (Projectile.timeLeft < 30)
			{
				Projectile.hostile = false;
			}
		}

		private void GoTowardsRadial(Vector2 goalPosition, Vector2 orbitPoint, float timeLeft, float maxSpeed = 24f)
		{
			float dRadial = (goalPosition - orbitPoint).Length() - (Projectile.Center - orbitPoint).Length();
			float dAngle = (goalPosition - orbitPoint).ToRotation() - (Projectile.Center - orbitPoint).ToRotation();
			while (dAngle > MathHelper.Pi)
			{
				dAngle -= MathHelper.TwoPi;
			}
			while (dAngle < -MathHelper.Pi)
			{
				dAngle += MathHelper.TwoPi;
			}

			Projectile.velocity = (new Vector2(dRadial, dAngle * (Projectile.Center - orbitPoint).Length()).RotatedBy((Projectile.Center - orbitPoint).ToRotation()) + (goalPosition - Projectile.Center)) / 2 / timeLeft;

			if (Projectile.velocity.Length() > maxSpeed)
			{
				Projectile.velocity.Normalize();
				Projectile.velocity *= maxSpeed;
			}
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
				Color color = new Color((int)255, (int)(195 * scale + 512 * (1 - scale)), (int)(32 * scale + 512 * (1 - scale))).MultiplyRGB(ftwColor);
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
				Color color = new Color((int)255, (int)(195 * scale + 512 * (1 - scale)), (int)(32 * scale + 512 * (1 - scale))).MultiplyRGB(ftwColor);
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

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffID.OnFire, 240, true);
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
			behindNPCs.Add(index);
			DrawLayer.AddProjectile<DrawLayerAdditiveBeforeNPCs>(index);
        }
	}

	public class SunPixieOrb : ModProjectile
	{
		public override string Texture => "Polarities/NPCs/SunPixie/SunPixieArena";

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 64;
			Projectile.height = 64;
			Projectile.alpha = 0;
			Projectile.timeLeft = 75;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.hide = true;

			if (Main.getGoodWorld) Projectile.scale = 1.5f;
		}

		public override void AI()
		{
			Projectile.frameCounter++;

			if (Projectile.timeLeft < 30)
			{
				Projectile.hostile = false;
			}
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
				Color color = new Color((int)255, (int)(195 * scale + 512 * (1 - scale)), (int)(32 * scale + 512 * (1 - scale))).MultiplyRGB(ftwColor);
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
				Color color = new Color((int)255, (int)(195 * scale + 512 * (1 - scale)), (int)(32 * scale + 512 * (1 - scale))).MultiplyRGB(ftwColor);
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

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffID.OnFire, 240, true);
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCs.Add(index);
			DrawLayer.AddProjectile<DrawLayerAdditiveBeforeNPCs>(index);
		}
	}

	public class SunPixieFlare : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 22;
			Projectile.height = 22;
			DrawOffsetX = -117;

			Projectile.alpha = 0;
			Projectile.timeLeft = 120;
			Projectile.penetrate = -1;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.hide = true;

			if (Main.getGoodWorld) Projectile.scale = 1.5f;
		}

		public override void AI()
		{
			if (Projectile.localAI[0] == 0)
			{
				Projectile.localAI[0] = 1;

				Projectile.rotation = Projectile.velocity.ToRotation();
			}

			Projectile.Center = Main.npc[(int)Projectile.ai[0]].Center;

			if (!Main.npc[(int)Projectile.ai[0]].active)
			{
				Projectile.Kill();
				return;
			}

			if (Projectile.timeLeft <= 30)
			{
				Projectile.hostile = true;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float radius = (128 + (20 * Math.Min(15, (30 - Projectile.timeLeft)))) * Projectile.scale;

			return Collision.CheckAABBvLineCollision(targetHitbox.TopRight(), targetHitbox.Size(), Projectile.Center + new Vector2(radius, 0).RotatedBy(Projectile.rotation), Projectile.Center - new Vector2(radius, 0).RotatedBy(Projectile.rotation));
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float alphaMult = 0.8f;

			Color ftwColor = Color.White;
			if (Main.getGoodWorld)
			{
				ftwColor = new Color(255, 64, 64);
			}

			float radius = (128 + (20 * Math.Clamp(30 - Projectile.timeLeft, 0, 15))) * Projectile.scale;

			const int numDraws = 10;

			Color color1 = new Color(255, 240, 168).MultiplyRGB(ftwColor);
			Color color2 = new Color(255, 195, 32).MultiplyRGB(ftwColor);

			for (int i = 1; i <= numDraws; i++)
            {
				Color color = Color.Lerp(color1, color2, i / (float)numDraws);
				float alpha = (Projectile.timeLeft < 30 ?
					8f * Math.Clamp(Projectile.timeLeft / 15f, 0, 1) / numDraws :
					4f / numDraws);

				alpha *= alphaMult;

				float widthMult = 3f * (float)Math.Sqrt(1 - i / (float)numDraws);

				Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), color * alpha, Projectile.rotation, TextureAssets.Projectile[Type].Size() / 2, new Vector2(i / (float)numDraws * radius * 2 / TextureAssets.Projectile[Type].Width(), widthMult) * Projectile.scale, SpriteEffects.None, 0);
			}

			return false;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffID.OnFire, 240, true);
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCs.Add(index);

			if (Projectile.timeLeft < 30)
			{
				DrawLayer.AddProjectile<DrawLayerAdditiveAfterNPCs>(index);
			}
			else
			{
				DrawLayer.AddProjectile<DrawLayerAdditiveBeforeNPCs>(index);
			}
		}
	}

	public class SunPixieDeathray : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 12;
			Projectile.height = 12;

			Projectile.alpha = 0;
			Projectile.timeLeft = 120;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.hide = true;

			Projectile.scale = 0f;

			Projectile.GetGlobalProjectile<PolaritiesProjectile>().ForceDraw = true;
		}

		float scaleMult => Main.getGoodWorld ? 1.5f : 1f;

		public override void AI()
		{
			if (Projectile.localAI[0] == 0)
			{
				Projectile.localAI[0] = 1;

				Projectile.rotation = Projectile.velocity.ToRotation();
			}

			if (Projectile.timeLeft > 110)
			{
				Projectile.scale += 0.1f * scaleMult;
			}
			else if (Projectile.timeLeft < 10)
			{
				Projectile.scale -= 0.1f * scaleMult;
			}

			Projectile.rotation += Projectile.ai[0] * (float)Math.Sin(MathHelper.Pi * (120 - Projectile.timeLeft) / 120f) / 120f * MathHelper.Pi;

			Projectile.Center = Main.npc[(int)Projectile.ai[1]].Center;

			if (!Main.npc[(int)Projectile.ai[1]].active)
			{
				Projectile.Kill();
				return;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float radius = 2000;

			return Collision.CheckAABBvLineCollision(targetHitbox.TopRight(), targetHitbox.Size(), Projectile.Center, Projectile.Center + new Vector2(radius, 0).RotatedBy(Projectile.rotation));
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Projectile.scale <= 0)
			{
				return false;
			}

			Color ftwColor = Color.White;
			if (Main.getGoodWorld)
			{
				ftwColor = new Color(255, 96, 96);
			}

			float alphaMult = 1f;

			float radius = 2000;

			Vector2 startPos = Projectile.Center;
			Vector2 drawCurrentSegmentPos = startPos;
			Vector2 goalPos = Projectile.Center + new Vector2(radius, 0).RotatedBy(Projectile.rotation);
			float segmentLength = 12 * Projectile.scale;

			int steps = 0;

			while ((drawCurrentSegmentPos - goalPos).Length() > segmentLength)
			{
				drawCurrentSegmentPos = startPos + steps * segmentLength * (goalPos - startPos).SafeNormalize(Vector2.Zero);

				float numLayers = 8;

				for (int i = 0; i < numLayers; i++)
				{
					float stepValue = 1 - 32f / (steps + 32f);
					float colorAmount = (numLayers - i + numLayers * stepValue) / (2 * numLayers);

					Color color = new Color((int)(255 * colorAmount + 255 * (1 - colorAmount)), (int)(220 * colorAmount + 255 * (1 - colorAmount)), (int)(64 * colorAmount + 255 * (1 - colorAmount))).MultiplyRGB(ftwColor);
					float alpha = 0.3f * (1 - 6f / (steps + 6f)) * alphaMult;
					float scale = 1f;

					Vector2 positionOffset = new Vector2(Projectile.scale * 4, 0).RotatedBy(i * MathHelper.TwoPi / numLayers + Projectile.timeLeft * 0.1f);

					Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, positionOffset + drawCurrentSegmentPos - Main.screenPosition, new Rectangle(0, 0, 12, 12), color * alpha, Projectile.rotation, new Vector2(6, 6), Projectile.scale * scale, SpriteEffects.None, 0);
				}

				steps++;
			}

			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCs.Add(index);
			DrawLayer.AddProjectile<DrawLayerAdditiveAfterNPCs>(index);
		}

        public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffID.OnFire, 240, true);
		}

		public override bool ShouldUpdatePosition() => false;
	}

	public class SunPixieRay : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 10;
			Projectile.height = 10;
			DrawOffsetX = -54;
			DrawOriginOffsetY = 0;
			DrawOriginOffsetX = 27;
			Projectile.alpha = 0;
			Projectile.timeLeft = 600;
			Projectile.penetrate = 1;
			Projectile.hostile = true;
			Projectile.tileCollide = Projectile.ai[0] == 0;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.hide = true;

			if (Main.getGoodWorld) Projectile.scale = 1.5f;
		}

		public override void AI()
		{
			if (Projectile.timeLeft % 8 == 7)
			{
				Dust.NewDustPerfect(Projectile.Center, Main.getGoodWorld ? 130 : 133, Vector2.Zero, Scale: 1).noGravity = true;
			}

			Projectile.rotation = Projectile.velocity.ToRotation();
			Projectile.velocity *= Projectile.ai[0]; //1 or 1.05f
			if (Projectile.velocity.Length() > 32)
			{
				Projectile.velocity.Normalize();
				Projectile.velocity *= 32;
			}
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffID.OnFire, 180, true);
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCs.Add(index);
			DrawLayer.AddProjectile<DrawLayerAdditiveAfterNPCs>(index);
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 8; i++)
			{
				Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Main.getGoodWorld ? 130 : 133, Scale: 1)].noGravity = true;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float alphaMult = DrawLayer.IsActive<DrawLayerAdditiveAfterNPCs>() ? 0.25f : 1f;

			Color ftwColor = Color.White;
			if (Main.getGoodWorld)
			{
				ftwColor = new Color(255, 96, 96);
			}

			Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 10), ftwColor * alphaMult, Projectile.rotation, new Vector2(59, 5), Projectile.scale, SpriteEffects.None, 0);
			return false;
		}
	}
}