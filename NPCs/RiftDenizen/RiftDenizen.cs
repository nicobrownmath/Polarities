using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Polarities.Buffs;
using Polarities.Projectiles;
using Polarities.Items;
using Polarities.Items.Placeable;
using Polarities.Items.Weapons;
using Polarities.Items.Armor;
using Polarities.Items.Placeable.Trophies;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using ReLogic.Content;

namespace Polarities.NPCs.RiftDenizen
{
	[AutoloadBossHead]
	public class RiftDenizen : ModNPC
	{
		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[NPC.type] = 1;
		}

		public override void SetDefaults()
		{
			NPC.aiStyle = -1;
			NPC.width = 2;
			NPC.height = 2;

			NPC.lifeMax = Main.expertMode ? 10000 : 8000;

			NPC.knockBackResist = 0f;
			NPC.value = Item.buyPrice(gold: 8);
			NPC.npcSlots = 15f;
			NPC.boss = true;
			//bossBag/* tModPorter Note: Removed. Spawn the treasure bag alongside other loot via npcLoot.Add(ItemDropRule.BossBag(type)) */ = ItemType<RiftDenizenBag>();
			NPC.lavaImmune = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.chaseable = false;

			NPC.hide = true;

			for (int i = 0; i < Main.maxBuffTypes; i++)
			{
				NPC.buffImmune[i] = true;
			}

			Music = MusicLoader.GetMusicSlot("Polarities/Sounds/Music/RiftDenizen");
		}

		public override bool? CanBeHitByItem(Player player, Item item)
        {
			return false;
        }

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
			return false;
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
			position += new Vector2(0, 20);
            return true;
        }

        public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
        {
			return false;
        }

		private Vector2[] defaultGuidePoints = new Vector2[]
			{
				new Vector2(150, 0),
				new Vector2(100, 100),
				new Vector2(0, 400),
				new Vector2(-100, 100),
				new Vector2(-150, 0),
				new Vector2(-100, -100),
				new Vector2(0, -400),
				new Vector2(100, -100),
			};

		public override void AI()
		{
			if (!SkyManager.Instance["Polarities: Rift Denizen"].IsActive())
			{
				SkyManager.Instance.Activate("Polarities: Rift Denizen", NPC.Center);
			}
			Player player = Main.player[NPC.target];
			if (!player.active || player.dead)
			{
				NPC.TargetClosest(false);
				player = Main.player[NPC.target];
				if (player.dead)
				{
					NPC.dontTakeDamage = true;
					if (NPC.ai[0] < 12)
                    {
						NPC.ai[0] = 12;
						NPC.ai[1] = 0;
					}
				}
			}

			NPC.velocity = new Vector2((int)player.Center.X, (int)player.Center.Y) + new Vector2(0, -300 / Main.GameZoomTarget) - NPC.Center;

			//AI cycling
			switch (NPC.ai[0])
			{
				case 0:
					SpawnAnimation();
					break;
				case 1:
					RiftChordProjectiles();
					break;
				case 2:
					RiftsClaws();
					break;
				case 3:
					RiftBurstProjectiles();
					break;
				case 4:
					StarPolygonRifts();
					break;
				case 5:
					RiftClawsBig();
					break;
				case 6:
					RiftsConnectingRays();
					break;
				case 7:
					ColumnsAndMeleeRifts();
					break;
				case 8:
				case 9:
					ParallaxRay();
					break;
				case 10:
				case 12:
					DeathAnimation();
					break;
				case 11:
					NPC.life = 0;
					NPC.checkDead();
					break;
				case 13:
					NPC.active = false;
					break;
			}
		}

		private int parallaxRays;
		private int maxAttackUnlocked = 1;

		private void PhaseTransition()
		{
			if ((NPC.life < 2 * NPC.lifeMax / 3 && parallaxRays == 0) || (NPC.life < NPC.lifeMax / 3 && parallaxRays == 1))
			{
				NPC.ai[0] = 8;
				parallaxRays++;
			}
			else if (NPC.dontTakeDamage)
			{
				NPC.ai[0]++;

				if (NPC.ai[0] < 9)
					NPC.ai[0] = 9;
			}
			else if (NPC.ai[0] == 0 || NPC.ai[0] == 8)
			{
				NPC.ai[0] = Main.rand.Next(1, Math.Min(maxAttackUnlocked, 7) + 1);

				if (maxAttackUnlocked < 8 && Main.rand.NextBool())
                {
					NPC.ai[0] = maxAttackUnlocked;
                }
			}
			else
			{
				NPC.ai[0] = (Main.rand.Next(0, Math.Min(maxAttackUnlocked, 7) - 1) + NPC.ai[0]) % Math.Min(maxAttackUnlocked, 7) + 1;

				if (maxAttackUnlocked < 8 && Main.rand.NextBool())
				{
					NPC.ai[0] = maxAttackUnlocked;
				}
			}
			if (NPC.ai[0] == maxAttackUnlocked && maxAttackUnlocked < 8)
			{
				maxAttackUnlocked++;
			}
		}

		private void SpawnAnimation()
		{
			RiftDenizenSky.xScale = Math.Min(1, Math.Max(1 / 8f, (NPC.ai[1] - 21) / 8f));
			RiftDenizenSky.yScale = Math.Min(1, Math.Max(0, NPC.ai[1] / 8f));
			RiftDenizenSky.skyAlpha = 1f;
			RiftDenizenSky.concavityFactor = 0.125f;

			//set boss guide points to defaults
			for (int i = 0; i < RiftDenizenSky.guidePoints.Length; i++) {
				RiftDenizenSky.guidePoints[i] = defaultGuidePoints[i];
			}

			if (NPC.ai[1] == 30)
			{
				SoundEngine.PlaySound(Polarities.GetSound("ReverbyScythe", 0.75f), NPC.Center);
			}

			NPC.ai[1]++;
			if (NPC.ai[1] == 60)
			{
				NPC.ai[1] = 0;
				PhaseTransition();
			}
		}

		private void DeathAnimation()
		{
			RiftDenizenSky.xScale = Math.Min(1, Math.Max(1 / 8f, (189 - NPC.ai[1]) / 8f));
			RiftDenizenSky.yScale = Math.Min(1, Math.Max(0, (210 - NPC.ai[1]) / 8f));
			RiftDenizenSky.skyAlpha = Math.Min(1, Math.Max(0, (240 - NPC.ai[1]) / 30f));

			//animation stuff
			//return to defaults, but modified
			if (NPC.ai[0] == 10)
			{
				for (int i = 0; i < RiftDenizenSky.guidePoints.Length; i++)
				{
					RiftDenizenSky.guidePoints[i] = (RiftDenizenSky.guidePoints[i] * 9 + defaultGuidePoints[i] * (480 - NPC.ai[1]) / 480 + new Vector2(NPC.ai[1] / 8f, 0).RotatedBy(MathHelper.Pi * NPC.ai[1] / 190f + 3f * i * MathHelper.TwoPi / RiftDenizenSky.guidePoints.Length)) / 10;
				}
			}
			else
			{
				for (int i = 0; i < RiftDenizenSky.guidePoints.Length; i++)
				{
					RiftDenizenSky.guidePoints[i] = (RiftDenizenSky.guidePoints[i] * 9 + defaultGuidePoints[i]) / 10;
				}
			}
			RiftDenizenSky.concavityFactor = 0.125f;

			if (NPC.ai[1] == 210 && NPC.ai[0] == 10)
			{
				SoundEngine.PlaySound(Polarities.GetSound("ReverbyRoar"), NPC.Center);

				NPC.NPCLoot();
			}

			NPC.ai[1]++;
			if (NPC.ai[1] == 240)
			{
				NPC.ai[1] = 0;
				PhaseTransition();
			}
		}

		private void RiftChordProjectiles()
		{
			Player player = Main.player[NPC.target];

			if (NPC.ai[1] == 60 || NPC.ai[1] == 120 || NPC.ai[1] == 180 || NPC.ai[1] == 240)
			{
				float rotationOffset = player.velocity == Vector2.Zero ? Main.rand.NextFloat(MathHelper.TwoPi) : player.velocity.ToRotation();
				for (int i = 0; i < 6; i++)
				{
					Vector2 spawnPosition = player.Center + new Vector2(600, 0).RotatedBy(rotationOffset + i * MathHelper.TwoPi / 6);

					NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPosition.X, (int)spawnPosition.Y + 32, NPCType<RiftOpening>(), ai1: 0);
				}

				//pulse
				for (int i = 0; i < RiftDenizenSky.guidePoints.Length; i++)
				{
					RiftDenizenSky.guidePoints[i] = RiftDenizenSky.guidePoints[i] * 1.1f;
				}
			}

			//return to defaults
			for (int i = 0; i < RiftDenizenSky.guidePoints.Length; i++)
			{
				RiftDenizenSky.guidePoints[i] = (RiftDenizenSky.guidePoints[i] * 9 + defaultGuidePoints[i]) / 10;
			}
			RiftDenizenSky.concavityFactor = 0.125f;

			NPC.ai[1]++;
			if (NPC.ai[1] == 300)
			{
				NPC.ai[1] = 0;
				PhaseTransition();
			}
		}

		private void RiftBurstProjectiles()
		{
			Player player = Main.player[NPC.target];

			if (NPC.ai[1] == 75 || NPC.ai[1] == 150 || NPC.ai[1] == 225 || NPC.ai[1] == 300)
			{
				float rotationOffset = player.velocity == Vector2.Zero ? Main.rand.NextFloat(MathHelper.TwoPi) : player.velocity.ToRotation();

				if (NPC.life < NPC.lifeMax / 2)
				{
					for (int i = -1; i <= 1; i++)
					{
						Vector2 spawnPosition = player.Center + new Vector2(600, 0).RotatedBy(rotationOffset + i * MathHelper.TwoPi / 28);
						NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPosition.X, (int)spawnPosition.Y + 32, NPCType<RiftOpening>(), ai1: 2);
					}
					for (int i = -4; i <= -2; i++)
					{
						Vector2 spawnPosition = player.Center + new Vector2(660, 0).RotatedBy(rotationOffset + i * MathHelper.TwoPi / 28);
						NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPosition.X, (int)spawnPosition.Y + 32, NPCType<RiftOpening>(), ai1: 5, ai2: 90, ai3: (player.Center - spawnPosition).ToRotation());
					}
					for (int i = 2; i <= 4; i++)
					{
						Vector2 spawnPosition = player.Center + new Vector2(660, 0).RotatedBy(rotationOffset + i * MathHelper.TwoPi / 28);
						NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPosition.X, (int)spawnPosition.Y + 32, NPCType<RiftOpening>(), ai1: 5, ai2: 90, ai3: (player.Center - spawnPosition).ToRotation());
					}
				}
				else
				{
					for (int i = -1; i <= 1; i++)
					{
						Vector2 spawnPosition = player.Center + new Vector2(570, 0).RotatedBy(rotationOffset + i * MathHelper.TwoPi / 28);
						NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPosition.X, (int)spawnPosition.Y + 32, NPCType<RiftOpening>(), ai1: 2);
					}
				}

				//directional pulse
				int intOffset = (int)(8 * (rotationOffset > 0 ? rotationOffset : rotationOffset + MathHelper.TwoPi) / MathHelper.TwoPi + 7.5f) % 8;
				for (int i = 0; i < 3; i++)
				{
					RiftDenizenSky.guidePoints[(i + intOffset) % 8] *= 1.1f;
				}
			}

			//animation stuff

			//return to defaults
			for (int i = 0; i < RiftDenizenSky.guidePoints.Length; i++)
			{
				RiftDenizenSky.guidePoints[i] = (RiftDenizenSky.guidePoints[i] * 9 + defaultGuidePoints[i]) / 10;
			}
			RiftDenizenSky.concavityFactor = 0.125f;

			NPC.ai[1]++;
			if (NPC.ai[1] == 360)
			{
				NPC.ai[1] = 0;
				PhaseTransition();
			}
		}

		private void RiftsClaws()
		{
			Player player = Main.player[NPC.target];

			if (NPC.ai[1] == 30 || NPC.ai[1] == 90 || NPC.ai[1] == 150)
			{
				float rotationOffset = player.velocity == Vector2.Zero ? Main.rand.NextFloat(MathHelper.TwoPi) : player.velocity.ToRotation();
				Vector2 spawnPosition = player.Center + new Vector2(240, 0).RotatedBy(rotationOffset);
				NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPosition.X, (int)spawnPosition.Y + 32, NPCType<RiftOpening>(), ai1: 1);

				//animation stuff
				//directional pulse
				int i = (int)(8 * (rotationOffset > 0 ? rotationOffset : rotationOffset + MathHelper.TwoPi) / MathHelper.TwoPi + 8.5f) % 8;
				RiftDenizenSky.guidePoints[i] *= 1.2f;
			}

			//animation stuff
			//return to defaults, but modified
			for (int i = 0; i < RiftDenizenSky.guidePoints.Length; i++)
			{
				RiftDenizenSky.guidePoints[i] = (RiftDenizenSky.guidePoints[i] * 9 + defaultGuidePoints[i] + new Vector2(3f * NPC.ai[1] * (330 - NPC.ai[1]) / 27225f, 0).RotatedBy(NPC.ai[1] / 5f + 3f * i * MathHelper.TwoPi / RiftDenizenSky.guidePoints.Length)) / 10;
			}
			RiftDenizenSky.concavityFactor = 0.125f;

			NPC.ai[1]++;
			if (NPC.ai[1] == 330)
			{
				NPC.ai[1] = 0;
				PhaseTransition();
			}
		}

		private void StarPolygonRifts()
		{
			Player player = Main.player[NPC.target];

			if (NPC.ai[1] == 0)
			{
				//interpolate over even integers from 8 to 16
				int numRifts = (int)((1 - NPC.life / (float)NPC.lifeMax) * 5 + 4) * 2;

				float rotationOffset = player.velocity == Vector2.Zero ? Main.rand.NextFloat(MathHelper.TwoPi) : player.velocity.ToRotation();

				float radius = numRifts * 80f;

				for (int i = 0; i < numRifts; i++)
				{
					Vector2 spawnPosition = player.Center + new Vector2(radius / 2, 0).RotatedBy(rotationOffset) + new Vector2(radius, 0).RotatedBy(rotationOffset + i * MathHelper.TwoPi / numRifts);

					NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPosition.X, (int)spawnPosition.Y + 32, NPCType<RiftOpening>(), ai0: i, ai1: 3, ai2: numRifts, ai3: rotationOffset + i * MathHelper.TwoPi / numRifts);
				}
			}

			//animation stuff

			//return to defaults but modified
			if (NPC.ai[1] < 220)
			{
				for (int i = 0; i < RiftDenizenSky.guidePoints.Length; i++)
				{
					RiftDenizenSky.guidePoints[i] = (RiftDenizenSky.guidePoints[i] * 9 + defaultGuidePoints[i] * (1 + NPC.ai[1] / 880)) / 10;
				}
				RiftDenizenSky.concavityFactor = 0.125f * (220 - NPC.ai[1]) / 220;
			}
			else
			{
				for (int i = 0; i < RiftDenizenSky.guidePoints.Length; i++)
				{
					RiftDenizenSky.guidePoints[i] = (RiftDenizenSky.guidePoints[i] * 9 + defaultGuidePoints[i]) / 10;
				}
				RiftDenizenSky.concavityFactor = (RiftDenizenSky.concavityFactor * 5 + 0.125f) / 6;
			}

			NPC.ai[1]++;
			if (NPC.ai[1] == 240)
			{
				NPC.ai[1] = 0;
				PhaseTransition();
			}
		}

		private void RiftClawsBig()
		{
			Player player = Main.player[NPC.target];

			if (NPC.ai[1] == 30)
			{
				int numRifts = 8;
				float radius = 400f;

				for (int i = 0; i < numRifts; i++)
				{
					Vector2 spawnPosition = player.Center + new Vector2(radius, 0).RotatedBy(i * MathHelper.TwoPi / numRifts);

					NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPosition.X, (int)spawnPosition.Y + 32, NPCType<RiftOpening>(), ai1: 4);
				}
			}

			//animation stuff
			if (NPC.ai[1] == 30)
			{
				for (int i = 0; i < RiftDenizenSky.guidePoints.Length; i++)
				{
					RiftDenizenSky.guidePoints[i] = RiftDenizenSky.guidePoints[i] * 1.1f;
				}
			}

			if (NPC.ai[1] >= 120 && NPC.ai[1] < 150)
			{
				//scale to rotatable size
				for (int i = 0; i < RiftDenizenSky.guidePoints.Length; i++)
				{
					RiftDenizenSky.guidePoints[i] = (RiftDenizenSky.guidePoints[i] * 9 + defaultGuidePoints[i].SafeNormalize(Vector2.Zero) * 200) / 10;;
				}
			}
			else if (NPC.ai[1] >= 150 && NPC.ai[1] < 210)
			{
				for (int i = 0; i < RiftDenizenSky.guidePoints.Length; i++)
				{
					RiftDenizenSky.guidePoints[i] = defaultGuidePoints[i].SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.Pi * (1 - Math.Cos((NPC.ai[1] - 150) / 60 * MathHelper.Pi))) * 200;
				}
			}
			else
			{
				//return to defaults
				for (int i = 0; i < RiftDenizenSky.guidePoints.Length; i++)
				{
					RiftDenizenSky.guidePoints[i] = (RiftDenizenSky.guidePoints[i] * 9 + defaultGuidePoints[i]) / 10;
				}
			}
			RiftDenizenSky.concavityFactor = 0.125f;

			NPC.ai[1]++;
			if (NPC.ai[1] == 210)
			{
				NPC.ai[1] = 0;
				PhaseTransition();
			}
		}

		private void ColumnsAndMeleeRifts()
		{
			Player player = Main.player[NPC.target];

			if (NPC.ai[1] % 30 == 0 && NPC.ai[1] >= 120 && NPC.ai[1] < 480)
			{
				Vector2 spawnPosition = player.Center;
				NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPosition.X, (int)spawnPosition.Y + 32, NPCType<RiftOpening>(), ai1: 5, ai2: 40, ai3: Main.rand.NextFloat(MathHelper.TwoPi));
			}

			if (NPC.ai[1] % 120 == 60 && NPC.ai[1] >= 60 && NPC.ai[1] < 480)
			{
				for (int i = -10; i <= 10; i++)
				{
					Vector2 spawnPosition = player.Center + new Vector2(i * 160, -700);
					NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPosition.X, (int)spawnPosition.Y + 32, NPCType<RiftOpening>(), ai0: 0, ai1: 6, ai2: 1400f, ai3: -MathHelper.PiOver2);

					spawnPosition = player.Center + new Vector2(i * 160, 700);
					NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPosition.X, (int)spawnPosition.Y + 32, NPCType<RiftOpening>(), ai1: 5, ai2: 85, ai3: -MathHelper.PiOver2);
				}

				for (int i = 0; i < RiftDenizenSky.guidePoints.Length; i++)
				{
					if (i != 0 && i != 4)
						RiftDenizenSky.guidePoints[i] *= 1.1f;
				}
			}

			//animation stuff

			//return to defaults
			for (int i = 0; i < RiftDenizenSky.guidePoints.Length; i++)
			{
				RiftDenizenSky.guidePoints[i] = (RiftDenizenSky.guidePoints[i] * 9 + defaultGuidePoints[(i + 2) % 8].RotatedBy(-MathHelper.PiOver2)) / 10;
			}
			RiftDenizenSky.concavityFactor = 0.125f;

			NPC.ai[1]++;
			if (NPC.ai[1] == 510)
			{
				NPC.ai[1] = 0;
				PhaseTransition();
			}
		}

		private void RiftsConnectingRays()
		{
			Player player = Main.player[NPC.target];

			if (NPC.ai[1] == 30 || NPC.ai[1] == 45 || NPC.ai[1] == 60 || NPC.ai[1] == 75 || NPC.ai[1] == 90 || NPC.ai[1] == 105 || NPC.ai[1] == 120)
			{
				float rotationOffset = player.velocity == Vector2.Zero ? Main.rand.NextFloat(MathHelper.TwoPi) : player.velocity.ToRotation();

				int numRifts = 2;
				float radius = 600f;

				for (int i = 0; i < numRifts; i++)
				{
					Vector2 spawnPosition = player.Center + new Vector2(radius, 0).RotatedBy(rotationOffset + i * MathHelper.TwoPi / numRifts);

					if (i == 0)
					{
						NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPosition.X, (int)spawnPosition.Y + 32, NPCType<RiftOpening>(), ai0: 1, ai1: 6, ai2: 1200f, ai3: rotationOffset + i * MathHelper.TwoPi / numRifts);
					}
					else
					{
						NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPosition.X, (int)spawnPosition.Y + 32, NPCType<RiftOpening>(), ai1: 5, ai2: 80, ai3: rotationOffset + i * MathHelper.TwoPi / numRifts);
					}
				}

				//directional pulse
				for (int i = (int)(8 * (rotationOffset > 0 ? rotationOffset : rotationOffset + MathHelper.TwoPi) / MathHelper.TwoPi + 0.5f) % 4; i < RiftDenizenSky.guidePoints.Length; i += 4)
				{
					RiftDenizenSky.guidePoints[i] *= 1.2f;
				}
			}

			//animation stuff

			//return to defaults
			for (int i = 0; i < RiftDenizenSky.guidePoints.Length; i++)
			{
				RiftDenizenSky.guidePoints[i] = (RiftDenizenSky.guidePoints[i] * 9 + defaultGuidePoints[i]) / 10;
			}
			RiftDenizenSky.concavityFactor = 0.125f;

			NPC.ai[1]++;
			if (NPC.ai[1] == 180)
			{
				NPC.ai[1] = 0;
				PhaseTransition();
			}
		}

		private void ParallaxRay()
		{
			Player player = Main.player[NPC.target];

			if (NPC.ai[1] == 45)
			{
				SoundEngine.PlaySound(Polarities.GetSound("ParallaxRay", 0.75f), player.Center);
			}

			if (NPC.ai[1] == 60 && Main.netMode != 1)
			{
				Projectile.NewProjectile(NPC.GetSource_FromAI(),player.Center, Vector2.Zero, ProjectileType<ParallaxRay>(), 40, 0f, Main.myPlayer, player.whoAmI);
				Projectile.NewProjectile(NPC.GetSource_FromAI(),player.Center, Vector2.Zero, ProjectileType<ParallaxRayBehindTiles>(), 40, 0f, Main.myPlayer, player.whoAmI);
			}

			//faster for the desperation attack
			if (((NPC.ai[0] == 8 && NPC.ai[1] % 60 == 30) || (NPC.ai[0] == 9 && NPC.ai[1] % 40 == 30)) && NPC.ai[1] >= 150 && NPC.ai[1] < 630 && Main.netMode != 1)
            {
				for (int i = 1; i < 4; i++)
                {
					for (int j = 0; j < i * 6; j++)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(),player.Center + new Vector2(200 * i, 0).RotatedBy(player.velocity.ToRotation() + j * MathHelper.TwoPi / (i * 6)), Vector2.Zero, ProjectileType<ParallaxBolt>(), 15, 0f, Main.myPlayer, player.whoAmI);
						Projectile.NewProjectile(NPC.GetSource_FromAI(),player.Center + new Vector2(200 * i, 0).RotatedBy(player.velocity.ToRotation() + j * MathHelper.TwoPi / (i * 6)), Vector2.Zero, ProjectileType<ParallaxBoltBehindTiles>(), 15, 0f, Main.myPlayer, player.whoAmI);
					}
                }
			}

			//animation stuff

			//return to defaults but modified
			if (NPC.ai[1] < 600)
			{
				for (int i = 0; i < RiftDenizenSky.guidePoints.Length; i++)
				{
					RiftDenizenSky.guidePoints[i] = (RiftDenizenSky.guidePoints[i] * 9 + defaultGuidePoints[i].SafeNormalize(Vector2.Zero) * 200 * (1 + NPC.ai[1] / 600) + new Vector2(30f * NPC.ai[1] / 600f, 0).RotatedBy(NPC.ai[1] / 20f + 2f * i * MathHelper.TwoPi / RiftDenizenSky.guidePoints.Length)) / 10;
				}
				RiftDenizenSky.concavityFactor = 0.125f * (600 - NPC.ai[1]) / 600;
			}
			else
			{
				for (int i = 0; i < RiftDenizenSky.guidePoints.Length; i++)
				{
					RiftDenizenSky.guidePoints[i] = (RiftDenizenSky.guidePoints[i] * 9 + defaultGuidePoints[i]) / 10;
				}
				RiftDenizenSky.concavityFactor = (RiftDenizenSky.concavityFactor * 5 + 0.125f) / 6;
			}

			NPC.ai[1]++;
			if (NPC.ai[1] == 660)
			{
				NPC.ai[1] = 0;
				PhaseTransition();
			}
		}

		public override bool CheckActive()
		{
			return true;
		}

		public override bool CheckDead()
        {
			if (NPC.ai[0] == 11)
			{
				if (!PolaritiesSystem.downedRiftDenizen)
				{
					//PolaritiesSystem.GenFractalAltar();

					PolaritiesSystem.downedRiftDenizen = true;
				}

				NPC.active = false;
				return false;
            }
			else
            {
				NPC.dontTakeDamage = true;
				NPC.life = 1;
				return false;
            }
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
			potionType = ItemID.HealingPotion;
        }

		public override void OnKill()
		{
			//if (Main.rand.NextBool(10) || NPC.GetGlobalNPC<PolaritiesNPC>().noHit)
			//{
			//	Item.NewItem(NPC.Hitbox, ItemType<RiftDenizenTrophy>());
			//}
			//if (NPC.GetGlobalNPC<PolaritiesNPC>().noHit)
			//{
			//	NPC.DropItemInstanced(NPC.position, NPC.Size, ItemType<BeyondBow>());
			//}
			//if (Main.expertMode)
			//{
			//	NPC.DropBossBags();
			//	/*if (Main.rand.NextBool(4))
			//	{
			//		Item.NewItem(npc.getRect(), ItemType<?>());
			//	}*/
			//}
			//else
			//{
			//	if (Main.rand.NextBool(7))
			//	{
			//		Item.NewItem(NPC.Hitbox, ItemType<RiftDenizenMask>());
			//	}

			//	Item.NewItem(NPC.Hitbox, ItemType<Tiles.Furniture.FractalAssemblerItem>());

			//	switch (Main.rand.Next(2))
			//	{
			//		case 0:
			//			Item.NewItem(NPC.Hitbox, ItemType<EndlessHookItem>());
			//			break;
			//		case 1:
			//			Item.NewItem(NPC.Hitbox, ItemType<Items.Accessories.DimensionalAnchor>());
			//			break;
			//	}

			//	switch (Main.rand.Next(4))
			//	{
			//		case 0:
			//			Item.NewItem(NPC.Hitbox, ItemType<InstabilityScepter>());
			//			break;
			//		case 1:
			//			Item.NewItem(NPC.Hitbox, ItemType<Parallaxian>());
			//			break;
			//		case 2:
			//			Item.NewItem(NPC.Hitbox, ItemType<RiftonaStick>());
			//			break;
			//		case 3:
			//			Item.NewItem(NPC.Hitbox, ItemType<ObserverOrb>());
			//			break;
			//	}
			//}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			return false;
		}
	}

	public class RiftOpening : ModNPC
	{
		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[NPC.type] = 1;
		}

		public override void SetDefaults()
		{
			NPC.aiStyle = -1;
			NPC.width = 64;
			NPC.height = 64;

			NPC.lifeMax = Main.expertMode ? 10000 : 8000;
			NPC.defense = 20;
			NPC.damage = 30;

			NPC.HitSound = SoundID.NPCHit23;

			NPC.knockBackResist = 0f;
			NPC.npcSlots = 0f;
			NPC.dontCountMe = true;
			NPC.lavaImmune = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;

			NPC.buffImmune[BuffID.Confused] = true;

			//npc.GetGlobalNPC<PolaritiesNPC>().usesProjectileHitCooldowns = true;
			//npc.GetGlobalNPC<PolaritiesNPC>().projectileHitCooldownTime = 10;
		}

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
			return false;
        }

        public override void AI()
		{
			NPC.realLife = NPC.FindFirstNPC(NPCType<RiftDenizen>());
			NPC boss = Main.npc[NPC.realLife];

			NPC.dontTakeDamage = boss.dontTakeDamage || Math.Abs(NPC.localAI[0]) < 30;

			if (NPC.localAI[0] < 0)
			{
				NPC.damage = 0;
				NPC.localAI[0]++;
				if (NPC.localAI[0] == 0)
                {
					NPC.active = false;
					return;
                }
            }

			if (!boss.active || boss.ai[0] >= 10)
            {
				NPC.localAI[0] = Math.Max(-30, -NPC.localAI[0]);
				return;
			}

			NPC.target = boss.target;
			Player player = Main.player[NPC.target];

			switch (NPC.ai[1])
			{
				case 0:
					//projectile chord rift
					if (NPC.localAI[0] == 0)
					{
						NPC.rotation = (player.Center - NPC.Center).ToRotation();
					}

					if (Main.netMode != 1)
					{
						if (NPC.localAI[0] == 70)
						{
							Projectile.NewProjectile(NPC.GetSource_FromAI(),NPC.Center, new Vector2(32, 0).RotatedBy(NPC.rotation), ProjectileType<RiftBolt>(), 15, 2f, Main.myPlayer, ai0: 56);
						}
						if (NPC.localAI[0] == 50 || NPC.localAI[0] == 60 || NPC.localAI[0] == 70)
						{
							Projectile.NewProjectile(NPC.GetSource_FromAI(),NPC.Center, new Vector2((float)Math.Sqrt(3) * 16, 0).RotatedBy(MathHelper.Pi / 6).RotatedBy(NPC.rotation), ProjectileType<RiftBolt>(), 15, 2f, Main.myPlayer, ai0: 56);
							Projectile.NewProjectile(NPC.GetSource_FromAI(),NPC.Center, new Vector2((float)Math.Sqrt(3) * 16, 0).RotatedBy(-MathHelper.Pi / 6).RotatedBy(NPC.rotation), ProjectileType<RiftBolt>(), 15, 2f, Main.myPlayer, ai0: 56);
						}
						if (NPC.localAI[0] == 30 || NPC.localAI[0] == 40 || NPC.localAI[0] == 50 || NPC.localAI[0] == 60 || NPC.localAI[0] == 70)
						{
							Projectile.NewProjectile(NPC.GetSource_FromAI(),NPC.Center, new Vector2(16, 0).RotatedBy(MathHelper.Pi / 3).RotatedBy(NPC.rotation), ProjectileType<RiftBolt>(), 15, 2f, Main.myPlayer, ai0: 56);
							Projectile.NewProjectile(NPC.GetSource_FromAI(),NPC.Center, new Vector2(16, 0).RotatedBy(-MathHelper.Pi / 3).RotatedBy(NPC.rotation), ProjectileType<RiftBolt>(), 15, 2f, Main.myPlayer, ai0: 56);
						}
					}

					if (NPC.localAI[0] > 140)
					{
						NPC.localAI[0] = -30;
					}
					break;
				case 1:
					//small claw rift
					if (NPC.localAI[0] == 0)
					{
						NPC.rotation = (player.Center - NPC.Center).ToRotation();
					}

					if (Main.netMode != 1 && NPC.localAI[0] == 60)
					{
						for (int i = 0; i < 4; i++)
						{
							Projectile.NewProjectile(NPC.GetSource_FromAI(),NPC.Center, (player.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(i * MathHelper.TwoPi / 4), ProjectileType<RiftClaw>(), 15, 2f, Main.myPlayer, ai0: NPC.whoAmI, ai1: MathHelper.TwoPi);
						}
					}

					if (NPC.localAI[0] > 240)
					{
						NPC.localAI[0] = -30;
					}
					break;
				case 2:
					//burst rift
					if (NPC.localAI[0] == 0)
					{
						NPC.rotation = (player.Center - NPC.Center).ToRotation();
					}

					if (Main.netMode != 1 && (NPC.localAI[0] == 60 || NPC.localAI[0] == 70 || NPC.localAI[0] == 80))
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(),NPC.Center, (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 16, ProjectileType<RiftBolt>(), 15, 2f, Main.myPlayer, ai0: 600);
						Projectile.NewProjectile(NPC.GetSource_FromAI(),NPC.Center, (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 20, ProjectileType<RiftBolt>(), 15, 2f, Main.myPlayer, ai0: 600);
						Projectile.NewProjectile(NPC.GetSource_FromAI(),NPC.Center, (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 24, ProjectileType<RiftBolt>(), 15, 2f, Main.myPlayer, ai0: 600);
					}

					if (NPC.localAI[0] > 90)
					{
						NPC.localAI[0] = -30;
					}
					break;
				case 3:
					//chords rift
					if (NPC.localAI[0] == 0)
					{
						NPC.rotation = NPC.ai[3] + MathHelper.Pi;
					}

					if (NPC.localAI[0] == 30 && Main.netMode != 1)
					{
						for (int i = 0; i <= ((int)NPC.ai[2] / 2 - 1); i++)
						{
							int extraTime = (int)((((int)NPC.ai[2] / 2 - 1) - Math.Abs(i)) * (30 * 4f / ((int)NPC.ai[2] / 2 - 1)));
							float rayDistance = (NPC.ai[2] * 80f) * (float)Math.Sqrt(2 + 2 * Math.Cos(i * MathHelper.TwoPi / NPC.ai[2]));
							float activationTime = 60 + extraTime;

							if (i != 0 || NPC.ai[0] < NPC.ai[2] / 2)
								Projectile.NewProjectile(NPC.GetSource_FromAI(),NPC.Center, new Vector2(1, 0).RotatedBy(NPC.rotation + i * MathHelper.Pi / NPC.ai[2]), ProjectileType<ChordRay>(), 15, 0f, Main.myPlayer, ai0: rayDistance, ai1: activationTime);
						}
					}

					if (NPC.localAI[0] > 270)
					{
						NPC.localAI[0] = -30;
					}
					break;
				case 4:
					//big claw rift
					if (NPC.localAI[0] == 0)
					{
						NPC.rotation = (player.Center - NPC.Center).ToRotation();
					}

					if (Main.netMode != 1 && NPC.localAI[0] == 60)
					{
						for (int i = 0; i < 8; i++)
						{
							Projectile.NewProjectile(NPC.GetSource_FromAI(),NPC.Center, new Vector2(1.1381f, 0).RotatedBy(NPC.rotation + i * MathHelper.TwoPi / 8), ProjectileType<RiftClaw>(), 15, 3f, Main.myPlayer, ai0: NPC.whoAmI, ai1: MathHelper.TwoPi - MathHelper.Pi / 8);
						}
					}

					if (NPC.localAI[0] > 240)
					{
						NPC.localAI[0] = -30;
					}
					break;
				case 5:
					//trivial rift
					if (NPC.localAI[0] == 0)
					{
						NPC.rotation = NPC.ai[3] + MathHelper.Pi;
					}

					if (NPC.localAI[0] > NPC.ai[2])
					{
						NPC.localAI[0] = -30;
					}
					break;
				case 6:
					//ray sniping rift
					if (NPC.localAI[0] == 0)
					{
						NPC.rotation = NPC.ai[3] + MathHelper.Pi;
					}

					float time = 30;
					if (NPC.ai[0] == 0)
					{
						time = 35;
					}

					if (NPC.localAI[0] == 20 && Main.netMode != 1)
					{
						for (int i = -(int)NPC.ai[0]; i <= (int)NPC.ai[0]; i++)
						{
							Projectile.NewProjectile(NPC.GetSource_FromAI(),NPC.Center + i * new Vector2(0, 16).RotatedBy(NPC.rotation), new Vector2(1, 0).RotatedBy(NPC.rotation), ProjectileType<ChordRay>(), 15, 0f, Main.myPlayer, ai0: NPC.ai[2], ai1: time);
						}
					}

					if (NPC.localAI[0] > time + 50)
					{
						NPC.localAI[0] = -30;
					}
					break;
			}

			if (NPC.localAI[0] == 0)
			{
				SoundEngine.PlaySound(SoundID.Item71, NPC.Center);

				Vector2 offset = new Vector2(0, 32).RotatedBy(NPC.rotation);
				NPC.NewNPC(NPC.GetSource_FromAI(), (int)(NPC.Center.X - offset.X), (int)(NPC.Center.Y - offset.Y + 24), NPCType<RiftOpeningHitbox>(), ai0: NPC.whoAmI);
				NPC.NewNPC(NPC.GetSource_FromAI(), (int)(NPC.Center.X + offset.X), (int)(NPC.Center.Y + offset.Y + 24), NPCType<RiftOpeningHitbox>(), ai0: NPC.whoAmI);
			}

			//timer
			NPC.localAI[0]++;
		}

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
			return Math.Abs(NPC.localAI[0]) >= 30;
        }

        public override bool? CanHitNPC(NPC target)
        {
			return Math.Abs(NPC.localAI[0]) >= 30;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			if (Main.expertMode)
			{
				//target.AddBuff(BuffType<FractalSubworldDebuff>(), 600);
				//target.GetModPlayer<PolaritiesPlayer>().suddenFractalizationChange = true;
			}
		}

        public override bool CheckDead()
        {
			NPC.life = NPC.lifeMax;
			return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D texture = TextureAssets.Npc[NPC.type].Value;
			Rectangle frame = texture.Frame();

			float xScale = Math.Min(1, Math.Max(1 / 8f, (Math.Abs(NPC.localAI[0]) - 22) / 8f));
			float yScale = Math.Min(1, Math.Max(0, Math.Abs(NPC.localAI[0]) / 8f));

			Main.spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, frame, Color.White, NPC.rotation, frame.Size() / 2, new Vector2(xScale, yScale), SpriteEffects.None, 0f);

			switch(NPC.ai[1])
            {
				case 0:
					//hex telegraph
					if (NPC.localAI[0] >= 30)
					{
						for (int i = -2; i <= 2; i++)
						{
							float telegraphDistance = 600 * (float)Math.Sqrt(2 + 2 * Math.Cos(i * MathHelper.Pi / 3));
							float alpha = Math.Max(0, (60 - NPC.localAI[0]) / 30f) / 2f;

							Main.spriteBatch.Draw(ModContent.Request<Texture2D>("Polarities/Projectiles/CallShootProjectile", AssetRequestMode.ImmediateLoad).Value, NPC.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.White * 0.5f * alpha, NPC.rotation + i * MathHelper.Pi / 6, new Vector2(0, 0.5f), new Vector2(telegraphDistance, 2), SpriteEffects.None, 0f);
						}
					}
					break;
			}

			return false;
		}



		public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
		{
			List<int> hitboxIds = new List<int>();
			for (int i = 0; i < Main.npc.Length; i++)
			{
				if (Main.npc[i].type == NPC.type && Main.npc[i].realLife == NPC.realLife)
				{
					hitboxIds.Add(i);
				}
			}
			if (projectile.usesLocalNPCImmunity || projectile.localNPCImmunity[NPC.whoAmI] != 0)
			{
				foreach (int who in hitboxIds)
				{
					projectile.localNPCImmunity[who] = projectile.localNPCImmunity[NPC.whoAmI];
					Main.npc[who].immune[projectile.owner] = NPC.immune[projectile.owner];
				}
			}
			else if (projectile.usesIDStaticNPCImmunity)
			{
				foreach (int who in hitboxIds)
				{

					Projectile.perIDStaticNPCImmunity[projectile.type][who] = Projectile.perIDStaticNPCImmunity[projectile.type][NPC.whoAmI];
				}
			}
			else
			{
				foreach (int who in hitboxIds)
				{
					Main.npc[who].immune[projectile.owner] = NPC.immune[projectile.owner];
				}
			}
		}

		public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit)
		{
			List<int> hitboxIds = new List<int>();
			for (int i = 0; i < Main.npc.Length; i++)
			{
				if (Main.npc[i].type == NPC.type && Main.npc[i].realLife == NPC.realLife)
				{
					hitboxIds.Add(i);
				}
			}
			foreach (int who in hitboxIds)
			{
				Main.npc[who].immune[player.whoAmI] = NPC.immune[player.whoAmI];
			}
		}
	}

	public class RiftOpeningHitbox : ModNPC
	{
        public override string Texture => "Polarities/NPCs/RiftDenizen/RiftOpening";

        public override void SetStaticDefaults()
		{
			Main.npcFrameCount[NPC.type] = 1;
		}

		public override void SetDefaults()
		{
			NPC.aiStyle = -1;
			NPC.width = 48;
			NPC.height = 48;

			NPC.lifeMax = Main.expertMode ? 10000 : 8000;
			NPC.defense = 20;
			NPC.damage = 30;

			NPC.HitSound = SoundID.NPCHit23;

			NPC.knockBackResist = 0f;
			NPC.npcSlots = 0f;
			NPC.dontCountMe = true;
			NPC.lavaImmune = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;

			NPC.hide = true;

			NPC.buffImmune[BuffID.Confused] = true;

			//npc.GetGlobalNPC<PolaritiesNPC>().usesProjectileHitCooldowns = true;
			//npc.GetGlobalNPC<PolaritiesNPC>().projectileHitCooldownTime = 10;
		}

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
		{
			return false;
		}

        public override void AI()
		{
			NPC.realLife = NPC.FindFirstNPC(NPCType<RiftDenizen>());

			NPC owner = Main.npc[(int)NPC.ai[0]];

			NPC.active = owner.active;
			NPC.dontTakeDamage = owner.dontTakeDamage;
		}

		public override bool CheckDead()
		{
			NPC.life = NPC.lifeMax;
			return false;
		}

		public override bool? CanHitNPC(NPC target)
		{
			NPC owner = Main.npc[(int)NPC.ai[0]];

			return Math.Abs(owner.localAI[0]) >= 30;
		}

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			NPC owner = Main.npc[(int)NPC.ai[0]];

			return Math.Abs(owner.localAI[0]) >= 30;
		}

        public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			if (Main.expertMode)
			{
				//target.AddBuff(BuffType<FractalSubworldDebuff>(), 600);
				//target.GetModPlayer<PolaritiesPlayer>().suddenFractalizationChange = true;
			}
		}

		public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
		{
			List<int> hitboxIds = new List<int>();
			for (int i = 0; i < Main.npc.Length; i++)
			{
				if (Main.npc[i].type == NPC.type && Main.npc[i].realLife == NPC.realLife)
				{
					hitboxIds.Add(i);
				}
			}
			if (projectile.usesLocalNPCImmunity || projectile.localNPCImmunity[NPC.whoAmI] != 0)
			{
				foreach (int who in hitboxIds)
				{
					projectile.localNPCImmunity[who] = projectile.localNPCImmunity[NPC.whoAmI];
					Main.npc[who].immune[projectile.owner] = NPC.immune[projectile.owner];
				}
			}
			else if (projectile.usesIDStaticNPCImmunity)
			{
				foreach (int who in hitboxIds)
				{

					Projectile.perIDStaticNPCImmunity[projectile.type][who] = Projectile.perIDStaticNPCImmunity[projectile.type][NPC.whoAmI];
				}
			}
			else
			{
				foreach (int who in hitboxIds)
				{
					Main.npc[who].immune[projectile.owner] = NPC.immune[projectile.owner];
				}
			}
		}

		public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit)
		{
			List<int> hitboxIds = new List<int>();
			for (int i = 0; i < Main.npc.Length; i++)
			{
				if (Main.npc[i].type == NPC.type && Main.npc[i].realLife == NPC.realLife)
				{
					hitboxIds.Add(i);
				}
			}
			foreach (int who in hitboxIds)
			{
				Main.npc[who].immune[player.whoAmI] = NPC.immune[player.whoAmI];
			}
		}
	}

	public class RiftDenizenSky : CustomSky
	{
		private bool isActive;

		public static float xScale = 0f;
		public static float yScale = 0f;
		public static float skyAlpha = 0f;
		public static float concavityFactor = 0.125f;

		public static Vector2[] guidePoints = new Vector2[8];

		private static readonly int maxRifts = 512;
		private float[,] rifts = new float[maxRifts, 7];

		//rifts[i,0] = x position
		//rifts[i,1] = y position
		//rifts[i,2] = parallax multiplier
		//rifts[i,3] = secondary scale multiplier
		//rifts[i,4] = cloud type
		//rifts[i,5] = cloud rotation
		//rifts[i,6] = spriteEffects

		public override void OnLoad()
		{
			xScale = 0f;
			yScale = 0f;
			skyAlpha = 0f;
			concavityFactor = 0.125f;

			guidePoints = new Vector2[8];

			//update rifts to prevent weird visual bug
			UpdateRifts();

			/*Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 500, 1, false, SurfaceFormat.Color);
			System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
			for (int j = 0; j < texture.Height; j++)
			{
				for (int i = 0; i < texture.Width; i++)
				{
					float x = (2 * i / (float)(texture.Width - 1) - 1);

					float alphaFloat = (x - 1) * (x - 1) * (x + 1) * (x + 1);

					int r = 255;
					int g = 255;
					int b = 255;
					int alpha = (int)(255 * alphaFloat);

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "RiftDenizenSky.png", FileMode.Create), texture.Width, texture.Height);*/
		}

		public override void Update(GameTime gameTime)
		{
			//update rifts
			UpdateRifts();
		}

		private void UpdateRifts()
		{
			for (int i = 0; i < maxRifts; i++)
			{
				Vector2 drawPos = new Vector2(rifts[i, 0], rifts[i, 1]) + Main.screenPosition * rifts[i, 2] - Main.screenPosition;
				if ((drawPos - new Vector2(Main.screenWidth, Main.screenHeight) / 2).Length() > (new Vector2(Main.screenWidth, Main.screenHeight) * 2).Length())
				{
					rifts[i, 2] = Main.rand.NextFloat(0.9f, 0.1f);
					rifts[i, 3] = Main.rand.NextFloat(0.8f, 1.25f);
					float parallaxScale = rifts[i, 2];
					Vector2 offset = new Vector2(Main.screenWidth, Main.screenHeight) * 10;

					//make it so offset can be onscreen if the player teleports/has just arrived in the dimension
					if ((drawPos - new Vector2(Main.screenWidth, Main.screenHeight) / 2).Length() > (new Vector2(Main.screenWidth, Main.screenHeight) * 3).Length())
					{
						while (offset.Length() > (new Vector2(Main.screenWidth, Main.screenHeight) * 2).Length())
						{
							offset = new Vector2(Main.rand.NextFloat(-Main.screenWidth, Main.screenWidth), Main.rand.NextFloat(-Main.screenHeight, Main.screenHeight)) * 2;
						}
					}
					else
					{
						offset = new Vector2(Main.screenWidth, Main.screenHeight).RotatedByRandom(MathHelper.TwoPi);
					}

					rifts[i, 0] = Main.screenPosition.X * (1 - parallaxScale) + Main.screenWidth / 2 + offset.X;
					rifts[i, 1] = Main.screenPosition.Y * (1 - parallaxScale) + Main.screenHeight / 2 + offset.Y;
					rifts[i, 4] = 1;//Main.rand.Next(1, 5);
					rifts[i, 6] = Main.rand.Next(2);

					//rifts don't rotate
					rifts[i, 5] = 0;
				}
			}
		}

		public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
		{
			//draw the sky and the boss
			if (maxDepth >= 0 && minDepth < 0)
			{
				Main.spriteBatch.Draw(ModContent.Request<Texture2D>("Polarities/Projectiles/CallShootProjectile", AssetRequestMode.ImmediateLoad).Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Color(36, 80, 123) * skyAlpha);

				Texture2D skyBeam = ModContent.Request<Texture2D>("Polarities/NPCs/RiftDenizen/RiftDenizenSky", AssetRequestMode.ImmediateLoad).Value;
				Main.spriteBatch.Draw(skyBeam, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2), skyBeam.Frame(), new Color(82, 179, 203) * skyAlpha, 0f, skyBeam.Frame().Size() / 2, new Vector2(Main.screenWidth / (float)skyBeam.Width, Main.screenHeight), SpriteEffects.None, 0f);

				DrawBoss(spriteBatch, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 - 300));
			}

			//draw the rifts
			DrawRifts(spriteBatch);

			//rendertarget stuff
			/*if (xScale == 1 && yScale == 1 && !File.Exists(Main.SavePath + Path.DirectorySeparatorChar + "RiftDenizen.png"))
			{
				Main.spriteBatch.End();
				Main.spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, (Effect)null, Main.Transform);

				var capture = new RenderTarget2D(spriteBatch.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, Main.spriteBatch.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

				Main.spriteBatch.GraphicsDevice.SetRenderTarget(capture);
				Main.spriteBatch.GraphicsDevice.Clear(Color.Transparent);

				//spriteBatch.Draw(ModContent.Request<Texture2D>("Polarities/Projectiles/CallShootProjectile"), new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Color(50, 125, 164) * skyAlpha);
				DrawBoss(spriteBatch, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2));
				//DrawRifts(spriteBatch);

				Main.spriteBatch.End();
				Main.spriteBatch.GraphicsDevice.SetRenderTarget(null);

				var stream = File.Create(Main.SavePath + Path.DirectorySeparatorChar + "RiftDenizen.png");
				capture.SaveAsPng(stream, capture.Width, capture.Height);
				stream.Dispose();
				capture.Dispose();

				Main.spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, (Effect)null, Main.Transform);
			}*/
		}

		private void DrawBoss(SpriteBatch spriteBatch, Vector2 bossPosition)
        {
			if (Math.Min(xScale, yScale) > 0)
			{
				Texture2D rayTexture = ModContent.Request<Texture2D>("Polarities/Projectiles/CallShootProjectile", AssetRequestMode.ImmediateLoad).Value;
				Rectangle frame = new Rectangle(0, 0, 1, 1);
				Vector2 origin = new Vector2(0, 0.5f);

				List<Vector2> vertices = new List<Vector2>();

				//compute border
				for (int i = 0; i < guidePoints.Length; i++)
				{
					Vector2 startPoint = guidePoints[i];
					Vector2 endPoint = guidePoints[(i + 1) % guidePoints.Length];
					Vector2 midPoint = (startPoint + endPoint) * concavityFactor;

					for (int j = 0; j < 100; j++)
					{
						Vector2 point = Vector2.Lerp(Vector2.Lerp(startPoint, midPoint, j / 100f), Vector2.Lerp(midPoint, endPoint, j / 100f), j / 100f);
						point.X *= xScale;
						point.Y *= yScale;

						vertices.Add(point);
					}
				}

				for (int i = 0; i < vertices.ToArray().Length; i++)
				{
					Vector2 point = vertices[i];
					Main.spriteBatch.Draw(rayTexture, bossPosition, frame, new Color(79, 166, 203), point.ToRotation(), origin, new Vector2(point.Length(), 3 * Math.Max(xScale, yScale)), SpriteEffects.None, 0f);
				}
				for (int i = 0; i < vertices.ToArray().Length; i++)
				{
					Vector2 point = vertices[i];
					Main.spriteBatch.Draw(rayTexture, bossPosition + new Vector2(0, -2.5f * yScale), frame, new Color(116, 216, 234), point.ToRotation(), origin, new Vector2(point.Length() * 0.83f, 3 * Math.Max(xScale, yScale)), SpriteEffects.None, 0f);
				}
				for (int i = 0; i < vertices.ToArray().Length; i++)
				{
					Vector2 point = vertices[i];
					Main.spriteBatch.Draw(rayTexture, bossPosition + new Vector2(0, -6f * yScale), frame, Color.White, point.ToRotation(), origin, new Vector2(point.Length() * 0.6f, 3 * Math.Max(xScale, yScale)), SpriteEffects.None, 0f);
				}

				//draw border
				for (int i = 0; i < vertices.ToArray().Length; i += 4)
				{
					Vector2 point = vertices[i];
					Vector2 nextPoint = vertices[(i + 4) % vertices.ToArray().Length];
					Main.spriteBatch.Draw(rayTexture, bossPosition + point, frame, new Color(2, 4, 48), (nextPoint - point).ToRotation(), origin, new Vector2((nextPoint - point).Length() + 1, 3 * Math.Max(xScale, yScale)), SpriteEffects.None, 0f);
				}

				Texture2D chainTexture = ModContent.Request<Texture2D>("Polarities/NPCs/RiftDenizen/RiftClawChain", AssetRequestMode.ImmediateLoad).Value;
				for (int i = 0; i < guidePoints.Length; i++)
				{
					Vector2 point = guidePoints[(i + 7) % guidePoints.Length];
					point.X *= xScale;
					point.Y *= yScale;

					Vector2[] bezierPoints = { bossPosition, bossPosition + new Vector2(0, -80 * yScale), bossPosition + point * concavityFactor * 2, bossPosition + point };
					float bezierProgress = 0;
					float bezierIncrement = 6;

					float rotation;

					while (bezierProgress < 1)
					{
						//draw stuff
						Vector2 oldPos = ModUtils.BezierCurve(bezierPoints, bezierProgress);

						//increment progress
						while ((oldPos - ModUtils.BezierCurve(bezierPoints, bezierProgress)).Length() < bezierIncrement)
						{
							bezierProgress += 0.2f / ModUtils.BezierCurveDerivative(bezierPoints, bezierProgress).Length();
						}

						Vector2 newPos = ModUtils.BezierCurve(bezierPoints, bezierProgress);
						rotation = (newPos - oldPos).ToRotation() + MathHelper.Pi;

						Vector2 drawPos = (oldPos + newPos) / 2;

						Main.spriteBatch.Draw(chainTexture, drawPos, chainTexture.Frame(), Color.White, rotation, chainTexture.Frame().Size() / 2, new Vector2(4f, 0.75f * Math.Max(xScale, yScale)), SpriteEffects.None, 0f);
					}
				}

				Texture2D clawTexture = ModContent.Request<Texture2D>("Polarities/NPCs/RiftDenizen/RiftDenizenClaw", AssetRequestMode.ImmediateLoad).Value;
				for (int i = 0; i < guidePoints.Length; i++)
				{
					Vector2 point = guidePoints[i];
					point.X *= xScale;
					point.Y *= yScale;
					Main.spriteBatch.Draw(clawTexture, bossPosition + point, clawTexture.Frame(), Color.White, point.ToRotation(), new Vector2(5, 7), Math.Max(xScale, yScale), SpriteEffects.None, 0f);
				}

				Texture2D eyeTexture = ModContent.Request<Texture2D>("Polarities/NPCs/RiftDenizen/RiftDenizenEye", AssetRequestMode.ImmediateLoad).Value;
				Main.spriteBatch.Draw(eyeTexture, bossPosition, eyeTexture.Frame(), Color.White, 0f, eyeTexture.Frame().Size() / 2, new Vector2(xScale, yScale), SpriteEffects.None, 0f);
			}
		}

		private void DrawRifts(SpriteBatch spriteBatch)
        {
			int[] riftOrder = new int[maxRifts];
			for (int i = 0; i < maxRifts; i++)
			{
				riftOrder[i] = i;
			}
			//CloudComparer riftComparer = new CloudComparer(rifts);
			Array.Sort(riftOrder, (i1, i2) => rifts[i2, 2].CompareTo(rifts[i1, 2]));

			for (int j = 0; j < maxRifts; j++)
			{
				int i = riftOrder[j];

				float zoom = Main.GameZoomTarget;

				Color drawColor = Color.White * skyAlpha;

				Texture2D riftTexture = ModContent.Request<Texture2D>("Polarities/Textures/Clouds/rift_1", AssetRequestMode.ImmediateLoad).Value;
				Vector2 drawPos = new Vector2(rifts[i, 0], rifts[i, 1]) + Main.screenPosition * rifts[i, 2] - Main.screenPosition;

				drawPos -= new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
				drawPos *= zoom;
				drawPos += new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);

				float textureRadius = riftTexture.Size().Length() / 2;
				if (drawPos.X > -textureRadius && drawPos.X < Main.screenWidth + textureRadius && drawPos.Y > -textureRadius && drawPos.Y < Main.screenHeight + textureRadius)
					Main.spriteBatch.Draw(riftTexture, drawPos, riftTexture.Frame(), drawColor, rifts[i, 5], riftTexture.Size() / 2, new Vector2(xScale, yScale) * 1.33f * (1 - rifts[i, 2]) * rifts[i, 3] * zoom, (SpriteEffects)rifts[i, 6], 0f);
			}
		}

		public override float GetCloudAlpha()
		{
			return 1f;
		}

		public override void Activate(Vector2 position, params object[] args)
		{
			isActive = true;
		}

		public override void Deactivate(params object[] args)
		{
			isActive = false;
		}

		public override void Reset()
		{
			isActive = false;
		}

		public override bool IsActive()
		{
			return isActive;
		}
	}

	public class ParallaxRay : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			/*Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 384, 384, false, SurfaceFormat.Color);
			List<Color> list = new List<Color>();
			for (int j = 0; j < texture.Height; j++)
			{
				for (int i = 0; i < texture.Width; i++)
				{
					float x = (2 * i / (float)(texture.Width - 1) - 1);
					float y = (2 * j / (float)(texture.Height - 1) - 1);

					float distanceSquared = x * x + y * y;
					float index = 1 - distanceSquared + (float)(12 * Math.Pow(distanceSquared, 8) * (1 - Math.Pow(distanceSquared, 2)));

					int r = 255 - (int)(205 * (1 - index));
					int g = 255 - (int)(130 * (1 - index));
					int b = 255 -(int)(101 * (1 - index));
					int alpha = distanceSquared >= 1 ? 0 : (int)(255 * index);

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "ParallaxRay.png", FileMode.Create), texture.Width, texture.Height);

			texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 384, 384, false, SurfaceFormat.Color);
			list = new List<Color>();
			for (int j = 0; j < texture.Height; j++)
			{
				for (int i = 0; i < texture.Width; i++)
				{
					float x = (2 * i / (float)(texture.Width - 1) - 1);
					float y = (2 * j / (float)(texture.Height - 1) - 1);

					float distanceSquared = x * x + y * y;
					float index = (float)(12 * Math.Pow(distanceSquared, 8) * (1 - Math.Pow(distanceSquared, 2)));

					int r = 255 - (int)(205 * (1 - index));
					int g = 255 - (int)(130 * (1 - index));
					int b = 255 - (int)(101 * (1 - index));
					int alpha = distanceSquared >= 1 ? 0 : (int)(255 * index);

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "ParallaxRayTelegraph.png", FileMode.Create), texture.Width, texture.Height);*/
		}

		public override void SetDefaults()
		{
			Projectile.width = 384;
			Projectile.height = 384;
			Projectile.alpha = 0;
			Projectile.light = 4f;
			Projectile.aiStyle = -1;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 600;
			Projectile.scale = 1f;
		}

		public override void AI()
		{
			Player player = Main.player[(int)Projectile.ai[0]];

			if (Projectile.localAI[0] < 60)
			{
				Projectile.hostile = false;
			}
			else
			{
				Projectile.hostile = Projectile.timeLeft >= 30;

				Projectile.velocity += (player.Center - Projectile.Center) / 1200f;
				Projectile.velocity *= 0.99f;
			}

			if (Projectile.localAI[0] == 45)
			{
				Main.LocalPlayer.GetModPlayer<PolaritiesPlayer>().AddScreenShake(60, 60);
				SoundEngine.PlaySound(SoundID.Zombie104, Projectile.Center);
			}

			if (!NPC.AnyNPCs(NPCType<RiftDenizen>()) && Projectile.timeLeft > 60)
            {
				Projectile.timeLeft = 60;
            }

			Projectile.localAI[0]++;
		}

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
			if (Main.expertMode)
			{
				//target.AddBuff(BuffType<FractalSubworldDebuff>(), 2400);
				//target.GetModPlayer<PolaritiesPlayer>().suddenFractalizationChange = true;
			}
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 circleCenter = Projectile.Center;
			float nearestX = Math.Max(targetHitbox.X, Math.Min(circleCenter.X, targetHitbox.X + targetHitbox.Size().X));
			float nearestY = Math.Max(targetHitbox.Y, Math.Min(circleCenter.Y, targetHitbox.Y + targetHitbox.Size().Y));
			return new Vector2(circleCenter.X - nearestX, circleCenter.Y - nearestY).Length() < Projectile.width / 2;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = ModContent.Request<Texture2D>("Polarities/NPCs/RiftDenizen/ParallaxRayTelegraph", AssetRequestMode.ImmediateLoad).Value;
			Rectangle frame = texture.Frame();

			for (int i = 0; i < 120; i++)
			{
				float distance = (float)Math.Exp((i + (Projectile.localAI[0] / 3f) % 1) / 30f);

				Vector2 eyePosition = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 - 300 / Main.GameZoomTarget);
				Vector2 projectilePosition = Projectile.Center - Main.screenPosition;

				Vector2 drawPosition = (distance * projectilePosition + (1 - distance) * eyePosition);
				float rotation = (projectilePosition - eyePosition).ToRotation();
				float scale = distance;
				float distanceFactor = Math.Min(1, (float)Math.Exp((Projectile.localAI[0] - 60) / 5f));

				float timeLeftThing = Math.Max(0, (60 - Projectile.timeLeft) / 60f);
				float alpha = (float)(1 + 3 * (1 - Math.Pow(timeLeftThing, 8)) * Math.Exp(-20 * Math.Pow(distance / distanceFactor - 1, 2)) - Math.Exp(-1 / Math.Pow(distance / distanceFactor, 2))) / 10f;
				float alpha2 = (float)((1 - Math.Pow(timeLeftThing, 8)) * Math.Exp(- Math.Pow(2 * timeLeftThing * distanceFactor / distance, 2)));
				alpha *= alpha2;

				Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, drawPosition, frame, Color.White * ((1 - Projectile.alpha / 255f) * alpha), rotation, frame.Size() / 2, scale * Projectile.scale, SpriteEffects.None, 0f);
			}
			if (Projectile.localAI[0] < 45)
			{
				Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, frame, Color.White * ((1 - Projectile.alpha / 255f) * 0.25f * (45 - Projectile.localAI[0]) / 45f), Projectile.rotation, frame.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
			}

			return false;
		}
	}

	public class ParallaxRayBehindTiles : ModProjectile
	{
        public override string Texture => "Polarities/NPCs/RiftDenizen/ParallaxRay";

        public override void SetStaticDefaults()
		{
		}

		public override void SetDefaults()
		{
			Projectile.width = 384;
			Projectile.height = 384;
			Projectile.alpha = 0;
			Projectile.light = 4f;
			Projectile.aiStyle = -1;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 600;
			Projectile.scale = 1f;

			Projectile.hide = true;
		}

		public override void AI()
		{
			Player player = Main.player[(int)Projectile.ai[0]];

			if (Projectile.localAI[0] < 60)
			{
				Projectile.hostile = false;
			}
			else
			{
				Projectile.hostile = Projectile.timeLeft >= 30;

				Projectile.velocity += (player.Center - Projectile.Center) / 1200f;
				Projectile.velocity *= 0.99f;
			}

			if (Projectile.localAI[0] == 45)
			{
				Main.LocalPlayer.GetModPlayer<PolaritiesPlayer>().AddScreenShake(60, 60);
				SoundEngine.PlaySound(SoundID.Zombie104, Projectile.Center);
			}

			if (!NPC.AnyNPCs(NPCType<RiftDenizen>()) && Projectile.timeLeft > 60)
			{
				Projectile.timeLeft = 60;
			}

			Projectile.localAI[0]++;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			if (Main.expertMode)
			{
				//target.AddBuff(BuffType<FractalSubworldDebuff>(), 2400);
				//target.GetModPlayer<PolaritiesPlayer>().suddenFractalizationChange = true;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 circleCenter = Projectile.Center;
			float nearestX = Math.Max(targetHitbox.X, Math.Min(circleCenter.X, targetHitbox.X + targetHitbox.Size().X));
			float nearestY = Math.Max(targetHitbox.Y, Math.Min(circleCenter.Y, targetHitbox.Y + targetHitbox.Size().Y));
			return new Vector2(circleCenter.X - nearestX, circleCenter.Y - nearestY).Length() < Projectile.width / 2;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
			PolaritiesSystem.DrawCacheProjsBehindWalls.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = ModContent.Request<Texture2D>("Polarities/NPCs/RiftDenizen/ParallaxRayTelegraph", AssetRequestMode.ImmediateLoad).Value;
			Rectangle frame = texture.Frame();

			for (int i = -360; i < 0; i++)
			{
				float distance = (float)Math.Exp((i + (Projectile.localAI[0] / 3f) % 1) / 30f);

				Vector2 eyePosition = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 - 300 / Main.GameZoomTarget);
				Vector2 projectilePosition = Projectile.Center - Main.screenPosition;

				Vector2 drawPosition = (distance * projectilePosition + (1 - distance) * eyePosition);
				float rotation = (projectilePosition - eyePosition).ToRotation();
				float scale = distance;
				float distanceFactor = Math.Min(1, (float)Math.Exp((Projectile.localAI[0] - 60) / 5f));

				float timeLeftThing = Math.Max(0, (60 - Projectile.timeLeft) / 60f);
				float alpha = (float)(1 + 3 * (1 - Math.Pow(timeLeftThing, 8)) * Math.Exp(-20 * Math.Pow(distance / distanceFactor - 1, 2)) - Math.Exp(-1 / Math.Pow(distance / distanceFactor, 2))) / 10f;
				float alpha2 = (float)((1 - Math.Pow(timeLeftThing, 8)) * Math.Exp(-Math.Pow(2 * timeLeftThing * distanceFactor / distance, 2)));
				alpha *= alpha2;

				Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, drawPosition, frame, Color.White * ((1 - Projectile.alpha / 255f) * alpha), rotation, frame.Size() / 2, scale * Projectile.scale, SpriteEffects.None, 0f);
			}

			return false;
		}
	}

	public class ParallaxBolt : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			/*Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 32, 32, false, SurfaceFormat.Color);
			List<Color> list = new List<Color>();
			for (int j = 0; j < texture.Height; j++)
			{
				for (int i = 0; i < texture.Width; i++)
				{
					float x = (2 * i / (float)(texture.Width - 1) - 1);
					float y = (2 * j / (float)(texture.Height - 1) - 1);

					float distanceSquared = x * x + y * y;
					float index = 1 - distanceSquared + (float)(12 * Math.Pow(distanceSquared, 8) * (1 - Math.Pow(distanceSquared, 2)));

					int r = 255 - (int)(205 * (1 - index));
					int g = 255 - (int)(130 * (1 - index));
					int b = 255 -(int)(101 * (1 - index));
					int alpha = distanceSquared >= 1 ? 0 : (int)(255 * index);

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "ParallaxBolt.png", FileMode.Create), texture.Width, texture.Height);

			texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 32, 32, false, SurfaceFormat.Color);
			list = new List<Color>();
			for (int j = 0; j < texture.Height; j++)
			{
				for (int i = 0; i < texture.Width; i++)
				{
					float x = (2 * i / (float)(texture.Width - 1) - 1);
					float y = (2 * j / (float)(texture.Height - 1) - 1);

					float distanceSquared = x * x + y * y;
					float index = (float)(12 * Math.Pow(distanceSquared, 8) * (1 - Math.Pow(distanceSquared, 2)));

					int r = 255 - (int)(205 * (1 - index));
					int g = 255 - (int)(130 * (1 - index));
					int b = 255 - (int)(101 * (1 - index));
					int alpha = distanceSquared >= 1 ? 0 : (int)(255 * index);

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "ParallaxBoltTelegraph.png", FileMode.Create), texture.Width, texture.Height);*/
		}

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.alpha = 0;
			Projectile.light = 4f;
			Projectile.aiStyle = -1;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 79;
			Projectile.scale = 1f;
		}

		public override void AI()
		{
			Player player = Main.player[(int)Projectile.ai[0]];

			Projectile.localAI[0]++;
			if (Projectile.localAI[0] < 58)
			{
				Projectile.hostile = false;
			}
			else
			{
				Projectile.hostile = Projectile.timeLeft >= 15;
			}
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			if (Main.expertMode)
			{
				//target.AddBuff(BuffType<FractalSubworldDebuff>(), 600);
				//target.GetModPlayer<PolaritiesPlayer>().suddenFractalizationChange = true;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 circleCenter = Projectile.Center;
			float nearestX = Math.Max(targetHitbox.X, Math.Min(circleCenter.X, targetHitbox.X + targetHitbox.Size().X));
			float nearestY = Math.Max(targetHitbox.Y, Math.Min(circleCenter.Y, targetHitbox.Y + targetHitbox.Size().Y));
			return new Vector2(circleCenter.X - nearestX, circleCenter.Y - nearestY).Length() < Projectile.width / 2;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Rectangle frame = texture.Frame();

			float dist = (Projectile.localAI[0] - 60) / 60f * 240f;

			for (float i = dist - 5; i <= dist + 5; i+= 0.25f)
			{
				float distance = (float)Math.Exp(i / 30f);

				if (distance >= 1)
				{
					Vector2 eyePosition = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 - 300 / Main.GameZoomTarget);
					Vector2 projectilePosition = Projectile.Center - Main.screenPosition;
					float scale = distance;
					Vector2 drawPosition = (distance * projectilePosition + (1 - distance) * eyePosition);

					float alpha = (1 - (dist - i) * (dist - i) / 25f) / 3f;
					scale *= alpha;
					float rotation = (projectilePosition - eyePosition).ToRotation();

					if (Projectile.timeLeft < 15)
					{
						alpha *= (Projectile.timeLeft / 15f) * (Projectile.timeLeft / 15f);
					}

					Main.spriteBatch.Draw(texture, drawPosition, frame, Color.White * ((1 - Projectile.alpha / 255f) * alpha), rotation, frame.Size() / 2, scale * Projectile.scale, SpriteEffects.None, 0f);
				}
			}

			if (Projectile.localAI[0] < 60)
			{
				texture = ModContent.Request<Texture2D>("Polarities/NPCs/RiftDenizen/ParallaxBoltTelegraph", AssetRequestMode.ImmediateLoad).Value;
				frame = texture.Frame();

				Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, frame, Color.White * ((1 - Projectile.alpha / 255f) * Projectile.localAI[0] / 90f), Projectile.rotation, frame.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
				Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, frame, Color.White * ((1 - Projectile.alpha / 255f) * Projectile.localAI[0] / 90f), Projectile.rotation, frame.Size() / 2, Projectile.scale * Projectile.localAI[0] / 60f, SpriteEffects.None, 0f);
			}

			if (Projectile.localAI[0] <= 65 && Projectile.localAI[0] >= 55)
			{
				texture = ModContent.Request<Texture2D>("Terraria/Images/Projectile_644", AssetRequestMode.ImmediateLoad).Value;
				frame = texture.Frame();

				float scale = Math.Max(0, 1 - (60 - Projectile.localAI[0]) * (60 - Projectile.localAI[0]) / 25f);

				Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, frame, Color.White * 0.5f, Projectile.rotation + MathHelper.PiOver4, frame.Size() / 2, scale * Projectile.scale, SpriteEffects.None, 0f);
				Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, frame, Color.White * 0.5f, Projectile.rotation + 3 * MathHelper.PiOver4, frame.Size() / 2, scale * Projectile.scale, SpriteEffects.None, 0f);
			}

			return false;
		}
	}

	public class ParallaxBoltBehindTiles : ModProjectile
	{
		public override string Texture => "Polarities/NPCs/RiftDenizen/ParallaxBolt";

		public override void SetStaticDefaults()
		{
		}

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.alpha = 0;
			Projectile.light = 4f;
			Projectile.aiStyle = -1;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 79;
			Projectile.scale = 1f;
			Projectile.hide = true;
		}

		public override void AI()
		{
			Player player = Main.player[(int)Projectile.ai[0]];

			Projectile.localAI[0]++;
			if (Projectile.localAI[0] < 58)
			{
				Projectile.hostile = false;
			}
			else
			{
				Projectile.hostile = Projectile.timeLeft >= 15;
			}
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			if (Main.expertMode)
			{
				//target.AddBuff(BuffType<FractalSubworldDebuff>(), 600);
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 circleCenter = Projectile.Center;
			float nearestX = Math.Max(targetHitbox.X, Math.Min(circleCenter.X, targetHitbox.X + targetHitbox.Size().X));
			float nearestY = Math.Max(targetHitbox.Y, Math.Min(circleCenter.Y, targetHitbox.Y + targetHitbox.Size().Y));
			return new Vector2(circleCenter.X - nearestX, circleCenter.Y - nearestY).Length() < Projectile.width / 2;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
			PolaritiesSystem.DrawCacheProjsBehindWalls.Add(index);
		}

        public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Rectangle frame = texture.Frame();

			float dist = (Projectile.localAI[0] - 60) / 60f * 240f;

			for (float i = dist - 5; i <= dist + 5; i += 0.25f)
			{
				float distance = (float)Math.Exp(i / 30f);

				if (distance < 1)
				{
					Vector2 eyePosition = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 - 300 / Main.GameZoomTarget);
					Vector2 projectilePosition = Projectile.Center - Main.screenPosition;
					float scale = distance;
					Vector2 drawPosition = (distance * projectilePosition + (1 - distance) * eyePosition);

					float alpha = (1 - (dist - i) * (dist - i) / 25f) / 3f;
					scale *= alpha;
					float rotation = (projectilePosition - eyePosition).ToRotation();

					if (Projectile.timeLeft < 15)
					{
						alpha *= (Projectile.timeLeft / 15f) * (Projectile.timeLeft / 15f);
					}

					Main.spriteBatch.Draw(texture, drawPosition, frame, Color.White * ((1 - Projectile.alpha / 255f) * alpha), rotation, frame.Size() / 2, scale * Projectile.scale, SpriteEffects.None, 0f);
				}
			}

			return false;
		}
	}

	public class RiftBolt : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 21;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.alpha = 0;
			Projectile.light = 1f;
			Projectile.aiStyle = -1;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 600;
			Projectile.scale = 1f;
		}

		public override void AI()
		{
			Projectile.timeLeft = Math.Min(Projectile.timeLeft, (int)Projectile.ai[0]);

			if (Projectile.timeLeft < 20)
            {
				Projectile.velocity = Vector2.Zero;
				Projectile.hostile = false;
            }

			Projectile.rotation = Projectile.velocity.ToRotation();
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			if (Main.expertMode)
			{
				//target.AddBuff(BuffType<FractalSubworldDebuff>(), 600);
				//target.GetModPlayer<PolaritiesPlayer>().suddenFractalizationChange = true;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = ModContent.Request<Texture2D>("Polarities/Projectiles/CallShootProjectile", AssetRequestMode.ImmediateLoad).Value;
			var frame = new Rectangle(0, 0, 1, 1);

			for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
			{
				if (Projectile.oldPos[i] != Projectile.oldPos[i + 1])
				{
					Main.spriteBatch.Draw(texture, Projectile.oldPos[i] + Projectile.Center - Projectile.position - Main.screenPosition, frame, Color.White * (1 - i / (float)(Projectile.oldPos.Length - 1)), Projectile.oldRot[i], frame.Size() / 2, new Vector2(Projectile.velocity.Length() / frame.Width, 4f * (1 - i / (float)(Projectile.oldPos.Length - 1))), SpriteEffects.None, 0f);
				}
			}

			texture = TextureAssets.Projectile[Projectile.type].Value;
			frame = texture.Frame();

			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, frame.Size() / 2, new Vector2(Projectile.velocity.Length() / frame.Width, 1f), SpriteEffects.None, 0f);

			return false;
		}
	}

	public class RiftClaw : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.alpha = 0;
			Projectile.light = 1f;
			Projectile.aiStyle = -1;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 180;
			Projectile.scale = 1f;

		}

		float distanceFactor = 600f;

		public override void AI()
		{
			NPC owner = Main.npc[(int)Projectile.ai[0]];

			if (Projectile.localAI[0] <= 30)
			{
				Projectile.scale = Projectile.localAI[0] / 30f;
			}
			else if (Projectile.localAI[0] <= 60)
			{
				distanceFactor = Math.Max(distanceFactor, (Main.player[owner.target].Center - owner.Center).Length() / Projectile.velocity.Length());

				Projectile.Center = owner.Center + Projectile.velocity * (float)(Math.Exp((Projectile.localAI[0] - 30) / 30) - 1) * distanceFactor / 1.7182f;
			}
			else if (Projectile.localAI[0] <= 120)
			{
				distanceFactor = Math.Max(distanceFactor, (Main.player[owner.target].Center - owner.Center).Length() / Projectile.velocity.Length());

				Projectile.Center = owner.Center + Projectile.velocity.RotatedBy((Projectile.localAI[0] - 60) * Projectile.ai[1] / 60f) * distanceFactor;
			}
			else if (Projectile.localAI[0] <= 150)
			{
				Projectile.Center = owner.Center + Projectile.velocity.RotatedBy(Projectile.ai[1]) * (float)(Math.Exp((150 - Projectile.localAI[0]) / 30) - 1) * distanceFactor / 1.7182f;
			}
			else
			{
				Projectile.scale = (180 - Projectile.localAI[0]) / 30f;
			}
			if (Projectile.Center == owner.Center)
			{
				Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.Atan2(48, 22) - MathHelper.PiOver2;
			}
			else
			{
				Projectile.rotation = (Projectile.Center - owner.Center).ToRotation() + (float)Math.Atan2(48, 22) - MathHelper.PiOver2;
			}

			Projectile.localAI[0]++;
		}

        public override bool ShouldUpdatePosition()
        {
			return false;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			if (Main.expertMode)
			{
				//target.AddBuff(BuffType<FractalSubworldDebuff>(), 600);
				//target.GetModPlayer<PolaritiesPlayer>().suddenFractalizationChange = true;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
			float point = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + new Vector2(96, 44).RotatedBy(Projectile.rotation) * Projectile.scale, 16 * Projectile.scale, ref point);
        }

        public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture;
			Rectangle frame;

			if (Projectile.localAI[0] <= 60)
			{
				texture = ModContent.Request<Texture2D>("Polarities/NPCs/RiftDenizen/ChordRayCap", AssetRequestMode.ImmediateLoad).Value;
				frame = texture.Frame();
				float alpha = (60 - Projectile.localAI[0]) / 60f;

				Main.spriteBatch.Draw(texture, Main.npc[(int)Projectile.ai[0]].Center - Main.screenPosition, frame, Color.White * 0.5f * alpha, Projectile.velocity.ToRotation(), new Vector2(32, 16), new Vector2(20 * Projectile.velocity.Length(), 0.75f), SpriteEffects.None, 0f);
			}
			else if (Projectile.localAI[0] <= 150 && Projectile.localAI[0] > 90)
			{
				texture = ModContent.Request<Texture2D>("Polarities/NPCs/RiftDenizen/ChordRayCap", AssetRequestMode.ImmediateLoad).Value;
				frame = texture.Frame();
				float alpha = (Projectile.localAI[0] - 90) / 60f;

				Main.spriteBatch.Draw(texture, Main.npc[(int)Projectile.ai[0]].Center - Main.screenPosition, frame, Color.White * 0.5f * alpha, Projectile.velocity.ToRotation() + Projectile.ai[1], new Vector2(32, 16), new Vector2(20 * Projectile.velocity.Length(), 0.75f), SpriteEffects.None, 0f);
			}

			Vector2 ownerCenter = Main.npc[(int)Projectile.ai[0]].Center;
			texture = ModContent.Request<Texture2D>("Polarities/NPCs/RiftDenizen/RiftClawChain", AssetRequestMode.ImmediateLoad).Value;
			frame = texture.Frame();
			float chainAlpha = (Projectile.localAI[0] <= 60 || Projectile.localAI[0] > 150) ? 1 : 0.25f;

			Main.spriteBatch.Draw(texture, ownerCenter - Main.screenPosition, frame, Color.White * chainAlpha, (Projectile.Center - ownerCenter).ToRotation(), new Vector2(0, 4), new Vector2((Projectile.Center - ownerCenter).Length() / 2, 1), SpriteEffects.None, 0f);


			texture = TextureAssets.Projectile[Projectile.type].Value;
			frame = texture.Frame();

			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				Main.spriteBatch.Draw(texture, Projectile.oldPos[i] + Projectile.Center - Projectile.position - Main.screenPosition, frame, Color.White * (1 - i / (float)Projectile.oldPos.Length), Projectile.oldRot[i], new Vector2(16, 16), Projectile.scale, SpriteEffects.None, 0f);
			}

			return false;
		}
	}

	public class ChordRay : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			/*
			Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 1, 32, false, SurfaceFormat.Color);
			List<Color> list = new List<Color>();
			for (int j = 0; j < texture.Height; j++)
			{
				for (int i = 0; i < texture.Width; i++)
				{
					float y = (2 * j / (float)(texture.Height - 1) - 1);

					float distanceSquared = y * y;
					float index = 1 - distanceSquared;

					int r = 255 - (int)(205 * (1 - index));
					int g = 255 - (int)(130 * (1 - index));
					int b = 255 -(int)(101 * (1 - index));
					int alpha = distanceSquared >= 1 ? 0 : (int)(255 * index);

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "ChordRay.png", FileMode.Create), texture.Width, texture.Height);

			texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 32, 32, false, SurfaceFormat.Color);
			list = new List<Color>();
			for (int j = 0; j < texture.Height; j++)
			{
				for (int i = 0; i < texture.Width; i++)
				{
					float x = (2 * i / (float)(texture.Width - 1) - 1);
					float y = (2 * j / (float)(texture.Height - 1) - 1);

					float distanceSquared = y * y;
					float index = (1 + x) / 2 * (1 - y * y);

					int r = 255 - (int)(205 * (1 - index));
					int g = 255 - (int)(130 * (1 - index));
					int b = 255 - (int)(101 * (1 - index));
					int alpha = (int)(255 * index);

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "ChordRayCap.png", FileMode.Create), texture.Width, texture.Height);
			*/
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.alpha = 0;
			Projectile.aiStyle = -1;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 240;
			Projectile.scale = 1f;
		}

		public override void AI()
		{
			if (Projectile.localAI[0] < Projectile.ai[1])
			{
				Projectile.alpha = (int)Math.Max(128, Math.Min(255, 255 + (Projectile.localAI[0] - Projectile.ai[1]) * 128f / 30f));

				Projectile.hostile = false;
			}
			else if (Projectile.localAI[0] == Projectile.ai[1])
			{
				Projectile.alpha = 0;

				Projectile.hostile = true;
				Projectile.timeLeft = 30;
				SoundEngine.PlaySound(SoundID.Item92, Projectile.Center);
			}

			Projectile.localAI[0]++;
		}

        public override bool ShouldUpdatePosition()
        {
			return false;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			if (Main.expertMode)
			{
				//target.AddBuff(BuffType<FractalSubworldDebuff>(), 600);
				//target.GetModPlayer<PolaritiesPlayer>().suddenFractalizationChange = true;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float point = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity * Projectile.ai[0], 8, ref point);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float projScale = (float)(5 + Math.Sin(Main.GameUpdateCount / 1.5f)) / 5f * (Projectile.hostile ? 0.5f : 0.05f);
			int projOffset = 16;

			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Rectangle frame = texture.Frame();
			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + Projectile.velocity * projScale * projOffset, frame, Color.White * (1 - Projectile.alpha / 255f), Projectile.velocity.ToRotation(), new Vector2(0, 16), new Vector2(Projectile.ai[0] - projScale * projOffset * 2, projScale), SpriteEffects.None, 0f);

			texture = ModContent.Request<Texture2D>("Polarities/NPCs/RiftDenizen/ChordRayCap", AssetRequestMode.ImmediateLoad).Value;
			frame = texture.Frame();
			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + Projectile.velocity * projScale * projOffset, frame, Color.White * (1 - Projectile.alpha / 255f), Projectile.velocity.ToRotation(), new Vector2(32, 16), projScale, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + Projectile.velocity * (Projectile.ai[0] - projScale * projOffset), frame, Color.White * (1 - Projectile.alpha / 255f), Projectile.velocity.ToRotation() + MathHelper.Pi, new Vector2(32, 16), projScale, SpriteEffects.None, 0f);

			return false;
		}
	}
}