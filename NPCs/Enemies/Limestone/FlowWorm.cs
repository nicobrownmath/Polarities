using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Polarities.Projectiles;
using Polarities.Buffs;
using Polarities.Items;
using Polarities.Items.Placeable;
using Polarities.Tiles;
using Polarities.Items.Weapons;
using Polarities.Items.Armor;
using Polarities.Items.Placeable.Banners;
using Terraria.GameContent.Bestiary;
using Polarities.Biomes;
using Polarities.Items.Placeable.Blocks;
using Terraria.Audio;
using Polarities.Items.Placeable.Walls;
using Terraria.GameContent.ItemDropRules;
using Polarities.Items.Consumables;
using Terraria.GameContent;
using ReLogic.Content;
using Polarities.Items.Accessories;
using Terraria.DataStructures;
using Polarities.Items.Materials;

namespace Polarities.NPCs.Enemies.Limestone
{
	public class FlowWorm : ModNPC
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Flow Worm");

			NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
			{
				SpecificallyImmuneTo = new int[] {
					BuffID.Confused
				}
			};
			NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);
		}

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				//flavor text
				this.TranslatedBestiaryEntry()
			});
		}

		public override void SetDefaults()
		{
			NPC.aiStyle = -1;

			NPC.width = 20;
			NPC.height = 20;

			NPC.defense = 0;
			NPC.damage = 24;
			NPC.knockBackResist = 0f;
			NPC.lifeMax = 90;
			NPC.noTileCollide = true;
			NPC.noGravity = true;
			NPC.lavaImmune = true;
			NPC.npcSlots = 1f;
			NPC.behindTiles = true;
			NPC.value = Item.buyPrice(silver: 1);
			NPC.HitSound = SoundID.NPCHit54;
			NPC.DeathSound = SoundID.NPCDeath52;

			Banner = Type;
			BannerItem = ItemType<FlowWormBanner>();

			NPC.GetGlobalNPC<MultiHitboxNPC>().useMultipleHitboxes = true;
			NPC.GetGlobalNPC<MultiHitboxNPC>().hitboxes = new Rectangle[numSegments];

			SpawnModBiomes = new int[1] { GetInstance<LimestoneCave>().Type };

			segmentPositions = new Vector2[numSegments * segmentsPerHitbox + 6];
		}

		const int numSegments = 10;
		const int segmentsPerHitbox = 8;
		const int segmentsHead = 16;
		const int segmentsTail = 14;
		const int hitboxSegmentOffset = 5;
		private Vector2[] segmentPositions;

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
			float rotationFade = 9f;
			float rotationAmount = 0.1f;

			//Do AI
            NPC.noGravity = Main.tile[(int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16)].LiquidAmount > 64 || (Main.tile[(int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16)].HasTile && Main.tileSolid[Main.tile[(int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16)].TileType]);

			if (NPC.noGravity)
			{
				NPC.ai[0] = 0;

				Vector2 targetPoint = player.Center + new Vector2(0, -240);
				Vector2 velocityGoal = 16 * (targetPoint - NPC.Center).SafeNormalize(Vector2.Zero);
				NPC.velocity += (velocityGoal - NPC.velocity) / 60;

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
					SoundEngine.PlaySound(SoundID.Roar, NPC.position, 1);
				}
			}
			else
			{
				Lighting.AddLight(NPC.Center, 123f / 255, 233f / 255, 60f / 255);

				NPC.direction = Main.rand.NextBool() ? -1 : 1;
				if (NPC.velocity.X == 0)
				{
					NPC.velocity.X = NPC.direction * 0.01f;
				}

				if (NPC.ai[0] < 60)
				{
					float targetX = player.Center.X;
					float velocityGoalX = (targetX - NPC.Center.X) / 20;
					NPC.velocity.X += (velocityGoalX - NPC.velocity.X) / 20;

					if (Math.Abs(NPC.velocity.X) > 5)
					{
						NPC.velocity.X = NPC.velocity.X > 0 ? 5 : -5;
					}

					NPC.ai[0]++;
				}
				else
				{
					NPC.velocity.X *= 0.95f;
					NPC.velocity.Y += 0.3f;
				}
			}

			float minSpeed = NPC.noGravity ? 5 : 0;
			float maxSpeed = NPC.noGravity ? 12 : 12;
			if (NPC.velocity.Length() > maxSpeed)
			{
				NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * maxSpeed;
			}
			if (NPC.velocity.Length() < minSpeed)
			{
				NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * minSpeed;
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
			for (int h = 0; h < numSegments; h++)
			{
				Vector2 spot = segmentPositions[h * segmentsPerHitbox + hitboxSegmentOffset];
				NPC.GetGlobalNPC<MultiHitboxNPC>().hitboxes[h] = new Rectangle((int)spot.X - NPC.width / 2, (int)spot.Y - NPC.height / 2, NPC.width, NPC.height);
			}

			//dig effect adapted from vanilla
			foreach (Rectangle hitbox in NPC.GetGlobalNPC<MultiHitboxNPC>().hitboxes)
			{
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

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (NPC.IsABestiaryIconDummy)
			{
				NPC.rotation = MathHelper.Pi;
				NPC.Center -= new Vector2(20, 0);
				segmentPositions[0] = NPC.Center + new Vector2(NPC.width / 2 - 2, 0).RotatedBy(NPC.rotation);
				const float rotAmoutPerSegment = 0.05f;
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
				float rotation = (segmentPositions[i - 1] - segmentPositions[i]).ToRotation();
				float scale = 1f;

				int segmentFramePoint = i < (segmentsHead + 1) ? 128 - 2 * (i - 1) //head
					: i >= segmentPositions.Length - segmentsTail ? 2 * (segmentPositions.Length - 1 - i) //tail
					: 68 - 2 * ((i - (segmentsHead + 1)) % segmentsPerHitbox); //body

				Tile posTile = Framing.GetTileSafely(drawPosition.ToTileCoordinates());
				if (posTile.HasTile && Main.tileBlockLight[posTile.TileType] && Lighting.GetColor((int)(drawPosition.X / 16), (int)(drawPosition.Y / 16)) == Color.Black && !Main.LocalPlayer.detectCreature)
                {
					continue;
                }

				Color color = Color.White; //Lighting.GetColor((int)(drawPosition.X / 16), (int)(drawPosition.Y / 16));
				spriteBatch.Draw(bodyTexture, drawPosition - screenPos, new Rectangle(segmentFramePoint, 0, 4, TextureAssets.Npc[Type].Height()), NPC.GetAlpha(NPC.GetNPCColorTintedByBuffs(color)), rotation, new Vector2(2, TextureAssets.Npc[Type].Height() / 2), new Vector2(scale, 1), SpriteEffects.None, 0f);
			}

			return false;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			if (spawnInfo.PlayerSafe) return 0f;

			Tile playerTile = Main.tile[(int)(spawnInfo.Player.Center.X / 16), (int)((spawnInfo.Player.Center.Y + 1 + spawnInfo.Player.height / 2) / 16)];
			if (spawnInfo.Player.InModBiome(GetInstance<LimestoneCave>()) && (spawnInfo.SpawnTileType == TileType<LimestoneTile>() || playerTile.TileType == TileType<LimestoneTile>()))
			{
				return 1f;
			}
			return 0f;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ItemType<AlkalineFluid>(), 2, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ItemType<KeyLimePie>(), 50));
		}

		public override bool CheckDead()
		{
			for (int i = 0; i < numSegments; i++)
			{
				Rectangle hitbox = NPC.GetGlobalNPC<MultiHitboxNPC>().hitboxes[i];
				for (int j = 0; j < 3; j++)
				{
					Main.dust[Dust.NewDust(hitbox.TopLeft(), hitbox.Width, hitbox.Height, 74, Scale: 1.75f)].noGravity = true;
				}
			}
			return true;
		}
	}
}

