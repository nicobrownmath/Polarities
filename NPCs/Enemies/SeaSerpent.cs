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
using Terraria.ModLoader.Utilities;
using MultiHitboxNPCLibrary;
using System.Collections.Generic;

namespace Polarities.NPCs.Enemies
{
	public class SeaSerpent : ModNPC
	{
		public override void SetStaticDefaults()
		{
			NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
			{
				SpecificallyImmuneTo = new int[] {
					BuffID.Confused,
					BuffID.OnFire
				}
			};
			NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);

            MultiHitboxNPC.MultiHitboxNPCTypes.Add(Type);
        }

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Ocean,
				//flavor text
				this.TranslatedBestiaryEntry()
			});
		}

		public override void SetDefaults()
		{
			NPC.aiStyle = -1;

			NPC.width = 28;
			NPC.height = 28;

			NPC.defense = 10;
			NPC.damage = 30;
			NPC.knockBackResist = 0f;
			NPC.lifeMax = 4000;
			NPC.noTileCollide = true;
			NPC.noGravity = true;
			NPC.lavaImmune = true;
			NPC.npcSlots = 0f;
			NPC.behindTiles = true;
			NPC.value = 1000;
			NPC.dontCountMe = true;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;

			Banner = Type;
			BannerItem = ItemType<SeaSerpentBanner>();

			numSegments = Main.rand.Next(20, 31);
			segmentPositions = new Vector2[numSegments * segmentsPerHitbox + (segmentsHead - segmentsPerHitbox) + (segmentsTail - segmentsPerHitbox)];
		}

		int numSegments;
		const int segmentsPerHitbox = 12;
		const int segmentsHead = 20;
		const int hitboxSegmentOffset = 7;
		const int segmentsTail = 13;
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
			float rotationFade = 12f;
			float rotationAmount = 0.3f;

			if (NPC.ai[0] == 0)
			{
				NPC.velocity = new Vector2(1, 0);
			}

			NPC.noGravity = Main.tile[(int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16)].LiquidAmount > 64 || Main.tile[(int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16)].HasTile;
			bool inWater = Main.tile[(int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16)].LiquidAmount > 64 && Main.tile[(int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16)].LiquidType == 0;

			if (NPC.noGravity)
			{
				Vector2 velocityGoal = Vector2.Zero;
				if (inWater)
				{
					if (player.wet)
					{
						//chase player
						Vector2 targetPoint = player.Center;
						velocityGoal = 12 * (targetPoint - NPC.Center).SafeNormalize(Vector2.Zero);
					}
					else
					{
						//swim passivey
						velocityGoal = NPC.velocity.SafeNormalize(Vector2.One) * 8;
					}
				}
				else
				{
					//seek water
					if (NPC.Center.X < Main.maxTilesX * 8)
					{
						//head up and to the right
						velocityGoal = new Vector2(-10, -10);
					}
					else
					{
						//head up and to the left
						velocityGoal = new Vector2(10, -10);
					}
				}
				NPC.velocity += (velocityGoal - NPC.velocity) / 120;
				NPC.velocity = NPC.velocity.RotatedBy(Math.Sin(NPC.ai[0]) * 0.075f);
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
			}

			NPC.ai[0] += 0.1f;

			if (NPC.velocity.Length() < 2 && NPC.velocity.Length() > 0.1f)
			{
				NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 2;
			}

			NPC.rotation = NPC.velocity.ToRotation();

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
		}

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
		{
			if (NPC.GetGlobalNPC<MultiHitboxNPC>().mostRecentHitbox.index == 0)
			{
				target.AddBuff(BuffID.Venom, 5 * 60);
			}
			else
			{
				damage /= 2;
			}
		}

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			if (NPC.GetGlobalNPC<MultiHitboxNPC>().mostRecentHitbox.index == 0)
			{
				target.AddBuff(BuffID.Venom, 5 * 60);
			}
			else
			{
				damage /= 2;
			}
		}

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			if (Main.hardMode)
			{
				return SpawnCondition.Ocean.Chance * 0.1f;
			}
			else
			{
				return 0f;
			}
		}

        public override bool CheckDead()
        {
			ICollection<RectangleHitbox> collection = NPC.GetGlobalNPC<MultiHitboxNPC>().hitboxes.AllHitboxes();
			foreach (RectangleHitbox hitbox in collection)
            {
				Vector2 gorePos = hitbox.hitbox.TopLeft();
				if (hitbox.index == 0)
				{
					Gore.NewGore(NPC.GetSource_Death(), gorePos, Vector2.Zero, Mod.Find<ModGore>("SeaSerpentGore1").Type);
				}
				else if (hitbox.index == collection.Count - 1)
				{

					Gore.NewGore(NPC.GetSource_Death(), gorePos, Vector2.Zero, Mod.Find<ModGore>("SeaSerpentGore3").Type);
				}
				else
				{
					Gore.NewGore(NPC.GetSource_Death(), gorePos, Vector2.Zero, Mod.Find<ModGore>("SeaSerpentGore2").Type);
				}
			}

			return true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
			npcLoot.Add(ItemDropRule.Common(ItemType<VenomGland>(), 1, 1, 2));
			npcLoot.Add(new MultiHitboxDropPerSegment(ItemType<SerpentScale>(), 4));
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
				SpriteEffects effects = SpriteEffects.None;
				float scale = 1f;

				if (Math.Abs(rotation) > MathHelper.PiOver2)
                {
					effects = SpriteEffects.FlipVertically;
				}

				int segmentFramePoint = i < (segmentsHead + 1) ? 96 - 2 * (i - 1) //head
				: i >= segmentPositions.Length - segmentsTail ? 2 * (segmentPositions.Length - 1 - i) //tail
				: 52 - 2 * ((i - (segmentsHead + 1)) % segmentsPerHitbox); //body

				Tile posTile = Framing.GetTileSafely(drawPosition.ToTileCoordinates());
				if (posTile.HasTile && Main.tileBlockLight[posTile.TileType] && Lighting.GetColor((int)(drawPosition.X / 16), (int)(drawPosition.Y / 16)) == Color.Black && !Main.LocalPlayer.detectCreature)
				{
					continue;
				}

				Color color = Lighting.GetColor((int)(drawPosition.X / 16), (int)(drawPosition.Y / 16));
				if (NPC.IsABestiaryIconDummy) color = Color.White;
				spriteBatch.Draw(bodyTexture, drawPosition - screenPos, new Rectangle(segmentFramePoint, 0, 4, TextureAssets.Npc[Type].Height()), NPC.GetAlpha(NPC.GetNPCColorTintedByBuffs(color)), rotation, new Vector2(2, TextureAssets.Npc[Type].Height() / 2), new Vector2(scale, 1), effects, 0f);
			}

			return false;
		}
	}
}

