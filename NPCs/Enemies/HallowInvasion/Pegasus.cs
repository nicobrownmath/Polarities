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
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;
using Polarities.Items.Mounts;

namespace Polarities.NPCs.Enemies.HallowInvasion
{
	public class Pegasus : ModNPC
	{
		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 6;

			NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
			{
				SpecificallyImmuneTo = new int[] {
					BuffID.Confused,
					BuffID.OnFire
				}
			};
			NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);

			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
			{
				PortraitPositionXOverride = 0,
				Position = new Vector2(16, 0),
				SpriteDirection = 1
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifiers);

			PolaritiesNPC.customNPCCapSlot[Type] = NPCCapSlotID.HallowInvasion;
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
			NPC.width = 46;
			NPC.height = 42;

			NPC.defense = 20;
			NPC.damage = 60;
			NPC.lifeMax = 800;
			NPC.knockBackResist = 0f;
			NPC.npcSlots = 1f;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.HitSound = SoundID.NPCHit12;
			NPC.DeathSound = SoundID.NPCDeath18;
			NPC.value = Item.buyPrice(silver: 25);

			Music = GetInstance<Biomes.HallowInvasion>().Music;
			SceneEffectPriority = SceneEffectPriority.Event;

			Banner = Type;
			BannerItem = ItemType<PegasusBanner>();

			SpawnModBiomes = new int[1] { GetInstance<Biomes.HallowInvasion>().Type };
		}

		public override void AI()
		{
			NPC.TargetClosest(false);
			Player player = Main.player[NPC.target];

			if (!PolaritiesSystem.hallowInvasion)
			{
				//flee if not in the invasion
				NPC.ai[0] = -1;
			}

			//sun knight riding it
			if (NPC.ai[2] > 0)
            {
				NPC sunKnight = Main.npc[(int)NPC.ai[2] - 1];

				if (!sunKnight.active)
                {
					NPC.ai[2] = 0;
				}
            }

			//movement ai
			switch (NPC.ai[0])
			{
				case 0:
					//horizontal alignment
					Vector2 goalVelocity = (player.Center - NPC.Center) / 30f;
					goalVelocity.X = (player.position.X - player.oldPosition.X);
					NPC.velocity += (goalVelocity - NPC.velocity) / 60f;

					NPC.ai[1]++;
					if (NPC.ai[1] >= 30 && Math.Abs(NPC.Center.Y + NPC.velocity.Y * 50 - player.Center.Y) < 8)
					{
						NPC.ai[0] = 1;
						NPC.ai[1] = 0;
					}
					break;
				case 1:
					//yeet
					if (NPC.ai[1] == 0)
					{
						NPC.direction = (player.Center.X > NPC.Center.X) ? 1 : -1;
					}
					NPC.velocity.X += NPC.direction * 0.2f;
					NPC.velocity.Y *= 0.98f;

					NPC.ai[1]++;
					if (NPC.ai[1] >= 60 && ((player.Center.X > NPC.Center.X) ^ NPC.direction == 1))
					{
						NPC.ai[0] = 0;
						NPC.ai[1] = 0;
					}
					break;
				case -1:
					//flee
					NPC.direction = (player.Center.X > NPC.Center.X) ? -1 : 1;
					NPC.velocity.X += NPC.direction * 0.2f;
					NPC.velocity.Y *= 0.98f;
					break;
			}

			NPC.rotation = NPC.velocity.X * 0.01f;

			NPC.spriteDirection = -NPC.direction;
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter++;
			if (NPC.frameCounter == 5)
			{
				NPC.frameCounter = 0;
				NPC.frame.Y += frameHeight;
				if (NPC.frame.Y == frameHeight * 6)
				{
					NPC.frame.Y = frameHeight;
				}
			}
		}

        public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.ByCondition(new PegasusMountDropCondition(), ItemType<PegasusMountItem>(), 2));
		}

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
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

			for (int i = 1; i <= 5; i++)
				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("PegasusGore" + i).Type);

			return true;
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
				return ModUtils.Lerp(Biomes.HallowInvasion.GetSpawnChance(0), Biomes.HallowInvasion.GetSpawnChance(1), 0.34f);
			}
			return 0f;
		}
	}

	class PegasusMountDropCondition : IItemDropRuleCondition
	{
		public bool CanDrop(DropAttemptInfo info)
		{
			return info.npc.ai[2] > 0;
		}

		public bool CanShowItemDropInUI()
		{
			return true;
		}

		public string GetConditionDescription()
		{
			return Language.GetTextValue("Mods.Polarities.DropConditions.PegasusMount");
		}
	}
}