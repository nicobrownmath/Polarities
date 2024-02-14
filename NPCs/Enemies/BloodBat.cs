using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Items.Placeable.Banners;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Bestiary;

namespace Polarities.NPCs.Enemies
{
	public class BloodBat : ModNPC
	{
		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 6;

			var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
			{
				//positions flying enemy in the center of the bestiary portrait
				PortraitPositionYOverride = -20f,
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                //spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon,
				//flavor text
				this.TranslatedBestiaryEntry()
			});
		}

		public override void SetDefaults()
		{
			NPC.width = 32;
			NPC.height = 32;
			DrawOffsetY = 2;

			NPC.damage = 20;
			NPC.defense = 6;
			NPC.lifeMax = 60;
			NPC.HitSound = SoundID.NPCHit18;
			NPC.DeathSound = SoundID.NPCDeath21;
			NPC.value = 150f;
			NPC.knockBackResist = 0.5f;
			NPC.npcSlots = 1f;
			NPC.aiStyle = 14;
			NPC.stairFall = true;

			Banner = NPC.type;
			BannerItem = ItemType<BloodBatBanner>();
		}

		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)/* tModPorter Note: bossLifeScale -> balance (bossAdjustment is different, see the docs for details) */
		{
			NPC.lifeMax = 120;
			NPC.damage = 30;
		}

		public override int SpawnNPC(int tileX, int tileY)
		{
			EntitySource_SpawnNPC source = new EntitySource_SpawnNPC();
			NPC.NewNPC(source, tileX * 16 + 8, tileY * 16 + 8, NPC.type);
			NPC.NewNPC(source, tileX * 16 + 8, tileY * 16 - 8, NPC.type);
			NPC.NewNPC(source, tileX * 16, tileY * 16, NPC.type);

			return NPC.NewNPC(source, tileX * 16 + 8, tileY * 16, NPC.type);
		}

		public override bool PreAI()
		{
			//vanilla bat AI
			NPC.noGravity = true;
			if (NPC.collideX)
			{
				NPC.velocity.X = NPC.oldVelocity.X * -0.5f;
				if (NPC.direction == -1 && NPC.velocity.X > 0f && NPC.velocity.X < 2f)
				{
					NPC.velocity.X = 2f;
				}
				if (NPC.direction == 1 && NPC.velocity.X < 0f && NPC.velocity.X > -2f)
				{
					NPC.velocity.X = -2f;
				}
			}
			if (NPC.collideY)
			{
				NPC.velocity.Y = NPC.oldVelocity.Y * -0.5f;
				if (NPC.velocity.Y > 0f && NPC.velocity.Y < 1f)
				{
					NPC.velocity.Y = 1f;
				}
				if (NPC.velocity.Y < 0f && NPC.velocity.Y > -1f)
				{
					NPC.velocity.Y = -1f;
				}
			}
			NPC.TargetClosest();
			if (Main.dayTime)
			{
				NPC.direction *= -1;
			}
			if (NPC.direction == -1 && NPC.velocity.X > -4f)
			{
				NPC.velocity.X = NPC.velocity.X - 0.1f;
				if (NPC.velocity.X > 4f)
				{
					NPC.velocity.X = NPC.velocity.X - 0.1f;
				}
				else if (NPC.velocity.X > 0f)
				{
					NPC.velocity.X = NPC.velocity.X + 0.05f;
				}
				if (NPC.velocity.X < -4f)
				{
					NPC.velocity.X = -4f;
				}
			}
			else if (NPC.direction == 1 && NPC.velocity.X < 4f)
			{
				NPC.velocity.X = NPC.velocity.X + 0.1f;
				if (NPC.velocity.X < -4f)
				{
					NPC.velocity.X = NPC.velocity.X + 0.1f;
				}
				else if (NPC.velocity.X < 0f)
				{
					NPC.velocity.X = NPC.velocity.X - 0.05f;
				}
				if (NPC.velocity.X > 4f)
				{
					NPC.velocity.X = 4f;
				}
			}
			if (NPC.directionY == -1 && (double)NPC.velocity.Y > -1.5)
			{
				NPC.velocity.Y = NPC.velocity.Y - 0.04f;
				if ((double)NPC.velocity.Y > 1.5)
				{
					NPC.velocity.Y = NPC.velocity.Y - 0.05f;
				}
				else if (NPC.velocity.Y > 0f)
				{
					NPC.velocity.Y = NPC.velocity.Y + 0.03f;
				}
				if ((double)NPC.velocity.Y < -1.5)
				{
					NPC.velocity.Y = -1.5f;
				}
			}
			else if (NPC.directionY == 1 && (double)NPC.velocity.Y < 1.5)
			{
				NPC.velocity.Y = NPC.velocity.Y + 0.04f;
				if ((double)NPC.velocity.Y < -1.5)
				{
					NPC.velocity.Y = NPC.velocity.Y + 0.05f;
				}
				else if (NPC.velocity.Y < 0f)
				{
					NPC.velocity.Y = NPC.velocity.Y - 0.03f;
				}
				if ((double)NPC.velocity.Y > 1.5)
				{
					NPC.velocity.Y = 1.5f;
				}
			}
			if (NPC.wet)
			{
				if (NPC.velocity.Y > 0f)
				{
					NPC.velocity.Y = NPC.velocity.Y * 0.95f;
				}
				NPC.velocity.Y = NPC.velocity.Y - 0.5f;
				if (NPC.velocity.Y < -4f)
				{
					NPC.velocity.Y = -4f;
				}
			}
			if (NPC.direction == -1 && NPC.velocity.X > -4f)
			{
				NPC.velocity.X = NPC.velocity.X - 0.1f;
				if (NPC.velocity.X > 4f)
				{
					NPC.velocity.X = NPC.velocity.X - 0.1f;
				}
				else if (NPC.velocity.X > 0f)
				{
					NPC.velocity.X = NPC.velocity.X + 0.05f;
				}
				if (NPC.velocity.X < -4f)
				{
					NPC.velocity.X = -4f;
				}
			}
			else if (NPC.direction == 1 && NPC.velocity.X < 4f)
			{
				NPC.velocity.X = NPC.velocity.X + 0.1f;
				if (NPC.velocity.X < -4f)
				{
					NPC.velocity.X = NPC.velocity.X + 0.1f;
				}
				else if (NPC.velocity.X < 0f)
				{
					NPC.velocity.X = NPC.velocity.X - 0.05f;
				}
				if (NPC.velocity.X > 4f)
				{
					NPC.velocity.X = 4f;
				}
			}
			if (NPC.directionY == -1 && (double)NPC.velocity.Y > -1.5)
			{
				NPC.velocity.Y = NPC.velocity.Y - 0.04f;
				if ((double)NPC.velocity.Y > 1.5)
				{
					NPC.velocity.Y = NPC.velocity.Y - 0.05f;
				}
				else if (NPC.velocity.Y > 0f)
				{
					NPC.velocity.Y = NPC.velocity.Y + 0.03f;
				}
				if ((double)NPC.velocity.Y < -1.5)
				{
					NPC.velocity.Y = -1.5f;
				}
			}
			else if (NPC.directionY == 1 && (double)NPC.velocity.Y < 1.5)
			{
				NPC.velocity.Y = NPC.velocity.Y + 0.04f;
				if ((double)NPC.velocity.Y < -1.5)
				{
					NPC.velocity.Y = NPC.velocity.Y + 0.05f;
				}
				else if (NPC.velocity.Y < 0f)
				{
					NPC.velocity.Y = NPC.velocity.Y - 0.03f;
				}
				if ((double)NPC.velocity.Y > 1.5)
				{
					NPC.velocity.Y = 1.5f;
				}
			}
			NPC.ai[1] += 1f;
			if (NPC.ai[1] > 200f)
			{
				if (!Main.player[NPC.target].wet && Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height))
				{
					NPC.ai[1] = 0f;
				}
				float num205 = 0.2f;
				float num206 = 0.1f;
				float num207 = 4f;
				float num208 = 1.5f;
				if (NPC.ai[1] > 1000f)
				{
					NPC.ai[1] = 0f;
				}
				NPC.ai[2] += 1f;
				if (NPC.ai[2] > 0f)
				{
					if (NPC.velocity.Y < num208)
					{
						NPC.velocity.Y = NPC.velocity.Y + num206;
					}
				}
				else if (NPC.velocity.Y > 0f - num208)
				{
					NPC.velocity.Y = NPC.velocity.Y - num206;
				}
				if (NPC.ai[2] < -150f || NPC.ai[2] > 150f)
				{
					if (NPC.velocity.X < num207)
					{
						NPC.velocity.X = NPC.velocity.X + num205;
					}
				}
				else if (NPC.velocity.X > 0f - num207)
				{
					NPC.velocity.X = NPC.velocity.X - num205;
				}
				if (NPC.ai[2] > 300f)
				{
					NPC.ai[2] = -300f;
				}
			}
			else
			{
				Boids();
			}

			return false;
		}

		public override void FindFrame(int frameHeight)
		{
			if (NPC.velocity.X > 0f)
			{
				NPC.spriteDirection = 1;
			}
			if (NPC.velocity.X < 0f)
			{
				NPC.spriteDirection = -1;
			}
			NPC.rotation = NPC.velocity.X * 0.1f;

			NPC.frameCounter++;
			if (NPC.frameCounter == 6)
			{
				NPC.frameCounter = 0;
				NPC.frame.Y = (NPC.frame.Y + frameHeight) % (Main.npcFrameCount[NPC.type] * frameHeight);
			}
		}

		private void Boids()
		{
			//boids
			Vector2 separation = Vector2.Zero;
			Vector2 alignment = Vector2.Zero;
			Vector2 cohesion = Vector2.Zero;
			int count = 0;

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC otherNPC = Main.npc[i];

				if (i != NPC.whoAmI && otherNPC.type == NPC.type && otherNPC.active && (NPC.Center - otherNPC.Center).Length() < 128)
				{
					count++;

					//separation component
					separation += 64f * (NPC.Center - otherNPC.Center).SafeNormalize(Vector2.Zero) / (NPC.Center - otherNPC.Center).Length();

					//alignment component
					alignment += 1 / 6f * (otherNPC.velocity - NPC.velocity);

					//cohesion component
					cohesion += 1 / 32f * (otherNPC.Center - NPC.Center);
				}

			}

			if (count > 0)
			{
				alignment /= count;
				cohesion /= count;
			}

			Vector2 goalVelocity = NPC.velocity + separation + alignment + cohesion;
			if (goalVelocity.Length() > 5)
			{
				goalVelocity.Normalize();
				goalVelocity *= 5;
			}
			NPC.velocity += (goalVelocity - NPC.velocity) / 30;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			return (Terraria.ModLoader.Utilities.SpawnCondition.OverworldNightMonster.Chance > 0 && Main.bloodMoon) ? 0.05f : 0f;
		}

		public override bool CheckDead()
		{
			for (int i = 1; i <= 3; i++)
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("BloodBatGore" + i).Type);
			return true;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.NormalvsExpert(ItemID.SharkToothNecklace, 150, 75));
			npcLoot.Add(ItemDropRule.NormalvsExpert(ItemID.MoneyTrough, 200, 100));
		}
	}
}