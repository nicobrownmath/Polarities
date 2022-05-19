using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using static Terraria.ModLoader.ModContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Creative;
using Terraria.Localization;
using Terraria.ModLoader;
using MonoMod.RuntimeDetour.HookGen;
using Terraria.DataStructures;
using Terraria.Utilities;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.ItemDropRules;
using Polarities.NPCs;
using Terraria.ID;
using Terraria.GameContent.Personalities;
using Terraria.GameContent.Events;
using Terraria.GameContent.UI;
using Polarities.Items.Books;
using Polarities.Items.Accessories;
using System.Collections.Generic;

namespace Polarities.NPCs.TownNPCs
{
	[AutoloadHead]
	public class Ghostwriter : ModNPC
	{
        public override void Load()
        {
            On.Terraria.GameContent.Personalities.AllPersonalitiesModifier.ModifyShopPrice_Relationships += AllPersonalitiesModifier_ModifyShopPrice_Relationships;

            IL.Terraria.NPC.checkDead += NPC_checkDead;
        }

		//makes them leave rather than die
		//TODO: Dust poof
        private void NPC_checkDead(ILContext il)
        {
			ILCursor c = new ILCursor(il);

			ILLabel label = null;

			if (!c.TryGotoNext(MoveType.After,
				i => i.MatchLdarg(0),
				i => i.MatchLdfld(typeof(NPC).GetField("type", BindingFlags.Public | BindingFlags.Instance)),
				i => i.MatchLdcI4(369),
				i => i.MatchBeq(out label)
				))
			{
				GetInstance<Polarities>().Logger.Debug("Failed to find patch location");
				return;
			}

			c.Emit(OpCodes.Ldarg, 0);
			c.Emit(OpCodes.Ldfld, typeof(NPC).GetField("type", BindingFlags.Public | BindingFlags.Instance));
			c.EmitDelegate<Func<int>>(() => { return NPCType<Ghostwriter>(); });
			c.Emit(OpCodes.Beq, label);
        }

		//makes them not like the princess
        private void AllPersonalitiesModifier_ModifyShopPrice_Relationships(On.Terraria.GameContent.Personalities.AllPersonalitiesModifier.orig_ModifyShopPrice_Relationships orig, HelperInfo info, Terraria.GameContent.ShopHelper shopHelperInstance)
        {
			if (info.npc.type == NPCType<Ghostwriter>())
			{
				if (info.nearbyNPCsByType[663])
				{
					shopHelperInstance.AddHappinessReportText("MehAboutPrincess", new { NPCName = NPC.GetFullnameByID(NPCID.Princess) });
				}
				return;
            }
            orig(info, shopHelperInstance);
        }

        public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 25;

			NPCID.Sets.ExtraFramesCount[Type] = 9;
			NPCID.Sets.AttackFrameCount[Type] = 4;
			NPCID.Sets.DangerDetectRange[Type] = 700;
			NPCID.Sets.AttackType[Type] = -1; //No attack
			NPCID.Sets.AttackTime[Type] = 0; //If we do somehow attack it takes no time
			NPCID.Sets.AttackAverageChance[Type] = int.MaxValue; //Just don't
			NPCID.Sets.HatOffsetY[Type] = 4; //Party hat Y offset

			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
			{
				Velocity = 1f,
				Direction = -1
			};

			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

			NPC.Happiness
				.SetBiomeAffection<DesertBiome>(AffectionLevel.Like)
				.SetBiomeAffection<HallowBiome>(AffectionLevel.Dislike)
				.SetNPCAffection(NPCID.Mechanic, AffectionLevel.Love)
				.SetNPCAffection(NPCID.Clothier, AffectionLevel.Like)
				.SetNPCAffection(NPCID.Guide, AffectionLevel.Like)
				.SetNPCAffection(NPCID.Angler, AffectionLevel.Dislike)
				.SetNPCAffection(NPCID.Dryad, AffectionLevel.Hate)
			;
		}

		public override void SetDefaults()
		{
			NPC.townNPC = true;
			NPC.friendly = true;
			NPC.width = 18;
			NPC.height = 40;
			NPC.aiStyle = 7;
			NPC.damage = 0;
			NPC.defense = 0;
			NPC.lifeMax = 250;
			NPC.HitSound = SoundID.NPCHit54;
			NPC.DeathSound = SoundID.NPCDeath52;
			NPC.knockBackResist = 0.5f;
			NPC.alpha = 64;

			NPC.noTileCollide = true;
			NPC.noGravity = true;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				//TODO: Like either books or graveyard instead of this
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
				this.TranslatedBestiaryEntry()
			});
		}

		public static bool SpawnCondition()
        {
			return NPC.downedSlimeKing || NPC.downedBoss1 || NPC.downedBoss2 || NPC.downedBoss3 || NPC.downedQueenBee || NPC.downedDeerclops || Main.hardMode || PolaritiesSystem.downedStormCloudfish || PolaritiesSystem.downedStarConstruct || PolaritiesSystem.downedGigabat || PolaritiesSystem.downedRiftDenizen;
		}

        public override bool CanTownNPCSpawn(int numTownNPCs, int money)
        {
			return SpawnCondition();
		}

        public override List<string> SetNPCNameList()
        {
			return new List<string>
			{
				"Anne",
				"Mary",
				"Shelley",
				"Jane",
				"Harper",
				"Terry",
				"Ursula"
			};
        }

        public override string GetChat()
        {
			string baseDialogueString = "Mods.Polarities.TownNPCDialogue.Ghostwriter.";

			if (NPC.homeless)
			{
				return Language.GetTextValue(baseDialogueString + "NoHome");
			}

			WeightedRandom<(string, object)> chat = new WeightedRandom<(string, object)>();

			chat.Add((baseDialogueString + "NoHome", null));
			chat.Add((baseDialogueString + "Generic0", null));
			chat.Add((baseDialogueString + "Generic1", null));
			chat.Add((baseDialogueString + "Generic2", null));
			if (NPC.FindFirstNPC(NPCID.Angler) >= 0)
			{
				chat.Add((baseDialogueString + "Angler", new { NPCName = Main.npc[NPC.FindFirstNPC(NPCID.Angler)].GivenName }));
			}
			if (NPC.FindFirstNPC(NPCID.Stylist) >= 0)
			{
				chat.Add((baseDialogueString + "Stylist", new { NPCName = Main.npc[NPC.FindFirstNPC(NPCID.Stylist)].GivenName }));
			}
			if (NPC.FindFirstNPC(NPCID.ArmsDealer) >= 0)
			{
				chat.Add((baseDialogueString + "ArmsDealer", new { NPCName = Main.npc[NPC.FindFirstNPC(NPCID.ArmsDealer)].GivenName }));
			}
			if (NPC.FindFirstNPC(NPCID.DD2Bartender) >= 0)
			{
				chat.Add((baseDialogueString + "DD2Bartender", new { NPCName = Main.npc[NPC.FindFirstNPC(NPCID.DD2Bartender)].GivenName }));
			}
			if (NPC.FindFirstNPC(NPCID.Clothier) >= 0)
			{
				chat.Add((baseDialogueString + "Clothier", new { NPCName = Main.npc[NPC.FindFirstNPC(NPCID.Clothier)].GivenName }));
			}
			if (NPC.FindFirstNPC(NPCID.Mechanic) >= 0)
			{
				chat.Add((baseDialogueString + "Mechanic", new { NPCName = Main.npc[NPC.FindFirstNPC(NPCID.Mechanic)].GivenName }));
			}
			if (NPC.FindFirstNPC(NPCID.SantaClaus) >= 0)
			{
				chat.Add((baseDialogueString + "SantaClaus", new { NPCName = Main.npc[NPC.FindFirstNPC(NPCID.SantaClaus)].GivenName }));
			}
			if (NPC.FindFirstNPC(NPCID.Wizard) >= 0)
			{
				chat.Add((baseDialogueString + "Wizard", new { NPCName = Main.npc[NPC.FindFirstNPC(NPCID.Wizard)].GivenName }));
			}
			if (NPC.FindFirstNPC(NPCID.Guide) >= 0 && Main.hardMode)
			{
				chat.Add((baseDialogueString + "GuideHardmode", new { NPCName = Main.npc[NPC.FindFirstNPC(NPCID.Guide)].GivenName }));
			}
			if (NPC.FindFirstNPC(NPCID.Dryad) >= 0 && Main.hardMode)
			{
				chat.Add((baseDialogueString + "DryadHardmode", new { NPCName = Main.npc[NPC.FindFirstNPC(NPCID.Dryad)].GivenName }));
			}
			if (NPC.FindFirstNPC(NPCID.Princess) >= 0)
			{
				chat.Add((baseDialogueString + "Princess", new { NPCName = Main.npc[NPC.FindFirstNPC(NPCID.Princess)].GivenName }));
			}
			if (Main.halloween)
			{
				chat.Add((baseDialogueString + "Halloween", null));
			}
			if (Main.xMas)
			{
				chat.Add((baseDialogueString + "XMas", null));
			}
			if (NPC.downedBoss1)
			{
				chat.Add((baseDialogueString + "EyeOfCthulhu", null));
			}
			if (NPC.downedBoss3)
			{
				chat.Add((baseDialogueString + "Skeletron", null));
			}
			if (PolaritiesSystem.downedRiftDenizen)
			{
				chat.Add((baseDialogueString + "RiftDenizen", null));
			}
			if (NPC.downedMechBoss2)
			{
				chat.Add((baseDialogueString + "Twins", null));
			}
			if (NPC.downedMechBoss3)
			{
				chat.Add((baseDialogueString + "Destroyer", null));
			}
			if (Main.bloodMoon)
			{
				chat.Add((baseDialogueString + "BloodMoon", null));
			}
			if (Main.pumpkinMoon)
			{
				chat.Add((baseDialogueString + "PumpkinMoon", null));
			}
			if (Main.snowMoon)
			{
				chat.Add((baseDialogueString + "FrostMoon", null));
			}
			if (Main.LocalPlayer.ZoneGraveyard)
			{
				chat.Add((baseDialogueString + "Graveyard", null));
			}

			(string, object) output = chat;
			return Language.GetTextValueWith(output.Item1, output.Item2);
		}

		public override void SetChatButtons(ref string button, ref string button2)
		{
			button = Language.GetTextValue("LegacyInterface.28");
		}

		public override void OnChatButtonClicked(bool firstButton, ref bool shop)
		{
			if (firstButton)
			{
				shop = true;
			}
		}

        public override bool CanGoToStatue(bool toKingStatue)
        {
			return !toKingStatue;
		}

		public override bool? CanBeHitByProjectile(Projectile projectile)
		{
			return false;
		}

		//TODO: Fill this out
		public override void SetupShop(Chest shop, ref int nextSlot)
		{
			if (Main.LocalPlayer.ZoneGraveyard)
			{
				shop.item[nextSlot].SetDefaults(ItemType<StrangeObituary>());
				nextSlot++;
			}
			shop.item[nextSlot].SetDefaults(ItemID.Book);
			nextSlot++;
			if (Main.hardMode)
			{
				shop.item[nextSlot].SetDefaults(ItemType<ReadingGlasses>());
				nextSlot++;
			}
			if (NPC.downedSlimeKing)
			{
				shop.item[nextSlot].SetDefaults(ItemType<KingSlimeBook>());
				nextSlot++;
			}
			if (PolaritiesSystem.downedStormCloudfish)
			{
				//shop.item[nextSlot].SetDefaults(ItemType<StormCloudfishBook>());
				nextSlot++;
			}
			if (NPC.downedBoss1)
			{
				shop.item[nextSlot].SetDefaults(ItemType<EyeOfCthulhuBook>());
				nextSlot++;
			}
			if (PolaritiesSystem.downedStarConstruct)
			{
				shop.item[nextSlot].SetDefaults(ItemType<StarConstructBook>());
				nextSlot++;
			}
			if (PolaritiesSystem.downedEaterOfWorlds)
			{
				shop.item[nextSlot].SetDefaults(ItemType<EaterOfWorldsBook>());
				nextSlot++;
			}
			if (PolaritiesSystem.downedBrainOfCthulhu)
			{
				shop.item[nextSlot].SetDefaults(ItemType<BrainOfCthulhuBook>());
				nextSlot++;
			}
			if (PolaritiesSystem.downedGigabat)
			{
				shop.item[nextSlot].SetDefaults(ItemType<GigabatBook>());
				nextSlot++;
			}
			if (NPC.downedQueenBee)
			{
				shop.item[nextSlot].SetDefaults(ItemType<QueenBeeBook>());
				nextSlot++;
			}
			if (NPC.downedBoss3)
			{
				shop.item[nextSlot].SetDefaults(ItemType<SkeletronBook>());
				nextSlot++;
			}
			if (NPC.downedDeerclops)
			{
				shop.item[nextSlot].SetDefaults(ItemType<DeerclopsBook>());
				nextSlot++;
			}
			if (PolaritiesSystem.downedRiftDenizen)
			{
				//shop.item[nextSlot].SetDefaults(ItemType<RiftDenizenBook>());
				nextSlot++;
			}
			if (Main.hardMode)
			{
				shop.item[nextSlot].SetDefaults(ItemType<WallOfFleshBook>());
				nextSlot++;
			}
			if (NPC.downedQueenSlime)
			{
				shop.item[nextSlot].SetDefaults(ItemType<QueenSlimeBook>());
				nextSlot++;
			}
			if (NPC.downedMechBoss1)
			{
				shop.item[nextSlot].SetDefaults(ItemType<DestroyerBook>());
				nextSlot++;
			}
			if (NPC.downedMechBoss2)
			{
				shop.item[nextSlot].SetDefaults(ItemType<TwinsBook>());
				nextSlot++;
			}
			if (NPC.downedMechBoss3)
			{
				shop.item[nextSlot].SetDefaults(ItemType<SkeletronPrimeBook>());
				nextSlot++;
			}
			if (PolaritiesSystem.downedSunPixie)
			{
				//shop.item[nextSlot].SetDefaults(ItemType<SunPixieBook>());
				nextSlot++;
			}
			if (PolaritiesSystem.downedEsophage)
			{
				//shop.item[nextSlot].SetDefaults(ItemType<EsophageBook>());
				nextSlot++;
			}
			if (NPC.downedPlantBoss)
			{
				shop.item[nextSlot].SetDefaults(ItemType<PlanteraBook>());
				nextSlot++;
			}
			if (PolaritiesSystem.downedSelfsimilarSentinel)
			{
				//shop.item[nextSlot].SetDefaults(ItemType<SelfsimilarSentinelBook>());
				nextSlot++;
			}
			if (NPC.downedGolemBoss)
			{
				shop.item[nextSlot].SetDefaults(ItemType<GolemBook>());
				nextSlot++;
			}
			if (NPC.downedFishron)
			{
				shop.item[nextSlot].SetDefaults(ItemType<DukeFishronBook>());
				nextSlot++;
			}
			if (DD2Event.DownedInvasionT3)
			{
				shop.item[nextSlot].SetDefaults(ItemType<BetsyBook>());
				nextSlot++;
			}
			if (NPC.downedEmpressOfLight)
			{
				shop.item[nextSlot].SetDefaults(ItemType<EmpressOfLightBook>());
				nextSlot++;
			}
			if (PolaritiesSystem.downedEclipxie)
			{
				//shop.item[nextSlot].SetDefaults(ItemType<EclipxieBook>());
				nextSlot++;
			}
			if (PolaritiesSystem.downedHemorrphage)
			{
				//shop.item[nextSlot].SetDefaults(ItemType<HemorrphageBook>());
				nextSlot++;
			}
			if (NPC.downedAncientCultist)
			{
				shop.item[nextSlot].SetDefaults(ItemType<LunaticCultistBook>());
				nextSlot++;
			}
			if (PolaritiesSystem.downedPolarities)
			{
				//shop.item[nextSlot].SetDefaults(ItemType<PolaritiesBook>());
				nextSlot++;
			}
			if (NPC.downedMoonlord)
			{
				shop.item[nextSlot].SetDefaults(ItemType<MoonLordBook>());
				nextSlot++;
			}
		}

        public override bool PreAI()
		{
			//float, relying on blocks/walls for movement
			float support = 10000f;
			int groundY = (int)((NPC.Center.Y + NPC.height / 2 + 8) / 16) + 4;
			for (int i = (int)(NPC.position.X / 16); i <= (int)((NPC.position.X + NPC.width) / 16); i++)
			{
				for (int j = (int)(NPC.position.Y / 16); j <= (int)((NPC.position.Y + NPC.height + 8) / 16) + 3; j++)
				{
					Tile tile = Main.tile[i, j];
					if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType])
					{
						float trySupport = j - (NPC.Center.Y / 16);
						if (tile.IsHalfBlock)
                        {
							trySupport += 0.5f;
						}
						if (trySupport < support) support = trySupport;
						groundY = j;
					}
				}
				if (support != 10000f) { break; }
			}

			bool effectivelyOnGround = support != 10000f;
			bool doWeHover = true;

			int num = 300;
			bool wantToGoInside = Main.raining;
			if (!Main.dayTime)
			{
				wantToGoInside = true;
			}
			if (Main.eclipse)
			{
				wantToGoInside = true;
			}
			if (Main.slimeRain)
			{
				wantToGoInside = true;
			}
			float num25 = 1f;
			if (Main.masterMode)
			{
				NPC.defense = (NPC.dryadWard ? (NPC.defDefense + 14) : NPC.defDefense);
			}
			else if (Main.expertMode)
			{
				NPC.defense = (NPC.dryadWard ? (NPC.defDefense + 10) : NPC.defDefense);
			}
			else
			{
				NPC.defense = (NPC.dryadWard ? (NPC.defDefense + 6) : NPC.defDefense);
			}
			if (NPC.isLikeATownNPC)
			{
				if (NPC.combatBookWasUsed)
				{
					num25 += 0.2f;
					NPC.defense += 6;
				}
				if (NPC.downedBoss1)
				{
					num25 += 0.1f;
					NPC.defense += 3;
				}
				if (NPC.downedBoss2)
				{
					num25 += 0.1f;
					NPC.defense += 3;
				}
				if (NPC.downedBoss3)
				{
					num25 += 0.1f;
					NPC.defense += 3;
				}
				if (NPC.downedQueenBee)
				{
					num25 += 0.1f;
					NPC.defense += 3;
				}
				if (Main.hardMode)
				{
					num25 += 0.4f;
					NPC.defense += 12;
				}
				if (NPC.downedQueenSlime)
				{
					num25 += 0.15f;
					NPC.defense += 6;
				}
				if (NPC.downedMechBoss1)
				{
					num25 += 0.15f;
					NPC.defense += 6;
				}
				if (NPC.downedMechBoss2)
				{
					num25 += 0.15f;
					NPC.defense += 6;
				}
				if (NPC.downedMechBoss3)
				{
					num25 += 0.15f;
					NPC.defense += 6;
				}
				if (NPC.downedPlantBoss)
				{
					num25 += 0.15f;
					NPC.defense += 8;
				}
				if (NPC.downedEmpressOfLight)
				{
					num25 += 0.15f;
					NPC.defense += 8;
				}
				if (NPC.downedGolemBoss)
				{
					num25 += 0.15f;
					NPC.defense += 8;
				}
				if (NPC.downedAncientCultist)
				{
					num25 += 0.15f;
					NPC.defense += 8;
				}
				NPCLoader.BuffTownNPC(ref num25, ref NPC.defense);
			}
			if (NPC.homeTileX == -1 && NPC.homeTileY == -1 && effectivelyOnGround)
			{
				NPC.homeTileX = (int)NPC.Center.X / 16;
				NPC.homeTileY = (int)(NPC.position.Y + (float)NPC.height + 4f) / 16;
			}
			bool flag23 = false;
			int num47 = (int)NPC.Center.X / 16;
			int num58 = groundY - 3;
			NPC.AI_007_FindGoodRestingSpot(num47, num58, out var floorX, out var floorY);
			NPC.directionY = -1;
			if (NPC.direction == 0)
			{
				NPC.direction = 1;
			}
			for (int i = 0; i < 255; i++)
			{
				if (Main.player[i].active && Main.player[i].talkNPC == NPC.whoAmI)
				{
					flag23 = true;
					if (NPC.ai[0] != 0f)
					{
						NPC.netUpdate = true;
					}
					NPC.ai[0] = 0f;
					NPC.ai[1] = 300f;
					NPC.localAI[3] = 100f;
					if (Main.player[i].position.X + (float)(Main.player[i].width / 2) < NPC.Center.X)
					{
						NPC.direction = -1;
					}
					else
					{
						NPC.direction = 1;
					}
				}
			}
			if (NPC.ai[3] == 1f)
			{
				NPC.life = -1;
				NPC.HitEffect();
				NPC.active = false;
				NPC.netUpdate = true;
				return false;
			}
			if (!WorldGen.InWorld(num47, num58) || Main.tile[num47, num58] == null)
			{
				return false;
			}
			if (!NPC.homeless && Main.netMode != 1 && NPC.townNPC && (wantToGoInside || Main.tileDungeon[Main.tile[num47, num58].TileType]) && !NPC.AI_007_TownEntities_IsInAGoodRestingSpot(num47, num58, floorX, floorY))
			{
				bool flag24 = true;
				Rectangle rectangle = default(Rectangle);
				for (int j = 0; j < 2; j++)
				{
					if (!flag24)
					{
						break;
					}
					rectangle = new Rectangle((int)(NPC.position.X + (float)(NPC.width / 2) - (float)(NPC.sWidth / 2) - (float)NPC.safeRangeX), (int)(NPC.position.Y + (float)(NPC.height / 2) - (float)(NPC.sHeight / 2) - (float)NPC.safeRangeY), NPC.sWidth + NPC.safeRangeX * 2, NPC.sHeight + NPC.safeRangeY * 2);
					if (j == 1)
					{
						rectangle = new Rectangle(floorX * 16 + 8 - NPC.sWidth / 2 - NPC.safeRangeX, floorY * 16 + 8 - NPC.sHeight / 2 - NPC.safeRangeY, NPC.sWidth + NPC.safeRangeX * 2, NPC.sHeight + NPC.safeRangeY * 2);
					}
					for (int k = 0; k < 255; k++)
					{
						if (Main.player[k].active)
						{
							Rectangle val = new Rectangle((int)Main.player[k].position.X, (int)Main.player[k].position.Y, Main.player[k].width, Main.player[k].height);
							if (val.Intersects(rectangle))
							{
								flag24 = false;
								break;
							}
						}
					}
				}
				if (flag24)
				{
					NPC.AI_007_TownEntities_TeleportToHome(floorX, floorY);
				}
			}
			float num69 = 200f;
			if (NPCID.Sets.DangerDetectRange[NPC.type] != -1)
			{
				num69 = NPCID.Sets.DangerDetectRange[NPC.type];
			}
			bool flag2 = false;
			bool flag3 = false;
			float num80 = -1f;
			float num91 = -1f;
			int num102 = 0;
			int num2 = -1;
			int num13 = -1;
			bool keepwalking4;
			if (Main.netMode != 1 && !flag23)
			{
				for (int l = 0; l < 200; l++)
				{
					if (!Main.npc[l].active)
					{
						continue;
					}
					bool? modCanHit = NPCLoader.CanHitNPC(Main.npc[l], NPC);
					if (modCanHit.HasValue && !modCanHit.Value)
					{
						continue;
					}
					bool canHitVal = modCanHit.HasValue && modCanHit.Value;
					if (!Main.npc[l].active || Main.npc[l].friendly || Main.npc[l].damage <= 0 || !(Main.npc[l].Distance(NPC.Center) < num69) || (!Main.npc[l].noTileCollide && !Collision.CanHit(NPC.Center, 0, 0, Main.npc[l].Center, 0, 0)))
					{
						continue;
					}
					bool flag4 = Main.npc[l].CanBeChasedBy(this);
					flag2 = true;
					float num17 = Main.npc[l].Center.X - NPC.Center.X;
					if (num17 < 0f && (num80 == -1f || num17 > num80))
					{
						num80 = num17;
						if (flag4)
						{
							num2 = l;
						}
					}
					if (num17 > 0f && (num91 == -1f || num17 < num91))
					{
						num91 = num17;
						if (flag4)
						{
							num13 = l;
						}
					}
				}
				if (flag2)
				{
					num102 = ((num80 == -1f) ? 1 : ((num91 != -1f) ? (num91 < 0f - num80).ToDirectionInt() : (-1)));
					float num18 = 0f;
					if (num80 != -1f)
					{
						num18 = 0f - num80;
					}
					if (num18 == 0f || (num91 < num18 && num91 > 0f))
					{
						num18 = num91;
					}
					if (NPC.ai[0] == 8f)
					{
						if (NPC.direction == -num102)
						{
							NPC.ai[0] = 1f;
							NPC.ai[1] = 300 + Main.rand.Next(300);
							NPC.ai[2] = 0f;
							NPC.localAI[3] = 0f;
							NPC.netUpdate = true;
						}
					}
					else if (NPC.ai[0] != 10f && NPC.ai[0] != 12f && NPC.ai[0] != 13f && NPC.ai[0] != 14f && NPC.ai[0] != 15f)
					{
						if (NPCID.Sets.PrettySafe[NPC.type] != -1 && (float)NPCID.Sets.PrettySafe[NPC.type] < num18)
						{
							flag2 = false;
							flag3 = NPCID.Sets.AttackType[NPC.type] > -1;
						}
						else if (NPC.ai[0] != 1f)
						{
							int tileX = (int)((NPC.position.X + (float)(NPC.width / 2) + (float)(15 * NPC.direction)) / 16f);
							int tileY = (int)((NPC.position.Y + (float)NPC.height - 16f) / 16f);
							bool currentlyDrowning = NPC.wet;
							NPC.AI_007_TownEntities_GetWalkPrediction(num47, floorX, false, currentlyDrowning, tileX, tileY, out keepwalking4, out var avoidFalling);
							keepwalking4 = true;
							if (!avoidFalling)
							{
								if (NPC.ai[0] == 3f || NPC.ai[0] == 4f || NPC.ai[0] == 16f || NPC.ai[0] == 17f)
								{
									NPC nPC = Main.npc[(int)NPC.ai[2]];
									if (nPC.active)
									{
										NPC.ai[0] = 1f;
										NPC.ai[1] = 120 + Main.rand.Next(120);
										NPC.ai[2] = 0f;
										NPC.localAI[3] = 0f;
										nPC.direction = -num102;
										NPC.netUpdate = true;
									}
								}
								NPC.ai[0] = 1f;
								NPC.ai[1] = 120 + Main.rand.Next(120);
								NPC.ai[2] = 0f;
								NPC.localAI[3] = 0f;
								NPC.direction = -num102;
								NPC.netUpdate = true;
							}
						}
						else if (NPC.ai[0] == 1f && NPC.direction != -num102)
						{
							NPC.direction = -num102;
							NPC.netUpdate = true;
						}
					}
				}
			}
			if (NPC.ai[0] == 0f)
			{
				if (NPC.localAI[3] > 0f)
				{
					NPC.localAI[3] -= 1f;
				}
				int num19 = 120;
				if (wantToGoInside && !flag23)
				{
					if (Main.netMode != 1)
					{
						if (num47 == floorX && num58 == floorY)
						{
							if (NPC.velocity.X != 0f)
							{
								NPC.netUpdate = true;
							}
							if (NPC.velocity.X > 0.1f)
							{
								NPC.velocity.X -= 0.1f;
							}
							else if (NPC.velocity.X < -0.1f)
							{
								NPC.velocity.X += 0.1f;
							}
							else
							{
								NPC.velocity.X = 0f;
								NPC.AI_007_TryForcingSitting(floorX, floorY);
							}
						}
						else
						{
							if (num47 > floorX)
							{
								NPC.direction = -1;
							}
							else
							{
								NPC.direction = 1;
							}
							NPC.ai[0] = 1f;
							NPC.ai[1] = 200 + Main.rand.Next(200);
							NPC.ai[2] = 0f;
							NPC.localAI[3] = 0f;
							NPC.netUpdate = true;
						}
					}
				}
				else
				{
					if (NPC.velocity.X > 0.1f)
					{
						NPC.velocity.X -= 0.1f;
					}
					else if (NPC.velocity.X < -0.1f)
					{
						NPC.velocity.X += 0.1f;
					}
					else
					{
						NPC.velocity.X = 0f;
					}
					if (Main.netMode != 1)
					{
						if (NPC.ai[1] > 0f)
						{
							NPC.ai[1] -= 1f;
						}
						bool flag5 = true;
						int tileX2 = (int)((NPC.position.X + (float)(NPC.width / 2) + (float)(15 * NPC.direction)) / 16f);
						int tileY2 = (int)((NPC.position.Y + (float)NPC.height - 16f) / 16f);
						bool currentlyDrowning2 = NPC.wet;
						NPC.AI_007_TownEntities_GetWalkPrediction(num47, floorX, false, currentlyDrowning2, tileX2, tileY2, out keepwalking4, out var avoidFalling2);
						keepwalking4 = true;
						if (NPC.wet)
						{
							bool currentlyDrowning3 = Collision.DrownCollision(NPC.position, NPC.width, NPC.height, 1f, includeSlopes: true);
							if (currentlyDrowning3)
							{
								NPC.ai[0] = 1f;
								NPC.ai[1] = 200 + Main.rand.Next(300);
								NPC.ai[2] = 0f;
								NPC.localAI[3] = 0f;
								NPC.netUpdate = true;
							}
						}
						if (avoidFalling2)
						{
							flag5 = false;
						}
						if (NPC.ai[1] <= 0f)
						{
							if (flag5 && !avoidFalling2)
							{
								NPC.ai[0] = 1f;
								NPC.ai[1] = 200 + Main.rand.Next(300);
								NPC.ai[2] = 0f;
								NPC.localAI[3] = 0f;
								NPC.netUpdate = true;
							}
							else
							{
								NPC.direction *= -1;
								NPC.ai[1] = 60 + Main.rand.Next(120);
								NPC.netUpdate = true;
							}
						}
					}
				}
				if (Main.netMode != 1 && (!wantToGoInside || NPC.AI_007_TownEntities_IsInAGoodRestingSpot(num47, num58, floorX, floorY)))
				{
					if (num47 < floorX - 25 || num47 > floorX + 25)
					{
						if (NPC.localAI[3] == 0f)
						{
							if (num47 < floorX - 50 && NPC.direction == -1)
							{
								NPC.direction = 1;
								NPC.netUpdate = true;
							}
							else if (num47 > floorX + 50 && NPC.direction == 1)
							{
								NPC.direction = -1;
								NPC.netUpdate = true;
							}
						}
					}
					else if (Main.rand.Next(80) == 0 && NPC.localAI[3] == 0f)
					{
						NPC.localAI[3] = 200f;
						NPC.direction *= -1;
						NPC.netUpdate = true;
					}
				}
			}
			else if (NPC.ai[0] == 1f)
			{
				if (Main.netMode != 1 && wantToGoInside && NPC.AI_007_TownEntities_IsInAGoodRestingSpot(num47, num58, floorX, floorY))
				{
					NPC.ai[0] = 0f;
					NPC.ai[1] = 200 + Main.rand.Next(200);
					NPC.localAI[3] = 60f;
					NPC.netUpdate = true;
				}
				else
				{
					bool flag6 = Collision.DrownCollision(NPC.position, NPC.width, NPC.height, 1f, includeSlopes: true);
					if (!flag6)
					{
						if (Main.netMode != 1 && !NPC.homeless && !Main.tileDungeon[Main.tile[num47, num58].TileType] && (num47 < floorX - 35 || num47 > floorX + 35))
						{
							if (NPC.position.X < (float)(floorX * 16) && NPC.direction == -1)
							{
								NPC.ai[1] -= 5f;
							}
							else if (NPC.position.X > (float)(floorX * 16) && NPC.direction == 1)
							{
								NPC.ai[1] -= 5f;
							}
						}
						NPC.ai[1] -= 1f;
					}
					if (NPC.ai[1] <= 0f)
					{
						NPC.ai[0] = 0f;
						NPC.ai[1] = 300 + Main.rand.Next(300);
						NPC.ai[2] = 0f;
						NPC.ai[1] += Main.rand.Next(900);
						NPC.localAI[3] = 60f;
						NPC.netUpdate = true;
					}
					if (NPC.closeDoor && ((NPC.position.X + (float)(NPC.width / 2)) / 16f > (float)(NPC.doorX + 2) || (NPC.position.X + (float)(NPC.width / 2)) / 16f < (float)(NPC.doorX - 2)))
					{
						Tile tileSafely = Framing.GetTileSafely(NPC.doorX, NPC.doorY);
						if (TileLoader.CloseDoorID(tileSafely) >= 0)
						{
							if (WorldGen.CloseDoor(NPC.doorX, NPC.doorY))
							{
								NPC.closeDoor = false;
								NetMessage.SendData(19, -1, -1, null, 1, NPC.doorX, NPC.doorY, NPC.direction);
							}
							if ((NPC.position.X + (float)(NPC.width / 2)) / 16f > (float)(NPC.doorX + 4) || (NPC.position.X + (float)(NPC.width / 2)) / 16f < (float)(NPC.doorX - 4) || (NPC.position.Y + (float)(NPC.height / 2)) / 16f > (float)(NPC.doorY + 4) || (NPC.position.Y + (float)(NPC.height / 2)) / 16f < (float)(NPC.doorY - 4))
							{
								NPC.closeDoor = false;
							}
						}
						else if (tileSafely.TileType == 389)
						{
							if (WorldGen.ShiftTallGate(NPC.doorX, NPC.doorY, closing: true))
							{
								NPC.closeDoor = false;
								NetMessage.SendData(19, -1, -1, null, 5, NPC.doorX, NPC.doorY);
							}
							if ((NPC.position.X + (float)(NPC.width / 2)) / 16f > (float)(NPC.doorX + 4) || (NPC.position.X + (float)(NPC.width / 2)) / 16f < (float)(NPC.doorX - 4) || (NPC.position.Y + (float)(NPC.height / 2)) / 16f > (float)(NPC.doorY + 4) || (NPC.position.Y + (float)(NPC.height / 2)) / 16f < (float)(NPC.doorY - 4))
							{
								NPC.closeDoor = false;
							}
						}
						else
						{
							NPC.closeDoor = false;
						}
					}
					float num20 = 1f;
					float num21 = 0.07f;
					if (NPC.friendly && (flag2 || flag6))
					{
						num20 = 1.5f;
						float num22 = 1f - (float)NPC.life / (float)NPC.lifeMax;
						num20 += num22 * 0.9f;
						num21 = 0.1f;
					}
					if (NPC.velocity.X < 0f - num20 || NPC.velocity.X > num20)
					{
						if (effectivelyOnGround)
						{
							NPC.velocity *= 0.8f;
						}
					}
					else if (NPC.velocity.X < num20 && NPC.direction == 1)
					{
						NPC.velocity.X += num21;
						if (NPC.velocity.X > num20)
						{
							NPC.velocity.X = num20;
						}
					}
					else if (NPC.velocity.X > 0f - num20 && NPC.direction == -1)
					{
						NPC.velocity.X -= num21;
						if (NPC.velocity.X > num20)
						{
							NPC.velocity.X = num20;
						}
					}
					/*bool flag7 = true;
					if ((float)(NPC.homeTileY * 16 - 32) > NPC.Bottom.Y)
					{
						flag7 = false;
					}
					if (!flag7 && wantToGoInside)
                    {
						doWeHover = false;
					}*/
					if (effectivelyOnGround)
					{
						int num23 = (int)((NPC.position.X + (float)(NPC.width / 2) + (float)(15 * NPC.direction)) / 16f);
						int num24 = (int)((NPC.position.Y + (float)NPC.height - 16f) / 16f);
						int num26 = 180;
						NPC.AI_007_TownEntities_GetWalkPrediction(num47, floorX, false, flag6, num23, num24, out var keepwalking3, out var avoidFalling3);
						keepwalking3 = true;
						bool flag8 = false;
						bool flag9 = false;
						if (NPC.wet && NPC.townNPC && (flag9 = flag6) && NPC.localAI[3] <= 0f)
						{
							avoidFalling3 = true;
							NPC.localAI[3] = num26;
							int num27 = 0;
							for (int m = 0; m <= 10 && Framing.GetTileSafely(num23 - NPC.direction, num24 - m).LiquidAmount != 0; m++)
							{
								num27++;
							}
							float num28 = 0.3f;
							float num29 = (float)Math.Sqrt((double)((float)(num27 * 16 + 16) * 2f * num28));
							if (num29 > 26f)
							{
								num29 = 26f;
							}
							NPC.velocity.Y = 0f - num29;
							NPC.localAI[3] = NPC.position.X;
							flag8 = true;
						}
						if (avoidFalling3 && !flag8)
						{
							int num30 = (int)((NPC.position.X + (float)(NPC.width / 2)) / 16f);
							int num31 = 0;
							for (int n = -1; n <= 1; n++)
							{
								Tile tileSafely2 = Framing.GetTileSafely(num30 + n, num24 + 1);
								if (tileSafely2.HasUnactuatedTile && Main.tileSolid[tileSafely2.TileType])
								{
									num31++;
								}
							}
							if (num31 <= 2)
							{
								if (NPC.velocity.X != 0f)
								{
									NPC.netUpdate = true;
								}
								keepwalking3 = (avoidFalling3 = false);
								NPC.ai[0] = 0f;
								NPC.ai[1] = 50 + Main.rand.Next(50);
								NPC.ai[2] = 0f;
								NPC.localAI[3] = 40f;
							}
						}
						if (NPC.position.X == NPC.localAI[3] && !flag8)
						{
							NPC.direction *= -1;
							NPC.netUpdate = true;
							NPC.localAI[3] = num26;
						}
						if (flag6 && !flag8)
						{
							if (NPC.localAI[3] > (float)num26)
							{
								NPC.localAI[3] = num26;
							}
							if (NPC.localAI[3] > 0f)
							{
								NPC.localAI[3] -= 1f;
							}
						}
						else
						{
							NPC.localAI[3] = -1f;
						}
						Tile tileSafely3 = Framing.GetTileSafely(num23, groundY - 1);
						Tile tileSafely4 = Framing.GetTileSafely(num23, groundY - 2);
						Tile tileSafely5 = Framing.GetTileSafely(num23, groundY - 3);
						bool flag10 = NPC.height / 16 < 3;
						if (NPC.townNPC && tileSafely5.HasUnactuatedTile && (TileLoader.OpenDoorID(tileSafely5) >= 0 || tileSafely5.TileType == 388) && (Main.rand.Next(4) == 0)) //this is normally 1/10 unless wantToGoInside
						{
							if (Main.netMode != 1)
							{
								if (WorldGen.OpenDoor(num23, groundY - 3, NPC.direction))
								{
									NPC.closeDoor = true;
									NPC.doorX = num23;
									NPC.doorY = groundY - 3;
									NetMessage.SendData(19, -1, -1, null, 0, num23, groundY - 3, NPC.direction);
									NPC.netUpdate = true;
									NPC.ai[1] += 80f;
								}
								else if (WorldGen.OpenDoor(num23, groundY - 3, -NPC.direction))
								{
									NPC.closeDoor = true;
									NPC.doorX = num23;
									NPC.doorY = groundY - 3;
									NetMessage.SendData(19, -1, -1, null, 0, num23, groundY - 3, -NPC.direction);
									NPC.netUpdate = true;
									NPC.ai[1] += 80f;
								}
								else if (WorldGen.ShiftTallGate(num23, floorY - 3, closing: false))
								{
									NPC.closeDoor = true;
									NPC.doorX = num23;
									NPC.doorY = groundY - 3;
									NetMessage.SendData(19, -1, -1, null, 4, num23, groundY - 3);
									NPC.netUpdate = true;
									NPC.ai[1] += 80f;
								}
								else
								{
									NPC.direction *= -1;
									NPC.netUpdate = true;
								}
							}
						}
						else
						{
							if ((NPC.velocity.X < 0f && NPC.spriteDirection == -1) || (NPC.velocity.X > 0f && NPC.spriteDirection == 1))
							{
								bool flag11 = false;
								bool flag13 = false;
								if (avoidFalling3)
								{
									if (!flag9)
									{
										flag11 = true;
									}
									if (flag2)
									{
										flag13 = true;
									}
								}
								if (flag13)
								{
									keepwalking3 = false;
									NPC.velocity.X = 0f;
									NPC.ai[0] = 8f;
									NPC.ai[1] = 240f;
									NPC.netUpdate = true;
								}
								if (flag11)
								{
									NPC.direction *= -1;
									NPC.velocity.X *= -1f;
									NPC.netUpdate = true;
								}
								if (keepwalking3)
								{
									NPC.ai[1] = 90f;
									NPC.netUpdate = true;
								}
								if (NPC.velocity.Y < 0f)
								{
									NPC.localAI[3] = NPC.position.X;
								}
							}
							if (NPC.velocity.Y < 0f && NPC.wet)
							{
								NPC.velocity.Y *= 1.2f;
							}
						}
					}
				}
			}
			else if (NPC.ai[0] == 2f || NPC.ai[0] == 11f)
			{
				if (Main.netMode != 1)
				{
					NPC.localAI[3] -= 1f;
					if (Main.rand.Next(60) == 0 && NPC.localAI[3] == 0f)
					{
						NPC.localAI[3] = 60f;
						NPC.direction *= -1;
						NPC.netUpdate = true;
					}
				}
				NPC.ai[1] -= 1f;
				NPC.velocity.X *= 0.8f;
				if (NPC.ai[1] <= 0f)
				{
					NPC.localAI[3] = 40f;
					NPC.ai[0] = 0f;
					NPC.ai[1] = 60 + Main.rand.Next(60);
					NPC.netUpdate = true;
				}
			}
			else if (NPC.ai[0] == 3f || NPC.ai[0] == 4f || NPC.ai[0] == 5f || NPC.ai[0] == 8f || NPC.ai[0] == 9f || NPC.ai[0] == 16f || NPC.ai[0] == 17f || NPC.ai[0] == 20f || NPC.ai[0] == 21f || NPC.ai[0] == 22f || NPC.ai[0] == 23f)
			{
				NPC.velocity.X *= 0.8f;
				NPC.ai[1] -= 1f;
				if (NPC.ai[0] == 8f && NPC.ai[1] < 60f && flag2)
				{
					NPC.ai[1] = 180f;
					NPC.netUpdate = true;
				}
				if (NPC.ai[0] == 5f)
				{
					Point coords = (NPC.Bottom + Vector2.UnitY * -2f).ToTileCoordinates();
					Tile tile = Main.tile[coords.X, coords.Y];
					if (tile.TileType != 15 && tile.TileType != 497)
					{
						NPC.ai[1] = 0f;
					}
					else
					{
						Main.sittingManager.AddNPC(NPC.whoAmI, coords);
					}
				}
				if (NPC.ai[1] <= 0f)
				{
					NPC.ai[0] = 0f;
					NPC.ai[1] = 60 + Main.rand.Next(60);
					NPC.ai[2] = 0f;
					NPC.localAI[3] = 30 + Main.rand.Next(60);
					NPC.netUpdate = true;
				}
			}
			else if (NPC.ai[0] == 6f || NPC.ai[0] == 7f || NPC.ai[0] == 18f || NPC.ai[0] == 19f)
			{
				if (NPC.ai[0] == 18f && (NPC.localAI[3] < 1f || NPC.localAI[3] > 2f))
				{
					NPC.localAI[3] = 2f;
				}
				NPC.velocity.X *= 0.8f;
				NPC.ai[1] -= 1f;
				int num32 = (int)NPC.ai[2];
				if (num32 < 0 || num32 > 255 || !Main.player[num32].CanBeTalkedTo || Main.player[num32].Distance(NPC.Center) > 200f || !Collision.CanHitLine(NPC.Top, 0, 0, Main.player[num32].Top, 0, 0))
				{
					NPC.ai[1] = 0f;
				}
				if (NPC.ai[1] > 0f)
				{
					int num33 = ((NPC.Center.X < Main.player[num32].Center.X) ? 1 : (-1));
					if (num33 != NPC.direction)
					{
						NPC.netUpdate = true;
					}
					NPC.direction = num33;
				}
				else
				{
					NPC.ai[0] = 0f;
					NPC.ai[1] = 60 + Main.rand.Next(60);
					NPC.ai[2] = 0f;
					NPC.localAI[3] = 30 + Main.rand.Next(60);
					NPC.netUpdate = true;
				}
			}
			bool flag16 = NPC.ai[0] < 2f && !flag2 && !NPC.wet;
			bool flag17 = (NPC.ai[0] < 2f || NPC.ai[0] == 8f) && (flag2 || flag3);
			if (NPC.localAI[1] > 0f)
			{
				NPC.localAI[1] -= 1f;
			}
			if (NPC.localAI[1] > 0f)
			{
				flag17 = false;
			}
			if (NPC.CanTalk && flag16 && NPC.ai[0] == 0f && effectivelyOnGround && Main.rand.Next(300) == 0)
			{
				int num89 = 420;
				num89 = ((Main.rand.Next(2) != 0) ? (num89 * Main.rand.Next(1, 3)) : (num89 * Main.rand.Next(1, 4)));
				int num90 = 100;
				int num92 = 20;
				for (int num93 = 0; num93 < 200; num93++)
				{
					NPC nPC4 = Main.npc[num93];
					bool flag18 = (nPC4.ai[0] == 1f && nPC4.closeDoor) || (nPC4.ai[0] == 1f && nPC4.ai[1] > 200f) || nPC4.ai[0] > 1f || nPC4.wet;
					if (nPC4 != NPC && nPC4.active && nPC4.CanBeTalkedTo && !flag18 && nPC4.Distance(NPC.Center) < (float)num90 && nPC4.Distance(NPC.Center) > (float)num92 && Collision.CanHit(NPC.Center, 0, 0, nPC4.Center, 0, 0))
					{
						int num94 = (NPC.position.X < nPC4.position.X).ToDirectionInt();
						NPC.ai[0] = 3f;
						NPC.ai[1] = num89;
						NPC.ai[2] = num93;
						NPC.direction = num94;
						NPC.netUpdate = true;
						nPC4.ai[0] = 4f;
						nPC4.ai[1] = num89;
						nPC4.ai[2] = NPC.whoAmI;
						nPC4.direction = -num94;
						nPC4.netUpdate = true;
						break;
					}
				}
			}
			else if (NPC.CanTalk && flag16 && NPC.ai[0] == 0f && effectivelyOnGround && Main.rand.Next(1800) == 0)
			{
				int num95 = 420;
				num95 = ((Main.rand.Next(2) != 0) ? (num95 * Main.rand.Next(1, 3)) : (num95 * Main.rand.Next(1, 4)));
				int num96 = 100;
				int num97 = 20;
				for (int num98 = 0; num98 < 200; num98++)
				{
					NPC nPC5 = Main.npc[num98];
					bool flag19 = (nPC5.ai[0] == 1f && nPC5.closeDoor) || (nPC5.ai[0] == 1f && nPC5.ai[1] > 200f) || nPC5.ai[0] > 1f || nPC5.wet;
					if (nPC5 != NPC && nPC5.active && nPC5.CanBeTalkedTo && !NPCID.Sets.IsTownPet[nPC5.type] && !flag19 && nPC5.Distance(NPC.Center) < (float)num96 && nPC5.Distance(NPC.Center) > (float)num97 && Collision.CanHit(NPC.Center, 0, 0, nPC5.Center, 0, 0))
					{
						int num99 = (NPC.position.X < nPC5.position.X).ToDirectionInt();
						NPC.ai[0] = 16f;
						NPC.ai[1] = num95;
						NPC.ai[2] = num98;
						NPC.localAI[2] = Main.rand.Next(4);
						NPC.localAI[3] = Main.rand.Next(3 - (int)NPC.localAI[2]);
						NPC.direction = num99;
						NPC.netUpdate = true;
						nPC5.ai[0] = 17f;
						nPC5.ai[1] = num95;
						nPC5.ai[2] = NPC.whoAmI;
						nPC5.localAI[2] = 0f;
						nPC5.localAI[3] = 0f;
						nPC5.direction = -num99;
						nPC5.netUpdate = true;
						break;
					}
				}
			}
			else if (flag16 && NPC.ai[0] == 0f && effectivelyOnGround && Main.rand.Next(1800) == 0)
			{
				NPC.ai[0] = 2f;
				NPC.ai[1] = 45 * Main.rand.Next(1, 2);
				NPC.netUpdate = true;
			}
			else if (flag16 && NPC.ai[0] == 0f && effectivelyOnGround && Main.rand.Next(1200) == 0)
			{
				int num107 = 220;
				int num108 = 150;
				for (int num109 = 0; num109 < 255; num109++)
				{
					Player player3 = Main.player[num109];
					if (player3.CanBeTalkedTo && player3.Distance(NPC.Center) < (float)num108 && Collision.CanHitLine(NPC.Top, 0, 0, player3.Top, 0, 0))
					{
						int direction4 = (NPC.position.X < player3.position.X).ToDirectionInt();
						NPC.ai[0] = 7f;
						NPC.ai[1] = num107;
						NPC.ai[2] = num109;
						NPC.direction = direction4;
						NPC.netUpdate = true;
						break;
					}
				}
			}
			else if (flag16 && NPC.ai[0] == 1f && effectivelyOnGround && num > 0 && Main.rand.Next(num) == 0)
			{
				Point b = (NPC.Bottom + Vector2.UnitY * -2f).ToTileCoordinates();
				b = new Point(b.X, num58);
				bool flag20 = WorldGen.InWorld(b.X, b.Y, 1);
				if (flag20)
				{
					for (int num110 = 0; num110 < 200; num110++)
					{
						if (Main.npc[num110].active && Main.npc[num110].aiStyle == 7 && Main.npc[num110].townNPC && Main.npc[num110].ai[0] == 5f && (Main.npc[num110].Bottom + Vector2.UnitY * -2f).ToTileCoordinates() == b)
						{
							flag20 = false;
							break;
						}
					}
					for (int num111 = 0; num111 < 255; num111++)
					{
						if (Main.player[num111].active && Main.player[num111].sitting.isSitting && Main.player[num111].Center.ToTileCoordinates() == b)
						{
							flag20 = false;
							break;
						}
					}
				}
				if (flag20)
				{
					Tile tile2 = Main.tile[b.X, b.Y];
					flag20 = tile2.TileType == 15 || tile2.TileType == 497;
					if (flag20 && tile2.TileType == 15 && tile2.TileFrameY >= 1080 && tile2.TileFrameY <= 1098)
					{
						flag20 = false;
					}
					if (flag20)
					{
						NPC.ai[0] = 5f;
						NPC.ai[1] = 900 + Main.rand.Next(10800);
						NPC.direction = ((tile2.TileFrameX != 0) ? 1 : (-1));
						NPC.Bottom = new Vector2((float)(b.X * 16 + 8 + 2 * NPC.direction), (float)(b.Y * 16 + 16));
						NPC.velocity = Vector2.Zero;
						NPC.localAI[3] = 0f;
						NPC.netUpdate = true;
					}
				}
			}
			else if (flag16 && NPC.ai[0] == 1f && effectivelyOnGround && Main.rand.Next(600) == 0 && Utils.PlotTileLine(NPC.Top, NPC.Bottom, NPC.width, DelegateMethods.SearchAvoidedByNPCs))
			{
				Point point = (NPC.Center + new Vector2((float)(NPC.direction * 10), 0f)).ToTileCoordinates();
				bool flag21 = WorldGen.InWorld(point.X, point.Y, 1);
				if (flag21)
				{
					Tile tileSafely6 = Framing.GetTileSafely(point.X, point.Y);
					if (!tileSafely6.HasUnactuatedTile || !TileID.Sets.InteractibleByNPCs[tileSafely6.TileType])
					{
						flag21 = false;
					}
				}
				if (flag21)
				{
					NPC.ai[0] = 9f;
					NPC.ai[1] = 40 + Main.rand.Next(90);
					NPC.velocity = Vector2.Zero;
					NPC.localAI[3] = 0f;
					NPC.netUpdate = true;
				}
			}

			if (NPC.ai[0] != 5)
			{
				if (doWeHover)
				{
					NPC.velocity.Y += Math.Clamp((support - 1.5f) * 0.1f, -0.1f, 0.1f);
					//kick to prevent slowing down
					if (Math.Abs((support - 1.5f)) < 0.05f)
					{
						NPC.velocity.Y *= 1.025f;
					}
				}
				else
				{
					NPC.velocity.Y += 0.1f;
				}
			}

			NPC.velocity *= 0.99f;

			if (NPC.velocity.Y < -2)
			{
				NPC.velocity.Y = -2;
			}
			if (NPC.velocity.Y > 2)
			{
				NPC.velocity.Y = 2;
			}
			NPC.rotation = NPC.velocity.X * 0.05f;

			return false;
		}

        public override void FindFrame(int frameHeight)
        {
			int num = frameHeight;

			int num173 = NPCID.Sets.ExtraFramesCount[Type];
			if (NPC.velocity.Y < 2)
			{
				if (NPC.direction == 1)
				{
					NPC.spriteDirection = 1;
				}
				if (NPC.direction == -1)
				{
					NPC.spriteDirection = -1;
				}
				int num174 = Main.npcFrameCount[Type] - NPCID.Sets.AttackFrameCount[Type];
				if (NPC.ai[0] == 23f)
				{
					NPC.frameCounter += 1.0;
					int num175 = NPC.frame.Y / num;
					int num182 = num174 - num175;
					if ((uint)(num182 - 1) > 1u && (uint)(num182 - 4) > 1u && num175 != 0)
					{
						NPC.frame.Y = 0;
						NPC.frameCounter = 0.0;
					}
					int num190 = ((!(NPC.frameCounter < 6.0)) ? (num174 - 4) : (num174 - 5));
					if (NPC.ai[1] < 6f)
					{
						num190 = num174 - 5;
					}
					NPC.frame.Y = num * num190;
				}
				else if (NPC.ai[0] >= 20f && NPC.ai[0] <= 22f)
				{
					int num191 = NPC.frame.Y / num;
					NPC.frame.Y = num191 * num;
				}
				else if (NPC.ai[0] == 2f)
				{
					NPC.frameCounter += 1.0;
					if (NPC.frame.Y / num == num174 - 1 && NPC.frameCounter >= 5.0)
					{
						NPC.frame.Y = 0;
						NPC.frameCounter = 0.0;
					}
					else if (NPC.frame.Y / num == 0 && NPC.frameCounter >= 40.0)
					{
						NPC.frame.Y = num * (num174 - 1);
						NPC.frameCounter = 0.0;
					}
					else if (NPC.frame.Y != 0 && NPC.frame.Y != num * (num174 - 1))
					{
						NPC.frame.Y = 0;
						NPC.frameCounter = 0.0;
					}
				}
				else if (NPC.ai[0] == 11f)
				{
					NPC.frameCounter += 1.0;
					if (NPC.frame.Y / num == num174 - 1 && NPC.frameCounter >= 50.0)
					{
						if (NPC.frameCounter == 50.0)
						{
							int num194 = Main.rand.Next(4);
							for (int l = 0; l < 3 + num194; l++)
							{
								int num195 = Dust.NewDust(NPC.Center + Vector2.UnitX * (float)(-NPC.direction) * 8f - Vector2.One * 5f + Vector2.UnitY * 8f, 3, 6, 216, -NPC.direction, 1f);
								Dust obj3 = Main.dust[num195];
								obj3.velocity /= 2f;
								Main.dust[num195].scale = 0.8f;
							}
							if (Main.rand.Next(30) == 0)
							{
								int num196 = Gore.NewGore(NPC.GetSource_FromAI(), NPC.Center + Vector2.UnitX * (float)(-NPC.direction) * 8f, Vector2.Zero, Main.rand.Next(580, 583));
								Gore obj4 = Main.gore[num196];
								obj4.velocity /= 2f;
								Main.gore[num196].velocity.Y = Math.Abs(Main.gore[num196].velocity.Y);
								Main.gore[num196].velocity.X = (0f - Math.Abs(Main.gore[num196].velocity.X)) * (float)NPC.direction;
							}
						}
						if (NPC.frameCounter >= 100.0 && Main.rand.Next(20) == 0)
						{
							NPC.frame.Y = 0;
							NPC.frameCounter = 0.0;
						}
					}
					else if (NPC.frame.Y / num == 0 && NPC.frameCounter >= 20.0)
					{
						NPC.frame.Y = num * (num174 - 1);
						NPC.frameCounter = 0.0;
						if (Main.netMode != 1)
						{
							EmoteBubble.NewBubble(89, new WorldUIAnchor((Entity)NPC), 90);
						}
					}
					else if (NPC.frame.Y != 0 && NPC.frame.Y != num * (num174 - 1))
					{
						NPC.frame.Y = 0;
						NPC.frameCounter = 0.0;
					}
				}
				else if (NPC.ai[0] == 5f)
				{
					NPC.frame.Y = num * (num174 - 3);
					NPC.frameCounter = 0.0;
				}
				else if (NPC.ai[0] == 6f)
				{
					NPC.frameCounter += 1.0;
					int num197 = NPC.frame.Y / num;
					int num181 = num174 - num197;
					if ((uint)(num181 - 1) > 1u && (uint)(num181 - 4) > 1u && num197 != 0)
					{
						NPC.frame.Y = 0;
						NPC.frameCounter = 0.0;
					}
					int num198 = 0;
					num198 = ((!(NPC.frameCounter < 10.0)) ? ((NPC.frameCounter < 16.0) ? (num174 - 5) : ((NPC.frameCounter < 46.0) ? (num174 - 4) : ((NPC.frameCounter < 60.0) ? (num174 - 5) : ((!(NPC.frameCounter < 66.0)) ? ((NPC.frameCounter < 72.0) ? (num174 - 5) : ((NPC.frameCounter < 102.0) ? (num174 - 4) : ((NPC.frameCounter < 108.0) ? (num174 - 5) : ((!(NPC.frameCounter < 114.0)) ? ((NPC.frameCounter < 120.0) ? (num174 - 5) : ((NPC.frameCounter < 150.0) ? (num174 - 4) : ((NPC.frameCounter < 156.0) ? (num174 - 5) : ((!(NPC.frameCounter < 162.0)) ? ((NPC.frameCounter < 168.0) ? (num174 - 5) : ((NPC.frameCounter < 198.0) ? (num174 - 4) : ((NPC.frameCounter < 204.0) ? (num174 - 5) : ((!(NPC.frameCounter < 210.0)) ? ((NPC.frameCounter < 216.0) ? (num174 - 5) : ((NPC.frameCounter < 246.0) ? (num174 - 4) : ((NPC.frameCounter < 252.0) ? (num174 - 5) : ((!(NPC.frameCounter < 258.0)) ? ((NPC.frameCounter < 264.0) ? (num174 - 5) : ((NPC.frameCounter < 294.0) ? (num174 - 4) : ((NPC.frameCounter < 300.0) ? (num174 - 5) : 0))) : 0)))) : 0)))) : 0)))) : 0)))) : 0)))) : 0);
					if (num198 == num174 - 4 && num197 == num174 - 5)
					{
						Vector2 position = NPC.Center + new Vector2((float)(10 * NPC.direction), -4f);
						for (int m = 0; m < 8; m++)
						{
							int num199 = Main.rand.Next(139, 143);
							int num200 = Dust.NewDust(position, 0, 0, num199, NPC.velocity.X + (float)NPC.direction, NPC.velocity.Y - 2.5f, 0, default(Color), 1.2f);
							Main.dust[num200].velocity.X += (float)NPC.direction * 1.5f;
							Dust obj5 = Main.dust[num200];
							obj5.position -= new Vector2(4f);
							Dust obj6 = Main.dust[num200];
							obj6.velocity *= 2f;
							Main.dust[num200].scale = 0.7f + Main.rand.NextFloat() * 0.3f;
						}
					}
					NPC.frame.Y = num * num198;
					if (NPC.frameCounter >= 300.0)
					{
						NPC.frameCounter = 0.0;
					}
				}
				else if ((NPC.ai[0] == 7f || NPC.ai[0] == 19f))
				{
					NPC.frameCounter += 1.0;
					int num201 = NPC.frame.Y / num;
					int num180 = num174 - num201;
					if ((uint)(num180 - 1) > 1u && (uint)(num180 - 4) > 1u && num201 != 0)
					{
						NPC.frame.Y = 0;
						NPC.frameCounter = 0.0;
					}
					int num202 = 0;
					if (NPC.frameCounter < 16.0)
					{
						num202 = 0;
					}
					else if (NPC.frameCounter == 16.0 && Main.netMode != 1)
					{
						EmoteBubble.NewBubbleNPC(new WorldUIAnchor((Entity)NPC), 112);
					}
					else if (NPC.frameCounter < 128.0)
					{
						num202 = ((NPC.frameCounter % 16.0 < 8.0) ? (num174 - 2) : 0);
					}
					else if (NPC.frameCounter < 160.0)
					{
						num202 = 0;
					}
					else if (NPC.frameCounter != 160.0 || Main.netMode == 1)
					{
						num202 = ((NPC.frameCounter < 220.0) ? ((NPC.frameCounter % 12.0 < 6.0) ? (num174 - 2) : 0) : 0);
					}
					else
					{
						EmoteBubble.NewBubbleNPC(new WorldUIAnchor((Entity)NPC), 60);
					}
					NPC.frame.Y = num * num202;
					if (NPC.frameCounter >= 220.0)
					{
						NPC.frameCounter = 0.0;
					}
				}
				else if (NPC.ai[0] == 9f)
				{
					NPC.frameCounter += 1.0;
					int num204 = NPC.frame.Y / num;
					int num179 = num174 - num204;
					if ((uint)(num179 - 1) > 1u && (uint)(num179 - 4) > 1u && num204 != 0)
					{
						NPC.frame.Y = 0;
						NPC.frameCounter = 0.0;
					}
					int num205 = 0;
					num205 = ((!(NPC.frameCounter < 10.0)) ? ((!(NPC.frameCounter < 16.0)) ? (num174 - 4) : (num174 - 5)) : 0);
					if (NPC.ai[1] < 16f)
					{
						num205 = num174 - 5;
					}
					if (NPC.ai[1] < 10f)
					{
						num205 = 0;
					}
					NPC.frame.Y = num * num205;
				}
				else if (NPC.ai[0] == 18f)
				{
					NPC.frameCounter += 1.0;
					int num206 = NPC.frame.Y / num;
					int num178 = num174 - num206;
					if ((uint)(num178 - 1) > 1u && (uint)(num178 - 4) > 1u && num206 != 0)
					{
						NPC.frame.Y = 0;
						NPC.frameCounter = 0.0;
					}
					int num207 = Main.npcFrameCount[Type] - 2;
					NPC.frame.Y = num * num207;
				}
				else if (NPC.ai[0] == 10f || NPC.ai[0] == 13f)
				{
					NPC.frameCounter += 1.0;
					int num208 = NPC.frame.Y / num;
					if ((uint)(num208 - num174) > 3u && num208 != 0)
					{
						NPC.frame.Y = 0;
						NPC.frameCounter = 0.0;
					}
					int num209 = 10;
					int num210 = 6;
					int num211 = 0;
					num211 = ((!(NPC.frameCounter < (double)num209)) ? ((NPC.frameCounter < (double)(num209 + num210)) ? num174 : ((NPC.frameCounter < (double)(num209 + num210 * 2)) ? (num174 + 1) : ((NPC.frameCounter < (double)(num209 + num210 * 3)) ? (num174 + 2) : ((NPC.frameCounter < (double)(num209 + num210 * 4)) ? (num174 + 3) : 0)))) : 0);
					NPC.frame.Y = num * num211;
				}
				else if (NPC.ai[0] == 15f)
				{
					NPC.frameCounter += 1.0;
					int num212 = NPC.frame.Y / num;
					if ((uint)(num212 - num174) > 3u && num212 != 0)
					{
						NPC.frame.Y = 0;
						NPC.frameCounter = 0.0;
					}
					float num213 = NPC.ai[1] / (float)NPCID.Sets.AttackTime[Type];
					int num3 = 0;
					num3 = ((num213 > 0.65f) ? num174 : ((num213 > 0.5f) ? (num174 + 1) : ((num213 > 0.35f) ? (num174 + 2) : ((num213 > 0f) ? (num174 + 3) : 0))));
					NPC.frame.Y = num * num3;
				}
				else if (NPC.ai[0] == 12f)
				{
					NPC.frameCounter += 1.0;
					int num4 = NPC.frame.Y / num;
					if ((uint)(num4 - num174) > 4u && num4 != 0)
					{
						NPC.frame.Y = 0;
						NPC.frameCounter = 0.0;
					}
					int num5 = num174 + NPC.GetShootingFrame(NPC.ai[2]);
					NPC.frame.Y = num * num5;
				}
				else if (NPC.ai[0] == 14f)
				{
					NPC.frameCounter += 1.0;
					int num6 = NPC.frame.Y / num;
					if ((uint)(num6 - num174) > 1u && num6 != 0)
					{
						NPC.frame.Y = 0;
						NPC.frameCounter = 0.0;
					}
					int num7 = 12;
					int num8 = ((NPC.frameCounter % (double)num7 * 2.0 < (double)num7) ? num174 : (num174 + 1));
					NPC.frame.Y = num * num8;
				}
				else if (NPC.ai[0] == 1001f)
				{
					NPC.frame.Y = num * (num174 - 1);
					NPC.frameCounter = 0.0;
				}
				else if (NPC.CanTalk && (NPC.ai[0] == 3f || NPC.ai[0] == 4f))
				{
					NPC.frameCounter += 1.0;
					int num9 = NPC.frame.Y / num;
					int num177 = num174 - num9;
					if ((uint)(num177 - 1) > 1u && (uint)(num177 - 4) > 1u && num9 != 0)
					{
						NPC.frame.Y = 0;
						NPC.frameCounter = 0.0;
					}
					bool flag7 = NPC.ai[0] == 3f;
					int num10 = 0;
					int num11 = 0;
					int num12 = -1;
					int num14 = -1;
					if (NPC.frameCounter < 10.0)
					{
						num10 = 0;
					}
					else if (NPC.frameCounter < 16.0)
					{
						num10 = num174 - 5;
					}
					else if (NPC.frameCounter < 46.0)
					{
						num10 = num174 - 4;
					}
					else if (NPC.frameCounter < 60.0)
					{
						num10 = num174 - 5;
					}
					else if (NPC.frameCounter < 216.0)
					{
						num10 = 0;
					}
					else if (NPC.frameCounter == 216.0 && Main.netMode != 1)
					{
						num12 = 70;
					}
					else if (NPC.frameCounter < 286.0)
					{
						num10 = ((NPC.frameCounter % 12.0 < 6.0) ? (num174 - 2) : 0);
					}
					else if (NPC.frameCounter < 320.0)
					{
						num10 = 0;
					}
					else if (NPC.frameCounter != 320.0 || Main.netMode == 1)
					{
						num10 = ((NPC.frameCounter < 420.0) ? ((NPC.frameCounter % 16.0 < 8.0) ? (num174 - 2) : 0) : 0);
					}
					else
					{
						num12 = 100;
					}
					if (NPC.frameCounter < 70.0)
					{
						num11 = 0;
					}
					else if (NPC.frameCounter != 70.0 || Main.netMode == 1)
					{
						num11 = ((!(NPC.frameCounter < 160.0)) ? ((NPC.frameCounter < 166.0) ? (num174 - 5) : ((NPC.frameCounter < 186.0) ? (num174 - 4) : ((NPC.frameCounter < 200.0) ? (num174 - 5) : ((!(NPC.frameCounter < 320.0)) ? ((NPC.frameCounter < 326.0) ? (num174 - 1) : 0) : 0)))) : ((NPC.frameCounter % 16.0 < 8.0) ? (num174 - 2) : 0));
					}
					else
					{
						num14 = 90;
					}
					if (flag7)
					{
						NPC nPC = Main.npc[(int)NPC.ai[2]];
						if (num12 != -1)
						{
							EmoteBubble.NewBubbleNPC(new WorldUIAnchor((Entity)NPC), num12, new WorldUIAnchor((Entity)nPC));
						}
						if (num14 != -1 && nPC.CanTalk)
						{
							EmoteBubble.NewBubbleNPC(new WorldUIAnchor((Entity)nPC), num14, new WorldUIAnchor((Entity)NPC));
						}
					}
					NPC.frame.Y = num * (flag7 ? num10 : num11);
					if (NPC.frameCounter >= 420.0)
					{
						NPC.frameCounter = 0.0;
					}
				}
				else if (NPC.CanTalk && (NPC.ai[0] == 16f || NPC.ai[0] == 17f))
				{
					NPC.frameCounter += 1.0;
					int num15 = NPC.frame.Y / num;
					int num176 = num174 - num15;
					if ((uint)(num176 - 1) > 1u && (uint)(num176 - 4) > 1u && num15 != 0)
					{
						NPC.frame.Y = 0;
						NPC.frameCounter = 0.0;
					}
					bool flag8 = NPC.ai[0] == 16f;
					int num16 = 0;
					int num17 = -1;
					if (NPC.frameCounter < 10.0)
					{
						num16 = 0;
					}
					else if (NPC.frameCounter < 16.0)
					{
						num16 = num174 - 5;
					}
					else if (NPC.frameCounter < 22.0)
					{
						num16 = num174 - 4;
					}
					else if (NPC.frameCounter < 28.0)
					{
						num16 = num174 - 5;
					}
					else if (NPC.frameCounter < 34.0)
					{
						num16 = num174 - 4;
					}
					else if (NPC.frameCounter < 40.0)
					{
						num16 = num174 - 5;
					}
					else if (NPC.frameCounter == 40.0 && Main.netMode != 1)
					{
						num17 = 45;
					}
					else if (NPC.frameCounter < 70.0)
					{
						num16 = num174 - 4;
					}
					else if (NPC.frameCounter < 76.0)
					{
						num16 = num174 - 5;
					}
					else if (NPC.frameCounter < 82.0)
					{
						num16 = num174 - 4;
					}
					else if (NPC.frameCounter < 88.0)
					{
						num16 = num174 - 5;
					}
					else if (NPC.frameCounter < 94.0)
					{
						num16 = num174 - 4;
					}
					else if (NPC.frameCounter < 100.0)
					{
						num16 = num174 - 5;
					}
					else if (NPC.frameCounter == 100.0 && Main.netMode != 1)
					{
						num17 = 45;
					}
					else if (NPC.frameCounter < 130.0)
					{
						num16 = num174 - 4;
					}
					else if (NPC.frameCounter < 136.0)
					{
						num16 = num174 - 5;
					}
					else if (NPC.frameCounter < 142.0)
					{
						num16 = num174 - 4;
					}
					else if (NPC.frameCounter < 148.0)
					{
						num16 = num174 - 5;
					}
					else if (NPC.frameCounter < 154.0)
					{
						num16 = num174 - 4;
					}
					else if (NPC.frameCounter < 160.0)
					{
						num16 = num174 - 5;
					}
					else if (NPC.frameCounter != 160.0 || Main.netMode == 1)
					{
						num16 = ((NPC.frameCounter < 220.0) ? (num174 - 4) : ((NPC.frameCounter < 226.0) ? (num174 - 5) : 0));
					}
					else
					{
						num17 = 75;
					}
					if (flag8 && num17 != -1)
					{
						int num18 = (int)NPC.localAI[2];
						int num19 = (int)NPC.localAI[3];
						int num20 = (int)Main.npc[(int)NPC.ai[2]].localAI[3];
						int num21 = (int)Main.npc[(int)NPC.ai[2]].localAI[2];
						int num22 = 3 - num18 - num19;
						int num23 = 0;
						if (NPC.frameCounter == 40.0)
						{
							num23 = 1;
						}
						if (NPC.frameCounter == 100.0)
						{
							num23 = 2;
						}
						if (NPC.frameCounter == 160.0)
						{
							num23 = 3;
						}
						int num25 = 3 - num23;
						int num26 = -1;
						int num27 = 0;
						while (num26 < 0)
						{
							num176 = num27 + 1;
							num27 = num176;
							if (num176 >= 100)
							{
								break;
							}
							num26 = Main.rand.Next(2);
							if (num26 == 0 && num21 >= num19)
							{
								num26 = -1;
							}
							if (num26 == 1 && num20 >= num18)
							{
								num26 = -1;
							}
							if (num26 == -1 && num25 <= num22)
							{
								num26 = 2;
							}
						}
						if (num26 == 0)
						{
							Main.npc[(int)NPC.ai[2]].localAI[3] += 1f;
							num20++;
						}
						if (num26 == 1)
						{
							Main.npc[(int)NPC.ai[2]].localAI[2] += 1f;
							num21++;
						}
						UnifiedRandom rand = Main.rand;
						int[] array = new int[] { 36, 37, 38 };
						int num28 = Utils.SelectRandom(rand, array);
						int num29 = num28;
						switch (num26)
						{
							case 0:
								switch (num28)
								{
									case 38:
										num29 = 37;
										break;
									case 37:
										num29 = 36;
										break;
									case 36:
										num29 = 38;
										break;
								}
								break;
							case 1:
								switch (num28)
								{
									case 38:
										num29 = 36;
										break;
									case 37:
										num29 = 38;
										break;
									case 36:
										num29 = 37;
										break;
								}
								break;
						}
						if (num25 == 0)
						{
							if (num20 >= 2)
							{
								num28 -= 3;
							}
							if (num21 >= 2)
							{
								num29 -= 3;
							}
						}
						EmoteBubble.NewBubble(num28, new WorldUIAnchor((Entity)NPC), num17);
						EmoteBubble.NewBubble(num29, new WorldUIAnchor((Entity)Main.npc[(int)NPC.ai[2]]), num17);
					}
					NPC.frame.Y = num * (flag8 ? num16 : num16);
					if (NPC.frameCounter >= 420.0)
					{
						NPC.frameCounter = 0.0;
					}
				}
				else if (NPC.velocity.X == 0f)
				{
					NPC.frame.Y = 0;
					NPC.frameCounter = 0.0;
				}
				else
				{
					int num31 = 6;
					NPC.frameCounter += Math.Abs(NPC.velocity.X) * 2f;
					NPC.frameCounter += 1.0;
					int num32 = num * 2;
					if (NPC.frame.Y < num32)
					{
						NPC.frame.Y = num32;
					}
					if (NPC.frameCounter > (double)num31)
					{
						NPC.frame.Y += num;
						NPC.frameCounter = 0.0;
					}
					if (NPC.frame.Y / num >= Main.npcFrameCount[Type] - num173)
					{
						NPC.frame.Y = num32;
					}
				}
			}
			else
			{
				NPC.frameCounter = 0.0;
				NPC.frame.Y = num;
			}
		}
    }
}

