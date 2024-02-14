using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Items.Placeable.Banners;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using ReLogic.Content;
using Terraria.GameContent.ItemDropRules;
using Polarities.Items.Weapons.Magic;

namespace Polarities.NPCs.Enemies.WorldEvilInvasion
{
	public class LightEater : ModNPC
	{
		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[NPC.type] = 2;

			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
			{
				Rotation = 5 * MathHelper.PiOver4
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifiers);

			PolaritiesNPC.npcTypeCap[Type] = 1;
			PolaritiesNPC.customNPCCapSlot[Type] = NPCCapSlotID.WorldEvilInvasion;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                //spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCorruption,
				//flavor text
				this.TranslatedBestiaryEntry()
			});
		}

		public override void SetDefaults()
		{
			NPC.aiStyle = -1;
			NPC.width = 78;
			NPC.height = 78;
			DrawOffsetY = 17;

			NPC.defense = 26;
			NPC.damage = 60;
			NPC.lifeMax = 4000;
			NPC.knockBackResist = 0f;
			NPC.npcSlots = 1f;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = Item.buyPrice(silver: 25);

			Music = GetInstance<Biomes.WorldEvilInvasion>().Music;
			SceneEffectPriority = SceneEffectPriority.Event;

			Banner = Type;
			BannerItem = ItemType<LightEaterBanner>();

			SpawnModBiomes = new int[1] { GetInstance<Biomes.WorldEvilInvasion>().Type };

			shadowCloneActive = new bool[8];
			shadowClonePosition = new Vector2[8];
			shadowCloneVelocity = new Vector2[8];
			shadowCloneRotation = new float[8];
		}

		Vector2 targetPoint;

		public override void AI()
		{
			NPC.TargetClosest(true);
			Player player = Main.player[NPC.target];

			Vector2 goalPosition;
			Vector2 goalVelocity;

			if (!PolaritiesSystem.worldEvilInvasion)
			{
				//flee if not in the invasion
				goalPosition = player.Center;
				goalVelocity = (NPC.Center - goalPosition).SafeNormalize(Vector2.Zero) * 8;
				NPC.velocity += (goalVelocity - NPC.velocity) / 30;

				NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;

				UpdateShadowClones(player);

				return;
			}

			switch (NPC.ai[0])
			{
				case 0:
					//float in a circle around the player before dashing at them
					if (NPC.ai[1] == 0)
					{
						if (Main.netMode != NetmodeID.MultiplayerClient)
						{
							NPC.ai[2] = Main.rand.NextBool() ? 1 : -1;
						}
						NPC.netUpdate = true;

						targetPoint = player.Center;
					}
					if (NPC.ai[1] < 120)
					{
						targetPoint += (player.Center - targetPoint) / 10f;

						goalPosition = (NPC.Center - targetPoint).RotatedBy(NPC.ai[2] * MathHelper.TwoPi / 3) + NPC.Center;
						goalVelocity = (goalPosition - NPC.Center).SafeNormalize(Vector2.Zero) * 12;
						NPC.velocity += (goalVelocity - NPC.velocity) / 30;

						if (NPC.ai[1] == 60 && Main.rand.NextBool())
                        {
							SwapWithRandomClone();
                        }
					}
					else if (NPC.ai[1] < 128)
					{
						goalPosition = player.Center;
						goalVelocity = (goalPosition - NPC.Center).SafeNormalize(Vector2.Zero) * 20;
						NPC.velocity += (goalVelocity - NPC.velocity) / 4;
					}
					else
					{
						NPC.velocity *= 0.99f;
					}

					NPC.ai[1]++;
					if (NPC.ai[1] == 180)
					{
						NPC.ai[1] = 0;
						if (Main.netMode != 1)
						{
							NPC.ai[0] = Main.rand.Next(2);
						}
						NPC.netUpdate = true;
					}
					break;
				case 1:
					//move towards the player

					goalPosition = player.Center + new Vector2(64, 0).RotatedBy(0.04f * NPC.ai[1]);
					goalVelocity = (goalPosition - NPC.Center).SafeNormalize(Vector2.Zero) * 8;
					NPC.velocity += (goalVelocity - NPC.velocity) / 30;

					if (Main.rand.NextBool(60))
                    {
						SwapWithRandomClone(mustBeCloserToPlayer: true, mustHaveMinDist: true);
                    }

					NPC.ai[1]++;
					if (NPC.ai[1] == 150)
					{
						NPC.ai[1] = 0;
						if (Main.netMode != 1)
						{
							NPC.ai[0] = Main.rand.Next(2);
						}
						NPC.netUpdate = true;
					}
					break;
			}

			NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;

			UpdateShadowClones(player);
		}

		void SwapWithRandomClone(bool mustBeCloserToPlayer = false, bool mustHaveMinDist = false)
		{
			int maxClones = Main.expertMode ? 8 : 4;
			int numClones = (int)(maxClones - 1 - NPC.life / (float)NPC.lifeMax * (maxClones + 2));

			if (numClones > 0)
			{
				int index = Main.rand.Next(numClones);

				if (mustBeCloserToPlayer)
                {
					if (Main.player[NPC.target].Distance(NPC.Center) < Main.player[NPC.target].Distance(shadowClonePosition[index]))
                    {
						return;
                    }
                }
				if (mustHaveMinDist)
				{
					if (Main.player[NPC.target].Distance(shadowClonePosition[index]) < NPC.width)
					{
						return;
					}
				}

				Vector2 oldCenter = NPC.Center;
				Vector2 oldVelocity = NPC.velocity;

				NPC.Center = shadowClonePosition[index];
				NPC.velocity = shadowCloneVelocity[index];

				shadowClonePosition[index] = oldCenter;
				shadowCloneVelocity[index] = oldVelocity;
			}
		}

		bool[] shadowCloneActive;
		Vector2[] shadowClonePosition;
		Vector2[] shadowCloneVelocity;
		float[] shadowCloneRotation;

		private void UpdateShadowClones(Player player)
		{
			for (int i = 0; i < 8; i++)
			{
				int maxClones = Main.expertMode ? 8 : 4;

				if (NPC.life * (maxClones + 2) < NPC.lifeMax * (maxClones - i) && !shadowCloneActive[i])
				{
					shadowCloneActive[i] = true;
					shadowClonePosition[i] = NPC.Center;
					shadowCloneVelocity[i] = new Vector2(4, 0).RotatedByRandom(MathHelper.TwoPi);
				}

				if (shadowCloneActive[i])
				{
					Vector2 goalPosition;
					Vector2 goalVelocity;

					if (!PolaritiesSystem.worldEvilInvasion)
					{
						//flee if not in the invasion
						goalPosition = player.Center;
						goalVelocity = (shadowClonePosition[i] - goalPosition).SafeNormalize(Vector2.Zero) * 8;
						shadowCloneVelocity[i] += (goalVelocity - shadowCloneVelocity[i]) / 30;

						shadowClonePosition[i] += shadowCloneVelocity[i];

						break;
					}

					float numClones = 1 + (int)(maxClones - (NPC.life * (maxClones + 2) / (float)NPC.lifeMax));

					switch (NPC.ai[0])
					{
						case 0:
							//float in a circle around the player before dashing at them
							if (NPC.ai[1] < 120)
							{
								float time = Math.Max(1, 60 - NPC.ai[1]);

								goalPosition = targetPoint + (NPC.Center - targetPoint).RotatedBy((i + 1) * MathHelper.TwoPi / (numClones + 1));
								goalVelocity = (goalPosition - shadowClonePosition[i]) / (float)Math.Sqrt(time);
								shadowCloneVelocity[i] += (goalVelocity - shadowCloneVelocity[i]) / (float)Math.Sqrt(time);

								//shadowCloneRotation[i] = NPC.rotation + (i + 1) * MathHelper.TwoPi / (numClones + 1);
								shadowCloneRotation[i] = shadowCloneVelocity[i].ToRotation() + MathHelper.PiOver2;
							}
							else if (NPC.ai[1] < 128)
							{
								goalPosition = player.Center;
								goalVelocity = (goalPosition - shadowClonePosition[i]).SafeNormalize(Vector2.Zero) * 20f;
								shadowCloneVelocity[i] += (goalVelocity - shadowCloneVelocity[i]) / 4f;

								shadowCloneRotation[i] = shadowCloneVelocity[i].ToRotation() + MathHelper.PiOver2;
							}
							else
							{
								shadowCloneVelocity[i] *= 0.99f;

								shadowCloneRotation[i] = shadowCloneVelocity[i].ToRotation() + MathHelper.PiOver2;
							}
							break;
						case 1:
							//move towards the player
							goalPosition = player.Center + new Vector2(64, 0).RotatedBy((i + 1) * MathHelper.TwoPi / (numClones + 1) + NPC.ai[1] * 0.04f);
							goalVelocity = (goalPosition - shadowClonePosition[i]).SafeNormalize(Vector2.Zero) * 8;
							shadowCloneVelocity[i] += (goalVelocity - shadowCloneVelocity[i]) / 30;

							shadowCloneRotation[i] = shadowCloneVelocity[i].ToRotation() + MathHelper.PiOver2;
							break;
					}

					shadowClonePosition[i] += shadowCloneVelocity[i];
				}
			}
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
		{
			target.AddBuff(BuffID.Blackout, 60 * Main.rand.Next(2, 7));
		}

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
		{
			return NPC.life * 2 > NPC.lifeMax;
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter++;
			if (NPC.frameCounter == 10)
			{
				NPC.frameCounter = 0;
				NPC.frame.Y = (NPC.frame.Y + frameHeight) % (frameHeight * 2);
			}
		}

		public static Asset<Texture2D> ShadowTexture;

		public override void Load()
		{
			ShadowTexture = Request<Texture2D>(Texture + "_Shadow");
		}

		public override void Unload()
		{
			ShadowTexture = null;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			for (int i = 0; i < 8; i++)
			{
				if (shadowCloneActive[i])
				{
					Color lightColor = Lighting.GetColor((int)(shadowClonePosition[i].X / 16), (int)(shadowClonePosition[i].Y / 16));
					spriteBatch.Draw(ShadowTexture.Value, shadowClonePosition[i] - screenPos + new Vector2(0, 4), NPC.frame, NPC.GetNPCColorTintedByBuffs(lightColor) * (0.8f + 0.1f * (1 - (float)NPC.life / NPC.lifeMax)), shadowCloneRotation[i], new Vector2(49, 57), NPC.scale, SpriteEffects.None, 0f);
				}
			}
			return true;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			spriteBatch.Draw(ShadowTexture.Value, NPC.Center - screenPos + new Vector2(0, 4), NPC.frame, NPC.GetNPCColorTintedByBuffs(drawColor) * 0.5f * (1 - (float)NPC.life / NPC.lifeMax), NPC.rotation, new Vector2(49, 57), NPC.scale, SpriteEffects.None, 0f);
		}

		public override bool CheckDead()
		{
			for (int i = 1; i <= 2; i++)
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("LightEaterGore" + i).Type);

			for (int i = 0; i < 64; i++)
			{
				Vector2 dustPos = NPC.Center + new Vector2(Main.rand.NextFloat(28), 0).RotatedByRandom(MathHelper.TwoPi);
				Dust dust = Dust.NewDustPerfect(dustPos, 18, Velocity: (dustPos - NPC.Center) / 12, Scale: 2f);
				dust.noGravity = true;
			}

			if (PolaritiesSystem.worldEvilInvasion)
			{
				//counts for 7 points
				PolaritiesSystem.worldEvilInvasionSize -= 7;
			}

			return true;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ItemType<ShadeStorm>(), 8));
			npcLoot.Add(ItemDropRule.Common(ItemID.DemoniteOre, 1, 2, 4));
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			//only spawns during the evil event
			if (spawnInfo.Player.InModBiome(GetInstance<Biomes.WorldEvilInvasion>()))
			{
				return Biomes.WorldEvilInvasion.GetSpawnChance(spawnInfo.Player.ZoneCorrupt);
			}
			return 0f;
		}
	}
}
