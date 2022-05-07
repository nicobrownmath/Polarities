using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Items.Placeable.Banners;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Collections.Generic;
using Polarities.Biomes;
using Terraria.DataStructures;
using Terraria.GameContent;
using ReLogic.Content;
using Terraria.GameContent.ItemDropRules;
using Terraria.Audio;
using Polarities.Projectiles;
using Terraria.GameContent.Bestiary;
using Polarities.Items.Weapons.Magic;

namespace Polarities.NPCs.Enemies.HallowInvasion
{
	public class SunServitor : ModNPC
	{
		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 4;

			NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
			{
				SpecificallyImmuneTo = new int[] {
					BuffID.Confused,
					BuffID.OnFire
				}
			};
			NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);

			PolaritiesNPC.customNPCCapSlot[Type] = NPCCapSlotID.HallowInvasion;
			PolaritiesNPC.npcTypeCap[Type] = 1;
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
			NPC.width = 20;
			NPC.height = 52;
			DrawOffsetY = -4;

			NPC.defense = 20;
			NPC.damage = 40;
			NPC.lifeMax = 1000;
			NPC.knockBackResist = 0f;
			NPC.npcSlots = 1f;
			NPC.noGravity = false;
			NPC.noTileCollide = true;
			NPC.HitSound = SoundID.NPCHit5;
			NPC.DeathSound = SoundID.NPCDeath7;
			NPC.value = Item.buyPrice(silver: 25);

			Music = GetInstance<Biomes.HallowInvasion>().Music;
			SceneEffectPriority = SceneEffectPriority.Event;

			Banner = Type;
			BannerItem = ItemType<SunServitorBanner>();

			SpawnModBiomes = new int[1] { GetInstance<Biomes.HallowInvasion>().Type };
		}

		public override void AI()
		{
			NPC.noGravity = true;

			NPC.TargetClosest(false);
			Player player = Main.player[NPC.target];

			if (!PolaritiesSystem.hallowInvasion)
			{
				//poof away if not in the invasion

				for (int a = 0; a < 12; a++)
				{
					Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, 6, newColor: Color.White, Scale: 2f).noGravity = true;
				}

				NPC.active = false;

				return;
			}

			switch (NPC.ai[0])
			{
				case 0:
					//teleport to player and initialize attack
					if (NPC.ai[1] == 0)
					{
						//get teleport point
						if (GetTeleportPoint(player))
						{
							//teleport succeeded
						}
						else
						{
							//teleport failed, decrement timer to try again
							NPC.ai[1]--;
						}
					}

					if (NPC.ai[1] == 30)
					{
						Teleport();
					}

					NPC.frame.Y = 0 * 64;

					NPC.ai[1]++;
					if (NPC.ai[1] == 60)
					{
						NPC.ai[1] = 0;
						NPC.ai[0] = Main.rand.Next(1, 3);
					}
					break;
				case 1:
					//scrolling sunbeams (totally not a radiance knockoff I swear), make gaps big enough for the player to fit in comfortably though

					if (NPC.ai[1] == 0)
					{
						NPC.ai[2] = player.Center.X + NPC.direction * 256;
					}

					if (NPC.ai[1] % 30 == 0 && NPC.ai[1] <= 120)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(NPC.ai[2], player.Center.Y), Vector2.Zero, ProjectileType<SunServitorBeam>(), 25, 0f, Main.myPlayer);

						NPC.ai[2] -= NPC.direction * 128;
					}

					if ((NPC.ai[2] > NPC.Center.X) == (NPC.spriteDirection == 1))
					{
						NPC.frame.Y = 1 * 64;
					}
					else
					{
						NPC.frame.Y = 2 * 64;
					}

					NPC.ai[1]++;
					if (NPC.ai[1] == 240)
					{
						NPC.ai[1] = 0;
						NPC.ai[0] = 0;
					}
					break;
				case 2:
					//sunbeams on either side of the player to box them in

					if (NPC.ai[1] == 30)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(player.Center.X - 256, player.Center.Y), Vector2.Zero, ProjectileType<SunServitorBeam>(), 25, 0f, Main.myPlayer);
						Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(player.Center.X + 256, player.Center.Y), Vector2.Zero, ProjectileType<SunServitorBeam>(), 25, 0f, Main.myPlayer);
					}

					NPC.frame.Y = 3 * 64;

					NPC.ai[1]++;
					if (NPC.ai[1] == 150)
					{
						NPC.ai[1] = 0;
						NPC.ai[0] = 0;
					}
					break;
			}
		}

		private bool GetTeleportPoint(Player player)
		{
			//try up to 20 times
			for (int i = 0; i < 40; i++)
			{
				Vector2 tryGoalPoint = player.Center + new Vector2(-NPC.width / 2 + Main.rand.NextFloat(100f, 500f) * (Main.rand.Next(2) * 2 - 1), Main.rand.NextFloat(-500f, 500f));
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
						if (Main.tile[x, y].HasUnactuatedTile && (Main.tileSolid[Main.tile[x, y].TileType] || Main.tileSolidTop[Main.tile[x, y].TileType]))
						{
							Vector2 goalPosition = new Vector2(tryGoalPoint.X, y * 16 - NPC.height);
							NPC.ai[2] = goalPosition.X;
							NPC.ai[3] = goalPosition.Y;

							NPC.direction = player.Center.X > goalPosition.X + NPC.width / 2 ? 1 : -1;

							return true;
						}
					}
				}
			}
			return false;
		}

		private void Teleport()
		{
			for (int a = 0; a < 12; a++)
			{
				Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, 6, newColor: Color.White, Scale: 1.4f).noGravity = true;
			}

			NPC.position = new Vector2(NPC.ai[2], NPC.ai[3]);
			NPC.spriteDirection = NPC.direction;

			for (int a = 0; a < 12; a++)
			{
				Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, 6, newColor: Color.White, Scale: 1.4f).noGravity = true;
			}
		}

		public static Asset<Texture2D> TelegraphTexture;

        public override void Load()
        {
			TelegraphTexture = Request<Texture2D>(Texture + "_Telegraph");
		}

        public override void Unload()
        {
			TelegraphTexture = null;
		}

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (NPC.ai[0] == 0 && NPC.ai[1] < 30 && NPC.ai[1] > 0)
			{
				Texture2D telegraphTexture = TelegraphTexture.Value;
				Rectangle telegraphFrame = telegraphTexture.Frame();

				float timeAlpha = NPC.ai[1] >= 30 ? 0 : 1 - NPC.ai[1] / 30f;

				Color mainColor = new Color(255, 245, 168);
				Color endColor = new Color(255, 0, 0);
				Color swordColor = Color.Lerp(mainColor, endColor, ((float)Math.Sin(Main.GlobalTimeWrappedHourly * 12f) + 1) / 4f);

				Vector2 telegraphPosition = new Vector2(NPC.ai[2], NPC.ai[3]) + NPC.Center - NPC.position;
				spriteBatch.Draw(telegraphTexture, telegraphPosition + new Vector2(0, DrawOffsetY) - screenPos, telegraphFrame, swordColor * timeAlpha, NPC.rotation, telegraphFrame.Size() / 2, NPC.scale, NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);

				for (int i = 0; i < 4; i++)
				{
					Vector2 randomOffset = new Vector2(2, 0).RotatedByRandom(MathHelper.TwoPi);
					spriteBatch.Draw(telegraphTexture, telegraphPosition + randomOffset + new Vector2(0, DrawOffsetY) - screenPos, telegraphFrame, Color.White * (0.25f * timeAlpha), NPC.rotation, telegraphFrame.Size() / 2, NPC.scale, NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
				}
			}

			Texture2D texture = TextureAssets.Npc[Type].Value;
			Rectangle frame = NPC.frame;

			spriteBatch.Draw(texture, NPC.Center + new Vector2(0, DrawOffsetY) - screenPos, frame, Color.White, NPC.rotation, frame.Size() / 2, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
			return false;
		}

		public override bool CheckDead()
		{
			if (PolaritiesSystem.hallowInvasion)
			{
				//counts for 4 points
				PolaritiesSystem.hallowInvasionSize -= 4;
			}

			for (int a = 0; a < 12; a++)
			{
				Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, 6, newColor: Color.White, Scale: 2f).noGravity = true;
			}

			return true;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ItemType<StarburstScepter>(), 8));
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			if (spawnInfo.Player.ZoneTowerNebula || spawnInfo.Player.ZoneTowerSolar || spawnInfo.Player.ZoneTowerStardust || spawnInfo.Player.ZoneTowerVortex)
			{
				return 0f;
			}

			//only spawns during the hallow event
			if (spawnInfo.Player.InModBiome(GetInstance<Biomes.HallowInvasion>()))
			{
				return Biomes.HallowInvasion.GetSpawnChance(2);
			}
			return 0f;
		}
	}

	public class SunServitorBeam : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			/*Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 1, 32, false, SurfaceFormat.Color);
			List<Color> list = new List<Color>();
			for (int j = 0; j < texture.Height; j++)
			{
				for (int i = 0; i < texture.Width; i++)
				{
					float y = (2 * j / (float)(texture.Height - 1) - 1);

					float distanceSquared = y * y;
					float index = 1 - distanceSquared;

					int r = 255 - (int)(0 * (1 - index));
					int g = 255 - (int)(15 * (1 - index));
					int b = 255 - (int)(87 * (1 - index));
					int alpha = distanceSquared >= 1 ? 0 : (int)(255 * index);

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "SunServitorBeam.png", FileMode.Create), texture.Width, texture.Height);*/
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 2;
			Projectile.height = 2;

			Projectile.timeLeft = 120;
			Projectile.penetrate = -1;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;

			Projectile.GetGlobalProjectile<PolaritiesProjectile>().ForceDraw = true;
		}

		public override void AI()
		{
			if (Projectile.timeLeft == 60)
			{
				Projectile.hostile = true;

				SoundEngine.PlaySound(SoundID.Item, new Vector2(Projectile.Center.X, Main.LocalPlayer.Center.Y), 122);
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), new Vector2(Projectile.Center.X, 0), new Vector2(Projectile.Center.X, Main.maxTilesY * 16));
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float projScale = (float)(5 + Math.Sin(Projectile.timeLeft / 1.5f)) / 5f * (Projectile.hostile ? 0.5f : 0.05f);

			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame();

			Color mainColor = new Color(255, 245, 168);
			Color endColor = new Color(255, 0, 0);
			Color swordColor = Color.Lerp(mainColor, endColor, ((float)Math.Sin(Main.GlobalTimeWrappedHourly * 12f) + 1) / 4f);

			Main.EntitySpriteDraw(texture, new Vector2(Projectile.Center.X, 0) - Main.screenPosition, frame, swordColor, 0f, new Vector2(16, 0), new Vector2(projScale, Main.maxTilesY * 16), SpriteEffects.None, 0);

			return false;
		}
	}
}