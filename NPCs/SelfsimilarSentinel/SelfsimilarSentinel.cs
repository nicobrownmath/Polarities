using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
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
using Polarities.Effects;
using Terraria.Utilities;
using Polarities.Items.Placeable.Blocks.Fractal;
using Polarities.NPCs.Enemies.HallowInvasion;
using Terraria.DataStructures;

namespace Polarities.NPCs.SelfsimilarSentinel
{
	public readonly struct ComplexNumber : IEquatable<ComplexNumber>
	{
		public readonly float re;
		public readonly float im;

		public static readonly ComplexNumber Zero = new ComplexNumber(0, 0);
		public static readonly ComplexNumber One = new ComplexNumber(1, 0);
		public static readonly ComplexNumber I = new ComplexNumber(0, 1);

		public ComplexNumber(float _re, float _im)
		{
			re = _re;
			im = _im;
		}

		bool IEquatable<ComplexNumber>.Equals(ComplexNumber other)
		{
			return re == other.re && im == other.im;
		}

		public static ComplexNumber operator +(ComplexNumber a) => a;
		public static ComplexNumber operator -(ComplexNumber a) => new ComplexNumber(-a.re, -a.im);

		public static ComplexNumber operator +(ComplexNumber a, ComplexNumber b) => new ComplexNumber(a.re + b.re, a.im + b.im);

		public static ComplexNumber operator -(ComplexNumber a, ComplexNumber b) => a + (-b);

		public static ComplexNumber operator *(ComplexNumber a, ComplexNumber b) => new ComplexNumber(a.re * b.re - a.im * b.im, a.re * b.im + a.im * b.re);

		public static Vector2 operator *(ComplexNumber a, Vector2 b) => new Vector2(a.re * b.X - a.im * b.Y, a.re * b.Y + a.im * b.X);

		public static ComplexNumber operator /(ComplexNumber a, ComplexNumber b)
		{
			if (b.re == 0 && b.im == 0)
			{
				throw new DivideByZeroException();
			}

			return a * new ComplexNumber(b.re, -b.im) / b.NormSquared();
		}

		public static ComplexNumber operator /(ComplexNumber a, float b)
		{
			if (b == 0)
			{
				throw new DivideByZeroException();
			}

			return new ComplexNumber(a.re / b, a.im / b);
		}

		public static ComplexNumber Pow(ComplexNumber a, float pow)
        {
			float dist = (float)Math.Pow(a.re * a.re + a.im * a.im, pow / 2);
			float theta = pow * (float)Math.Atan2(a.im, a.re);
			float R = dist * (float)Math.Cos(theta);
			float I = dist * (float)Math.Sin(theta);

			return new ComplexNumber(R, I);
        }

		public float NormSquared()
        {
			return re * re + im * im;
        }

		public float Norm()
		{
			return (float)Math.Sqrt(re * re + im * im);
		}

		public float Angle()
        {
			return (float)Math.Atan2(im, re);
        }

		public ComplexNumber Conjugate()
        {
			return new ComplexNumber(re, -im);
        }

		public Vector2 ToVector2()
        {
			return new Vector2(re, im);
        }
	}
	public readonly struct HyperbolicTransform : IEquatable<HyperbolicTransform>
	{
		public readonly ComplexNumber u;
		public readonly ComplexNumber v;

		public static readonly HyperbolicTransform Identity = new HyperbolicTransform(ComplexNumber.One, ComplexNumber.Zero);

		bool IEquatable<HyperbolicTransform>.Equals(HyperbolicTransform other)
		{
			return u.Equals(other.u) && v.Equals(other.v);
		}

		public HyperbolicTransform(float _ux, float _uy, float _vx, float _vy)
		{
			u = new ComplexNumber(_ux, _uy);
			v = new ComplexNumber(_vx, _vy);
			float det = (float)Math.Sqrt(u.NormSquared() - v.NormSquared());
			u /= det;
			v /= det;
		}

		public HyperbolicTransform(ComplexNumber _u, ComplexNumber _v)
        {
			u = _u;
			v = _v;
			float det = (float)Math.Sqrt(u.NormSquared() - v.NormSquared());
			u /= det;
			v /= det;
		}

		public static HyperbolicTransform operator *(HyperbolicTransform a, HyperbolicTransform b)
        {
			return new HyperbolicTransform(b.u * a.u + b.v * a.v.Conjugate(), b.u * a.v + b.v * a.u.Conjugate());
        }

		public static HyperbolicTransform operator /(HyperbolicTransform a, HyperbolicTransform b)
        {
			return b.Inverse() * a;
        }

		public static ComplexNumber operator *(HyperbolicTransform a, ComplexNumber b)
		{
			return (a.u * b + a.v.Conjugate()) / (a.v * b + a.u.Conjugate());
		}

		public static HyperbolicTransform Pow(HyperbolicTransform a, float pow)
        {
			float valSquared = -a.u.im * a.u.im + a.v.re * a.v.re + a.v.im * a.v.im;
			if (valSquared == 0)
			{
				return new HyperbolicTransform(new ComplexNumber(a.u.re, a.u.im * pow), a.v * new ComplexNumber(pow, 0));
			}
			else
			{
				ComplexNumber val = valSquared >= 0 ? new ComplexNumber((float)Math.Sqrt(valSquared), 0) : new ComplexNumber(0, (float)Math.Sqrt(-valSquared));
				ComplexNumber pow1 = ComplexNumber.Pow(new ComplexNumber(a.u.re, 0) + val, pow) / (new ComplexNumber(2, 0) * val);
				ComplexNumber pow2 = ComplexNumber.Pow(new ComplexNumber(a.u.re, 0) - val, pow) / (new ComplexNumber(2, 0) * val);

				ComplexNumber u = val * (pow1 + pow2) + new ComplexNumber(0, a.u.im) * (pow1 - pow2);
				ComplexNumber v = new ComplexNumber(a.v.re, 0) * (pow1 - pow2) + new ComplexNumber(0, a.v.im) * (pow1 - pow2);

				return new HyperbolicTransform(u, v);
			}
		}

		public Vector2 GetPosition()
		{
			return (v / u).Conjugate().ToVector2();
		}

		public float GetRotation()
		{
			return u.Angle() * 2;
		}

		public float GetScale()
        {
			//scale capped because floating points are weird
			return Math.Max(0, Math.Min(1, 1 / u.NormSquared()));
        }

		public float AngleTo(HyperbolicTransform target)
        {
			return (target / this).GetPosition().ToRotation();
		}

		public float DistanceTo(HyperbolicTransform target)
		{
			float x = (target / this).GetPosition().Length();
			return (float)(Math.Log((1 + x) / (1 - x))) / 2;
		}

		public float RayDistanceTo(HyperbolicTransform target)
        {
			if (Math.Abs(AngleTo(target)) >= MathHelper.PiOver2) return DistanceTo(target);
			return LineDistanceTo(target);
        }

		public float LineDistanceTo(HyperbolicTransform target)
        {
			HyperbolicTransform basePoint = this / target;
			HyperbolicTransform secondPoint = basePoint.Inverse() * Rotation(-basePoint.AngleTo(Identity) * 2) * basePoint;
			return Identity.DistanceTo(secondPoint) / 2;
		}

		public HyperbolicTransform Inverse()
        {
			return new HyperbolicTransform(u.Conjugate(), -v);
        }

		public static HyperbolicTransform Rotation(float angle)
		{
			return new HyperbolicTransform(
					(float)Math.Cos(angle / 2),
					(float)Math.Sin(angle / 2),
					0,
					0
				);
		}

		public static HyperbolicTransform Translation(float distance)
		{
			return new HyperbolicTransform(
					(float)Math.Cosh(distance),
					0,
					(float)Math.Sinh(distance),
					0
				);
		}

		public static HyperbolicTransform Translation(float distance, float angle)
		{
			return Rotation(angle) * Translation(distance) * Rotation(-angle);
		}

		public static HyperbolicTransform LimitRotation(float amount)
        {
			return new HyperbolicTransform(
				1,
				amount,
				0,
				amount
				);
		}

		public static HyperbolicTransform LimitRotation(float amount, float angle)
		{
			return Rotation(angle) * LimitRotation(amount) * Rotation(-angle);
		}

		public static HyperbolicTransform FromVelocity(Vector2 velocity)
        {
			if (velocity == Vector2.Zero) return Identity;
			return Rotation(velocity.ToRotation()) * Translation(velocity.Length()) * Rotation(-velocity.ToRotation());
        }

		public static HyperbolicTransform FromPosition(Vector2 position)
		{
			if (position == Vector2.Zero) return Identity;
			float length = (float)Math.Log((1 + Math.Min(position.Length(), 0.999f)) / (1 - Math.Min(position.Length(), 0.999f)));
			return Rotation(position.ToRotation()) * Translation(length / 2) * Rotation(-position.ToRotation());
        }
	}

	[AutoloadBossHead]
	public class SelfsimilarSentinel : ModNPC
	{
		public const float ARENA_RADIUS = 1000;

		public const int projectileDamage = 20;

		//max arm length
		public static float maxArmLength = (float)Math.Log(3) / 2; //This is acosh(1/cos(π/6))

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Selfsimilar Sentinel");
			Main.npcFrameCount[NPC.type] = 5;
			NPCID.Sets.MustAlwaysDraw[NPC.type] = true;
		}

		public override void SetDefaults()
		{
			NPC.aiStyle = -1;
			NPC.width = 64;
			NPC.height = 64;
			NPC.defense = 30;

			NPC.lifeMax = Main.expertMode ? 64000 : 48000;
			NPC.knockBackResist = 0f;
			NPC.value = Item.buyPrice(0, 15, 0, 0);
			NPC.npcSlots = 15f;
			NPC.boss = true;
			NPC.lavaImmune = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.HitSound = SoundID.Tink;

			NPC.buffImmune[BuffID.Confused] = true;

			Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/SentinelP1");
		}

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
		{
			NPC.lifeMax = (int)(64000 * bossLifeScale);
		}

		public static int secondStageHeadSlot;

		public override void BossHeadSlot(ref int index)
		{
			if (NPC.ai[0] >= 5 || NPC.ai[0] == -2)
			{
				index = secondStageHeadSlot;
			}
		}

		public override void BossHeadRotation(ref float rotation)
		{
			rotation = transform.GetRotation() - MathHelper.PiOver2;
		}

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
			scale = NPC.scale;
            return null;
        }

		//spawning
		public static NPC SpawnSentinel(Vector2 position)
        {
			if (NPC.AnyNPCs(NPCType<SelfsimilarSentinel>())) return null;

			if (PolaritiesSystem.sentinelCaves.Count == 0)
			{
				return null;
			}

			Vector2 worldPosition = GetNearestArenaPosition(position);

			if ((worldPosition - position).Length() > ARENA_RADIUS)
			{
				return null;
			}
			else
            {
				NPC sentinel = Main.npc[NPC.NewNPC(new EntitySource_BossSpawn(Main.player[Player.FindClosest(position, 2, 2)]), (int)worldPosition.X, (int)worldPosition.Y, NPCType<SelfsimilarSentinel>(), ai0: -1)];
				(sentinel.ModNPC as SelfsimilarSentinel).worldPosition = worldPosition;

				for (int i = (int)(worldPosition.X - ARENA_RADIUS) / 16; i <= (int)(worldPosition.X + ARENA_RADIUS) / 16; i++)
				{
					for (int j = (int)(worldPosition.Y - ARENA_RADIUS) / 16; j <= (int)(worldPosition.Y + ARENA_RADIUS) / 16; j++)
					{
						if ((new Vector2(i * 16 + 8, j * 16 + 8) - worldPosition).Length() <= ARENA_RADIUS)
                        {
							if (Main.tile[i, j].HasTile && Main.tile[i,j].TileType == TileType<SelfsimilarOreTile>())
                            {
								Main.tile[i, j].ClearTile();
								WorldGen.SquareTileFrame(i, j);

								(sentinel.ModNPC as SelfsimilarSentinel).totalOre++;
							}
                        }
					}
				}

				return sentinel;
			}
        }

		public static Vector2 GetNearestArenaPosition(Vector2 position)
		{
			if (PolaritiesSystem.sentinelCaves.Count == 0)
			{
				return Vector2.Zero;
			}

			Vector2 worldPosition = PolaritiesSystem.sentinelCaves[0];

			foreach (Vector2 cavePosition in PolaritiesSystem.sentinelCaves)
			{
				if ((cavePosition - position).Length() < (worldPosition - position).Length())
					worldPosition = cavePosition;
			}
			return worldPosition;
		}


        public Vector2 worldPosition;
		public HyperbolicTransform transform = HyperbolicTransform.Identity;
		public int totalOre = 0;

        public override bool PreAI()
        {
			try
            {
				AI();
            }
			catch (Exception e)
            {
				Mod.Logger.Debug(e);
            }
			return false;
        }

        public override void AI()
		{
			//Initialization
			if (NPC.localAI[0] == 0)
            {
				NPC.localAI[0] = 1;

				NPC.frame.Width = 78;

				if (worldPosition == Vector2.Zero) worldPosition = NPC.Center;
				//this has to be offset from the center ever so slightly to prevent ray collision bugs
				transform = HyperbolicTransform.Rotation(Main.rand.NextFloat(MathHelper.TwoPi)) * HyperbolicTransform.Translation(0.1f / ARENA_RADIUS);

				//this is initialized here to prevent any weird spinning
				//in practice it doesn't matter unless spawned with cheats
				lookingTransform = HyperbolicTransform.Translation(0.01f);

				//start in spawn animation
				NPC.ai[0] = -1;

				InitializeAIStates();
			}

			//ranges from 0 to 1
			Player player = Main.player[NPC.target];
			float amountThroughFirstPhase = Math.Min(1, Math.Max(0, (1 - NPC.life / (float)NPC.lifeMax) * 2));

			NPC.dontTakeDamage = false;

			//if all players dead or out of bounds, despawn
			if (!player.active || player.dead || ((player.Center - worldPosition).Length() > ARENA_RADIUS && amountThroughFirstPhase < 1))
			{
				NPC.TargetClosest(false);
				player = Main.player[NPC.target];
				if (player.dead || (player.Center - worldPosition).Length() > ARENA_RADIUS)
				{
					//despawn
					NPC.alpha++;
					if (NPC.alpha >= 255)
                    {
						NPC.active = false;
                    }
				}
			}
			else
			{
				//target is in bounds
				NPC.alpha -= 3;
				if (NPC.alpha < 0) NPC.alpha = 0;

				//prevent fractal antidote if not in spawn or death animation
				if (NPC.ai[0] >= 0)
					player.GetModPlayer<PolaritiesPlayer>().noFractalAntidote = true;
            }

			if (NPC.alpha > 0) NPC.dontTakeDamage = true;

			Vector2 targetPosition = (player.Center - worldPosition) / ARENA_RADIUS;
			if (targetPosition.Length() > 0.99f)
			{
				targetPosition.Normalize();
				targetPosition *= 0.99f;
			}
			HyperbolicTransform target = HyperbolicTransform.FromPosition(targetPosition);

			//default values for looking data: change nothing
			HyperbolicTransform goalLookingTransform = lookingTransform;
			float lookingTransformMovementMult = 0f;
			float goalLookingRotation = lookingRotation;
			float rotationLerp = 0f;

			//get current frame data
			int currentFrame = NPC.frame.X / NPC.frame.Width * 5 + NPC.frame.Y / NPC.frame.Height;
			int currentSecondaryFrame = secondaryFrame.X / secondaryFrame.Width * 5 + secondaryFrame.Y / secondaryFrame.Height;

			//count down teleport flash timer
			if (teleportFlashTime > 0)
			{
				teleportFlashTime--;
			}

			//don't do standard AI if we're in an animation
			if (NPC.ai[0] < 0)
			{
				switch(NPC.ai[0])
                {
					case -1:
						//spawn animation
						NPC.dontTakeDamage = true;

						if (NPC.ai[1] < 360 && NPC.ai[1] >= 120 && NPC.ai[1] % 20 == 0)
						{
							SoundEngine.PlaySound(SoundID.DD2_WitherBeastAuraPulse, NPC.Center);
							SoundEngine.PlaySound(SoundID.Item15.WithVolumeScale(0.5f), NPC.Center);
						}

						if (NPC.ai[1] == 360)
						{
							teleportFlashTime = maxTeleportFlashTime;
							Main.NewText("Selfsimilar Sentinel has awoken!", 171, 64, 255);

							//face player on starting
							//to do this we get a transform that rotates around the player
							lookingTransform = target * HyperbolicTransform.Rotation(0.01f) * target.Inverse();
							lookingRotation = MathHelper.PiOver2;

							//wave of wisps
                            for (int i = 0; i < 64; i++)
							{
								HyperbolicWisp.NewProjectile(NPC.GetSource_FromAI(), worldPosition, transform * HyperbolicTransform.Rotation(Main.rand.NextFloat(MathHelper.TwoPi)), new Vector2(Main.rand.NextFloat(8, 12), 0), 0, 0, extraUpdates: 0, ai0: Main.rand.NextFloat(-0.0025f, 0.0025f), ai1: 1, timeLeft: Main.rand.Next(60, 120)).localAI[1] = Main.rand.NextFloat(1000f);
							}
						}

						NPC.ai[1]++;
						if (NPC.ai[1] >= 370)
						{
							NPC.ai[1] = 0;
							GotoNextAIState();
						}
						break;
					case -2:
						//death animation
						NPC.dontTakeDamage = true;
						NPC.velocity = Vector2.Zero;

						arcChainTextureFrame.Y = 0;

						if (NPC.ai[1] == 0)
                        {
							ClearProjectiles();

							HyperbolicTransform oldTransform = transform;
							transform = GetClosestNode(HyperbolicTransform.Identity);
							if (oldTransform.DistanceTo(transform) > maxArmLength / 2)
								teleportFlashTime = maxTeleportFlashTime;

							List<HyperbolicTransform> segments = GetVisibleSegments();

							NPC.ai[2] = 0;
							foreach (HyperbolicTransform localTransform in segments)
							{
								if (localTransform.GetScale() > 0.1f)
									NPC.ai[2] += localTransform.GetScale();
							}

							//adjust totalOre to ensure we output the actual correct amount of ore at the end
							int trueTotalOre = totalOre;
							bool check = true;
							while (check)
                            {
								int outputOre = 0;
								foreach (HyperbolicTransform localTransform in segments)
								{
									if (localTransform.GetScale() > 0.1f)
										outputOre += (int)(totalOre * localTransform.GetScale() / NPC.ai[2]);
								}
								if (outputOre < trueTotalOre)
                                {
									totalOre++;
                                }
								else
                                {
									check = false;
                                }
							}

							Main.LocalPlayer.GetModPlayer<PolaritiesPlayer>().AddScreenShake(12, 60);

							SoundEngine.PlaySound(SoundID.Zombie93, NPC.Center);
						}

						if (DeathAnimationSegmentsLeft(NPC.ai[1]) != DeathAnimationSegmentsLeft(NPC.ai[1] + 1))
                        {
							NPC.ai[1]--;
							foreach (HyperbolicTransform localTransform in GetVisibleSegments(onlyEndpoints: true))
							{
								if (localTransform.GetScale() > 0.1f)
								{
									if (Main.netMode != NetmodeID.MultiplayerClient)
                                    {
										int itemCount = (int)(totalOre * localTransform.GetScale() / NPC.ai[2]);
										if (itemCount > 0)
											Item.NewItem(NPC.GetSource_Death(), localTransform.GetPosition() * ARENA_RADIUS + worldPosition, ItemType<SelfsimilarOre>(), itemCount);
									}
									if (Main.netMode != NetmodeID.Server)
                                    {
										for (int i = 0; i < 3; i++)
										{
											Vector2 goreVelocity = localTransform.GetScale() * new Vector2(8, 0).RotatedBy(i * MathHelper.TwoPi / 3f + localTransform.GetRotation());
											Vector2 gorePosition = localTransform.GetPosition() * ARENA_RADIUS + worldPosition + goreVelocity * 4;
											Gore.NewGorePerfect(NPC.GetSource_Death(), gorePosition, goreVelocity, GoreHelper.GoreType("SelfsimilarSentinelSpikeGore"), localTransform.GetScale());
										}

										for (int i = 0; i < 6; i++)
										{
											Vector2 goreVelocity = localTransform.GetScale() * new Vector2(Main.rand.Next(2, 4), 0).RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi));
											Vector2 gorePosition = localTransform.GetPosition() * ARENA_RADIUS + worldPosition + goreVelocity * 4;
											Gore.NewGorePerfect(NPC.GetSource_Death(), gorePosition, goreVelocity, GoreHelper.GoreType("SelfsimilarSentinelShrapnel"), localTransform.GetScale());
										}
									}
								}
							}
							NPC.ai[1]++;

							SoundEngine.PlaySound(SoundID.NPCDeath14.WithVolumeScale(NPC.ai[1] / 240f), NPC.Center);

							//screenshake pulse
							Main.LocalPlayer.GetModPlayer<PolaritiesPlayer>().AddScreenShake(12, 12);
						}

						//energy wisps
						for (int i = 0; i < 3; i++)
						{
							if (Main.rand.NextFloat() < Math.Pow(1 - NPC.ai[1] / 240f, 2))
							{
								HyperbolicWisp.NewProjectile(NPC.GetSource_FromAI(), worldPosition, transform * HyperbolicTransform.Rotation(Main.rand.NextFloat(MathHelper.TwoPi)), new Vector2(Main.rand.NextFloat(2, 6), 0), 0, 0, extraUpdates: 3, ai0: Main.rand.NextFloat(-0.0025f, 0.0025f), ai1: 3);
							}
						}

						NPC.ai[1]++;
						if (NPC.ai[1] >= 241)
						{
							NPC.life = 0;
							NPC.checkDead();
						}
						break;
                }
			}
			else
			{
				//create new hitbox projectile if none exists
				Projectile.NewProjectile(NPC.GetSource_FromAI(), worldPosition, Vector2.Zero, ProjectileType<SelfsimilarSentinelCollision>(), projectileDamage, 0f, Main.myPlayer, NPC.whoAmI);

				bool gotoNextAIState = false;

				//if below health threshold, go to the transition to phase 2 immediately
				if (amountThroughFirstPhase >= 1 && NPC.ai[0] < 5)
				{
					NPC.ai[1] = 0;
					NPC.ai[0] = 5;

					ClearProjectiles();

					Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/SentinelP2");
				}

				//if in phase 2, force player into arena
				if (amountThroughFirstPhase >= 1)
				{
					float radius = ARENA_RADIUS - player.width / 2;
					if (NPC.ai[0] == 5) radius = radius * (120 / (NPC.ai[1] + 1));
					if ((Main.LocalPlayer.Center - worldPosition).Length() > radius)
					{
						Vector2 normal = (Main.LocalPlayer.Center - worldPosition).SafeNormalize(Vector2.Zero);
						Vector2 relativeVelocity = Main.LocalPlayer.velocity;

						Main.LocalPlayer.Center = worldPosition + normal * radius;

						if (relativeVelocity.X * normal.X + relativeVelocity.Y * normal.Y > 0)
						{
							Main.LocalPlayer.velocity -= normal * (relativeVelocity.X * normal.X + relativeVelocity.Y * normal.Y);
							//if hitting it moving downwards, reset ground effects
							if (normal.ToRotation() > 0 && normal.ToRotation() < MathHelper.PiOver2 && relativeVelocity.ToRotation() > 0 && relativeVelocity.ToRotation() < MathHelper.PiOver2)
							{
								if (Main.LocalPlayer.carpet)
								{
									Main.LocalPlayer.canCarpet = true;
									Main.LocalPlayer.carpetTime = 0;
									Main.LocalPlayer.carpetFrame = -1;
									Main.LocalPlayer.carpetFrameCounter = 0f;
								}
								Main.LocalPlayer.wingTime = Main.LocalPlayer.wingTimeMax;
								Main.LocalPlayer.rocketTime = Main.LocalPlayer.rocketTimeMax;
								Main.LocalPlayer.canJumpAgain_Blizzard = true;
								Main.LocalPlayer.canJumpAgain_Cloud = true;
								Main.LocalPlayer.canJumpAgain_Fart = true;
								Main.LocalPlayer.canJumpAgain_Sail = true;
								Main.LocalPlayer.canJumpAgain_Sandstorm = true;
								Main.LocalPlayer.canJumpAgain_Unicorn = true;
								Main.LocalPlayer.canJumpAgain_WallOfFleshGoat = true;
								Main.LocalPlayer.canJumpAgain_Santank = true;
								Main.LocalPlayer.canJumpAgain_Basilisk = true;
								Main.LocalPlayer.jump = 15;
								if (Main.LocalPlayer.wet && Main.LocalPlayer.merman) Main.LocalPlayer.jump = 30;
								Main.LocalPlayer.UpdateJumpHeight();
							}
						}
					}
				}

				switch (NPC.ai[0])
				{
					case 0:
						//shoot a randomly spiraling set of outwards-moving projectiles, denser at lower health
						int attackTime = 240;
						int downTime = 60;

						//initialize
						if (NPC.ai[1] % (attackTime + downTime) == 0)
						{
							//firing rate
							NPC.localAI[1] = 2 - amountThroughFirstPhase;

							const int maxPerPulse = 20;
							const float oddsScalingFactor = 1.33f;
							//odds of ai[2]=2 are 1-1/1.33 = 1/4
							NPC.ai[2] = maxPerPulse - (int)(Math.Log(Main.rand.NextFloat(1f, (float)Math.Pow(oddsScalingFactor, maxPerPulse - 1))) / Math.Log(oddsScalingFactor));

							//generate random spacing that's not too close to any rational number by using... continued fractions! The only time continued fractions have ever been useful for anything.
							float spacing = Main.rand.NextFloat(0, 1);
							for (int i = 0; i < 10; i++)
							{
								spacing = 1 / (Main.rand.Next(1, 4) + spacing);
							}
							if (spacing > 0.5f) spacing = 1 - spacing;
							if (Main.rand.NextBool()) spacing = -spacing;

							//ai[3] is the angle between shots
							NPC.ai[3] = spacing * MathHelper.TwoPi / NPC.ai[2];
							//localAI[2] is random initial angle
							NPC.localAI[2] = Main.rand.NextFloat(MathHelper.TwoPi);

							//localAI[3] tracks total shots fired so far
							NPC.localAI[3] = 0;
						}

						if (NPC.ai[1] % (attackTime + downTime) < attackTime)
						{
							while (NPC.localAI[3] * NPC.ai[2] * NPC.localAI[1] <= NPC.ai[1] % (attackTime + downTime))
							{
								for (int i = 0; i < NPC.ai[2]; i++)
								{
									const float shotSpeed = 8;
									float shotTranslationLength = shotSpeed * ((NPC.ai[1] % (attackTime + downTime)) - NPC.localAI[3] * NPC.ai[2] * NPC.localAI[1]) / ARENA_RADIUS;
									HyperbolicTransform shotTranslation = HyperbolicTransform.Translation(shotTranslationLength);
									HyperbolicTransform shotTransform = transform * HyperbolicTransform.Rotation(NPC.localAI[2] + i * MathHelper.TwoPi / NPC.ai[2] + NPC.localAI[3] * NPC.ai[3]) * shotTranslation;
									Projectile bolt = HyperbolicBolt.NewProjectile(NPC.GetSource_FromAI(), worldPosition, shotTransform, new Vector2(shotSpeed, 0), projectileDamage, 0f, ai0: 0f);

									//transform trail visual improvement
									for (int j = 1; j < (bolt.ModProjectile as HyperbolicBolt).oldTransform.Length; j++)
                                    {
										(bolt.ModProjectile as HyperbolicBolt).oldTransform[j] = transform;
									}
								}
								NPC.localAI[3]++;

								SoundEngine.PlaySound(SoundID.Item33.WithVolumeScale(0.5f), transform.GetPosition() * ARENA_RADIUS + worldPosition);
							}
						}

						//rotate, but don't change the point we're looking at
						if (NPC.ai[1] % (attackTime + downTime) < attackTime)
						{
							goalLookingRotation = lookingRotation + NPC.ai[3] / (NPC.ai[2] * NPC.localAI[1]);
							rotationLerp = 1f;
						}
						else
                        {
							goalLookingRotation = lookingRotation;
							rotationLerp = 0f;
						}

						//static base looking direction
						goalLookingTransform = lookingTransform;
						lookingTransformMovementMult = 0f;

						//move towards player slowly
						NPC.velocity = new Vector2(0.75f, 0).RotatedBy(transform.AngleTo(target));

						NPC.ai[1]++;
						if (NPC.ai[1] >= (attackTime + downTime))
						{
							gotoNextAIState = true;
						}
						break;
					case 1:
						//deathray(s?) to the edge of the arena which produces some limit-parallel projectiles (deathray should look like hypercycles
						if (NPC.ai[1] == 0)
						{
							NPC.ai[3] = (int)(180 - amountThroughFirstPhase * 90);
						}

						if (NPC.ai[1] % NPC.ai[3] == 0 && NPC.ai[1] <= NPC.ai[3] * 3 + 60)
						{
							NPC.ai[2] = transform.AngleTo(target);

							if (NPC.ai[3] > 120 || NPC.ai[1] == 0) NPC.localAI[1] = NPC.ai[2];

							HyperbolicRay.NewProjectile(NPC.GetSource_FromThis(), worldPosition, transform * HyperbolicTransform.Rotation(NPC.ai[2]), Vector2.Zero, projectileDamage, 0f, ai0: NPC.whoAmI, ai1: 0f);
						}

						if (NPC.ai[1] >= 60 && (NPC.ai[1] - 60 + NPC.ai[3]) % NPC.ai[3] >= 0 && (NPC.ai[1] - 60 + NPC.ai[3]) % NPC.ai[3] < 60 && NPC.ai[1] <= NPC.ai[3] * 3 + 120)
						{
							//don't switch direction to the new telegraph immediately when timings overlap
							if ((NPC.ai[1]) % NPC.ai[3] <= 60) NPC.localAI[1] = NPC.ai[2];

							NPC.velocity = new Vector2(-(60 - (NPC.ai[1] - 60 + NPC.ai[3]) % NPC.ai[3]) / 30f, 0).RotatedBy(NPC.localAI[1]);
						}
						else
						{
							NPC.velocity = Vector2.Zero;
						}

						//limit rotation perpendicular to (transform * HyperbolicTransform.Rotation(npc.localAI[1]));
						goalLookingTransform = (transform * HyperbolicTransform.Rotation(NPC.localAI[1])) * HyperbolicTransform.LimitRotation(0.01f) * (transform * HyperbolicTransform.Rotation(NPC.localAI[1])).Inverse();
						lookingTransformMovementMult = 0.1f;
						goalLookingRotation = MathHelper.PiOver2;
						rotationLerp = 0.1f;

						NPC.ai[1]++;
						if (NPC.ai[1] >= NPC.ai[3] * 4 + 90)
						{
							gotoNextAIState = true;
						}
						break;
					case 2:
						//old attack: shoot projectiles in 2 directions on unbounded trajectories
						/*if (npc.ai[1] == 60)
						{
							HyperbolicTransform shotTransform = HyperbolicTransform.Rotation(transform.AngleTo(target));

							float maxSpread = 20;
							float shotSpeed = amountThroughFirstPhase * 4 + 8;

							for (int i = -(int)maxSpread; i <= maxSpread; i++)
							{
								HyperbolicBolt.NewProjectile(NPC.GetSource_FromAI(), worldPosition, transform * shotTransform, new Vector2(shotSpeed, 0), projectileDamage, 0f, ai0: 2f, ai1: i / maxSpread);
								HyperbolicBolt.NewProjectile(NPC.GetSource_FromAI(), worldPosition, transform * shotTransform * HyperbolicTransform.Rotation(MathHelper.Pi), new Vector2(shotSpeed, 0), projectileDamage, 0f, ai0: 2f, ai1: i / maxSpread);
							}
						}*/

						//shoot projectiles on outward horocycles
						if (NPC.ai[1] >= 60 && NPC.ai[1] <= 180 && NPC.ai[1] % 60 == 0)
                        {
							int numShots = 3 * (int)(3 + amountThroughFirstPhase * 4);
							for (int i = 0; i < numShots; i++)
							{
								HyperbolicBolt.NewProjectile(NPC.GetSource_FromAI(), worldPosition, transform * HyperbolicTransform.Rotation(i * MathHelper.TwoPi / numShots), new Vector2(8, 0), projectileDamage, 0f, ai0: 2f, ai1: 1f);
								HyperbolicBolt.NewProjectile(NPC.GetSource_FromAI(), worldPosition, transform * HyperbolicTransform.Rotation(i * MathHelper.TwoPi / numShots), new Vector2(8, 0), projectileDamage, 0f, ai0: 2f, ai1: -1f);
							}

							SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion, transform.GetPosition() * ARENA_RADIUS + worldPosition);
						}

						//move towards player slowly
						NPC.velocity = new Vector2(0.75f, 0).RotatedBy(transform.AngleTo(target));

						//static
						goalLookingTransform = lookingTransform;
						lookingTransformMovementMult = 0f;

						NPC.ai[1]++;
						if (NPC.ai[1] >= 300)
						{
							gotoNextAIState = true;
						}
						break;
					case 3:
						//boss shoots loads of homing projectiles in all directions that can travel in at most a horocycle of curvature

						if (NPC.ai[1] == 0)
						{
							NPC.ai[2] = (int)(amountThroughFirstPhase * 6 + 3);
						}

						if (NPC.ai[1] % 15 == 0 && NPC.ai[1] < 360)
						{
							float angleGoal = transform.AngleTo(target);
							HyperbolicTransform shotTransform = HyperbolicTransform.Rotation(angleGoal);

							for (int i = 0; i < NPC.ai[2]; i++)
							{
								HyperbolicBolt.NewProjectile(NPC.GetSource_FromAI(), worldPosition, transform * shotTransform * HyperbolicTransform.Rotation(i * MathHelper.TwoPi / NPC.ai[2]), new Vector2(8, 0), projectileDamage, 0f, ai0: 3f);
							}

							SoundEngine.PlaySound(SoundID.Item33.WithVolumeScale(0.5f), transform.GetPosition() * ARENA_RADIUS + worldPosition);
						}

						if (NPC.ai[1] < 360)
                        {
							//look to player
							goalLookingTransform = target * HyperbolicTransform.Rotation(0.01f) * target.Inverse();
							lookingTransformMovementMult = 0.1f;
							goalLookingRotation = MathHelper.PiOver2;
							rotationLerp = 0.1f;
						}
						else
						{
							//static
							goalLookingTransform = lookingTransform;
							lookingTransformMovementMult = 0f;
						}

						//move towards player slowly
						NPC.velocity = new Vector2(0.75f, 0).RotatedBy(transform.AngleTo(target));

						NPC.ai[1]++;
						if (NPC.ai[1] >= 420)
						{
							gotoNextAIState = true;
						}
						break;
					case 4:
						//create beams and sweep them by moving in a circle
						if (NPC.ai[1] == 0)
						{
							int numBeams = (int)(amountThroughFirstPhase * 5 + 3);
							float randomOffset = Main.rand.NextFloat(MathHelper.TwoPi);
							NPC.ai[2] = Main.rand.NextFloat(MathHelper.TwoPi);
							NPC.ai[3] = Main.rand.NextBool() ? 1 : -1;

							for (int i = 0; i < numBeams; i++)
							{
								HyperbolicRay.NewProjectile(NPC.GetSource_FromThis(), worldPosition, transform * HyperbolicTransform.Rotation(i * MathHelper.TwoPi / numBeams + randomOffset), Vector2.Zero, 20, 0f, timeLeft: 540, ai0: NPC.whoAmI, ai1: 1f);
							}
						}

						float speed = (float)Math.Sin(NPC.ai[1] * MathHelper.Pi / 600) * 6f;
						NPC.velocity = new Vector2(speed, 0f).RotatedBy(NPC.ai[3] * NPC.ai[1] * MathHelper.TwoPi / 240 + NPC.ai[2]);

						//static looking direction
						goalLookingTransform = lookingTransform;
						lookingTransformMovementMult = 0f;

						NPC.ai[1]++;
						if (NPC.ai[1] >= 600)
						{
							gotoNextAIState = true;
						}
						break;
					case 5:
						NPC.velocity = Vector2.Zero;
						NPC.dontTakeDamage = true;

						if (NPC.ai[1] == 0)
						{
							currentFrame = 16;
							NPC.frameCounter = 0;
						}

						//static
						goalLookingTransform = lookingTransform;
						lookingTransformMovementMult = 0f;

						NPC.ai[1]++;
						if (NPC.ai[1] >= 120)
						{
							gotoNextAIState = true;
						}
						break;
					case 6:
						//move towards player, arcs active, while regularly firing projectiles from all segments that head towards the limit points

						//Blink to the closest segment to the player
						if (NPC.ai[1] % 60 == 0)
						{
							HyperbolicTransform oldTransform = transform;
							transform = GetClosestNode(target);
							if (oldTransform.DistanceTo(transform) > maxArmLength / 2)
								teleportFlashTime = maxTeleportFlashTime;
						}

						NPC.velocity = new Vector2(0.75f, 0).RotatedBy(transform.AngleTo(target));

						if (NPC.ai[1] % 60 == 0 && NPC.ai[1] >= 60)
						{
							List<HyperbolicTransform> segments = GetVisibleSegments(0.01f);
							foreach (HyperbolicTransform localTransform in segments)
							{
								for (int i = 0; i < 3; i++)
								{
									HyperbolicTransform shotTransform = localTransform * HyperbolicTransform.Rotation(i * MathHelper.TwoPi / 3);
									if (shotTransform.RayDistanceTo(HyperbolicTransform.Identity) < 1.6f)
									{
										HyperbolicBolt.NewProjectile(NPC.GetSource_FromAI(), worldPosition, shotTransform, new Vector2(12, 0), projectileDamage, 0f, 240, 0f);
									}
								}
							}

							HyperbolicRay.NewProjectile(NPC.GetSource_FromThis(), worldPosition, transform * HyperbolicTransform.Rotation(transform.AngleTo(target)), Vector2.Zero, projectileDamage, 0f, timeLeft: 60, ai0: NPC.whoAmI, ai1: 2f);

							SoundEngine.PlaySound(SoundID.Item33.WithVolumeScale(0.5f), transform.GetPosition() * ARENA_RADIUS + worldPosition);
						}

						//look to player
						goalLookingTransform = target * HyperbolicTransform.Rotation(0.01f) * target.Inverse();
						lookingTransformMovementMult = 0.1f;
						goalLookingRotation = MathHelper.PiOver2;
						rotationLerp = 0.1f;

						if (NPC.ai[1] < 60)
						{
							arcChainTextureFrame.Y = arcChainTextureFrame.Height * arcChainTelegraphAnimation[(int)NPC.ai[1] / 4 % 8];
						}
						else
						{
							arcChainTextureFrame.Y = arcChainTextureFrame.Height * 5;
						}

						if (NPC.ai[1] == 60) SoundEngine.PlaySound(SoundID.Item67, NPC.Center);

						NPC.ai[1]++;
						if (NPC.ai[1] >= 420)
						{
							gotoNextAIState = true;
							arcChainTextureFrame.Y = 0;
						}
						break;
					case 7:
						//lingering deathrays from nearby segments
						if (NPC.ai[1] % 20 == 0 && NPC.ai[1] <= 360 && NPC.ai[1] >= 60)
						{
							//blink to a random location around the player
							HyperbolicTransform oldTransform = transform;

							//go to the closest segment to the player
							transform = GetClosestNode(target);

							while (teleportFlashTime < maxTeleportFlashTime)
							{
								//go outwards randomly a few segments
								HyperbolicTransform newTransform = transform;

								//Only go out at most 1
								//Strictly speaking some of this complexity isn't necessary
								for (int i = 0; i < 1; i++)
								{
									if (!Main.rand.NextBool(3 - i))
									{
										if (i == 0)
										{
											newTransform *= HyperbolicTransform.Rotation(Main.rand.Next(3) * MathHelper.TwoPi / 3 + MathHelper.Pi) * HyperbolicTransform.Translation(maxArmLength);
										}
										else
										{
											newTransform *= HyperbolicTransform.Rotation((Main.rand.Next(2) * 2 - 1) * MathHelper.TwoPi / 3 + MathHelper.Pi) * HyperbolicTransform.Translation(maxArmLength);
										}
									}
									else break;
								}

								if (oldTransform.DistanceTo(newTransform) > maxArmLength / 2)
								{
									transform = newTransform;
									teleportFlashTime = maxTeleportFlashTime;
								}
							}

							NPC.localAI[1] = transform.AngleTo(target);
							HyperbolicTransform shotTransform = transform * HyperbolicTransform.Rotation(transform.AngleTo(target));
							HyperbolicRay.NewProjectile(NPC.GetSource_FromThis(), worldPosition, shotTransform, Vector2.Zero, projectileDamage, 0f, timeLeft: 120, ai0: NPC.whoAmI, ai1: 3f);
						}

						NPC.velocity = Vector2.Zero;

						if (NPC.ai[1] < 60)
						{
							//look to player
							goalLookingTransform = target * HyperbolicTransform.Rotation(0.01f) * target.Inverse();
							lookingTransformMovementMult = 0.1f;
							goalLookingRotation = MathHelper.PiOver2;
							rotationLerp = 0.1f;
						}
						else
						{
							//look to limit point of ray
							goalLookingTransform = (transform * HyperbolicTransform.Rotation(NPC.localAI[1])) * HyperbolicTransform.LimitRotation(0.01f) * (transform * HyperbolicTransform.Rotation(NPC.localAI[1])).Inverse();
							lookingTransformMovementMult = 0.2f;
							goalLookingRotation = MathHelper.PiOver2;
							rotationLerp = 0.1f;
						}

						NPC.ai[1]++;
						if (NPC.ai[1] >= 510)
						{
							gotoNextAIState = true;
						}
						break;
					case 8:
						//Blink to the closest segment to the player
						if (NPC.ai[1] == 0)
						{
							HyperbolicTransform oldTransform = transform;
							transform = GetClosestNode(target);
							if (oldTransform.DistanceTo(transform) > maxArmLength / 2)
								teleportFlashTime = maxTeleportFlashTime;
						}

						//Everything fires things directly at the player because why not
						if (NPC.ai[1] == 60)
						{
							List<HyperbolicTransform> segments = GetVisibleSegments(0.001f);
							foreach (HyperbolicTransform localTransform in segments)
							{
								if (localTransform.DistanceTo(target) < 3f)
								{
									HyperbolicBolt.NewProjectile(NPC.GetSource_FromAI(), worldPosition, localTransform * HyperbolicTransform.Rotation(localTransform.AngleTo(target)), new Vector2(10, 0), projectileDamage, 0f, timeLeft: 720, ai0: 4f);
								}
							}
						}
						if (NPC.ai[1] <= 60)
						{
							//look to player
							goalLookingTransform = target * HyperbolicTransform.Rotation(0.01f) * target.Inverse();
							lookingTransformMovementMult = 0.1f;
							goalLookingRotation = MathHelper.PiOver2;
							rotationLerp = 0.1f;
						}
						else
                        {
							//static
							goalLookingTransform = lookingTransform;
							lookingTransformMovementMult = 0f;
						}

						if (NPC.ai[1] >= 90 && NPC.ai[1] < 165 && NPC.ai[1] % 5 == 0)
						{
							SoundEngine.PlaySound(SoundID.Item33.WithVolumeScale(0.5f), transform.GetPosition() * ARENA_RADIUS + worldPosition);
						}

						NPC.velocity = Vector2.Zero;

						NPC.ai[1]++;
						if (NPC.ai[1] >= 570)
						{
							gotoNextAIState = true;
						}
						break;
					case 9:
						//blink to a random faraway thing and move towards the center, firing at the player
						const int timeBeforeAttackActuallyStarts = 30;

						if (NPC.ai[1] == 0)
						{
							HyperbolicTransform oldTransform = transform;

							//go to the closest segment to the center
							transform = GetClosestNode(HyperbolicTransform.Identity);

							//go outwards randomly until we're far enough away
							bool stepped = false;
							while (transform.DistanceTo(HyperbolicTransform.Identity) < 4f)
							{
								if (!stepped)
								{
									stepped = true;
									transform *= HyperbolicTransform.Rotation(Main.rand.Next(3) * MathHelper.TwoPi / 3 + MathHelper.Pi) * HyperbolicTransform.Translation(maxArmLength);
								}
								else
								{
									transform *= HyperbolicTransform.Rotation((Main.rand.Next(2) * 2 - 1) * MathHelper.TwoPi / 3 + MathHelper.Pi) * HyperbolicTransform.Translation(maxArmLength);
								}
							}

							NPC.ai[2] = transform.AngleTo(HyperbolicTransform.Identity);
							NPC.ai[3] = transform.DistanceTo(HyperbolicTransform.Identity);

							if (oldTransform.DistanceTo(transform) > maxArmLength / 2)
								teleportFlashTime = maxTeleportFlashTime;

							//ray telegraph
							HyperbolicTransform closestNodePlayer = GetClosestNode(target);
							HyperbolicRay.NewProjectile(NPC.GetSource_FromThis(), worldPosition, closestNodePlayer * HyperbolicTransform.Rotation(closestNodePlayer.AngleTo(transform)), Vector2.Zero, 0, 0, timeLeft: 30, ai0: NPC.whoAmI, ai1: 5f);
						}

						//look along movement direction
						goalLookingTransform = transform * HyperbolicTransform.Translation(0.01f, NPC.ai[2]) * transform.Inverse();
						lookingTransformMovementMult = 0.1f;
						goalLookingRotation = 0;
						rotationLerp = 0.1f;

						if ((NPC.ai[1] - timeBeforeAttackActuallyStarts) % 84 == 60 && (NPC.ai[1] - timeBeforeAttackActuallyStarts) >= 60 && (NPC.ai[1] - timeBeforeAttackActuallyStarts) <= 480)
						{
							HyperbolicTransform closestTransform = GetClosestNode(target);

							int numShots = (int)(((NPC.ai[1] - timeBeforeAttackActuallyStarts) - 60) / 84 + 1) * 3;
							for (int i = 0; i < numShots; i++)
							{
								HyperbolicBolt.NewProjectile(NPC.GetSource_FromAI(), worldPosition, closestTransform * HyperbolicTransform.Rotation(i * MathHelper.TwoPi / numShots + closestTransform.AngleTo(target)), new Vector2(8, 0), projectileDamage, 0f, 600, 4f);
							}

							SoundEngine.PlaySound(SoundID.Item33.WithVolumeScale(0.5f), closestTransform.GetPosition() * ARENA_RADIUS + worldPosition);
						}

						if (((NPC.ai[1] - timeBeforeAttackActuallyStarts) - 30) % 84 == 60 && ((NPC.ai[1] - timeBeforeAttackActuallyStarts) - 30) >= 60 && ((NPC.ai[1] - timeBeforeAttackActuallyStarts) - 30 <= 480))
                        {
							SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion, transform.GetPosition() * ARENA_RADIUS + worldPosition);
						}

						float speedModifier = 1f;
						if (NPC.ai[1] < timeBeforeAttackActuallyStarts) speedModifier = 0f;
						else if ((NPC.ai[1] - timeBeforeAttackActuallyStarts) <= 60) speedModifier = (NPC.ai[1] - timeBeforeAttackActuallyStarts) / 60f;
						else if ((NPC.ai[1] - timeBeforeAttackActuallyStarts) >= 480) speedModifier = (540 - (NPC.ai[1] - timeBeforeAttackActuallyStarts)) / 60f;
						NPC.velocity = new Vector2(NPC.ai[3] * ARENA_RADIUS / 480f * speedModifier, 0).RotatedBy(NPC.ai[2]);

						NPC.ai[1]++;
						if (NPC.ai[1] >= 540 + timeBeforeAttackActuallyStarts)
						{
							gotoNextAIState = true;
						}
						break;
					case 10:
						//Activate chains, move in a horocycle around the player while shooting a projectile-emitting sweeping deathray inwards, follow up with a few extra projectiles
						int maxAttackTime = 540;
						int attackStartup = 60;
						int attackBeforeStart = 120;

						if (NPC.ai[1] == attackBeforeStart)
						{
							HyperbolicTransform oldTransform = transform;

							//blink close to player and face them
							transform = GetClosestNode(target);
							int i = (int)((transform.AngleTo(target) + MathHelper.Pi) * (3 / MathHelper.TwoPi) + 2);
							transform *= HyperbolicTransform.Rotation(i * MathHelper.TwoPi / 3);

							if (oldTransform.DistanceTo(transform) > maxArmLength / 2)
								teleportFlashTime = maxTeleportFlashTime;

							//sweep away from the player
							NPC.ai[2] = transform.AngleTo(target) > 0 ? 1 : -1;

							HyperbolicRay.NewProjectile(NPC.GetSource_FromThis(), worldPosition, transform, Vector2.Zero, projectileDamage, 0f, timeLeft: 210, ai0: NPC.whoAmI, ai1: 4f);
						}

						if (NPC.ai[1] >= attackStartup + attackBeforeStart && (NPC.ai[1] - attackBeforeStart - attackStartup) % 20 == 0 && NPC.ai[1] < 210 + attackBeforeStart)
						{
							//shoot projectiles from the deathray
							for (int i = 1; i < 12; i++)
							{
								HyperbolicTransform shotTransform = transform * HyperbolicTransform.Translation(i * 0.15f) * HyperbolicTransform.Rotation(MathHelper.PiOver2 * NPC.ai[2]);
								HyperbolicBolt.NewProjectile(NPC.GetSource_FromAI(), worldPosition, shotTransform, new Vector2(8, 0), projectileDamage, 0f, ai0: 5f, ai1: -NPC.ai[2]);
							}

							SoundEngine.PlaySound(SoundID.Item33.WithVolumeScale(0.5f), transform.GetPosition() * ARENA_RADIUS + worldPosition);
						}

						//if laser is done, blink close to the player on the horocycle and shoot at them
						if (NPC.ai[1] >= 210 + attackBeforeStart && NPC.ai[1] <= maxAttackTime - attackStartup)
						{
							HyperbolicTransform oldTransform = transform;

							bool trying = true;
							while (trying)
							{
								trying = false;

								HyperbolicTransform newTransform = transform * HyperbolicTransform.Rotation(-NPC.ai[2] * MathHelper.TwoPi / 3 + MathHelper.Pi) * HyperbolicTransform.Translation(maxArmLength) * HyperbolicTransform.Rotation(-NPC.ai[2] * MathHelper.TwoPi / 3);

								if ((newTransform * HyperbolicTransform.LimitRotation(-NPC.ai[2] * 0.5f / (float)Math.Sqrt(3))).DistanceTo(target) < (transform * HyperbolicTransform.LimitRotation(-NPC.ai[2] * 0.5f / (float)Math.Sqrt(3))).DistanceTo(target))
								{
									transform = newTransform;
									trying = true;
								}
							}

							if (oldTransform.DistanceTo(transform) > maxArmLength / 2)
							{
								teleportFlashTime = maxTeleportFlashTime;

								if (NPC.ai[1] > 210 + attackBeforeStart)
								{
									HyperbolicBolt.NewProjectile(NPC.GetSource_FromAI(), worldPosition, transform, new Vector2(8, 0), projectileDamage, 0f, ai0: 4f);

									SoundEngine.PlaySound(SoundID.Item33.WithVolumeScale(0.5f), transform.GetPosition() * ARENA_RADIUS + worldPosition);
								}
							}
						}

						if (NPC.ai[1] >= attackBeforeStart)
						{
							speedModifier =
								((NPC.ai[1] - attackBeforeStart) < attackStartup) ?
									(NPC.ai[1] - attackBeforeStart) / attackStartup :
								(NPC.ai[1] >= (maxAttackTime - attackStartup) ?
									(maxAttackTime - NPC.ai[1]) / attackStartup :
								1f);
							HyperbolicTransform limitRotation = HyperbolicTransform.LimitRotation(16f / ARENA_RADIUS * speedModifier * NPC.ai[2]);
							transform *= limitRotation;

							//look within
							goalLookingTransform = transform * HyperbolicTransform.LimitRotation(0.01f) * transform.Inverse();
							lookingTransformMovementMult = 0.1f;
							goalLookingRotation = MathHelper.PiOver2;
							rotationLerp = 0.1f;
						}

						NPC.velocity = Vector2.Zero;

						if (NPC.ai[1] < attackBeforeStart)
						{
							if ((int)(Math.Pow(NPC.ai[1] / attackBeforeStart, 2) * 15 % 2) == 0)
							{
								arcChainTextureFrame.Y = arcChainTextureFrame.Height * arcChainTelegraphAnimation[(int)NPC.ai[1] / 4 % 8];
							}
							else
							{
								arcChainTextureFrame.Y = 0;
							}
						}
						else if (NPC.ai[1] >= (maxAttackTime - attackStartup))
							arcChainTextureFrame.Y = 0;
						else
							arcChainTextureFrame.Y = arcChainTextureFrame.Height * 5;

						if (NPC.ai[1] == attackBeforeStart) SoundEngine.PlaySound(SoundID.Item67, NPC.Center);

						NPC.ai[1]++;
						if (NPC.ai[1] >= maxAttackTime)
						{
							gotoNextAIState = true;
						}
						break;
					case 11:
						//Blink rapidly along the horocycle enclosing the player, shooting homing projectiles at them
						attackTime = 120;
						int attackIterations = 4;
						int attackExtra = 240; //135;

						if (NPC.ai[1] % attackTime == 0 && NPC.ai[1] < attackTime * attackIterations)
						{
							HyperbolicTransform oldTransform = transform;

							//go to the point on/near the horocycle containing the player closest to them
							transform = GetClosestNode(target);
							int turn = (int)((transform.AngleTo(target) + MathHelper.Pi) * (3 / MathHelper.TwoPi) + 2);
							transform *= HyperbolicTransform.Rotation(turn * MathHelper.TwoPi / 3);

							//pick random direction
							NPC.ai[2] = Main.rand.NextBool() ? 1 : -1;

							int stepsAlongSecondHorocycle = 6;
							//go backwards along our horocycle 6 steps
							for (int i = 0; i < stepsAlongSecondHorocycle; i++)
							{
								transform *= HyperbolicTransform.Rotation(-NPC.ai[2] * MathHelper.TwoPi / 3 + MathHelper.Pi) * HyperbolicTransform.Translation(maxArmLength) * HyperbolicTransform.Rotation(-NPC.ai[2] * MathHelper.TwoPi / 3);
							}

							if (oldTransform.DistanceTo(transform) > maxArmLength / 2)
								teleportFlashTime = maxTeleportFlashTime;
						}
						else if (NPC.ai[1] % 10 == 0 && NPC.ai[1] < attackTime * attackIterations)
						{
							//step along our horocycle
							HyperbolicTransform oldTransform = transform;

							transform *= HyperbolicTransform.Rotation(NPC.ai[2] * MathHelper.TwoPi / 3 + MathHelper.Pi) * HyperbolicTransform.Translation(maxArmLength) * HyperbolicTransform.Rotation(NPC.ai[2] * MathHelper.TwoPi / 3);

							if (oldTransform.DistanceTo(transform) > maxArmLength / 2)
								teleportFlashTime = maxTeleportFlashTime;

							//shoot horocyclic homing projectiles inwards
							HyperbolicBolt.NewProjectile(NPC.GetSource_FromAI(), worldPosition, transform, new Vector2(8, 0), projectileDamage, 0f, 600, 3f);

							SoundEngine.PlaySound(SoundID.Item33.WithVolumeScale(0.5f), transform.GetPosition() * ARENA_RADIUS + worldPosition);
						}

						NPC.velocity = Vector2.Zero;

						//look to player
						goalLookingTransform = target * HyperbolicTransform.Rotation(0.01f) * target.Inverse();
						lookingTransformMovementMult = 0.1f;
						goalLookingRotation = MathHelper.PiOver2;
						rotationLerp = 0.1f;

						NPC.ai[1]++;
						if (NPC.ai[1] >= attackTime * attackIterations + attackExtra)
						{
							gotoNextAIState = true;
						}
						break;
					case 12:
						//Aperiogons become unsafe at random
						if (NPC.ai[1] == 0)
						{
							HyperbolicTransform oldTransform = transform;
							transform = GetClosestNode(target);
							if (oldTransform.DistanceTo(transform) > maxArmLength / 2)
								teleportFlashTime = maxTeleportFlashTime;

							//create aperiogon-filling projectiles
							oldTransform = transform;
							transform = GetClosestNode(HyperbolicTransform.Identity);
							List<HyperbolicTransform> segments = GetVisibleSegments(0.001f);
							foreach (HyperbolicTransform localTransform in segments)
							{
								int a = 0;
								for (int j = 0; j < 2; j++)
								{
									a += Main.rand.Next(180) + 180;
									HyperbolicAperiogon.NewProjectile(NPC.GetSource_FromThis(), worldPosition, localTransform, Vector2.Zero, (int)(projectileDamage * 1.5f), 0f, timeLeft: a, ai0: NPC.whoAmI);
								}
							}
							//two extras from the main segment that wouldn't be covered otherwise
							for (int i = 1; i < 3; i++)
							{
								HyperbolicTransform localTransform = transform * HyperbolicTransform.Rotation(i * MathHelper.TwoPi / 3);

								int a = 0;
								for (int j = 0; j < 2; j++)
								{
									a += Main.rand.Next(180) + 180;
									HyperbolicAperiogon.NewProjectile(NPC.GetSource_FromThis(), worldPosition, localTransform, Vector2.Zero, (int)(projectileDamage * 1.5f), 0f, timeLeft: a, ai0: NPC.whoAmI);
								}
							}
							transform = oldTransform;
						}

						NPC.velocity = Vector2.Zero;

						if (NPC.ai[1] % 20 == 0)
						{
							SoundEngine.PlaySound(SoundID.DD2_WitherBeastAuraPulse, transform.GetPosition() * ARENA_RADIUS + worldPosition);
						}

						//SPEEN
						float spinRate;
						if (NPC.ai[1] < 180)
                        {
							spinRate = 0.2f * NPC.ai[1] / 180f;
                        }
						else
                        {
							spinRate = 0.2f * (720 - NPC.ai[1]) / 540f;
						}

						goalLookingTransform = lookingTransform;
						lookingTransformMovementMult = 0f;
						goalLookingRotation = lookingRotation + spinRate;
						rotationLerp = 1f;

						NPC.ai[1]++;
						if (NPC.ai[1] >= 720)
						{
							gotoNextAIState = true;
						}
						break;
				}

				if (gotoNextAIState)
				{
					//go to next AI state
					NPC.ai[1] = 0;

					GotoNextAIState();
				}
			}

			if (teleportFlashTime == maxTeleportFlashTime)
			{
				Vector2 soundPos = transform.GetPosition() * ARENA_RADIUS + worldPosition;
				SoundEngine.PlaySound(SoundID.Item25.WithVolumeScale(0.5f).WithPitchOffset(-0.5f), soundPos);
			}

			//phase 2 main segment animation is always the same
			if (NPC.ai[0] >= 5 || NPC.ai[0] == -2)
			{
				NPC.frameCounter++;
				if (NPC.frameCounter >= 5)
				{
					NPC.frameCounter = 0;

					currentFrame++;
					if (currentFrame >= 20)
					{
						currentFrame = 16;
					}
				}
			}

			//secondary frame animation
			secondaryFrameCounter++;
			if (secondaryFrameCounter % 3 == 0)
			{
				currentSecondaryFrame = secondaryFrameCounter / 3 % phase2SecondaryFrameAnimation.Length;

				currentSecondaryFrame = phase2SecondaryFrameAnimation[currentSecondaryFrame];
			}

			//update lookingTransform with velocity
			//newLookingTransform * transform * HyperbolicTransform.FromVelocity(npc.velocity / ARENA_RADIUS) = transform * HyperbolicTransform.FromVelocity(npc.velocity / ARENA_RADIUS) * transform.Inverse() * lookingTransform * transform;
			lookingTransform = transform * HyperbolicTransform.FromVelocity(NPC.velocity / ARENA_RADIUS) * transform.Inverse() * lookingTransform * transform * HyperbolicTransform.FromVelocity(NPC.velocity / ARENA_RADIUS).Inverse() * transform.Inverse();

			//update rotation data looking at animation
			lookingTransform *= HyperbolicTransform.Pow(goalLookingTransform / lookingTransform, lookingTransformMovementMult);

			//bound goalLookingRotation
			while (goalLookingRotation > MathHelper.Pi) goalLookingRotation -= MathHelper.TwoPi;
			while (goalLookingRotation < -MathHelper.Pi) goalLookingRotation += MathHelper.TwoPi;

			lookingRotation = Utils.AngleLerp(lookingRotation, goalLookingRotation, rotationLerp);

			//update frames with new frame data
			NPC.frame = new Rectangle((currentFrame / 5) * NPC.frame.Width, (currentFrame % 5) * NPC.frame.Height, NPC.frame.Width, NPC.frame.Height);
			secondaryFrame = new Rectangle((currentSecondaryFrame / 5) * NPC.frame.Width, (currentSecondaryFrame % 5) * NPC.frame.Height, NPC.frame.Width, NPC.frame.Height);



			//update hyperbolic motion
			transform *= HyperbolicTransform.FromVelocity(NPC.velocity / ARENA_RADIUS);
			SetPosition();

			//don't take damage when too small
			if (!NPC.dontTakeDamage)
				NPC.dontTakeDamage = NPC.scale < 0.05f;

			//we don't actually want to move normally with velocity
			NPC.position -= NPC.velocity;
		}


		private float[] aiWeightsPhase1 = new float[5];
		private float[] aiWeightsPhase2 = new float[7];
		private void GotoNextAIState()
		{
			if (NPC.ai[0] < 5)
			{
				WeightedRandom<int> aiStatePool = new WeightedRandom<int>();
				for (int state = 0; state < aiWeightsPhase1.Length; state++)
				{
					//weights are squared to bias more towards attacks that haven't been used in a while
					aiStatePool.Add(state, Math.Pow(aiWeightsPhase1[state], 2));
				}
				NPC.ai[0] = aiStatePool;
				for (int state = 0; state < aiWeightsPhase1.Length; state++)
				{
					if (NPC.ai[0] != state)
						aiWeightsPhase1[state] += aiWeightsPhase1[(int)NPC.ai[0]] / (aiWeightsPhase1.Length - 1);
				}
				aiWeightsPhase1[(int)NPC.ai[0]] = 0f;
			}
			else
            {
				WeightedRandom<int> aiStatePool = new WeightedRandom<int>();
				for (int state = 0; state < aiWeightsPhase2.Length; state++)
				{
					//weights are squared to bias more towards attacks that haven't been used in a while
					aiStatePool.Add(state, Math.Pow(aiWeightsPhase2[state], 2));
				}
				NPC.ai[0] = aiStatePool + 6;
				for (int state = 0; state < aiWeightsPhase2.Length; state++)
				{
					if (NPC.ai[0] - 6 != state)
						aiWeightsPhase2[state] += aiWeightsPhase2[(int)NPC.ai[0] - 6] / (aiWeightsPhase2.Length - 1);
				}
				aiWeightsPhase2[(int)NPC.ai[0] - 6] = 0f;
			}
		}
		private void InitializeAIStates()
		{
			for (int state = 0; state < aiWeightsPhase1.Length; state++)
			{
				aiWeightsPhase1[state] = 1f;
			}
			for (int state = 0; state < aiWeightsPhase2.Length; state++)
			{
				aiWeightsPhase2[state] = 1f;
			}
		}


		public void SetPosition()
		{
			NPC.scale = transform.GetScale();
			NPC.width = (int)(NPC.scale * 64);
			NPC.height = (int)(NPC.scale * 64);
			NPC.Center = worldPosition + transform.GetPosition() * ARENA_RADIUS;
			NPC.rotation = transform.GetRotation();
		}

		private void ClearProjectiles()
        {
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				if (Main.projectile[i].type == ProjectileType<HyperbolicBolt>() || Main.projectile[i].type == ProjectileType<HyperbolicRay>() || Main.projectile[i].type == ProjectileType<HyperbolicAperiogon>() || Main.projectile[i].type == ProjectileType<SelfsimilarSentinelCollision>())
				{
					Main.projectile[i].Kill();
				}
			}
		}

		private int DeathAnimationSegmentsLeft(float ai1)
        {
			float progress = (240 - ai1 - 1) / 240f;
			float continuousVal = progress * (progress + 0.1f) / 1.1f;
			return Math.Min(9, (int)(Math.Floor(10 * continuousVal)));
		}



        public override bool CheckActive()
        {
			return false;
        }

        private HyperbolicTransform GetClosestNode(HyperbolicTransform target)
        {
			HyperbolicTransform outTransform = transform;
			HyperbolicTransform newTransform = outTransform;
			float minDistance = outTransform.DistanceTo(target);

			bool trying = true;
			while (trying)
			{
				trying = false;
				for (int i = 0; i < 3; i++)
				{
					HyperbolicTransform testTransform = outTransform * HyperbolicTransform.Rotation(i * MathHelper.TwoPi / 3 + MathHelper.Pi) * HyperbolicTransform.Translation(maxArmLength);
					float myDistance = testTransform.DistanceTo(target);
					if (myDistance < minDistance)
					{
						minDistance = myDistance;
						newTransform = testTransform;
						trying = true;
					}
				}
				outTransform = newTransform;
			}

			return outTransform;
		}

		private List<HyperbolicTransform> GetVisibleSegments(float minScaleValue = 0.01f, bool onlyEndpoints = false)
		{
			List<HyperbolicTransform> output = new List<HyperbolicTransform>();
			foreach((HyperbolicTransform transform, float scale, float scaleFactor) tuple in GetVisibleSegmentsScales(minScaleValue, onlyEndpoints: onlyEndpoints))
            {
				output.Add(tuple.transform);
            }
			return output;
		}

		private float ArmLength()
		{
			if (NPC.ai[0] == -2)
            {
				return maxArmLength;
            }
			else if (NPC.ai[0] < 5)
			{
				return 0.2f;
			}
			else if (NPC.ai[0] == 5)
			{
				const float maxTimer = 120;
				float progress = Math.Min(1, (NPC.ai[1] + 1) / maxTimer);
				if (NPC.ai[0] != 5) progress = 0;

				float totalLength = progress / (1 - progress);

				if (totalLength > 1) return maxArmLength;
				else return (maxArmLength - 0.2f) * totalLength + 0.2f;
			}
			else
            {
				return maxArmLength;
            }
		}

		public List<(HyperbolicTransform transform, float scale, float scaleFactor)> GetVisibleSegmentsScales(float minScaleValue = 0.01f, int depthCap = 30, bool onlyEndpoints = false)
		{
			float armLength = ArmLength();
			int numLayers;
			int trueNumLayers = 0;

			//set arm length and numLayers
			if (NPC.ai[0] == -2)
            {
				//death animation
				numLayers = DeathAnimationSegmentsLeft(NPC.ai[1]);
            }
			else if (NPC.ai[0] < 5)
			{
				float amountThroughFirstPhase = Math.Min(1, Math.Max(0, (1 - NPC.life / (float)NPC.lifeMax) * 2));

				trueNumLayers = (int)(-Math.Log(1 - amountThroughFirstPhase) / Math.Log(2)) + 1;
				numLayers = Math.Min(6, trueNumLayers);
			}
			else if (NPC.ai[0] == 5)
			{
				numLayers = 8;
			}
			else
			{
				//this can afford to be large because we prune branches that get too small
				numLayers = 30;
			}
			numLayers = Math.Min(numLayers, depthCap);

			float ScalingFactor(int depth)
			{
				if (NPC.ai[0] == -2)
				{
					return 1f;
				}
				if (NPC.ai[0] < 5)
				{
					if (depth == trueNumLayers - 1)
					{
						float amountThroughFirstPhase = Math.Min(1, Math.Max(0, (1 - NPC.life / (float)NPC.lifeMax) * 2));
						return 0.5f * (float)((-Math.Log(1 - amountThroughFirstPhase) / Math.Log(2) + 1) - trueNumLayers);
					}
					return 0.5f;
				}
				else if (NPC.ai[0] == 5)
				{
					const float maxTimer = 120;
					float progress = Math.Min(1, (NPC.ai[1] + 1) / maxTimer);
					if (NPC.ai[0] != 5) progress = 0;

					float totalLength = progress / (1 - progress);

					if (totalLength > depth + 1) return 1f;
					else if (totalLength < depth) return 0.5f;
					else return (totalLength - depth) * 0.5f + 0.5f;
				}
				else
				{
					return 1f;
				}
			}

			List<(HyperbolicTransform transform, float scale, float scaleFactor)> output = new List<(HyperbolicTransform transform, float scale, float scaleFactor)>();

			void GetSegments(HyperbolicTransform localTransform, float scale, int iterations, int depth)
			{
				float scalingFactor = ScalingFactor(depth);

				if (iterations > 0)
				{
					for (int i = 0; i < 2; i++)
					{
						HyperbolicTransform newTransform = localTransform * HyperbolicTransform.Rotation((i * 2 - 1) * MathHelper.TwoPi / 3 + MathHelper.Pi) * HyperbolicTransform.Translation(armLength * scale * scalingFactor);

						int newIterations = (newTransform.GetScale() * scalingFactor > localTransform.GetScale()) ? iterations : iterations - 1;

						//only continue if we're either big enough or getting bigger
						if (newIterations == iterations || newTransform.GetScale() > minScaleValue)
							GetSegments(newTransform, scale * scalingFactor, newIterations, depth + 1);
					}
				}

				//only add if we're big enough
				if (localTransform.GetScale() > minScaleValue)
				{
					if (!onlyEndpoints || depth == numLayers)
						output.Add((localTransform, scale * ScalingFactor(depth - 1) / ScalingFactor(0), ScalingFactor(depth - 1) / ScalingFactor(0)));
				}
			}

			//add stuff
			if (numLayers > 0)
			{
				for (int i = 0; i < 3; i++)
				{
					GetSegments(transform * HyperbolicTransform.Rotation(i * MathHelper.TwoPi / 3 + MathHelper.Pi) * HyperbolicTransform.Translation(armLength * ScalingFactor(0)), ScalingFactor(0), numLayers - 1, 1);
				}
			}

			if (!onlyEndpoints || 0 == numLayers)
				output.Add((transform, 1f, 1f));

			return output;
		}


		private static readonly int[] phase2SecondaryFrameAnimation = {
			0, 
			1, 2, 3, 4, 5, 5, 5, 4, 3, 2, 1,
			0, 
			6, 7, 8, 9, 10, 10, 10, 9, 8, 7, 6,
			0, 
			11, 12, 13, 14, 15, 15, 15, 14, 13, 12, 11 };
		private static readonly int[] arcChainTelegraphAnimation = {
			1, 2, 3, 4, 4, 3, 2, 1
		};



		private HyperbolicTransform lookingTransform = HyperbolicTransform.Identity;
		private float lookingRotation = 0f;

		private Rectangle secondaryFrame = new Rectangle(0, 0, 78, 78);
		private int secondaryFrameCounter = 0;

		private Rectangle arcChainTextureFrame = new Rectangle(0, 0, 82, 36);

		private float teleportFlashTime;
		const float maxTeleportFlashTime = 10;


		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			NPC.frame.Width = 78;

			Texture2D texture = TextureAssets.Npc[NPC.type].Value;
			Rectangle frame = NPC.frame;
			Vector2 center = new Vector2(NPC.frame.Width, NPC.frame.Height) * 0.5f;

			Texture2D spikeTexture = ModContent.Request<Texture2D>("Polarities/NPCs/SelfsimilarSentinel/SelfsimilarSentinelSpike").Value;
			Rectangle spikeTextureFrame = spikeTexture.Frame();
			Vector2 spikeTextureCenter = new Vector2(21, 84);

			Texture2D arcChainTexture = ModContent.Request<Texture2D>("Polarities/NPCs/SelfsimilarSentinel/SelfsimilarSentinelChain").Value;
			Vector2 arcChainTextureCenter = new Vector2(arcChainTextureFrame.Width * 0.5f - 1, arcChainTextureFrame.Height * 0.5f);

			Texture2D arcChainGlowTexture = ModContent.Request<Texture2D>("Polarities/NPCs/SelfsimilarSentinel/SelfsimilarSentinelChainGlow").Value;
			Rectangle arcChainGlowFrame = arcChainGlowTexture.Frame();
			Vector2 arcChainGlowCenter = arcChainGlowFrame.Center();

			Texture2D glowAuraTexture = ModContent.Request<Texture2D>("Polarities/Textures/Glow256").Value;
			Rectangle glowAuraFrame = glowAuraTexture.Frame();
			Vector2 glowAuraCenter = glowAuraFrame.Center();


			float npcAlpha = 1 - NPC.alpha / 255f;


			bool drawMain = true;
			if (NPC.ai[0] == -1 && NPC.ai[1] < 360) drawMain = false;

			if (drawMain)
			{
				const float minScaleValue = 0.01f;

				//draw glow behind main sentinel
				if (NPC.ai[0] >= 5 || NPC.ai[0] == -2)
				{
					float alpha = 0.2f * (NPC.ai[0] == 5 ? (NPC.ai[1] / 120f) : 1) * (NPC.ai[0] == -2 ? (1 - NPC.ai[1] / 240f) : 1);
					spriteBatch.Draw(glowAuraTexture, transform.GetPosition() * ARENA_RADIUS + worldPosition - Main.screenPosition, glowAuraFrame, new Color(247, 173, 255) * (alpha * npcAlpha), transform.GetRotation(), glowAuraCenter, transform.GetScale(), SpriteEffects.None, 0f);
				}

				void DrawArcChain(HyperbolicTransform localTransform, float scale, float length)
				{
					bool chainIsLaser = arcChainTextureFrame.Y >= arcChainTextureFrame.Height * 5;

					if (chainIsLaser)
					{
						float segmentScale = HyperbolicTransform.Translation(Math.Min(localTransform.RayDistanceTo(HyperbolicTransform.Identity), (localTransform * HyperbolicTransform.Translation(length) * HyperbolicTransform.Rotation(MathHelper.Pi)).RayDistanceTo(HyperbolicTransform.Identity))).GetScale();

						if (segmentScale * scale >= 0.5f)
						{
							const float arcChainGlowStretching = 7f;
							const int arcChainGlowStep = 4;

							//offset because otherwise it shows up on the other side of the node
							for (int i = 3; i < length * ARENA_RADIUS / scale / arcChainGlowStretching - 2; i += arcChainGlowStep)
							{
								HyperbolicTransform pointTransform = localTransform * HyperbolicTransform.Translation(i / (float)ARENA_RADIUS * scale * arcChainGlowStretching);
								spriteBatch.Draw(arcChainGlowTexture, pointTransform.GetPosition() * ARENA_RADIUS + worldPosition - Main.screenPosition, arcChainGlowFrame, new Color(247, 173, 255) * (0.1f * npcAlpha), pointTransform.GetRotation(), arcChainGlowCenter, new Vector2(arcChainGlowStretching, 1) * pointTransform.GetScale() * scale, SpriteEffects.None, 0f);
							}
						}
					}

					float arcChainStretching = 1f;

					//don't question this value
					const float chainOffset = 0.75f;

					for (int i = 0; i < length * ARENA_RADIUS / scale / arcChainStretching; i += (arcChainTextureFrame.Width - 2))
					{
						HyperbolicTransform pointTransform = localTransform * HyperbolicTransform.Translation((i + chainOffset) / (float)ARENA_RADIUS * scale * arcChainStretching);

						float alphaMult = (pointTransform.GetScale() * scale - minScaleValue) > 0.1f ? 1 : Math.Max(0, (pointTransform.GetScale() * scale - minScaleValue) / 0.1f);

						spriteBatch.Draw(arcChainTexture, pointTransform.GetPosition() * ARENA_RADIUS + worldPosition - Main.screenPosition, arcChainTextureFrame, Color.White * (alphaMult * npcAlpha), pointTransform.GetRotation(), arcChainTextureCenter, new Vector2(arcChainStretching, 1) * pointTransform.GetScale() * scale, SpriteEffects.None, 0f);
					}
				}

				float armLength = ArmLength();
				List<(HyperbolicTransform transform, float scale, float scaleFactor)> transformScaleList = GetVisibleSegmentsScales(minScaleValue);
				foreach ((HyperbolicTransform transform, float scale, float scaleFactor) tuple in transformScaleList)
				{
					if (!transform.Equals(tuple.transform))
					{
						DrawArcChain(tuple.transform * HyperbolicTransform.Rotation(MathHelper.Pi), tuple.scale / tuple.scaleFactor, armLength * tuple.scale / tuple.scaleFactor);
					}
				}

				foreach ((HyperbolicTransform transform, float scale, float scaleFactor) tuple in transformScaleList)
				{
					float alphaMult = (tuple.transform.GetScale() * tuple.scale - minScaleValue) > 0.1f ? 1 : Math.Max(0, (tuple.transform.GetScale() * tuple.scale - minScaleValue) / 0.1f);

					for (int i = 0; i < 3; i++)
					{
						spriteBatch.Draw(spikeTexture, tuple.transform.GetPosition() * ARENA_RADIUS + worldPosition - Main.screenPosition, spikeTextureFrame, Color.White * (alphaMult * npcAlpha), tuple.transform.GetRotation() + i * MathHelper.TwoPi / 3 + MathHelper.PiOver2, spikeTextureCenter, tuple.transform.GetScale() * tuple.scale, SpriteEffects.None, 0f);
					}
					Rectangle usedFrame = transform.Equals(tuple.transform) ? frame : secondaryFrame;

					float drawRotation = tuple.transform.AngleTo(lookingTransform * tuple.transform) + tuple.transform.GetRotation() + lookingRotation - MathHelper.PiOver2;
					spriteBatch.Draw(texture, tuple.transform.GetPosition() * ARENA_RADIUS + worldPosition - Main.screenPosition, usedFrame, Color.White * (alphaMult * npcAlpha), drawRotation, center, tuple.transform.GetScale() * tuple.scale, SpriteEffects.None, 0f);
				}

				//teleport flash
				if (teleportFlashTime > 0)
				{
					float alpha = teleportFlashTime / maxTeleportFlashTime;
					spriteBatch.Draw(glowAuraTexture, transform.GetPosition() * ARENA_RADIUS + worldPosition - Main.screenPosition, glowAuraFrame, Color.White * (alpha * npcAlpha), transform.GetRotation(), glowAuraCenter, transform.GetScale(), SpriteEffects.None, 0f);
				}
			}

			//phase 1 circle arena drawing code
			float arenaAlpha = 1f;
			if (!drawMain) arenaAlpha = NPC.ai[1] / 360f;
			if (NPC.ai[0] <= 5 && NPC.ai[0] != -2)
			{
				float circleRadius = ARENA_RADIUS;
				texture = ModContent.Request<Texture2D>(Polarities.CallShootProjectile).Value;
				frame = new Rectangle(0, 0, 1, 1);

				for (int i = 0; i < circleRadius * MathHelper.TwoPi; i++)
				{
					spriteBatch.Draw(texture, worldPosition - Main.screenPosition + new Vector2(circleRadius, 0).RotatedBy(i / circleRadius), frame, Color.LightPink * (arenaAlpha * npcAlpha), i / circleRadius, new Vector2(0.5f), 1f, SpriteEffects.None, 0f);
				}
			}

			return false;
		}

		public override void DrawBehind(int index)
		{
			if (NPC.ai[0] >= 5 || NPC.ai[0] == -2)
			{
				float radius = ARENA_RADIUS;
				if (NPC.ai[0] == 5) radius *= (120 / (NPC.ai[1] + 1));
				else if (NPC.ai[0] == -2 && NPC.ai[1] > 120) radius *= (122 / (242 - NPC.ai[1]));
				DrawLayer.AddDraw<DrawLayerBeforeScreenObstruction>(new SentinelArenaDraw(worldPosition, radius));

				float progress = 1f;
				if (NPC.ai[0] == 5) progress = NPC.ai[1] / 120f;
				else if (NPC.ai[0] == -2) progress = 1 - NPC.ai[1] / 240f;
				DrawLayer.AddDraw<DrawLayerBehindWalls>(new SentinelBackgroundDraw(progress));
			}
		}

		public override bool CheckDead()
		{
			if (NPC.ai[0] == -2)
			{
				if (!PolaritiesSystem.downedSelfsimilarSentinel)
				{
					PolaritiesSystem.downedSelfsimilarSentinel = true;
					if (Main.netMode == NetmodeID.Server)
					{
						NetMessage.SendData(MessageID.WorldData); // Immediately inform clients of new world state.
					}
				}

				return true;
			}

			NPC.ai[0] = -2;
			NPC.ai[1] = 0;
			NPC.life = NPC.lifeMax;
			NPC.dontTakeDamage = true;
			return false;
		}

		public override void OnKill()
		{
			//if (Main.rand.NextBool(10) || NPC.GetGlobalNPC<PolaritiesNPC>().noHit)
			//{
			//	Item.NewItem(NPC.getRect(), ItemType<SelfsimilarSentinelTrophy>());
			//}

			//if (NPC.GetGlobalNPC<PolaritiesNPC>().noHit)
			//{
			//	//npc.DropItemInstanced(npc.position, npc.Size, ItemType<FlawlessDrop>());
			//}

			//if (Main.expertMode)
			//{
			//	NPC.DropBossBags();
			//	if (Main.rand.NextBool(4))
			//	{
			//		//Item.NewItem(npc.getRect(), ItemType<SentinelPet>());
			//	}
			//}
			//else
			//{
			//	if (Main.rand.NextBool(7))
			//	{
			//		Item.NewItem(NPC.getRect(), ItemType<SelfsimilarSentinelMask>());
			//	}
			//}
		}

		public override void BossLoot(ref string name, ref int potionType)
		{
			potionType = ItemID.GreaterHealingPotion;
		}

		/*public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
		{
			if (!File.Exists(Main.SavePath + Path.DirectorySeparatorChar + "SelfsimilarSentinel.png"))
			{
				spriteBatch.End();
				spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, (Effect)null, Main.Transform);

				var capture = new RenderTarget2D(spriteBatch.GraphicsDevice, Main.screenWidth * 2, Main.screenWidth * 2, false, Main.spriteBatch.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

				spriteBatch.GraphicsDevice.SetRenderTarget(capture);
				spriteBatch.GraphicsDevice.Clear(Color.Transparent);

				transform = HyperbolicTransform.Rotation(-MathHelper.PiOver2);
				lookingTransform = HyperbolicTransform.LimitRotation(0.01f, MathHelper.PiOver2);
				lookingRotation = MathHelper.PiOver2;
				PreDraw(spriteBatch, drawColor);

				spriteBatch.End();
				spriteBatch.GraphicsDevice.SetRenderTarget(null);

				var stream = File.Create(Main.SavePath + Path.DirectorySeparatorChar + "SelfsimilarSentinel.png");
				capture.SaveAsPng(stream, capture.Width, capture.Height);
				stream.Dispose();
				capture.Dispose();

				spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, (Effect)null, Main.Transform);
			}
		}*/
	}

	public class SentinelBackgroundDraw : IDrawType
	{
		private float progress;

		public SentinelBackgroundDraw(float _progress)
		{
			progress = _progress;
		}

        public void Draw()
        {
			Texture2D texture = ModContent.Request<Texture2D>("Polarities/Projectiles/CallShootProjectile").Value;
			Rectangle frame = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

			Main.spriteBatch.Draw(texture, frame, new Color(1,2,24) * progress);
		}
    }

	public class SentinelArenaDraw : IDrawType
    {
		private Vector2 worldPosition;
		private float radius;

		static Texture2D sentinelArena;

		public SentinelArenaDraw(Vector2 _worldPosition, float _radius)
        {
			worldPosition = _worldPosition;
			radius = _radius;
        }

        public void Draw()
		{
			if (sentinelArena == null) sentinelArena = ModContent.Request<Texture2D>("Polarities/NPCs/SelfsimilarSentinel/SentinelArena").Value; //ArenaSpotlight
			Rectangle circleFrame = sentinelArena.Frame();
			Vector2 circleCenter = circleFrame.Center();

			float scale = radius / circleCenter.X;

			Vector2 extraTopLeft = worldPosition - Main.screenPosition - circleCenter * scale;
			Rectangle extendedFrame = new Rectangle((int)(- extraTopLeft.X / scale), (int)(- extraTopLeft.Y / scale), (int)Math.Ceiling(Main.screenWidth / scale), (int)Math.Ceiling(Main.screenHeight / scale));
			Vector2 drawCenter = extraTopLeft / scale + circleCenter;
			Main.spriteBatch.Draw(sentinelArena, worldPosition - Main.screenPosition, extendedFrame, Color.Black, 0f, drawCenter, scale, SpriteEffects.None, 0f);
		}
	}

    public class SelfsimilarSentinelCollision : ModProjectile
	{
		public override string Texture => "Polarities/NPCs/SelfsimilarSentinel/SelfsimilarSentinel";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Selfsimilar Sentinel");
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 64;
			Projectile.height = 64;

			Projectile.timeLeft = 1;
			Projectile.penetrate = 1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.hide = true;
		}

		public override void AI()
		{
			NPC owner = Main.npc[(int)Projectile.ai[0]];
			if (!owner.active)
			{
				Projectile.Kill();
				return;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			const float minDamagingScale = 0.05f;

			NPC owner = Main.npc[(int)Projectile.ai[0]];
			if (owner.ai[0] == 5 || owner.ai[0] == -2) return false;
			else if (owner.ai[0] < 5)
			{
				Vector2 worldPosition = (owner.ModNPC as SelfsimilarSentinel).worldPosition;
				List<(HyperbolicTransform transform, float scale, float scaleFactor)> segmentScales = (owner.ModNPC as SelfsimilarSentinel).GetVisibleSegmentsScales(0.01f, depthCap: 3);
				foreach((HyperbolicTransform transform, float scale, float scaleFactor) tuple in segmentScales)
				{
					Vector2 transformPos = tuple.transform.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition;
					float transformRot = tuple.transform.GetRotation();
					float transformScale = tuple.transform.GetScale();

					if (tuple.scale * transformScale > minDamagingScale)
					{
						if (
							ModUtils.CheckAABBvTriangle(
								targetHitbox,
								transformPos + new Vector2(60 * transformScale * tuple.scale, 0).RotatedBy(transformRot),
								transformPos + new Vector2(60 * transformScale * tuple.scale, 0).RotatedBy(transformRot + MathHelper.TwoPi / 3),
								transformPos + new Vector2(60 * transformScale * tuple.scale, 0).RotatedBy(transformRot - MathHelper.TwoPi / 3)
								)
							)
						{
							return true;
						}
					}
				}
				return false;
            }
			else
            {
				Vector2 worldPosition = (owner.ModNPC as SelfsimilarSentinel).worldPosition;
				HyperbolicTransform transform = (owner.ModNPC as SelfsimilarSentinel).transform;

				bool doArcs = false;

				if ((owner.ai[0] == 6 && owner.ai[1] >= 60) || (owner.ai[0] == 10 && owner.ai[1] >= 120 && owner.ai[1] < 540)) doArcs = true;

				Vector2 targetPosition = (targetHitbox.Center() - worldPosition) / SelfsimilarSentinel.ARENA_RADIUS;
				if (targetPosition.Length() > 0.99f)
				{
					targetPosition.Normalize();
					targetPosition *= 0.99f;
				}
				HyperbolicTransform target = HyperbolicTransform.FromPosition(targetPosition);

				//transform as close to the player as possible
				HyperbolicTransform newTransform = transform;
				float minDistance = transform.DistanceTo(target);
				bool trying = true;
				while (trying)
				{
					trying = false;
					for (int i = 0; i < 3; i++)
					{
						HyperbolicTransform testTransform = transform * HyperbolicTransform.Rotation(i * MathHelper.TwoPi / 3 + MathHelper.Pi) * HyperbolicTransform.Translation(SelfsimilarSentinel.maxArmLength);
						float myDistance = (testTransform).DistanceTo(target);
						if (myDistance < minDistance)
						{
							minDistance = myDistance;
							newTransform = testTransform;
							trying = true;
						}
					}

					if (newTransform.GetScale() > minDamagingScale || transform.GetScale() < minDamagingScale)
					{
						transform = newTransform;
					}
					else
                    {
						trying = false;
                    }
				}

				Vector2 transformPos = transform.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition;
				float transformRot = transform.GetRotation();
				float scale = transform.GetScale();

				if (scale > minDamagingScale)
					if (ModUtils.CheckAABBvTriangle(targetHitbox, transformPos + new Vector2(60 * scale, 0).RotatedBy(transformRot), transformPos + new Vector2(60 * scale, 0).RotatedBy(transformRot + MathHelper.TwoPi / 3), transformPos + new Vector2(60 * scale, 0).RotatedBy(transformRot - MathHelper.TwoPi / 3))) return true;

				if (doArcs)
				{
					for (int i = 0; i < 3; i++)
					{
						HyperbolicTransform localTransform = transform * HyperbolicTransform.Rotation(i * MathHelper.TwoPi / 3 + MathHelper.Pi) * HyperbolicTransform.Translation(SelfsimilarSentinel.maxArmLength);

						//circle also contains inverse of transformPos
						Vector2 tPosInverse = worldPosition + SelfsimilarSentinel.ARENA_RADIUS * SelfsimilarSentinel.ARENA_RADIUS * (transformPos - worldPosition) / (transformPos - worldPosition).LengthSquared();
						Vector2 arcEnd = localTransform.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition;

						Line line1 = new Line(2 * tPosInverse.X - 2 * transformPos.X, 2 * tPosInverse.Y - 2 * transformPos.Y, transformPos.LengthSquared() - tPosInverse.LengthSquared());
						Line line2 = new Line(2 * tPosInverse.X - 2 * arcEnd.X, 2 * tPosInverse.Y - 2 * arcEnd.Y, arcEnd.LengthSquared() - tPosInverse.LengthSquared());

						Vector2 arcCenter = line1.Intersection(line2);

						float transformCurving = localTransform.GetRotation() - localTransform.GetPosition().ToRotation();
						if (transformCurving > MathHelper.Pi) transformCurving -= MathHelper.TwoPi;
						if (transformCurving < -MathHelper.Pi) transformCurving += MathHelper.TwoPi;
						int arcDirection = transformCurving > 0 ? -1 : 1;

						float arcAngle = ModUtils.AngleBetween(transformPos - arcCenter, arcEnd - arcCenter) * arcDirection;
						if (arcAngle > MathHelper.Pi) arcAngle -= MathHelper.TwoPi;
						if (arcAngle < -MathHelper.Pi) arcAngle += MathHelper.TwoPi;

						Arc connectionArc = new Arc(arcCenter, transformPos, arcAngle);
						if (ModUtils.CheckAABBvArc(targetHitbox, connectionArc)) return true;
					}
				}

				return false;
            }
		}

		public override bool ShouldUpdatePosition()
		{
			return false;
		}
    }
	
	public class HyperbolicBolt : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Hyperbolic Bolt");

			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;

			/*Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 200, 1, false, SurfaceFormat.Color);
			List<Color> list = new List<Color>();
			for (int j = 0; j < texture.Height; j++)
			{
				for (int i = 0; i < texture.Width; i++)
				{
					float y = (i + 1) / (float)texture.Width;

					int r = 255;
					int g = 255;
					int b = 255;
					int alpha = (int)(255 * y);

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "HyperbolicBoltTelegraph.png", FileMode.Create), texture.Width, texture.Height);*/
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 10;
			Projectile.height = 10;

			Projectile.alpha = 0;
			Projectile.timeLeft = 600;
			Projectile.penetrate = 1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public HyperbolicTransform transform = HyperbolicTransform.Identity;
		public HyperbolicTransform[] oldTransform;
		public Vector2 worldPosition;

		public static Projectile NewProjectile(IEntitySource source, Vector2 worldPosition, HyperbolicTransform transform, Vector2 velocity, int damage, float knockBack, int timeLeft = 600, float ai0 = 0, float ai1 = 0, int extraUpdates = 0)
		{
			try
			{
				Projectile p = Main.projectile[Projectile.NewProjectile(source, Vector2.Zero, velocity, ProjectileType<HyperbolicBolt>(), damage, knockBack, Main.myPlayer, ai0, ai1)];
				(p.ModProjectile as HyperbolicBolt).worldPosition = worldPosition;
				(p.ModProjectile as HyperbolicBolt).transform = transform;
				(p.ModProjectile as HyperbolicBolt).SetPosition();
				p.timeLeft = timeLeft;
				p.extraUpdates = extraUpdates;

				//initialize transform trail
				(p.ModProjectile as HyperbolicBolt).Initialize();

				return p;
			}
			catch (Exception e)
            {
				return null;
            }
		}

		public void Initialize()
        {
			oldTransform = new HyperbolicTransform[10];
			for (int i = 0; i < oldTransform.Length; i++)
            {
				oldTransform[i] = transform;
            }
        }

		public override void AI()
		{
			//behavior depends on ai[0]:
			switch (Projectile.ai[0])
			{
				case 0:
					//decelerate, then accelerate, to give player time to react
					if (Projectile.localAI[1] >= 5 && Projectile.localAI[1] < 15)
					{
						Projectile.velocity *= 0.8f;
					}
					else if (Projectile.localAI[1] >= 30 && Projectile.localAI[1] < 40)
					{
						Projectile.velocity /= 0.8f;
					}
					break;
				case 1:
					//do nothing of consequence
					break;
				case 2:
					//decelerate, then accelerate, to give player time to react
					if (Projectile.localAI[1] >= 5 && Projectile.localAI[1] < 15)
					{
						Projectile.velocity *= 0.8f;
					}
					else if (Projectile.localAI[1] >= 30 && Projectile.localAI[1] < 40)
					{
						Projectile.velocity /= 0.8f;
					}

					float trueSpeed = Projectile.velocity.Length() / SelfsimilarSentinel.ARENA_RADIUS;
					float maxAngle = 2 * (float)Math.Acos(1 / (float)Math.Cosh(trueSpeed));
					//curve according to ai[1]
					transform = transform * HyperbolicTransform.Rotation(Projectile.ai[1] * maxAngle);
					break;
				case 3:
					//decelerate, then accelerate, to give player time to react
					if (Projectile.localAI[1] >= 5 && Projectile.localAI[1] < 15)
					{
						Projectile.velocity *= 0.8f;
					}
					else if (Projectile.localAI[1] >= 30 && Projectile.localAI[1] < 40)
					{
						Projectile.velocity /= 0.8f;
					}

					//home in on player, turning with at most horocyclic curvature
					Player player = Main.LocalPlayer;
					Vector2 targetPosition = (player.Center - worldPosition) / SelfsimilarSentinel.ARENA_RADIUS;
					if (targetPosition.Length() > 0.99f)
					{
						targetPosition.Normalize();
						targetPosition *= 0.99f;
					}
					HyperbolicTransform target = HyperbolicTransform.FromPosition(targetPosition);

					float angleGoal = transform.AngleTo(target);
					while (angleGoal > MathHelper.Pi) angleGoal -= MathHelper.TwoPi;
					while (angleGoal < -MathHelper.Pi) angleGoal += MathHelper.TwoPi;

					trueSpeed = Projectile.velocity.Length() / SelfsimilarSentinel.ARENA_RADIUS;
					maxAngle = 2 * (float)Math.Acos(1 / (float)Math.Cosh(trueSpeed));

					if (angleGoal > maxAngle) angleGoal = maxAngle;
					if (angleGoal < -maxAngle) angleGoal = -maxAngle;

					transform = transform * HyperbolicTransform.Rotation(angleGoal);
					break;
				case 4:
					//have a telegraph, start moving after half a second
					if (Projectile.localAI[1] == 0)
					{
						Projectile.velocity /= 1000f;
					}
					if (Projectile.localAI[1] == 30)
					{
						Projectile.velocity *= 1000f;
					}
					break;
				case 5:
					//decelerate, then accelerate, to give player time to react
					if (Projectile.localAI[1] == 0)
					{
						Projectile.velocity *= (float)Math.Pow(0.8f, 20);
					}
					if (Projectile.localAI[1] < 20)
					{
						Projectile.velocity /= 0.8f;
					}

					trueSpeed = Projectile.velocity.Length() / SelfsimilarSentinel.ARENA_RADIUS;
					maxAngle = 2 * (float)Math.Acos(1 / (float)Math.Cosh(trueSpeed));
					//curve according to ai[1]
					transform = transform * HyperbolicTransform.Rotation(Projectile.ai[1] * maxAngle);
					break;
			}

			Projectile.localAI[1]++;

			//update hyperbolic motion
			transform = transform * HyperbolicTransform.FromVelocity(Projectile.velocity / SelfsimilarSentinel.ARENA_RADIUS);

			//update transform trail
			for (int i = oldTransform.Length - 1; i > 0; i--)
            {
				oldTransform[i] = oldTransform[i - 1];
            }
			oldTransform[0] = transform;

			//set position in the world
			SetPosition();
		}

		public void SetPosition()
		{
			Projectile.scale = transform.GetScale();
			Projectile.width = (int)(Projectile.scale * 10);
			Projectile.height = (int)(Projectile.scale * 10);
			Projectile.Center = worldPosition + transform.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS;
			Projectile.rotation = transform.GetRotation() + Projectile.velocity.ToRotation();
		}

        public override bool ShouldUpdatePosition()
        {
			return false;
        }

        public override bool? CanDamage()/* tModPorter Suggestion: Return null instead of true */
		{
			//projectile must be big enough to hit the player
			return (Projectile.scale >= 0.025f);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Rectangle frame = texture.Frame();
			Vector2 center = frame.Center();

			Texture2D glowTexture = ModContent.Request<Texture2D>($"Polarities/Textures/Glow58").Value;
			Rectangle glowFrame = glowTexture.Frame();
			Vector2 glowCenter = glowFrame.Center();

			const float minScale = 0.1f;

			//bloom effect
			if (transform.GetScale() > minScale)
				Main.spriteBatch.Draw(glowTexture, transform.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition - Main.screenPosition, glowFrame, new Color(247, 173, 255) * 0.5f, transform.GetRotation(), glowCenter, Math.Max(minScale, transform.GetScale()) * 0.5f, SpriteEffects.None, 0f);

			//telegraph
			if (Projectile.ai[0] == 4 && Projectile.localAI[1] < 30)
			{
				Texture2D telegraphTexture = ModContent.Request<Texture2D>("Polarities/NPCs/SelfsimilarSentinel/HyperbolicBoltTelegraph").Value;

				float numSegments = (1 - minScale / transform.GetScale()) * oldTransform.Length;
				for (int i = 0; i < numSegments - 1; i += 2)
				{
					Rectangle telegraphFrame = telegraphTexture.Frame(oldTransform.Length / 2, 1, i / 2, 0);

					Vector2 transformPosA = (transform * HyperbolicTransform.Translation(i * 24 / (float)SelfsimilarSentinel.ARENA_RADIUS)).GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition;
					Vector2 transformPosB = (transform * HyperbolicTransform.Translation((i + 2) * 24 / (float)SelfsimilarSentinel.ARENA_RADIUS)).GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition;

					if ((transformPosA - transformPosB).Length() > 0)
					{
						float progress = (1 - i / (float)oldTransform.Length);
						Color color = new Color(247, 173, 255);
						float alpha = 4 * (1 - Projectile.localAI[1] / 30f) * Projectile.localAI[1] / 30f;
						Main.spriteBatch.Draw(telegraphTexture, transformPosA - Main.screenPosition, telegraphFrame, color * alpha/*(alpha * progress)*/, (transformPosB - transformPosA).ToRotation(), new Vector2(0, 0.5f), new Vector2((transformPosB - transformPosA).Length() / telegraphFrame.Width, 2 * progress * Math.Max(minScale, oldTransform[i].GetScale())), SpriteEffects.None, 0f);
					}
				}
			}
			else
			{

				Texture2D trailTexture = ModContent.Request<Texture2D>(Polarities.CallShootProjectile).Value;
				Rectangle trailFrame = new Rectangle(0, 0, 1, 1);

				//only draw large enough segments
				float numSegments = (1 - minScale / transform.GetScale()) * oldTransform.Length;
				for (int i = 0; i < numSegments - 1; i += 2)
				{
					Vector2 transformPosA = oldTransform[i].GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition;
					Vector2 transformPosB = oldTransform[i + 2].GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition;

					if ((transformPosA - transformPosB).Length() > 0)
					{
						float progress = (1 - i / (float)oldTransform.Length);
						Color color = new Color((int)(243 + 12 * progress), (int)(112 + 143 * progress), 255);
						Main.spriteBatch.Draw(trailTexture, transformPosA - Main.screenPosition, trailFrame, color, (transformPosB - transformPosA).ToRotation(), new Vector2(0, 0.5f), new Vector2((transformPosB - transformPosA).Length(), 4 * progress * Math.Max(minScale, oldTransform[i].GetScale())), SpriteEffects.None, 0f);
					}
				}
			}

			Main.spriteBatch.Draw(texture, transform.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition - Main.screenPosition, frame, Color.White, transform.GetRotation(), center, Math.Max(minScale, transform.GetScale()), SpriteEffects.None, 0f);
			return false;
		}
	}

	public class HyperbolicRay : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Hyperbolic Ray");
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 10;
			Projectile.height = 10;

			Projectile.timeLeft = 120;
			Projectile.penetrate = 1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public HyperbolicTransform transform = HyperbolicTransform.Identity;
		public Vector2 worldPosition;

		public static Projectile NewProjectile(IEntitySource source, Vector2 worldPosition, HyperbolicTransform transform, Vector2 velocity, int damage, float knockBack, int timeLeft = 120, float ai0 = 0, float ai1 = 0)
		{
			try
			{
				Projectile p = Main.projectile[Projectile.NewProjectile(source, Vector2.Zero, velocity, ProjectileType<HyperbolicRay>(), damage, knockBack, Main.myPlayer, ai0, ai1)];
				(p.ModProjectile as HyperbolicRay).worldPosition = worldPosition;
				(p.ModProjectile as HyperbolicRay).transform = transform;
				(p.ModProjectile as HyperbolicRay).SetPosition();

				p.timeLeft = timeLeft;
				p.hostile = false;

				return p;
			}
			catch (Exception e)
			{
				return null;
			}
		}

		public override void AI()
		{
			NPC owner = Main.npc[(int)Projectile.ai[0]];
			if (!owner.active)
            {
				Projectile.Kill();
				return;
            }

			HyperbolicTransform ownerTransform = (owner.ModNPC as SelfsimilarSentinel).transform;

			transform = ownerTransform * HyperbolicTransform.FromVelocity(owner.velocity / SelfsimilarSentinel.ARENA_RADIUS) * ownerTransform.Inverse() * transform;

			if (Projectile.localAI[1] == 0)
            {
				Projectile.localAI[0] = 0.05f;
			}

			bool playSound = false;

			switch (Projectile.ai[1])
			{
				case 0:
					if (Projectile.localAI[1] == 60)
					{
						Projectile.hostile = true;

						for (int i = -80; i <= 80; i++)
						{
							//HyperbolicTransform shotTransform = transform * HyperbolicTransform.LimitRotation(i * 0.125f) * HyperbolicTransform.FromVelocity(new Vector2(4f, 0)) * HyperbolicTransform.Rotation(MathHelper.Pi);
							HyperbolicTransform shotTransform = transform * HyperbolicTransform.LimitRotation((i + (float)Math.Pow(i, 3) * 0.001f) * 0.125f) * HyperbolicTransform.FromVelocity(new Vector2(1.7f, 0)) * HyperbolicTransform.Rotation(MathHelper.Pi);
							if (shotTransform.RayDistanceTo(HyperbolicTransform.Identity) < 2f)
							{
								//HyperbolicBolt.NewProjectile(NPC.GetSource_FromAI(), worldPosition, shotTransform, new Vector2(256, 0), SelfsimilarSentinel.projectileDamage, 0f, ai0: 1f);
								HyperbolicBolt.NewProjectile(Projectile.GetSource_FromAI(), worldPosition, shotTransform, new Vector2(12, 0), SelfsimilarSentinel.projectileDamage, 0f, ai0: -1f);
							}
						}
						HyperbolicHorocycle.NewProjectile(Projectile.GetSource_FromAI(), worldPosition, transform * HyperbolicTransform.Translation(1.7f), new Vector2(-6, 0), SelfsimilarSentinel.projectileDamage, 0f, ai1: Projectile.ai[1]);

						SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion, (transform * HyperbolicTransform.FromVelocity(new Vector2(2f, 0))).GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition);

						playSound = true;
					}

					if (Projectile.localAI[1] >= 60 && Projectile.localAI[1] < 70)
					{
						Projectile.localAI[0] = 0.66f * (Projectile.localAI[1] - 59) / 10f;
					}
					else if (Projectile.timeLeft < 10)
                    {
						Projectile.localAI[0] = 0.66f * Projectile.timeLeft / 10f;
					}
					Projectile.localAI[1]++;
					break;
				case 1:
					if (Projectile.localAI[1] == 60)
					{
						Projectile.hostile = true;

						playSound = true;
					}

					if (Projectile.localAI[1] >= 60 && Projectile.localAI[1] < 70)
					{
						Projectile.localAI[0] = 0.66f * (Projectile.localAI[1] - 59) / 10f;
					}
					else if (Projectile.timeLeft < 10)
					{
						Projectile.localAI[0] = 0.66f * Projectile.timeLeft / 10f;
					}
					Projectile.localAI[1]++;
					break;
				case 2:
					if (Projectile.localAI[1] == 45)
					{
						Projectile.hostile = true;
						Projectile.localAI[0] = 0.66f;

						playSound = true;
					}
					else if (Projectile.timeLeft < 10)
					{
						Projectile.localAI[0] = 0.66f * Projectile.timeLeft / 10f;
					}
					Projectile.localAI[1]++;
					break;
				case 3:
					if (Projectile.localAI[1] == 45)
					{
						Projectile.hostile = true;

						playSound = true;
					}

					if (Projectile.localAI[1] >= 45 && Projectile.localAI[1] < 55)
					{
						Projectile.localAI[0] = 0.66f * (Projectile.localAI[1] - 44) / 10f;
					}
					else if (Projectile.timeLeft < 10)
					{
						Projectile.localAI[0] = 0.66f * Projectile.timeLeft / 10f;
					}
					Projectile.localAI[1]++;
					break;
				case 4:
					if (Projectile.localAI[1] == 60)
					{
						Projectile.hostile = true;

						playSound = true;
					}

					if (Projectile.localAI[1] >= 60 && Projectile.localAI[1] < 70)
					{
						Projectile.localAI[0] = 0.66f * (Projectile.localAI[1] - 59) / 10f;
					}
					else if (Projectile.timeLeft < 10)
					{
						Projectile.localAI[0] = 0.66f * Projectile.timeLeft / 10f;
					}

					transform = ownerTransform;

					Projectile.localAI[1]++;
					break;
				case 5:
					//ray is just a short-lived telegraph
					Projectile.localAI[0] = 0.05f * Projectile.timeLeft / 30f;
					Projectile.localAI[1]++;
					break;
			}

			if (playSound)
            {
				SoundEngine.PlaySound(SoundID.Item67, Projectile.Center);
			}

			//update hyperbolic motion
			transform = transform * HyperbolicTransform.FromVelocity(Projectile.velocity / SelfsimilarSentinel.ARENA_RADIUS);

			//set position in the world
			SetPosition();
		}

		public void SetPosition()
		{
			Projectile.scale = transform.GetScale();
			Projectile.width = (int)(Projectile.scale * 10);
			Projectile.height = (int)(Projectile.scale * 10);
			Projectile.Center = worldPosition + transform.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS;
			Projectile.rotation = transform.GetRotation();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
			//we only collide with the main arc because determining the arcs of the hypercycles and cap is beyond my patience level for now

			Circle worldCircle = new Circle(worldPosition, SelfsimilarSentinel.ARENA_RADIUS);

			//inverse of projectile.Center in worldCircle
			Vector2 invertedProjectileCenter = worldCircle.Center + SelfsimilarSentinel.ARENA_RADIUS * SelfsimilarSentinel.ARENA_RADIUS * (Projectile.Center - worldCircle.Center) / (Projectile.Center - worldCircle.Center).LengthSquared();
			//intersection of the line through projectile.Center of angle transform.GetRotation()+π/2 and the perpendicular bisector of projectile.Center and invertedCenter
			//solves this
			//rayLineCenter.X * Math.Cos(transform.GetRotation()) + rayLineCenter.Y * Math.Sin(transform.GetRotation()) = projectile.Center.X * Math.Cos(transform.GetRotation()) + projectile.Center.Y * Math.Sin(transform.GetRotation())
			//(rayLineCenter.X - projectile.Center.X) * (rayLineCenter.X - projectile.Center.X) + (rayLineCenter.Y - projectile.Center.Y) * (rayLineCenter.Y - projectile.Center.Y) = (rayLineCenter.X - invertedProjectileCenter.X) * (rayLineCenter.X - invertedProjectileCenter.X) + (rayLineCenter.Y - invertedProjectileCenter.Y) * (rayLineCenter.Y - invertedProjectileCenter.Y)
			//rayLineCenter.Y = rayLineCenter.X * (invertedProjectileCenter.X - projectile.Center.X) / (projectile.Center.Y - invertedProjectileCenter.Y) + (projectile.Center.LengthSquared() - invertedProjectileCenter.LengthSquared()) / (2 * (projectile.Center.Y - invertedProjectileCenter.Y))
			float bisectorSlope = (invertedProjectileCenter.X - Projectile.Center.X) / (Projectile.Center.Y - invertedProjectileCenter.Y);
			float bisectorHeight = (Projectile.Center.LengthSquared() - invertedProjectileCenter.LengthSquared()) / (2 * (Projectile.Center.Y - invertedProjectileCenter.Y));
			//rayLineCenter.Y = rayLineCenter.X * bisectorSlope + bisectorHeight
			float rayLineCenterX = (float)(
				((Projectile.Center.X * Math.Cos(transform.GetRotation()) + Projectile.Center.Y * Math.Sin(transform.GetRotation())) - Math.Sin(transform.GetRotation()) * bisectorHeight)
				/
				(Math.Cos(transform.GetRotation()) + Math.Sin(transform.GetRotation()) * bisectorSlope));
			Vector2 rayLineCenter = new Vector2(rayLineCenterX, rayLineCenterX * bisectorSlope + bisectorHeight);

			//rayLine: circle passing through projectile.Center in direction transform.GetRotation() at a right angle to worldCircle
			Circle rayLine = new Circle(rayLineCenter, (Projectile.Center - rayLineCenter).Length());

			float transformCurving = transform.GetRotation() - transform.GetPosition().ToRotation();
			if (transformCurving > MathHelper.Pi) transformCurving -= MathHelper.TwoPi;
			if (transformCurving < -MathHelper.Pi) transformCurving += MathHelper.TwoPi;
			int arcDirection = transformCurving > 0 ? -1 : 1;

			//rayLine.radius = Math.Tan(MathHelper.PiOver2 - arcAngle) * ARENA_RADIUS
			float angleOffset = (worldPosition - rayLine.Center).ToRotation() - (Projectile.Center - rayLine.Center).ToRotation();
			if (angleOffset < -MathHelper.Pi) angleOffset += MathHelper.TwoPi;
			if (angleOffset > MathHelper.Pi) angleOffset -= MathHelper.TwoPi;
			float arcAngle = angleOffset + arcDirection * (MathHelper.PiOver2 - (float)Math.Atan(rayLine.radius / SelfsimilarSentinel.ARENA_RADIUS));

			Arc rayArc = new Arc(rayLine.Center, Projectile.Center, arcAngle);

			return ModUtils.CheckAABBvArc(targetHitbox, rayArc);
        }

        public override bool ShouldUpdatePosition()
		{
			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Rectangle frame = texture.Frame();
			Vector2 center = new Vector2(0, frame.Height * 0.5f);

			Texture2D glowTexture = ModContent.Request<Texture2D>("Polarities/NPCs/SelfsimilarSentinel/HyperbolicRayGlow").Value;
			Rectangle glowFrame = glowTexture.Frame();
			Vector2 glowCenter = new Vector2(0, glowFrame.Height * 0.5f);

			Texture2D capTexture = ModContent.Request<Texture2D>("Polarities/NPCs/SelfsimilarSentinel/HyperbolicRayCap").Value;
			Rectangle capFrame = capTexture.Frame();
			Vector2 capCenter = new Vector2(0, capTexture.Height * 0.5f);


			float verticalScale = Projectile.localAI[0];


			float minScaling = 0.01f;

			float glowStepMultiplier = 1f;
			float glowStepLength = 4;


			float progress = 0;

			//draw glowing
			bool continuing = true;
			while (continuing)
			{
				HyperbolicTransform localTransform = transform * HyperbolicTransform.Translation(glowStepMultiplier * glowStepLength * progress / SelfsimilarSentinel.ARENA_RADIUS);

				Vector2 offset = localTransform.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS;
				Main.spriteBatch.Draw(glowTexture, worldPosition - Main.screenPosition + offset, glowFrame, new Color(247, 173, 255) * 0.04f, localTransform.GetRotation(), glowCenter, new Vector2(glowStepMultiplier, verticalScale) * localTransform.GetScale(), SpriteEffects.None, 0f);

				progress++;
				continuing = localTransform.GetScale() > minScaling || Math.Abs(localTransform.AngleTo(HyperbolicTransform.Identity)) < MathHelper.PiOver2;
			}

			float stepMultiplier = 2f;
			float stepLength = frame.Width - 2;

			//draw cap
			Main.spriteBatch.Draw(capTexture, worldPosition - Main.screenPosition + transform.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS, capFrame, Color.White, transform.GetRotation(), capCenter, new Vector2(1, verticalScale) * transform.GetScale(), SpriteEffects.None, 0f);

			progress = 32 / stepLength / stepMultiplier * verticalScale;

			//draw main laser
			continuing = true;
			while (continuing)
			{
				HyperbolicTransform localTransform = transform * HyperbolicTransform.Translation(stepMultiplier * stepLength * progress / SelfsimilarSentinel.ARENA_RADIUS);

				Vector2 offset = localTransform.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS;
				Main.spriteBatch.Draw(texture, worldPosition - Main.screenPosition + offset, frame, Color.White, localTransform.GetRotation(), center, new Vector2(stepMultiplier, verticalScale) * localTransform.GetScale(), SpriteEffects.None, 0f);

				progress++;
				continuing = localTransform.GetScale() > minScaling || Math.Abs(localTransform.AngleTo(HyperbolicTransform.Identity)) < MathHelper.PiOver2;

			}
			return false;
		}
	}

	public class HyperbolicAperiogon : ModProjectile, IDrawType
	{
        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Hyperbolic Pulse");

			/*Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 4096, 4096, false, SurfaceFormat.Color);
			System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
			for (int i = 0; i < texture.Width; i++)
			{
				for (int j = 0; j < texture.Height; j++)
				{
					float x = (2 * i / (float)(texture.Width - 1) - 1);
					float y = (2 * j / (float)(texture.Width - 1) - 1);

					float distanceSquared = x * x + y * y;

					int r = 255;
					int g = 255;
					int b = 255;
					int alpha = distanceSquared >= 1 ? 0 : 255;

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "HyperbolicAperiogon.png", FileMode.Create), texture.Width, texture.Height);*/

			/*Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 500, 500, false, SurfaceFormat.Color);
			System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
			for (int i = 0; i < texture.Width; i++)
			{
				for (int j = 0; j < texture.Height; j++)
				{
					float x = (2 * i / (float)(texture.Width - 1) - 1);
					float y = (2 * j / (float)(texture.Height - 1) - 1);

					float distanceSquared = x * x + y * y;//(float)Math.Pow(Math.Pow(x, 8) + Math.Pow(y, 8), 1 / 4f);

					int r = 255;
					int g = 255;
					int b = 255;
					float alphaFloat = (Math.Max(0, Math.Min(1, 4 * (1 - distanceSquared))));
					alphaFloat = 1 - (2 - alphaFloat) * alphaFloat;
					int alpha = distanceSquared >= 1 ? 255 : (int)(255 * alphaFloat);

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "SentinelArena.png", FileMode.Create), texture.Width, texture.Height);*/
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 10;
			Projectile.height = 10;

			Projectile.timeLeft = 240;
			Projectile.penetrate = 1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;

			Projectile.hide = true;
		}

        public HyperbolicTransform transform = HyperbolicTransform.Identity;
		public Vector2 worldPosition;

		public static Projectile NewProjectile(IEntitySource source, Vector2 worldPosition, HyperbolicTransform transform, Vector2 velocity, int damage, float knockBack, int timeLeft = 240, float ai0 = 0, float ai1 = 0)
		{
			try
			{
				//if we're too far from the center and we don't matter, return null and don't create a projectile
				HyperbolicTransform transform1 = transform * HyperbolicTransform.Rotation(-MathHelper.Pi / 3) * HyperbolicTransform.Translation(0.5f * SelfsimilarSentinel.maxArmLength) * HyperbolicTransform.Rotation(MathHelper.PiOver2);
				HyperbolicTransform transform2 = transform * HyperbolicTransform.Rotation(MathHelper.Pi / 3) * HyperbolicTransform.Translation(0.5f * SelfsimilarSentinel.maxArmLength) * HyperbolicTransform.Rotation(-MathHelper.PiOver2);
				Vector2 line1Start = transform1.GetPosition();
				float line1Angle = transform1.GetRotation();
				Line line1 = new Line((float)Math.Sin(line1Angle), -(float)Math.Cos(line1Angle), line1Start.Y * (float)Math.Cos(line1Angle) - line1Start.X * (float)Math.Sin(line1Angle));
				Vector2 line2Start = transform2.GetPosition();
				float line2Angle = transform2.GetRotation();
				Line line2 = new Line((float)Math.Sin(line2Angle), -(float)Math.Cos(line2Angle), line2Start.Y * (float)Math.Cos(line2Angle) - line2Start.X * (float)Math.Sin(line2Angle));

				Vector2 horocycleCenter = line1.Intersection(line2);
				float horocycleRadius = (line1Start - horocycleCenter).Length();

				if (horocycleRadius < 0.01f)
				{
					return null;
				}

				Projectile p = Main.projectile[Projectile.NewProjectile(source, Vector2.Zero, velocity, ProjectileType<HyperbolicAperiogon>(), damage, knockBack, Main.myPlayer, ai0, ai1)];
				(p.ModProjectile as HyperbolicAperiogon).worldPosition = worldPosition;
				(p.ModProjectile as HyperbolicAperiogon).transform = transform;
				(p.ModProjectile as HyperbolicAperiogon).SetPosition();

				p.timeLeft = timeLeft;
				p.hostile = false;

				return p;
			}
			catch (Exception e)
			{
				return null;
			}
		}

		public override void AI()
		{
			NPC owner = Main.npc[(int)Projectile.ai[0]];
			if (!owner.active)
			{
				Projectile.Kill();
				return;
			}

			if (Projectile.timeLeft == 30)
            {
				Projectile.hostile = true;

				//play sound at actual center with volume proportional to radius
				HyperbolicTransform transform1 = transform * HyperbolicTransform.Rotation(-MathHelper.Pi / 3) * HyperbolicTransform.Translation(0.5f * SelfsimilarSentinel.maxArmLength) * HyperbolicTransform.Rotation(MathHelper.PiOver2);
				HyperbolicTransform transform2 = transform * HyperbolicTransform.Rotation(MathHelper.Pi / 3) * HyperbolicTransform.Translation(0.5f * SelfsimilarSentinel.maxArmLength) * HyperbolicTransform.Rotation(-MathHelper.PiOver2);
				Vector2 line1Start = transform1.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition;
				float line1Angle = transform1.GetRotation();
				Line line1 = new Line((float)Math.Sin(line1Angle), -(float)Math.Cos(line1Angle), line1Start.Y * (float)Math.Cos(line1Angle) - line1Start.X * (float)Math.Sin(line1Angle));
				Vector2 line2Start = transform2.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition;
				float line2Angle = transform2.GetRotation();
				Line line2 = new Line((float)Math.Sin(line2Angle), -(float)Math.Cos(line2Angle), line2Start.Y * (float)Math.Cos(line2Angle) - line2Start.X * (float)Math.Sin(line2Angle));

				Vector2 horocycleCenter = line1.Intersection(line2);
				float horocycleRadius = (line1Start - horocycleCenter).Length();

				SoundEngine.PlaySound(SoundID.NPCDeath14.WithVolumeScale((float)Math.Sqrt(horocycleRadius / SelfsimilarSentinel.ARENA_RADIUS)), horocycleCenter);
            }
			else if (Projectile.timeLeft == 20)
            {
				Projectile.hostile = false;
            }

			HyperbolicTransform ownerTransform = (owner.ModNPC as SelfsimilarSentinel).transform;

			transform = ownerTransform * HyperbolicTransform.FromVelocity(owner.velocity / SelfsimilarSentinel.ARENA_RADIUS) * ownerTransform.Inverse() * transform;

			//update hyperbolic motion
			transform = transform * HyperbolicTransform.FromVelocity(Projectile.velocity / SelfsimilarSentinel.ARENA_RADIUS);

			//set position in the world
			SetPosition();
		}

		public void SetPosition()
		{
			Projectile.scale = transform.GetScale();
			Projectile.width = (int)(Projectile.scale * 10);
			Projectile.height = (int)(Projectile.scale * 10);
			Projectile.Center = worldPosition + transform.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS;
			Projectile.rotation = transform.GetRotation();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (!Projectile.hostile) return false;

			Vector2 targetPosition = (targetHitbox.Center() - worldPosition) / SelfsimilarSentinel.ARENA_RADIUS;
			if (targetPosition.Length() > 0.99f)
			{
				targetPosition.Normalize();
				targetPosition *= 0.99f;
			}
			HyperbolicTransform target = HyperbolicTransform.FromPosition(targetPosition);

			//transform as close to the player as possible on the horocycle
			HyperbolicTransform closestTransform = transform;
			int direction = closestTransform.AngleTo(target) > 0 ? 1 : -1;
			bool trying = true;
			while (trying)
			{
				trying = false;

				HyperbolicTransform newTransform = closestTransform * HyperbolicTransform.Rotation(-direction * MathHelper.TwoPi / 3 + MathHelper.Pi) * HyperbolicTransform.Translation(SelfsimilarSentinel.maxArmLength) * HyperbolicTransform.Rotation(-direction * MathHelper.TwoPi / 3);

				if (newTransform.DistanceTo(target) < closestTransform.DistanceTo(target))
				{
					closestTransform = newTransform;
					trying = true;
				}
			}

			//If target is contained in the aperiogon, return true
			if (Math.Abs(closestTransform.AngleTo(target)) < MathHelper.Pi / 3) return true;

			Vector2 transformPos = closestTransform.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition;

			//if targethitbox intersects an edge of the aperiogon, return true
			for (int i = 1; i < 3; i++)
			{
				HyperbolicTransform localTransform = closestTransform * HyperbolicTransform.Rotation(i * MathHelper.TwoPi / 3 + MathHelper.Pi) * HyperbolicTransform.Translation(SelfsimilarSentinel.maxArmLength);

				//circle also contains inverse of transformPos
				Vector2 tPosInverse = worldPosition + SelfsimilarSentinel.ARENA_RADIUS * SelfsimilarSentinel.ARENA_RADIUS * (transformPos - worldPosition) / (transformPos - worldPosition).LengthSquared();
				Vector2 arcEnd = localTransform.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition;

				Line line1 = new Line(2 * tPosInverse.X - 2 * transformPos.X, 2 * tPosInverse.Y - 2 * transformPos.Y, transformPos.LengthSquared() - tPosInverse.LengthSquared());
				Line line2 = new Line(2 * tPosInverse.X - 2 * arcEnd.X, 2 * tPosInverse.Y - 2 * arcEnd.Y, arcEnd.LengthSquared() - tPosInverse.LengthSquared());

				Vector2 arcCenter = line1.Intersection(line2);

				float transformCurving = localTransform.GetRotation() - localTransform.GetPosition().ToRotation();
				if (transformCurving > MathHelper.Pi) transformCurving -= MathHelper.TwoPi;
				if (transformCurving < -MathHelper.Pi) transformCurving += MathHelper.TwoPi;
				int arcDirection = transformCurving > 0 ? -1 : 1;

				float arcAngle = ModUtils.AngleBetween(transformPos - arcCenter, arcEnd - arcCenter) * arcDirection;
				if (arcAngle > MathHelper.Pi) arcAngle -= MathHelper.TwoPi;
				if (arcAngle < -MathHelper.Pi) arcAngle += MathHelper.TwoPi;

				Arc connectionArc = new Arc(arcCenter, transformPos, arcAngle);
				if (ModUtils.CheckAABBvArc(targetHitbox, connectionArc)) return true;
			}

			//If a point of the aperiogon is contained in targetHitbox, return true
			if (ModUtils.CheckAABBvPoint(targetHitbox, Projectile.Center)) return true;

			return false;
		}

		public override bool ShouldUpdatePosition()
		{
			return false;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			if (Projectile.timeLeft <= 180)
            {
				DrawLayer.AddProjectile<DrawLayerAdditiveBeforeNPCs>(index);
			}
		}

		public void Draw()
		{
			Texture2D whitePixel = ModContent.Request<Texture2D>(Polarities.CallShootProjectile).Value;
			Rectangle whitePixelFrame = new Rectangle(0,0,1,1);

			//transform as close to the player as possible on the horocycle
			HyperbolicTransform closestTransform = transform;
			int direction = closestTransform.AngleTo(HyperbolicTransform.Identity) > 0 ? 1 : -1;
			bool trying = true;
			while (trying)
			{
				trying = false;

				HyperbolicTransform newTransform = closestTransform * HyperbolicTransform.Rotation(-direction * MathHelper.TwoPi / 3 + MathHelper.Pi) * HyperbolicTransform.Translation(SelfsimilarSentinel.maxArmLength) * HyperbolicTransform.Rotation(-direction * MathHelper.TwoPi / 3);

				if (newTransform.DistanceTo(HyperbolicTransform.Identity) < closestTransform.DistanceTo(HyperbolicTransform.Identity))
				{
					closestTransform = newTransform;
					trying = true;
				}
			}

			const float minDrawScale = 0.01f;

			float alpha = Projectile.timeLeft > 30 ?
				0.1f * Math.Max(0, (180 - Projectile.timeLeft) / 150f)
				:
				Projectile.timeLeft / 30f
				;
			Color color = new Color((int)((243 + 12 * alpha) * alpha), (int)((112 + 143 * alpha) * alpha), (int)(255 * alpha));

			//draw near edges
			for (direction = -1; direction < 2; direction += 2)
			{
				Vector2 center = new Vector2(0, whitePixelFrame.Height * 0.5f);

				for (int i = 0; i < 20; i++)
				{
					HyperbolicTransform localTransform = closestTransform * HyperbolicTransform.LimitRotation(direction * i / (float)Math.Sqrt(3)) * HyperbolicTransform.Rotation(direction * MathHelper.Pi / 6);

					if (localTransform.GetScale() < minDrawScale) break;

					int steps = 8;

					for (int j = 1; j < steps; j++)
					{
						HyperbolicTransform transformPoint = localTransform * HyperbolicTransform.Rotation(-direction * MathHelper.PiOver2) * HyperbolicTransform.Translation(j / (float)steps * SelfsimilarSentinel.maxArmLength) * HyperbolicTransform.Rotation(direction * MathHelper.PiOver2);

						float width = transformPoint.GetScale() * 100;
						float height = transformPoint.GetScale() / steps * 2 * SelfsimilarSentinel.ARENA_RADIUS * SelfsimilarSentinel.maxArmLength;

						Vector2 transformPosition = transformPoint.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition;
						Main.spriteBatch.Draw(whitePixel, transformPosition - Main.screenPosition, whitePixelFrame, color, transformPoint.GetRotation(), center, new Vector2(width, height), SpriteEffects.None, 0f);
					}
				}
			}

			//draw main circle
			HyperbolicTransform transform1 = closestTransform * HyperbolicTransform.Rotation(-MathHelper.Pi / 3) * HyperbolicTransform.Translation(0.5f * SelfsimilarSentinel.maxArmLength) * HyperbolicTransform.Rotation(MathHelper.PiOver2);
			HyperbolicTransform transform2 = closestTransform * HyperbolicTransform.Rotation(MathHelper.Pi / 3) * HyperbolicTransform.Translation(0.5f * SelfsimilarSentinel.maxArmLength) * HyperbolicTransform.Rotation(-MathHelper.PiOver2);
			Vector2 line1Start = transform1.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition;
			float line1Angle = transform1.GetRotation();
			Line line1 = new Line((float)Math.Sin(line1Angle), -(float)Math.Cos(line1Angle), line1Start.Y * (float)Math.Cos(line1Angle) - line1Start.X * (float)Math.Sin(line1Angle));
			Vector2 line2Start = transform2.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition;
			float line2Angle = transform2.GetRotation();
			Line line2 = new Line((float)Math.Sin(line2Angle), -(float)Math.Cos(line2Angle), line2Start.Y * (float)Math.Cos(line2Angle) - line2Start.X * (float)Math.Sin(line2Angle));

			Vector2 horocycleCenter = line1.Intersection(line2);
			float horocycleRadius = (line1Start - horocycleCenter).Length();

			Texture2D giantCircle = TextureAssets.Projectile[Projectile.type].Value;
			Rectangle circleFrame = giantCircle.Frame();
			Vector2 circleCenter = circleFrame.Center();

			float circleScale = horocycleRadius / circleCenter.X;
			Main.spriteBatch.Draw(giantCircle, horocycleCenter - Main.screenPosition, circleFrame, color, 0f, circleCenter, circleScale, SpriteEffects.None, 0f);
		}
	}

	public class HyperbolicHorocycle : ModProjectile, IDrawType
	{
        public override string Texture => "Polarities/NPCs/SelfsimilarSentinel/HyperbolicAperiogon";

        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Hyperbolic Pulse");
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 10;
			Projectile.height = 10;

			Projectile.timeLeft = 30;
			Projectile.penetrate = 1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;

			Projectile.hide = true;
		}

		public HyperbolicTransform transform = HyperbolicTransform.Identity;
		public Vector2 worldPosition;

		public static Projectile NewProjectile(IEntitySource source, Vector2 worldPosition, HyperbolicTransform transform, Vector2 velocity, int damage, float knockBack, int timeLeft = 30, float ai0 = 0, float ai1 = 0)
		{
			try
			{
				Projectile p = Main.projectile[Projectile.NewProjectile(source, Vector2.Zero, velocity, ProjectileType<HyperbolicHorocycle>(), damage, knockBack, Main.myPlayer, ai0, ai1)];
				(p.ModProjectile as HyperbolicHorocycle).worldPosition = worldPosition;
				(p.ModProjectile as HyperbolicHorocycle).transform = transform;
				(p.ModProjectile as HyperbolicHorocycle).SetPosition();

				p.timeLeft = timeLeft;

				return p;
			}
			catch (Exception e)
			{
				return null;
			}
		}

		public override void AI()
		{
			NPC owner = Main.npc[(int)Projectile.ai[0]];
			if (!owner.active)
			{
				Projectile.Kill();
				return;
			}

			if (Projectile.timeLeft == 20)
			{
				Projectile.hostile = false;
			}

			HyperbolicTransform ownerTransform = (owner.ModNPC as SelfsimilarSentinel).transform;

			transform = ownerTransform * HyperbolicTransform.FromVelocity(owner.velocity / SelfsimilarSentinel.ARENA_RADIUS) * ownerTransform.Inverse() * transform;

			//update hyperbolic motion
			transform *= HyperbolicTransform.FromVelocity(Projectile.velocity / SelfsimilarSentinel.ARENA_RADIUS);

			//set position in the world
			SetPosition();
		}

		public void SetPosition()
		{
			Projectile.scale = transform.GetScale();
			Projectile.width = (int)(Projectile.scale * 10);
			Projectile.height = (int)(Projectile.scale * 10);
			Projectile.Center = worldPosition + transform.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS;
			Projectile.rotation = transform.GetRotation();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (!Projectile.hostile) return false;

			HyperbolicTransform transform1 = transform;
			HyperbolicTransform transform2 = transform * HyperbolicTransform.Rotation(MathHelper.Pi / 3) * HyperbolicTransform.Translation(SelfsimilarSentinel.maxArmLength) * HyperbolicTransform.Rotation(-MathHelper.TwoPi / 3);
			Vector2 line1Start = transform1.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition;
			float line1Angle = transform1.GetRotation();
			Line line1 = new Line((float)Math.Sin(line1Angle), -(float)Math.Cos(line1Angle), line1Start.Y * (float)Math.Cos(line1Angle) - line1Start.X * (float)Math.Sin(line1Angle));
			Vector2 line2Start = transform2.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition;
			float line2Angle = transform2.GetRotation();
			Line line2 = new Line((float)Math.Sin(line2Angle), -(float)Math.Cos(line2Angle), line2Start.Y * (float)Math.Cos(line2Angle) - line2Start.X * (float)Math.Sin(line2Angle));

			Vector2 horocycleCenter = line1.Intersection(line2);
			float horocycleRadius = (line1Start - horocycleCenter).Length();

			return ModUtils.CheckAABBvDisc(targetHitbox, new Circle(horocycleCenter, horocycleRadius));
		}

		public override bool ShouldUpdatePosition()
		{
			return false;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			if (Projectile.timeLeft <= 30)
				DrawLayer.AddProjectile<DrawLayerAdditiveBeforeNPCs>(index);
		}

		public void Draw()
		{
			float alpha = Projectile.timeLeft / 30f;
			Color color = new Color((int)((243 + 12 * alpha) * alpha), (int)((112 + 143 * alpha) * alpha), (int)(255 * alpha));

			//draw main circle
			HyperbolicTransform transform1 = transform;
			HyperbolicTransform transform2 = transform * HyperbolicTransform.Rotation(MathHelper.Pi / 3) * HyperbolicTransform.Translation(SelfsimilarSentinel.maxArmLength) * HyperbolicTransform.Rotation(-MathHelper.TwoPi / 3);
			Vector2 line1Start = transform1.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition;
			float line1Angle = transform1.GetRotation();
			Line line1 = new Line((float)Math.Sin(line1Angle), -(float)Math.Cos(line1Angle), line1Start.Y * (float)Math.Cos(line1Angle) - line1Start.X * (float)Math.Sin(line1Angle));
			Vector2 line2Start = transform2.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition;
			float line2Angle = transform2.GetRotation();
			Line line2 = new Line((float)Math.Sin(line2Angle), -(float)Math.Cos(line2Angle), line2Start.Y * (float)Math.Cos(line2Angle) - line2Start.X * (float)Math.Sin(line2Angle));

			Vector2 horocycleCenter = line1.Intersection(line2);
			float horocycleRadius = (line1Start - horocycleCenter).Length();

			Texture2D giantCircle = TextureAssets.Projectile[Projectile.type].Value;
			Rectangle circleFrame = giantCircle.Frame();
			Vector2 circleCenter = circleFrame.Center();

			float circleScale = horocycleRadius / circleCenter.X;
			Main.spriteBatch.Draw(giantCircle, horocycleCenter - Main.screenPosition, circleFrame, color, 0f, circleCenter, circleScale, SpriteEffects.None, 0f);
		}
	}

	public class HyperbolicWisp : ModProjectile
	{
        public override string Texture => "Terraria/Projectile_"+ProjectileID.RainbowCrystalExplosion;

        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Hyperbolic Wisp");

			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 10;
			Projectile.height = 10;

			Projectile.alpha = 0;
			Projectile.timeLeft = 600;
			Projectile.penetrate = 1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;

			Projectile.hide = true;
		}

		public HyperbolicTransform transform = HyperbolicTransform.Identity;
		public HyperbolicTransform[] oldTransform;
		public Vector2 worldPosition;

		public static Projectile NewProjectile(IEntitySource source, Vector2 worldPosition, HyperbolicTransform transform, Vector2 velocity, int damage, float knockBack, int timeLeft = 600, float ai0 = 0, float ai1 = 0, int extraUpdates = 0)
		{
			try
			{
				Projectile p = Main.projectile[Projectile.NewProjectile(source, Vector2.Zero, velocity, ProjectileType<HyperbolicWisp>(), damage, knockBack, Main.myPlayer, ai0, ai1)];
				(p.ModProjectile as HyperbolicWisp).worldPosition = worldPosition;
				(p.ModProjectile as HyperbolicWisp).transform = transform;
				(p.ModProjectile as HyperbolicWisp).SetPosition();
				p.timeLeft = timeLeft * (extraUpdates + 1);
				p.extraUpdates = extraUpdates;

				//initialize transform trail
				(p.ModProjectile as HyperbolicWisp).Initialize();

				return p;
			}
			catch (Exception e)
			{
				return null;
			}
		}

		public void Initialize()
		{
			oldTransform = new HyperbolicTransform[10];
			for (int i = 0; i < oldTransform.Length; i++)
			{
				oldTransform[i] = transform;
			}
		}

		public override void AI()
		{
			switch (Projectile.ai[1])
			{
				case 0:
					Projectile.velocity *= 0.99f;
					//transform waving
					transform *= HyperbolicTransform.Rotation((float)Math.Sqrt(Projectile.velocity.Length()) * (0.02f * (float)Math.Cos(0.1f * Projectile.localAI[1]) + Projectile.ai[0]));

					//if we've stopped mining ore, flicker out
					if (Main.LocalPlayer.GetModPlayer<PolaritiesPlayer>().selfsimilarHitTimer == 0)
					{
						Projectile.ai[1] = 1;
						Projectile.timeLeft = Main.rand.Next(180) + 20;
					}
					else if (NPC.AnyNPCs(NPCType<SelfsimilarSentinel>()))
					{
						Projectile.ai[1] = 2;
						Projectile.timeLeft = 360 * (Projectile.extraUpdates + 1);
                    }
					break;
				case 1:
					//die
					Projectile.velocity *= 0.95f;
					//transform waving
					transform *= HyperbolicTransform.Rotation((float)Math.Sqrt(Projectile.velocity.Length()) * (0.02f * (float)Math.Cos(0.1f * Projectile.localAI[1]) + Projectile.ai[0]));

					if (Projectile.timeLeft <= 20)
						Projectile.scale -= Projectile.scale / Projectile.timeLeft;
					break;
				case 2:
					//home in on near-ish to the center
					Projectile.velocity *= 0.99f;
					//transform waving
					transform *= HyperbolicTransform.Rotation((float)Math.Sqrt(Projectile.velocity.Length()) * (0.02f * (float)Math.Cos(0.1f * Projectile.localAI[1]) + Projectile.ai[0]));

					if (Projectile.timeLeft < 300 * (Projectile.extraUpdates + 1))
					{
						float homingAmount = 16f * Math.Min(1, 2.5f * (1 - Projectile.timeLeft / (300f * (Projectile.extraUpdates + 1))));
						float radius = 0.75f * (0.5f + Projectile.timeLeft / (300f * (Projectile.extraUpdates + 1)));

						HyperbolicTransform goalTransform = HyperbolicTransform.FromVelocity(new Vector2(Projectile.ai[0] * radius, 0).RotatedBy(transform.GetPosition().ToRotation() + MathHelper.PiOver2));

						Vector2 goalVelocity = homingAmount * new Vector2(1f / (transform.DistanceTo(goalTransform) + 1f), 0).RotatedBy(transform.AngleTo(goalTransform));
						Projectile.velocity += (goalVelocity - Projectile.velocity) / 60f;

						//set transform to rotate to face velocity and set velocity to pure x
						transform *= HyperbolicTransform.Rotation(Projectile.velocity.ToRotation());
						Projectile.velocity = new Vector2(Projectile.velocity.Length(), 0);
					}
					break;
				case 3:
					//do nothing special
					//transform waving
					transform *= HyperbolicTransform.Rotation((float)Math.Sqrt(Projectile.velocity.Length()) * (0.02f * (float)Math.Cos(0.1f * Projectile.localAI[1]) + Projectile.ai[0]));
					break;
			}

			Projectile.localAI[1]++;
			
			//update hyperbolic motion
			transform = transform * HyperbolicTransform.FromVelocity(Projectile.velocity / SelfsimilarSentinel.ARENA_RADIUS);

			//update transform trail
			for (int i = oldTransform.Length - 1; i > 0; i--)
			{
				oldTransform[i] = oldTransform[i - 1];
			}
			oldTransform[0] = transform;

			//set position in the world
			SetPosition();
		}

		public void SetPosition()
		{
			Projectile.Center = worldPosition + transform.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS;
			Projectile.rotation = transform.GetRotation() + Projectile.velocity.ToRotation();
		}

		public override bool ShouldUpdatePosition()
		{
			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
			behindNPCsAndTiles.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Rectangle frame = texture.Frame();
			Vector2 center = frame.Center();

			const float scaleMultiplier = 0.3f;

			//only draw large enough segments
			for (int i = 0; i < oldTransform.Length; i++)
			{
				float progress = 4 * (1 - i / (float)oldTransform.Length) * (i / (float)oldTransform.Length);
				Color color = new Color((int)(243 + 8 * progress), (int)(112 + 102 * progress), 255); //12, 143
				Main.spriteBatch.Draw(texture, oldTransform[i].GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition - Main.screenPosition, frame, color, oldTransform[i].GetRotation() + MathHelper.PiOver2, center, new Vector2(progress * Projectile.scale, 1) * oldTransform[i].GetScale() * scaleMultiplier, SpriteEffects.None, 0f);
			}
			return false;
		}
	}

	public class SentinelOreChunk : ModProjectile
	{
        public override string Texture => "Polarities/Gores/SelfsimilarSentinelShrapnel";

        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Selfsimilar Ore Cunk");

			Main.projFrames[Projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 10;
			Projectile.height = 10;

			Projectile.alpha = 0;
			Projectile.timeLeft = 360;
			Projectile.penetrate = 1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public HyperbolicTransform transform = HyperbolicTransform.Identity;
		public Vector2 worldPosition;

		private static int mostRecentTinkSound;

		public static Projectile NewProjectile(IEntitySource source, Vector2 worldPosition, HyperbolicTransform transform, Vector2 velocity, int damage, float knockBack, int timeLeft = 360, float ai0 = 0, float ai1 = 0, int extraUpdates = 0)
		{
			try
			{
				Projectile p = Main.projectile[Projectile.NewProjectile(source, Vector2.Zero, velocity, ProjectileType<SentinelOreChunk>(), damage, knockBack, Main.myPlayer, ai0, ai1)];
				(p.ModProjectile as SentinelOreChunk).worldPosition = worldPosition;
				(p.ModProjectile as SentinelOreChunk).transform = transform;
				(p.ModProjectile as SentinelOreChunk).SetPosition();
				p.timeLeft = timeLeft * (extraUpdates + 1);
				p.extraUpdates = extraUpdates;

				(p.ModProjectile as SentinelOreChunk).Initialize();

				return p;
			}
			catch (Exception e)
			{
				return null;
			}
		}

		public void Initialize()
        {
			//goal position data
			Projectile.localAI[0] = Main.rand.NextFloat(0.02f);
			Projectile.localAI[1] = transform.GetPosition().ToRotation() + Main.rand.NextFloat(-MathHelper.PiOver2, MathHelper.PiOver2);

			Projectile.frame = Main.rand.Next(4);
        }

		public override void AI()
		{
			switch (Projectile.ai[1])
			{
				case 0:
					//home in on near-ish to the center
					Projectile.velocity *= 0.98f;

					if (Projectile.timeLeft < 180 * (Projectile.extraUpdates + 1))
					{
						HyperbolicTransform goalTransform = HyperbolicTransform.FromVelocity(new Vector2(Projectile.localAI[0]).RotatedBy(Projectile.localAI[1]));

						float homingAmount = 28f;

						float speed = 1f / (transform.DistanceTo(goalTransform) + 1f);
						Vector2 goalVelocity = homingAmount * new Vector2(speed, 0).RotatedBy(transform.AngleTo(goalTransform));
						Projectile.velocity += (goalVelocity - Projectile.velocity) / 60f;

						//stop if close enough
						if (Projectile.velocity.Length() / SelfsimilarSentinel.ARENA_RADIUS > transform.DistanceTo(goalTransform))
                        {
							Projectile.velocity = Vector2.Zero;

							Projectile.ai[1] = 1;

							if (Main.GameUpdateCount >= mostRecentTinkSound + 5)
							{
								mostRecentTinkSound = (int)Main.GameUpdateCount;
								SoundEngine.PlaySound(SoundID.Tink, Projectile.Center);
							}
						}
					}
					break;
			}


			//update hyperbolic motion
			transform *= HyperbolicTransform.FromVelocity(Projectile.velocity / SelfsimilarSentinel.ARENA_RADIUS);

			//set position in the world
			SetPosition();
		}

		public void SetPosition()
		{
			Projectile.scale = transform.GetScale();
			Projectile.Center = worldPosition + transform.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS;
			Projectile.rotation = transform.GetRotation() + Projectile.velocity.ToRotation();
		}

		public override bool ShouldUpdatePosition()
		{
			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Rectangle frame = texture.Frame(1, 4, 0, Projectile.frame);
			Vector2 center = frame.Size() / 2;

			const float scaleMultiplier = 1f;
			Main.spriteBatch.Draw(texture, transform.GetPosition() * SelfsimilarSentinel.ARENA_RADIUS + worldPosition - Main.screenPosition, frame, Color.White, transform.GetRotation() + Projectile.ai[0], center, transform.GetScale() * scaleMultiplier, SpriteEffects.None, 0f);

			return false;
		}
	}
}