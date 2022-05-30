using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Items.Placeable.Banners;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent;
using Terraria.Audio;
using Polarities.Items.Weapons.Melee;

namespace Polarities.NPCs.Enemies.WorldEvilInvasion
{
	public class TendrilAmalgam : ModNPC
	{
		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 19;

			NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
			{
				SpecificallyImmuneTo = new int[] {
					BuffID.Confused,
				}
			};
			NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);

			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
			{
				Position = new Vector2(0, 4)
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifiers);

			PolaritiesNPC.npcTypeCap[Type] = 1;
			PolaritiesNPC.customNPCCapSlot[Type] = NPCCapSlotID.WorldEvilInvasion;

			PolaritiesNPC.forceCountForRadar.Add(Type);
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
			NPC.width = 112;
			NPC.height = 112;

			NPC.defense = 30;
			NPC.damage = 0;
			NPC.lifeMax = 5000;
			NPC.knockBackResist = 0f;
			NPC.npcSlots = 1f;
			NPC.noGravity = false;
			NPC.noTileCollide = true;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = Item.buyPrice(silver: 25);

			NPC.behindTiles = true;

			Music = GetInstance<Biomes.WorldEvilInvasion>().Music;
			SceneEffectPriority = SceneEffectPriority.Event;

			Banner = Type;
			BannerItem = ItemType<LightEaterBanner>();

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
				NPC.ai[0] = 0;
			}

			NPC.ai[0]--;
			if (NPC.ai[0] <= 0)
			{
				bool attemptSuccessful = false;
				Teleport(player, ref attemptSuccessful);
				if (attemptSuccessful)
				{
					if (Main.netMode != 1)
					{
						if (Main.rand.NextBool(3))
						{
							//1/3 chance to shoot waves of spikes to the sides after teleporting
							NPC.ai[0] = 360;
							NPC.ai[2] = 0;
						}
						else
						{
							NPC.ai[0] = 240;
							NPC.ai[2] = 1;
						}
					}
					NPC.netUpdate = true;
				}
			}

			switch (NPC.ai[2])
			{
				case 0:
					int period = Main.expertMode ? 15 : 30;
					int speed = Main.expertMode ? 6 : 3;

					if (NPC.ai[0] % period == 0 && NPC.ai[0] > 60 && NPC.ai[0] < 360)
					{
						Vector2 lineDirection = new Vector2(0, 16);
						Vector2 offset = Vector2.Zero;

						Vector2 startPos = NPC.Center + new Vector2((360 - NPC.ai[0]) * speed, -100);

						while (offset.Length() < 1000)
						{
							offset += lineDirection;

							Tile tile = Main.tile[(int)((startPos + offset).X / 16), (int)((startPos + offset).Y / 16)];
							if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType])
							{
								Vector2 location = new Vector2(16 * (int)((startPos + offset).X / 16) + 8, 16 * (int)((startPos + offset).Y / 16) + 8);

								if (Main.netMode != 1)
									Projectile.NewProjectile(NPC.GetSource_FromAI(), location, Vector2.Zero, ProjectileType<TendrilAmalgamSpike>(), 23, 0, Main.myPlayer, lineDirection.ToRotation() + MathHelper.Pi, -16 * 60);
								break;
							}
						}

						lineDirection = new Vector2(0, 16);
						offset = Vector2.Zero;

						startPos = NPC.Center + new Vector2(-(360 - NPC.ai[0]) * speed, -100);

						while (offset.Length() < 1000)
						{
							offset += lineDirection;

							Tile tile = Main.tile[(int)((startPos + offset).X / 16), (int)((startPos + offset).Y / 16)];
							if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType])
							{
								Vector2 location = new Vector2(16 * (int)((startPos + offset).X / 16) + 8, 16 * (int)((startPos + offset).Y / 16) + 8);

								if (Main.netMode != 1)
									Projectile.NewProjectile(NPC.GetSource_FromAI(), location, Vector2.Zero, ProjectileType<TendrilAmalgamSpike>(), 23, 0, Main.myPlayer, lineDirection.ToRotation() + MathHelper.Pi, -16 * 60);
								break;
							}
						}
					}
					break;
				case 1:
					if (NPC.life * 2 >= NPC.lifeMax || !Main.expertMode)
					{
						if (NPC.ai[0] == 90 || NPC.ai[0] == 120 || NPC.ai[0] == 150)
						{
							//add an attack charge if it has fewer than 3
							if (NPC.ai[1] < 3)
								NPC.ai[1]++;
						}
					}
					else
					{
						if (NPC.ai[0] == 90 || NPC.ai[0] == 120 || NPC.ai[0] == 150)
						{
							//add charges in pairs up to 6;
							NPC.ai[1] += 2;
							if (NPC.ai[1] > 6)
							{
								NPC.ai[1] = 6;
							}
						}
					}

					if (NPC.ai[1] > 0)
					{
						//raycast in a random direction from the player
						//if there is a tile there, create a projectile at that position facing the player
						Vector2 lineDirection = new Vector2(16, 0).RotatedByRandom(MathHelper.TwoPi);
						Vector2 offset = Vector2.Zero;

						while (offset.Length() < 200)
						{
							offset += lineDirection;

							Tile tile = Main.tile[(int)((player.Center + offset).X / 16), (int)((player.Center + offset).Y / 16)];
							if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType])
							{
								Vector2 location = new Vector2(16 * (int)((player.Center + offset).X / 16) + 8, 16 * (int)((player.Center + offset).Y / 16) + 8);

								if (Main.netMode != 1)
									Projectile.NewProjectile(NPC.GetSource_FromAI(), location, Vector2.Zero, ProjectileType<TendrilAmalgamSpike>(), 28, 0, Main.myPlayer, lineDirection.ToRotation() + MathHelper.Pi, -16 * 60);

								NPC.ai[1]--;
								break;
							}
						}
					}
					break;
			}

			NPC.dontTakeDamage = (NPC.frame.Y >= 4 * NPC.frame.Height);
		}

		private float directional;

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(directional);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			directional = reader.ReadSingle();
		}

		private void Teleport(Player player, ref bool attemptSuccessful)
		{
			//try up to 40 times
			for (int i = 0; i < 40; i++)
			{
				if (Main.netMode != 1)
				{
					directional = Main.rand.NextFloat(100f, 500f) * (Main.rand.Next(2) * 2 - 1);
					NPC.ai[3] = Main.rand.NextFloat(-500f, 500f);
				}
				NPC.netUpdate = true;

				Vector2 tryGoalPoint = player.Center + new Vector2(-NPC.width / 2 + directional, NPC.ai[3]);
				tryGoalPoint.Y = 16 * (int)(tryGoalPoint.Y / 16);
				tryGoalPoint -= new Vector2(0, NPC.height);

				bool viable = true;

				for (int x = (int)((tryGoalPoint.X) / 16); x <= (int)((tryGoalPoint.X + NPC.width) / 16); x++)
				{
					for (int y = (int)((tryGoalPoint.Y) / 16); y <= (int)((tryGoalPoint.Y + NPC.height) / 16); y++)
					{
						if (Main.tile[x, y].HasUnactuatedTile)
						{
							viable = false;
							break;
						}
					}
					if (!viable)
					{
						break;
					}
				}

				if (viable)
				{
					for (int y = (int)((tryGoalPoint.Y + NPC.height) / 16); y < Main.maxTilesY; y++)
					{
						int x = (int)((tryGoalPoint.X + NPC.width / 2) / 16);
						viable = true;

						for (int xVal = (int)((tryGoalPoint.X) / 16); xVal <= (int)((tryGoalPoint.X + NPC.width) / 16); xVal++)
						{
							if (!Main.tile[xVal, y].HasUnactuatedTile || !(Main.tileSolid[Main.tile[xVal, y].TileType] || Main.tileSolid[Main.tile[xVal, y].TileType]))
							{
								viable = false;
								break;
							}
						}

						if (viable)
						{
							//teleport effects
							for (int a = 0; a < 12; a++)
							{
								Dust.NewDust(NPC.position + new Vector2(NPC.width / 2, NPC.height), 1, 1, 18, newColor: Color.Transparent, Alpha: 32, Scale: 1.4f);
							}

							if (!PolaritiesSystem.worldEvilInvasion)
                            {
								//flee if not in event
								NPC.active = false;
								return;
                            }

							NPC.position = new Vector2(tryGoalPoint.X, y * 16 - NPC.height);

							//after-teleport effects
							for (int a = 0; a < 12; a++)
							{
								Dust.NewDust(NPC.position + new Vector2(NPC.width / 2, NPC.height), 1, 1, 18, newColor: Color.Transparent, Alpha: 32, Scale: 1.4f);
							}
							break;
						}
					}
					attemptSuccessful = true;
					break;
				}
			}
			NPC.spriteDirection = Main.rand.Next(2) * 2 - 1;
			NPC.netUpdate = true;
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter++;
			if (NPC.frameCounter == 4)
			{
				NPC.frameCounter = 0;

				NPC.frame.Y += frameHeight;

				if (NPC.ai[0] > 30 && NPC.frame.Y == 4 * frameHeight)
				{
					NPC.frame.Y = 0;
				}
				else if (NPC.frame.Y == 19 * frameHeight)
				{
					NPC.frame.Y = 0;
				}
			}
		}

		public override bool CheckDead()
		{
			for (int i = 0; i < 64; i++)
			{
				Vector2 dustPos = NPC.Center + new Vector2(Main.rand.NextFloat(40), 0).RotatedByRandom(MathHelper.TwoPi);
				Dust dust = Dust.NewDustPerfect(dustPos, 18, Velocity: (dustPos - NPC.Center) / 12, Alpha: 32, Scale: 2f);
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
			npcLoot.Add(ItemDropRule.NormalvsExpert(ItemType<TwistedTendril>(), 4, 3));
			npcLoot.Add(ItemDropRule.Common(ItemID.RottenChunk, 1, 2, 5));
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


	public class TendrilAmalgamSpike : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 3;
		}

		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;
			DrawOffsetX = 0;
			DrawOriginOffsetY = -16;
			DrawOriginOffsetX = -80;

			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hide = true;
			Projectile.timeLeft = 240;
		}

		public override bool? CanDamage()
		{
			return Projectile.ai[1] > 0 ? null : false;
		}

		// Change the way of collision check of the projectile
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 unit = new Vector2(1, 0).RotatedBy(Projectile.rotation);
			float Distance = Projectile.ai[1];

			float point = 0f;
			// Run an AABB versus Line check to look for collisions, look up AABB collision first to see how it works
			// It will look for collisions on the given line using AABB
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
				Projectile.Center + unit * Distance, 5, ref point);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			lightColor = Lighting.GetColor((int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16));

			if (Projectile.ai[1] > 0)
				Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, new Rectangle(177 - (int)Projectile.ai[1], Projectile.frame * 34, (int)Projectile.ai[1] + 17, 34), lightColor, Projectile.rotation, new Vector2(17, 17), 1f, SpriteEffects.None, 0);

			return false;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
			behindNPCsAndTiles.Add(index);
		}

		// The AI of the projectile
		public override void AI()
		{
			if (Projectile.localAI[0] == 0)
			{
				Projectile.localAI[0] = 1;
				Projectile.frame = Main.rand.Next(3);
				Projectile.rotation = Projectile.ai[0];
				Projectile.ai[0] = 0;
			}

			if (Projectile.ai[0] < 120)
			{
				if (Projectile.ai[1] == -16 * 20 || Projectile.ai[1] == -16 * 40 || Projectile.ai[1] == -16 * 60)
				{
					for (int i = 0; i < 16; i++)
					{
						Dust.NewDustPerfect(Projectile.Center, 14, new Vector2(1, 0).RotatedBy(i * MathHelper.TwoPi / 16), 0, Color.Transparent, 1);
					}
				}

				Projectile.ai[1] += 16;

				if (Projectile.ai[1] == 0)
				{
					SoundEngine.PlaySound(SoundID.Item71, Projectile.Center);
				}

				if (Projectile.ai[1] > 177)
				{
					Projectile.ai[1] = 177;

					Projectile.ai[0]++;
				}
			}
			else
			{
				Projectile.ai[1] -= 16;
			}
		}
	}
}