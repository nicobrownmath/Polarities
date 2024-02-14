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
using ReLogic.Content;
using MultiHitboxNPCLibrary;

namespace Polarities.NPCs.Enemies.WorldEvilInvasion
{
	public class RavenousCursed : ModNPC
	{
		public override void SetStaticDefaults()
		{
			PolaritiesNPC.customNPCCapSlot[Type] = NPCCapSlotID.WorldEvilInvasionWorm;

            MultiHitboxNPC.MultiHitboxNPCTypes.Add(Type);
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
			NPC.width = 18;
			NPC.height = 18;
			NPC.defense = 10;
			NPC.damage = 36;
			NPC.knockBackResist = 0f;
			NPC.lifeMax = 4000;
			NPC.noTileCollide = true;
			NPC.noGravity = true;
			NPC.lavaImmune = true;
			NPC.npcSlots = 1f;
			NPC.behindTiles = true;
			NPC.value = Item.buyPrice(silver: 5);
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;

			Music = GetInstance<Biomes.WorldEvilInvasion>().Music;
			SceneEffectPriority = SceneEffectPriority.Event;

			Banner = Type;
			BannerItem = ItemType<LivingSpineBanner>();

			SpawnModBiomes = new int[1] { GetInstance<Biomes.WorldEvilInvasion>().Type };

			segmentPositions = new Vector2[numSegments * segmentsPerHitbox + 12];
			segmentTypes = new int[numSegments];
		}

		const int numSegments = 20;
		const int segmentsPerHitbox = 9;
		const int segmentsHead = 18;
		const int segmentsTail = 12;
		const int hitboxSegmentOffset = 14;
		private Vector2[] segmentPositions;
		private int[] segmentTypes;

		private int attackType;

		public override void AI()
		{
			Player player = Main.player[NPC.target];
			if (!player.active || player.dead)
			{
				NPC.TargetClosest(false);
				player = Main.player[NPC.target];
			}

			if (NPC.localAI[0] == 0)
			{
				NPC.rotation = (player.Center - NPC.Center).ToRotation();
				NPC.localAI[0] = 1;

				segmentPositions[0] = NPC.Center + new Vector2(NPC.width / 2 - 2, 0).RotatedBy(NPC.rotation);
				for (int i = 1; i < segmentPositions.Length; i++)
				{
					segmentPositions[i] = segmentPositions[i - 1] - new Vector2(NPC.width, 0).RotatedBy(NPC.rotation);
				}

				for (int i = 0; i < segmentTypes.Length; i++)
				{
					segmentTypes[i] = Main.rand.Next(3);
				}
			}

			//changeable ai values
			float rotationFade = 11f;
			float rotationAmount = 0.1f;

			//Do AI
			NPC.noGravity = Main.tile[(int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16)].LiquidAmount > 64 || (Main.tile[(int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16)].HasTile && Main.tileSolid[Main.tile[(int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16)].TileType]);

			if (NPC.noGravity)
			{
				if (PolaritiesSystem.worldEvilInvasion)
				{
					Vector2 targetPoint = player.Center + new Vector2(0, -240);
					Vector2 velocityGoal = 16 * (targetPoint - NPC.Center).SafeNormalize(Vector2.Zero);
					NPC.velocity += (velocityGoal - NPC.velocity) / 90;

					if (Main.netMode != 1)
					{
						attackType = Main.rand.Next(3);
					}
					NPC.netUpdate = true;
				}

				//dig sounds, adapted from vanilla
				Vector2 vector18 = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
				float num191 = Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2);
				float num192 = Main.player[NPC.target].position.Y + (float)(Main.player[NPC.target].height / 2);
				num191 = (float)((int)(num191 / 16f) * 16);
				num192 = (float)((int)(num192 / 16f) * 16);
				vector18.X = (float)((int)(vector18.X / 16f) * 16);
				vector18.Y = (float)((int)(vector18.Y / 16f) * 16);
				num191 -= vector18.X;
				num192 -= vector18.Y;
				float num193 = (float)System.Math.Sqrt((double)(num191 * num191 + num192 * num192));
				if (NPC.soundDelay == 0)
				{
					float num195 = num193 / 40f;
					if (num195 < 10f)
					{
						num195 = 10f;
					}
					if (num195 > 20f)
					{
						num195 = 20f;
					}
					NPC.soundDelay = (int)num195;
					SoundEngine.PlaySound(SoundID.WormDig, NPC.position);
				}
			}
			else
			{
				if (Main.netMode != 1)
				{
					NPC.direction = Main.rand.NextBool() ? -1 : 1;
				}
				if (NPC.velocity.X == 0)
				{
					NPC.velocity.X = NPC.direction * 0.01f;
				}
				NPC.netUpdate = true;

				switch (attackType)
				{
					case 0:
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + NPC.velocity, new Vector2(Main.rand.NextFloat(15, 17), 0).RotatedBy(NPC.velocity.ToRotation()).RotatedByRandom(0.2f), ProjectileType<RavenousCursedFlame>(), 20, 0, Main.myPlayer);
						if (NPC.soundDelay <= 0)
						{
							NPC.soundDelay = 20;
							SoundEngine.PlaySound(SoundID.Item34, NPC.Center);
						}
						NPC.velocity.Y += 0.15f;
						break;
					case 1:
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + NPC.velocity, new Vector2(2, 0).RotatedByRandom(MathHelper.TwoPi), ProjectileType<RavenousCursedSpark>(), 18, 0, Main.myPlayer);
						NPC.velocity.Y += 0.15f;
						break;
					default:
						NPC.velocity.Y += 0.3f;
						break;
				}
			}

			float minSpeed = 6;
			float maxSpeed = 16;
			if (NPC.velocity.Length() > maxSpeed)
			{
				NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * maxSpeed;
			}
			if (NPC.velocity.Length() < minSpeed)
			{
				//prevents extreme ascent in worms with min speed
				if (NPC.noGravity = false && NPC.velocity.Y < 0)
				{
					float xVelocity = (NPC.velocity.X > 0 ? 1 : (NPC.velocity.X < 0 ? -1 : (Main.rand.NextBool() ? 1 : -1))) * (float)Math.Sqrt(minSpeed * minSpeed - NPC.velocity.Y * NPC.velocity.Y);
					NPC.velocity = new Vector2(xVelocity, NPC.velocity.Y);
				}
				else
				{
					NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * minSpeed;
				}
			}

			NPC.rotation = NPC.velocity.ToRotation();
			NPC.noGravity = true;

			//update segment positions
			segmentPositions[0] = NPC.Center + NPC.velocity + new Vector2(NPC.width / 2 - 2, 0).RotatedBy(NPC.rotation);
			Vector2 rotationGoal = Vector2.Zero;

			for (int i = 1; i < segmentPositions.Length; i++)
			{
				if (i > 1)
				{
					rotationGoal = ((rotationFade - 1) * rotationGoal + (segmentPositions[i - 1] - segmentPositions[i - 2])) / rotationFade;
				}

				segmentPositions[i] = segmentPositions[i - 1] + (rotationAmount * rotationGoal + (segmentPositions[i] - segmentPositions[i - 1]).SafeNormalize(Vector2.Zero)).SafeNormalize(Vector2.Zero) * 2;
			}

			//position hitbox segments
			List<RectangleHitboxData> hitboxes = new List<RectangleHitboxData>();
			for (int h = 0; h < numSegments; h++)
			{
				Vector2 spot = segmentPositions[h * segmentsPerHitbox + hitboxSegmentOffset];
				hitboxes.Add(new RectangleHitboxData(new Rectangle((int)spot.X - NPC.width / 2, (int)spot.Y - NPC.height / 2, NPC.width, NPC.height)));
			}
			NPC.GetGlobalNPC<MultiHitboxNPC>().AssignHitboxFrom(hitboxes);

			//dig effect adapted from vanilla
			foreach (RectangleHitbox rectangleHitbox in NPC.GetGlobalNPC<MultiHitboxNPC>().hitboxes.AllHitboxes())
			{
				Rectangle hitbox = rectangleHitbox.hitbox;
				int num180 = (int)(hitbox.X / 16f) - 1;
				int num181 = (int)((hitbox.X + hitbox.Width) / 16f) + 2;
				int num182 = (int)(hitbox.Y / 16f) - 1;
				int num183 = (int)((hitbox.Y + hitbox.Height) / 16f) + 2;
				if (num180 < 0)
				{
					num180 = 0;
				}
				if (num181 > Main.maxTilesX)
				{
					num181 = Main.maxTilesX;
				}
				if (num182 < 0)
				{
					num182 = 0;
				}
				if (num183 > Main.maxTilesY)
				{
					num183 = Main.maxTilesY;
				}
				for (int num184 = num180; num184 < num181; num184++)
				{
					for (int num185 = num182; num185 < num183; num185++)
					{
						if (Main.tile[num184, num185] != null && (Main.tile[num184, num185].HasUnactuatedTile && (Main.tileSolid[(int)Main.tile[num184, num185].TileType] || Main.tileSolidTop[(int)Main.tile[num184, num185].TileType] && Main.tile[num184, num185].TileFrameY == 0) || Main.tile[num184, num185].LiquidAmount > 64))
						{
							Vector2 vector17;
							vector17.X = (float)(num184 * 16);
							vector17.Y = (float)(num185 * 16);
							if (hitbox.X + hitbox.Width > vector17.X && hitbox.X < vector17.X + 16f && hitbox.Y + hitbox.Height > vector17.Y && hitbox.Y < vector17.Y + 16f)
							{
								if (Main.rand.NextBool(100) && Main.tile[num184, num185].HasUnactuatedTile)
								{
									WorldGen.KillTile(num184, num185, true, true, false);
								}
								if (Main.netMode != 1 && Main.tile[num184, num185].TileType == 2)
								{
									ushort arg_BFCA_0 = Main.tile[num184, num185 - 1].TileType;
								}
							}
						}
					}
				}
			}
		}

		public static Asset<Texture2D> GlowTexture;

		public override void Load()
		{
			GlowTexture = Request<Texture2D>(Texture + "_Mask");
		}

		public override void Unload()
		{
			GlowTexture = null;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (NPC.IsABestiaryIconDummy)
			{
				NPC.rotation = MathHelper.Pi;
				NPC.Center -= new Vector2(20, 0);
				segmentPositions[0] = NPC.Center + new Vector2(NPC.width / 2 - 2, 0).RotatedBy(NPC.rotation);
				const float rotAmoutPerSegment = 0.01f;
				for (int i = 1; i < segmentPositions.Length; i++)
				{
					segmentPositions[i] = segmentPositions[i - 1] - new Vector2(2, 0).RotatedBy(NPC.rotation + rotAmoutPerSegment * i);
				}
			}

			//draw body
			Texture2D bodyTexture = TextureAssets.Npc[Type].Value;
			for (int i = segmentPositions.Length - 1; i > 0; i--)
			{
				Vector2 drawPosition = (segmentPositions[i] + segmentPositions[i - 1]) / 2;

				int buffer = 16;
				if (!spriteBatch.GraphicsDevice.ScissorRectangle.Intersects(new Rectangle((int)(drawPosition - screenPos).X - buffer, (int)(drawPosition - screenPos).Y - buffer, buffer * 2, buffer * 2)))
				{
					continue;
				}

				float rotation = (segmentPositions[i - 1] - segmentPositions[i]).ToRotation();
				float scale = 1f;

				int segmentFramePoint = i < (segmentsHead + 1) ? 122 - 2 * (i - 1) //head
					: i >= segmentPositions.Length - segmentsTail ? 2 * (segmentPositions.Length - 1 - i) //tail
					: 62 - 2 * ((i - (segmentsHead + 1)) % segmentsPerHitbox); //body
				int segmentFrameOffset = bodyTexture.Width / 3 * (i < (segmentsHead + 1) ? segmentTypes[0] //head
					: i >= segmentPositions.Length - segmentsTail ? segmentTypes[segmentTypes.Length - 1] //tail
					: segmentTypes[((i - (segmentsHead + 1)) / segmentsPerHitbox) + 1]); //body

				segmentFramePoint += segmentFrameOffset;

				Tile posTile = Framing.GetTileSafely(drawPosition.ToTileCoordinates());
				if (posTile.HasTile && Main.tileBlockLight[posTile.TileType] && Lighting.GetColor((int)(drawPosition.X / 16), (int)(drawPosition.Y / 16)) == Color.Black && !Main.LocalPlayer.detectCreature)
				{
					continue;
				}

				Color color = Lighting.GetColor((int)(drawPosition.X / 16), (int)(drawPosition.Y / 16));
				spriteBatch.Draw(bodyTexture, drawPosition - screenPos, new Rectangle(segmentFramePoint, 0, 4, TextureAssets.Npc[Type].Height()), NPC.GetAlpha(NPC.GetNPCColorTintedByBuffs(color)), rotation, new Vector2(2, TextureAssets.Npc[Type].Height() / 2), new Vector2(scale, 1), SpriteEffects.None, 0f);
				spriteBatch.Draw(GlowTexture.Value, drawPosition - screenPos, new Rectangle(segmentFramePoint, 0, 4, TextureAssets.Npc[Type].Height()), NPC.GetAlpha(NPC.GetNPCColorTintedByBuffs(Color.White)), rotation, new Vector2(2, TextureAssets.Npc[Type].Height() / 2), new Vector2(scale, 1), SpriteEffects.None, 0f);
			}

			return false;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			//only spawns during the evil event
			if (spawnInfo.Player.InModBiome(GetInstance<Biomes.WorldEvilInvasion>()))
			{
				return Biomes.WorldEvilInvasion.GetSpawnChance(spawnInfo.Player.ZoneCorrupt) * 0.5f;
			}
			return 0f;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ItemID.CursedFlame, 1, 2, 4));
		}

		public override bool CheckDead()
		{
			ICollection<RectangleHitbox> collection = NPC.GetGlobalNPC<MultiHitboxNPC>().hitboxes.AllHitboxes();
			foreach (RectangleHitbox hitbox in collection)
			{
				Vector2 gorePos = hitbox.hitbox.TopLeft();
				if (hitbox.index == 0)
				{
					Gore.NewGore(NPC.GetSource_Death(), gorePos, Vector2.Zero, Mod.Find<ModGore>("RavenousCursedGore" + (1 + segmentTypes[hitbox.index] * 3)).Type);
				}
				else if (hitbox.index == collection.Count - 1)
				{

					Gore.NewGore(NPC.GetSource_Death(), gorePos, Vector2.Zero, Mod.Find<ModGore>("RavenousCursedGore" + (3 + segmentTypes[hitbox.index] * 3)).Type);
				}
				else
				{
					Gore.NewGore(NPC.GetSource_Death(), gorePos, Vector2.Zero, Mod.Find<ModGore>("RavenousCursedGore" + (2 + segmentTypes[hitbox.index] * 3)).Type);
				}
			}

			if (PolaritiesSystem.worldEvilInvasion)
			{
				//counts for 2 points
				PolaritiesSystem.worldEvilInvasionSize -= 2;
			}
			return true;
		}
	}

	public class RavenousCursedFlame : ModProjectile
	{
		public override string Texture => "Polarities/Textures/Pixel";

		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;

			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = true;
			Projectile.hide = true;
			Projectile.timeLeft = 30;
		}

		// The AI of the projectile
		public override void AI()
		{
			int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 75, Projectile.velocity.X, Projectile.velocity.Y, Scale: 2.5f);
			Main.dust[dust].noGravity = true;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			target.AddBuff(BuffID.CursedInferno, 60 * 8);
		}
	}

	public class RavenousCursedSpark : ModProjectile
	{
		public override string Texture => "Polarities/Textures/Pixel";

		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;

			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = true;
			Projectile.hide = true;
			Projectile.timeLeft = 120;
		}

		// The AI of the projectile
		public override void AI()
		{
			int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 75, Projectile.velocity.X * 0.8f, Projectile.velocity.Y * 0.8f, Scale: 1.5f);
			Main.dust[dust].noGravity = true;

			Projectile.velocity *= 0.95f;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			target.AddBuff(BuffID.CursedInferno, 60 * 8);
		}
	}
}

