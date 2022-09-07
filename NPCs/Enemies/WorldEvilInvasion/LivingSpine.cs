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
	public class LivingSpine : ModNPC
	{
		public override void SetStaticDefaults()
		{
			NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
			{
				SpecificallyImmuneTo = new int[] {
					BuffID.Confused
				}
			};
			NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);

			PolaritiesNPC.customNPCCapSlot[Type] = NPCCapSlotID.WorldEvilInvasionWorm;

            MultiHitboxNPC.MultiHitboxNPCTypes.Add(Type);
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
			NPC.width = 22;
			NPC.height = 22;
			NPC.defense = 10;
			NPC.damage = 45;
			NPC.knockBackResist = 0f;
			NPC.lifeMax = 3800;
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

			segmentPositions = new Vector2[numSegments * segmentsPerHitbox + 6];
		}

		const int numSegments = 10;
		const int segmentsPerHitbox = 11;
		const int segmentsHead = 11;
		const int segmentsTail = 11;
		const int hitboxSegmentOffset = 5;
		private Vector2[] segmentPositions;

		private bool useAttack;
		private bool tendrilActive;
		private Vector2 tendrilPosition;
		private int tendrilLatchTime = -10;

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
					if (tendrilLatchTime <= 0)
					{
						Vector2 targetPoint = player.Center + new Vector2(0, -240);
						Vector2 velocityGoal = 16 * (targetPoint - NPC.Center).SafeNormalize(Vector2.Zero);
						NPC.velocity += (velocityGoal - NPC.velocity) / 75;
					}

					if (!tendrilActive)
					{
						if (Main.netMode != 1)
						{
							useAttack = Main.rand.NextBool();
						}
						NPC.netUpdate = true;

						tendrilLatchTime = -10;
					}
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

				if (useAttack && PolaritiesSystem.worldEvilInvasion)
				{
					if (tendrilLatchTime < 0)
					{
						tendrilLatchTime++;

						if (tendrilLatchTime == 0 && (NPC.Center - player.Center).Length() > 360)
						{
							tendrilLatchTime = -1;
						}

						if (tendrilLatchTime == 0)
						{
							tendrilActive = true;
							tendrilPosition = NPC.Center;
							SoundEngine.PlaySound(SoundID.Item17, NPC.Center);
						}
					}
				}
				NPC.velocity.Y += 0.3f;
			}
			if (tendrilActive)
			{
				if (tendrilLatchTime == 0)
				{
					if ((player.Center - tendrilPosition).Length() < 16)
					{
						tendrilLatchTime++;
					}
					else
					{
						tendrilPosition += (player.Center - tendrilPosition).SafeNormalize(Vector2.Zero) * 16;
					}
				}

				if (tendrilLatchTime > 0)
				{
					tendrilPosition = player.Center;

					NPC.velocity += (tendrilPosition - NPC.Center).SafeNormalize(Vector2.Zero) * 0.75f;

					if (NPC.velocity.Length() > 12)
					{
						NPC.velocity.Normalize();
						NPC.velocity *= 12;
					}

					tendrilLatchTime++;
					if (tendrilLatchTime > 40)
					{
						useAttack = false;
						tendrilActive = false;
						tendrilLatchTime = -10;
					}
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

		public static Asset<Texture2D> ChainTexture;
		public static Asset<Texture2D> TendrilTexture;

		public override void Load()
        {
			ChainTexture = Request<Texture2D>(Texture + "_Chain");
			TendrilTexture = Request<Texture2D>(Texture + "_Tendril");
		}

        public override void Unload()
        {
			ChainTexture = null;
			TendrilTexture = null;
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

			if (tendrilActive)
			{
				Vector2 mainCenter = tendrilPosition;
				Vector2 center = NPC.Center;
				Vector2 distToNPC = mainCenter - center;
				float projRotation = distToNPC.ToRotation();
				float distance = distToNPC.Length();
				while (distance > 5f && !float.IsNaN(distance))
				{
					distToNPC.Normalize();
					distToNPC *= 6f;
					center += distToNPC;
					distToNPC = mainCenter - center;
					distance = distToNPC.Length();

					//Draw chain
					spriteBatch.Draw(ChainTexture.Value, center - screenPos,
						new Rectangle(0, 0, 6, 6), Lighting.GetColor((int)(center.X / 16), (int)(center.Y / 16)), projRotation,
						new Vector2(6 * 0.5f, 6 * 0.5f), 1f, SpriteEffects.None, 0f);
				}
				spriteBatch.Draw(TendrilTexture.Value, tendrilPosition - screenPos,
					new Rectangle(0, 0, 10, 10), Lighting.GetColor((int)(tendrilPosition.X / 16), (int)(tendrilPosition.Y / 16)), projRotation,
					new Vector2(10 * 0.5f, 10 * 0.5f), 1f, SpriteEffects.None, 0f);
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

				int segmentFramePoint = i < (segmentsHead + 1) ? 56 - 2 * (i - 1) //head
					: i >= segmentPositions.Length - segmentsTail ? 2 * (segmentPositions.Length - 1 - i) //tail
					: 34 - 2 * ((i - (segmentsHead + 1)) % segmentsPerHitbox); //body

				Tile posTile = Framing.GetTileSafely(drawPosition.ToTileCoordinates());
				if (posTile.HasTile && Main.tileBlockLight[posTile.TileType] && Lighting.GetColor((int)(drawPosition.X / 16), (int)(drawPosition.Y / 16)) == Color.Black && !Main.LocalPlayer.detectCreature)
				{
					continue;
				}

				Color color = Lighting.GetColor((int)(drawPosition.X / 16), (int)(drawPosition.Y / 16));
				spriteBatch.Draw(bodyTexture, drawPosition - screenPos, new Rectangle(segmentFramePoint, 0, 4, TextureAssets.Npc[Type].Height()), NPC.GetAlpha(NPC.GetNPCColorTintedByBuffs(color)), rotation, new Vector2(2, TextureAssets.Npc[Type].Height() / 2), new Vector2(scale, 1), SpriteEffects.None, 0f);
			}

			return false;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			//only spawns during the evil event
			if (spawnInfo.Player.InModBiome(GetInstance<Biomes.WorldEvilInvasion>()))
			{
				return Biomes.WorldEvilInvasion.GetSpawnChance(spawnInfo.Player.ZoneCrimson) * 0.5f;
			}
			return 0f;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ItemID.Vertebrae, 1, 2, 4));
		}

		public override bool CheckDead()
		{
			ICollection<RectangleHitbox> collection = NPC.GetGlobalNPC<MultiHitboxNPC>().hitboxes.AllHitboxes();
			foreach (RectangleHitbox hitbox in collection)
			{
				Vector2 gorePos = hitbox.hitbox.TopLeft();
				if (hitbox.index == 0)
				{
					Gore.NewGore(NPC.GetSource_Death(), gorePos, Vector2.Zero, Mod.Find<ModGore>("LivingSpineGore1").Type);
				}
				else if (hitbox.index == collection.Count - 1)
				{

					Gore.NewGore(NPC.GetSource_Death(), gorePos, Vector2.Zero, Mod.Find<ModGore>("LivingSpineGore3").Type);
				}
				else
				{
					Gore.NewGore(NPC.GetSource_Death(), gorePos, Vector2.Zero, Mod.Find<ModGore>("LivingSpineGore2").Type);
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
}

