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
using Polarities.Tiles;
using Polarities.Items.Weapons;
using Polarities.Items.Armor;
using Polarities.Items.Placeable.Banners;
using Terraria.GameContent.Bestiary;
using Polarities.Biomes;
using Polarities.Items.Placeable.Blocks;
using Terraria.Audio;
using Polarities.Items.Placeable.Walls;
using Terraria.GameContent.ItemDropRules;
using Polarities.Items.Consumables;
using Terraria.GameContent;
using ReLogic.Content;
using Polarities.Items.Accessories;
using Terraria.DataStructures;
using Polarities.Items.Materials;
using Terraria.ModLoader.Utilities;
using System.Collections.Generic;
using Polarities.Effects;
using MultiHitboxNPCLibrary;

namespace Polarities.NPCs.ConvectiveWanderer
{
	[AutoloadBossHead]
	public class ConvectiveWanderer : ModNPC, IMultiHitboxSegmentUpdate
	{
		#region Bestiary/Wiki image generation
		/*public override void Load()
		{
			IL.Terraria.Main.UpdateMenu += Main_UpdateMenu;
		}

		private void Main_UpdateMenu(MonoMod.Cil.ILContext il)
		{
			MonoMod.Cil.ILCursor c = new MonoMod.Cil.ILCursor(il);

			c.EmitDelegate<Action>(() =>
			{
				if (!(bool)(typeof(ModLoader).GetField("isLoading", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null)))
				{
					String filePath = Main.SavePath + Path.DirectorySeparatorChar + "ConvectiveWanderer.png";

					if (!File.Exists(filePath))
					{
						Main.spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Matrix.CreateTranslation(0f - Main.GameViewMatrix.Translation.X, 0f - Main.GameViewMatrix.Translation.Y, 0f) * Matrix.CreateScale(0.5f, 0.5f, 1f));

						const int extraWidth = 2400;

						var capture = new RenderTarget2D(Main.spriteBatch.GraphicsDevice, Main.screenWidth + extraWidth, Main.screenHeight, false, Main.spriteBatch.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
						var capture2 = new RenderTarget2D(Main.spriteBatch.GraphicsDevice, Main.screenWidth + extraWidth, Main.screenHeight, false, Main.spriteBatch.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

						Main.spriteBatch.GraphicsDevice.SetRenderTarget(capture);
						Main.spriteBatch.GraphicsDevice.Clear(Color.Transparent);

						NPC me = new NPC();
						me.SetDefaults(NPCType<ConvectiveWanderer>());
						me.IsABestiaryIconDummy = false;
						me.Center = new Vector2(extraWidth / 2, 0);
						me.rotation = 0f;

						(me.ModNPC as ConvectiveWanderer).segmentPositions[0] = me.Center;
						for (int i = 1; i < (me.ModNPC as ConvectiveWanderer).segmentPositions.Length; i++)
						{
							//TODO: Set segmnt angles and pulse scales for this when supported
							float rotationOffset = (float)Math.Cos(i * MathHelper.TwoPi / ((me.ModNPC as ConvectiveWanderer).segmentPositions.Length - 16));
							(me.ModNPC as ConvectiveWanderer).segmentPositions[i] = (me.ModNPC as ConvectiveWanderer).segmentPositions[i - 1] - new Vector2(segmentSeparation, 0).RotatedBy(me.rotation + rotationOffset);
						}

						me.ModNPC.PreDraw(Main.spriteBatch, -capture.Size() / 2, Color.White);

						Main.spriteBatch.End();
						Main.spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);

						Main.spriteBatch.GraphicsDevice.SetRenderTarget(capture2);
						Main.spriteBatch.GraphicsDevice.Clear(Color.Transparent);

						for (int i = 0; i < 4; i++)
						{
							Main.spriteBatch.Draw(capture, Main.GameViewMatrix.Translation + new Vector2(2f, 0).RotatedBy(i * MathHelper.PiOver2), null, new Color(64, 64, 64), 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
						}
						Main.spriteBatch.Draw(capture, Main.GameViewMatrix.Translation, null, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);

						Main.spriteBatch.End();
						Main.spriteBatch.GraphicsDevice.SetRenderTarget(null);


						var stream = File.Create(filePath);
						capture2.SaveAsPng(stream, capture2.Width, capture2.Height);
						stream.Dispose();
						capture2.Dispose();
					}
				}
			});
		}*/
		#endregion

		#region Setup
		public override void SetStaticDefaults()
		{
			//group with other bosses
			NPCID.Sets.BossBestiaryPriority.Add(Type);

			NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
			{
				SpecificallyImmuneTo = new int[] {
					BuffID.Confused,
					BuffID.OnFire,
					BuffID.Frostburn,
					BuffID.OnFire3,
					BuffID.ShadowFlame,
					BuffID.CursedInferno,
					BuffType<Incinerating>()
				}
			};
			NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheUnderworld,
				//flavor text
				this.TranslatedBestiaryEntry()
			});
		}

		public override void SetDefaults()
		{
			NPC.aiStyle = -1;

			NPC.width = 72;
			NPC.height = 72;

			NPC.defense = 25;
			NPC.damage = 50;
			NPC.knockBackResist = 0f;
			NPC.lifeMax = Main.masterMode ? 168000 / 3 : Main.expertMode ? 136000 / 2 : 100000;
			NPC.noTileCollide = true;
			NPC.noGravity = true;
			NPC.lavaImmune = true;
			NPC.npcSlots = 1f;
			NPC.boss = true;
			NPC.behindTiles = false;
			NPC.value = Item.sellPrice(gold: 15);
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;

			NPC.hide = true;

			SpawnModBiomes = new int[1] { GetInstance<LavaOcean>().Type };

			numSegments = 40;
			segmentPositions = new Vector2[numSegments * segmentsPerHitbox + segmentsTailTendrils + 3];
			segmentAngles = new float[segmentPositions.Length];
			segmentPulseScaleAngles = new float[segmentPositions.Length + specialSegmentsHead];
			segmentHeats = new float[segmentPositions.Length];
			for (int i = 0; i < segmentHeats.Length; i++)
            {
				segmentHeats[i] = 1f;
            }

			drawDatas = new PriorityQueue<DrawData, float>(MAX_DRAW_CAPACITY);
		}

        int numSegments;

		const int HITBOXES_PER_TENTACLE = 16;

		const int segmentsPerHitbox = 4;
		const int specialSegmentsHeadMultiplier = 8;
		const int specialSegmentsHead = 8 * specialSegmentsHeadMultiplier - 2;
		const int hitboxSegmentOffset = 2;
		const int segmentsTail = 8;
		const int segmentsTailTendrils = 8;
		const float segmentSeparation = 16f;

		private Vector2[] segmentPositions;
		private float[] segmentAngles;
		private float[] segmentPulseScaleAngles;
		private float[] segmentHeats;

		float tendrilOutwardness;
		float upDashTelegraphProgress;

		bool inPhase2 = false;
		#endregion

		#region AI
		public override void BossHeadRotation(ref float rotation)
		{
			rotation = SegmentRotation(0);
		}

		public override void OnSpawn(IEntitySource source)
		{
			Player player = Main.player[NPC.target];
			if (!player.active || player.dead)
			{
				NPC.TargetClosest(false);
				player = Main.player[NPC.target];
			}

			NPC.rotation = (player.Center - NPC.Center).ToRotation();

			segmentPositions[0] = NPC.Center;
			for (int i = 1; i < segmentPositions.Length; i++)
			{
				segmentPositions[i] = segmentPositions[i - 1] - new Vector2(NPC.width, 0).RotatedBy(NPC.rotation);
			}
		}

        public override void AI()
		{
			Player player = Main.player[NPC.target];
			if (!player.active || player.dead)
			{
				NPC.TargetClosest(false);
				player = Main.player[NPC.target];
			}

			//changeable ai values
			float rotationFade = 9f;
			float rotationAmount = 0.01f;
			float angleLerpAmount = 0.5f;
			float pulseLerpAmount = 0.1f;
			float angleSpeed = 0f;
			float pulseSpeed = MathHelper.TwoPi / 120f;
			bool gotoNextAttack = false;

			//drawcode values
			tendrilOutwardness = 0f;
			upDashTelegraphProgress = 0f;

			NPC.noGravity = true;

			#region Main AI

			float headRotation = SegmentRotation(0);

			switch (NPC.ai[0])
			{
				case 0:
					//TODO: spawn animation
					NPC.ai[0] = 1;
					break;

                #region Head Swing Attack
                case 1:
					{
						const int attackRepetitions = 4;
						const int attackFirstSetupExtraTime = 60;
						const int attackSetupTime = 60;
						const int attackFreezeTime = 13;
						const int attackSwingTime = 20;
						const int totalAttackTime = attackSetupTime + attackFreezeTime + attackSwingTime;

						//TODO: Tentacle twisting to correspond to the goal direction (this should also exist for the other attacks)
						//TODO: Better visual cues (both for charging time and for freeze)
						//TODO: Prim trail for swing?
						float attackProgress = (int)(NPC.ai[1] - attackFirstSetupExtraTime) % totalAttackTime;

						bool playerAhead = Vector2.Dot(NPC.Center - player.Center, new Vector2(-1, 0).RotatedBy(headRotation)) > 0;
						int side = Vector2.Dot(NPC.Center - player.Center, new Vector2(0, -1).RotatedBy(headRotation)) > 0 ? 1 : -1;

						const float goalDistance = 113f; //160f;

						if (attackProgress < 0)
						{
							float timeLeft = attackSetupTime - attackProgress;

							//extra preparation on the first occurence
							const float goalSetupDistance = 160f;
							//otherAngle = Asin(goalDistance / distToPlayer)
							//heightAtIntersect = sin(otherAngle) * distAlongLine
							//distAlongLine = distToInnerCircle - interDist
							//distToInnerCircle = distToPlayer * cos(otherAngle)
							//interDist = sqrt(goalSetupDistance ^ 2 - goalDistance ^ 2)
							//angleOffset = Asin(heightAtIntersect / goalSetupDistance)

							//the angle offset from the player at which the tangent to the goal distance circle from the npc center intersects the setup goal distance circle
							float angleOffset = (NPC.Center - player.Center).Length() <= goalSetupDistance ? 0 :
								(float)Math.Asin(goalDistance / (NPC.Center - player.Center).Length() * ((NPC.Center - player.Center).Length() * (float)Math.Sqrt(1 - Math.Pow(goalDistance / (NPC.Center - player.Center).Length(), 2)) - (float)Math.Sqrt(goalSetupDistance * goalSetupDistance - goalDistance * goalDistance)) / goalSetupDistance);
								;

							Vector2 goalPosition = player.Center + (NPC.Center - player.Center).SafeNormalize(Vector2.Zero).RotatedBy(angleOffset * side) * goalSetupDistance;
							Vector2 goalVelocity = (goalPosition - NPC.Center) / timeLeft;
							NPC.velocity += (goalVelocity - NPC.velocity) / (float)Math.Pow(timeLeft, 0.25f);

							angleSpeed = NPC.velocity.Length() * 0.003f;
						}
						else if (attackProgress < attackSetupTime)
						{
							//if the player is ahead of us, chase them, else, try braking and turning around
							if (playerAhead)
							{
								/*Vector2 goalPosition = player.Center + (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * goalDistance;
								float goalRotation = (NPC.Center - player.Center).ToRotation() + MathHelper.PiOver4 * side;

								float timeLeft = attackSetupTime - attackProgress;

								float velocityMultiplier = (goalPosition - NPC.Center).Length() / (float)Math.Sqrt(timeLeft);
								Vector2 goalVelocity = ((goalPosition - NPC.Center).SafeNormalize(Vector2.Zero) + new Vector2(0.5f, 0).RotatedBy(goalRotation)) * velocityMultiplier;

								Vector2 testVelocity = NPC.velocity + (goalVelocity - NPC.velocity) / (float)Math.Sqrt(timeLeft);*

								//don't risk turning away from the player if we're close enough
								if ((NPC.Center - player.Center).Length() < goalDistance)
								{
									NPC.velocity *= 0.95f;
								}
								else
								{
									NPC.velocity = testVelocity;
								}*/

								const float goalSetupDistance = 160f;

								float timeLeft = attackSetupTime - attackProgress;

								if ((NPC.Center - player.Center).Length() > goalSetupDistance)
								{
									//the angle offset from the player at which the tangent to the goal distance circle from the npc center intersects the setup goal distance circle
									float angleOffset = (NPC.Center - player.Center).Length() <= goalSetupDistance ? 0 :
										(float)Math.Asin(goalDistance / (NPC.Center - player.Center).Length() * ((NPC.Center - player.Center).Length() * (float)Math.Sqrt(1 - Math.Pow(goalDistance / (NPC.Center - player.Center).Length(), 2)) - (float)Math.Sqrt(goalSetupDistance * goalSetupDistance - goalDistance * goalDistance)) / goalSetupDistance);
									;

									Vector2 goalPosition = player.Center + (NPC.Center - player.Center).SafeNormalize(Vector2.Zero).RotatedBy(angleOffset * side) * goalSetupDistance;
									Vector2 goalVelocity = (goalPosition - NPC.Center) / timeLeft;
									NPC.velocity += (goalVelocity - NPC.velocity) / (float)Math.Pow(timeLeft, 0.25f);
								}
								else
								{
									//try to slow down if already close enough to the player
									NPC.velocity *= 0.95f;
								}
							}
							else
							{
								NPC.velocity *= 0.95f;
								float angularSpeed = 1 / (NPC.velocity.Length() + 1);
								NPC.velocity += new Vector2(angularSpeed, 0).RotatedBy(headRotation + MathHelper.PiOver2 * side);
							}

							angleSpeed = NPC.velocity.Length() * 0.0075f * side;
						}
						else
						{
							if (attackProgress == attackSetupTime)
							{
								NPC.ai[2] = Vector2.Dot(NPC.Center - player.Center, new Vector2(0, -1).RotatedBy(headRotation)) > 0 ? 1 : -1;
							}

							if (attackProgress < attackSetupTime + attackFreezeTime)
							{
								NPC.velocity = Vector2.Zero;

								angleSpeed = (attackSetupTime + attackFreezeTime - attackProgress) * 0.01f * NPC.ai[2];
							}
							else
							{
								//swing
								float angularSpeed = (attackProgress - (attackSetupTime + attackFreezeTime)) * ((attackSetupTime + attackFreezeTime + attackSwingTime) - attackProgress) / (attackSwingTime * attackSwingTime / 8f);

								NPC.velocity = new Vector2(angularSpeed, 0).RotatedBy(headRotation + MathHelper.PiOver2 * NPC.ai[2]);

								angleSpeed = angularSpeed * 0.2f * NPC.ai[2];

								//TODO: Consider bringing back the p1 projectiles and adding more projectiles in p2 (possibly staggered) if this is too easy
								//TODO: Possibly (in phase 2) charge after the last attack?
								if (inPhase2 && (attackProgress - (attackSetupTime + attackFreezeTime)) % 2 == 0 && attackProgress != attackSetupTime + attackFreezeTime) //exclude the first projectile because it looks weird
								{
									float projSpeed = 0.2f;
									Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(280, 0).RotatedBy(headRotation), new Vector2(projSpeed, 0).RotatedBy(headRotation), ProjectileType<ConvectiveWandererAcceleratingShot>(), 25, 2f, Main.myPlayer, ai0: 2f);
								}
							}
						}

						NPC.ai[1]++;
						if (NPC.ai[1] == totalAttackTime * attackRepetitions + attackFirstSetupExtraTime)
						{
							gotoNextAttack = true;
						}
					}
					break;
				#endregion

				#region Dash by player and produce projectiles
				case 2:
					{
						const int attackRepetitions = 4;
						const int totalAttackSetupTime = 90;
						const int totalAttackDashTime = 90;
						const int totalAttackStoppedTime = 60;
						const int totalAttackTime = totalAttackSetupTime + totalAttackDashTime + totalAttackStoppedTime;
						const int extraTimeForProjectileDisappearance = 210; //TODO: Maybe do something during this time

						const float dashStartDistance = 3600f;
						const float dashStartVelocity = 110f;
						const float dashApproachDistance = 160f;

						float attackProgress = (int)NPC.ai[1] % totalAttackTime;

						int side = Vector2.Dot(NPC.Center - player.Center, new Vector2(0, -1).RotatedBy(headRotation)) > 0 ? 1 : -1;
						bool playerAhead = Vector2.Dot(NPC.Center - player.Center, new Vector2(-1, 0).RotatedBy(headRotation)) > 0;

						if (NPC.ai[1] >= totalAttackTime * attackRepetitions + totalAttackSetupTime)
						{
							//allow extra time for projectiles to despawn
							float timeLeft = totalAttackTime * attackRepetitions + totalAttackSetupTime + extraTimeForProjectileDisappearance - NPC.ai[1];

							Idle(timeLeft);

							if (NPC.ai[1] > totalAttackSetupTime)
							{
								tendrilOutwardness = timeLeft / totalAttackSetupTime;
							}
						}
						else if (attackProgress < totalAttackSetupTime)
						{
							float timeLeft = totalAttackSetupTime - attackProgress;

							Vector2 goalPosition = player.Center + (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * dashStartDistance;
							Vector2 goalVelocity = (goalPosition - NPC.Center) / timeLeft;
							NPC.velocity += (goalVelocity - NPC.velocity) / timeLeft;

							if (NPC.ai[1] > totalAttackSetupTime)
							{
								tendrilOutwardness = timeLeft / totalAttackSetupTime;
							}
						}
						else if (attackProgress < totalAttackSetupTime + totalAttackDashTime)
						{
							float timeLeft = totalAttackSetupTime + totalAttackDashTime - attackProgress;

							if (attackProgress == totalAttackSetupTime)
							{
								NPC.velocity = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy((Main.rand.NextBool() ? 1 : -1) * 0.33f) * dashStartVelocity;
							}
							else
							{
								NPC.velocity *= timeLeft / (timeLeft + 1);
								rotationAmount *= timeLeft / (timeLeft + 1);

								float reducedDistance = (player.Center - NPC.Center).Length() / dashApproachDistance;

								float offsetAngle = Math.Min(MathHelper.PiOver2, MathHelper.PiOver2 / reducedDistance);
								if (playerAhead)
								{
									NPC.velocity = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(-side * offsetAngle) * NPC.velocity.Length();
								}
								else
								{
									NPC.velocity = (NPC.Center - player.Center).SafeNormalize(Vector2.Zero).RotatedBy(side * offsetAngle) * NPC.velocity.Length();
								}
							}

							tendrilOutwardness = 1 - timeLeft / totalAttackStoppedTime;
						}
						else
						{
							NPC.velocity = Vector2.Zero;
							rotationAmount = 0f;

							tendrilOutwardness = 1f;

							//TODO: Drawcode tendrils glowing in preparation

							if (attackProgress == totalAttackSetupTime + totalAttackDashTime + totalAttackStoppedTime / 2)
                            {
								//TODO: Create sound on projectile firing
								//TODO: Orthogonal flashes on projectile creation
								//TODO: Some sort of p2 modification
								for (int i = 0; i < segmentPositions.Length - segmentsTailTendrils - 2; i++)
                                {
									if (TendrilIndex(i) == 0)
                                    {
										Vector2 position = SegmentPosition(i);
										float rotation = SegmentRotation(i);
										float radius = SegmentRadius(i);
										float angle = SegmentAngle(i);

										const float projSpeed = 32f;
										int projsPerSegment = 16;

										for (int j = 0; j < projsPerSegment; j++)
                                        {
											Projectile.NewProjectile(NPC.GetSource_FromAI(), position + new Vector2(0, radius).RotatedBy(rotation) * (float)Math.Sin(angle + j * MathHelper.TwoPi / projsPerSegment), new Vector2(0, projSpeed).RotatedBy(rotation) * (float)Math.Sin(angle + j * MathHelper.TwoPi / projsPerSegment), ProjectileType<ConvectiveWandererAcceleratingShot>(), 25, 2f, Main.myPlayer, ai0: 0f);
                                        }
                                    } 
                                }
                            }
						}

						angleSpeed = NPC.velocity.Length() * 0.003f;

						NPC.ai[1]++;
						if (NPC.ai[1] == totalAttackTime * attackRepetitions + totalAttackSetupTime + extraTimeForProjectileDisappearance) //run an extra setup to move on after the final dash
						{
							gotoNextAttack = true;
						}
					}
					break;
				#endregion

				#region Dash up and produce projectiles
				case 3:
                    {
						const int attackRepetitions = 4;
						const int attackSetupTime = 120;
						const int attackSetupFrozenTime = 30;
						const int attackDashTime = 150;
						const int totalAttackTime = attackSetupTime + attackDashTime;
						const int extraTimeForProjectileDisappearance = 120;

						float attackProgress = (int)NPC.ai[1] % totalAttackTime;

						int side = Vector2.Dot(NPC.Center - player.Center, new Vector2(0, -1).RotatedBy(headRotation)) > 0 ? 1 : -1;
						bool playerAhead = Vector2.Dot(NPC.Center - player.Center, new Vector2(-1, 0).RotatedBy(headRotation)) > 0;

						if (NPC.ai[1] > totalAttackTime * attackRepetitions)
                        {
							//allow extra time for projectiles to leave
							float timeLeft = totalAttackTime * attackRepetitions + extraTimeForProjectileDisappearance - NPC.ai[1];
							Idle(timeLeft);

							angleSpeed = NPC.velocity.Length() * 0.003f;
						}
						else if (attackProgress < attackSetupTime)
						{
							//set up attack
							//TODO: Telegraph at all
							float timeLeft = attackSetupTime - attackSetupFrozenTime - attackProgress;

							if (timeLeft > 0)
							{
								Vector2 goalPosition = player.Center + new Vector2(0, 3600);
								GoTowardsRadial(goalPosition, player.Center, timeLeft);
							}
							else
                            {
								Vector2 goalPosition = new Vector2(NPC.Center.X, player.Center.Y + 3600);
								NPC.velocity = goalPosition - NPC.Center;

								upDashTelegraphProgress = (timeLeft + attackSetupFrozenTime) * (timeLeft + attackSetupFrozenTime) * (-timeLeft) / (float)Math.Pow(attackSetupFrozenTime, 3) * 27f / 4f;
							}

							angleSpeed = NPC.velocity.Length() * 0.003f;
						}
						//TODO: Trail projectiles?
						else if (attackProgress < attackSetupTime + attackDashTime)
						{
							float timeLeft = attackSetupTime + attackDashTime - attackProgress;

							if (attackProgress == attackSetupTime)
							{
								NPC.velocity = new Vector2(0, -100);
							}

							NPC.velocity *= timeLeft / (timeLeft + 1);

							angleSpeed = NPC.velocity.Length() * 0.003f;

							float radius = SegmentRadius(0);
							const int rowsProjectiles = 1;

							int numPerRow = inPhase2 ? 4 : 3;

							for (int j = 0; j < rowsProjectiles; j++)
							{
								float angle = SegmentAngle(0) - angleSpeed * j / (float)rowsProjectiles;

								for (int i = 0; i < numPerRow; i++)
								{
									float angleBonus = i * MathHelper.TwoPi / numPerRow;
									const float projSpeed = 2f;
									float projSpeedMult = (float)Math.Sin(angle + angleBonus);
									Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center - NPC.velocity * j / (float)rowsProjectiles + new Vector2(radius * projSpeedMult, 0), new Vector2(projSpeed * projSpeedMult, 0), ProjectileType<ConvectiveWandererAcceleratingShot>(), 25, 2f, Main.myPlayer, ai0: 1f);
								}
							}
						}

						NPC.ai[1]++;
						if (NPC.ai[1] == totalAttackTime * attackRepetitions + extraTimeForProjectileDisappearance)
						{
							gotoNextAttack = true;
						}
					}
					break;
				#endregion

				#region Circle player while charging up heat pulse
				case 4:
                    {
						const int totalAttackTime = 480;



						NPC.ai[1]++;
						if (NPC.ai[1] == totalAttackTime)
						{
							gotoNextAttack = true;
						}
					}
					break;
				#endregion

				#region Swim around under the player and cause screenshake, producing flamethrowers from terrain/lava surface, and causing lava debris to fall from the ceiling
				case 5:
                    {

                    }
					break;
				#endregion

				#region Tentacles spin and rotate outwards, producing projectiles
				case 6:
                    {

                    }
					break;
				#endregion

				#region Tentacles point backwards, boss shoots giant mouth flamethrower
				case 7:
                    {

                    }
					break;
					#endregion
			}

			if (gotoNextAttack)
			{
				if (NPC.life * 2 < NPC.lifeMax)
				{
					inPhase2 = true;
					//TODO: Blue visuals for phase 2
				}

				//TODO: go to next attack with a SC/Sentinel-like system
				NPC.ai[0] = (NPC.ai[0] + Main.rand.Next(0, 2)) % 3 + 1;
				NPC.ai[1] = 0;
			}

			//TODO: Add a bit of occasional random variation to some of the attacks to ensure their positioning is varied
			//(This should only be varied some of the time, a la sun pixie's projectile rings)

			//yes, I stole this from sun pixie
			//avoiding the player is important okay
			//TODO: Make this take into account our direction better to avoid sudden curvature
			//TODO: Maximum turn parameter
			//TODO: Use this in attack 2 setup
			void GoTowardsRadial(Vector2 goalPosition, Vector2 orbitPoint, float timeLeft, float maxSpeed = float.PositiveInfinity)
			{
				if (timeLeft == 1)
				{
					//force precise movement if no time is left
					NPC.velocity = goalPosition - NPC.Center;
				}
				else
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
				}

				if (NPC.velocity.Length() > maxSpeed)
				{
					NPC.velocity.Normalize();
					NPC.velocity *= maxSpeed;
				}
			}

			//idle behavior
			//TODO: Adjust this to be better and probably less circly?
			void Idle(float timeLeft)
			{
				int side = Vector2.Dot(NPC.Center - player.Center, new Vector2(0, -1).RotatedBy(headRotation)) > 0 ? 1 : -1;

				Vector2 goalPosition = player.Center + (NPC.Center - player.Center).SafeNormalize(Vector2.Zero).RotatedBy(side) * 1200f;
				Vector2 goalVelocity = (goalPosition - NPC.Center) / 30f;
				NPC.velocity += (goalVelocity - NPC.velocity) / 30f;
			}

			#endregion

			NPC.noGravity = true;
			NPC.rotation = NPC.velocity.ToRotation();

			tendrilOutwardness = Math.Clamp(tendrilOutwardness, 0, 1);

			//update segment positions
			segmentPositions[0] = NPC.Center + NPC.velocity;
			Vector2 rotationGoal = Vector2.Zero;

			for (int i = 1; i < segmentPositions.Length; i++)
			{
				if (i > 1)
				{
					rotationGoal = ((rotationFade - 1) * rotationGoal + (segmentPositions[i - 1] - segmentPositions[i - 2])) / rotationFade;
				}

				segmentPositions[i] = segmentPositions[i - 1] + (rotationAmount * rotationGoal + (segmentPositions[i] - segmentPositions[i - 1]).SafeNormalize(Vector2.Zero)).SafeNormalize(Vector2.Zero) * segmentSeparation;
			}

			//update segment angles
			segmentAngles[0] = (float)Math.IEEERemainder(segmentAngles[0] + angleSpeed, MathHelper.TwoPi);
			for (int i = 1; i < segmentAngles.Length; i++)
            {
				segmentAngles[i] = Utils.AngleLerp(segmentAngles[i], segmentAngles[i - 1], angleLerpAmount);
			}

			//update pulse scales
			segmentPulseScaleAngles[0] = (float)Math.IEEERemainder(segmentAngles[0] + pulseSpeed, MathHelper.TwoPi);
			for (int i = 1; i < segmentPulseScaleAngles.Length; i++)
			{
				segmentPulseScaleAngles[i] = Utils.AngleLerp(segmentPulseScaleAngles[i], segmentPulseScaleAngles[i - 1], pulseLerpAmount);
			}

			//update heat values
			const float heatLerpAmount = 0.5f;
			const float heatGain = 0.01f;
			const float heatLoss = 0.0025f;
			for (int i = 0; i < segmentHeats.Length; i++)
            {
				Point tilePoint = SegmentPosition(i).ToTileCoordinates();
				Tile tile = Framing.GetTileSafely(tilePoint);
				if ((tile.LiquidAmount == 255 && tile.LiquidType == LiquidID.Lava) || tile.TileType == TileID.Ash || tile.TileType == TileID.Hellstone || tile.TileType == TileType<MantellarOreTile>() || tilePoint.Y >= Main.maxTilesY)
                {
					segmentHeats[i] += heatGain;
				}
				else
                {
					segmentHeats[i] -= heatLoss;
                }
				segmentHeats[i] = Math.Clamp(segmentHeats[i], 0, 1);

			}
			for (int i = 1; i < segmentHeats.Length; i++)
			{
				float diff = segmentHeats[i] - segmentHeats[i - 1];
				segmentHeats[i] -= diff * heatLerpAmount / 2;
				segmentHeats[i - 1] += diff * heatLerpAmount / 2;
			}


			//position hitbox segments
			//the order in which we do this matters as it determines hit priority
			//doing the tentacles first ensures the tentacles always shield the head

			//position tentacle hitbox segments
			float tentacleBaseAngle = SegmentAngle(TENTACLE_ATTACH_SEGMENT_INDEX);
			float tentacleBaseRotation = SegmentRotation(TENTACLE_ATTACH_SEGMENT_INDEX) + MathHelper.PiOver2;
			float tentacleBaseRadius = SegmentRadius(TENTACLE_ATTACH_SEGMENT_INDEX) + TentacleRadius(0);
			Vector2 tentacleBasePosition = SegmentPosition(TENTACLE_ATTACH_SEGMENT_INDEX);

			List<RectangleHitboxData> hitboxes = new List<RectangleHitboxData>();
			for (int tentacleIndex = 0; tentacleIndex < NUM_TENTACLES; tentacleIndex++)
			{
				for (int segmentIndex = 0; segmentIndex < HITBOXES_PER_TENTACLE; segmentIndex++)
				{
					float indexForDrawing = segmentIndex * TENTACLE_SEGMENTS / (float)HITBOXES_PER_TENTACLE;

					Vector2 spot = TentacleSegmentPosition(indexForDrawing, tentacleBaseAngle + tentacleIndex * MathHelper.TwoPi / NUM_TENTACLES, tentacleBaseRotation, tentacleBaseRadius, tentacleBasePosition);
					float radius = TentacleRadius(indexForDrawing);

					hitboxes.Add(new RectangleHitboxData(new Rectangle((int)(spot.X - radius), (int)(spot.Y - radius), (int)(radius * 2), (int)(radius * 2))));
				}
			}

			//position body and head segments
			for (int h = 0; h < numSegments + 1; h++)
			{
				Vector2 spot = h == numSegments ?
					 SegmentPosition(-(segmentsPerHitbox - hitboxSegmentOffset) * specialSegmentsHeadMultiplier) : //head segment
					 segmentPositions[h * segmentsPerHitbox + hitboxSegmentOffset]; //body/tail segment
				hitboxes.Add(new RectangleHitboxData(new Rectangle((int)spot.X - NPC.width / 2, (int)spot.Y - NPC.height / 2, NPC.width, NPC.height)));
			}

			NPC.GetGlobalNPC<MultiHitboxNPC>().AssignHitboxFrom(hitboxes);
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffType<Incinerating>(), 60, true);
		}

		public void MultiHitboxSegmentUpdate(NPC npc, RectangleHitbox mostRecentHitbox)
		{
			if (mostRecentHitbox.index < NUM_TENTACLES * HITBOXES_PER_TENTACLE)
			{
				//hitting tentacle segments is bad
				npc.HitSound = new LegacySoundStyle(SoundID.Tink, 0);
				npc.GetGlobalNPC<PolaritiesNPC>().neutralTakenDamageMultiplier = 0.5f;
			}
			else if (mostRecentHitbox.index < NUM_TENTACLES * HITBOXES_PER_TENTACLE + numSegments)
			{
				//hitting body segments is meh
				npc.HitSound = SoundID.NPCHit2;
				npc.GetGlobalNPC<PolaritiesNPC>().neutralTakenDamageMultiplier = 1f;
			}
			else
			{
				//hitting the head is great
				npc.HitSound = SoundID.NPCHit1;
				npc.GetGlobalNPC<PolaritiesNPC>().neutralTakenDamageMultiplier = 2f;
			}
		}
		#endregion

		#region Death Behavior
		public override bool CheckDead()
		{
			if (!PolaritiesSystem.downedConvectiveWanderer)
			{
				NPC.SetEventFlagCleared(ref PolaritiesSystem.downedConvectiveWanderer, -1);

				PolaritiesSystem.GenerateMantellarOre();
			}

			/*TODO: Gores:
			for (int i = 0; i < NPC.GetGlobalNPC<MultiHitboxNPC>().hitboxes.Length; i++)
			{
				Vector2 gorePos = NPC.GetGlobalNPC<MultiHitboxNPC>().hitboxes[i].TopLeft();
				if (i == 0)
				{
					Gore.NewGore(NPC.GetSource_Death(), gorePos, Vector2.Zero, Mod.Find<ModGore>("ConvectiveWandererGore1").Type);
				}
				else if (i == NPC.GetGlobalNPC<MultiHitboxNPC>().hitboxes.Length - 1)
				{

					Gore.NewGore(NPC.GetSource_Death(), gorePos, Vector2.Zero, Mod.Find<ModGore>("ConvectiveWandererGore3").Type);
				}
				else
				{
					Gore.NewGore(NPC.GetSource_Death(), gorePos, Vector2.Zero, Mod.Find<ModGore>("ConvectiveWandererGore2").Type);
				}
			}*/
			return true;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			//TODO:
		}
        #endregion

        #region Drawcode
        //a whole bunch of drawing stuff and helper methods
        //abandon all hope ye who enter here

        //a PriorityQueue that stores our drawData
        PriorityQueue<DrawData, float> drawDatas;

		//The maximum capacity potentially required by drawDatas
		int MAX_DRAW_CAPACITY = 8669;

		const int NUM_TENTACLES = 8;

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (NPC.IsABestiaryIconDummy)
			{
				//TODO: Bestiary portrait
				return false;
			}

			if (RenderTargetLayer.IsActive<ConvectiveWandererTarget>())
			{
				//stuff to draw to our target (includes main worm)
				drawDatas.Clear();

				//register body data
				for (int i = segmentPositions.Length - 1; i > -specialSegmentsHead; i--)
				{
					//most draws I've recorded as needed for this is: 6796
					DrawSegment(drawDatas, spriteBatch, screenPos, i, NPC.GetNPCColorTintedByBuffs(Color.White));
				}

				//register tentacle data
				for (int i = 0; i < NUM_TENTACLES; i++)
				{
					//most draws I've recorded as needed for this is needed for this is: 1873
					DrawTentacle(drawDatas, spriteBatch, screenPos, i * MathHelper.TwoPi / NUM_TENTACLES, NPC.GetNPCColorTintedByBuffs(Color.White));
				}

				//debug stuff that lets us know if we've gone above the default max draw capacity (this doesn't really cause problems but I would like to know when it occurs)
				if (drawDatas.Count > MAX_DRAW_CAPACITY) Main.NewText("drawData capacity exceeded: " + drawDatas.Count + " > " + MAX_DRAW_CAPACITY); //for finding max capacity used
				if (drawDatas.Count > MAX_DRAW_CAPACITY) MAX_DRAW_CAPACITY = drawDatas.Count;

				//draw all data
				while (drawDatas.TryDequeue(out var drawData, out _))
				{
					drawData.Draw(spriteBatch);
				}
			}
			else if (DrawLayer.IsActive<DrawLayerAdditiveAfterLiquids>())
            {
				//stuff to draw additively after most things (mostly telegraphs and effects
				if (upDashTelegraphProgress > 0)
                {
					Main.spriteBatch.Draw(Textures.Glow256.Value, NPC.Center - screenPos, Textures.Glow256.Frame(), ModUtils.ConvectiveFlameColor(upDashTelegraphProgress * upDashTelegraphProgress * 0.125f) * upDashTelegraphProgress, 0f, Textures.Glow256.Size() / 2, new Vector2(1, 64), SpriteEffects.None, 0);
                }
            }

			return false;
		}

		public override void DrawBehind(int index)
		{
			RenderTargetLayer.AddNPC<ConvectiveWandererTarget>(index);
			DrawLayer.AddNPC<DrawLayerAdditiveAfterLiquids>(index);
		}



		const int BASE_TEXTURE_HEIGHT = 128;
		const int SIDES_PER_SEGMENT = 2;
		const int DRAWS_PER_SIDE = 4;
		const int DRAWS_PER_SEGMENT = SIDES_PER_SEGMENT * DRAWS_PER_SIDE;

		int SegmentWidth(int index)
        {
			return (index < 1) ? (int)(segmentSeparation / specialSegmentsHeadMultiplier) : (int)segmentSeparation;
		}
		Vector2 BaseSegmentPosition(int index)
		{
			if (index < 0)
			{
				return segmentPositions[0] + new Vector2(-SegmentWidth(index) * index, 0).RotatedBy(SegmentRotation(1));
			}
			else
			{
				return segmentPositions[index];
			}
		}
		Vector2 SegmentPosition(float index)
		{
			if (index % 1 == 0)
				return (BaseSegmentPosition((int)index) + BaseSegmentPosition((int)index - 1)) / 2;
			return Vector2.Lerp(SegmentPosition((int)Math.Floor(index)), SegmentPosition((int)Math.Ceiling(index)), index % 1);
		}
		float SegmentRotation(float index)
		{
			if (index % 1 == 0)
				return (BaseSegmentPosition((int)index - 1) - BaseSegmentPosition((int)index)).ToRotation();
			return Utils.AngleLerp(SegmentRotation((int)Math.Floor(index)), SegmentRotation((int)Math.Ceiling(index)), index % 1);
		}
		float SegmentHeat(float index)
		{
			if (index < 1) index = 1;
			if (index % 1 == 0)
				return (segmentHeats[(int)index] + segmentHeats[(int)index - 1]) / 2;
			return ModUtils.Lerp(SegmentHeat((int)Math.Floor(index)), SegmentHeat((int)Math.Ceiling(index)), index % 1);
		}

		//TODO: Enable opening/closing mouth via PulseScale or something similar
		float SegmentAngle(int index)
		{
			if (index < 1) index = 1;
			return segmentAngles[index];
		}
		float PulseScale(int index)
		{
			//restrict pulsing for head segments
			float headRestrictionMultiplier = index < 1 ?
				(index + specialSegmentsHead) / (float)(specialSegmentsHead + 1)
				: 1f;
			float extraPulseScale = (float)Math.Sin(segmentPulseScaleAngles[index + specialSegmentsHead]) * 0.1f * headRestrictionMultiplier;
			return 1 + extraPulseScale;
		}

		float TrueBaseScale(int index)
		{
			if (index >= segmentPositions.Length - 2 - segmentsTailTendrils) return 0f;
			return index >= (segmentPositions.Length - 2 - segmentsTailTendrils - segmentsTail) ?
				(float)Math.Sqrt(1 - Math.Pow((segmentPositions.Length - 2 - segmentsTailTendrils - index) / (float)segmentsTail - 1, 2)) :
				index < 1 ?
				(float)Math.Sqrt(1 - Math.Pow((index + specialSegmentsHead) / (float)specialSegmentsHead - 1, 2)) :
				1f;
		}
		float BaseScale(int index)
		{
			return TrueBaseScale(index) * PulseScale(index);
		}
		float SegmentRadius(int index)
		{
			return BASE_TEXTURE_HEIGHT / MathHelper.TwoPi * SIDES_PER_SEGMENT * BaseScale(index);
		}

		int TendrilIndex(int index)
		{
			int indexInTailTendrils = (index - (segmentPositions.Length - segmentsTailTendrils - 2)) % segmentsTailTendrils;
			if (indexInTailTendrils < 0) indexInTailTendrils += segmentsTailTendrils;
			return indexInTailTendrils;
		}

		void DrawSegment(PriorityQueue<DrawData, float> drawDatas, SpriteBatch spriteBatch, Vector2 screenPos, int index, Color color)
		{
			//TODO: Give tail doubled-up segments for a more continuous appearance, since it changes width
			Vector2 segmentPosition = SegmentPosition(index);

			//don't draw if offscreen
			int buffer = 80;
			if (!spriteBatch.GraphicsDevice.ScissorRectangle.Intersects(new Rectangle((int)(segmentPosition - screenPos).X - buffer, (int)(segmentPosition - screenPos).Y - buffer, buffer * 2, buffer * 2)))
			{
				return;
			}

			//draw head in more detail due to lack of obscuring bristles
			int segmentWidth = SegmentWidth(index);
			int drawWidthPerSegment = segmentWidth * 2;

			float TendrilRadius(float index, int tendrilIndex, float tendrilOutwardness)
			{
				float indexMultiplierNonOut = 16f * ((tendrilIndex + 0.5f) > 0 ? (float)Math.Sqrt((tendrilIndex + 0.5f)) : -(float)Math.Sqrt(-(tendrilIndex + 0.5f)));
				float indexMultiplierOut = segmentSeparation * tendrilIndex;
				float indexMultiplier = ModUtils.Lerp(indexMultiplierNonOut, indexMultiplierOut, tendrilOutwardness);
				return indexMultiplier * PulseScale((int)index) + SegmentRadius((int)index - tendrilIndex - 1);
			}
			int GetSegmentFrontPoint(int index)
			{
				return index < 1 ?
					(264 - drawWidthPerSegment) - segmentWidth * (index + specialSegmentsHead - 1) : //head
					(160 - drawWidthPerSegment) - segmentWidth * (index - 1) % 128; //body and tail
			}

			float globalDepthModifier = index * 16f;
			if (index < 1) globalDepthModifier -= specialSegmentsHead * 1024f;

			//base segments
			if (index < segmentPositions.Length - segmentsTailTendrils - 2)
			{ 
				float segmentRotation = SegmentRotation(index);

				float segmentAngle = SegmentAngle(index);
				float segmentPulseScale = PulseScale(index);

				int segmentFramePoint = GetSegmentFrontPoint(index);

				float radius = SegmentRadius(index);

				Color heatForColorValue = Color.Lerp(Color.Red, Color.White, SegmentHeat(index));

				float scaleMultToMatch = (float)Math.Tan(MathHelper.Pi / DRAWS_PER_SEGMENT) * radius;

				for (int i = 0; i < SIDES_PER_SEGMENT; i++)
				{
					for (int j = 0; j < DRAWS_PER_SIDE; j++)
					{
						float totalAngle = segmentAngle + i * MathHelper.TwoPi / SIDES_PER_SEGMENT + j * MathHelper.TwoPi / DRAWS_PER_SEGMENT;

						float offsetDist = (float)Math.Sin(totalAngle) * radius;
						Vector2 sectionOffset = new Vector2(0, offsetDist).RotatedBy(segmentRotation);

						Vector2 scale = new Vector2(1, (float)Math.Cos(totalAngle) * scaleMultToMatch / (BASE_TEXTURE_HEIGHT / DRAWS_PER_SIDE) * 2);

						float depthColorModifier = ((float)Math.Cos(totalAngle) + 2) / 3;
						Color depthModifiedColor = color.MultiplyRGB(new Color(new Vector3(depthColorModifier))).MultiplyRGB(heatForColorValue);

						SpriteEffects bodyEffects = SpriteEffects.None;

						//allow drawing of backwards segments if in the head, but prune otherwise for efficiency since non-head reversed segments are obscured
						if (index < 1 && scale.Y < 0)
						{
							bodyEffects = SpriteEffects.FlipVertically;
							scale.Y = -scale.Y;
						}

						if (scale.Y > 0)
						{
							Rectangle frame = new Rectangle(segmentFramePoint, j * BASE_TEXTURE_HEIGHT / DRAWS_PER_SIDE, drawWidthPerSegment, BASE_TEXTURE_HEIGHT / DRAWS_PER_SIDE);
							Vector2 origin = frame.Size() / 2;

							drawDatas.Enqueue(new DrawData(TextureAssets.Npc[Type].Value, segmentPosition + sectionOffset - screenPos, frame, depthModifiedColor, segmentRotation, origin, scale, bodyEffects, 0), (float)Math.Cos(totalAngle) - globalDepthModifier);
						}

						for (int finIndex = -1; finIndex <= 1; finIndex += 2)
						{
							//fins are rotated and don't scale with extra pulsing
							float offsetAngle = totalAngle + finIndex * MathHelper.Pi / 4;

							Vector2 finScale = new Vector2(1, (float)Math.Abs(Math.Sin(offsetAngle)) * scaleMultToMatch / (BASE_TEXTURE_HEIGHT / DRAWS_PER_SIDE) * 2 / segmentPulseScale);

							Rectangle finFrame = new Rectangle(segmentFramePoint, (BASE_TEXTURE_HEIGHT + 2) + j * BASE_TEXTURE_HEIGHT / DRAWS_PER_SIDE, drawWidthPerSegment, BASE_TEXTURE_HEIGHT / DRAWS_PER_SIDE - 2);
							Vector2 finOrigin = new Vector2(finFrame.Width / 2, 0);

							SpriteEffects finEffects = SpriteEffects.None;
							if (Math.Sin(offsetAngle) < 0)
							{
								finEffects = SpriteEffects.FlipVertically;
								finOrigin = new Vector2(finFrame.Width / 2, finFrame.Height);
							}

							//only add if we'd actually be visible
							if (Math.Cos(totalAngle) > 0 || Math.Abs(Math.Sin(totalAngle) + Math.Sin(offsetAngle)) > 1)
								drawDatas.Enqueue(new DrawData(TextureAssets.Npc[Type].Value, segmentPosition + sectionOffset - screenPos, finFrame, depthModifiedColor, segmentRotation, finOrigin, finScale, finEffects, 0), (float)Math.Cos(totalAngle) * 2f - globalDepthModifier);

							//internal fin code in the event I decide to give the beak internal teeth
							/*if (index < 1)
                            {
								//extra inner fins to make the teeth look thicker

								SpriteEffects innerFinEffects = SpriteEffects.FlipVertically;
								Vector2 innerFinOrigin = new Vector2(finFrame.Width / 2, finFrame.Height);
								if (Math.Sin(offsetAngle) < 0)
								{
									innerFinEffects = SpriteEffects.None;
									innerFinOrigin = new Vector2(finFrame.Width / 2, 0);
								}
								Rectangle innerFinFrame = new Rectangle(segmentFramePoint, 2 * (BASE_TEXTURE_HEIGHT + 2) + j * BASE_TEXTURE_HEIGHT / DRAWS_PER_SIDE, drawWidthPerSegment, BASE_TEXTURE_HEIGHT / DRAWS_PER_SIDE);
								drawDatas.Enqueue(new DrawData(TextureAssets.Npc[Type].Value, segmentPosition + sectionOffset - screenPos, innerFinFrame, depthModifiedColor, segmentRotation, innerFinOrigin, finScale, innerFinEffects, 0), (float)Math.Cos(totalAngle) * 0.5f - globalDepthModifier);
							}*/
						}
					}
				}
			}

			//tendril 'skirts'
			//TODO: make gradient more continuous with a custom sprite a la denizen's telegraphs
			//TODO: Make brighter?
			if (index > 0 && index < segmentPositions.Length - 2)
			{
				//don't draw if our tendrils are pointing more outwards
				int tendrilIndex = TendrilIndex(index);

				int numTendrilSidesPerSegment = index < segmentPositions.Length - segmentsTailTendrils - 2 ? 16 : 8;

				//don't adjust outwardness for the tail tendrils
				float effectiveOutwardness = index < segmentPositions.Length - segmentsTailTendrils - 2 ? tendrilOutwardness : 0f;

				float effectiveIndexForPositioning = index - tendrilIndex * effectiveOutwardness;

				float globalSegmentDepthModifier = effectiveIndexForPositioning * 16f;

				Vector2 pos0 = SegmentPosition(effectiveIndexForPositioning + (1 - effectiveOutwardness));
				Vector2 pos1 = SegmentPosition(effectiveIndexForPositioning);
				Vector2 pos2 = SegmentPosition(effectiveIndexForPositioning - (1 - effectiveOutwardness));

				float rot0 = SegmentRotation(effectiveIndexForPositioning + (1 - effectiveOutwardness));
				float rot1 = SegmentRotation(effectiveIndexForPositioning);
				float rot2 = SegmentRotation(effectiveIndexForPositioning - (1 - effectiveOutwardness));

				float tendrilsRadius0 = TendrilRadius(index + 1, tendrilIndex + 1, effectiveOutwardness);
				float tendrilsRadius1 = TendrilRadius(index, tendrilIndex, effectiveOutwardness);
				float tendrilsRadius2 = TendrilRadius(index - 1, tendrilIndex - 1, effectiveOutwardness);

				float tendrilProgress = 1 - tendrilIndex / (float)segmentsTailTendrils;

				float baseAngle0 = SegmentAngle(index + 1) + MathHelper.Pi / numTendrilSidesPerSegment;
				float baseAngle1 = SegmentAngle(index) + MathHelper.Pi / numTendrilSidesPerSegment;
				float baseAngle2 = SegmentAngle(index - 1) + MathHelper.Pi / numTendrilSidesPerSegment;

				for (int i = 0; i < numTendrilSidesPerSegment; i++)
				{
					float totalAngle0 = baseAngle0 + i * MathHelper.TwoPi / numTendrilSidesPerSegment;
					float totalAngle1 = baseAngle1 + i * MathHelper.TwoPi / numTendrilSidesPerSegment;
					float totalAngle2 = baseAngle2 + i * MathHelper.TwoPi / numTendrilSidesPerSegment;

					float offsetDist0 = (float)Math.Sin(totalAngle0) * tendrilsRadius0;
					float offsetDist1 = (float)Math.Sin(totalAngle1) * tendrilsRadius1;
					float offsetDist2 = (float)Math.Sin(totalAngle2) * tendrilsRadius2;

					Vector2 tendrilPos0 = pos0 + new Vector2(0, offsetDist0).RotatedBy(rot0);
					Vector2 tendrilPos1 = pos1 + new Vector2(0, offsetDist1).RotatedBy(rot1);
					Vector2 tendrilPos2 = pos2 + new Vector2(0, offsetDist2).RotatedBy(rot2);

					Vector2 startPosition = (tendrilPos0 + tendrilPos1) / 2;
					Vector2 endPosition = (tendrilPos1 + tendrilPos2) / 2;

					float depthColorModifier = ((float)Math.Cos(totalAngle1) + 3) / 4;
					Color tendrilColor = ModUtils.ConvectiveFlameColor(tendrilProgress * tendrilProgress / 2f * SegmentHeat(index - tendrilIndex)).MultiplyRGB(new Color(new Vector3(depthColorModifier))) * tendrilProgress;
					float tendrilWidth = tendrilProgress * 2f + 2f;

					drawDatas.Enqueue(new DrawData(Textures.PixelTexture.Value, startPosition - screenPos, Textures.PixelTexture.Frame(), color.MultiplyRGBA(tendrilColor), (endPosition - startPosition).ToRotation(), new Vector2(0, 0.5f), new Vector2((endPosition - startPosition).Length(), tendrilWidth), SpriteEffects.None, 0), (float)Math.Cos(totalAngle1) * 64f - globalSegmentDepthModifier);
				}
			}
		}

		const int TENTACLE_SEGMENTS = 32;
		const int TENTACLE_HEAD_SEGMENTS = 8;
		const int TENTACLE_SEGMENT_SEPARATION = 8;

		const int TENTACLE_ATTACH_SEGMENT_INDEX = -4;

		void DrawTentacle(PriorityQueue<DrawData, float> drawDatas, SpriteBatch spriteBatch, Vector2 screenPos, float angleOffset, Color color)
		{
			Vector2 segmentPosition = SegmentPosition(TENTACLE_ATTACH_SEGMENT_INDEX);
			float segmentRotation = SegmentRotation(TENTACLE_ATTACH_SEGMENT_INDEX);
			float radius = SegmentRadius(TENTACLE_ATTACH_SEGMENT_INDEX);
			float segmentAngle = SegmentAngle(TENTACLE_ATTACH_SEGMENT_INDEX);
			float totalAngle = segmentAngle + angleOffset;

			for (int i = -TENTACLE_HEAD_SEGMENTS; i < TENTACLE_SEGMENTS; i++)
            {
				DrawTentacleSegment(drawDatas, spriteBatch, screenPos, totalAngle, segmentRotation + MathHelper.PiOver2, segmentPosition, radius + TentacleRadius(0), i, color);
            }
		}

		const int BASE_TENTACLE_TEXTURE_HEIGHT = 96;
		const int SIDES_PER_TENTACLE_SEGMENT = 1;
		const int DRAWS_PER_TENTACLE_SIDE = 12;
		const int DRAWS_PER_TENTACLE_SEGMENT = SIDES_PER_TENTACLE_SEGMENT * DRAWS_PER_TENTACLE_SIDE;

		const float TENTACLE_HEAD_SEPARATION_SCALE_MULT = 4;

		const float TENTACLE_POSITION_INDEX_OFFSET = TENTACLE_HEAD_SEGMENTS / TENTACLE_HEAD_SEPARATION_SCALE_MULT + 2;
		const float EFFECTIVE_TENTACLE_SEGMENTS = TENTACLE_SEGMENTS + TENTACLE_POSITION_INDEX_OFFSET;

		//a whole bunch of tentacle-related methods
		int TentacleSegmentWidth(float index)
		{
			return (index > 0) ? TENTACLE_SEGMENT_SEPARATION : (int)(TENTACLE_SEGMENT_SEPARATION / TENTACLE_HEAD_SEPARATION_SCALE_MULT);
		}
		Vector2 TentacleEffectiveBasePosition(float index, float baseAngle, float baseRotation, float baseRadius, Vector2 originalBasePosition)
		{
			float offsetDistForEffectiveBasePosition = (float)Math.Sin(baseAngle + TentacleBaseAngleOffset(index)) * baseRadius;
			return originalBasePosition + new Vector2(0, offsetDistForEffectiveBasePosition).RotatedBy(baseRotation - MathHelper.PiOver2);
		}

		//TODO: Make this adjustable in AI
        float TentacleBaseAngleOffset(float index)
		{
			return (index <= 0) ? 0 :
				index * 0.1f; //* (float)Math.Cos(Main.GlobalTimeWrappedHourly * 0.5f);
		}
		float TentacleRotation(float index)
        {
			return 0f; //2.5f * (float)Math.Pow((1 - Math.Cos(Main.GlobalTimeWrappedHourly * 1f)) / 2f, (index / EFFECTIVE_TENTACLE_SEGMENTS + 0.5f) * 4f);
		}

		Vector2 TentacleRadialOffset(float index, float baseRadius)
		{
			//baseRadius / Sin(angle) = BigLength
			//DistTraveled = (indexAsFloat * TENTACLE_SEGMENT_SEPARATION)
			//(BigLength - DistTraveled) * Sin(angle) = TentacleRadius(index);

			//BigLength = TentacleRadius(index) / Sin(angle) + DistTraveled
			//Sin(angle) = (baseRadius - TentacleRadius(index)) / DistTraveled

			float indexAsFloat = index;
			if (indexAsFloat <= 0) indexAsFloat /= TENTACLE_HEAD_SEPARATION_SCALE_MULT;

			float DistTraveled = indexAsFloat * TENTACLE_SEGMENT_SEPARATION;

			float sideScaleMult = (float)Math.Sin(MathHelper.TwoPi / NUM_TENTACLES) / 2f;

			float goalRadius = TentacleRadius(index) / sideScaleMult; //ModUtils.Lerp(baseRadius, TentacleRadius(index) / sideScaleMult, (float)(1 + Math.Cos(Main.GlobalTimeWrappedHourly * 1f)) / 2f);

			float segmentSqueezing = (float)(1 - Math.Pow(1 - index / EFFECTIVE_TENTACLE_SEGMENTS, 12f));
			float distFromCenter = ModUtils.Lerp(baseRadius, goalRadius, segmentSqueezing);

			float discriminant = DistTraveled * DistTraveled - (baseRadius - distFromCenter) * (baseRadius - distFromCenter);
			float absThing = discriminant > 0 ? -(float)Math.Sqrt(discriminant) : (float)Math.Sqrt(-discriminant);

			float tentacleRotation = TentacleRotation(index);

			return new Vector2(distFromCenter - baseRadius, absThing).RotatedBy(tentacleRotation);
		}
		Vector2 TentacleOffsetUsingAngle(float index, float baseAngle, float baseRadius)
		{
			return new Vector2((float)Math.Sin(baseAngle + TentacleBaseAngleOffset(index)), 1) * TentacleRadialOffset(index, baseRadius);
		}
		Vector2 TentacleSegmentPosition(float index, float baseAngle, float baseRotation, float baseRadius, Vector2 originalBasePosition)
		{
			float effectiveIndex = index + TENTACLE_POSITION_INDEX_OFFSET;
			if (index >= -TENTACLE_POSITION_INDEX_OFFSET)
			{
				return TentacleEffectiveBasePosition(effectiveIndex, baseAngle, baseRotation, baseRadius, originalBasePosition) + TentacleOffsetUsingAngle(effectiveIndex, baseAngle, baseRadius).RotatedBy(baseRotation);
			}
			else
			{
				return TentacleSegmentPosition(-TENTACLE_POSITION_INDEX_OFFSET, baseAngle, baseRotation, baseRadius, originalBasePosition) + new Vector2(-TentacleSegmentWidth(effectiveIndex) * effectiveIndex, 0).RotatedBy(TentacleSegmentRotation(1 - TENTACLE_POSITION_INDEX_OFFSET, baseAngle, baseRotation, baseRadius, originalBasePosition));
			}
		}
		float TentacleSegmentRotation(float index, float baseAngle, float baseRotation, float baseRadius, Vector2 originalBasePosition)
		{
			return (TentacleSegmentPosition(index - 1, baseAngle, baseRotation, baseRadius, originalBasePosition) - TentacleSegmentPosition(index, baseAngle, baseRotation, baseRadius, originalBasePosition)).ToRotation();
		}
		float TentacleRadiusMult(float index)
		{
			return (index >= 0) ?
			ModUtils.Lerp((float)Math.Sqrt(1 - Math.Pow(index / EFFECTIVE_TENTACLE_SEGMENTS, 2)), (1 - index / EFFECTIVE_TENTACLE_SEGMENTS), 0.75f) :
			(float)Math.Sqrt(1 - Math.Pow((index + TENTACLE_HEAD_SEGMENTS) / TENTACLE_HEAD_SEGMENTS - 1, 2));
		}
		float TentacleRadius(float index)
		{
			return BASE_TENTACLE_TEXTURE_HEIGHT / MathHelper.TwoPi * SIDES_PER_TENTACLE_SEGMENT * TentacleRadiusMult(index);
		}

		//TODO: Make the tentacles more smoothly merge into the body?
		void DrawTentacleSegment(PriorityQueue<DrawData, float> drawDatas, SpriteBatch spriteBatch, Vector2 screenPos, float baseAngle, float baseRotation, Vector2 originalBasePosition, float baseRadius, int index, Color color)
		{
			Vector2 segmentPosition = TentacleSegmentPosition(index, baseAngle, baseRotation, baseRadius, originalBasePosition);

			//don't draw if offscreen
			int buffer = 80;
			if (!spriteBatch.GraphicsDevice.ScissorRectangle.Intersects(new Rectangle((int)(segmentPosition - screenPos).X - buffer, (int)(segmentPosition - screenPos).Y - buffer, buffer * 2, buffer * 2)))
			{
				return;
			}

			int segmentWidth = TentacleSegmentWidth(index);
			int drawWidthPerSegment = segmentWidth * 2;

			float segmentRotation = (TentacleSegmentPosition(index - 1, baseAngle, baseRotation, baseRadius, originalBasePosition) - segmentPosition).ToRotation();
			float segmentAngle = -baseAngle - TentacleBaseAngleOffset(index) + MathHelper.Pi / DRAWS_PER_TENTACLE_SEGMENT; //adding this last term helps minimize clipping

			//add this to segment angle to look better when not in drill form
			//TODO: This is kind of meh
			//TODO: More accurate depth calculation when TentacleRotation != 0
			segmentAngle += TentacleRotation(index);

			float segmentRadius = TentacleRadius(index);
			float scaleMultToMatch = (float)Math.Tan(MathHelper.Pi / DRAWS_PER_TENTACLE_SEGMENT) * segmentRadius;

			float effectiveIndexForFraming = index < 0 ? index * TENTACLE_HEAD_SEPARATION_SCALE_MULT : index;
			int segmentFramePoint = (int)((360 - drawWidthPerSegment) - (segmentWidth * (effectiveIndexForFraming + 64 - 1) % 64));

			float generalDepthFromAngle = (float)Math.Cos(baseAngle + TentacleBaseAngleOffset(index));
			float segmentDepthModifier = (generalDepthFromAngle + 0.5f) * (specialSegmentsHead * 65536f + index) + specialSegmentsHead * 512f;

			float scaleLength = (TentacleSegmentPosition(index, baseAngle, baseRotation, baseRadius, originalBasePosition) - TentacleSegmentPosition(index - 1, baseAngle, baseRotation, baseRadius, originalBasePosition)).Length() / TENTACLE_SEGMENT_SEPARATION;
			
			for (int i = 0; i < SIDES_PER_TENTACLE_SEGMENT; i++)
            {
				for (int j = 0; j < DRAWS_PER_TENTACLE_SIDE; j++)
				{
					float totalAngle = segmentAngle + i * MathHelper.TwoPi / SIDES_PER_TENTACLE_SEGMENT + j * MathHelper.TwoPi / DRAWS_PER_TENTACLE_SEGMENT;

					float offsetDist = (float)Math.Sin(totalAngle) * segmentRadius;
					Vector2 sectionOffset = new Vector2(0, offsetDist).RotatedBy(segmentRotation);

					Vector2 scale = new Vector2(scaleLength, (float)Math.Cos(totalAngle) * scaleMultToMatch / (BASE_TENTACLE_TEXTURE_HEIGHT / DRAWS_PER_TENTACLE_SIDE) * 2);

					float depthColorModifier = ((float)Math.Cos(totalAngle) + 2) / 3;
					Color depthModifiedColor = color.MultiplyRGB(new Color(new Vector3(depthColorModifier)));

					SpriteEffects bodyEffects = SpriteEffects.None;

					if (scale.Y > 0)
					{
						Rectangle frame = new Rectangle(segmentFramePoint, j * BASE_TENTACLE_TEXTURE_HEIGHT / DRAWS_PER_TENTACLE_SIDE, TENTACLE_SEGMENT_SEPARATION * 2, BASE_TENTACLE_TEXTURE_HEIGHT / DRAWS_PER_TENTACLE_SIDE);
						Vector2 origin = frame.Size() / 2;

						drawDatas.Enqueue(new DrawData(TextureAssets.Npc[Type].Value, segmentPosition + sectionOffset - screenPos, frame, depthModifiedColor, segmentRotation, origin, scale, bodyEffects, 0), (float)Math.Cos(totalAngle) * 0.1f + segmentDepthModifier);
					}
				}
			}
		}
        #endregion
    }


	public class ConvectiveWandererAcceleratingShot : ModProjectile
	{
		public override string Texture => "Polarities/Textures/Glow58";

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.scale = 0.5f;
			Projectile.width = 36;
			Projectile.height = 36;
			Projectile.alpha = 0;
			Projectile.timeLeft = 600;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;

			Projectile.hide = false;
		}

        public override void OnSpawn(IEntitySource source)
		{
			switch (Projectile.ai[0])
			{
				case 0:
					Projectile.timeLeft = 450;
					break;
				case 1:
					Projectile.timeLeft = 600;
					break;
				case 2:
					Projectile.timeLeft = 600;
					break;
			}
			Projectile.localAI[0] = Projectile.timeLeft;
		}

        public override void AI()
		{
			float acceleration = 1;
			switch (Projectile.ai[0])
            {
				case 0:
					acceleration = 0.975f;
					break;
				case 1:
					acceleration = 1.01f;
					break;
				case 2:
					acceleration = 1.04f;
					break;
			}

			Projectile.velocity *= acceleration;

			Projectile.rotation = Projectile.velocity.ToRotation();

			float progress = (float)Math.Pow(Projectile.timeLeft / Projectile.localAI[0], 0.25f);

			Vector2 oldCenter = Projectile.Center;
			Projectile.scale = 0.5f * (float)Math.Sqrt(progress);
			if (Projectile.timeLeft < 30) Projectile.scale *= Projectile.timeLeft / 30f;
			Projectile.width = (int)(Projectile.scale * 36);
			Projectile.height = (int)(Projectile.scale * 36);
			Projectile.Center = oldCenter;
		}

        public override bool? CanDamage()
        {
            return Projectile.timeLeft < 30 ? false : null;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffType<Incinerating>(), 60, true);
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			DrawLayer.AddProjectile<DrawLayerAdditiveAfterLiquids>(index);
		}

		//TODO: Trails? Sparkle? Slighly blobbier appearance?
		public override bool PreDraw(ref Color lightColor)
		{
			float scaleFactor = Projectile.velocity.Length() / 16f;

			float progress = (float)Math.Pow(Projectile.timeLeft / Projectile.localAI[0], 0.25f);
			float colorPulse = (3 + (float)Math.Sin(Projectile.timeLeft * MathHelper.TwoPi / 60f)) / 3;
			Color drawColor = ModUtils.ConvectiveFlameColor((float)Math.Pow(0.5f * progress * colorPulse, 2)) * progress;
			if (Projectile.timeLeft < 30) drawColor *= Projectile.timeLeft / 30f;

			Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), drawColor, Projectile.rotation, TextureAssets.Projectile[Type].Size() / 2, new Vector2(1 + scaleFactor, 1) * Projectile.scale, SpriteEffects.None, 0);
			return false;
		}
	}
}