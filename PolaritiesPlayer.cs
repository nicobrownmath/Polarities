using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using static Terraria.ModLoader.ModContent;
using Terraria.GameInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using Polarities.Items;
using Polarities.NPCs;
using MonoMod.Cil;
using Terraria.ModLoader.IO;
using Terraria.Enums;
using Terraria.Utilities;
using Polarities.Items.Armor;
using Polarities.Buffs;
using System.Reflection;
using Mono.Cecil.Cil;
using Polarities.Items.Books;
using Polarities.Items.Accessories;
using Polarities.Items.Placeable.Walls;
using Polarities.Biomes;
using Polarities.Items.Placeable.Blocks;
using Polarities.Items.Placeable;
using Polarities.Items.Materials;
using Polarities.Projectiles;
using Polarities.Items.Consumables.Crates;
using Terraria.Localization;
using Polarities.Items.Placeable.Bars;
using Polarities.Items.Armor.ConvectiveArmor;
using Terraria.Audio;
using Polarities.NPCs.ConvectiveWanderer;
using Polarities.Effects;
using Polarities.Items.Weapons.Summon.Minions;
using Polarities.Items.Armor.MechaMayhemArmor;
using static Terraria.ModLoader.PlayerDrawLayer;
using MultiHitboxNPCLibrary;
using static tModPorter.ProgressUpdate;
using Polarities.Items.Armor.StormcloudArmor;
using Polarities.Items.Armor.Vanity.TuringSet;
using Polarities.Items.Accessories.Wings;
using Polarities.Items.Armor.Vanity.ElectroManiacSet;
using Polarities.Items.Armor.Vanity.BubbySet;
using Polarities.Items.Weapons;
using Polarities.Items.Weapons.Ranged;

namespace Polarities
{
	public class PolaritiesPlayer : ModPlayer
	{
        public override void Load()
        {
			//allow crits from enemies
            IL.Terraria.Player.Update_NPCCollision += Player_Update_NPCCollision;
            //modify color of damage text for crits from enemies
            IL.Terraria.Player.Hurt += Player_Hurt;
            //modify damage numbers for negative life regen
            IL.Terraria.Player.UpdateLifeRegen += Player_UpdateLifeRegen;
        }

        public int warhammerDefenseBoost = 0;
		public int warhammerTimeBoost = 0;

		public Dictionary<float, float> screenShakes = new Dictionary<float, float>(); //key is time at which the screenshake ends, value is magnitude
		int screenshakeRandomSeed;

		public float hookSpeedMult;
		public float manaStarMultiplier;
		public float orbMinionSlots;
		public int royalOrbHitCount;
		public Vector2 velocityMultiplier;
		public bool strangeObituary;
		public float usedBookSlots;
		public float maxBookSlots;
		public bool canJumpAgain_Sail_Extra;
		public bool jumpAgain_Sail_Extra;
		public bool hasGlide;
		public int skeletronBookCooldown;
		public int beeRingTimer;
		public bool stormcore;
		public bool stargelAmulet;
		public bool hopperCrystal;
		public bool limestoneShield;
		public int limestoneShieldCooldown;
		public bool limestoneSetBonus;
		public int limestoneSetBonusHitCooldown;
		public bool skeletronBook;
		public int moonLordLifestealCooldown;
		public int wingTimeBoost;
		public float critDamageBoostMultiplier;
		public int ignoreCritDefenseAmount;
		public bool snakescaleSetBonus;
		public int desiccation;
		public int incineration;
		public int incinerationResistanceTime;
		public bool coneVenom;
		public float runSpeedBoost;
		public float spawnRate;
		public bool solarEnergizer;
		public bool wormScarf;
		public int wyvernsNestDamage;
		public Vector3 light;
		public DamageClass convectiveSetBonusType;
		public int convectiveSetBonusCharge;
		public StatModifier dartDamage;
		public bool justHit;
		public float candyCaneAtlatlBoost;
		public StatModifier nonMagicDamage;
		public bool hydraHide;
		public float hydraHideTime = 0;
        public bool convectiveDash;
        public int convectiveDashCharge;
		public Vector2 convectiveDashVelocity = Vector2.Zero;
		public bool convectiveDashing;
		public int convectiveDashStartTime;
        public bool stormcloudArmor;
        public int stormcloudArmorCooldown;
		public int tolerancePotionDelayTime = 3600;

        public bool flawlessMechArmorSet;
		public int flawlessMechSetBonusTime;
		public int flawlessMechSetBonusCooldown;
		public bool flawlessMechMask;
		public int flawlessMechMaskCooldown;
		public bool flawlessMechChestplate;
		public bool flawlessMechTail;
		public int flawlessMechTailCooldown;

		public const int MECH_ARMOR_SET_COOLDOWN = 120;
		public const int MECH_ARMOR_SET_TIME = 240;
		public const int MECH_MASK_COOLDOWN = 20;
		public const int MECH_TAIL_COOLDOWN = 60;

		//direction of dash
		public int dashDir;
		//index of dash in Dash.dashes
		public int dashIndex;
		//time left until next dash
		public int dashDelay;
		//time left in dash
		public int dashTimer;

		//dash directions
		public const int DashRight = 2;
		public const int DashLeft = 3;

		public int itemHitCooldown = 0;

		//trail info
        const int trailCacheLength = 20;
        public Vector2[] oldCenters = new Vector2[trailCacheLength];
        public Vector2[] oldVelocities = new Vector2[trailCacheLength];
        public int[] oldDirections = new int[trailCacheLength];

        public int bubbyWingFrameCounter;
        public int bubbyWingFrame;


        public override void ResetEffects()
        {
            //update old positions, velocities, etc.
            for (int i = trailCacheLength - 1; i > 0; i--)
            {
                oldCenters[i] = oldCenters[i - 1];
                oldVelocities[i] = oldVelocities[i - 1];
                oldDirections[i] = oldDirections[i - 1];
            }
            oldCenters[0] = Player.Center;
            oldVelocities[0] = Player.velocity;
            oldDirections[0] = Player.direction;

            //reset a bunch of values
            warhammerDefenseBoost = 0;
			warhammerTimeBoost = 0;
			hookSpeedMult = 1f;
			manaStarMultiplier = 1f;
			orbMinionSlots = 1f;
			strangeObituary = false;
			usedBookSlots = 0f;
			hasGlide = false;
			stormcore = false;
			stargelAmulet = false;
			hopperCrystal = false;
			limestoneShield = false;
			limestoneSetBonus = false;
			skeletronBook = false;
			wingTimeBoost = 0;
			critDamageBoostMultiplier = 1f;
			ignoreCritDefenseAmount = 0;
			snakescaleSetBonus = false;
			desiccation = 0;
			incineration = 0;
			coneVenom = false;
			runSpeedBoost = 1f;
			spawnRate = 1f;
			solarEnergizer = false;
			wormScarf = false;
			wyvernsNestDamage = 0;
			incinerationResistanceTime = 0;
			light = Vector3.Zero;
			convectiveSetBonusType = null;
			dartDamage = StatModifier.Default;
			justHit = false;
			nonMagicDamage = StatModifier.Default;
			hydraHide = false;
			convectiveDash = false;
			stormcloudArmor = false;

            if (skeletronBookCooldown > 0) skeletronBookCooldown--;
			if (beeRingTimer > 0) beeRingTimer--;
			if (limestoneShieldCooldown > 0) limestoneShieldCooldown--;
			if (limestoneSetBonusHitCooldown > 0) limestoneSetBonusHitCooldown--;
			if (moonLordLifestealCooldown > 0) moonLordLifestealCooldown--;
			if (candyCaneAtlatlBoost > 0) candyCaneAtlatlBoost--;
            if (stormcloudArmorCooldown > 0) stormcloudArmorCooldown--;
            if (hydraHideTime > 0)
			{
				hydraHideTime--;
				Player.lifeRegenTime = 120;
            }

            //mech flawless stuff
            flawlessMechArmorSet = false;
			flawlessMechChestplate = false;
			flawlessMechMask = false;
			flawlessMechTail = false;
			if (flawlessMechMaskCooldown > 0) flawlessMechMaskCooldown--;
			if (flawlessMechTailCooldown > 0) flawlessMechTailCooldown--;
			if (flawlessMechSetBonusTime > 0) flawlessMechSetBonusTime--;
			//only cool this down if the player isn't holding a weapon
			if (flawlessMechSetBonusCooldown > 0 && Player.HeldItem.damage <= 0 && (Player.HeldItem.DamageType == DamageClass.Default || Player.HeldItem.DamageType == DamageClass.Generic))
			{
				flawlessMechSetBonusCooldown--;

				if (flawlessMechSetBonusCooldown == 0)
				{
					SoundEngine.PlaySound(SoundID.Item15, Player.position);

					for (int i = 0; i < 24; i++)
					{
						Dust.NewDustPerfect(Player.MountedCenter, 114, new Vector2(4, 0).RotatedBy(MathHelper.TwoPi * i / 24f), Scale: 1f).noGravity = true;
					}
				}
			}

			screenshakeRandomSeed = Main.rand.Next();

			//currently this is only used with warhammers
			//if I ever use player rotation for anything else besides mounts it may need to be made into a more complex system
			if (!Player.mount.Active && !Player.sleeping.isSleeping)
			{
				Player.fullRotation *= 0.9f;

				Player.fullRotationOrigin = Player.Size / 2;
				if (Player.velocity.Y == 0)
				{
					Player.fullRotationOrigin.Y += Player.height / 3.5f * Player.gravDir;
				}
			}
			else if (Player.controlMount)
			{
				Player.fullRotation = 0f;
			}

			//reset dash data
			dashIndex = 0;
			if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[DashRight] < 15)
			{
				dashDir = DashRight;
			}
			else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[DashLeft] < 15)
			{
				dashDir = DashLeft;
			}
			else
			{
				dashDir = -1;
			}

			if (itemHitCooldown > 0) itemHitCooldown--;

			Player.velocity /= velocityMultiplier;
			velocityMultiplier = Vector2.One;
		}

		//this is reset here to ensure books update properly
        public override void PostUpdateBuffs()
		{
			maxBookSlots = 1f;
		}

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			if (!Player.mount.Active)
			{
				if (PlayerInput.Triggers.Current.Jump && hasGlide)
				{
					Player.maxFallSpeed = 1f;
				}
			}

			//target via orbs
			if (PlayerInput.Triggers.JustPressed.MouseRight && (Player.HeldItem.DamageType == DamageClass.Summon || Player.HeldItem.DamageType.GetEffectInheritance(DamageClass.Summon)) && Player.channel)
			{
				Player.MinionNPCTargetAim(false);
			}

			//convective set bonus
			if (convectiveSetBonusType != null)
			{
				convectiveSetBonusCharge++;

				if (convectiveSetBonusCharge == 600)
                {
					SoundEngine.PlaySound(SoundID.Item15, Player.position);

					for (int i = 0; i < 24; i++)
					{
						float speedProgress = Main.rand.NextFloat(1f);

						ConvectiveWandererVortexParticle particle = Particle.NewParticle<ConvectiveWandererVortexParticle>(Player.MountedCenter, new Vector2(4 + speedProgress * 8, 0).RotatedBy(MathHelper.TwoPi * i / 24f), 0f, 0f, Scale: 0.15f, Color: ModUtils.ConvectiveFlameColor((1 - speedProgress) * (1 - speedProgress) / 2f));
						ParticleLayer.BeforePlayersAdditive.Add(particle);
					}
				}

				if (convectiveSetBonusCharge >= 600 && Polarities.ArmorSetBonusHotkey.JustPressed)
				{
					if (convectiveSetBonusType == DamageClass.Melee)
                    {
						Projectile.NewProjectile(Player.GetSource_FromAI(), Player.MountedCenter, Vector2.Zero, ProjectileType<ConvectiveArmorMeleeExplosion>(), (int)Player.GetTotalDamage(DamageClass.Melee).ApplyTo(400), Player.GetTotalKnockback(DamageClass.Melee).ApplyTo(5f), Player.whoAmI);
                    }
					else if (convectiveSetBonusType == DamageClass.Ranged)
					{
						Projectile.NewProjectile(Player.GetSource_FromAI(), Player.MountedCenter, Main.MouseWorld - Player.MountedCenter, ProjectileType<ConvectiveArmorRangedDeathray>(), (int)Player.GetTotalDamage(DamageClass.Ranged).ApplyTo(300), Player.GetTotalKnockback(DamageClass.Ranged).ApplyTo(5f), Player.whoAmI);
					}
					else if (convectiveSetBonusType == DamageClass.Magic)
					{
						SoundEngine.PlaySound(SoundID.Item88, Main.MouseWorld);
						for (int i = 0; i < 32; i++)
						{
							Vector2 offset = new Vector2(Main.rand.NextFloat(1f), 0).RotatedByRandom(MathHelper.TwoPi);
							Projectile.NewProjectile(Player.GetSource_FromAI(), Main.MouseWorld + new Vector2(0, 1000), new Vector2(0, -54).RotatedBy(offset.X * 0.2f) * (1 + offset.Y * 0.4f), ProjectileType<ConvectiveArmorMagicEruption>(), (int)Player.GetTotalDamage(DamageClass.Magic).ApplyTo(80), Player.GetTotalKnockback(DamageClass.Magic).ApplyTo(2f), Player.whoAmI);
						}
					}
					else if (convectiveSetBonusType == DamageClass.Summon)
					{
						SoundEngine.PlaySound(SoundID.NPCDeath14, Player.Center);
						for (int i = 0; i < Player.maxMinions; i++)
						{
							Main.projectile[Projectile.NewProjectile(Player.GetSource_FromAI(), Player.MountedCenter, new Vector2(16, 0).RotatedBy(PolaritiesSystem.timer * 0.05f + MathHelper.TwoPi * i / Player.maxMinions), ProjectileType<ConvectiveArmorSummonVortex>(), 400, Player.GetTotalKnockback(DamageClass.Summon).ApplyTo(3f), Player.whoAmI)].originalDamage = 400;
						}
					}

					AddScreenShake(15, 30);

					convectiveSetBonusCharge = 0;
				}
			}
			else
			{
				convectiveSetBonusCharge = 0;
			}

			//flawless mech armor set
			if (PlayerInput.Triggers.Current.MouseRight && flawlessMechArmorSet && flawlessMechSetBonusCooldown == 0)
			{
				flawlessMechSetBonusTime = MECH_ARMOR_SET_TIME;
				flawlessMechSetBonusCooldown = MECH_ARMOR_SET_COOLDOWN + MECH_ARMOR_SET_TIME;
			}

			//convective dash
			if (convectiveDash)
            {
				if (Polarities.ConvectiveDashHotkey.JustPressed && convectiveDashCharge > 60 && !Player.mount.Active)
				{
					//start dash
					convectiveDashCharge -= convectiveDashCharge % 60; //return to last charge level

                    convectiveDashing = true;
					convectiveDashVelocity = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.Zero) * 24;
					convectiveDashStartTime = PolaritiesSystem.timer;

					SoundEngine.PlaySound(SoundID.NPCDeath14, Player.Center);

					for (int i = 0; i < 24; i++)
                    {
                        Vector2 particleSpawnPos = Player.Center + new Vector2(Main.rand.NextFloat(24), 0).RotatedByRandom(MathHelper.TwoPi);
                        Vector2 particleVelocity = new Vector2(Main.rand.NextFloat(6, 20), 0).RotatedByRandom(MathHelper.TwoPi) - convectiveDashVelocity;
                        ConvectiveWandererVortexParticle particle = Particle.NewParticle<ConvectiveWandererVortexParticle>(particleSpawnPos, particleVelocity, 0f, 0f, Scale: Main.rand.NextFloat(0.1f, 0.2f), Color: ModUtils.ConvectiveFlameColor(Main.rand.NextFloat(0.2f, 0.4f)));
                        ParticleLayer.AfterLiquidsAdditive.Add(particle);
                    }
                }
				else if (convectiveDashing && (!Polarities.ConvectiveDashHotkey.Current || Player.mount.Active))
                {
                    convectiveDashCharge = 0;
                    convectiveDashing = false;

					Player.velocity *= 0.25f;
                }

				if (convectiveDashing)
				{
					convectiveDashCharge -= 4;

					if (convectiveDashCharge < 0)
					{
						convectiveDashCharge = 0;
						convectiveDashing = false;

						Player.velocity *= 0.25f;
					}
					else
					{
						Player.velocity = convectiveDashVelocity;
						Player.ChangeDir(convectiveDashVelocity.X > 0 ? 1 : -1);

						//prevent other movement things from interfering
						Player.maxFallSpeed = convectiveDashVelocity.Length();
						Player.controlDown = false;
						Player.controlJump = false;
						Player.controlLeft = false;
						Player.controlRight = false;
						Player.controlUp = false;

						Vector2 particleSpawnPos = Player.Center + new Vector2(Main.rand.NextFloat(24), 0).RotatedByRandom(MathHelper.TwoPi);
						Vector2 particleVelocity = -convectiveDashVelocity.RotatedByRandom(0.1f);
						ConvectiveWandererVortexParticle particle = Particle.NewParticle<ConvectiveWandererVortexParticle>(particleSpawnPos, particleVelocity, 0f, 0f, Scale: Main.rand.NextFloat(0.1f, 0.2f), Color: ModUtils.ConvectiveFlameColor(Main.rand.NextFloat(0.2f, 0.4f)));
						ParticleLayer.AfterLiquidsAdditive.Add(particle);

						//ramming! (based on EoC)
						Rectangle rectangle = new Rectangle((int)((double)Player.position.X + (double)Player.velocity.X * 0.5 - 4.0), (int)((double)Player.position.Y + (double)Player.velocity.Y * 0.5 - 4.0), Player.width + 8, Player.height + 8);
						for (int i = 0; i < 200; i++)
						{
							NPC nPC = Main.npc[i];
							if (!nPC.active || nPC.dontTakeDamage || nPC.friendly || (nPC.aiStyle == 112 && !(nPC.ai[2] <= 1f)) || !Player.CanNPCBeHitByPlayerOrPlayerProjectile(nPC))
							{
								continue;
							}

							if (MultiHitboxNPC.CollideWithRectangle(nPC, rectangle, needCanBeDamaged: true) && (nPC.noTileCollide || Player.CanHit(nPC)))
							{
								float num = Player.GetTotalDamage(DamageClass.Generic).ApplyTo(100f);
								float num12 = Player.GetTotalKnockback(DamageClass.Generic).ApplyTo(10f);
								bool crit = false;
								if ((float)Main.rand.Next(100) < Player.GetTotalCritChance(DamageClass.Generic))
								{
									crit = true;
								}
								int num20 = Player.direction;
								if (Player.velocity.X < 0f)
								{
									num20 = -1;
								}
								if (Player.velocity.X > 0f)
								{
									num20 = 1;
								}
								if (Player.whoAmI == Main.myPlayer)
								{
									Player.ApplyDamageToNPC(nPC, (int)num, num12, num20, crit);
								}
								Player.GiveImmuneTimeForCollisionAttack(10);
								nPC.immune[Player.whoAmI] = 6;
							}
						}
					}
				}
				else if (convectiveDashCharge < 240)
				{
					if (convectiveDashCharge % 60 == 0)
					{
						float progress = convectiveDashCharge / 240f;
						BuildingEruptionChargingParticle particle = Particle.NewParticle<BuildingEruptionChargingParticle>(Vector2.Zero, Vector2.Zero, 0f, 0f, Scale: 1f, Color: ModUtils.ConvectiveFlameColor(progress * progress * 0.5f));
						particle.playerOwner = Player.whoAmI;
						ParticleLayer.BeforePlayersAdditive.Add(particle);
					}

					convectiveDashCharge++;

					if (convectiveDashCharge % 60 == 0)
					{
						SoundEngine.PlaySound(SoundID.Item114.WithPitchOffset((convectiveDashCharge - 60) / 180f - 0.5f), Player.Center);
					}
				}
            }
            else
            {
                if (convectiveDashing)
				{
					Player.velocity *= 0.25f;
                    convectiveDashing = false;
                }

                if (!convectiveDash) convectiveDashCharge = 0;
            }
		}

		public override void PostUpdateEquips()
		{
			if (Player.HasBuff<GolemBookBuff>() && Player.wingsLogic == 0)
			{
				Player.noFallDmg = true;
				Player.jumpSpeedBoost += 16;
				Player.statDefense += 6;
			}

			canJumpAgain_Sail_Extra = false;
			if (Player.HasBuff(BuffType<KingSlimeBookBuff>()))
			{
				if (Player.hasJumpOption_Sail) { canJumpAgain_Sail_Extra = true; }
				Player.hasJumpOption_Sail = true;
			}

			if (stargelAmulet)
			{
				float amountOfDay;
				if (Main.dayTime)
				{
					amountOfDay = 1f - (float)Math.Abs(Main.time - Main.dayLength / 2) / (float)Main.dayLength;
				}
				else
				{
					amountOfDay = (float)Math.Abs(Main.time - Main.nightLength / 2) / (float)Main.nightLength;
				}
				Player.GetDamage(DamageClass.Generic) += 0.12f * amountOfDay;
				Player.endurance *= (1 - 0.1f * (1 - amountOfDay));
			}

			//wing time boost
			Player.wingTimeMax += wingTimeBoost;

			//run speed boost
			Player.maxRunSpeed *= runSpeedBoost;
			Player.accRunSpeed *= runSpeedBoost;

			//custom slimes
			foreach (int i in PolaritiesNPC.customSlimes)
            {
				Player.npcTypeNoAggro[i] = Player.npcTypeNoAggro[NPCID.BlueSlime];
            }
		}

        public override void PostUpdate()
		{
			if (hopperCrystal && Player.justJumped)
			{
				Player.velocity.X = (Player.velocity.X > 0 ? 1 : -1) * Math.Min(2 * Player.maxRunSpeed, Math.Abs(2 * Player.velocity.X));
			}

			if (stormcore && 0.2f + Player.slotsMinions <= Player.maxMinions && Main.rand.NextBool(60))
			{
				Main.projectile[Projectile.NewProjectile(Player.GetSource_FromAI(), Player.Center.X + 500 * (2 * (float)Main.rand.NextDouble() - 1), Player.Center.Y - 500, 0, 0, ProjectileType<StormcoreMinion>(), 1, Player.GetTotalKnockback(DamageClass.Summon).ApplyTo(0.5f), Player.whoAmI, 0, 0)].originalDamage = 1;
			}

			if (wyvernsNestDamage > 0)
            {
				//sentries don't despawn while using the wyvern's nest
				for (int i = 0; i < Main.maxProjectiles; i++)
                {
					if (Main.projectile[i].active && Main.projectile[i].owner == Player.whoAmI && Main.projectile[i].sentry)
                    {
						Main.projectile[i].timeLeft = Projectile.SentryLifeTime;
					}
                }
				for (int i = Player.ownedProjectileCounts[ProjectileType<WyvernsNestMinion>()]; i < Player.maxTurrets; i++)
				{
					Main.projectile[Projectile.NewProjectile(Player.GetSource_FromAI(), Player.Center, Vector2.Zero, ProjectileType<WyvernsNestMinion>(), wyvernsNestDamage, Player.GetTotalKnockback(DamageClass.Summon).ApplyTo(2f), Player.whoAmI, 0, 0)].originalDamage = 20;
				}
			}

            if (stormcloudArmor && stormcloudArmorCooldown <= 0 && Player.ownedProjectileCounts[ProjectileType<StormcloudArmorRaincloud>()] < 8)
            {
                stormcloudArmorCooldown = 240;

                float distance = 750f;
                bool isTarget = false;
                int targetID = -1;
                for (int k = 0; k < 200; k++)
                {
                    if (Main.npc[k].active && !Main.npc[k].dontTakeDamage && !Main.npc[k].friendly && Main.npc[k].lifeMax > 5 && !Main.npc[k].immortal && Main.npc[k].chaseable)
                    {
                        Vector2 newMove = Main.npc[k].Center - Player.Center;
                        float distanceTo = (float)Math.Sqrt(newMove.X * newMove.X + newMove.Y * newMove.Y);
                        if (distanceTo < distance)
                        {
                            targetID = k;
                            distance = distanceTo;
                            isTarget = true;
                        }
                    }
                }

                if (isTarget)
                {
                    NPC target = Main.npc[targetID];
                    Projectile.NewProjectile(Player.GetSource_FromAI(), new Vector2(target.Center.X, target.position.Y - 128), Vector2.Zero, ProjectileType<StormcloudArmorRaincloud>(), (int)Player.GetTotalDamage(DamageClass.Summon).ApplyTo(8), 0, Player.whoAmI);
                }
            }

            if (canJumpAgain_Sail_Extra)
			{
				if (Player.justJumped || Player.controlHook || Player.velocity.Y == 0f)
				{
					jumpAgain_Sail_Extra = true;
				}
				if (!Player.canJumpAgain_Sail && jumpAgain_Sail_Extra)
				{
					jumpAgain_Sail_Extra = false;
					Player.canJumpAgain_Sail = true;
				}
			}

			if (Player.HasBuff(BuffType<QueenBeeBookBuff>()) && Player.ownedProjectileCounts[ProjectileType<QueenBeeBookBee>()] + Player.ownedProjectileCounts[ProjectileType<QueenBeeBookBeeLarge>()] < 6 && beeRingTimer == 0)
			{
				int buffIndex = Player.FindBuffIndex(BuffType<QueenBeeBookBuff>());
				if (Player.strongBees && Main.rand.NextBool())
				{
					Projectile.NewProjectile(Player.GetSource_Buff(buffIndex), Main.MouseWorld, Vector2.Zero, ProjectileType<QueenBeeBookBeeLarge>(), 8, 0.5f, Player.whoAmI, Main.rand.NextFloat(0, 2 * MathHelper.Pi));
				}
				else
				{
					Projectile.NewProjectile(Player.GetSource_Buff(buffIndex), Main.MouseWorld, Vector2.Zero, ProjectileType<QueenBeeBookBee>(), 5, 0, Player.whoAmI, Main.rand.NextFloat(0, 2 * MathHelper.Pi));
				}
				beeRingTimer = 5;
			}

			if (Player.controlUseItem)
			{
				if (flawlessMechMask || flawlessMechChestplate || flawlessMechTail)
				{
					Player.direction = (Main.MouseWorld.X > Player.Center.X) ? 1 : -1;
				}

				//mask has deathrays
				if (flawlessMechMask && flawlessMechMaskCooldown == 0)
				{
					flawlessMechMaskCooldown = MECH_MASK_COOLDOWN;

					if (flawlessMechSetBonusTime > 0)
					{
						flawlessMechMaskCooldown /= 3;
					}

					int baseDamage = 50;

					Vector2 position = Player.MountedCenter + new Vector2(Player.direction * 4, -11);
					Vector2 velocity = (Main.MouseWorld - position).SafeNormalize(new Vector2(0, 1));

					Projectile.NewProjectile(Player.GetSource_FromAI(), position, velocity, ProjectileType<FlawlessMechMaskDeathray>(), (int)Player.GetTotalDamage(DamageClass.Generic).ApplyTo(baseDamage), 0, Player.whoAmI);
				}
				//chestplate has arm swings
				if (flawlessMechChestplate)
				{
					if (Player.ownedProjectileCounts[ProjectileType<MiniPrimeArmSlash>()] == 0)
					{
						int baseDamage = 50;

						Vector2 position = Player.MountedCenter;
						Projectile.NewProjectile(Player.GetSource_FromAI(), position, Vector2.Zero, ProjectileType<MiniPrimeArmSlash>(), (int)Player.GetTotalDamage(DamageClass.Generic).ApplyTo(baseDamage), Player.GetTotalKnockback(DamageClass.Generic).ApplyTo(4f), Player.whoAmI);
					}
				}
				//tail has probes
				if (flawlessMechTail && flawlessMechTailCooldown == 0)
				{
					flawlessMechTailCooldown = MECH_TAIL_COOLDOWN;

					if (flawlessMechSetBonusTime > 0)
					{
						flawlessMechTailCooldown /= 3;
					}

					Vector2 position = Player.MountedCenter;
					Vector2 velocity = new Vector2(14, 0).RotatedByRandom(MathHelper.TwoPi);
					int baseDamage = 40;

					Projectile.NewProjectile(Player.GetSource_FromAI(), position, velocity, ProjectileType<MiniProbe>(), (int)Player.GetTotalDamage(DamageClass.Generic).ApplyTo(baseDamage), Player.GetTotalKnockback(DamageClass.Generic).ApplyTo(2f), Player.whoAmI);
				}
			}

			if (limestoneSetBonusHitCooldown > 0)
			{
				Player.statDefense = 0;
			}

            //update bubby vanity wing frames
            if (Player.velocity.Y == 0)
            {
                bubbyWingFrame = 0;
                bubbyWingFrameCounter = 1;
            }
            else
            {
                bubbyWingFrameCounter++;
                if (bubbyWingFrameCounter == 2)
                {
                    bubbyWingFrameCounter = 0;
                    bubbyWingFrame++;
                    if (bubbyWingFrame >= 7 && Player.velocity.Y < 0)
                    {
                        bubbyWingFrame = 1;
                    }
                    if ((bubbyWingFrame < 7 || bubbyWingFrame >= 15) && Player.velocity.Y > 0)
                    {
                        bubbyWingFrame = 7;
                    }
                }
            }

            Lighting.AddLight(Player.Center, light);
		}

        public void AddScreenShake(float magnitude, float timeLeft)
        {
			if (magnitude > 0 && timeLeft > 0)
			{
				float endTime = timeLeft + PolaritiesSystem.timer;
				if (screenShakes.ContainsKey(endTime))
				{
					screenShakes[endTime] += magnitude / timeLeft;
				}
				else
				{
					screenShakes.Add(endTime, magnitude / timeLeft);
				}
			}
		}

        public override void ModifyScreenPosition()
		{
			if (screenShakes.Keys.Count > 0)
			{
				List<float> removeTimesLeft = new List<float>();

				Polarities.preGeneratedRand.SetIndex(screenshakeRandomSeed);
				foreach (float timeLeft in screenShakes.Keys)
				{
					if (timeLeft <= PolaritiesSystem.timer)
					{
						removeTimesLeft.Add(timeLeft);
					}
					else
					{
						Main.screenPosition += new Vector2(Polarities.preGeneratedRand.NextNormallyDistributedFloat(screenShakes[timeLeft] * (timeLeft - (float)PolaritiesSystem.timer)), 0).RotatedBy(Polarities.preGeneratedRand.NextFloat(MathHelper.TwoPi));
					}
				}
				foreach (float timeLeft in removeTimesLeft) screenShakes.Remove(timeLeft);
			}

			//to prevent jittering of some things
            Main.screenPosition.X = (int)Main.screenPosition.X;
            Main.screenPosition.Y = (int)Main.screenPosition.Y;
        }

        public override void HideDrawLayers(PlayerDrawSet drawInfo)
		{
			if (drawInfo.drawPlayer?.HeldItem?.ModItem != null && drawInfo.drawPlayer.HeldItem.ModItem is IDrawHeldItem drawHeldItem)
			{
				if (!drawHeldItem.DoVanillaDraw())
					PlayerDrawLayers.HeldItem.Hide();
			}
			if (ArmorMasks.headIndexToArmorDraw.ContainsKey(drawInfo.drawPlayer.head))
            {
				if (!ArmorMasks.headIndexToArmorDraw[drawInfo.drawPlayer.head].DoVanillaDraw())
                {
					PlayerDrawLayers.Head.Hide();
				}
			}
			if (ArmorMasks.legIndexToArmorDraw.ContainsKey(drawInfo.drawPlayer.legs))
			{
				if (!ArmorMasks.legIndexToArmorDraw[drawInfo.drawPlayer.legs].DoVanillaDraw())
				{
					PlayerDrawLayers.Leggings.Hide();
				}
            }
            if (ArmorMasks.wingIndexToArmorDraw.ContainsKey(drawInfo.drawPlayer.wings))
            {
                if (!ArmorMasks.wingIndexToArmorDraw[drawInfo.drawPlayer.wings].DoVanillaDraw())
                {
                    PlayerDrawLayers.Wings.Hide();
                }
            }
        }

        public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
        {
			if (attempt.veryrare && !attempt.inLava && !attempt.inHoney && !attempt.crate && Player.ZoneForest)
            {
				if (Main.rand.NextBool(3)) {
					itemDrop = ItemType<Items.Weapons.Ranged.Atlatls.Axolatl>();
				}
            }
			if (attempt.veryrare && !attempt.inLava && !attempt.inHoney && !attempt.crate && Player.ZoneBeach)
			{
				if (Main.rand.NextBool(4))
				{
					itemDrop = ItemType<Items.Weapons.Melee.Warhammers.Warhammerhead>();
				}
			}
			if (Player.InModBiome(GetInstance<SaltCave>()) && !attempt.inLava && !attempt.inHoney)
			{
				if (attempt.crate)
				{
					if (attempt.uncommon)
						itemDrop = ItemType<SaltCrate>();
					return;
				}
				if ((!attempt.common && !attempt.uncommon && !attempt.rare && !attempt.veryrare && !attempt.legendary) || Main.rand.NextBool())
				{
					switch (Main.rand.Next(2))
					{
						case 0:
							itemDrop = ItemID.OldShoe;
							return;
						case 1:
							itemDrop = ItemID.TinCan;
							return;
					}
				}
				if (attempt.questFish == ItemType<PickledHerring>() && Main.rand.NextBool(3))
				{
					itemDrop = attempt.questFish;
					return;
				}
				if (attempt.common)
                {
					itemDrop = ItemType<Salt>();
                }
				if (attempt.uncommon)
                {
					itemDrop = ItemType<SaltCrystals>();
                }
				if (attempt.rare)
                {
					itemDrop = ItemType<BrineShrimp>();
				}
				if (attempt.veryrare)
                {
					if (Main.hardMode) itemDrop = ItemType<SaltKillifishStaff>();
                }
				if (attempt.legendary)
                {

                }
			}
        }

        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
			if (item.useAmmo == AmmoID.Dart)
			{
				damage *= dartDamage.Additive * dartDamage.Multiplicative;
				damage.Base += dartDamage.Base;
				damage.Flat += dartDamage.Flat;
			}
        }

        public override bool? CanHitNPC(Item item, NPC target)
		{
			if (target.GetGlobalNPC<PolaritiesNPC>().usesProjectileHitCooldowns && itemHitCooldown > 0)
			{
				return false;
			}

			return base.CanHitNPC(item, target);
		}

        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
		{
			OnHitNPCWithAnything(target, damage, knockback, crit, item.DamageType);

			if (target.GetGlobalNPC<PolaritiesNPC>().usesProjectileHitCooldowns)
			{
				itemHitCooldown = target.GetGlobalNPC<PolaritiesNPC>().projectileHitCooldownTime;
			}

			if (item.DamageType == DamageClass.Summon || item.DamageType.GetEffectInheritance(DamageClass.Summon))
			{
				royalOrbHitCount++;
			}
		}

        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
		{
			OnHitNPCWithAnything(target, damage, knockback, crit, proj.DamageType);

			if (proj.IsTypeSummon())
			{
				royalOrbHitCount++;
			}

			if ((proj.sentry || ProjectileID.Sets.SentryShot[proj.type]) && Player.HasBuff(BuffType<BetsyBookBuff>()))
            {
				target.AddBuff(BuffID.OnFire, 300);
				target.AddBuff(BuffID.OnFire3, 300);
			}
		}

		public void OnHitNPCWithAnything(NPC target, int damage, float knockback, bool crit, DamageClass damageClass)
		{
			if (snakescaleSetBonus && crit)
			{
				target.AddBuff(BuffID.Venom, 5 * 60);
			}

			if (moonLordLifestealCooldown == 0 && Player.HasBuff(BuffType<MoonLordBookBuff>()) && !Player.moonLeech)
			{
				float baseLifestealAmount = (float)Math.Log(damage * Math.Pow(Main.rand.NextFloat(1f), 4));
				if (baseLifestealAmount >= 1)
				{
					moonLordLifestealCooldown = 10;
					Player.statLife += (int)baseLifestealAmount;
					Player.HealEffect((int)baseLifestealAmount);
				}
			}

			if (damageClass != DamageClass.Magic && !damageClass.GetEffectInheritance(DamageClass.Magic) && solarEnergizer)
            {
				Player.statMana ++;
			}
		}

        public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
		{
			//TODO: (MAYBE) Replace with source propagation system once supported/if it doesn't end up being trivially supported, also move terraprisma to be obtained on any flawless run if/when this system is added
			for (int i = 0; i < Main.maxNPCs; i++)
            {
				if (Main.npc[i].active)
					Main.npc[i].GetGlobalNPC<PolaritiesNPC>().flawless = false;
			}

			if (strangeObituary)
			{
				Player.KillMe(PlayerDeathReason.ByCustomReason(Language.GetTextValueWith("Mods.Polarities.DeathMessage.StrangeObituary", new { PlayerName = Player.name })), 1.0, 0, false);
				return;
			}

			if (Player.HasBuff(BuffType<EyeOfCthulhuBookBuff>()))
			{
				Projectile.NewProjectile(Player.GetSource_Buff(Player.FindBuffIndex(BuffType<EyeOfCthulhuBookBuff>())), Player.Center, new Vector2(4, 0).RotatedByRandom(2 * Math.PI), ProjectileType<Items.Books.EyeOfCthulhuBookEye>(), 12, 3, Player.whoAmI);
			}

			if (skeletronBook && skeletronBookCooldown == 0)
			{
				skeletronBookCooldown = 3 * 60 * 60;
			}

			if (limestoneShield && limestoneShieldCooldown == 0)
			{
				limestoneShieldCooldown = 60 * 30;
			}

			if (limestoneSetBonus)
			{
				limestoneSetBonusHitCooldown = 300;
			}

			hydraHideTime = 120;

			justHit = true;
		}

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (ArmorMasks.bodyIndexToBodyMaskColor.ContainsKey(Player.body))
            {
				Color bodyMaskColor = ArmorMasks.bodyIndexToBodyMaskColor[Player.body].BodyColor(ref drawInfo);
				drawInfo.bodyGlowColor = bodyMaskColor;
				drawInfo.armGlowColor = bodyMaskColor;
			}
        }

        public override void PreUpdate()
		{
			//check for incineration by tile
			bool incinerating = false;
			for (int i = (int)((Player.position.X - 1) / 16); i < (int)((Player.position.X + 1 + Player.width) / 16) + 1; i++)
			{
				for (int j = (int)((Player.position.Y - 1) / 16); j < (int)((Player.position.Y + 1 + Player.height) / 16) + 1; j++)
				{
					if (Main.tile[i, j].TileType == TileType<MantellarOreTile>() || (Main.tile[i, j].TileType == TileType<BarTile>() && Main.tile[i, j].TileFrameX == 18))
					{
						incinerating = true;
					}
				}
			}
			if (incinerating)
			{
				Player.AddBuff(BuffType<Incinerating>(), Main.rand.Next(1, 3));
			}


			if (Main.expertMode && Framing.GetTileSafely(Player.Center.ToTileCoordinates()).WallType == WallType<RockSaltWallNatural>() && Player.wet && Player.adjWater)
			{
				Player.AddBuff(BuffType<Desiccating>(), 2);
			}

			if (Main.expertMode && Framing.GetTileSafely(Player.Center.ToTileCoordinates()).WallType == WallType<LimestoneWallNatural>())
			{
				if (Main.rand.NextBool(60) && Main.netMode != 1)
				{
					int positionX = ((int)(Player.Center.X + Main.rand.Next(-600, 600))) / 16;
					int positionY = ((int)(Player.Center.Y)) / 16 - 10;

					if (!Main.tile[positionX, positionY].HasUnactuatedTile)
					{
						for (int i = 0; i < Math.Min(1000, positionY); i++)
						{
							if (Main.tile[positionX, positionY - i].HasUnactuatedTile)
							{
								if (Main.tile[positionX, positionY - i].TileType == TileType<LimestoneTile>())
								{
									Projectile.NewProjectile(null, new Vector2(16 * positionX + 8, 16 * (positionY - i) + 16), Vector2.Zero, ProjectileType<Stalactite>(), Main.hardMode ? 24 : 12, 5f, Main.myPlayer);
								}
								if (Main.tileSolid[Main.tile[positionX, positionY - i].TileType])
								{
									break;
								}
							}
						}
					}
				}
			}
		}

        public override void PreUpdateMovement()
        {
            //apply dash if it exists
			if (Dash.HasDash(dashIndex) && CanUseAnyDash())
            {
				Dash dash = Dash.GetDash(dashIndex);

				if (dashDir != -1 && dashDelay == 0 && dash.TryUse(Player))
                {
					Vector2 newVelocity = Player.velocity;

					switch (dashDir)
					{
						case DashLeft when Player.velocity.X > -dash.Speed:
						case DashRight when Player.velocity.X < dash.Speed:
							{
								// X-velocity is set here
								float dashDirection = dashDir == DashRight ? 1 : -1;
								newVelocity.X = dashDirection * dash.Speed;
								break;
							}
						default:
							return; // not moving fast enough, so don't start our dash
					}

					// start our dash
					dashDelay = dash.Cooldown;
					dashTimer = dash.Duration;
					Player.velocity = newVelocity;

					dash.OnDash(Player);
				}
			}

			if (dashDelay > 0)
			{
				dashDelay--;
			}

			if (dashTimer > 0)
			{
				//dash is active
				if (Dash.HasDash(dashIndex) && CanUseAnyDash())
				{
					Dash dash = Dash.GetDash(dashIndex);

					dash.Update(Player, dashTimer);
				}

				// count down frames remaining
				dashTimer--;
			}

			Player.velocity *= velocityMultiplier;
		}

        bool CanUseAnyDash()
		{
			return Player.dashType == 0 && !Player.setSolar && !Player.mount.Active;
		}

        public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			ModifyHitNPCWithAnything(target, item.DamageType, ref damage, ref knockback, ref crit);
		}

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			ModifyHitNPCWithAnything(target, proj.DamageType, ref damage, ref knockback, ref crit);
		}

		public void ModifyHitNPCWithAnything(NPC target, DamageClass damageType, ref int damage, ref float knockback, ref bool crit)
		{
			if (damageType != DamageClass.Magic && damageType.GetModifierInheritance(DamageClass.Magic).damageInheritance == 0f && damageType.GetModifierInheritance(DamageClass.Generic).Equals(StatInheritanceData.Full))
            {
				damage = (int)(damage * nonMagicDamage.Additive * nonMagicDamage.Multiplicative);
            }

			if (target.HasBuff(BuffType<Pinpointed>()) && Main.rand.NextBool()) crit = true;

			target.GetGlobalNPC<PolaritiesNPC>().ignoredDefenseFromCritAmount = 0;
			if (crit)
			{
				damage = Math.Max(damage, (int)(damage * critDamageBoostMultiplier));
				target.GetGlobalNPC<PolaritiesNPC>().ignoredDefenseFromCritAmount = ignoreCritDefenseAmount;
			}
		}

        public override void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit)
		{
			ModifyHitByAnything(ref damage, ref crit);
		}

		public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref bool crit)
		{
			ModifyHitByAnything(ref damage, ref crit);
		}

		public void ModifyHitByAnything(ref int damage, ref bool crit)
		{
			if (Player.HasBuff(BuffType<Pinpointed>()) && Main.rand.NextBool()) crit = true;
		}

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (crit && !pvp)
            {
				damage = (int)(Main.CalculateDamagePlayersTake(damage, Player.statDefense)) * 2;
				customDamage = true;
            }
			return true;
        }

        private void Player_Update_NPCCollision(ILContext il)
		{
			ILCursor c = new ILCursor(il);

			if (!c.TryGotoNext(MoveType.After,
				i => i.MatchLdarg(0),
				i => i.MatchLdloc(1),
				i => i.MatchCall(typeof(Terraria.DataStructures.PlayerDeathReason).GetMethod("ByNPC", BindingFlags.Public | BindingFlags.Static)),
				i => i.MatchLdloc(11),
				i => i.MatchLdloc(10),
				i => i.MatchLdcI4(0),
				i => i.MatchLdcI4(0),
				i => i.MatchLdcI4(0)
				))
			{
				GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
				return;
			}

			//replace current stack thing with crit
			c.Emit(OpCodes.Pop);
			c.Emit(OpCodes.Ldloc, 14);
		}

		static readonly Color DamagedFriendlyCritFromEnemyColor = new Color(255, 0, 0);
		private void Player_Hurt(ILContext il)
		{
			ILCursor c = new ILCursor(il);

			if (!c.TryGotoNext(MoveType.After,
				i => i.MatchLdarg(6),
				i => i.MatchBrtrue(out _),
				i => i.MatchLdsfld(typeof(Terraria.CombatText).GetField("DamagedFriendly", BindingFlags.Public | BindingFlags.Static)),
				i => i.MatchBr(out _),
				i => i.MatchLdsfld(typeof(Terraria.CombatText).GetField("DamagedFriendlyCrit", BindingFlags.Public | BindingFlags.Static)),
				i => i.MatchStloc(8)
				))
			{
				GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
				return;
			}

			c.Emit(OpCodes.Ldloc, 8); //defaultColor
			c.Emit(OpCodes.Ldarg, 6); //Crit
			c.Emit(OpCodes.Ldarg, 4); //pvp
			c.EmitDelegate<Func<Color, bool, bool, Color>>((Color defaultColor, bool Crit, bool pvp) => {
				return (Crit && !pvp) ? DamagedFriendlyCritFromEnemyColor : defaultColor;
			});
			c.Emit(OpCodes.Stloc, 8);
		}

        public override void UpdateLifeRegen()
        {
			if (hydraHide && hydraHideTime > 0)
            {
                Player.lifeRegen += 10;
                Player.lifeRegenTime = 3600;
				Player.bleed = false; //force regen
            }
        }

        public override void NaturalLifeRegen(ref float regen)
        {
			bool ignoreMovementPenalty = false;

            if (hydraHide && hydraHideTime > 0)
            {
                regen *= 2f;
				ignoreMovementPenalty = true;
            }

			if (ignoreMovementPenalty && Player.velocity.X != 0f && Player.grappling[0] <= 0)
            {
				regen *= 2.5f;
            }
        }

        public override void UpdateBadLifeRegen()
        {
			if (coneVenom)
			{
				if (Player.lifeRegen > 0) Player.lifeRegen = 0;
				Player.lifeRegenTime = 0;
				Player.lifeRegen -= 70;
			}

            if (desiccation > 0)
            {
				if (Player.lifeRegen > 0) Player.lifeRegen = 0;
				Player.lifeRegenTime = 0;
				if (desiccation > 60 * 10)
				{
					Player.lifeRegen -= 60;
				}
			}

			if (incineration * 2 - incinerationResistanceTime > 0)
			{
				if (Player.lifeRegen > 0) Player.lifeRegen = 0;
				Player.lifeRegenTime = 0;
				Player.lifeRegen -= incineration * 2 - incinerationResistanceTime;
			}
		}

		//return true if we do vanilla life ticking, return false if we do our own
		public bool DoUpdateBadLifeRegen()
		{
			int incineratonLifeRegenValue = incineration * 2 - incinerationResistanceTime;
			if (incineratonLifeRegenValue > 0)
			{
				int lifeRegenStep = (int)Math.Ceiling(incineratonLifeRegenValue / 12f);
				while (Player.lifeRegenCount <= -lifeRegenStep * 120)
				{
					Player.lifeRegenCount += lifeRegenStep * 120;
					Player.statLife -= lifeRegenStep;
					CombatText.NewText(Player.Hitbox, CombatText.LifeRegen, lifeRegenStep, dramatic: false, dot: true);
					if (Player.statLife <= 0 && Player.whoAmI == Main.myPlayer)
					{
						Player.KillMe(PlayerDeathReason.ByCustomReason(Language.GetTextValueWith("Mods.Polarities.DeathMessage.Incinerating", new { PlayerName = Player.name })), 10.0, 0);
					}
				}
				return false;
			}

			if (coneVenom)
			{
				while (Player.lifeRegenCount <= -720)
				{
					Player.lifeRegenCount += 720;
					Player.statLife -= 6;
					CombatText.NewText(Player.Hitbox, CombatText.LifeRegen, 6, dramatic: false, dot: true);
					if (Player.statLife <= 0 && Player.whoAmI == Main.myPlayer)
					{
						Player.KillMe(PlayerDeathReason.ByCustomReason(Language.GetTextValueWith("Mods.Polarities.DeathMessage.ConeVenom", new { PlayerName = Player.name })), 10.0, 0);
					}
				}
				return false;
			}

			if (desiccation > 60 * 10)
            {
				while (Player.lifeRegenCount <= -600)
				{
					Player.lifeRegenCount += 600;
					Player.statLife -= 5;
					CombatText.NewText(Player.Hitbox, CombatText.LifeRegen, 5, dramatic: false, dot: true);
					if (Player.statLife <= 0 && Player.whoAmI == Main.myPlayer)
					{
						Player.KillMe(PlayerDeathReason.ByCustomReason(Language.GetTextValueWith("Mods.Polarities.DeathMessage.Desiccating", new { PlayerName = Player.name })), 10.0, 0);
					}
				}
				return false;
			}

			return true;
        }

		private void Player_UpdateLifeRegen(ILContext il)
		{
			ILCursor c = new ILCursor(il);

			if (!c.TryGotoNext(MoveType.Before,
				i => i.MatchLdarg(0),
				i => i.MatchLdfld(typeof(Player).GetField("burned", BindingFlags.Public | BindingFlags.Instance)),
				i => i.MatchBrtrue(out _),
				i => i.MatchLdarg(0),
				i => i.MatchLdfld(typeof(Player).GetField("suffocating", BindingFlags.Public | BindingFlags.Instance)),
				i => i.MatchBrtrue(out _),
				i => i.MatchLdarg(0),
				i => i.MatchLdfld(typeof(Player).GetField("tongued", BindingFlags.Public | BindingFlags.Instance)),
				i => i.MatchBrfalse(out _),
				i => i.MatchCall(typeof(Main).GetProperty("expertMode", BindingFlags.Public | BindingFlags.Static).GetGetMethod()),
				i => i.MatchBrfalse(out _)
				))
			{
				GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
				return;
			}

			ILLabel label = c.DefineLabel();
			label.Target = c.Next;

			c.Emit(OpCodes.Ldarg, 0);
			c.EmitDelegate<Func<Player, bool>>((player) =>
			{
				return player.GetModPlayer<PolaritiesPlayer>().DoUpdateBadLifeRegen();
			});
			c.Emit(OpCodes.Brtrue, label);
			c.Emit(OpCodes.Ret);
		}

		//modded dev armor
		public void TryGettingPolaritiesDevArmor(IEntitySource source)
		{
			if (Main.rand.NextBool(Main.tenthAnniversaryWorld ? 10 : 20))
			{
                switch (Main.rand.Next(3))
                {
                    case 0:
                        Player.QuickSpawnItem(source, ModContent.ItemType<TuringHead>());
                        Player.QuickSpawnItem(source, ModContent.ItemType<TuringBody>());
                        Player.QuickSpawnItem(source, ModContent.ItemType<TuringLegs>());
                        Player.QuickSpawnItem(source, ModContent.ItemType<TuringWings>());
                        break;
                    case 1:
                        Player.QuickSpawnItem(source, ModContent.ItemType<ElectroManiacHead>());
                        Player.QuickSpawnItem(source, ModContent.ItemType<ElectroManiacBody>());
                        Player.QuickSpawnItem(source, ModContent.ItemType<ElectroManiacLegs>());
                        break;
                    case 2:
                        Player.QuickSpawnItem(source, ModContent.ItemType<BubbyHead>());
                        Player.QuickSpawnItem(source, ModContent.ItemType<BubbyBody>());
                        Player.QuickSpawnItem(source, ModContent.ItemType<BubbyLegs>());
                        Player.QuickSpawnItem(source, ModContent.ItemType<BubbyWings>());
                        Player.QuickSpawnItem(source, ModContent.ItemType<VariableWispon>());
                        break;
                }
            }
		}
    }
}

