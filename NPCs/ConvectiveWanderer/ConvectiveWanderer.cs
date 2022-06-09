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
using Polarities.Items.Weapons.Melee;
using Terraria.Localization;
using ReLogic.Utilities;

namespace Polarities.NPCs.ConvectiveWanderer
{
	[AutoloadBossHead]
	//TODO: Localization for projectiles
	//TODO: Localization for boss
	//TODO: Bestiary entry
	public class ConvectiveWanderer : ModNPC, IMultiHitboxSegmentUpdate
	{
		public override void Load()
		{
			#region Bestiary/Wiki image generation
			/*IL.Terraria.Main.UpdateMenu += Main_UpdateMenu;
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


						Main.spriteBatch.Begin((SpriteSortMode)0, new BlendState()
						{
							//custom blendstate to make it so outlines in darker places are more darker
							BlendFactor = Color.White,

							AlphaBlendFunction = BlendFunction.Add,
							AlphaSourceBlend = Blend.One,
							AlphaDestinationBlend = Blend.InverseSourceAlpha,

							ColorBlendFunction = BlendFunction.Add,
							ColorSourceBlend = Blend.SourceColor,
							ColorDestinationBlend = Blend.InverseSourceAlpha,
						},
						Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);

						Main.spriteBatch.GraphicsDevice.SetRenderTarget(capture2);
						Main.spriteBatch.GraphicsDevice.Clear(Color.Transparent);

						for (int i = 0; i < 4; i++)
						{
							Main.spriteBatch.Draw(capture, Main.GameViewMatrix.Translation + new Vector2(2f, 0).RotatedBy(i * MathHelper.PiOver2), null, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
						}

						Main.spriteBatch.End();
						Main.spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);
						Main.spriteBatch.GraphicsDevice.SetRenderTarget(capture2);

						Main.spriteBatch.Draw(capture, Main.GameViewMatrix.Translation, null, Color.Black, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);

						Main.spriteBatch.End();
						Main.spriteBatch.Begin((SpriteSortMode)0, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);
						Main.spriteBatch.GraphicsDevice.SetRenderTarget(capture2);

						Main.spriteBatch.Draw(capture, Main.GameViewMatrix.Translation, null, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);

						Main.spriteBatch.End();
						Main.spriteBatch.GraphicsDevice.SetRenderTarget(null);


						var stream = File.Create(filePath);
						capture2.SaveAsPng(stream, capture2.Width, capture2.Height);
						stream.Dispose();
						capture2.Dispose();
					}
				}
			});*/
			#endregion
		}

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
			NPC.damage = 32;
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

		float tentacleAngleMultiplier = 0f;
		float tentacleCompression = 1f;
		float tentacleTiltAngle = 0f;
		float tentacleCurveAmount = 0f;
		float tendrilOutwardness = 0f;
		float upDashTelegraphProgress = 0f;

		bool inPhase2 = false;


		public static void SpawnOn(Player player)
		{
			NPC wanderer = Main.npc[NPC.NewNPC(NPC.GetBossSpawnSource(player.whoAmI), (int)player.Center.X, (int)player.Center.Y + 1400, NPCType<ConvectiveWanderer>())];
			Main.NewText(Language.GetTextValue("Announcement.HasAwoken", wanderer.TypeName), 171, 64, 255);
			SoundEngine.PlaySound(SoundID.Roar, player.position);
		}
		#endregion

		#region AI
		public override void BossHeadRotation(ref float rotation)
		{
			rotation = NPC.rotation;
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

				if (NPC.ai[0] >= 0)
				{
					NPC.ai[0] = -1;
					NPC.ai[1] = 0;
				}
			}

			//Despawn if the player leaves the underworld for too long
			if (!player.ZoneUnderworldHeight)
            {
				NPC.localAI[3]++;
				if (NPC.localAI[3] > 600)
				{
					NPC.ai[0] = -1;
					NPC.ai[1] = 0;
				}
			}
			else
            {
				NPC.localAI[3] = 0;
            }

			//things for use with tentacle methods
			float tentacleBaseAngle = SegmentAngle(TENTACLE_ATTACH_SEGMENT_INDEX);
			float tentacleBaseRotation = SegmentRotation(TENTACLE_ATTACH_SEGMENT_INDEX) + MathHelper.PiOver2;
			float tentacleBaseRadius = SegmentRadius(TENTACLE_ATTACH_SEGMENT_INDEX) + TentacleRadius(0);
			Vector2 tentacleBasePosition = SegmentPosition(TENTACLE_ATTACH_SEGMENT_INDEX);

			//changeable ai values
			float rotationFade = 9f;
			float rotationAmount = 0.01f;
			float angleLerpAmount = 0.5f;
			float pulseLerpAmount = 0.1f;
			float angleSpeed = 0f;
			float pulseSpeed = MathHelper.TwoPi / 120f;
			bool gotoNextAttack = false;
			bool useDefaultRotation = true;
			float velocityFade = 0f;

			//drawcode values
			if (NPC.ai[0] >= 0)
			{
				//only reset these if we're in the actual fight and not despawning or dying or something
				tendrilOutwardness = 0f;
				tentacleCompression = 1f;
				tentacleTiltAngle = 0f;
				tentacleCurveAmount = 0f;
			}
			upDashTelegraphProgress = 0f;

			NPC.noGravity = true;

			#region Main AI
			//TODO: All attacks need sounds
			//TODO: Most attacks could use some particles
			//TODO: Some of the attacks could use a little screenshake
			switch (NPC.ai[0])
			{
                #region Despawning
                case -1:
					//despawning
					{
						tendrilOutwardness = ModUtils.Lerp(tendrilOutwardness, 0f, 0.1f);
						tentacleCompression = ModUtils.Lerp(tentacleCompression, 1f, 0.1f);
						tentacleTiltAngle = ModUtils.Lerp(tentacleTiltAngle, 0f, 0.1f);
						tentacleCurveAmount = ModUtils.Lerp(tentacleCurveAmount, 0f, 0.1f);

						NPC.velocity.Y += 0.3f;
						if (NPC.velocity.Y < 0)
						{
							NPC.velocity.Y *= 0.97f;
						}
						NPC.velocity.X *= 0.97f;

						int side = NPC.velocity.X > 0 ? 1 : -1;

						tentacleAngleMultiplier += (side * 0.1f - tentacleAngleMultiplier) / 10f;
						angleSpeed = NPC.velocity.Length() * 0.075f * tentacleAngleMultiplier;

						if (NPC.timeLeft > 10) NPC.timeLeft = 10;
					}
					break;
                #endregion

                #region Spawn Animation
                case 0:
					//TODO: spawn animation
					NPC.ai[0] = 1;
					break;
				#endregion

				#region Head Swing Attack
				case 1:
					{
						const int attackRepetitions = 4;
						const int attackFirstSetupExtraTime = 60;
						const int attackSetupTime = 60;
						const int attackFreezeTime = 13;
						const int attackSwingTime = 20;
						const int totalAttackTime = attackSetupTime + attackFreezeTime + attackSwingTime;

						//TODO: Better visual cues (both for charging time and for freeze)
						//TODO: Prim trail for swing?
						float attackProgress = (int)(NPC.ai[1] - attackFirstSetupExtraTime) % totalAttackTime;

						bool playerAhead = Vector2.Dot(NPC.Center - player.Center, new Vector2(-1, 0).RotatedBy(NPC.rotation)) > 0;
						int side = Vector2.Dot(NPC.Center - player.Center, new Vector2(0, -1).RotatedBy(NPC.rotation)) > 0 ? 1 : -1;

						const float goalDistance = 113f;

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

							tentacleAngleMultiplier += (side * 0.1f - tentacleAngleMultiplier) / 10f;
							angleSpeed = NPC.velocity.Length() * 0.03f * tentacleAngleMultiplier;
						}
						else if (attackProgress < attackSetupTime)
						{
							//if the player is ahead of us, chase them, else, try braking and turning around
							if (playerAhead)
							{
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
								//turn towards player
								float angularSpeed = 0.1f / (NPC.velocity.Length() + 1) * Math.Abs(Vector2.Dot((NPC.Center - player.Center).SafeNormalize(Vector2.Zero), new Vector2(1, 0).RotatedBy(NPC.rotation)));
								NPC.rotation += angularSpeed * side;
								useDefaultRotation = false;
								NPC.velocity = new Vector2(Math.Max(0.1f, NPC.velocity.Length() * 0.95f), 0).RotatedBy(NPC.rotation);
							}

							tentacleAngleMultiplier += (side * 0.1f - tentacleAngleMultiplier) / 10f;
							angleSpeed = NPC.velocity.Length() * 0.075f * tentacleAngleMultiplier;
						}
						else
						{
							if (attackProgress == attackSetupTime)
							{
								NPC.ai[2] = Vector2.Dot(NPC.Center - player.Center, new Vector2(0, -1).RotatedBy(NPC.rotation)) > 0 ? 1 : -1;
							}

							if (attackProgress < attackSetupTime + attackFreezeTime)
							{
								NPC.velocity = Vector2.Zero;

								tentacleAngleMultiplier += (NPC.ai[2] * 0.1f - tentacleAngleMultiplier) / 10f;
								angleSpeed = (attackSetupTime + attackFreezeTime - attackProgress) * 0.01f * NPC.ai[2];
							}
							else
							{
								//swing
								float angularSpeed = (6f * MathHelper.PiOver2 / attackSwingTime) * (attackProgress - (attackSetupTime + attackFreezeTime)) * ((attackSetupTime + attackFreezeTime + attackSwingTime) - attackProgress) / (attackSwingTime * attackSwingTime);

								NPC.rotation += angularSpeed * NPC.ai[2];
								useDefaultRotation = false;

								tentacleAngleMultiplier += (NPC.ai[2] * 0.1f - tentacleAngleMultiplier) / 10f;
								angleSpeed = angularSpeed * 2f * NPC.ai[2];

								//TODO: Consider bringing back the p1 projectiles and adding more projectiles in p2 (possibly staggered) if this is too easy
								//TODO: Possibly (in phase 2) charge after the last attack?
								if (inPhase2 && (attackProgress - (attackSetupTime + attackFreezeTime)) % 2 == 0 && attackProgress != attackSetupTime + attackFreezeTime) //exclude the first projectile because it looks weird
								{
									float projSpeed = 0.2f;
									Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(280, 0).RotatedBy(NPC.rotation), new Vector2(projSpeed, 0).RotatedBy(NPC.rotation), ProjectileType<ConvectiveWandererAcceleratingShot>(), 12, 2f, Main.myPlayer, ai0: 2f, ai1: inPhase2 ? 0.5f : 0f);
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
						const int attackRepetitions = 3;
						const int totalAttackSetupTime = 90;
						const int totalAttackDashTime = 90;
						const int totalAttackStoppedTime = 60;
						const int totalAttackTime = totalAttackSetupTime + totalAttackDashTime + totalAttackStoppedTime;
						const int extraTimeForProjectileDisappearance = 300;

						const float dashStartDistance = 3600f;
						const float dashStartVelocity = 110f;
						const float dashApproachDistance = 160f;

						float attackProgress = (int)NPC.ai[1] % totalAttackTime;

						int side = Vector2.Dot(NPC.Center - player.Center, new Vector2(0, -1).RotatedBy(NPC.rotation)) > 0 ? 1 : -1;
						bool playerAhead = Vector2.Dot(NPC.Center - player.Center, new Vector2(-1, 0).RotatedBy(NPC.rotation)) > 0;

						if (NPC.ai[1] >= totalAttackTime * attackRepetitions + totalAttackSetupTime)
						{
							//allow extra time for projectiles to despawn
							float timeLeft = totalAttackTime * attackRepetitions + totalAttackSetupTime + extraTimeForProjectileDisappearance - NPC.ai[1];

							if (timeLeft > 60)
							{
								//chase the player through the minefield
								Vector2 goalPosition = player.Center + (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * 100f;
								Vector2 goalVelocity = (goalPosition - NPC.Center) / 60f;
								NPC.velocity += (goalVelocity - NPC.velocity) / 30f;

								if (NPC.ai[1] == totalAttackTime * attackRepetitions + totalAttackSetupTime) NPC.velocity = goalVelocity;

								tentacleAngleMultiplier += (side * 0.1f - tentacleAngleMultiplier) / 10f;
								angleSpeed = NPC.velocity.Length() * 0.03f * tentacleAngleMultiplier;
							}
							else
							{
								Idle();
							}

							tendrilOutwardness = 0;
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

							tentacleAngleMultiplier += (side * 0.1f - tentacleAngleMultiplier) / 10f;
							angleSpeed = NPC.velocity.Length() * 0.03f * tentacleAngleMultiplier;
						}
						else if (attackProgress < totalAttackSetupTime + totalAttackDashTime)
						{
							float timeLeft = totalAttackSetupTime + totalAttackDashTime - attackProgress;

							if (attackProgress == totalAttackSetupTime)
							{
								//set clockwise vs. counterclockwise on the first attack, so it's the same every time, to ensure we get a good mixture of angles
								if (NPC.ai[1] == totalAttackSetupTime)
                                {
									NPC.ai[2] = Main.rand.NextBool() ? 1 : -1;
								}
								NPC.velocity = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(NPC.ai[2] * 0.33f) * dashStartVelocity;
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

							tentacleAngleMultiplier += (side * 0.1f - tentacleAngleMultiplier) / 10f;
							angleSpeed = NPC.velocity.Length() * 0.03f * tentacleAngleMultiplier;
						}
						else
						{
							NPC.velocity = Vector2.Zero;
							rotationAmount = 0f;

							tendrilOutwardness = 1f;

							//TODO: Drawcode tendrils glowing brighter in preparation

							if (attackProgress == totalAttackSetupTime + totalAttackDashTime + totalAttackStoppedTime / 2)
                            {
								//TODO: Create sound on projectile firing
								//TODO: Orthogonal flashes on projectile creation
								for (int i = 0; i < segmentPositions.Length - segmentsTailTendrils - 2; i++)
                                {
									if (TendrilIndex(i) == 0)
                                    {
										Vector2 position = SegmentPosition(i);
										float rotation = SegmentRotation(i);

										float radius = SegmentRadius(i);
										float angle = SegmentAngle(i);

										const float projSpeed = 32f;
										int projsPerSegment = inPhase2 ? 16 : 8;

										for (int j = 0; j < projsPerSegment; j++)
										{
											Projectile.NewProjectile(NPC.GetSource_FromAI(), position + new Vector2(0, radius).RotatedBy(rotation) * (float)Math.Sin(angle + j * MathHelper.TwoPi / projsPerSegment), new Vector2(0, projSpeed).RotatedBy(rotation) * (float)Math.Sin(angle + j * MathHelper.TwoPi / projsPerSegment), ProjectileType<ConvectiveWandererAcceleratingShot>(), 12, 2f, Main.myPlayer, ai0: 0f, ai1: inPhase2 ? 0.5f : 0f);
										}
                                    } 
                                }
                            }

							tentacleAngleMultiplier += (side * 0.1f - tentacleAngleMultiplier) / 10f;
							angleSpeed = NPC.velocity.Length() * 0.03f * tentacleAngleMultiplier;
						}

						NPC.ai[1]++;
						if (NPC.ai[1] == totalAttackTime * attackRepetitions + totalAttackSetupTime + extraTimeForProjectileDisappearance) //run an extra setup to move on after the final dash
						{
							gotoNextAttack = true;
						}
					}
					break;
				#endregion

				//note: this attack has some issues with the telegraph
				//it should feel less arbitrary, and should probably be visible earlier, while still movable, to clue the player in that they need to manipulate it
				#region Dash up and produce projectiles
				case 3:
                    {
						const int attackRepetitions = 4;
						int attackSetupTime = inPhase2 ? 45 : 120;
						int firstAttackExtraSetup = 120 - attackSetupTime;
						const int attackSetupFrozenTime = 30;
						const int attackDashTime = 150;
						int totalAttackTime = attackSetupTime + attackDashTime;
						const int extraTimeForProjectileDisappearance = 120;

						float attackProgress = (int)(NPC.ai[1] - firstAttackExtraSetup) % totalAttackTime;

						int side = Vector2.Dot(NPC.Center - player.Center, new Vector2(0, -1).RotatedBy(NPC.rotation)) > 0 ? 1 : -1;

						if (NPC.ai[1] >= totalAttackTime * attackRepetitions)
						{
							//allow extra time for projectiles to leave
							float timeLeft = totalAttackTime * attackRepetitions + extraTimeForProjectileDisappearance - NPC.ai[1];
							Idle();
						}
						else if (attackProgress < attackSetupTime)
						{
							//set up attack
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

							tentacleAngleMultiplier += (side * 0.1f - tentacleAngleMultiplier) / 10f;
							angleSpeed = NPC.velocity.Length() * 0.03f * tentacleAngleMultiplier;
						}
						else if (attackProgress < attackSetupTime + attackDashTime)
						{
							float timeLeft = attackSetupTime + attackDashTime - attackProgress;

							if (attackProgress == attackSetupTime)
							{
								NPC.velocity = new Vector2(0, -100);

								NPC.ai[2] = side;
							}

							NPC.velocity *= timeLeft / (timeLeft + 1);

							tentacleAngleMultiplier += (NPC.ai[2] * 0.1f - tentacleAngleMultiplier) / 10f;
							angleSpeed = NPC.velocity.Length() * 0.03f * tentacleAngleMultiplier;

							float radius = SegmentRadius(0);
							/*const int rowsProjectiles = 1;

							int numPerRow = inPhase2 ? 4 : 3;

							for (int j = 0; j < rowsProjectiles; j++)
							{
								float angle = SegmentAngle(0) - angleSpeed * j / (float)rowsProjectiles;

								for (int i = 0; i < numPerRow; i++)
								{
									float angleBonus = i * MathHelper.TwoPi / numPerRow;
									const float projSpeed = 2f;
									float projSpeedMult = (float)Math.Sin(angle + angleBonus);
									Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center - NPC.velocity * j / (float)rowsProjectiles + new Vector2(radius * projSpeedMult, 0), new Vector2(projSpeed * projSpeedMult, 0), ProjectileType<ConvectiveWandererAcceleratingShot>(), 12, 2f, Main.myPlayer, ai0: 1f, ai1: inPhase2 ? 0.5f : 0f);
								}
							}*/

							float projSpeed = 64f * (float)Math.Pow(1 - timeLeft / (attackDashTime + 4), 2);

							for (int i = -1; i <= 1; i += 2)
							{
								Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(radius * i, 0), NPC.velocity / 2 + new Vector2(projSpeed * i, 0), ProjectileType<ConvectiveWandererAcceleratingShot>(), 12, 2f, Main.myPlayer, ai0: 5f, ai1: inPhase2 ? 0.5f : 0f);

								//TODO: could be a random value for each dash instead of 1 to force you to look at the screen
								//1 has them stop at player height so it's probably the most dangerous value and therefore we may want to bias towards it
								if (attackProgress % 3 == 1) Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(radius * i, 0), NPC.velocity / 4 + new Vector2(projSpeed / 2f * i, 0), ProjectileType<ConvectiveWandererAcceleratingShot>(), 12, 2f, Main.myPlayer, ai0: 5f, ai1: inPhase2 ? 0.5f : 0f);
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

				//note: the final explosion from this attack is underwhelming, and it should chase the player for a bit longer before slowing down
				#region Circle player while charging up heat pulse
				case 4:
					{
						const int preSetupTime = 90;
						const int setupTime = 90;
						const int chaseTime = 600;
						const int runAwayTime = 300;
						const int totalAttackTime = preSetupTime + setupTime + chaseTime + runAwayTime;

						if (NPC.ai[1] < preSetupTime)
						{
							Idle();
						}
						else if (NPC.ai[1] < preSetupTime + setupTime + chaseTime)
                        {
							rotationAmount *= 1.5f;
							const float orbitDistance = 320f;

							if (NPC.ai[1] == preSetupTime)
                            {
								NPC.ai[2] = player.Center.X;
								NPC.ai[3] = player.Center.Y;
							}
							else
							{
								NPC.ai[2] += (player.Center.X - NPC.ai[2]) / 120f;
								NPC.ai[3] += (player.Center.Y - NPC.ai[3]) / 120f;
							}

							Vector2 arenaTargetPosition = new Vector2(NPC.ai[2], NPC.ai[3]);

							int side = Vector2.Dot(NPC.Center - arenaTargetPosition, new Vector2(0, -1).RotatedBy(NPC.rotation)) > 0 ? 1 : -1;

							if (NPC.ai[1] == preSetupTime + setupTime)
							{
								float ai0 = side > 0 ? 3 : 3.5f;
								Main.projectile[Projectile.NewProjectile(NPC.GetSource_FromAI(), arenaTargetPosition, Vector2.Zero, ProjectileType<ConvectiveWandererHeatVortex>(), 16, 2f, Main.myPlayer, ai0: NPC.whoAmI, ai1: inPhase2 ? 0.5f : 0f)].localAI[0] = ai0;
							}

							float goalDistance = (NPC.Center - arenaTargetPosition).Length();

							if (goalDistance < orbitDistance)
							{
								Vector2 goalVelocity = (NPC.Center - arenaTargetPosition).SafeNormalize(Vector2.Zero).RotatedBy(side * MathHelper.PiOver2) * totalAttackTime / 10f;
								NPC.velocity += (goalVelocity - NPC.velocity) / 20f;
							}
							else
							{
								float tangentAngle = (float)Math.Asin(orbitDistance / goalDistance);

								Vector2 goalVelocity = (arenaTargetPosition - NPC.Center).RotatedBy(-side * tangentAngle) / 10f;
								NPC.velocity += (goalVelocity - NPC.velocity) / 20f;
							}

							tentacleAngleMultiplier += (side * 0.1f - tentacleAngleMultiplier) / 10f;
							angleSpeed = NPC.velocity.Length() * 0.03f * tentacleAngleMultiplier;

							int segmentToSpawnParticleFrom = Main.rand.Next(1, numSegments * segmentsPerHitbox);
							Vector2 particleSpawnPos = SegmentPosition(segmentToSpawnParticleFrom) + new Vector2(0, Main.rand.NextFloat(-1f, 1f)).RotatedBy(SegmentRotation(segmentToSpawnParticleFrom)) * SegmentRadius(segmentToSpawnParticleFrom);
							float particleAngling = -Main.rand.NextFloat(0.2f, 0.8f) * side;

							ConvectiveWandererVortexParticle particle = Particle.NewParticle<ConvectiveWandererVortexParticle>(particleSpawnPos, (arenaTargetPosition - particleSpawnPos).RotatedBy(particleAngling).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(12f, 20f), (arenaTargetPosition - particleSpawnPos).ToRotation() + particleAngling, 0f, Scale: Main.rand.NextFloat(0.1f, 0.2f), Color: ModUtils.ConvectiveFlameColor((inPhase2 ? 1f : 0.4f) * Main.rand.NextFloat(0.5f, 1f)));
							particle.owner = NPC.whoAmI;
							particle.angling = particleAngling;
							ParticleLayer.AfterLiquidsAdditive.Add(particle);
						}
						else
                        {
							Idle();
                        }

						NPC.ai[1]++;
						if (NPC.ai[1] == totalAttackTime)
						{
							gotoNextAttack = true;
						}
					}
					break;
				#endregion

				//note: this attack isn't really harder in p2 currently, just more punishing if you mess up, I should probably like make it move out of the way more slowly or target the player more accurately or something
				//it's currently way too stunlocky as well it may honestly be better to just remove/totally redo it
				//it's also the only attack I'd currently consider actively unfair
				#region Tentacles spin and rotate outwards, producing projectiles
				case 5:
					{
						const int attackRepetitions = 4;
						const int extraStartSetupTime = 120;
						const int setupStartTime = 30;
						const int setupTime = 60;
						const int attackTime = 120;
						const int resetTime = 30;
						const int extraEndTime = 120;
						const int totalAttackTime = setupTime + attackTime + resetTime;

						float attackProgress = (int)(NPC.ai[1] - extraStartSetupTime) % totalAttackTime;

						int side = Vector2.Dot(NPC.Center - player.Center, new Vector2(0, -1).RotatedBy(NPC.rotation)) > 0 ? 1 : -1;

						if (NPC.ai[1] >= totalAttackTime * attackRepetitions + extraStartSetupTime)
                        {
							Idle();
                        }
						else if (attackProgress < setupTime)
                        {
							if (NPC.ai[1] == 0)
								NPC.ai[2] = side;
							if (attackProgress == 0 && NPC.ai[1] > extraStartSetupTime)
								NPC.ai[2] *= -1;

							if (attackProgress < 0)
							{
								if (attackProgress < -60)
								{
									Idle();
								}
								else
								{
									if ((NPC.Center - player.Center).Length() > 600f)
									{
										GoTowardsRadial(player.Center + new Vector2(0, -600).RotatedBy(NPC.Center.X > player.Center.X ? MathHelper.Pi / 3 : -MathHelper.Pi / 3), player.Center, -attackProgress);
									}
									else
									{
										NPC.ai[1] = extraStartSetupTime;
									}
								}
							}
							else
							{
								if (attackProgress < setupStartTime && (NPC.Center - player.Center).Length() > 400f)
								{
									float timeLeftForSetupStart = setupStartTime - attackProgress;

									//TODO: Don't point away from the player quite as much
									float offsetAmount = Math.Max(0, (600f - (NPC.Center - player.Center).Length()) / 200f) * MathHelper.Pi / 6f;
									Vector2 goalPosition = player.Center + (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * 400f;
									goalPosition = NPC.Center + (goalPosition - NPC.Center).RotatedBy(-side * offsetAmount);
									Vector2 goalVelocity = (goalPosition - NPC.Center) / timeLeftForSetupStart;
									NPC.velocity += (goalVelocity - NPC.velocity) / (float)Math.Sqrt(timeLeftForSetupStart);
								}
								else
								{
									if (Vector2.Dot((NPC.Center - player.Center).SafeNormalize(Vector2.Zero), new Vector2(-1, 0).RotatedBy(NPC.rotation)) > 0.5)
									{
										NPC.velocity = Vector2.Zero;
									}
									else
									{
										//turn towards player
										float angularSpeed = 0.075f / (NPC.velocity.Length() + 1) * Math.Abs(-0.5f + Vector2.Dot((NPC.Center - player.Center).SafeNormalize(Vector2.Zero), new Vector2(-1, 0).RotatedBy(NPC.rotation)));
										NPC.rotation += angularSpeed * side;
										useDefaultRotation = false;
										NPC.velocity = new Vector2(Math.Max(0.1f, NPC.velocity.Length() * 0.95f), 0).RotatedBy(NPC.rotation);
									}

									rotationAmount *= 2f;
								}
							}

							float timeLeft = setupTime - Math.Max(attackProgress, 0);

							tentacleAngleMultiplier += (-NPC.ai[2] * 0.1f * (1 - timeLeft / setupTime) - tentacleAngleMultiplier) / timeLeft;
							angleSpeed = NPC.velocity.Length() * 0.03f * tentacleAngleMultiplier + attackProgress / setupTime * NPC.ai[2] * 0.06f;

							tentacleCompression = timeLeft / setupTime;
						}
						else if (attackProgress < setupTime + attackTime)
						{
							float timeLeft = setupTime + attackTime - attackProgress;

							rotationAmount *= 2f;

							tentacleAngleMultiplier += (-NPC.ai[2] * 0.1f * timeLeft / attackTime - tentacleAngleMultiplier) / 10f;
							angleSpeed = NPC.ai[2] * 0.06f;
							tentacleCompression = 0f;

							float tentacleRotProgress = (1 - timeLeft / attackTime);
							tentacleTiltAngle = tentacleRotProgress * tentacleRotProgress * (3 - 2 * tentacleRotProgress) * MathHelper.Pi * 0.625f;
							tentacleCurveAmount = -tentacleTiltAngle / 8;

							NPC.velocity = Vector2.Zero;

							if (inPhase2)
							{
								if (attackProgress == setupTime)
								{
									for (int i = 0; i < NUM_TENTACLES; i++)
									{
										Vector2 spot = TentacleSegmentPosition(32, tentacleBaseAngle + i * MathHelper.TwoPi / NUM_TENTACLES, tentacleBaseRotation, tentacleBaseRadius, tentacleBasePosition);
										float angle = MathHelper.Pi + TentacleSegmentRotation(32, tentacleBaseAngle + i * MathHelper.TwoPi / NUM_TENTACLES, tentacleBaseRotation, tentacleBaseRadius, tentacleBasePosition);

										Projectile.NewProjectile(NPC.GetSource_FromAI(), spot, new Vector2(1, 0).RotatedBy(angle), ProjectileType<ConvectiveWandererTentacleDeathray>(), 12, 2f, Main.myPlayer, ai0: i, ai1: NPC.whoAmI);
									}
								}
							}
							if (attackProgress % 5 == 0)
							{
								for (int i = 0; i < NUM_TENTACLES; i++)
								{
									Vector2 spot = TentacleSegmentPosition(32, tentacleBaseAngle + i * MathHelper.TwoPi / NUM_TENTACLES, tentacleBaseRotation, tentacleBaseRadius, tentacleBasePosition);
									float angle = MathHelper.Pi + TentacleSegmentRotation(32, tentacleBaseAngle + i * MathHelper.TwoPi / NUM_TENTACLES, tentacleBaseRotation, tentacleBaseRadius, tentacleBasePosition);

									Projectile.NewProjectile(NPC.GetSource_FromAI(), spot, new Vector2(2, 0).RotatedBy(angle), ProjectileType<ConvectiveWandererAcceleratingShot>(), 12, 2f, Main.myPlayer, ai0: 1f, ai1: inPhase2 ? 0.5f : 0f);
								}
							}
						}
						else
						{
							NPC.velocity = Vector2.Zero;

							rotationAmount *= 2f;

							float timeLeft = setupTime + attackTime + resetTime - attackProgress;

							tentacleAngleMultiplier = NPC.ai[2] * 0.1f * (1 - timeLeft / resetTime);
							angleSpeed = NPC.ai[2] * 0.06f * timeLeft / resetTime;
							tentacleCompression = 1 - timeLeft / resetTime;
							float tentacleRotProgress = timeLeft / resetTime;
							tentacleTiltAngle = tentacleRotProgress * tentacleRotProgress * (3 - 2 * tentacleRotProgress) * MathHelper.Pi * 0.625f;
							tentacleCurveAmount = -tentacleTiltAngle / 8;
						}

						NPC.ai[1]++;
						if (NPC.ai[1] == totalAttackTime * attackRepetitions + extraStartSetupTime + extraEndTime)
						{
							gotoNextAttack = true;
						}
					}
					break;
				#endregion

				//note: have the boss drill into the ground/lava surface to trigger this attack, helps make it feel less disconnected
				//note: this attack could probably be trivialized by hollowing out everything everywhere and ceiling grappling or using inf flight so I should prevent that by either adding a height cap or making the pillar heights adaptive
				#region Create flame pillars from terrain, while dashing at them from below and to the side
				case 6:
					{
						const int setupTime = 60;
						const int mainAttackTime = 720;
						const int windDownTime = 40;
						const int totalAttackTime = setupTime + mainAttackTime + windDownTime;

						if (NPC.ai[1] == 0)
                        {
                            NPC.ai[2] = Vector2.Dot(NPC.Center - (player.Center + new Vector2(0, 600)), new Vector2(0, -1).RotatedBy(NPC.rotation)) > 0 ? 1 : -1;
						}

						//old motion from before I added the dashes, could be useful for something else
						/*Vector2 orbitPoint = player.Center + new Vector2((float)Math.Sin(NPC.ai[1] * 0.01f) * NPC.ai[2] * 300, 500);
						float angle = ((NPC.Center - orbitPoint) * new Vector2(1, 4)).ToRotation();
						float dist = ((NPC.Center - orbitPoint) * new Vector2(1, 4)).Length();
						Vector2 goalPosition = orbitPoint + new Vector2(800, 0).RotatedBy(angle + MathHelper.PiOver2 * dist / 800f * NPC.ai[2]) * new Vector2(1, 0.25f);

						GoTowardsRadial(goalPosition, player.Center, 45f);*/

						//TODO: This attack should have some screenshakes due to the environmental effects

						if (NPC.ai[1] >= setupTime + mainAttackTime)
                        {
							Idle();
                        }
						else if ((NPC.ai[1] - setupTime) % 180 < 60)
						{
							Vector2 goalPosition = player.Center + new Vector2(-NPC.ai[2] * 600, 400);
							GoTowardsRadial(goalPosition, player.Center, 60 - (NPC.ai[1] - setupTime) % 180);
						}
						else
						{
							//dash at player
							if ((NPC.ai[1] - setupTime) % 180 == 60)
							{
								NPC.velocity = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 8f;

								NPC.ai[2] *= -1;
							}
							else
							{
								NPC.velocity *= (float)Math.Pow(8, 1 / 120f);
							}
							//TODO: Try making sideways projectiles during the dash and see if that's still avoidable
						}

						if (NPC.ai[1] % (inPhase2 ? 40 : 60) == 0 && NPC.ai[1] >= setupTime && NPC.ai[1] < totalAttackTime - windDownTime)
                        {
							const float minOffset = 200f;
							const float maxOffset = 400f;

							bool playerInLava = Framing.GetTileSafely(player.Center.ToTileCoordinates()).LiquidAmount == 255;

							float startingOffset = Main.rand.NextFloat(minOffset, maxOffset);

							float startingProportion = Main.rand.NextBool(3) ? 0 : Main.rand.NextFloat(1f);

							float projPositionX = player.Center.X + startingOffset * startingProportion;
							for (int i = 0; i < 20; i++)
                            {
								if (projPositionX >= Main.maxTilesX * 16) break;

								//new projectile
								CreateFlamePillar();

								//increase proj offset
								projPositionX += Main.rand.NextFloat(minOffset, maxOffset);
							}
							projPositionX = player.Center.X - startingOffset * (1 - startingProportion);
							for (int i = 0; i < 20; i++)
							{
								if (projPositionX <= 0) break;

								//new projectile
								CreateFlamePillar();

								//increase proj offset
								projPositionX -= Main.rand.NextFloat(minOffset, maxOffset);
							}

							void CreateFlamePillar()
							{
								Vector2 projPosition = new Vector2(projPositionX, 16 * (int)(player.Center.Y / 16));

								if (playerInLava)
								{
									if (Framing.GetTileSafely(projPosition.ToTileCoordinates()).HasUnactuatedTile && Main.tileSolid[Framing.GetTileSafely(projPosition.ToTileCoordinates()).TileType])
									{
										while (Framing.GetTileSafely(projPosition.ToTileCoordinates()).HasUnactuatedTile && Main.tileSolid[Framing.GetTileSafely(projPosition.ToTileCoordinates()).TileType])
										{
											projPosition.Y -= 16;

											if (projPosition.Y <= 0) break;
										}
										projPosition.Y += 16;
									}
									else
									{
										while (!Framing.GetTileSafely(projPosition.ToTileCoordinates()).HasUnactuatedTile && Main.tileSolid[Framing.GetTileSafely(projPosition.ToTileCoordinates()).TileType])
										{
											projPosition.Y += 16;

											if (projPosition.Y >= Main.maxTilesY * 16) break;
										}
									}
								}
								else
								{
									if ((Framing.GetTileSafely(projPosition.ToTileCoordinates()).HasUnactuatedTile && Main.tileSolid[Framing.GetTileSafely(projPosition.ToTileCoordinates()).TileType]) || (Framing.GetTileSafely(projPosition.ToTileCoordinates()).LiquidAmount == 255))
									{
										while ((Framing.GetTileSafely(projPosition.ToTileCoordinates()).HasUnactuatedTile && Main.tileSolid[Framing.GetTileSafely(projPosition.ToTileCoordinates()).TileType]) || (Framing.GetTileSafely(projPosition.ToTileCoordinates()).LiquidAmount == 255))
										{
											projPosition.Y -= 16;

											if (projPosition.Y <= 0) break;
										}
										projPosition.Y += 16;
									}
									else
									{
										while (!((Framing.GetTileSafely(projPosition.ToTileCoordinates()).HasUnactuatedTile && Main.tileSolid[Framing.GetTileSafely(projPosition.ToTileCoordinates()).TileType]) || (Framing.GetTileSafely(projPosition.ToTileCoordinates()).LiquidAmount == 255)))
										{
											projPosition.Y += 16;

											if (projPosition.Y >= Main.maxTilesY * 16) break;
										}
									}
									//adjust to match lava surface
									if (Framing.GetTileSafely(projPosition.ToTileCoordinates()).LiquidAmount > 0)
									{
										projPosition.Y -= 16 * (Framing.GetTileSafely(projPosition.ToTileCoordinates()).LiquidAmount / 255f);
									}
								}
								projPosition.Y += 16;
								Projectile.NewProjectile(NPC.GetSource_FromAI(), projPosition, new Vector2(0, -1), ProjectileType<ConvectiveWandererFlamePillar>(), 12, 2f, player.whoAmI, ai1: inPhase2 ? 0.5f : 0f);
							}
						}

						tentacleAngleMultiplier += (NPC.ai[2] * 0.1f - tentacleAngleMultiplier) / 10f;
						angleSpeed = NPC.velocity.Length() * 0.075f * tentacleAngleMultiplier;

						NPC.ai[1]++;
						if (NPC.ai[1] == totalAttackTime)
                        {
							gotoNextAttack = true;
                        }
                    }
					break;
				#endregion

				//note: p2 version may be too difficult (not sure), rotation speed feels a little fast at times, boss should maybe slow down and not rotate a little before spawning the flamethrower? (only do that last one if I get complaints about the flamethrower I think it's fine but I could be wrong)
				#region Tentacles point backwards, boss shoots giant mouth flamethrower
				case 7:
					{
						const int attackRepetitions = 3;
						const int startSetupTime = 120;
						const int setupTime = 60;
						const int attackTime = 300;
						const int windDownTime = 60;
						const int endWindDownTime = 60;
						const int totalAttackTime = setupTime + attackTime + windDownTime;

						int side = Vector2.Dot(NPC.Center - player.Center, new Vector2(0, -1).RotatedBy(NPC.rotation)) > 0 ? 1 : -1;

						float attackProgress = (int)(NPC.ai[1] - startSetupTime) % totalAttackTime;

						if (NPC.ai[1] >= startSetupTime + totalAttackTime * attackRepetitions)
                        {
							Idle();
						}
						else if (attackProgress < setupTime)
						{
							NPC.ai[2] = side;

							float timeLeft = setupTime - Math.Max(0, attackProgress);

							angleSpeed = NPC.ai[2] * 0.1f * (1 - timeLeft / setupTime);
							tentacleAngleMultiplier += (-0.1f * angleSpeed - tentacleAngleMultiplier) * 0.1f;
							tentacleCompression = timeLeft / setupTime;

							float tentacleRotProgress = (1 - timeLeft / setupTime);
							tentacleTiltAngle = tentacleRotProgress * tentacleRotProgress * (3 - 2 * tentacleRotProgress) * MathHelper.TwoPi * 7f / 16f;
							tentacleCurveAmount = -2f * tentacleRotProgress * (1 - tentacleRotProgress) * (1 - tentacleCompression);

							Vector2 goalPosition = player.Center + (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * Math.Min(200f, (NPC.Center - player.Center).Length());
							Vector2 goalVelocity = (goalPosition - NPC.Center) / (timeLeft + 5);
							NPC.velocity += (goalVelocity - NPC.velocity) / Math.Max(1, timeLeft - 30f);
						}
						else if (attackProgress < setupTime + attackTime)
                        {
							//set direction to turn
							if (attackProgress == setupTime)
                            {
								NPC.ai[2] = side;
                            }

							//turn in one direction
							float angularSpeed = 0.02f / (NPC.velocity.Length() + 1);
							NPC.rotation += angularSpeed * NPC.ai[2];
							useDefaultRotation = false;
							rotationAmount *= 2f;//1.5f;
							NPC.velocity = new Vector2(Math.Max(0.1f, NPC.velocity.Length() * 0.9f), 0).RotatedBy(NPC.rotation);

							if (attackProgress == setupTime)
							{
								Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(80, 0).RotatedBy(NPC.rotation), new Vector2(1, 0).RotatedBy(NPC.rotation), ProjectileType<ConvectiveWandererFlamethrower>(), 16, 2f, Main.myPlayer, ai0: NPC.whoAmI, ai1: inPhase2 ? 0.5f : 0);
							}
							if ((attackProgress - setupTime) % 60 == 0 && attackProgress > setupTime)
                            {
								int numProjectiles = (inPhase2 ? 8 : 4);
								for (int i = 0; i < numProjectiles; i++)
								{
									Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(112, 0).RotatedBy(NPC.rotation), new Vector2(4, 0).RotatedBy(NPC.rotation + i * MathHelper.TwoPi / numProjectiles + MathHelper.TwoPi / 8f), ProjectileType<ConvectiveWandererAcceleratingShot>(), 12, 2f, Main.myPlayer, ai0: 1, ai1: inPhase2 ? 0.5f : 0);
								}
                            }

							angleSpeed = NPC.ai[2] * 0.1f;
							tentacleAngleMultiplier += (-0.1f * angleSpeed - tentacleAngleMultiplier) * 0.1f;
							tentacleCompression = 0;
							tentacleTiltAngle = MathHelper.TwoPi * 7f / 16f;
							tentacleCurveAmount = 0;
						}
						else
						{
							float timeLeft = setupTime + attackTime + windDownTime - attackProgress;

							NPC.rotation = Utils.AngleLerp(NPC.rotation, (player.Center - NPC.Center).ToRotation(), 0.05f);
							useDefaultRotation = false;
							rotationAmount *= 2f;
							velocityFade = 0.9f;
							NPC.velocity = new Vector2(32f * (timeLeft / windDownTime) * (1 - timeLeft / windDownTime), 0).RotatedBy(NPC.rotation + MathHelper.Pi);

							tentacleCompression = 1 - timeLeft / windDownTime;

							angleSpeed = NPC.ai[2] * 0.1f * (timeLeft / windDownTime);
							tentacleAngleMultiplier += (-0.1f * angleSpeed - tentacleAngleMultiplier) * 0.1f;
							float tentacleRotProgress = timeLeft / windDownTime;
							tentacleTiltAngle = tentacleRotProgress * tentacleRotProgress * (3 - 2 * tentacleRotProgress) * MathHelper.TwoPi * 7f / 16f;
							tentacleCurveAmount = 2f * tentacleRotProgress * (1 - tentacleRotProgress) * (1 - tentacleCompression);
						}

						NPC.ai[1]++;
						if (NPC.ai[1] == startSetupTime + endWindDownTime + totalAttackTime * attackRepetitions)
						{
							gotoNextAttack = true;
						}
					}
					break;
				#endregion

				//TODO: This attack needs more charging visuals coming from the boss itself
				#region Open tentacles and cup them forwards, spin rapidly to produce a giant sphere of heat, then like shoot it at the player
				case 8:
                    {
						const int startSetupTime = 120;
						const int setupTime = 60;
						const int attackRepetitions = 4;
						const int mainAttackPartTime = 240;
						const int mainAttackTime = attackRepetitions * mainAttackPartTime;
						const int windDownTime = 60;
						const int endWindDownTime = 60;
						const int totalAttackTime = setupTime + mainAttackTime + windDownTime;

						int side = Vector2.Dot(NPC.Center - player.Center, new Vector2(0, -1).RotatedBy(NPC.rotation)) > 0 ? 1 : -1;

						float attackProgress = (int)(NPC.ai[1] - startSetupTime) % totalAttackTime;

						if (NPC.ai[1] >= startSetupTime + totalAttackTime)
						{
							//end-attack-series wind down
							Idle();
						}
						else if (attackProgress < setupTime)
						{
							//setup
							if (NPC.ai[1] == 0) NPC.ai[2] = side;

							float timeLeft = setupTime - Math.Max(0, attackProgress);

							Vector2 goalPosition = player.Center;
							Vector2 goalVelocity = (goalPosition - NPC.Center) / 120f;
							NPC.velocity += (goalVelocity - NPC.velocity) / Math.Max(1, timeLeft - 30f);

							angleSpeed = 0f;
							tentacleAngleMultiplier += (-0.3f * angleSpeed - tentacleAngleMultiplier) * 0.1f;
							tentacleCompression = timeLeft / setupTime;

							float tentacleRotProgress = (1 - timeLeft / setupTime);
							tentacleTiltAngle = tentacleRotProgress * tentacleRotProgress * (3 - 2 * tentacleRotProgress) * 1.4f;
							tentacleCurveAmount = -tentacleTiltAngle / 2f;
						}
						else if (attackProgress < setupTime + mainAttackTime)
						{
							//attack
							float timeLeft = (setupTime + mainAttackTime - attackProgress) % mainAttackPartTime;

							if (timeLeft == 0)
							{
								Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(240, 0).RotatedBy(NPC.rotation), NPC.velocity, ProjectileType<ConvectiveWandererChargedShot>(), 16, 2f, Main.myPlayer, ai0: NPC.whoAmI, ai1: inPhase2 ? 0.5f : 0);
							}

							Vector2 goalPosition = player.Center;
							Vector2 goalVelocity = (goalPosition - NPC.Center) / 120f * (2f - timeLeft / mainAttackPartTime);

							float accFactor = (timeLeft + 1) * (mainAttackPartTime - timeLeft) / mainAttackPartTime;
							NPC.velocity += (goalVelocity - NPC.velocity) / accFactor;

							float angleForRotation = ((attackProgress - setupTime) % (mainAttackPartTime * 2)) / mainAttackPartTime * MathHelper.Pi;
							angleSpeed = NPC.ai[2] * (float)Math.Sin(angleForRotation) * 0.1f;
							tentacleAngleMultiplier += (-0.3f * angleSpeed - tentacleAngleMultiplier) * 0.1f;
							tentacleCompression = 0;
							tentacleTiltAngle = 1.4f;
							tentacleCurveAmount = -tentacleTiltAngle / 2f;
						}
						else
						{
							//wind down
							float timeLeft = setupTime + mainAttackTime + windDownTime - attackProgress;

							NPC.velocity *= 0.98f;

							angleSpeed = 0f;
							tentacleAngleMultiplier += (-0.3f * angleSpeed - tentacleAngleMultiplier) * 0.1f;
							tentacleCompression = 1 - timeLeft / windDownTime;

							float tentacleRotProgress = timeLeft / windDownTime;
							tentacleTiltAngle = tentacleRotProgress * tentacleRotProgress * (3 - 2 * tentacleRotProgress) * 1.4f;
							tentacleCurveAmount = -tentacleTiltAngle / 2f;
						}

						NPC.ai[1]++;
						if (NPC.ai[1] == startSetupTime + endWindDownTime + totalAttackTime)
						{
							gotoNextAttack = true;
						}
					}
					break;
				#endregion

				//some sonic attack (a charge?) based on the clicking sounds of that one polychaete
			}

			//TODO: I feel like a last-ditch attack of some sort would fit this boss pretty well
			//TODO: Make attacks using the tentacles not start occuring until a certain health threshold
			if (gotoNextAttack)
			{
				//TODO: Change this health threshold depending on difficulty
				if (NPC.life * 2 < NPC.lifeMax)
				{
					inPhase2 = true;
					//TODO: Proper phase transition
				}

				//TODO: go to next attack with a SC/Sentinel-like system
				NPC.ai[0] = (NPC.ai[0] + Main.rand.Next(0, 7)) % 8 + 1;
				NPC.ai[1] = 0;
			}


			//TODO: Add a bit of occasional random variation to some of the attacks to ensure their positioning is varied
			//(This should only be varied some of the time, a la sun pixie's projectile rings)

			//yes, I stole this from sun pixie
			//avoiding the player is important okay
			//TODO: Make this take into account our direction better to avoid sudden curvature
			//TODO: Maximum turn parameter
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
			void Idle()
			{
				int side = Vector2.Dot(NPC.Center - player.Center, new Vector2(0, -1).RotatedBy(NPC.rotation)) > 0 ? 1 : -1;

				Vector2 goalPosition = player.Center + (NPC.Center - player.Center).SafeNormalize(Vector2.Zero).RotatedBy(side) * 1200f;
				Vector2 goalVelocity = (goalPosition - NPC.Center) / 30f;
				NPC.velocity += (goalVelocity - NPC.velocity) / 30f;

				tentacleAngleMultiplier += (side * 0.1f - tentacleAngleMultiplier) / 10f;
				angleSpeed = NPC.velocity.Length() * 0.03f * tentacleAngleMultiplier;
			}

			#endregion

			NPC.noGravity = true;

			tendrilOutwardness = Math.Clamp(tendrilOutwardness, 0, 1);

			//update segment positions
			segmentPositions[0] = NPC.Center + NPC.velocity;

			Vector2 rotationGoal = Vector2.Zero;
			if (!useDefaultRotation) //uses whatever custom rotation value we've set to determine the position of the first segment instead of the default worm behavior
			{
				segmentPositions[1] = segmentPositions[0] + new Vector2(-segmentSeparation, 0).RotatedBy(NPC.rotation);
				rotationGoal = segmentPositions[1] - segmentPositions[0];
			}

			for (int i = (useDefaultRotation ? 1 : 2); i < segmentPositions.Length; i++)
			{
				//this is mainly just used when going backwards
				segmentPositions[i] += NPC.velocity * (float)Math.Pow(velocityFade, i);

				if (i > 1) rotationGoal = ((rotationFade - 1) * rotationGoal + (segmentPositions[i - 1] - segmentPositions[i - 2])) / rotationFade;
				segmentPositions[i] = segmentPositions[i - 1] + (rotationAmount * rotationGoal + (segmentPositions[i] - segmentPositions[i - 1]).SafeNormalize(Vector2.Zero)).SafeNormalize(Vector2.Zero) * segmentSeparation;
			}

			if (useDefaultRotation)
				NPC.rotation = SegmentRotation(0);

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


			//position hitbox segments
			//the order in which we do this matters as it determines hit priority
			//doing the tentacles first ensures the tentacles always shield the head

			//position tentacle hitbox segments
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

		//TODO: Make it so that we can actually hit the head segment even when only partially open
		public void MultiHitboxSegmentUpdate(NPC npc, RectangleHitbox mostRecentHitbox)
		{
			if (mostRecentHitbox.index < NUM_TENTACLES * HITBOXES_PER_TENTACLE)
			{
				//hitting tentacle segments is bad
				npc.HitSound = SoundID.Tink;
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

		//TODO: Smoother transition from head to body
        #region Drawcode
        //a whole bunch of drawing stuff and helper methods
        //abandon all hope ye who enter here

        //a PriorityQueue that stores our drawData
        PriorityQueue<DrawData, float> drawDatas;

		//The maximum capacity potentially required by drawDatas
		int MAX_DRAW_CAPACITY = 8740;

		public const int NUM_TENTACLES = 8;

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (NPC.IsABestiaryIconDummy)
			{
				//TODO: Bestiary portrait
				return false;
			}

			if (DrawLayer.IsActive<DrawLayerAdditiveAfterLiquids>())
            {
				//stuff to draw additively after most things (mostly telegraphs and effects
				if (upDashTelegraphProgress > 0)
                {
					//TODO: Possibly replace with/add a screenshake
					Main.spriteBatch.Draw(Textures.Glow256.Value, NPC.Center - screenPos, Textures.Glow256.Frame(), ModUtils.ConvectiveFlameColor(upDashTelegraphProgress * upDashTelegraphProgress * 0.125f + (inPhase2 ? 0.875f : 0f)) * upDashTelegraphProgress, 0f, Textures.Glow256.Size() / 2, new Vector2(1, 64), SpriteEffects.None, 0);
                }
            }
			else
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
		public Vector2 SegmentPosition(float index)
		{
			if (index % 1 == 0)
				return (BaseSegmentPosition((int)index) + BaseSegmentPosition((int)index - 1)) / 2;
			return Vector2.Lerp(SegmentPosition((int)Math.Floor(index)), SegmentPosition((int)Math.Ceiling(index)), index % 1);
		}
		public float SegmentRotation(float index)
		{
			if (index % 1 == 0)
				return (BaseSegmentPosition((int)index - 1) - BaseSegmentPosition((int)index)).ToRotation();
			return Utils.AngleLerp(SegmentRotation((int)Math.Floor(index)), SegmentRotation((int)Math.Ceiling(index)), index % 1);
		}

		//TODO: Enable opening/closing mouth via PulseScale or something similar
		public float SegmentAngle(int index)
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
		public float SegmentRadius(int index)
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

				int inc = inPhase2 ? 1 : 0;

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
						Color depthModifiedColor = color.MultiplyRGB(new Color(new Vector3(depthColorModifier)));

						SpriteEffects bodyEffects = SpriteEffects.None;

						//allow drawing of backwards segments if in the head, but prune otherwise for efficiency since non-head reversed segments are obscured
						if (index < 1 && scale.Y < 0)
						{
							bodyEffects = SpriteEffects.FlipVertically;
							scale.Y = -scale.Y;
						}

						if (scale.Y > 0)
						{
							Rectangle frame = new Rectangle(segmentFramePoint, j * BASE_TEXTURE_HEIGHT / DRAWS_PER_SIDE + (BASE_TEXTURE_HEIGHT + 2) * 2 * inc, drawWidthPerSegment, BASE_TEXTURE_HEIGHT / DRAWS_PER_SIDE);
							Vector2 origin = frame.Size() / 2;

							drawDatas.Enqueue(new DrawData(TextureAssets.Npc[Type].Value, segmentPosition + sectionOffset - screenPos, frame, depthModifiedColor, segmentRotation, origin, scale, bodyEffects, 0), (float)Math.Cos(totalAngle) - globalDepthModifier);
						}

						for (int finIndex = -1; finIndex <= 1; finIndex += 2)
						{
							//fins are rotated and don't scale with extra pulsing
							float offsetAngle = totalAngle + finIndex * MathHelper.Pi / 4;

							Vector2 finScale = new Vector2(1, (float)Math.Abs(Math.Sin(offsetAngle)) * scaleMultToMatch / (BASE_TEXTURE_HEIGHT / DRAWS_PER_SIDE) * 2 / segmentPulseScale);

							Rectangle finFrame = new Rectangle(segmentFramePoint, (BASE_TEXTURE_HEIGHT + 2) + j * BASE_TEXTURE_HEIGHT / DRAWS_PER_SIDE + (BASE_TEXTURE_HEIGHT + 2) * 2 * inc, drawWidthPerSegment, BASE_TEXTURE_HEIGHT / DRAWS_PER_SIDE - 2);
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
			//TODO: Adjust color to be darker when cold and more unilaterally blue-white when hot
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
					Color tendrilColor = ModUtils.ConvectiveFlameColor(tendrilProgress * tendrilProgress * 0.5f + (inPhase2 ? 0.5f : 0)).MultiplyRGB(new Color(new Vector3(depthColorModifier))) * tendrilProgress;
					float tendrilWidth = tendrilProgress * 2f + 2f;

					drawDatas.Enqueue(new DrawData(Textures.PixelTexture.Value, startPosition - screenPos, Textures.PixelTexture.Frame(), color.MultiplyRGBA(tendrilColor), (endPosition - startPosition).ToRotation(), new Vector2(0, 0.5f), new Vector2((endPosition - startPosition).Length(), tendrilWidth), SpriteEffects.None, 0), (float)Math.Cos(totalAngle1) * 64f - globalSegmentDepthModifier);
				}
			}
		}

		const int TENTACLE_SEGMENTS = 32;
		const int TENTACLE_HEAD_SEGMENTS = 8;
		const int TENTACLE_SEGMENT_SEPARATION = 8;

		public const int TENTACLE_ATTACH_SEGMENT_INDEX = -4;

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

        float TentacleBaseAngleOffset(float index)
		{
			return (index <= 0) ? 0 :
				index * tentacleAngleMultiplier;
		}
		float TentacleRotation(float index)
        {
			return tentacleTiltAngle + tentacleCurveAmount * index / EFFECTIVE_TENTACLE_SEGMENTS;
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

			float goalRadius = ModUtils.Lerp(baseRadius, TentacleRadius(index) / sideScaleMult, tentacleCompression);

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
		public Vector2 TentacleSegmentPosition(float index, float baseAngle, float baseRotation, float baseRadius, Vector2 originalBasePosition)
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
		public float TentacleSegmentRotation(float index, float baseAngle, float baseRotation, float baseRadius, Vector2 originalBasePosition)
		{
			return (TentacleSegmentPosition(index - 1, baseAngle, baseRotation, baseRadius, originalBasePosition) - TentacleSegmentPosition(index, baseAngle, baseRotation, baseRadius, originalBasePosition)).ToRotation();
		}
		float TentacleRadiusMult(float index)
		{
			return (index >= 0) ?
			ModUtils.Lerp((float)Math.Sqrt(1 - Math.Pow(index / EFFECTIVE_TENTACLE_SEGMENTS, 2)), (1 - index / EFFECTIVE_TENTACLE_SEGMENTS), 0.75f) :
			(float)Math.Sqrt(1 - Math.Pow((index + TENTACLE_HEAD_SEGMENTS) / TENTACLE_HEAD_SEGMENTS - 1, 2));
		}
		public float TentacleRadius(float index)
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
			float segmentAngle = -baseAngle - TentacleBaseAngleOffset(index)
				+ MathHelper.Pi / DRAWS_PER_TENTACLE_SEGMENT; //helps minimize clipping

			//adjust angle when pointing backwards
			//I don't know why this is the way it is
			if (TentacleRotation(index) > MathHelper.PiOver2)
            {
				segmentAngle += MathHelper.Pi;
				segmentAngle *= -1;
				segmentAngle += MathHelper.Pi / 6f;
			}

			float segmentRadius = TentacleRadius(index);
			float scaleMultToMatch = (float)Math.Tan(MathHelper.Pi / DRAWS_PER_TENTACLE_SEGMENT) * segmentRadius;

			float effectiveIndexForFraming = index < 0 ? index * TENTACLE_HEAD_SEPARATION_SCALE_MULT : index;
			int segmentFramePoint = (int)((360 - drawWidthPerSegment) - (segmentWidth * (effectiveIndexForFraming + 64 - 1) % 64));

			float generalDepthFromAngle = (float)Math.Cos(baseAngle + TentacleBaseAngleOffset(index));
			float segmentDepthModifier = (generalDepthFromAngle + 0.5f) * (specialSegmentsHead * 65536f + index) + specialSegmentsHead * 512f;

			float scaleLength = (TentacleSegmentPosition(index, baseAngle, baseRotation, baseRadius, originalBasePosition) - TentacleSegmentPosition(index - 1, baseAngle, baseRotation, baseRadius, originalBasePosition)).Length() / TENTACLE_SEGMENT_SEPARATION;

			int inc = inPhase2 ? 1 : 0;

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
						Rectangle frame = new Rectangle(segmentFramePoint, j * BASE_TENTACLE_TEXTURE_HEIGHT / DRAWS_PER_TENTACLE_SIDE + (BASE_TEXTURE_HEIGHT + 2) * 2 * inc, TENTACLE_SEGMENT_SEPARATION * 2, BASE_TENTACLE_TEXTURE_HEIGHT / DRAWS_PER_TENTACLE_SIDE);
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

        public override void SetStaticDefaults()
        {
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 10;
		}

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

			Projectile.GetGlobalProjectile<PolaritiesProjectile>().canLeaveWorld = true;
		}

        public override void OnSpawn(IEntitySource source)
		{
			switch ((int)Projectile.ai[0])
			{
				case 0:
				case 3:
				case 4:
					Projectile.timeLeft = 450;
					break;
				case 1:
				case 2:
				case 5:
					Projectile.timeLeft = 600;
					break;
			}
			Projectile.localAI[0] = Projectile.timeLeft;
		}

        public override void AI()
		{
			float acceleration = 1;
			float velRotation = 0f;
			float gravity = 0f;
			switch ((int)Projectile.ai[0])
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
				case 3:
					acceleration = 1.01f;
					velRotation = (Projectile.ai[0] % 1 == 0) ? 0.02f : -0.02f;
					break;
				case 4:
					acceleration = 1.01f;
					velRotation = (Projectile.ai[0] % 1 - 0.5f) * MathHelper.TwoPi;
					break;
				case 5:
					gravity = 0.3f;
					break;
			}

			Projectile.velocity = Projectile.velocity.RotatedBy(velRotation) * acceleration + new Vector2(0, gravity);

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

		public override bool PreDraw(ref Color lightColor)
		{
			float scaleFactor = Projectile.velocity.Length() / 16f;

			float progress = (float)Math.Pow(Projectile.timeLeft / Projectile.localAI[0], 0.25f);
			float colorPulse = (3 + (float)Math.Sin(Projectile.timeLeft * MathHelper.TwoPi / 60f)) / 3;
			Color drawColor = ModUtils.ConvectiveFlameColor((float)Math.Pow(0.5f * progress * colorPulse + Projectile.ai[1], 2)) * progress;
			if (Projectile.timeLeft < 30) drawColor *= Projectile.timeLeft / 30f;

			for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
			{
				if (Projectile.oldPos[i + 1] != Vector2.Zero)
				{
					float trailProgress = 1 - i / (float)Projectile.oldPos.Length;
					Vector2 scale = new Vector2((Projectile.oldPos[i] - Projectile.oldPos[i + 1]).Length() * 0.12f, trailProgress * 0.6f) * Projectile.scale;
					float trailAlpha = Math.Min(1, (Projectile.oldPos[i] - Projectile.oldPos[i + 1]).Length() / 4f);
					Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.oldPos[i] + Projectile.Center - Projectile.position - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), drawColor * trailAlpha, Projectile.oldRot[i], TextureAssets.Projectile[Type].Size() / 2, scale, SpriteEffects.None, 0);
				}
			}

			Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), drawColor, Projectile.rotation, TextureAssets.Projectile[Type].Size() / 2, new Vector2(1 + scaleFactor, 1) * Projectile.scale, SpriteEffects.None, 0);
			return false;
		}
	}

	//TODO: This currently has a really small hitbox, may want to make it a big bigger
	public class ConvectiveWandererHeatVortex : ModProjectile
	{
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
					String filePath = Main.SavePath + System.IO.Path.DirectorySeparatorChar + "ConvectiveWandererHeatVortex.png";

					if (!System.IO.File.Exists(filePath))
					{
						const int textureSize = 256;

						Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, textureSize, textureSize, false, SurfaceFormat.Color);
						System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
						for (int i = 0; i < texture.Width; i++)
						{
							for (int j = 0; j < texture.Height; j++)
							{
								float x = (2 * i / (float)(texture.Width - 1) - 1);
								float y = (2 * j / (float)(texture.Height - 1) - 1);

								float distance = (float)Math.Sqrt(x * x + y * y);
								float angle = (float)Math.Atan2(y, x);

								float alphaMultiplier = (float)Math.Exp(1 + 1 / (distance * distance - 1));

								int r = 255;
								int g = 255;
								int b = 255;

								float phase = angle * 2 - distance * MathHelper.Pi;
								float val = (float)Math.Sin(phase) + (float)Math.Sin(phase * 2) / 2 + (float)Math.Sin(phase * 3) / 3 + (float)Math.Sin(phase * 4) / 4;

								float baseAlpha = (val * distance + 1) / (distance + 1);

								float alpha = distance >= 1 ? 0 :
									baseAlpha * alphaMultiplier;

								list.Add(new Color((int)(r * alpha), (int)(g * alpha), (int)(b * alpha), (int)(255 * alpha)));
							}
						}
						texture.SetData(list.ToArray());
						texture.SaveAsPng(new System.IO.FileStream(filePath, System.IO.FileMode.Create), texture.Width, texture.Height);
					}
				}
			});
        }*/

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.scale = 0.1f;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.alpha = 0;
			Projectile.timeLeft = 720;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;

			Projectile.hide = false;
		}

        public override void AI()
		{
			Projectile.spriteDirection = Projectile.localAI[0] == 3f ? 1 : -1;

			if (Projectile.timeLeft > 120)
			{
				NPC owner = Main.npc[(int)Projectile.ai[0]];
				Projectile.velocity = new Vector2(owner.ai[2], owner.ai[3]) - Projectile.Center;

				Vector2 oldCenter = Projectile.Center;

				Projectile.scale = 1.5f * (1 - (Projectile.timeLeft - 120) / 600f);

				Projectile.width = (int)(Projectile.scale * 128);
				Projectile.height = (int)(Projectile.scale * 128);
				Projectile.Center = oldCenter;

				if (Projectile.timeLeft % 60 == 0 && Projectile.timeLeft <= 600)
				{
					int numProjectiles = Projectile.ai[1] == 0 ? 4 : 8;
					float randRotation = Main.rand.NextFloat(MathHelper.TwoPi);
					for (int i = 0; i < numProjectiles; i++)
					{
						Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(4, 0).RotatedBy(i * MathHelper.TwoPi / numProjectiles + randRotation), ProjectileType<ConvectiveWandererAcceleratingShot>(), 12, 2f, Main.myPlayer, ai0: Projectile.localAI[0], ai1: Projectile.ai[1]);
					}
				}

				Projectile.rotation += 0.1f * Projectile.spriteDirection;
			}
			else
            {
				Projectile.velocity = (Main.LocalPlayer.Center - Projectile.Center) / 120f * (Projectile.timeLeft / 120f);

				for (int i = 0; i < (Projectile.ai[1] == 0f ? 1 : 2); i++)
					if (Main.rand.Next(120) >= Projectile.timeLeft)
					{
						MakeParticle();
					}

				Projectile.rotation += 0.1f * 120f / (Projectile.timeLeft) * Projectile.spriteDirection;
			}
		}

		void MakeParticle(float minSpeed = 12f, float maxSpeed = 20f)
		{
			float colorPulse = (3 + (float)Math.Sin(Projectile.timeLeft * MathHelper.TwoPi / 30f)) / 4;
			Color drawColor = ModUtils.ConvectiveFlameColor((float)Math.Pow(colorPulse * 0.25f + 0.25f + Projectile.ai[1], 2));

			ConvectiveWandererVortexParticle particle = Particle.NewParticle<ConvectiveWandererVortexParticle>(Projectile.Center, new Vector2(Main.rand.NextFloat(minSpeed, maxSpeed), 0).RotatedByRandom(MathHelper.TwoPi), 0f, 0f, Scale: Main.rand.NextFloat(0.1f, 0.2f), Color: drawColor);
			ParticleLayer.AfterLiquidsAdditive.Add(particle);
		}

		public override void Kill(int timeLeft)
		{
			int numProjectiles = Projectile.ai[1] == 0 ? 16 : 32;
			float randRotation = Main.rand.NextFloat(MathHelper.TwoPi);

			for (int i = 0; i < numProjectiles; i++)
			{
				for (int j = 0; j < 5; j++)
				{
					float speedMult = (5 - j) / 5f;
					float rotationAmount = (1 - speedMult) * 0.004f;
					Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(8 * speedMult, 0).RotatedBy(j * MathHelper.Pi + i * MathHelper.TwoPi / numProjectiles + randRotation), ProjectileType<ConvectiveWandererAcceleratingShot>(), 12, 2f, Main.myPlayer, ai0: 4.5f + 0.5f * rotationAmount * Projectile.spriteDirection, ai1: Projectile.ai[1]);
				}
			}

			for (int i = 0; i < (Projectile.ai[1] == 0f ? 1 : 2) * 100; i++)
			{
				MakeParticle(maxSpeed: 40);
			}
		}

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return CustomCollision.CheckAABBvDisc(targetHitbox, new Circle(Projectile.Center, Projectile.width / 2));
		}

		public override bool? CanDamage()
		{
			return Projectile.timeLeft > 660 ? false : null;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffType<Incinerating>(), 60, true);
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			DrawLayer.AddProjectile<DrawLayerAdditiveAfterLiquids>(index);
		}

		//TODO: Should probably have inward pulses of some sort
		//TODO: Those should switch to rapidly-speeding-up outward pulses when about to explode (also should emit sparks then)
		//TODO: Explosion should look better
		public override bool PreDraw(ref Color lightColor)
		{
			float colorPulse = (3 + (float)Math.Sin(Projectile.timeLeft * MathHelper.TwoPi / 30f)) / 4;
			Color drawColor = ModUtils.ConvectiveFlameColor((float)Math.Pow(colorPulse * 0.25f + 0.25f + Projectile.ai[1], 2));

			Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), drawColor * 0.5f, Projectile.rotation, TextureAssets.Projectile[Type].Size() / 2, Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), drawColor * 0.5f, Projectile.rotation + MathHelper.PiOver2, TextureAssets.Projectile[Type].Size() / 2, Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), drawColor * 0.5f, Projectile.rotation * 2, TextureAssets.Projectile[Type].Size() / 2, Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), drawColor * 0.5f, Projectile.rotation * 2 + MathHelper.PiOver2, TextureAssets.Projectile[Type].Size() / 2, Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			Main.EntitySpriteDraw(Textures.Glow256.Value, Projectile.Center - Main.screenPosition, Textures.Glow256.Frame(), drawColor * 0.5f, 0f, Textures.Glow256.Size() / 2, Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			return false;
		}
	}

	public class ConvectiveWandererVortexParticle : Particle
	{
		public override string Texture => "Polarities/Textures/Glow58";

		public override void Initialize()
		{
			Color = Color.White;
			Glow = true;
			TimeLeft = 60;
		}

		public int owner = -1;
		public int projectileOwner = -1;
		public float angling;

		public override void AI()
		{
			if (owner >= 0 && Main.npc[owner].active && Main.npc[owner].ai[0] == 4 && Main.npc[owner].ai[1] >= 150 && Main.npc[owner].ai[1] < 780)
			{
				Vector2 goalPos = new Vector2(Main.npc[owner].ai[2], Main.npc[owner].ai[3]);

				float goalRadius = Math.Max(0, 64 * (Main.npc[owner].ai[1] - 180f) / 600f);

				TimeLeft = 60;

				if ((goalPos - Position).Length() - goalRadius < Velocity.Length())
                {
					Kill();
					return;
                }

				Velocity = (goalPos - Position).SafeNormalize(Vector2.Zero).RotatedBy(angling) * Velocity.Length();

				Alpha = Math.Min(1, ((goalPos - Position).Length() - goalRadius) / 60f);
			}
			else
            {
				Velocity *= 0.95f;
				owner = -1;

				Scale = InitialScale * (float)(1 - Math.Pow(1 - Math.Min(1, TimeLeft / 60f), 2));
				Alpha = Math.Max(1, TimeLeft / 60f);
			}

			Rotation = Velocity.ToRotation();
		}

        public override void Draw(SpriteBatch spritebatch)
		{
			Asset<Texture2D> particleTexture = particleTextures[Type];

			Vector2 drawPosition = Position - Main.screenPosition;
			if (projectileOwner > -1) drawPosition += Main.projectile[projectileOwner].Center;
			Color drawColor = Glow ? Color * Alpha : Lighting.GetColor(drawPosition.ToTileCoordinates()).MultiplyRGBA(Color * Alpha);

			spritebatch.Draw(particleTexture.Value, drawPosition, particleTexture.Frame(), drawColor, Rotation, particleTexture.Size() / 2, Scale * new Vector2(Velocity.Length() / 2f * Alpha, 1), SpriteEffects.None, 0f);
		}
    }

	//TODO: Remove this if it ends up unused
	public class ConvectiveWandererLateralDeathray : ModProjectile
    {
		public override string Texture => "Polarities/Textures/Glow58";

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.timeLeft = 180;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Projectile.rotation = Projectile.velocity.ToRotation();
		}

		public override void AI()
		{
			float progress = (float)Math.Pow(Projectile.timeLeft / 180f, 0.25f);

			Projectile.scale = 0.5f * (float)Math.Sqrt(progress) * 30f;
			if (Projectile.timeLeft < 30) Projectile.scale *= Projectile.timeLeft / 30f;
			if (Projectile.timeLeft > 170) Projectile.scale *= (180f - Projectile.timeLeft) / 10f;
		}

		public override bool? CanDamage()
		{
			return (Projectile.timeLeft < 30 || Projectile.timeLeft > 170) ? false : null;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffType<Incinerating>(), 60, true);
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			DrawLayer.AddProjectile<DrawLayerAdditiveAfterLiquids>(index);
		}

		public override bool ShouldUpdatePosition() => false;

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float length = 4000;

			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center - new Vector2(length / 2, 0).RotatedBy(Projectile.rotation), Projectile.Center + new Vector2(length / 2, 0).RotatedBy(Projectile.rotation));
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			float length = 4000;

			Rectangle lineFrame = new Rectangle(29, 0, 1, 58);
			Vector2 lineCenter = lineFrame.Size() / 2;

			float progress = (float)Math.Pow(Projectile.timeLeft / 180f, 0.25f);
			float colorPulse = (3 + (float)Math.Sin(Projectile.timeLeft * MathHelper.TwoPi / 60f)) / 3;
			Color drawColor = ModUtils.ConvectiveFlameColor((float)Math.Pow(0.5f * progress * colorPulse + 0.5f, 2)) * progress;

			Vector2 drawPos = Projectile.Center - Main.screenPosition;

			for (int i = 1; i <= 4; i++)
			{
				float drawScale = Projectile.scale * i / 4f;

				Main.EntitySpriteDraw(texture, drawPos, lineFrame, drawColor, Projectile.rotation, lineCenter, new Vector2(length, drawScale / lineFrame.Height), SpriteEffects.None, 0);
			}

			return false;
		}
	}

	public class ConvectiveWandererTentacleDeathray : ModProjectile
	{
		public override string Texture => "Polarities/Textures/Glow58";

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.timeLeft = 120;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Projectile.rotation = Projectile.velocity.ToRotation();
		}

		public override void AI()
		{
			float progress = (float)Math.Pow(Projectile.timeLeft / 180f, 0.25f);

			Projectile.scale = 0.5f * (float)Math.Sqrt(progress) * 60f;
			if (Projectile.timeLeft < 10) Projectile.scale *= Projectile.timeLeft / 10f;
			if (Projectile.timeLeft > 110) Projectile.scale *= (120f - Projectile.timeLeft) / 10f;


			int i = (int)Projectile.ai[0];
			NPC owner = Main.npc[(int)Projectile.ai[1]];
			ConvectiveWanderer wanderer = owner.ModNPC as ConvectiveWanderer;

			float tentacleBaseAngle = wanderer.SegmentAngle(ConvectiveWanderer.TENTACLE_ATTACH_SEGMENT_INDEX);
			float tentacleBaseRotation = wanderer.SegmentRotation(ConvectiveWanderer.TENTACLE_ATTACH_SEGMENT_INDEX) + MathHelper.PiOver2;
			float tentacleBaseRadius = wanderer.SegmentRadius(ConvectiveWanderer.TENTACLE_ATTACH_SEGMENT_INDEX) + wanderer.TentacleRadius(0);
			Vector2 tentacleBasePosition = wanderer.SegmentPosition(ConvectiveWanderer.TENTACLE_ATTACH_SEGMENT_INDEX);

			Vector2 spot = wanderer.TentacleSegmentPosition(32, tentacleBaseAngle + i * MathHelper.TwoPi / ConvectiveWanderer.NUM_TENTACLES, tentacleBaseRotation, tentacleBaseRadius, tentacleBasePosition);
			float angle = MathHelper.Pi + wanderer.TentacleSegmentRotation(32, tentacleBaseAngle + i * MathHelper.TwoPi / ConvectiveWanderer.NUM_TENTACLES, tentacleBaseRotation, tentacleBaseRadius, tentacleBasePosition);

			Projectile.Center = spot + new Vector2(Projectile.scale / 2f, 0).RotatedBy(angle);
			Projectile.rotation = angle;
		}

		public override bool? CanDamage()
		{
			return (Projectile.timeLeft < 10 || Projectile.timeLeft > 110) ? false : null;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffType<Incinerating>(), 60, true);
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			DrawLayer.AddProjectile<DrawLayerAdditiveAfterLiquids>(index);
		}

		public override bool ShouldUpdatePosition() => false;

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float length = 2000;

			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + new Vector2(length / 2, 0).RotatedBy(Projectile.rotation));
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			float length = 2000;

			Rectangle lineFrame = new Rectangle(29, 0, 1, 58);
			Vector2 lineCenter = new Vector2(0, lineFrame.Height / 2);

			Rectangle capFrame = new Rectangle(0, 0, 29, 58);
			Vector2 capCenter = new Vector2(capFrame.Width, capFrame.Height / 2);

			float progress = (float)Math.Pow(Projectile.timeLeft / 120f, 0.25f);
			float colorPulse = (3 + (float)Math.Sin(Projectile.timeLeft * MathHelper.TwoPi / 60f)) / 3;
			Color drawColor = ModUtils.ConvectiveFlameColor((float)Math.Pow(0.5f * progress * colorPulse + 0.5f, 2)) * progress;

			Vector2 drawPos = Projectile.Center - Main.screenPosition;

			for (int i = 1; i <= 4; i++)
			{
				float drawScale = Projectile.scale * i / 4f;

				Main.EntitySpriteDraw(texture, drawPos, lineFrame, drawColor * 0.5f, Projectile.rotation, lineCenter, new Vector2(length, drawScale / lineFrame.Height), SpriteEffects.None, 0);
				Main.EntitySpriteDraw(texture, drawPos, capFrame, drawColor * 0.5f, Projectile.rotation, capCenter, drawScale / lineFrame.Height, SpriteEffects.None, 0);
			}

			return false;
		}
	}

	public class ConvectiveWandererFlamePillar : ModProjectile
    {
		public override string Texture => "Polarities/Textures/Pixel";

		public override void SetDefaults()
		{
			Projectile.width = 60;
			Projectile.height = 60;
			Projectile.aiStyle = -1;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 120;
			Projectile.tileCollide = false;
			Projectile.hide = false;
		}

        public override void OnSpawn(IEntitySource source)
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Projectile.localAI[0] = Main.rand.Next(4095);
		}

		public override bool ShouldUpdatePosition() => false;

        public override void AI()
		{
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float progress = 1 - Projectile.timeLeft / 120f;
			float width = 16f * (Math.Min(0.5f, Math.Min(progress * 4f, (1 - progress) * 4f)) * 1.5f);
			return CustomCollision.CheckAABBvTriangle(targetHitbox, Projectile.Center + new Vector2(0, -128f).RotatedBy(Projectile.rotation) * (Math.Min(Math.Max(progress - 0.5f, 0) * 4, 1) * 32 + 1) * (Math.Min(progress * 4, 1) + 1f) * 0.15f, Projectile.Center + new Vector2(width, 0), Projectile.Center - new Vector2(width, 0));
		}

		public override bool? CanDamage()
		{
			float progress = 1 - Projectile.timeLeft / 120f;
			return (progress < 0.5f || progress > 0.9f) ? false : null;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float progress = 1 - Projectile.timeLeft / 120f;
			Vector2 flameScale = new Vector2(Math.Min(0.5f, Math.Min(progress * 4f, (1 - progress) * 4f)) * 1.5f, (Math.Min(Math.Max(progress - 0.5f, 0) * 4, 1) * 32 + 1)) * (Math.Min(progress * 4, 1) + 1f) * 0.15f;
			Vector2 flamePos = Projectile.Center - Main.screenPosition + new Vector2(0, -64 * flameScale.X).RotatedBy(Projectile.rotation);

            AsthenosProjectile.asthenosRandomValues.SetIndex((int)Projectile.localAI[0]);
			AsthenosProjectile.DrawFlame(Main.spriteBatch, flamePos, Projectile.rotation, flameScale, 0.5f + Projectile.ai[1], PolaritiesSystem.timer * 3, 2, alpha: 1f, goalAngle: Projectile.rotation);

			return false;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			DrawLayer.AddProjectile<DrawLayerAdditiveAfterLiquids>(index);
		}

        public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffType<Incinerating>(), 60, true);
		}
	}

	public class ConvectiveWandererFlamethrower : ModProjectile
	{
		public override string Texture => "Polarities/Textures/Pixel";

		public override void SetDefaults()
		{
			Projectile.width = 60;
			Projectile.height = 60;
			Projectile.aiStyle = -1;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 300;
			Projectile.tileCollide = false;
			Projectile.hide = false;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Projectile.localAI[0] = Main.rand.Next(4095);
		}

		public override bool ShouldUpdatePosition() => false;

		public override void AI()
		{
			NPC owner = Main.npc[(int)Projectile.ai[0]];
			Projectile.velocity = new Vector2(1, 0).RotatedBy(owner.rotation);
			Projectile.Center = owner.Center + 112 * Projectile.velocity;

			Projectile.rotation = owner.rotation + MathHelper.PiOver2;

			float progress = 1 - Projectile.timeLeft / 300f;
			float widthMult = Math.Min(0.5f, Math.Min(progress * 4f, (1 - progress) * 4f)) * (1 + Projectile.ai[1]);

			float colorPulse = (3 + (float)Math.Sin(Projectile.timeLeft * MathHelper.TwoPi / 30f)) / 4;
			Color drawColor = ModUtils.ConvectiveFlameColor((float)Math.Pow(colorPulse * 0.25f + 0.25f + Projectile.ai[1], 2));

			float length = 128f * (Math.Min(Math.Max(progress - 0.01f, 0) * 4, 1) * 96 + 1) * (Math.Min(progress * 4, 1) + 1f) * 0.15f;
			for (int i = 0; i < 4 * (1 + Projectile.ai[1]) * widthMult; i++)
			{
				ConvectiveWandererVortexParticle particle = Particle.NewParticle<ConvectiveWandererVortexParticle>(Projectile.Center + new Vector2(Main.rand.NextFloat(-1f, 1f) * 16f * widthMult, -(float)Math.Pow(Main.rand.NextFloat(), 2) * length).RotatedBy(Projectile.rotation), new Vector2(0, -Main.rand.NextFloat(4f, 32f)).RotatedBy(Projectile.rotation), 0f, 0f, Scale: Main.rand.NextFloat(0.1f, 0.2f), Color: drawColor);
				ParticleLayer.AfterLiquidsAdditive.Add(particle);
			}

			if (Projectile.timeLeft == 300)
			{
				SoundEngine.PlaySound(Sounds.ConvectiveWandererFlamethrowerStart, Projectile.Center);
			}
			if (Projectile.soundDelay == 0 && Projectile.timeLeft >= 134)
            {
				//TODO: Make this an actual looped sound and give it volume control
				SoundEngine.PlaySound(Sounds.ConvectiveWandererFlamethrowerLoop, Projectile.Center);

				Projectile.soundDelay = 134;
			}

			if (Projectile.timeLeft >= 10)
			{
				Main.LocalPlayer.GetModPlayer<PolaritiesPlayer>().AddScreenShake(8 * widthMult, 10);
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float progress = 1 - Projectile.timeLeft / 300f;
			float width = 16f * (Math.Min(0.5f, Math.Min(progress * 4f, (1 - progress) * 4f)) * 3f * (1 + Projectile.ai[1]));
			return CustomCollision.CheckAABBvTriangle(targetHitbox, Projectile.Center + new Vector2(0, -128f).RotatedBy(Projectile.rotation) * (Math.Min(Math.Max(progress - 0.01f, 0) * 4, 1) * 96 + 1) * (Math.Min(progress * 4, 1) + 1f) * 0.15f, Projectile.Center + new Vector2(width, 0).RotatedBy(Projectile.rotation), Projectile.Center - new Vector2(width, 0).RotatedBy(Projectile.rotation));
		}

		public override bool? CanDamage()
		{
			float progress = 1 - Projectile.timeLeft / 300f;
			return (progress < 0.01f || progress > 0.99f) ? false : null;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float progress = 1 - Projectile.timeLeft / 300f;
			Vector2 flameScale = new Vector2(Math.Min(0.5f, Math.Min(progress * 4f, (1 - progress) * 4f)) * 3f * (1 + Projectile.ai[1]), (Math.Min(Math.Max(progress - 0.01f, 0) * 4, 1) * 96 + 1)) * (Math.Min(progress * 4, 1) + 1f) * 0.15f;
			Vector2 flamePos = Projectile.Center - Main.screenPosition + new Vector2(0, -64 * flameScale.X).RotatedBy(Projectile.rotation);

			AsthenosProjectile.asthenosRandomValues.SetIndex((int)Projectile.localAI[0]);
			AsthenosProjectile.DrawFlame(Main.spriteBatch, flamePos, Projectile.rotation, flameScale, 0.5f + Projectile.ai[1], PolaritiesSystem.timer * 3, 2, alpha: 1f, goalAngle: Projectile.rotation);

			return false;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			DrawLayer.AddProjectile<DrawLayerAdditiveAfterLiquids>(index);
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffType<Incinerating>(), 60, true);
		}
	}

	//TODO: I think this shares the hitbox issue
	public class ConvectiveWandererChargedShot : ModProjectile
	{
		public override string Texture => "Polarities/Items/Weapons/Ranged/ContagunProjectile";

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.scale = 0.1f;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.alpha = 0;
			Projectile.timeLeft = 720;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;

			Projectile.hide = false;
		}

		public override void AI()
		{
			Projectile.rotation += 0.05f;

			if (Projectile.timeLeft >= 600)
			{
				float scaleFactor = (720 - Projectile.timeLeft) / 120f;
				Projectile.scale = 2f * scaleFactor * (2 - scaleFactor);

				Projectile.width = (int)(Projectile.scale * 128);
				Projectile.height = (int)(Projectile.scale * 128);

				NPC owner = Main.npc[(int)Projectile.ai[0]];
				Projectile.velocity = new Vector2(1, 0).RotatedBy(owner.rotation);
				Projectile.Center = owner.Center + 240 * Projectile.velocity;

				if (Projectile.timeLeft == 600)
				{
					Projectile.velocity = new Vector2(4, 0).RotatedBy(owner.rotation) + owner.velocity;
				}

				//TODO: Ensure other attached projectiles die if the owner dies
				if (!owner.active) Projectile.Kill();

				if (Projectile.timeLeft >= 630)
				{
					float colorPulse = (3 + (float)Math.Sin(Projectile.timeLeft * MathHelper.TwoPi / 30f)) / 4;
					Color drawColor = ModUtils.ConvectiveFlameColor((float)Math.Pow(colorPulse * 0.25f + 0.25f + Projectile.ai[1], 2));

					Vector2 particlePos = new Vector2(240, 0).RotatedByRandom(MathHelper.TwoPi);
					ConvectiveWandererVortexParticle particle = Particle.NewParticle<ConvectiveWandererVortexParticle>(particlePos, -particlePos / 20f, 0f, 0f, Scale: Main.rand.NextFloat(0.1f, 0.2f), Color: drawColor);
					particle.projectileOwner = Projectile.whoAmI;
					ParticleLayer.AfterLiquidsAdditive.Add(particle);
				}
			}
			else
            {
				Projectile.velocity *= 1.01f;

				//explode if too far
				if (Projectile.Distance(Main.LocalPlayer.Center) > 1200)
                {
					int numProjectiles = 16;
					float rotationOffset = Main.rand.NextFloat(MathHelper.TwoPi);

					for (int i = 0; i < numProjectiles; i++)
                    {
						Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, new Vector2(8, 0).RotatedBy(i * MathHelper.TwoPi / numProjectiles + rotationOffset), ProjectileType<ConvectiveWandererAcceleratingShot>(), 12, 2f, Projectile.owner, ai0: 1, ai1: Projectile.ai[1]);
                    }
					if (Projectile.ai[1] != 0)
					{
						for (int i = 0; i < numProjectiles; i++)
						{
							Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, new Vector2(6, 0).RotatedBy(i * MathHelper.TwoPi / numProjectiles + MathHelper.Pi / numProjectiles + rotationOffset), ProjectileType<ConvectiveWandererAcceleratingShot>(), 12, 2f, Projectile.owner, ai0: 1, ai1: Projectile.ai[1]);
						}
					}

					Projectile.Kill();
                }
            }
		}

		void MakeParticle(float minSpeed = 12f, float maxSpeed = 20f)
		{
			float colorPulse = (3 + (float)Math.Sin(Projectile.timeLeft * MathHelper.TwoPi / 30f)) / 4;
			Color drawColor = ModUtils.ConvectiveFlameColor((float)Math.Pow(colorPulse * 0.25f + 0.25f + Projectile.ai[1], 2));

			ConvectiveWandererVortexParticle particle = Particle.NewParticle<ConvectiveWandererVortexParticle>(Projectile.Center, new Vector2(Main.rand.NextFloat(minSpeed, maxSpeed), 0).RotatedByRandom(MathHelper.TwoPi), 0f, 0f, Scale: Main.rand.NextFloat(0.1f, 0.2f), Color: drawColor);
			ParticleLayer.AfterLiquidsAdditive.Add(particle);
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < (Projectile.ai[1] == 0f ? 1 : 2) * 100; i++)
			{
				MakeParticle(maxSpeed: 40);
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return CustomCollision.CheckAABBvDisc(targetHitbox, new Circle(Projectile.Center, Projectile.width / 2));
		}

		public override bool? CanDamage()
		{
			return Projectile.timeLeft > 690 ? false : null;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffType<Incinerating>(), 60, true);
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			DrawLayer.AddProjectile<DrawLayerAdditiveAfterLiquids>(index);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float colorPulse = (3 + (float)Math.Sin(Projectile.timeLeft * MathHelper.TwoPi / 30f)) / 4;
			Color drawColor = ModUtils.ConvectiveFlameColor((float)Math.Pow(colorPulse * 0.25f + 0.25f + Projectile.ai[1], 2));

			//TODO: Trail when moving

			Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), drawColor * 0.25f, Projectile.rotation, TextureAssets.Projectile[Type].Size() / 2, Projectile.scale / 2, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), drawColor * 0.25f, -Projectile.rotation + MathHelper.PiOver2, TextureAssets.Projectile[Type].Size() / 2, Projectile.scale / 2, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), drawColor * 0.25f, Projectile.rotation * 2, TextureAssets.Projectile[Type].Size() / 2, Projectile.scale / 2, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), drawColor * 0.25f, -Projectile.rotation * 2 + MathHelper.PiOver2, TextureAssets.Projectile[Type].Size() / 2, Projectile.scale / 2, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			Main.EntitySpriteDraw(Textures.Glow256.Value, Projectile.Center - Main.screenPosition, Textures.Glow256.Frame(), drawColor, 0f, Textures.Glow256.Size() / 2, Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			return false;
		}
	}
}