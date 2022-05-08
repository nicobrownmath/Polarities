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
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent;
using ReLogic.Content;

namespace Polarities.NPCs.Enemies.WorldEvilInvasion
{
	public class Crimago : ModNPC
	{
		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[NPC.type] = 4;

			NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
			{
				SpecificallyImmuneTo = new int[] {
					BuffID.Confused,
				}
			};
			NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);

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
			NPC.width = 58;
			NPC.height = 58;
			DrawOffsetY = 22;

			NPC.defense = 25;
			NPC.damage = 45;
			NPC.lifeMax = 3900;
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
			BannerItem = ItemType<CrimagoBanner>();

			SpawnModBiomes = new int[1] { GetInstance<Biomes.WorldEvilInvasion>().Type };
		}

		private int scytheLeftFrame;
		private int scytheLeftFrameCounter;
		private int scytheRightFrame;
		private int scytheRightFrameCounter;

		public override void AI()
		{
			NPC.TargetClosest(false);
			Player player = Main.player[NPC.target];

			if (!PolaritiesSystem.worldEvilInvasion)
			{
				//flee if not in the invasion
				Vector2 goalPosition = NPC.Center * 2 - player.Center;
				Vector2 goalVelocity = (goalPosition - NPC.Center) / 15;
				NPC.velocity += (goalVelocity - NPC.velocity) / 15;

				NPC.rotation = NPC.velocity.X / 16;

				if (NPC.velocity.Length() > 16)
				{
					NPC.velocity.Normalize();
					NPC.velocity *= 16;
				}

				UpdateScythes();

				return;
			}

			switch (NPC.ai[0])
			{
				case 0:
					//try to fly into the player from the sides, slashing once when you get close enough
					NPC.ai[1]++;
					if (NPC.ai[1] % 240 == 0)
					{
						if (Main.netMode != 1)
						{
							if (Main.rand.NextBool())
							{
								NPC.ai[0] = (Main.rand.Next(2) + NPC.ai[0] + 1) % 3;
							}
						}
						NPC.netUpdate = true;
					}

					Vector2 goalPosition = player.Center + new Vector2(360 * (float)Math.Sin((NPC.ai[1] - NPC.ai[2]) / 60), 30 * (float)Math.Sin((NPC.ai[1] - NPC.ai[2]) / 15));
					Vector2 goalVelocity = (goalPosition - NPC.Center) / 15;
					NPC.velocity += (goalVelocity - NPC.velocity) / 15;

					if (Math.Abs(360 * (float)Math.Sin((NPC.ai[1] - NPC.ai[2]) / 60)) < 180 && Math.Abs(360 * (float)Math.Sin((NPC.ai[1] - NPC.ai[2]) / 60)) > 170 && Math.Sin((NPC.ai[1] - NPC.ai[2]) / 30) < 0)
					{
						if (player.Center.X < NPC.Center.X)
						{
							ScytheLeft();
						}
						else
						{
							ScytheRight();
						}
					}
					break;
				case 1:
					//try to fly above the player and rain scythes on them
					NPC.ai[1]++;
					if (NPC.ai[1] % 240 == 0)
					{
						if (Main.netMode != 1)
						{
							NPC.ai[0] = (Main.rand.Next(2) + NPC.ai[0] + 1) % 3;
						}
						NPC.netUpdate = true;
						NPC.ai[2] = NPC.ai[1];
					}

					goalPosition = player.Center + new Vector2(0, -270) + new Vector2(60 * (float)Math.Sin(NPC.ai[1] / 20), 60 * (float)Math.Sin(NPC.ai[1] / 15));
					goalVelocity = (goalPosition - NPC.Center) / 30;
					NPC.velocity += (goalVelocity - NPC.velocity) / 30;

					if (NPC.ai[1] % 120 == 30)
					{
						ScytheLeft();
					}
					else if (NPC.ai[1] % 120 == 90)
					{
						ScytheRight();
					}
					break;
				case 2:
					//slash repeatedly at the player while flying side-to-side above them
					NPC.ai[1]++;
					if (NPC.ai[1] % 240 == 0 && Main.netMode != 1 && Main.rand.NextBool())
					{
						if (Main.netMode != 1)
						{
							if (Main.rand.NextBool())
							{
								NPC.ai[0] = (Main.rand.Next(2) + NPC.ai[0] + 1) % 3;
							}
						}
						NPC.netUpdate = true;
					}

					goalPosition = player.Center + new Vector2(0, -270) + new Vector2(360 * (float)Math.Sin((NPC.ai[1] - NPC.ai[2]) / 60), 60 * (float)Math.Sin((NPC.ai[1] - NPC.ai[2]) / 15));
					goalVelocity = (goalPosition - NPC.Center) / 30;
					NPC.velocity += (goalVelocity - NPC.velocity) / 30;

					if (NPC.ai[1] % 120 == 60)
					{
						if (player.Center.X < NPC.Center.X)
						{
							ScytheLeft();
						}
						else
						{
							ScytheRight();
						}
					}

					break;
			}

			NPC.rotation = NPC.velocity.X / 16;

			if (NPC.velocity.Length() > 16)
			{
				NPC.velocity.Normalize();
				NPC.velocity *= 16;
			}

			UpdateScythes();
		}

		private void ScytheLeft()
		{
			if (scytheLeftFrameCounter == 0)
			{
				scytheLeftFrameCounter++;
			}
		}

		private void ScytheRight()
		{
			if (scytheRightFrameCounter == 0)
			{
				scytheRightFrameCounter++;
			}
		}

		private void UpdateScythes()
		{
			int damage = 22;
			int speed = 12;

			if (scytheLeftFrameCounter != 0)
			{
				scytheLeftFrameCounter++;
				if (scytheLeftFrameCounter == 6)
				{
					scytheLeftFrameCounter = 1;
					scytheLeftFrame++;
					if (scytheLeftFrame == 4)
					{
						scytheLeftFrame = 0;
						scytheLeftFrameCounter = 0;

						//shoot scythe
						Player player = Main.player[NPC.target];
						Vector2 shotPosition = NPC.Center + new Vector2(-60, 0).RotatedBy(NPC.rotation);

						if (Main.netMode != 1)
						{
							float shotRotation = (player.Center - shotPosition).ToRotation();

							Projectile.NewProjectile(NPC.GetSource_FromAI(), shotPosition, new Vector2(speed, 0).RotatedBy(shotRotation), ProjectileType<CrimagoSlash>(), damage, 2f, Main.myPlayer);
							Projectile.NewProjectile(NPC.GetSource_FromAI(), shotPosition, new Vector2(speed, 0).RotatedBy(shotRotation + 0.05f), ProjectileType<CrimagoSlash>(), damage, 2f, Main.myPlayer);
							Projectile.NewProjectile(NPC.GetSource_FromAI(), shotPosition, new Vector2(speed, 0).RotatedBy(shotRotation - 0.05f), ProjectileType<CrimagoSlash>(), damage, 2f, Main.myPlayer);

							if (Main.expertMode && NPC.life * 2 < NPC.lifeMax)
							{
								Projectile.NewProjectile(NPC.GetSource_FromAI(), shotPosition, new Vector2(speed, 0).RotatedBy(shotRotation + 0.1f), ProjectileType<CrimagoSlash>(), damage, 2f, Main.myPlayer);
								Projectile.NewProjectile(NPC.GetSource_FromAI(), shotPosition, new Vector2(speed, 0).RotatedBy(shotRotation - 0.1f), ProjectileType<CrimagoSlash>(), damage, 2f, Main.myPlayer);
							}
						}

						SoundEngine.PlaySound(SoundID.Item, shotPosition, 71);
					}
				}
			}
			if (scytheRightFrameCounter != 0)
			{
				scytheRightFrameCounter++;
				if (scytheRightFrameCounter == 6)
				{
					scytheRightFrameCounter = 1;
					scytheRightFrame++;
					if (scytheRightFrame == 4)
					{
						scytheRightFrame = 0;
						scytheRightFrameCounter = 0;

						//shoot scythe
						Player player = Main.player[NPC.target];
						Vector2 shotPosition = NPC.Center + new Vector2(60, 0).RotatedBy(NPC.rotation);

						if (Main.netMode != 1)
						{
							float shotRotation = (player.Center - shotPosition).ToRotation();

							Projectile.NewProjectile(NPC.GetSource_FromAI(), shotPosition, new Vector2(speed, 0).RotatedBy(shotRotation), ProjectileType<CrimagoSlash>(), damage, 2f, Main.myPlayer);
							Projectile.NewProjectile(NPC.GetSource_FromAI(), shotPosition, new Vector2(speed, 0).RotatedBy(shotRotation + 0.05f), ProjectileType<CrimagoSlash>(), damage, 2f, Main.myPlayer);
							Projectile.NewProjectile(NPC.GetSource_FromAI(), shotPosition, new Vector2(speed, 0).RotatedBy(shotRotation - 0.05f), ProjectileType<CrimagoSlash>(), damage, 2f, Main.myPlayer);

							if (Main.expertMode && NPC.life * 2 < NPC.lifeMax)
							{
								Projectile.NewProjectile(NPC.GetSource_FromAI(), shotPosition, new Vector2(speed, 0).RotatedBy(shotRotation + 0.1f), ProjectileType<CrimagoSlash>(), damage, 2f, Main.myPlayer);
								Projectile.NewProjectile(NPC.GetSource_FromAI(), shotPosition, new Vector2(speed, 0).RotatedBy(shotRotation - 0.1f), ProjectileType<CrimagoSlash>(), damage, 2f, Main.myPlayer);
							}
						}

						SoundEngine.PlaySound(SoundID.Item, shotPosition, 71);
					}
				}
			}
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter++;
			if (NPC.frameCounter == 5)
			{
				NPC.frameCounter = 0;
				NPC.frame.Y = (NPC.frame.Y + frameHeight) % (frameHeight * 4);
			}
		}

		public static Asset<Texture2D> WingsTexture;
		public static Asset<Texture2D> EyesTexture;
		public static Asset<Texture2D> ClawTexture;

		public override void Load()
		{
			WingsTexture = Request<Texture2D>(Texture + "_Wings");
			EyesTexture = Request<Texture2D>(Texture + "_Eyes");
			ClawTexture = Request<Texture2D>(Texture + "_Claw");
		}

        public override void Unload()
        {
			WingsTexture = null;
			EyesTexture = null;
			ClawTexture = null;
		}

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D mask = WingsTexture.Value;
			Vector2 drawOrigin = new Vector2(mask.Width * 0.5f, (mask.Height / 4) * 0.5f);
			Vector2 drawPos = NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY + 4);
			spriteBatch.Draw(mask, drawPos, NPC.frame, NPC.GetNPCColorTintedByBuffs(drawColor), NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);

			mask = ClawTexture.Value;
			spriteBatch.Draw(mask, drawPos, new Rectangle(0, scytheLeftFrame * NPC.frame.Height, NPC.frame.Width, NPC.frame.Height), NPC.GetNPCColorTintedByBuffs(drawColor), NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
			spriteBatch.Draw(mask, drawPos, new Rectangle(0, scytheRightFrame * NPC.frame.Height, NPC.frame.Width, NPC.frame.Height), NPC.GetNPCColorTintedByBuffs(drawColor), NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.FlipHorizontally, 0f);

			return true;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Player player = Main.player[NPC.target];
			Vector2 offset = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 2;

			if (NPC.IsABestiaryIconDummy)
            {
				offset = new Vector2(0, 2);
            }

			Texture2D mask = EyesTexture.Value;
			Vector2 drawOrigin = new Vector2(mask.Width * 0.5f, (mask.Height / 4) * 0.5f);
			Vector2 drawPos = NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY + 4) + offset;
			spriteBatch.Draw(mask, drawPos, NPC.frame, NPC.GetNPCColorTintedByBuffs(drawColor), NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
		}

		public override bool CheckDead()
		{
			for (int i = 1; i <= 4; i++)
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("CrimagoGore" + i).Type);

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

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
			//TODO: npcLoot.Add(ItemDropRule.Common(ItemType<PincerStaff>(), 8));
			npcLoot.Add(ItemDropRule.Common(ItemID.CrimtaneOre, 1, 2, 4));
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
	}

	public class CrimagoSlash : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 1;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 20;
			Projectile.height = 20;
			DrawOffsetX = -6;

			DrawOriginOffsetY = 0;
			DrawOriginOffsetX = 0;
			Projectile.alpha = 0;
			Projectile.timeLeft = 96;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			if (Projectile.timeLeft < 256 / 8)
			{
				Projectile.alpha += 8;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float trailLength = 4f;
			float trailSize = 4;
			for (int i = (int)trailLength - 1; i >= 0; i--)
			{
				Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Projectile.velocity * (i / trailLength) * trailSize - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), lightColor * (1 - Projectile.alpha / 256f) * (1 - i / trailLength), Projectile.rotation, TextureAssets.Projectile[Type].Size() / 2, Projectile.scale * (1 - i / trailLength), SpriteEffects.None, 0);
			}
			return true;
		}
	}
}
