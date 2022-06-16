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

namespace Polarities.NPCs.StarConstruct
{
	[AutoloadBossHead]
	public class StarConstruct : ModNPC
	{
		//textures for cacheing
		public static Asset<Texture2D> LegTexture;
		public static Asset<Texture2D> ChainTexture;
		public static Asset<Texture2D> ClawTexture;
		public static Asset<Texture2D> DashTexture;

		public override void Load()
		{
			LegTexture = Mod.GetAsset<Texture2D>("NPCs/StarConstruct/StarConstructLeg");
			ChainTexture = Mod.GetAsset<Texture2D>("NPCs/StarConstruct/StarConstructChain");
			ClawTexture = Mod.GetAsset<Texture2D>("NPCs/StarConstruct/StarConstructClaw");
			DashTexture = Mod.GetAsset<Texture2D>("NPCs/StarConstruct/StarConstructDash");

			//TODO: Use the old scream style on like april fools or something
			//Scream = new SoundStyle("Terraria/Sounds/Roar_2") { Volume = 1f, Pitch = 0.7f };

			/*IL.Terraria.Main.UpdateMenu += Main_UpdateMenu;
		}

		private void Main_UpdateMenu(MonoMod.Cil.ILContext il)
		{
			MonoMod.Cil.ILCursor c = new MonoMod.Cil.ILCursor(il);

			c.EmitDelegate<Action>(() =>
			{
				if (!(bool)(typeof(ModLoader).GetField("isLoading", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null)))
				{
					String filePath = Main.SavePath + Path.DirectorySeparatorChar + "StarConstruct.png";

					if (!File.Exists(filePath))
					{
						Main.spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);

						var capture = new RenderTarget2D(Main.spriteBatch.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, Main.spriteBatch.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

						Main.spriteBatch.GraphicsDevice.SetRenderTarget(capture);
						Main.spriteBatch.GraphicsDevice.Clear(Color.Transparent);

						NPC me = new NPC();
						me.SetDefaults(NPCType<StarConstruct>());
						me.IsABestiaryIconDummy = true;
						me.Center = Vector2.Zero;

						Main.instance.DrawNPCDirect(Main.spriteBatch, me, false, -capture.Size() / 2);

						Main.spriteBatch.End();
						Main.spriteBatch.GraphicsDevice.SetRenderTarget(null);

						var stream = File.Create(filePath);
						capture.SaveAsPng(stream, capture.Width, capture.Height);
						stream.Dispose();
						capture.Dispose();
					}
				}
			});*/
		}

		public override void Unload()
        {
			LegTexture = null;
			ChainTexture = null;
			ClawTexture = null;
			DashTexture = null;
		}

        public override void SetStaticDefaults()
		{
			//group with other bosses
			NPCID.Sets.BossBestiaryPriority.Add(Type);

			NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
			{
				SpecificallyImmuneTo = new int[] {
					BuffID.Poisoned,
					BuffID.OnFire,
					BuffID.Confused
				}
			};
			NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);

			var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
			{
				PortraitPositionYOverride = -0f,
				Position = new Vector2(0f, 4f)
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

			Main.npcFrameCount[NPC.type] = 2;

			NPCID.Sets.TrailCacheLength[NPC.type] = 10;
			NPCID.Sets.TrailingMode[NPC.type] = 3;
		}

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                //spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				//flavor text
				this.TranslatedBestiaryEntry()
			});
		}

        private int[] arm;
		static int baseArmCount => Main.getGoodWorld ? 6 : 4;
		public static float VelocityMultiplier => Main.expertMode ? 10 : 6;

		public override void SetDefaults()
		{
			NPC.aiStyle = -1;
			NPC.width = 96;
			NPC.height = 96;
			NPC.defense = 16;
			NPC.damage = 32;
			NPC.lifeMax = Main.masterMode ? 5100 / 3 : Main.expertMode ? 4200 / 2 : 3200;
			NPC.knockBackResist = 0f;
			NPC.value = Item.buyPrice(0, 2, 0, 0);
			NPC.npcSlots = 15f;
			NPC.boss = true;
			NPC.lavaImmune = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath14;

			Music = MusicID.Boss1;
			if (ModLoader.HasMod("PolaritiesMusic"))
			{
				Music = MusicLoader.GetMusicSlot(ModLoader.GetMod("PolaritiesMusic"), "Sounds/Music/StarConstruct");
			}

			//for drawBehind
			NPC.hide = true;

			arm = new int[16];
		}

		//public static SoundStyle Scream;

		public override void AI()
		{
			Lighting.AddLight(NPC.Center, 2f, 2f, 2f);

			Player player = Main.player[NPC.target];
			if (!player.active || player.dead || Main.dayTime)
			{
				NPC.TargetClosest(false);
				player = Main.player[NPC.target];
				if (player.dead || Main.dayTime)
				{
					if (NPC.timeLeft > 10)
					{
						NPC.timeLeft = 10;
					}
					NPC.ai[1] = 1;
					NPC.noTileCollide = true;
					NPC.velocity.Y += 0.1f;
					return;
				}
			}

			NPC.noTileCollide = false;
			NPC.noGravity = false;
			NPC.dontTakeDamage = false;

			//if initializing spawn hooks
			if (NPC.localAI[0] == 0)
			{
				//scream
				SoundEngine.PlaySound(Sounds.StarConstructRoar, NPC.Center);
				Main.NewText("Star Construct has awoken!", 171, 64, 255);

				NPC.localAI[0] = 1;

				for (int i = 0; i < baseArmCount; i++)
				{
					arm[i] = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<StarConstructClaw>(), ai0: NPC.whoAmI, ai1: i);
				}
				NPC.netUpdate = true;

				InitializeAIStates();
			}

			bool gotoNextAttack = false;
			switch (NPC.ai[0])
			{
				case 0:
					//default running behavior with forced jumps for the player to go under
					RunAbout(VelocityMultiplier, jumpSpeed: Main.getGoodWorld ? 12 : 15, forceJump: true);

					NPC.ai[1]++;
					if (NPC.ai[1] >= 180)
					{
						gotoNextAttack = true;
					}
					break;
				case 1:
					//hook pincering
					if ((NPC.Center - player.Center).Length() > 500 || NPC.ai[1] < 30)
					{
						//attempt to chase if too far
						RunAbout(VelocityMultiplier, jumpSpeed: 15);
					}
					else
					{
						//stop if in acceptable range
						RunAbout(0, canRise: false);
					}

					if (NPC.ai[1] == 30)
					{
						NPC.ai[3] = Main.rand.NextFloat(MathHelper.TwoPi);

						SoundEngine.PlaySound(Sounds.StarConstructScream, NPC.Center);
					}
					else if (NPC.ai[1] % 120 == 30)
					{
						NPC.ai[3] += MathHelper.Pi + Main.rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2);
					}

					NPC.ai[1]++;
					if (NPC.ai[1] >= 450)
					{
						NPC.ai[3] = 0;
						gotoNextAttack = true;
					}
					break;
				case 2:
					//boss jumps up and towards the player, and moves close to the player, flying if it can't keep up otherwise, hooks swing outward and back inward, so the player is forced in and out

					if (NPC.ai[1] == 90)
					{
						NPC.ai[3] = player.Center.X > NPC.Center.X ? 1 : -1;

						SoundEngine.PlaySound(Sounds.StarConstructScream, NPC.Center);
					}

					if (NPC.ai[1] >= 450)
					{
						//stop
						RunAbout(0, canRise: false);

						NPC.ai[3] = 0;
						NPC.frame.Y = 0;
						NPC.rotation = 0;
					}
					else if (((NPC.Center - player.Center).Length() > 660 || NPC.frame.Y > 0) && NPC.ai[1] >= 90)
					{
						//chase if outside the acceptable range
						if ((NPC.Center - player.Center).Length() > 500)
						{
							Vector2 goalPosition = player.Center + (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * 500;
							Vector2 goalVelocity = (goalPosition - NPC.Center) / 15f;
							NPC.velocity += (goalVelocity - NPC.velocity) / 15f;
						}
						else
						{
							NPC.velocity *= 0.95f;
						}

						//get stuck in star form
						NPC.frame.Y = NPC.frame.Height;
						NPC.noGravity = true;
						NPC.noTileCollide = true;

						NPC.rotation += 0.25f * NPC.ai[3];
					}
					else if ((NPC.Center - player.Center).Length() > 500 || NPC.ai[1] < 90)
					{
						//attempt to chase if near the edge of the area
						//we can jump if during the initial segment of the attack
						RunAbout(VelocityMultiplier, jumpSpeed: (NPC.ai[1] < 90) ? 15 : 0);
					}
					else
					{
						//stop if in acceptable range
						RunAbout(0, canRise: false);
					}

					NPC.ai[1]++;
					if (NPC.ai[1] >= 480)
					{
						gotoNextAttack = true;
					}
					break;
				case 3:
					//boss planteras after the player, slamming its claws down
					if (NPC.ai[1] < 60)
					{
						RunAbout(VelocityMultiplier, canRise: true);

						if (NPC.ai[1] == 30)
						{
							SoundEngine.PlaySound(Sounds.StarConstructScream, NPC.Center);
						}
					}
					else if (NPC.ai[1] < 540)
					{
						if (NPC.ai[1] == 60)
						{
							NPC.ai[3] = player.Center.X > NPC.Center.X ? 1 : -1;
						}

						FlyingLegAnim();
						Vector2 hookCenter = Vector2.Zero;
						int numActiveHooks = 0;
						for (int i = 0; i < NPC.localAI[0] * baseArmCount; i++)
						{
							if (Main.npc[arm[i]].localAI[1] == 0)
							{
								hookCenter += Main.npc[arm[i]].Center;
								numActiveHooks++;
							}
						}
						if (numActiveHooks > 0)
						{
							hookCenter /= numActiveHooks;

							Vector2 goalPosition = hookCenter + new Vector2(0, -360);
							Vector2 goalVelocity = (goalPosition - NPC.Center) / 15f;
							NPC.velocity += (goalVelocity - NPC.velocity) / 15f;

							NPC.noGravity = true;
							NPC.noTileCollide = true;
						}
						else
						{
							RunAbout(0, canRise: false);
						}
					}
					else
					{
						RunAbout(VelocityMultiplier, canRise: true);
					}

					NPC.ai[1]++;
					if (NPC.ai[1] >= 630)
					{
						gotoNextAttack = true;

						NPC.ai[3] = 0;
					}
					break;
				case 4:
					//phase 1 jump and dive: boss leaps into the air before yeeting itself at the player, shooting a projectile burst on tile impact
					if (NPC.localAI[0] == 1 || !Main.expertMode)
					{
						if (NPC.ai[1] < 120)
						{
							if (NPC.velocity.Y == 0 && NPC.ai[1] >= 60)
							{
								NPC.ai[1] = 120;

								NPC.velocity = new Vector2(player.Center.X > NPC.Center.X ? VelocityMultiplier : -VelocityMultiplier, -20);

								//dusts on jumping
								JumpDusts();
							}
							else
							{
								RunAbout(VelocityMultiplier, canRise: true);
							}
						}
						else if (NPC.ai[1] < 240)
						{
							FlyingLegAnim();
							if (NPC.velocity.Y >= -0.3f)
							{
								NPC.ai[1] = 240;

								NPC.velocity = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * VelocityMultiplier + (player.position - player.oldPosition);
								if (NPC.velocity.Y < VelocityMultiplier / 2)
								{
									NPC.velocity.Y = VelocityMultiplier / 2;
								}

								SoundEngine.PlaySound(Sounds.StarConstructScream, NPC.Center);

								NPC.frame.Y = NPC.frame.Height;

								NPC.noGravity = true;
							}
						}
						else if (NPC.ai[1] < 480)
						{
							NPC.noGravity = true;

							NPC.rotation += NPC.velocity.X > 0 ? 0.25f : -0.25f;

							FallingConstructDusts();
							//falling construct dusts

							if (NPC.velocity.Y == 0)
							{
								NPC.velocity.X = 0;
								NPC.ai[1] = 480;

								//starburst
								//shoot more projectiles in FTW
								for (int i = 0; i < baseArmCount * 3; i++)
								{
									Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, 32), new Vector2(8, 0).RotatedBy(-MathHelper.TwoPi * (2 * i + 1) / (baseArmCount * 6)), ProjectileType<UnfriendlyFallenStar>(), 16, 0, Main.myPlayer);
								}
								Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<StarConstructImpact>(), 16, 0, Main.myPlayer);
								SoundEngine.PlaySound(SoundID.NPCDeath14, NPC.Center);
							}
						}
						else
						{
							RunAbout(0, canRise: false);

							NPC.frame.Y = 0;
							NPC.rotation = 0;
						}

						NPC.ai[1]++;
						if (NPC.ai[1] >= 510)
						{
							gotoNextAttack = true;
						}
					}
					else
					{
						//phase 2 jump and teleport-dive: boss leaps into the air towards the player before freezing in place, creating a direction telegraph, and then deathray-teleporting into position and shooting a projectile burst + explosion

						if (NPC.ai[1] < 120)
						{
							if (NPC.velocity.Y == 0 && NPC.ai[1] >= 60)
							{
								NPC.ai[1] = 120;

								NPC.velocity = new Vector2(player.Center.X > NPC.Center.X ? VelocityMultiplier : -VelocityMultiplier, -20);

								//dusts on jumping
								JumpDusts();
							}
							else
							{
								RunAbout(VelocityMultiplier, canRise: true);
							}
						}
						else if (NPC.ai[1] < 240)
						{
							FlyingLegAnim();
							if (NPC.velocity.Y >= -0.3f)
							{
								NPC.ai[1] = 240;

								NPC.frame.Y = NPC.frame.Height;
								NPC.ai[3] = player.Center.X > NPC.Center.X ? 1 : -1;

								NPC.localAI[1] = (player.Center + new Vector2((player.position.X - player.oldPosition.X) * 45, 0) - NPC.Center).ToRotation();

								NPC.noGravity = true;
								NPC.noTileCollide = true;
							}
						}
						else if (NPC.ai[1] < 360)
						{
							NPC.velocity *= 0.95f;

							NPC.rotation += NPC.ai[3] * 20f / (390f - NPC.ai[1]);

							//Telegraph rotation
							if (NPC.ai[1] < 340)
							{
								NPC.localAI[1] = Utils.AngleLerp(NPC.localAI[1], (player.Center + new Vector2((player.position.X - player.oldPosition.X) * 45, 0) - NPC.Center).ToRotation(), 0.075f);
							}
							else if (NPC.ai[1] == 340)
							{
								NPC.velocity = new Vector2(-VelocityMultiplier * 0.5f, 0).RotatedBy(NPC.localAI[1]);

								//sound as telegraph
								SoundEngine.PlaySound(Sounds.StarConstructScream, NPC.Center);
							}

							NPC.noGravity = true;
							NPC.noTileCollide = true;
						}
						else
						{
							if (NPC.ai[1] == 360)
							{
								Vector2 startPosition = NPC.Center;

								NPC.velocity = new Vector2(16, 0).RotatedBy(NPC.localAI[1]);
								if (NPC.velocity.Y > 0)
								{
									for (int k = 0; k < 150; k++)
									{
										if (player.Hitbox.Top <= NPC.Hitbox.Bottom && Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, player.Hitbox.Top > NPC.Hitbox.Bottom, player.Hitbox.Top > NPC.Hitbox.Bottom) != new Vector2(0, NPC.velocity.Y))
										{
											break;
										}
										else
										{
											NPC.position += NPC.velocity;
										}
									}
								}
								else
								{
									NPC.position += NPC.velocity * 150;
								}
								NPC.position += Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, (player.Hitbox.Top > NPC.Hitbox.Bottom), (player.Hitbox.Top > NPC.Hitbox.Bottom));
								NPC.velocity = Vector2.Zero;
								CollisionDusts();

								player.GetModPlayer<PolaritiesPlayer>().AddScreenShake(36, 30);

								//starburst
								//shoot more projectiles in FTW
								for (int i = 0; i < (baseArmCount * 3); i++)
								{
									Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, 32), new Vector2(8, 0).RotatedBy(-MathHelper.TwoPi * (2 * i + 1) / (baseArmCount * 6)), ProjectileType<UnfriendlyFallenStar>(), 16, 0, Main.myPlayer);
								}
								Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<StarConstructDash>(), 16, 0, Main.myPlayer, ai0: startPosition.X, ai1: startPosition.Y);

								SoundEngine.PlaySound(SoundID.NPCDeath14, NPC.Center);
							}
							RunAbout(0, canRise: false);

							NPC.frame.Y = 0;
							NPC.rotation = 0;
						}

						NPC.ai[1]++;
						if (NPC.ai[1] >= 390)
						{
							gotoNextAttack = true;
						}
					}
					break;
				case 5:
					//helicopter attack
					if (NPC.ai[1] < 120)
					{
						if (NPC.velocity.Y == 0 && NPC.ai[1] >= 60)
						{
							NPC.ai[1] = 120;

							NPC.velocity.Y = -15;

							//dusts on jumping
							JumpDusts();
						}
						else
						{
							RunAbout(VelocityMultiplier);
						}
					}
					else if (NPC.ai[1] < 260)
					{
						if (NPC.ai[1] == 120)
						{
							NPC.velocity.Y = -15;

							//dusts on jumping
							JumpDusts();
						}

						if (NPC.ai[1] == 170)
						{
							SoundEngine.PlaySound(Sounds.StarConstructScream, NPC.Center);

							NPC.ai[3] = player.Center.X > NPC.Center.X ? 1 : -1;

							NPC.velocity.X = 0;
						}

						if (NPC.ai[1] < 170)
						{
							NPC.velocity.X *= 0.97f;
						}
						else
						{
							NPC.noGravity = true;
							NPC.noTileCollide = true;

							//this is multiplied by 1 / (NPC.localAI[0] == 1 ? 60f : 40f) * baseArmCount / 4f because having more claws doesn't really make this attack more dangerous
							//This makes it harder both in phase 2 and in FTW
							NPC.velocity.Y = (player.Center.Y - NPC.Center.Y) / (NPC.localAI[0] == 1 ? 60f : 40f) * baseArmCount / 4f;
							NPC.velocity.X += (NPC.ai[1] < 215 ? NPC.ai[3] : -NPC.ai[3]) * VelocityMultiplier * 0.05f;

							NPC.frame.Y = NPC.frame.Height;
							NPC.rotation += NPC.velocity.X * 0.02f;
						}

						FlyingLegAnim();
					}
					else
					{
						RunAbout(0, canRise: false);
						NPC.frame.Y = 0;
						NPC.rotation = 0;
						NPC.ai[3] = 0;
					}

					NPC.ai[1]++;
					if (NPC.ai[1] >= 270)
					{
						gotoNextAttack = true;
					}
					break;
				case 6:
					//jump into the air and shoot converging star streams directly downwards
					if (NPC.ai[1] < 120)
					{
						if (NPC.velocity.Y == 0 && NPC.ai[1] >= 60)
						{
							NPC.ai[1] = 120;

							NPC.velocity.Y = -17;

							//dusts on jumping
							JumpDusts();
						}
						else
						{
							RunAbout(VelocityMultiplier);
						}
					}
					else if (NPC.ai[1] < 180)
					{
						NPC.velocity.X *= 0.97f;

						FlyingLegAnim();
					}
					else if (NPC.ai[1] < 300)
					{
						if (NPC.ai[1] == 180)
						{
							NPC.velocity.X = 0;

							SoundEngine.PlaySound(Sounds.StarConstructScream, NPC.Center);
						}

						NPC.velocity.Y -= 0.1f * (float)Math.Sin((NPC.ai[1] - 180) / (1500f / VelocityMultiplier) * MathHelper.PiOver2);

						NPC.noGravity = true;
						NPC.noTileCollide = true;

						FlyingLegAnim();
					}
					else
					{
						RunAbout(0, canRise: false);
					}

					NPC.ai[1]++;
					if (NPC.ai[1] >= 420)
					{
						gotoNextAttack = true;
					}
					break;
				case -1:
					//transition to phase 2
					if (NPC.ai[1] == 0)
					{
						//do this so we can detect jumping
						NPC.ai[2] = 1;
					}
					if (NPC.ai[1] < 120)
					{
						RunAbout(0, jumpSpeed: 15, forceJump: true, jumpCooldown: 0);
						NPC.ai[3] = player.Center.X > NPC.Center.X ? 1 : -1;

						//if just jumped, set ai[1] to 120
						if (NPC.ai[2] == 0)
						{
							NPC.ai[1] = 120;
						}
					}
					else if (NPC.ai[1] < 210)
					{
						//spin about
						NPC.velocity.X *= 0.97f;
						NPC.velocity.Y += 0.5f;
						if (NPC.velocity.Y > 0) NPC.velocity.Y = 0;

						if (NPC.ai[1] >= 150)
						{
							NPC.frame.Y = NPC.frame.Height;
							NPC.rotation += NPC.ai[3] * 60f / (360f - NPC.ai[1]);
						}

						if (NPC.ai[1] == 150)
						{
							SoundEngine.PlaySound(Sounds.StarConstructScream, NPC.Center);
						}

						if (NPC.ai[1] == 180)
						{
							NPC.localAI[0] = 2;

							//spawn new arms
							for (int i = baseArmCount; i < baseArmCount * 2; i++)
							{
								arm[i] = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<StarConstructClaw>(), ai0: NPC.whoAmI, ai1: i);
							}

							//update frame
							NPC.frame.X = NPC.frame.Width;
						}

						NPC.noGravity = true;
						NPC.noTileCollide = true;
					}
					else
					{
						RunAbout(0, canRise: false);
						NPC.rotation = 0;
						NPC.frame.Y = 0;
					}

					NPC.ai[1]++;
					if (NPC.ai[1] >= 220)
					{
						gotoNextAttack = true;
					}

					NPC.dontTakeDamage = true;
					break;
			}
			if (gotoNextAttack)
			{
				//reset timer and jump cooldown
				NPC.ai[1] = 0;
				NPC.ai[2] = 0;

				if (NPC.life < NPC.lifeMax * (Main.expertMode ? 0.6f : 0.5f) && NPC.localAI[0] == 1)
				{
					//begin phase 2 transition
					NPC.ai[0] = -1;
				}
				else
				{
					//typical transition
					GotoNextAIState();
				}
			}

			//don't collide if moving upwards
			if (NPC.velocity.Y < 0)
			{
				NPC.noTileCollide = true;
			}

			//special code for gravity and tile collision, so it only collides downwards
			if (!NPC.noGravity)
			{
				NPC.velocity.Y += 0.3f;
				if (NPC.velocity.Y > 16)
				{
					NPC.velocity.Y = 16;
				}
			}
			if (!NPC.noTileCollide && NPC.velocity.Y > 0 && Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height, (player.Hitbox.Top > NPC.Hitbox.Bottom), (player.Hitbox.Top > NPC.Hitbox.Bottom)) != new Vector2(0, NPC.velocity.Y))
			{
				if (NPC.velocity.Y >= 12)
				{
					CollisionDusts();
				}

				NPC.position.Y += Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height).Y;
				NPC.velocity.Y = 0;
			}
			//don't use vanilla gravity/tile collision
			NPC.noGravity = true;
			NPC.noTileCollide = true;

			if (legAnimPhase >= MathHelper.TwoPi * legAnimPeriodMult) legAnimPhase -= MathHelper.TwoPi * legAnimPeriodMult;
			else if (legAnimPhase < 0) legAnimPhase += MathHelper.TwoPi * legAnimPeriodMult;
		}

		private float[] aiWeights = new float[7];
		private void GotoNextAIState()
		{
			WeightedRandom<int> aiStatePool = new WeightedRandom<int>();
			for (int state = 0; state < aiWeights.Length; state++)
			{
				//weights are squared to bias more towards attacks that haven't been used in a while
				aiStatePool.Add(state, Math.Pow(aiWeights[state], 2));
			}
			NPC.ai[0] = aiStatePool;
			for (int state = 0; state < aiWeights.Length; state++)
			{
				if (NPC.ai[0] != state)
					aiWeights[state] += aiWeights[(int)NPC.ai[0]] / (aiWeights.Length - 1);
			}
			aiWeights[(int)NPC.ai[0]] = 0f;

			/*debug thing 
			string aiWeightsString = "";
			for (int state = 0; state < aiWeights.Length; state++)
			{
				aiWeightsString += "aiWeights["+state+"]: "+aiWeights[state]+"\n";
			}
			Main.NewTextMultiline("AI0: " + (int)NPC.ai[0] + "\n" + aiWeightsString);*/
		}
		private void InitializeAIStates()
		{
			aiWeights[0] = 0;
			for (int state = 1; state < aiWeights.Length; state++)
			{
				aiWeights[state] = 1f;
			}
		}

		private void RunAbout(float speed, bool canRise = true, float jumpSpeed = 0, bool forceTurn = false, int jumpCooldown = 60, bool forceJump = false)
		{
			if (!NPC.noGravity)
			{
				NPC.TargetClosest(false);
				//accelerate towards target
				if (NPC.velocity.Y == 0)
				{
					if (Math.Abs(NPC.velocity.X) < speed)
					{
						//only accelerate if going towards the target or if far enough away
						if ((NPC.velocity.X != 0 && ((Main.player[NPC.target].Center.X > NPC.Center.X) == (NPC.velocity.X > 0))) || Math.Abs(NPC.Center.X - Main.player[NPC.target].Center.X) > 128)
						{
							NPC.velocity.X += Main.player[NPC.target].Center.X > NPC.Center.X ? 0.4f : -0.4f;
						}
						else if (Math.Abs(NPC.Center.X - Main.player[NPC.target].Center.X) <= 128 && (NPC.velocity.X == 0 || ((Main.player[NPC.target].Center.X > NPC.Center.X) != (NPC.velocity.X > 0))))
						{
							//if too close and going away, accelerate away
							NPC.velocity.X += Main.player[NPC.target].Center.X > NPC.Center.X ? -0.4f : 0.4f;
						}
					}
					else
					{
						//decelerate if going too fast
						if (Math.Abs(NPC.velocity.X) > 0.4f)
						{
							NPC.velocity.X += NPC.velocity.X > 0 ? -0.4f : 0.4f;
						}
						else
						{
							NPC.velocity.X = 0;
						}
					}
					//smooth legs if still
					if (NPC.velocity.X == 0)
					{
						FlyingLegAnim();
					}

					//move along the leg animation
					legAnimPhase += NPC.velocity.X;

					//count and use jump cooldown
					NPC.ai[2]++;
					if (NPC.ai[2] >= jumpCooldown)
					{
						//try to jump since we're on the ground
						if (jumpSpeed > 0 && (forceJump || Main.player[NPC.target].Hitbox.Bottom < NPC.Hitbox.Top))
						{
							NPC.ai[2] = 0;
							NPC.velocity.Y = -jumpSpeed;

							//force a turn if needed
							if (forceTurn)
							{
								NPC.velocity.X = Main.player[NPC.target].Center.X > NPC.Center.X ? speed : -speed;
							}

							//dusts on jumping
							JumpDusts();
						}
					}
				}
				else
				{
					FlyingLegAnim();
				}
				if (canRise && NPC.velocity.Y >= 0)
				{
					//rise through tiles if possible
					if ((15 + NPC.Hitbox.Bottom) / 16 > Main.player[NPC.target].Hitbox.Bottom / 16)
					{
						if (Collision.TileCollision(NPC.position, new Vector2(0, 16), NPC.width, NPC.height - 17) != new Vector2(0, 16) && NPC.velocity.Y > -8)
						{
							NPC.noGravity = true;
							NPC.velocity.Y = 0;
							NPC.position.Y -= 8;
						}
					}
				}
			}
			else
			{
				FlyingLegAnim();
			}

			NPC.velocity *= 0.99f;
		}

		private void CollisionDusts()
		{
			//collision dusts
			SoundEngine.PlaySound(SoundID.Item10, NPC.Center);
			Color newColor7 = Color.CornflowerBlue;
			if (Main.tenthAnniversaryWorld)
			{
				newColor7 = Color.HotPink;
				newColor7.A = (byte)(newColor7.A / 2);
			}
			float scaleMult = 8;
			Vector2 useVelocity = NPC.velocity.SafeNormalize(-Vector2.UnitY) * 16;
			for (int num573 = 0; num573 < 7 * scaleMult; num573++)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, 58, useVelocity.X * 0.1f, useVelocity.Y * 0.1f, 150, default(Color), 0.8f);
			}
			for (float num574 = 0f; num574 < 1f * scaleMult; num574 += 0.125f)
			{
				Vector2 center25 = NPC.Center;
				Vector2 unitY11 = Vector2.UnitY;
				double radians36 = num574 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f;
				Vector2 center2 = default(Vector2);
				Dust.NewDustPerfect(center25, 278, unitY11.RotatedBy(radians36, center2) * (4f + Main.rand.NextFloat() * 4f), 150, newColor7).noGravity = true;
			}
			for (float num575 = 0f; num575 < 1f * scaleMult; num575 += 0.25f)
			{
				Vector2 center26 = NPC.Center;
				Vector2 unitY12 = Vector2.UnitY;
				double radians37 = num575 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f;
				Vector2 center2 = default(Vector2);
				Dust.NewDustPerfect(center26, 278, unitY12.RotatedBy(radians37, center2) * (2f + Main.rand.NextFloat() * 3f), 150, Color.Gold).noGravity = true;
			}
			Vector2 value15 = new Vector2((float)Main.screenWidth, (float)Main.screenHeight);
			if (NPC.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + value15 / 2f, value15 + new Vector2(400f))))
			{
				for (int num576 = 0; num576 < 7 * scaleMult; num576++)
				{
					Vector2 val29 = NPC.Center - new Vector2(11) + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)) * (NPC.Size - new Vector2(22)) / 2f;
					Vector2 val30 = Main.rand.NextVector2CircularEdge(0.5f, 0.5f) * useVelocity.Length();

					int[] array18 = new int[8] { 16, 17, 17, 17, 17, 17, 17, 17 };
					Gore.NewGore(NPC.GetSource_FromAI(), val29, val30, Utils.SelectRandom(Main.rand, array18));
				}
			}
		}

		private void FallingConstructDusts()
		{
			Vector2 value34 = new Vector2((float)Main.screenWidth, (float)Main.screenHeight);
			for (int i = 0; i < 4; i++)
			{
				if (NPC.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + value34 / 2f, value34 + new Vector2(400f))) && Main.rand.NextBool(6))
				{
					int[] array6 = new int[4] { 16, 17, 17, 17 };
					int num855 = Utils.SelectRandom(Main.rand, array6);
					if (Main.tenthAnniversaryWorld)
					{
						int[] array7 = new int[4] { 16, 16, 16, 17 };
						num855 = Utils.SelectRandom(Main.rand, array7);
					}
					Gore.NewGore(NPC.GetSource_FromAI(), NPC.Center - new Vector2(11) + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)) * (NPC.Size - new Vector2(22)) / 2f, NPC.velocity * 0.2f, num855);
				}
				if (Main.rand.NextBool(20) || (Main.tenthAnniversaryWorld && Main.rand.NextBool(15)))
				{
					Dust.NewDust(NPC.position, NPC.width, NPC.height, 58, NPC.velocity.X * 0.5f, NPC.velocity.Y * 0.5f, 150, default(Color), 1.2f);
				}
			}
		}

		private void JumpDusts()
        {
			Vector2 value34 = new Vector2((float)Main.screenWidth, (float)Main.screenHeight);
			for (int i = 0; i < 64; i++)
			{
				if (NPC.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + value34 / 2f, value34 + new Vector2(400f))) && Main.rand.NextBool(6))
				{
					int[] array6 = new int[4] { 16, 17, 17, 17 };
					int num855 = Utils.SelectRandom(Main.rand, array6);
					if (Main.tenthAnniversaryWorld)
					{
						int[] array7 = new int[4] { 16, 16, 16, 17 };
						num855 = Utils.SelectRandom(Main.rand, array7);
					}
					Gore.NewGore(NPC.GetSource_FromAI(), NPC.Center - new Vector2(11) + new Vector2(Main.rand.NextFloat(-1f, 1f), 1f) * (NPC.Size - new Vector2(22)) / 2f, NPC.velocity * Main.rand.NextFloat(0.1f, 0.5f), num855);
				}
				if (Main.rand.NextBool(20) || (Main.tenthAnniversaryWorld && Main.rand.NextBool(15)))
				{
					float speedMult = Main.rand.NextFloat(0f, 0.9f);
					Dust.NewDust(NPC.BottomLeft, NPC.width, 0, DustID.Enchanted_Pink, NPC.velocity.X * speedMult, NPC.velocity.Y * speedMult, 150, default(Color), 1.2f);
				}
			}
		}

		public override bool CheckActive()
		{
			return !Main.player[NPC.target].active || !Main.player[NPC.target].dead || Main.dayTime;
		}

		private bool canDamage
		{
			get => NPC.ai[0] == 4 && (NPC.localAI[0] == 1 || !Main.expertMode) && NPC.ai[1] >= 240 && NPC.ai[1] < 480;
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (canDamage)
			{
				return null;
			}
			return false;
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return canDamage;
		}

		private const float legAnimPeriodMult = 10f;
		private float legAnimPhase = MathHelper.PiOver2 * legAnimPeriodMult;
		private const float animReturnSpeed = 0.5f;

		private void FlyingLegAnim()
		{
			if (legAnimPhase < MathHelper.PiOver2 * legAnimPeriodMult || (legAnimPhase >= MathHelper.Pi * legAnimPeriodMult && legAnimPhase < 3 * MathHelper.PiOver2 * legAnimPeriodMult)) legAnimPhase += animReturnSpeed;
			else legAnimPhase -= animReturnSpeed;
		}

        public override void FindFrame(int frameHeight)
        {
            if (NPC.IsABestiaryIconDummy)
			{
				NPC.frame.X = 0;
				NPC.frame.Y = 0;

				NPC.velocity.X = 6;
				legAnimPhase += NPC.velocity.X;

				NPC.ai[0] = 0;
			}
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			drawColor = NPC.GetNPCColorTintedByBuffs(Color.White);

			//just set the npc frame width to this every time because idk where else to do it
			NPC.frame.Width = 126;

			//for bestiary dummy drawing
			if (NPC.IsABestiaryIconDummy)
            {
				for (int i = 0; i < 4; i++)
				{
					Vector2 hookCenter = NPC.Center + new Vector2(100, 0).RotatedBy(-i * MathHelper.Pi / 3);
					float hookRotation = MathHelper.Pi - i * MathHelper.Pi / 3;

					//just draw like a normal chain, but using the bezier curve thing because otherwise we get these weird jumps for some reason
					Vector2[] bezierPoints = { NPC.Center, hookCenter };
					float bezierProgress = 0;
					float bezierIncrement = 18;

					Vector2 textureCenter = new Vector2(11, 10);

					while (bezierProgress < 1)
					{
						//draw stuff
						Vector2 oldPos = ModUtils.BezierCurve(bezierPoints, bezierProgress);

						//increment progress
						while ((oldPos - ModUtils.BezierCurve(bezierPoints, bezierProgress)).Length() < bezierIncrement)
						{
							bezierProgress += 0.1f / ModUtils.BezierCurveDerivative(bezierPoints, bezierProgress).Length();
						}

						Vector2 newPos = ModUtils.BezierCurve(bezierPoints, bezierProgress);
						float rotation = (newPos - oldPos).ToRotation();

						Vector2 drawingPos = (oldPos + newPos) / 2;

						Main.spriteBatch.Draw(ChainTexture.Value, drawingPos - screenPos, new Rectangle(0, 0, 22, 20), drawColor, rotation, textureCenter, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipVertically, 0f);
					}

					Vector2 hookDrawOrigin = ClawTexture.Value.Size() / 2;
					Rectangle hookFrame = ClawTexture.Value.Frame();
					Vector2 hookDrawPos = hookCenter - screenPos;
					if (NPC.spriteDirection == -1)
					{
						Main.spriteBatch.Draw(ClawTexture.Value, hookDrawPos, hookFrame, drawColor, hookRotation, hookDrawOrigin, NPC.scale, SpriteEffects.None, 0f);
					}
					else
					{
						Main.spriteBatch.Draw(ClawTexture.Value, hookDrawPos, hookFrame, drawColor, hookRotation, hookDrawOrigin, NPC.scale, SpriteEffects.FlipHorizontally, 0f);
					}
				}
			}

			if (NPC.ai[0] == 4 && Main.expertMode && NPC.localAI[0] != 1 && NPC.ai[1] >= 240 && NPC.ai[1] < 360)
			{
				if (NPC.ai[1] < 340)
				{;
					Rectangle frame = DashTexture.Value.Frame();
					Vector2 origin = new Vector2(DashTexture.Value.Width, DashTexture.Value.Height / 2);

					float scale = (340 - NPC.ai[1]) / 40f;

					spriteBatch.Draw(DashTexture.Value, NPC.Center - screenPos, frame, Color.White * (0.2f / scale), NPC.localAI[1] + MathHelper.Pi, origin, new Vector2(10, scale), SpriteEffects.None, 0f);
				}
				else
				{
					Rectangle frame = new Rectangle(0, 0, 1, 1);
					Vector2 origin = new Vector2(0, frame.Height / 2f);

					spriteBatch.Draw(Textures.PixelTexture.Value, NPC.Center - screenPos, frame, Color.White, NPC.localAI[1], origin, new Vector2(2400, 1), SpriteEffects.None, 0f);
				}
			}

			Vector2 drawOrigin = NPC.frame.Size() / 2;
			Vector2 drawPos = NPC.Center - screenPos;
			for (int i = 0; i < 4; i++)
			{
				float flattening = 8f;
				float heightFactor = 8f;

				float xOffset = 4f / 3f * legAnimPeriodMult * (float)Math.Sin(legAnimPhase / legAnimPeriodMult + i * MathHelper.Pi - 1 / 4f * Math.Sin(legAnimPhase / legAnimPeriodMult + i * MathHelper.Pi));
				float yOffset = legAnimPeriodMult * flattening * heightFactor * (float)Math.Exp(flattening / (Math.Cos(legAnimPhase / legAnimPeriodMult + i * MathHelper.Pi) - 1));

				Vector2 drawLegOffset = new Vector2(xOffset, yOffset);

				spriteBatch.Draw(LegTexture.Value, drawPos - drawLegOffset, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, (i == 0 || i == 3) ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
			}

			if (canDamage)
			{
				//adapted from fallen star drawcode
				float scaleMultiplier = 3f;

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

			spriteBatch.Draw(TextureAssets.Npc[Type].Value, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);

			//phase transition aura
			if (NPC.ai[0] == -1 && NPC.ai[1] >= 150 && NPC.ai[1] < 210)
			{
				Rectangle frame2 = Textures.Glow256.Value.Frame();

				float scale = 1.5f * (NPC.ai[1] - 150) * (210 - NPC.ai[1]) / (30f * 30f);

				Color glowColor = new Color(Vector3.Lerp(new Color(47, 255, 255).ToVector3(), new Color(255, 239, 186).ToVector3(), (NPC.ai[1] - 150) / 50f));

				spriteBatch.Draw(Textures.Glow256.Value, NPC.Center - screenPos, frame2, glowColor, 0f, frame2.Size() / 2, NPC.scale * scale, SpriteEffects.None, 0f);
			}
			return false;
		}

		public override void DrawBehind(int index)
		{
			Main.instance.DrawCacheNPCProjectiles.Add(index);
		}

		public override bool CheckDead()
		{
			if (!PolaritiesSystem.downedStarConstruct)
			{
				NPC.SetEventFlagCleared(ref PolaritiesSystem.downedStarConstruct, -1);
			}

			SoundEngine.PlaySound(Sounds.StarConstructRoar, NPC.Center);

			for (int i = 1; i <= 6; i++)
			{
				Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity, Mod.Find<ModGore>("StarConstructGore" + i).Type);
			}
			return true;
		}

        public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(new FlawlessOrRandomDropRule(ItemType<Items.Placeable.Trophies.StarConstructTrophy>(), 10));
			npcLoot.Add(ItemDropRule.BossBag(ItemType<Items.Consumables.TreasureBags.StarConstructBag>()));
			npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ItemType<Items.Placeable.Relics.StarConstructRelic>()));
			npcLoot.Add(ModUtils.MasterModeDropOnAllPlayersOrFlawless(ItemType<Items.Pets.StarConstructPetItem>(), 4));

			//normal mode loot
			LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
			notExpertRule.OnSuccess(ItemDropRule.Common(ItemType<Items.Armor.Vanity.StarConstructMask>(), 7));
			notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.SkyMill));
			notExpertRule.OnSuccess(ItemDropRule.Common(ItemType<Items.Placeable.Bars.SunplateBar>(), minimumDropped: 12, maximumDropped: 20));
			notExpertRule.OnSuccess(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemType<Items.Hooks.Starhook>(), ItemType<Items.Accessories.StarBadge>(), ItemType<StrangeClock>()));
			npcLoot.Add(notExpertRule);

			npcLoot.Add(ItemDropRule.ByCondition(new FlawlessDropCondition(), ItemType<Items.Weapons.Melee.Stardance>()));
		}
    }

	internal class StarConstructClaw : ModNPC
	{
		static int baseArmCount => Main.getGoodWorld ? 6 : 4;

		public override void SetStaticDefaults()
		{
			NPCID.Sets.CantTakeLunchMoney[Type] = true;

			var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
			{
				//don't show up in bestiary
				Hide = true
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

			NPCID.Sets.TrailCacheLength[NPC.type] = 10;
			NPCID.Sets.TrailingMode[NPC.type] = 3;
		}

		public override void SetDefaults()
		{
			NPC.aiStyle = -1;
			NPC.width = 32;
			NPC.height = 32;
			DrawOffsetY = 7;

			NPC.defense = 5;
			NPC.damage = 24;
			NPC.lifeMax = 750;
			NPC.knockBackResist = 0f;
			NPC.npcSlots = 0f;
			NPC.lavaImmune = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath14;

			NPC.dontCountMe = true;
			NPC.dontTakeDamage = true;
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
		{
			NPC.lifeMax = (int)(1000 * bossLifeScale);
		}

		public override void AI()
		{
			Lighting.AddLight(NPC.Center, 1f, 1f, 1f);

			if (NPC.localAI[0] == 0)
			{
				NPC.localAI[0] = 1;
			}

			NPC construct = Main.npc[(int)NPC.ai[0]];

			if (!construct.active)
			{
				NPC.life = 0;
				NPC.HitEffect(0, 0);
				NPC.checkDead();
				NPC.active = false;
			}

			Player player = Main.player[construct.target];

			switch (construct.ai[0])
			{
				case 0:
					//float in place during the standard running attack
					FloatInPlace(StarConstruct.VelocityMultiplier);
					break;
				case 1:
					//partially enclose the player before dashing at them, repeatedly
					if (construct.ai[1] < 30 || construct.ai[1] >= 420)
					{
						FloatInPlace(StarConstruct.VelocityMultiplier);
					}
					else if (construct.ai[1] >= 390)
					{
						float goalRadius = Math.Max(200, (NPC.Center - player.Center).Length());
						float goalAngle = (construct.Center - player.Center).ToRotation();
						float nextRadius = ModUtils.Lerp((NPC.Center - player.Center).Length(), goalRadius, 0.05f);
						float nextAngle = Utils.AngleLerp((NPC.Center - player.Center).ToRotation(), goalAngle, 0.05f);
						NPC.velocity = player.Center + new Vector2(nextRadius, 0).RotatedBy(nextAngle) - NPC.Center;

						NPC.rotation = (NPC.Center - player.Center).ToRotation();
					}
					else
					{
						if ((construct.ai[1] - 30) % 120 < 60)
						{
							float interpolationFactor = ((construct.ai[1] - 30) % 120 + 5f) / 300f;

							//the distance between arms is decreased slightly in ftw to make p2 actually survivable
							float rotationAmountMult = baseArmCount == 4 ? MathHelper.Pi / 6 : MathHelper.Pi / 7; 

							float goalRadius = 200;
							float goalAngle = MathHelper.WrapAngle(construct.ai[3] + ((int)NPC.ai[1] / 2) * rotationAmountMult + (NPC.ai[1] % 2) * MathHelper.Pi + MathHelper.Pi) - MathHelper.Pi; //this needs +pi - pi for some reason
							float nextRadius = ModUtils.Lerp((NPC.Center - player.Center).Length(), goalRadius, interpolationFactor);
							float nextAngle = Utils.AngleLerp((NPC.Center - player.Center).ToRotation(), goalAngle, interpolationFactor);
							NPC.velocity = player.Center + new Vector2(nextRadius, 0).RotatedBy(nextAngle) - NPC.Center;
						}
						else if ((construct.ai[1] - 30) % 120 == 60)
						{
							NPC.velocity = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * StarConstruct.VelocityMultiplier * -0.66f;
						}
						else if ((construct.ai[1] - 30) % 120 == 85)
						{
							NPC.velocity *= -4f;

							//play whoosh sound
							SoundEngine.PlaySound(SoundID.Item1, NPC.Center);
						}
						else if ((construct.ai[1] - 30) % 120 == 110)
						{
							NPC.velocity = Vector2.Zero;
						}

						NPC.rotation = (NPC.Center - player.Center).ToRotation();
					}
					break;
				case 2:
					//swing hooks in a predictable pattern
					if (construct.ai[1] < 90 || construct.ai[1] >= 450)
					{
						FloatInPlace(StarConstruct.VelocityMultiplier);

						//ai[2] is for sounds
						NPC.ai[2] = 0;
					}
					else
					{
						NPC.rotation = (construct.Center - NPC.Center).ToRotation();
						float goalRadius = 100 + 500 * (float)Math.Pow(Math.Sin((construct.ai[1] - 90) / 360 * MathHelper.Pi), 2);

						//some sort of nonsense magic formula for angle
						float goalRotation = (NPC.ai[1] >= baseArmCount ? 1 : -1) * construct.ai[3] * (MathHelper.TwoPi * (float)Math.Cos((construct.ai[1] - 90) / 360 * MathHelper.Pi) + MathHelper.TwoPi / baseArmCount * ((NPC.ai[1] * construct.ai[3]) * (NPC.ai[1] >= baseArmCount ? -1 : 1) + 3.5f + (construct.ai[3] == 1 ? 0 : (NPC.ai[1] >= baseArmCount ? 3 : 1)) + (NPC.ai[1] >= baseArmCount ? -3 : 0)));

						Vector2 targetCenter = construct.Center - construct.velocity + new Vector2(goalRadius, 0).RotatedBy(goalRotation);
						Vector2 direction = targetCenter - NPC.Center;

						if (direction.Length() > Math.Max(StarConstruct.VelocityMultiplier, goalRadius * 0.1f)) { direction.Normalize(); direction *= Math.Max(StarConstruct.VelocityMultiplier, goalRadius * 0.1f); }
						NPC.velocity = direction + construct.velocity;

						if ((MathHelper.WrapAngle(construct.ai[3] * goalRotation) + MathHelper.Pi) % (MathHelper.TwoPi / baseArmCount) - MathHelper.Pi / baseArmCount < 0)
						{
							if (NPC.ai[2] == 0)
							{
								if (goalRadius > 360)
								{
									//play whoosh sound
									SoundEngine.PlaySound(SoundID.Item1, NPC.Center);
								}
								NPC.ai[2] = 1;
							}
						}
						else
						{
							NPC.ai[2] = 0;
						}
					}
					break;
				case 3:
					//boss planteras/phages after the player
					if (construct.ai[1] < 60)
					{
						FloatInPlace(StarConstruct.VelocityMultiplier);
					}
					else if (construct.ai[1] >= 570)
					{
						FloatInPlace(StarConstruct.VelocityMultiplier * 2);
					}
					else if (construct.ai[1] >= 540)
					{
						Vector2 goalPosition = new Vector2(NPC.Center.X, player.Center.Y - 300);
						Vector2 goalVelocity = (goalPosition - NPC.Center) / 10f;
						NPC.velocity += (goalVelocity - NPC.velocity) / 10f;
					}
					else
					{
						NPC.rotation = -MathHelper.PiOver2;

						if (construct.ai[1] == 60)
						{
							//attach all hooks directly downwards to start with
							int x = (int)NPC.Center.X / 16;
							for (int y = (int)NPC.Center.Y / 16; y < (int)NPC.Center.Y / 16 + 100; y++)
							{
								Tile targetTile = Framing.GetTileSafely(x, y);
								if (targetTile.HasUnactuatedTile && Main.tileSolid[targetTile.TileType])
								{
									NPC.ai[2] = x * 16 + 8;
									NPC.ai[3] = y * 16 + 8;
									break;
								}
							}

							//set ourselves to disconnected from the ground
							NPC.localAI[1] = 1;
						}

						const int loopTime = 240;

						int myTime = (int)((construct.ai[1] - 30) + (loopTime * 5 * index) / numHooks);
						int maxTime = (int)construct.localAI[0] * 60 + 10;

						if (myTime % loopTime < maxTime)
						{
							//set ourselves to disconnected from the ground
							NPC.localAI[1] = 1;

							//move up
							if (myTime % loopTime < 15)
							{
								Vector2 goalPosition = new Vector2(NPC.Center.X, player.Center.Y - 300);
								Vector2 goalVelocity = (goalPosition - NPC.Center) / 10f;
								NPC.velocity += (goalVelocity - NPC.velocity) / 10f;
							}
							//move to relevant position
							else if (myTime % loopTime < maxTime - 25)
							{
								Vector2 goalPosition = player.Center + new Vector2((float)Math.Sin(index / (float)numHooks * MathHelper.TwoPi + ((construct.ai[3] == 1) ? 0 : MathHelper.Pi)) * 300, -300);
								Vector2 goalVelocity = (goalPosition - NPC.Center) / 10f + (player.position - player.oldPosition);
								NPC.velocity += (goalVelocity - NPC.velocity) / 10f;
							}
							//hold still and move up pre-slam
							else
							{
								NPC.velocity = new Vector2(0, -StarConstruct.VelocityMultiplier * 0.66f);
							}
						}
						else
						{
							//set new hook point
							if (myTime % loopTime == maxTime)
							{
								int x = (int)NPC.Center.X / 16;

								//default values assuming we don't find an actual tile
								NPC.ai[2] = x * 16 + 8;
								NPC.ai[3] = (player.position.Y / 16 + 100) * 16 + 8;

								for (int y = (int)player.position.Y / 16; y < (int)player.position.Y / 16 + 100; y++)
								{
									Tile targetTile = Framing.GetTileSafely(x, y);
									if (targetTile.HasUnactuatedTile && Main.tileSolid[targetTile.TileType])
									{
										NPC.ai[2] = x * 16 + 8;
										NPC.ai[3] = y * 16 + 8;
										break;
									}
								}

								//set ourselves to disconnected from the ground
								NPC.localAI[1] = 1;

								//play whoosh sound
								SoundEngine.PlaySound(SoundID.Item1, NPC.Center);
							}

							//go to hook point
							NPC.velocity = new Vector2(NPC.ai[2], NPC.ai[3]) - NPC.Center;
							if (NPC.velocity.Length() > StarConstruct.VelocityMultiplier * 3)
							{
								NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * StarConstruct.VelocityMultiplier * 3;
							}
							else
							{
								//slam down
								if (NPC.localAI[1] == 1)
								{
									SoundEngine.PlaySound(SoundID.NPCDeath14, NPC.Center);

									//set ourselves to connected to the ground
									NPC.localAI[1] = 0;

									//collision dusts
									SoundEngine.PlaySound(SoundID.Item10, NPC.Center);
									Color newColor7 = Color.CornflowerBlue;
									if (Main.tenthAnniversaryWorld)
									{
										newColor7 = Color.HotPink;
										newColor7.A = (byte)(newColor7.A / 2);
									}
									float scaleMult = 1;
									Vector2 useVelocity = NPC.velocity.SafeNormalize(-Vector2.UnitY) * 16;
									for (int num573 = 0; num573 < 7 * scaleMult; num573++)
									{
										Dust.NewDust(NPC.position, NPC.width, NPC.height, 58, useVelocity.X * 0.1f, useVelocity.Y * 0.1f, 150, default(Color), 0.8f);
									}
									for (float num574 = 0f; num574 < 1f * scaleMult; num574 += 0.125f)
									{
										Vector2 center25 = NPC.Center;
										Vector2 unitY11 = Vector2.UnitY;
										double radians36 = num574 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f;
										Vector2 center2 = default(Vector2);
										Dust.NewDustPerfect(center25, 278, unitY11.RotatedBy(radians36, center2) * (4f + Main.rand.NextFloat() * 4f), 150, newColor7).noGravity = true;
									}
									for (float num575 = 0f; num575 < 1f * scaleMult; num575 += 0.25f)
									{
										Vector2 center26 = NPC.Center;
										Vector2 unitY12 = Vector2.UnitY;
										double radians37 = num575 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f;
										Vector2 center2 = default(Vector2);
										Dust.NewDustPerfect(center26, 278, unitY12.RotatedBy(radians37, center2) * (2f + Main.rand.NextFloat() * 3f), 150, Color.Gold).noGravity = true;
									}
									Vector2 value15 = new Vector2((float)Main.screenWidth, (float)Main.screenHeight);
									if (NPC.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + value15 / 2f, value15 + new Vector2(400f))))
									{
										for (int num576 = 0; num576 < 7 * scaleMult; num576++)
										{
											Vector2 val29 = NPC.Center - new Vector2(11) + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)) * (NPC.Size - new Vector2(22)) / 2f;
											Vector2 val30 = Main.rand.NextVector2CircularEdge(0.5f, 0.5f) * useVelocity.Length();
											int[] array18 = new int[8] { 16, 17, 17, 17, 17, 17, 17, 17 };
											Gore.NewGore(NPC.GetSource_FromAI(), val29, val30, Utils.SelectRandom(Main.rand, array18));
										}
									}
								}
							}
						}
					}
					break;
				case 4:
					//hide away during the dive
					if (construct.localAI[0] == 1 || !Main.expertMode)
					{
						if (construct.ai[1] < 240 || construct.ai[1] >= 480)
						{
							FloatInPlace(StarConstruct.VelocityMultiplier);
						}
						else
						{
							Vector2 targetCenter = construct.Center - construct.velocity;
							Vector2 direction = targetCenter - NPC.Center;
							if (direction.Length() > StarConstruct.VelocityMultiplier) { direction.Normalize(); direction *= StarConstruct.VelocityMultiplier; }
							NPC.velocity = direction + construct.velocity;
						}
					}
					else
					{
						if (construct.ai[1] < 240 || construct.ai[1] >= 360)
						{
							if (construct.ai[1] == 360 || construct.ai[1] == 361)
							{
								NPC.Center = construct.Center;
							}
							FloatInPlace(StarConstruct.VelocityMultiplier);
						}
						else
						{
							Vector2 targetCenter = construct.Center - construct.velocity;
							Vector2 direction = targetCenter - NPC.Center;
							if (direction.Length() > StarConstruct.VelocityMultiplier) { direction.Normalize(); direction *= StarConstruct.VelocityMultiplier; }
							NPC.velocity = direction + construct.velocity;
						}
					}
					break;
				case 5:
					//helicopter attack!
					if (construct.ai[1] < 120 || construct.ai[1] >= 260)
					{
						FloatInPlace(StarConstruct.VelocityMultiplier);
					}
					else
					{
						float goalRotation = NPC.ai[1] * MathHelper.TwoPi / numHooks + construct.ai[1] * MathHelper.TwoPi / 60f;
						Vector2 goalOffset = new Vector2(200, 0).RotatedBy(goalRotation);
						float angleFactor = (float)Math.Atan(construct.velocity.X * 0.25f) * 0.33f;
						goalOffset.Y *= angleFactor;
						goalOffset = goalOffset.RotatedBy(angleFactor);
						Vector2 goalPosition = construct.Center + goalOffset;
						Vector2 direction = goalPosition - NPC.Center;
						if (direction.Length() > StarConstruct.VelocityMultiplier * 2) { direction.Normalize(); direction *= StarConstruct.VelocityMultiplier * 2; }
						NPC.velocity = direction + construct.velocity;

						NPC.rotation = (construct.Center - NPC.Center).ToRotation();
					}
					break;
				case 6:
					//angle inwards gradually and shoot star streams
					if (construct.ai[1] < 120 || construct.ai[1] >= 300)
					{
						FloatInPlace(StarConstruct.VelocityMultiplier);
					}
					else if (construct.ai[1] < 180)
					{
						float goalAngle = -(index + 0.5f) * MathHelper.TwoPi / numHooks;
						Vector2 goalPosition = construct.Center + new Vector2(0, 100).RotatedBy(goalAngle);
						Vector2 direction = goalPosition - NPC.Center;
						if (direction.Length() > StarConstruct.VelocityMultiplier) { direction.Normalize(); direction *= StarConstruct.VelocityMultiplier; }
						NPC.velocity = direction + construct.velocity;

						NPC.rotation = (construct.Center - NPC.Center).ToRotation();
					}
					else
					{
						float progress = 1 - (construct.ai[1] - 180) / (1500f / StarConstruct.VelocityMultiplier);
						float angleSpread = progress * MathHelper.TwoPi / numHooks;

						int modifiedIndex = (index + numHooks / 2) % numHooks - numHooks / 2;

						float goalAngle = -(modifiedIndex + 0.5f) * angleSpread;
						Vector2 goalPosition = construct.Center + new Vector2(0, 100).RotatedBy(goalAngle);
						Vector2 direction = goalPosition - NPC.Center;
						if (direction.Length() > StarConstruct.VelocityMultiplier) { direction.Normalize(); direction *= StarConstruct.VelocityMultiplier; }
						NPC.velocity = direction + construct.velocity;

						NPC.rotation = (construct.Center - NPC.Center).ToRotation();

						if (construct.ai[1] % 8 == 0)
						{
							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 8).RotatedBy(goalAngle), ProjectileType<UnfriendlyFallenStar>(), 16, 0, Main.myPlayer);
						}
					}
					break;
				case -1:
					//phase transition
					if (construct.ai[1] < 120 || construct.ai[1] >= 210)
					{
						FloatInPlace(StarConstruct.VelocityMultiplier);
					}
					else if (construct.ai[1] < 150)
					{
						float goalAngle = -(index + 0.5f) * MathHelper.TwoPi / numHooks;
						Vector2 goalPosition = construct.Center + new Vector2(0, 100).RotatedBy(goalAngle);
						Vector2 direction = goalPosition - NPC.Center;
						if (direction.Length() > StarConstruct.VelocityMultiplier) { direction.Normalize(); direction *= StarConstruct.VelocityMultiplier; }
						NPC.velocity = direction + construct.velocity;

						NPC.rotation = (construct.Center - NPC.Center).ToRotation();
					}
					else
					{
						float goalAngle = -(index + 0.5f) * MathHelper.TwoPi / numHooks + construct.ai[3] * (construct.ai[1] - 150) * MathHelper.TwoPi / 60f;
						Vector2 goalPosition = construct.Center + new Vector2(0, 100).RotatedBy(goalAngle);
						Vector2 direction = goalPosition - NPC.Center;
						if (direction.Length() > StarConstruct.VelocityMultiplier) { direction.Normalize(); direction *= StarConstruct.VelocityMultiplier; }
						NPC.velocity = direction + construct.velocity;

						NPC.rotation = (construct.Center - NPC.Center).ToRotation();
					}
					break;
				default:
					FloatInPlace(StarConstruct.VelocityMultiplier);
					break;
			}
		}

		private int index
		{
			get => (int)(Main.npc[(int)NPC.ai[0]].localAI[0] == 1 ? NPC.ai[1] : ((NPC.ai[1] * 2) % (baseArmCount * 2) + (int)NPC.ai[1] / baseArmCount));
		}
		private int numHooks
		{
			get => (int)Main.npc[(int)NPC.ai[0]].localAI[0] * baseArmCount;
		}

		private void FloatInPlace(float VelocityMultiplier)
		{
			NPC construct = Main.npc[(int)NPC.ai[0]];
			NPC.rotation = (construct.Center - NPC.Center).ToRotation();

			Vector2 targetCenter = construct.localAI[0] == 1 ?
				construct.Center + new Vector2(100, 0).RotatedBy(-NPC.ai[1] * Math.PI / (baseArmCount - 1)) - construct.velocity :
				construct.Center + new Vector2(100, 0).RotatedBy(-((NPC.ai[1] * 2) % (2 * baseArmCount) + (int)NPC.ai[1] / baseArmCount) * Math.PI / (2 * baseArmCount - 1)) - construct.velocity;
			Vector2 direction = targetCenter - NPC.Center;
			if (direction.Length() > VelocityMultiplier) { direction.Normalize(); direction *= VelocityMultiplier; }
			NPC.velocity = direction + construct.velocity;
		}

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			NPC construct = Main.npc[(int)NPC.ai[0]];

			drawColor = construct.GetNPCColorTintedByBuffs(Color.White);

			//trail during spinning phase
			if (construct.ai[0] == 2 && construct.ai[1] >= 60)
			{
				float scale = 1f;
				if (construct.ai[1] < 90)
				{
					scale = (construct.ai[1] - 60) / 30f;
				}
				else if (construct.ai[1] >= 450)
				{
					scale = (480 - construct.ai[1]) / 30f;
				}

				Texture2D clawAuraTexture = Textures.Glow58.Value;

				for (int i = 0; i < NPC.oldPos.Length - 1; i++)
				{
					Main.spriteBatch.Draw(clawAuraTexture, NPC.oldPos[i] - NPC.position + NPC.Center - screenPos, clawAuraTexture.Frame(), new Color(47, 255, 255), (NPC.oldPos[i] - NPC.oldPos[i + 1]).ToRotation(), clawAuraTexture.Frame().Size() / 2, new Vector2(1, 1 - i / (float)NPC.oldPos.Length) * scale, SpriteEffects.None, 0f);
				}
			}
			//trail during helicopter phase
			else if (construct.ai[0] == 5 && construct.ai[1] >= 140 && construct.ai[1] < 290)
			{
				float scale = 1f;
				if (construct.ai[1] < 170)
				{
					scale = (construct.ai[1] - 120) / 30f;
				}
				else if (construct.ai[1] >= 260)
				{
					scale = (290 - construct.ai[1]) / 30f;
				}

				Texture2D clawAuraTexture = Textures.Glow58.Value;

				for (int i = 0; i < NPC.oldPos.Length - 1; i++)
				{
					Main.spriteBatch.Draw(clawAuraTexture, NPC.oldPos[i] - NPC.position + NPC.Center - screenPos, clawAuraTexture.Frame(), new Color(47, 255, 255), (NPC.oldPos[i] - NPC.oldPos[i + 1]).ToRotation(), clawAuraTexture.Frame().Size() / 2, new Vector2(1, 1 - i / (float)NPC.oldPos.Length) * scale, SpriteEffects.None, 0f);
				}
			}
			//flash and trail during stomping phase
			else if (construct.ai[0] == 3 && construct.ai[1] >= 65 && construct.ai[1] < 540)
			{
				const int loopTime = 240;

				int myTime = (int)((construct.ai[1] - 30) + (loopTime * 5 * index) / numHooks);
				int maxTime = (int)construct.localAI[0] * 60 + 10;

				if (myTime % loopTime < maxTime && myTime % loopTime >= maxTime - 25)
				{
					Texture2D clawAuraTexture = Textures.Glow58.Value;
					float scale = 2.5f * (maxTime - (myTime % loopTime)) / 25f;

					Main.spriteBatch.Draw(clawAuraTexture, NPC.Center - screenPos, clawAuraTexture.Frame(), new Color(47, 255, 255), 0f, clawAuraTexture.Frame().Size() / 2, scale, SpriteEffects.None, 0f);
				}
				else if (construct.ai[1] >= 100 && myTime % loopTime >= maxTime && NPC.localAI[1] == 1)
				{
					Texture2D clawAuraTexture = Textures.Glow58.Value;
					float scale = 1f;

					for (int i = 0; i < NPC.oldPos.Length - 1; i++)
					{
						Main.spriteBatch.Draw(clawAuraTexture, NPC.oldPos[i] - NPC.position + NPC.Center - screenPos, clawAuraTexture.Frame(), new Color(47, 255, 255), (NPC.oldPos[i] - NPC.oldPos[i + 1]).ToRotation(), clawAuraTexture.Frame().Size() / 2, new Vector2(1, 1 - i / (float)NPC.oldPos.Length) * scale, SpriteEffects.None, 0f);
					}
				}
			}
			//clawing phase flash and trail
			else if (construct.ai[0] == 1 && construct.ai[1] >= 30 && construct.ai[1] < 390)
			{
				if ((construct.ai[1] - 30) % 120 >= 60 && (construct.ai[1] - 30) % 120 < 85)
				{
					Texture2D clawAuraTexture = Textures.Glow58.Value;
					float scale = 2.5f * (85 - ((construct.ai[1] - 30) % 120)) / 25f;

					Main.spriteBatch.Draw(clawAuraTexture, NPC.Center - screenPos, clawAuraTexture.Frame(), new Color(47, 255, 255), 0f, clawAuraTexture.Frame().Size() / 2, scale, SpriteEffects.None, 0f);
				}
				else if ((construct.ai[1] - 30) % 120 >= 85 && (construct.ai[1] - 30) % 120 < 110)
				{
					Texture2D clawAuraTexture = Textures.Glow58.Value;
					float scale = 1f;

					for (int i = 0; i < NPC.oldPos.Length - 1; i++)
					{
						Main.spriteBatch.Draw(clawAuraTexture, NPC.oldPos[i] - NPC.position + NPC.Center - screenPos, clawAuraTexture.Frame(), new Color(47, 255, 255), (NPC.oldPos[i] - NPC.oldPos[i + 1]).ToRotation(), clawAuraTexture.Frame().Size() / 2, new Vector2(1, 1 - i / (float)NPC.oldPos.Length) * scale, SpriteEffects.None, 0f);
					}
				}
			}
			//flash and trail-ish thing during starstreams phase
			else if (construct.ai[0] == 6)
			{
				if (construct.ai[1] >= 155 && construct.ai[1] < 180)
				{
					Texture2D clawAuraTexture = Textures.Glow58.Value;
					float scale = 2.5f * (180 - construct.ai[1]) / 25f;

					Main.spriteBatch.Draw(clawAuraTexture, NPC.Center - screenPos, clawAuraTexture.Frame(), new Color(255, 239, 186), 0f, clawAuraTexture.Frame().Size() / 2, scale, SpriteEffects.None, 0f);
				}
				else if (construct.ai[1] >= 180 && construct.ai[1] < 300)
				{
					Texture2D clawAuraTexture = Textures.Glow58.Value;
					float scale = 1f;
					if (construct.ai[1] < 210)
					{
						scale = (construct.ai[1] - 180) / 30f;
					}
					else if (construct.ai[1] >= 270)
					{
						scale = (300 - construct.ai[1]) / 30f;
					}

					for (int i = 0; i < NPC.oldPos.Length - 1; i++)
					{
						float offsetAngle = Utils.AngleLerp(NPC.rotation, NPC.oldRot[i], 3f);
						float angle = (new Vector2(-i * 12f * scale, 0).RotatedBy(offsetAngle) - new Vector2(-(i + 1) * 12f * scale, 0).RotatedBy(Utils.AngleLerp(NPC.rotation, NPC.oldRot[i + 1], 3f))).ToRotation();
						Main.spriteBatch.Draw(clawAuraTexture, NPC.oldPos[i] - NPC.position + new Vector2(-i * 12f * scale, 0).RotatedBy(offsetAngle) + NPC.Center - screenPos, clawAuraTexture.Frame(), new Color(255, 239, 186), angle, clawAuraTexture.Frame().Size() / 2, new Vector2(1, 1 - i / (float)NPC.oldPos.Length) * scale, SpriteEffects.None, 0f);
					}
				}
			}

			//chain
			if (construct.ai[0] == 3 && construct.ai[1] < 600)
			{
				//bezier curve thing for the stomping chains
				float interpolationAmount = 1f;
				if (construct.ai[1] < 60)
				{
					interpolationAmount = Math.Max(0, (construct.ai[1] - 30) / 30f);
				}
				else if (construct.ai[1] >= 540)
				{
					interpolationAmount = (600 - construct.ai[1]) / 60f;
				}

				Vector2[] bezierPoints = { construct.Center, new Vector2(NPC.Center.X, construct.Center.Y - 100) * interpolationAmount + (construct.Center + NPC.Center) / 2 * (1 - interpolationAmount), NPC.Center };
				float bezierProgress = 0;
				float bezierIncrement = 18;

				Texture2D chainTexture = StarConstruct.ChainTexture.Value;
				Vector2 textureCenter = new Vector2(11, 10);

				while (bezierProgress < 1)
				{
					//draw stuff
					Vector2 oldPos = ModUtils.BezierCurve(bezierPoints, bezierProgress);

					//increment progress
					while ((oldPos - ModUtils.BezierCurve(bezierPoints, bezierProgress)).Length() < bezierIncrement)
					{
						bezierProgress += 0.1f / ModUtils.BezierCurveDerivative(bezierPoints, bezierProgress).Length();
					}

					Vector2 newPos = ModUtils.BezierCurve(bezierPoints, bezierProgress);
					float rotation = (newPos - oldPos).ToRotation();

					Vector2 drawingPos = (oldPos + newPos) / 2;

					Main.spriteBatch.Draw(chainTexture, drawingPos - screenPos, new Rectangle(0, 0, 22, 20), drawColor, rotation, textureCenter, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipVertically, 0f);
				}
			}
			else if (construct.ai[0] == 1 && construct.ai[1] >= 30 && construct.ai[1] < 390)
			{
				//bezier curve thing for the clawing chains
				float interpolationAmount = 1f;
				if (construct.ai[1] < 60)
				{
					interpolationAmount = (construct.ai[1] - 30) / 30f;
				}
				else if (construct.ai[1] >= 360)
				{
					interpolationAmount = (390 - construct.ai[1]) / 30f;
				}

				Player player = Main.player[construct.target];

				Vector2[] bezierPoints = { construct.Center, Vector2.Lerp((construct.Center + NPC.Center) / 2f, NPC.Center * 2 - player.Center, interpolationAmount), NPC.Center };
				float bezierProgress = 0;
				float bezierIncrement = 18;

				Texture2D chainTexture = StarConstruct.ChainTexture.Value;
				Vector2 textureCenter = new Vector2(11, 10);

				while (bezierProgress < 1)
				{
					//draw stuff
					Vector2 oldPos = ModUtils.BezierCurve(bezierPoints, bezierProgress);

					//increment progress
					while ((oldPos - ModUtils.BezierCurve(bezierPoints, bezierProgress)).Length() < bezierIncrement)
					{
						bezierProgress += 0.1f / ModUtils.BezierCurveDerivative(bezierPoints, bezierProgress).Length();
					}

					Vector2 newPos = ModUtils.BezierCurve(bezierPoints, bezierProgress);
					float rotation = (newPos - oldPos).ToRotation();

					Vector2 drawingPos = (oldPos + newPos) / 2;

					Main.spriteBatch.Draw(chainTexture, drawingPos - screenPos, new Rectangle(0, 0, 22, 20), drawColor, rotation, textureCenter, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipVertically, 0f);
				}
			}
			else
			{
				//just draw like a normal chain, but using the bezier curve thing because otherwise we get these weird jumps for some reason
				Vector2[] bezierPoints = { construct.Center, NPC.Center };
				float bezierProgress = 0;
				float bezierIncrement = 18;

				Texture2D chainTexture = StarConstruct.ChainTexture.Value;
				Vector2 textureCenter = new Vector2(11, 10);

				while (bezierProgress < 1)
				{
					//draw stuff
					Vector2 oldPos = ModUtils.BezierCurve(bezierPoints, bezierProgress);

					//increment progress
					while ((oldPos - ModUtils.BezierCurve(bezierPoints, bezierProgress)).Length() < bezierIncrement)
					{
						bezierProgress += 0.1f / ModUtils.BezierCurveDerivative(bezierPoints, bezierProgress).Length();
					}

					Vector2 newPos = ModUtils.BezierCurve(bezierPoints, bezierProgress);
					float rotation = (newPos - oldPos).ToRotation();

					Vector2 drawingPos = (oldPos + newPos) / 2;

					Main.spriteBatch.Draw(chainTexture, drawingPos - screenPos, new Rectangle(0, 0, 22, 20), drawColor, rotation, textureCenter, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipVertically, 0f);
				}
			}


			Texture2D texture = TextureAssets.Npc[NPC.type].Value;
			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height / Main.npcFrameCount[NPC.type] * 0.5f);
			Vector2 drawPos = NPC.Center - screenPos;
			if (NPC.spriteDirection == -1)
			{
				Main.spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
			}
			else
			{
				Main.spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.FlipHorizontally, 0f);
			}


			//trail-like thing for phase transition
			if (construct.ai[0] == -1 && construct.ai[1] >= 150 && construct.ai[1] < 210)
			{
				Texture2D clawAuraTexture = Textures.Glow58.Value;
				float scale = (construct.ai[1] - 150) * (210 - construct.ai[1]) / (30f * 30f);

				Color glowColor = new Color(Vector3.Lerp(new Color(47, 255, 255).ToVector3(), new Color(255, 239, 186).ToVector3(), (construct.ai[1] - 150) / 50f));

				for (int i = 0; i < NPC.oldPos.Length - 1; i++)
				{
					float offsetAngle = Utils.AngleLerp(NPC.rotation, NPC.oldRot[i], 0.75f);
					float angle = (new Vector2(-i * 12f * scale, 0).RotatedBy(offsetAngle) - new Vector2(-(i + 1) * 12f * scale, 0).RotatedBy(Utils.AngleLerp(NPC.rotation, NPC.oldRot[i + 1], 0.75f))).ToRotation();
					Main.spriteBatch.Draw(clawAuraTexture, new Vector2(-i * 12f * scale, 0).RotatedBy(offsetAngle) + NPC.Center - screenPos, clawAuraTexture.Frame(), glowColor, angle, clawAuraTexture.Frame().Size() / 2, new Vector2(1, 1 - i / (float)NPC.oldPos.Length) * scale, SpriteEffects.None, 0f);
				}
			}

			return false;
		}

		public override bool CheckActive()
		{
			return !Main.npc[(int)NPC.ai[0]].active;
		}

		public override bool CheckDead()
		{
			//spawn gores
			Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity + new Vector2(6, 0).RotatedByRandom(Math.PI * 2), Mod.Find<ModGore>("StarConstructClawGore").Type);
			return true;
		}

		public override bool PreKill()
		{
			return false;
		}
	}

	[AutoloadBossHead]
	internal class DormantConstruct : ModNPC
	{
		//textures for cacheing
		public static Asset<Texture2D> LegTexture;

		public override void Load()
		{
			LegTexture = Mod.GetAsset<Texture2D>("NPCs/StarConstruct/DormantConstructLeg");
		}

		public override void Unload()
		{
			LegTexture = null;
		}

		public override void SetStaticDefaults()
		{
			NPCID.Sets.CantTakeLunchMoney[Type] = true;

			var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
			{
				//don't show up in bestiary
				Hide = true
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

			Main.npcFrameCount[NPC.type] = 11;

			NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
			{
				ImmuneToAllBuffsThatAreNotWhips = true
			};
			NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);

			PolaritiesNPC.npcTypeCap[Type] = 1;
		}

		public override void SetDefaults()
		{
			NPC.aiStyle = -1;
			NPC.width = 96;
			NPC.height = 96;
			NPC.defense = 10;
			NPC.chaseable = false;
			NPC.lifeMax = 200;
			NPC.knockBackResist = 0f;
			NPC.npcSlots = 1f;
			NPC.rarity = 6;
			NPC.lavaImmune = true;
			NPC.noGravity = false;
			NPC.noTileCollide = false;
			NPC.HitSound = SoundID.NPCHit4;
		}

		private static int FRAMESPEED = 7;

		public override void AI()
		{
			Lighting.AddLight(NPC.Center, 2f, 2f, 2f);

			if (NPC.ai[0] == 1)
			{
				NPC.frameCounter++;
				if (NPC.frameCounter >= 11 * FRAMESPEED)
				{
					NPC.active = false;
					NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Hitbox.Bottom, NPCType<StarConstruct>());
				}
			}

			if (NPC.velocity.Y != 0)
			{
				NPC.rotation += 0.2f;

				if (NPC.soundDelay == 0)
				{
					NPC.soundDelay = 20 + Main.rand.Next(40);
					SoundEngine.PlaySound(SoundID.Item9, NPC.Center);
				}

				//falling construct dusts
				Vector2 value34 = new Vector2((float)Main.screenWidth, (float)Main.screenHeight);
				for (int i = 0; i < 4; i++)
				{
					if (NPC.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + value34 / 2f, value34 + new Vector2(400f))) && Main.rand.Next(6) == 0)
					{
						int[] array6 = new int[4] { 16, 17, 17, 17 };
						int num855 = Utils.SelectRandom(Main.rand, array6);
						if (Main.tenthAnniversaryWorld)
						{
							int[] array7 = new int[4] { 16, 16, 16, 17 };
							num855 = Utils.SelectRandom(Main.rand, array7);
						}
						Gore.NewGore(NPC.GetSource_FromAI(), NPC.Center - new Vector2(11) + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)) * (NPC.Size - new Vector2(22)) / 2f, NPC.velocity * 0.2f, num855);
					}
					if (Main.rand.Next(20) == 0 || (Main.tenthAnniversaryWorld && Main.rand.Next(15) == 0))
					{
						Dust.NewDust(NPC.position, NPC.width, NPC.height, 58, NPC.velocity.X * 0.5f, NPC.velocity.Y * 0.5f, 150, default(Color), 1.2f);
					}
				}
			}
			else if (NPC.ai[1] == 1)
			{
				NPC.rotation = 0;

				NPC.ai[1] = 2;

				//collision dusts
				SoundEngine.PlaySound(SoundID.Item10, NPC.Center);
				Color newColor7 = Color.CornflowerBlue;
				if (Main.tenthAnniversaryWorld)
				{
					newColor7 = Color.HotPink;
					newColor7.A = (byte)(newColor7.A / 2);
				}
				float scaleMult = 8;
				Vector2 useVelocity = NPC.velocity.SafeNormalize(-Vector2.UnitY) * 16;
				for (int num573 = 0; num573 < 7 * scaleMult; num573++)
				{
					Dust.NewDust(NPC.position, NPC.width, NPC.height, 58, useVelocity.X * 0.1f, useVelocity.Y * 0.1f, 150, default(Color), 0.8f);
				}
				for (float num574 = 0f; num574 < 1f * scaleMult; num574 += 0.125f)
				{
					Vector2 center25 = NPC.Center;
					Vector2 unitY11 = Vector2.UnitY;
					double radians36 = num574 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f;
					Vector2 center2 = default(Vector2);
					Dust.NewDustPerfect(center25, 278, unitY11.RotatedBy(radians36, center2) * (4f + Main.rand.NextFloat() * 4f), 150, newColor7).noGravity = true;
				}
				for (float num575 = 0f; num575 < 1f * scaleMult; num575 += 0.25f)
				{
					Vector2 center26 = NPC.Center;
					Vector2 unitY12 = Vector2.UnitY;
					double radians37 = num575 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f;
					Vector2 center2 = default(Vector2);
					Dust.NewDustPerfect(center26, 278, unitY12.RotatedBy(radians37, center2) * (2f + Main.rand.NextFloat() * 3f), 150, Color.Gold).noGravity = true;
				}
				Vector2 value15 = new Vector2((float)Main.screenWidth, (float)Main.screenHeight);
				if (NPC.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + value15 / 2f, value15 + new Vector2(400f))))
				{
					for (int num576 = 0; num576 < 7 * scaleMult; num576++)
					{
						Vector2 val29 = NPC.Center - new Vector2(11) + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)) * (NPC.Size - new Vector2(22)) / 2f;
						Vector2 val30 = Main.rand.NextVector2CircularEdge(0.5f, 0.5f) * useVelocity.Length();

						int[] array18 = new int[8] { 16, 17, 17, 17, 17, 17, 17, 17 };
						Gore.NewGore(NPC.GetSource_FromAI(), val29, val30, Utils.SelectRandom(Main.rand, array18));
					}
				}
			}

			if (NPC.ai[1] == 0)
			{
				NPC.ai[1] = 1;
			}

			if (Main.dayTime)
			{
				//spawn dusts and despawn
				for (int j = 0; j < 10; j++)
				{
					Dust.NewDust(NPC.position, NPC.width, NPC.height, 15, NPC.velocity.X, NPC.velocity.Y, 150, default(Color), 1.2f);
				}
				for (int k = 0; k < 3; k++)
				{
					Gore.NewGore(NPC.GetSource_Death(), NPC.Center - new Vector2(11) + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)) * (NPC.Size - new Vector2(22)) / 2f, new Vector2(NPC.velocity.X, NPC.velocity.Y), Main.rand.Next(16, 18));
				}

				NPC.active = false;
			}
		}

		private static float legAnimPeriodMult = 10f;
		private float legAnimPhase = MathHelper.PiOver2 * legAnimPeriodMult;

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			drawColor = NPC.GetNPCColorTintedByBuffs(Color.White);

			if (NPC.velocity.Y != 0)
			{
				//adapted from fallen star drawcode
				float scaleMultiplier = 3f;

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

			Texture2D texture = LegTexture.Value;
			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height / Main.npcFrameCount[NPC.type] * 0.5f);
			Vector2 drawPos = NPC.Center - screenPos;
			for (int i = 0; i < 4; i++)
			{
				float flattening = 8f;
				float heightFactor = 8f;

				float xOffset = 4f / 3f * legAnimPeriodMult * (float)Math.Sin(legAnimPhase / legAnimPeriodMult + i * MathHelper.Pi - 1 / 4f * Math.Sin(legAnimPhase / legAnimPeriodMult + i * MathHelper.Pi));
				float yOffset = legAnimPeriodMult * flattening * heightFactor * (float)Math.Exp(flattening / (Math.Cos(legAnimPhase / legAnimPeriodMult + i * MathHelper.Pi) - 1));

				Vector2 drawLegOffset = new Vector2(xOffset, yOffset);

				spriteBatch.Draw(texture, drawPos - drawLegOffset, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, (i == 0 || i == 3) ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
			}

			texture = TextureAssets.Npc[Type].Value;
			drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height / Main.npcFrameCount[NPC.type] * 0.5f);
			drawPos = NPC.Center - screenPos;
			spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
			return false;
		}

		public override bool? CanBeHitByProjectile(Projectile projectile)
		{
			if (projectile.type == ProjectileID.FallingStar && projectile.damage > 500)
			{
				return false;
			}
			return null;
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frame.Y = frameHeight * ((int)NPC.frameCounter / FRAMESPEED);
		}

		public override bool CheckDead()
		{
			if (NPC.ai[0] != 1)
			{
				NPC.life = NPC.lifeMax;
				NPC.dontTakeDamage = true;
				NPC.ai[0] = 1;
			}
			return false;
		}

		public override bool PreKill()
		{
			return false;
		}

		static int spawnX;
		static int spawnY;
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			if ((!PolaritiesSystem.downedStarConstruct || Main.rand.NextBool(2)) && Main.invasionType == 0 && !Main.pumpkinMoon && !Main.snowMoon)
			{
				if ((spawnInfo.Player.ZoneOverworldHeight || spawnInfo.Player.ZoneSkyHeight) && spawnInfo.Player.statLifeMax2 >= 300 && !spawnInfo.Player.ShoppingZone_AnyBiome)
				{
					int x = (int)spawnInfo.Player.Center.X + Main.rand.Next(-1000, 1000);
					int y = (int)spawnInfo.Player.position.Y - 1000;
					bool canSpawn = true;
					for (int i = Math.Max(0, x / 16 - 10); i <= Math.Min(Main.maxTilesX, x / 16 + 10); i++)
					{
						for (int j = Math.Max(0, y / 16 - 10); j <= Math.Min(Main.maxTilesY, y / 16 + 10); j++)
						{
							if (Main.wallHouse[Main.tile[i, j].WallType] || (Main.tile[i, j].HasUnactuatedTile && Main.tileSolid[Main.tile[i, j].TileType]))
							{
								canSpawn = false;
								break;
							}
						}
						if (!canSpawn) return 0f;
					}
					if (canSpawn)
					{
						spawnX = x;
						spawnY = y;
						//TODO: Balance this rate
						return Terraria.ModLoader.Utilities.SpawnCondition.OverworldNightMonster.Chance * 0.005f;
					}
				}
			}

			return 0f;
		}

        public override int SpawnNPC(int tileX, int tileY)
        {
			return NPC.NewNPC(new EntitySource_SpawnNPC(), spawnX, spawnY, Type);
        }
    }

	public class UnfriendlyFallenStar : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
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
			if (Projectile.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + value34 / 2f, value34 + new Vector2(400f))) && Main.rand.Next(6) == 0)
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
			if (Main.rand.Next(20) == 0 || (Main.tenthAnniversaryWorld && Main.rand.Next(15) == 0))
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 58, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 150, default(Color), 1.2f);
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
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 58, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 150, default(Color), 0.8f);
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

	public class StarConstructImpact : ModProjectile
	{
		public override string Texture => "Polarities/Textures/Glow256";

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 150;
			Projectile.height = 150;

			Projectile.timeLeft = 20;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			Vector2 oldCenter = Projectile.Center;

			Projectile.scale = (float)Math.Sqrt(Projectile.timeLeft / 20f);
			Projectile.width = (int)(150 * Projectile.scale);
			Projectile.height = (int)(150 * Projectile.scale);

			Projectile.Center = oldCenter;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float nearestX = Math.Max(targetHitbox.X, Math.Min(Projectile.Center.X, targetHitbox.X + targetHitbox.Size().X));
			float nearestY = Math.Max(targetHitbox.Y, Math.Min(Projectile.Center.Y, targetHitbox.Y + targetHitbox.Size().Y));
			return (new Vector2(Projectile.Center.X - nearestX, Projectile.Center.Y - nearestY)).Length() < Projectile.width / 2;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame();

			Color color = new Color(255, 239, 186);

			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, frame, color, Projectile.rotation, frame.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);

			return false;
		}

		public override bool ShouldUpdatePosition() => false;
	}

	public class StarConstructDash : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			/*Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 256, 256, false, SurfaceFormat.Color);
			System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
			for (int j = 0; j < texture.Height; j++)
			{
				for (int i = 0; i < texture.Width; i++)
				{
					float x = i / (float)texture.Width;
					float y = (2 * j / (float)(texture.Width - 1) - 1);

					float index = y * y / ((1 - x) * (1 - x));

					int r = 255;
					int g = 255;
					int b = 255;
					int alpha = index >= 1 ? 0 : (int)(255 * Math.Exp(1 + 1 / (index - 1)));

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "StarConstructDash.png", FileMode.Create), texture.Width, texture.Height);*/
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 150;
			Projectile.height = 150;

			Projectile.timeLeft = 20;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		private Vector2 startPoint { get => new Vector2(Projectile.ai[0], Projectile.ai[1]); }

		public override void AI()
		{
			Vector2 oldCenter = Projectile.Center;

			Projectile.scale = (float)Math.Sqrt(Projectile.timeLeft / 20f);
			Projectile.width = (int)(150 * Projectile.scale);
			Projectile.height = (int)(150 * Projectile.scale);

			Projectile.Center = oldCenter;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float nearestX = Math.Max(targetHitbox.X, Math.Min(Projectile.Center.X, targetHitbox.X + targetHitbox.Size().X));
			float nearestY = Math.Max(targetHitbox.Y, Math.Min(Projectile.Center.Y, targetHitbox.Y + targetHitbox.Size().Y));

			Vector2 offset = Projectile.width / 2 * (startPoint - Projectile.Center).RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero);
			return CustomCollision.CheckAABBvTriangle(targetHitbox, startPoint, Projectile.Center + offset, Projectile.Center - offset) || (new Vector2(Projectile.Center.X - nearestX, Projectile.Center.Y - nearestY)).Length() < Projectile.width / 2;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame();
			Vector2 origin = new Vector2(0, frame.Height / 2);

			Color color = new Color(255, 239, 186);

			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, frame, color, (startPoint - Projectile.Center).ToRotation(), origin, new Vector2((startPoint - Projectile.Center).Length() / frame.Width, Projectile.scale), SpriteEffects.None, 0f);

			Texture2D texture2 = Textures.Glow256.Value;
			Rectangle frame2 = texture.Frame(2, 1, 1, 0);

			Main.spriteBatch.Draw(texture2, Projectile.Center - Main.screenPosition, frame2, color, (startPoint - Projectile.Center).ToRotation() + MathHelper.Pi, origin, Projectile.scale, SpriteEffects.None, 0f);

			return false;
		}

		public override bool ShouldUpdatePosition() => false;
	}
}