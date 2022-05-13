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
using System.Collections.Generic;
using Polarities.Effects;

namespace Polarities.NPCs.ConvectiveWanderer
{
	public class ConvectiveWanderer : ModNPC
	{
        /*public override void Load()
		{
			IL.Terraria.Main.UpdateMenu += Main_UpdateMenu;
		}

		private void Main_UpdateMenu(MonoMod.Cil.ILContext il)
		{
			MonoMod.Cil.ILCursor c = new MonoMod.Cil.ILCursor(il);

			c.EmitDelegate<Action>(() =>
			{
				if (!(bool)(typeof(ModLoader).GetField("isLoading", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null)))
				{
					String filePath = Main.SavePath + Path.DirectorySeparatorChar + "ConvectiveWanderer.png";

					if (!File.Exists(filePath))
					{
						Main.spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Matrix.CreateTranslation(0f - Main.GameViewMatrix.Translation.X, 0f - Main.GameViewMatrix.Translation.Y, 0f) * Matrix.CreateScale(0.5f, 0.5f, 1f));

						const int extraWidth = 2400;

						var capture = new RenderTarget2D(Main.spriteBatch.GraphicsDevice, Main.screenWidth + extraWidth, Main.screenHeight, false, Main.spriteBatch.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
						var capture2 = new RenderTarget2D(Main.spriteBatch.GraphicsDevice, Main.screenWidth + extraWidth, Main.screenHeight, false, Main.spriteBatch.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

						Main.spriteBatch.GraphicsDevice.SetRenderTarget(capture);
						Main.spriteBatch.GraphicsDevice.Clear(Color.Transparent);

						NPC me = new NPC();
						me.SetDefaults(NPCType<ConvectiveWanderer>());
						me.IsABestiaryIconDummy = false;
						me.Center = new Vector2(extraWidth / 2, 0);
						me.rotation = 0f;

						(me.ModNPC as ConvectiveWanderer).segmentPositions[0] = me.Center;
						for (int i = 1; i < (me.ModNPC as ConvectiveWanderer).segmentPositions.Length; i++)
						{
							//TODO: Set segmnt angles and pulse scales for this when supported
							float rotationOffset = (float)Math.Cos(i * MathHelper.TwoPi / ((me.ModNPC as ConvectiveWanderer).segmentPositions.Length - 16));
							(me.ModNPC as ConvectiveWanderer).segmentPositions[i] = (me.ModNPC as ConvectiveWanderer).segmentPositions[i - 1] - new Vector2(segmentSeparation, 0).RotatedBy(me.rotation + rotationOffset);
						}

						me.ModNPC.PreDraw(Main.spriteBatch, -capture.Size() / 2, Color.White);

						Main.spriteBatch.End();
						Main.spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);

						Main.spriteBatch.GraphicsDevice.SetRenderTarget(capture2);
						Main.spriteBatch.GraphicsDevice.Clear(Color.Transparent);

						for (int i = 0; i < 4; i++)
						{
							Main.spriteBatch.Draw(capture, Main.GameViewMatrix.Translation + new Vector2(2f, 0).RotatedBy(i * MathHelper.PiOver2), null, new Color(64, 64, 64), 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
						}
						Main.spriteBatch.Draw(capture, Main.GameViewMatrix.Translation, null, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);

						Main.spriteBatch.End();
						Main.spriteBatch.GraphicsDevice.SetRenderTarget(null);


						var stream = File.Create(filePath);
						capture2.SaveAsPng(stream, capture2.Width, capture2.Height);
						stream.Dispose();
						capture2.Dispose();
					}
				}
			});
		}*/

        public override void SetStaticDefaults()
		{
			NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
			{
				SpecificallyImmuneTo = new int[] {
					BuffID.Confused,
					BuffID.OnFire,
					BuffID.Frostburn,
					BuffID.OnFire3,
					BuffID.ShadowFlame,
					BuffID.CursedInferno,
					//BuffType<Incinerating>()
				}
			};
			NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheUnderworld,
				//flavor text
				this.TranslatedBestiaryEntry()
			});
		}

		public override void SetDefaults()
		{
			NPC.aiStyle = -1;

			NPC.width = 72;
			NPC.height = 72;

			NPC.defense = 25;
			NPC.damage = 150;
			NPC.knockBackResist = 0f;
			NPC.lifeMax = Main.masterMode ? 168000 / 3 : Main.expertMode ? 136000 / 2 : 100000;
			NPC.noTileCollide = true;
			NPC.noGravity = true;
			NPC.lavaImmune = true;
			NPC.npcSlots = 1f;
			NPC.boss = true;
			NPC.behindTiles = false;
			NPC.value = Item.sellPrice(gold: 15);
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;

			NPC.hide = true;

			SpawnModBiomes = new int[1] { GetInstance<LavaOcean>().Type };

			numSegments = 40;
			segmentPositions = new Vector2[numSegments * segmentsPerHitbox + segmentsTailTendrils + 1];

			numTentacleSegments = NUM_TENTACLES * HITBOXES_PER_TENTACLE;

			NPC.GetGlobalNPC<MultiHitboxNPC>().useMultipleHitboxes = true;
			NPC.GetGlobalNPC<MultiHitboxNPC>().hitboxes = new Rectangle[numTentacleSegments + numSegments + 1]; //extra segment for the head

			//changes hit sounds to indicate damage reduction
			NPC.GetGlobalNPC<MultiHitboxNPC>().SegmentUpdate = (index) =>
			{
				if (index < numTentacleSegments)
				{
					//hitting tentacle segments is bad
					NPC.HitSound = new LegacySoundStyle(SoundID.Tink, 0);
					NPC.takenDamageMultiplier = 0.5f;
				}
				else if (index < numTentacleSegments + numSegments)
				{
					//hitting body segments is meh
					NPC.HitSound = SoundID.NPCHit2;
					NPC.takenDamageMultiplier = 1f;
				}
				else
				{
					//hitting the head is great
					NPC.HitSound = SoundID.NPCHit1;
					NPC.takenDamageMultiplier = 2f;
				}
			};

			drawDatas = new PriorityQueue<DrawData, float>(MAX_DRAW_CAPACITY);
		}

		int numSegments;
		int numTentacleSegments;

		const int HITBOXES_PER_TENTACLE = 16;

		const int segmentsPerHitbox = 4;
		const int specialSegmentsHeadMultiplier = 8;
		const int specialSegmentsHead = 8 * specialSegmentsHeadMultiplier - 2;
		const int hitboxSegmentOffset = 2;
		const int segmentsTail = 8;
		const int segmentsTailTendrils = 8;
		const float segmentSeparation = 16f;
		private Vector2[] segmentPositions;

        public override void OnSpawn(IEntitySource source)
		{
			Player player = Main.player[NPC.target];
			if (!player.active || player.dead)
			{
				NPC.TargetClosest(false);
				player = Main.player[NPC.target];
			}

			NPC.rotation = (player.Center - NPC.Center).ToRotation();

			segmentPositions[0] = NPC.Center;
			for (int i = 1; i < segmentPositions.Length; i++)
			{
				segmentPositions[i] = segmentPositions[i - 1] - new Vector2(NPC.width, 0).RotatedBy(NPC.rotation);
			}
		}

        public override void AI()
		{
			Player player = Main.player[NPC.target];
			if (!player.active || player.dead)
			{
				NPC.TargetClosest(false);
				player = Main.player[NPC.target];
			}

			//changeable ai values
			float rotationFade = 9f;
			float rotationAmount = 0.01f;

			NPC.noGravity = true;

			//TODO: Actual AI
			NPC.velocity = (player.Center - NPC.Center) / 120f;


			NPC.noGravity = true;
			NPC.rotation = NPC.velocity.ToRotation();

			//update segment positions
			segmentPositions[0] = NPC.Center + NPC.velocity;
			Vector2 rotationGoal = Vector2.Zero;

			for (int i = 1; i < segmentPositions.Length; i++)
			{
				if (i > 1)
				{
					rotationGoal = ((rotationFade - 1) * rotationGoal + (segmentPositions[i - 1] - segmentPositions[i - 2])) / rotationFade;
				}

				segmentPositions[i] = segmentPositions[i - 1] + (rotationAmount * rotationGoal + (segmentPositions[i] - segmentPositions[i - 1]).SafeNormalize(Vector2.Zero)).SafeNormalize(Vector2.Zero) * segmentSeparation;
			}

			//position hitbox segments
			//the order in which we do this matters as it determines hit priority
			//doing the tentacles first ensures the tentacles always shield the head

			//position tentacle hitbox segments
			float tentacleBaseAngle = SegmentAngle(TENTACLE_ATTACH_SEGMENT_INDEX);
			float tentacleBaseRotation = SegmentRotation(TENTACLE_ATTACH_SEGMENT_INDEX) + MathHelper.PiOver2;
			float tentacleBaseRadius = Radius(TENTACLE_ATTACH_SEGMENT_INDEX) + TentacleRadius(0);
			Vector2 tentacleBasePosition = SegmentPosition(TENTACLE_ATTACH_SEGMENT_INDEX);

			for (int tentacleIndex = 0; tentacleIndex < NUM_TENTACLES; tentacleIndex++)
			{
				for (int segmentIndex = 0; segmentIndex < HITBOXES_PER_TENTACLE; segmentIndex++)
				{
					int hitboxIndex = tentacleIndex * HITBOXES_PER_TENTACLE + segmentIndex;

					float indexForDrawing = segmentIndex * TENTACLE_SEGMENTS / (float)HITBOXES_PER_TENTACLE;

					Vector2 spot = TentacleSegmentPosition(indexForDrawing, tentacleBaseAngle + tentacleIndex * MathHelper.TwoPi / NUM_TENTACLES, tentacleBaseRotation, tentacleBaseRadius, tentacleBasePosition);
					float radius = TentacleRadius(indexForDrawing);

					NPC.GetGlobalNPC<MultiHitboxNPC>().hitboxes[hitboxIndex] = new Rectangle((int)(spot.X - radius), (int)(spot.Y - radius), (int)(radius * 2), (int)(radius * 2));
				}
			}

			//position body and head segments
			for (int h = 0; h < numSegments + 1; h++)
			{
				int hitboxIndex = h + numTentacleSegments;

				Vector2 spot = h == numSegments ?
					 SegmentPosition(-(segmentsPerHitbox - hitboxSegmentOffset) * specialSegmentsHeadMultiplier) : //head segment
					 segmentPositions[h * segmentsPerHitbox + hitboxSegmentOffset]; //body/tail segment
				NPC.GetGlobalNPC<MultiHitboxNPC>().hitboxes[hitboxIndex] = new Rectangle((int)spot.X - NPC.width / 2, (int)spot.Y - NPC.height / 2, NPC.width, NPC.height);
			}
		}

		public override bool CheckDead()
		{
			for (int i = 0; i < NPC.GetGlobalNPC<MultiHitboxNPC>().hitboxes.Length; i++)
			{
				Vector2 gorePos = NPC.GetGlobalNPC<MultiHitboxNPC>().hitboxes[i].TopLeft();
				if (i == 0)
				{
					Gore.NewGore(NPC.GetSource_Death(), gorePos, Vector2.Zero, Mod.Find<ModGore>("ConvectiveWandererGore1").Type);
				}
				else if (i == NPC.GetGlobalNPC<MultiHitboxNPC>().hitboxes.Length - 1)
				{

					Gore.NewGore(NPC.GetSource_Death(), gorePos, Vector2.Zero, Mod.Find<ModGore>("ConvectiveWandererGore3").Type);
				}
				else
				{
					Gore.NewGore(NPC.GetSource_Death(), gorePos, Vector2.Zero, Mod.Find<ModGore>("ConvectiveWandererGore2").Type);
				}
			}
			return true;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			//TODO:
		}




		//a whole bunch of drawing stuff and helper methods
		//abandon all hope ye who enter here

		//a PriorityQueue that stores our drawData
		PriorityQueue<DrawData, float> drawDatas;

		//The maximum capacity potentially required by drawDatas
		const int MAX_DRAW_CAPACITY = 15248;

		const int NUM_TENTACLES = 8;

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (NPC.IsABestiaryIconDummy)
			{
				//TODO: Bestiary portrait
				return false;
			}

			//TODO: Use a rendertarget to allow additive drawing in front of lava without having to do the whole thing again
			drawDatas.Clear();

			//register body data
			for (int i = segmentPositions.Length - 1; i > -specialSegmentsHead; i--)
			{
				DrawSegment(drawDatas, spriteBatch, screenPos, i, NPC.GetNPCColorTintedByBuffs(Color.White));
			}

			for (int i = 0; i < NUM_TENTACLES; i++)
            {
				DrawTentacle(drawDatas, spriteBatch, screenPos, i * MathHelper.TwoPi / NUM_TENTACLES, NPC.GetNPCColorTintedByBuffs(Color.White));
            }

			if (drawDatas.Count > MAX_DRAW_CAPACITY) Main.NewText("drawData capacity exceeded: " + drawDatas.Count + " > " + MAX_DRAW_CAPACITY); //for finding max capacity used

			//draw all data
			while (drawDatas.TryDequeue(out var drawData, out _))
			{
				drawData.Draw(spriteBatch);
			}

			return false;
		}

		const int BASE_TEXTURE_HEIGHT = 32;
		const int SIDES_PER_SEGMENT = 8;
		const int DRAWS_PER_SIDE = 2;
		const int DRAWS_PER_SEGMENT = SIDES_PER_SEGMENT * DRAWS_PER_SIDE;

		int SegmentWidth(int index)
        {
			return (index < 1) ? (int)(segmentSeparation / specialSegmentsHeadMultiplier) : (int)segmentSeparation;
		}
		Vector2 BaseSegmentPosition(int index)
		{
			if (index < 0)
			{
				return segmentPositions[0] + new Vector2(-SegmentWidth(index) * index, 0).RotatedBy(SegmentRotation(1));
			}
			else
			{
				return segmentPositions[index];
			}
		}
		Vector2 SegmentPosition(int index)
		{
			return (BaseSegmentPosition(index) + BaseSegmentPosition(index - 1)) / 2;
		}
		float SegmentRotation(int index)
		{
			return (BaseSegmentPosition(index - 1) - BaseSegmentPosition(index)).ToRotation();
		}

		//TODO: Dynamic rotation and pulsing instead of preset values
		//TODO: Enable opening/closing mouth via PulseScale
		float SegmentAngle(int index)
		{
			if (index < 1) index = 1;
			return 0f; //(index - 1) * 0.01f + Main.GlobalTimeWrappedHourly;
		}
		float PulseScale(int index)
		{
			//restrict pulsing for head segments
			float headRestrictionMultiplier = index < 1 ?
				(index + specialSegmentsHead) / (float)(specialSegmentsHead + 1)
				: 1f;
			float extraPulseScale = 0f; //(float)Math.Sin((index - 1) * 0.1f - Main.GlobalTimeWrappedHourly * 4f) * 0.1f * headRestrictionMultiplier
			return 1 + extraPulseScale;
		}

		float TrueBaseScale(int index)
		{
			if (index >= segmentPositions.Length - segmentsTailTendrils) return 0f;
			return index >= (segmentPositions.Length - segmentsTailTendrils - segmentsTail) ?
				(float)Math.Sqrt(1 - Math.Pow((segmentPositions.Length - segmentsTailTendrils - index) / (float)segmentsTail - 1, 2)) :
				index < 1 ?
				(float)Math.Sqrt(1 - Math.Pow((index + specialSegmentsHead) / (float)specialSegmentsHead - 1, 2)) :
				1f;
		}
		float BaseScale(int index)
		{
			return TrueBaseScale(index) * PulseScale(index);
		}
		float Radius(int index)
		{
			return BASE_TEXTURE_HEIGHT / MathHelper.TwoPi * SIDES_PER_SEGMENT * BaseScale(index);
		}

		void DrawSegment(PriorityQueue<DrawData, float> drawDatas, SpriteBatch spriteBatch, Vector2 screenPos, int index, Color color)
		{
			//TODO: Give tail doubled-up segments for a more continuous appearance, since it changes width
			Vector2 segmentPosition = SegmentPosition(index);

			//don't draw if offscreen
			int buffer = 80;
			if (!spriteBatch.GraphicsDevice.ScissorRectangle.Intersects(new Rectangle((int)(segmentPosition - screenPos).X - buffer, (int)(segmentPosition - screenPos).Y - buffer, buffer * 2, buffer * 2)))
			{
				return;
			}

			//draw head in more detail due to lack of obscuring bristles
			int segmentWidth = SegmentWidth(index);
			int drawWidthPerSegment = segmentWidth * 2;

			int TendrilIndex(int index)
			{
				int indexInTailTendrils = (index - (segmentPositions.Length - segmentsTailTendrils - 1)) % segmentsTailTendrils;
				if (indexInTailTendrils < 0) indexInTailTendrils += segmentsTailTendrils;
				return indexInTailTendrils;
			}
			float TendrilRadius(int index, int tendrilIndex)
			{
				return (float)Math.Sqrt(tendrilIndex) * 16f * PulseScale(index) + Radius(index - tendrilIndex + 1);
			}
			int GetSegmentFrontPoint(int index)
			{
				return index < 1 ?
					(192 - drawWidthPerSegment) - segmentWidth * (index + specialSegmentsHead - 1) : //head
					(64 - drawWidthPerSegment) - segmentWidth * (index - 1) % 32; //body and tail
			}

			float globalDepthModifier = index * 16f;
			if (index < 1) globalDepthModifier -= specialSegmentsHead * 1024f;

			//base segments
			if (index < segmentPositions.Length - segmentsTailTendrils)
			{ 
				float segmentRotation = SegmentRotation(index);

				float segmentAngle = SegmentAngle(index);
				float segmentPulseScale = PulseScale(index);

				int segmentFramePoint = GetSegmentFrontPoint(index);

				float radius = Radius(index);

				float scaleMultToMatch = (float)Math.Tan(MathHelper.Pi / DRAWS_PER_SEGMENT) * radius;

				for (int i = 0; i < SIDES_PER_SEGMENT; i++)
				{
					for (int j = 0; j < DRAWS_PER_SIDE; j++)
					{
						float totalAngle = segmentAngle + i * MathHelper.TwoPi / SIDES_PER_SEGMENT + j * MathHelper.TwoPi / DRAWS_PER_SEGMENT;

						float offsetDist = (float)Math.Sin(totalAngle) * radius;
						Vector2 sectionOffset = new Vector2(0, offsetDist).RotatedBy(segmentRotation);

						Vector2 scale = new Vector2(1, (float)Math.Cos(totalAngle) * scaleMultToMatch / (BASE_TEXTURE_HEIGHT / DRAWS_PER_SIDE) * 2);

						float depthColorModifier = ((float)Math.Cos(totalAngle) + 2) / 3;
						Color depthModifiedColor = color.MultiplyRGB(new Color(new Vector3(depthColorModifier)));

						SpriteEffects bodyEffects = SpriteEffects.None;

						//allow drawing of backwards segments if in the head, but prune otherwise for efficiency since non-head reversed segments are obscured
						if (index < 1 && scale.Y < 0)
						{
							bodyEffects = SpriteEffects.FlipVertically;
							scale.Y = -scale.Y;
						}

						if (scale.Y > 0)
						{
							Rectangle frame = new Rectangle(segmentFramePoint, j * BASE_TEXTURE_HEIGHT / DRAWS_PER_SIDE, drawWidthPerSegment, BASE_TEXTURE_HEIGHT / DRAWS_PER_SIDE);
							Vector2 origin = frame.Size() / 2;

							drawDatas.Enqueue(new DrawData(TextureAssets.Npc[Type].Value, segmentPosition + sectionOffset - screenPos, frame, depthModifiedColor, segmentRotation, origin, scale, bodyEffects, 0), (float)Math.Cos(totalAngle) - globalDepthModifier);
						}

						for (int finIndex = -1; finIndex <= 1; finIndex += 2)
						{
							//fins are rotated and don't scale with extra pulsing
							float offsetAngle = totalAngle + finIndex * MathHelper.Pi / 4;

							Vector2 finScale = new Vector2(1, (float)Math.Abs(Math.Sin(offsetAngle)) * scaleMultToMatch / (BASE_TEXTURE_HEIGHT / DRAWS_PER_SIDE) * 2 / segmentPulseScale);

							Rectangle finFrame = new Rectangle(segmentFramePoint, (BASE_TEXTURE_HEIGHT + 2) + j * BASE_TEXTURE_HEIGHT / DRAWS_PER_SIDE, drawWidthPerSegment, BASE_TEXTURE_HEIGHT / DRAWS_PER_SIDE);
							Vector2 finOrigin = new Vector2(finFrame.Width / 2, 0);

							SpriteEffects finEffects = SpriteEffects.None;
							if (Math.Sin(offsetAngle) < 0)
							{
								finEffects = SpriteEffects.FlipVertically;
								finOrigin = new Vector2(finFrame.Width / 2, finFrame.Height);
							}

							drawDatas.Enqueue(new DrawData(TextureAssets.Npc[Type].Value, segmentPosition + sectionOffset - screenPos, finFrame, depthModifiedColor, segmentRotation, finOrigin, finScale, finEffects, 0), (float)Math.Cos(totalAngle) * 2f - globalDepthModifier);

							if (index < 1)
                            {
								//extra inner fins to make the teeth look thicker

								SpriteEffects innerFinEffects = SpriteEffects.FlipVertically;
								Vector2 innerFinOrigin = new Vector2(finFrame.Width / 2, finFrame.Height);
								if (Math.Sin(offsetAngle) < 0)
								{
									innerFinEffects = SpriteEffects.None;
									innerFinOrigin = new Vector2(finFrame.Width / 2, 0);
								}
								Rectangle innerFinFrame = new Rectangle(segmentFramePoint, 2 * (BASE_TEXTURE_HEIGHT + 2) + j * BASE_TEXTURE_HEIGHT / DRAWS_PER_SIDE, drawWidthPerSegment, BASE_TEXTURE_HEIGHT / DRAWS_PER_SIDE);
								drawDatas.Enqueue(new DrawData(TextureAssets.Npc[Type].Value, segmentPosition + sectionOffset - screenPos, innerFinFrame, depthModifiedColor, segmentRotation, innerFinOrigin, finScale, innerFinEffects, 0), (float)Math.Cos(totalAngle) * 0.5f - globalDepthModifier);
							}
						}
					}
				}
			}

			//tendril 'skirts'
			//TODO: make gradient more continuous with a custom sprite a la denizen's telegraphs
			if (index >= 0 && index < segmentPositions.Length - 1)
			{
				int numTendrilSidesPerSegment = index < segmentPositions.Length - segmentsTailTendrils ? 16 : 8;

				Vector2 pos0 = SegmentPosition(index + 1);
				Vector2 pos1 = SegmentPosition(index);
				Vector2 pos2 = SegmentPosition(index - 1);

				float rot0 = SegmentRotation(index + 1);
				float rot1 = SegmentRotation(index);
				float rot2 = SegmentRotation(index - 1);

				int tendrilIndex = TendrilIndex(index);

				float tendrilsRadius0 = TendrilRadius(index + 1, tendrilIndex + 1);
				float tendrilsRadius1 = TendrilRadius(index, tendrilIndex);
				float tendrilsRadius2 = TendrilRadius(index - 1, tendrilIndex - 1);

				float tendrilProgress = (1 - tendrilIndex / (float)segmentsTailTendrils);

				float baseAngle0 = SegmentAngle(index + 1) + MathHelper.Pi / numTendrilSidesPerSegment;
				float baseAngle1 = SegmentAngle(index) + MathHelper.Pi / numTendrilSidesPerSegment;
				float baseAngle2 = SegmentAngle(index - 1) + MathHelper.Pi / numTendrilSidesPerSegment;

				for (int i = 0; i < numTendrilSidesPerSegment; i++)
				{
					float totalAngle0 = baseAngle0 + i * MathHelper.TwoPi / numTendrilSidesPerSegment;
					float totalAngle1 = baseAngle1 + i * MathHelper.TwoPi / numTendrilSidesPerSegment;
					float totalAngle2 = baseAngle2 + i * MathHelper.TwoPi / numTendrilSidesPerSegment;

					float offsetDist0 = (float)Math.Sin(totalAngle0) * tendrilsRadius0;
					float offsetDist1 = (float)Math.Sin(totalAngle1) * tendrilsRadius1;
					float offsetDist2 = (float)Math.Sin(totalAngle2) * tendrilsRadius2;

					Vector2 tendrilPos0 = pos0 + new Vector2(0, offsetDist0).RotatedBy(rot0);
					Vector2 tendrilPos1 = pos1 + new Vector2(0, offsetDist1).RotatedBy(rot1);
					Vector2 tendrilPos2 = pos2 + new Vector2(0, offsetDist2).RotatedBy(rot2);

					Vector2 startPosition = (tendrilPos0 + tendrilPos1) / 2;
					Vector2 endPosition = (tendrilPos1 + tendrilPos2) / 2;

					float depthColorModifier = ((float)Math.Cos(totalAngle1) + 3) / 4;
					Color tendrilColor = ModUtils.ConvectiveFlameColor(tendrilProgress * tendrilProgress / 2f).MultiplyRGB(new Color(new Vector3(depthColorModifier))) * tendrilProgress;
					float tendrilWidth = tendrilProgress * 2f + 2f;

					drawDatas.Enqueue(new DrawData(Textures.PixelTexture.Value, startPosition - screenPos, Textures.PixelTexture.Frame(), color.MultiplyRGBA(tendrilColor), (endPosition - startPosition).ToRotation(), new Vector2(0, 0.5f), new Vector2((endPosition - startPosition).Length(), tendrilWidth), SpriteEffects.None, 0), (float)Math.Cos(totalAngle1) * 64f - globalDepthModifier);
				}
			}
		}

		const int TENTACLE_SEGMENTS = 32;
		const int TENTACLE_HEAD_SEGMENTS = 8;
		const int TENTACLE_SEGMENT_SEPARATION = 8;

		const int TENTACLE_ATTACH_SEGMENT_INDEX = -4;

		void DrawTentacle(PriorityQueue<DrawData, float> drawDatas, SpriteBatch spriteBatch, Vector2 screenPos, float angleOffset, Color color)
		{
			Vector2 segmentPosition = SegmentPosition(TENTACLE_ATTACH_SEGMENT_INDEX);
			float segmentRotation = SegmentRotation(TENTACLE_ATTACH_SEGMENT_INDEX);
			float radius = Radius(TENTACLE_ATTACH_SEGMENT_INDEX);
			float segmentAngle = SegmentAngle(TENTACLE_ATTACH_SEGMENT_INDEX);
			float totalAngle = segmentAngle + angleOffset;

			for (int i = -TENTACLE_HEAD_SEGMENTS; i < TENTACLE_SEGMENTS; i++)
            {
				DrawTentacleSegment(drawDatas, spriteBatch, screenPos, totalAngle, segmentRotation + MathHelper.PiOver2, segmentPosition, radius + TentacleRadius(0), i, color);
            }
		}

		const int BASE_TENTACLE_TEXTURE_HEIGHT = 32;
		const int SIDES_PER_TENTACLE_SEGMENT = 4;
		const int DRAWS_PER_TENTACLE_SIDE = 2;
		const int DRAWS_PER_TENTACLE_SEGMENT = SIDES_PER_TENTACLE_SEGMENT * DRAWS_PER_TENTACLE_SIDE;

		const float TENTACLE_HEAD_SEPARATION_SCALE_MULT = 4;

		const float TENTACLE_POSITION_INDEX_OFFSET = TENTACLE_HEAD_SEGMENTS / TENTACLE_HEAD_SEPARATION_SCALE_MULT + 2;
		const float EFFECTIVE_TENTACLE_SEGMENTS = TENTACLE_SEGMENTS + TENTACLE_POSITION_INDEX_OFFSET;

		//a whole bunch of tentacle-related methods
		int TentacleSegmentWidth(float index)
		{
			return (index > 0) ? TENTACLE_SEGMENT_SEPARATION : (int)(TENTACLE_SEGMENT_SEPARATION / TENTACLE_HEAD_SEPARATION_SCALE_MULT);
		}
		float TentacleBaseAngleOffset(float index)
		{
			return (index <= 0) ? 0 :
				index * 0.1f; //* (float)Math.Cos(Main.GlobalTimeWrappedHourly * 0.5f);
		}
		Vector2 TentacleEffectiveBasePosition(float index, float baseAngle, float baseRotation, float baseRadius, Vector2 originalBasePosition)
		{
			float offsetDistForEffectiveBasePosition = (float)Math.Sin(baseAngle + TentacleBaseAngleOffset(index)) * baseRadius;
			return originalBasePosition + new Vector2(0, offsetDistForEffectiveBasePosition).RotatedBy(baseRotation - MathHelper.PiOver2);
		}
		Vector2 TentacleRadialOffset(float index, float baseRadius)
		{
			//baseRadius / Sin(angle) = BigLength
			//DistTraveled = (indexAsFloat * TENTACLE_SEGMENT_SEPARATION)
			//(BigLength - DistTraveled) * Sin(angle) = TentacleRadius(index);

			//BigLength = TentacleRadius(index) / Sin(angle) + DistTraveled
			//Sin(angle) = (baseRadius - TentacleRadius(index)) / DistTraveled

			float indexAsFloat = index;
			if (indexAsFloat <= 0) indexAsFloat /= TENTACLE_HEAD_SEPARATION_SCALE_MULT;

			float DistTraveled = indexAsFloat * TENTACLE_SEGMENT_SEPARATION;

			float sideScaleMult = (float)Math.Sin(MathHelper.TwoPi / NUM_TENTACLES) / 2f;

			float goalRadius = TentacleRadius(index) / sideScaleMult; //ModUtils.Lerp(baseRadius, TentacleRadius(index) / sideScaleMult, (float)(1 + Math.Cos(Main.GlobalTimeWrappedHourly * 1f)) / 2f);

			float segmentSqueezing = (float)(1 - Math.Pow(1 - index / EFFECTIVE_TENTACLE_SEGMENTS, 8f));
			float distFromCenter = ModUtils.Lerp(baseRadius, goalRadius, segmentSqueezing);

			float discriminant = DistTraveled * DistTraveled - (baseRadius - distFromCenter) * (baseRadius - distFromCenter);
			float absThing = discriminant > 0 ? -(float)Math.Sqrt(discriminant) : (float)Math.Sqrt(-discriminant);

			float tentacleRotation = 0f; //2.5f * (float)Math.Pow((1 - Math.Cos(Main.GlobalTimeWrappedHourly * 1f)) / 2f, (index / EFFECTIVE_TENTACLE_SEGMENTS + 0.5f) * 4f);

			return new Vector2(distFromCenter - baseRadius, absThing).RotatedBy(tentacleRotation);

			//this would be for a straight line to the meeting point
			//float goalForAsin = baseRadius / (TENTACLE_SEGMENTS * TENTACLE_SEGMENT_SEPARATION);

			//float angle = 3 * MathHelper.PiOver2 - (float)Math.Asin(goalForAsin);
			//return new Vector2(DistTraveled, 0).RotatedBy(angle);
		}
		Vector2 TentacleOffsetUsingAngle(float index, float baseAngle, float baseRadius)
		{
			return new Vector2((float)Math.Sin(baseAngle + TentacleBaseAngleOffset(index)), 1) * TentacleRadialOffset(index, baseRadius);
		}
		Vector2 TentacleSegmentPosition(float index, float baseAngle, float baseRotation, float baseRadius, Vector2 originalBasePosition)
		{
			float effectiveIndex = index + TENTACLE_POSITION_INDEX_OFFSET;
			if (index >= -TENTACLE_POSITION_INDEX_OFFSET)
			{
				return TentacleEffectiveBasePosition(effectiveIndex, baseAngle, baseRotation, baseRadius, originalBasePosition) + TentacleOffsetUsingAngle(effectiveIndex, baseAngle, baseRadius).RotatedBy(baseRotation);
			}
			else
			{
				return TentacleSegmentPosition(-TENTACLE_POSITION_INDEX_OFFSET, baseAngle, baseRotation, baseRadius, originalBasePosition) + new Vector2(-TentacleSegmentWidth(effectiveIndex) * effectiveIndex, 0).RotatedBy(TentacleSegmentRotation(1 - TENTACLE_POSITION_INDEX_OFFSET, baseAngle, baseRotation, baseRadius, originalBasePosition));
			}
		}
		float TentacleSegmentRotation(float index, float baseAngle, float baseRotation, float baseRadius, Vector2 originalBasePosition)
		{
			return (TentacleSegmentPosition(index - 1, baseAngle, baseRotation, baseRadius, originalBasePosition) - TentacleSegmentPosition(index, baseAngle, baseRotation, baseRadius, originalBasePosition)).ToRotation();
		}
		float TentacleRadiusMult(float index)
		{
			return (index >= 0) ?
			ModUtils.Lerp((float)Math.Sqrt(1 - Math.Pow(index / EFFECTIVE_TENTACLE_SEGMENTS, 2)), (1 - index / EFFECTIVE_TENTACLE_SEGMENTS), 0.75f) :
			(float)Math.Sqrt(1 - Math.Pow((index + TENTACLE_HEAD_SEGMENTS) / TENTACLE_HEAD_SEGMENTS - 1, 2));
		}
		float TentacleRadius(float index)
		{
			return BASE_TENTACLE_TEXTURE_HEIGHT / DRAWS_PER_TENTACLE_SIDE * MathHelper.TwoPi / DRAWS_PER_TENTACLE_SEGMENT * TentacleRadiusMult(index);
		}

		void DrawTentacleSegment(PriorityQueue<DrawData, float> drawDatas, SpriteBatch spriteBatch, Vector2 screenPos, float baseAngle, float baseRotation, Vector2 originalBasePosition, float baseRadius, int index, Color color)
		{
			Vector2 segmentPosition = TentacleSegmentPosition(index, baseAngle, baseRotation, baseRadius, originalBasePosition);

			//don't draw if offscreen
			int buffer = 80;
			if (!spriteBatch.GraphicsDevice.ScissorRectangle.Intersects(new Rectangle((int)(segmentPosition - screenPos).X - buffer, (int)(segmentPosition - screenPos).Y - buffer, buffer * 2, buffer * 2)))
			{
				return;
			}

			int segmentWidth = TentacleSegmentWidth(index);
			int drawWidthPerSegment = segmentWidth * 2;

			float segmentRotation = (TentacleSegmentPosition(index - 1, baseAngle, baseRotation, baseRadius, originalBasePosition) - segmentPosition).ToRotation();
			float segmentAngle = -baseAngle - TentacleBaseAngleOffset(index);

			float segmentRadius = TentacleRadius(index);
			float scaleMultToMatch = (float)Math.Tan(MathHelper.Pi / DRAWS_PER_TENTACLE_SEGMENT) * segmentRadius;

			int segmentFramePoint = (288 - drawWidthPerSegment) - (segmentWidth * (index - 1) % 64 + 64) % 64;

			float generalDepthFromAngle = (float)Math.Cos(baseAngle + TentacleBaseAngleOffset(index));
			float segmentDepthModifier = (generalDepthFromAngle + 0.5f) * (specialSegmentsHead * 65536f + index) + specialSegmentsHead * 512f;
			
			for (int i = 0; i < SIDES_PER_TENTACLE_SEGMENT; i++)
            {
				for (int j = 0; j < DRAWS_PER_TENTACLE_SIDE; j++)
				{
					float totalAngle = segmentAngle + i * MathHelper.TwoPi / SIDES_PER_TENTACLE_SEGMENT + j * MathHelper.TwoPi / DRAWS_PER_TENTACLE_SEGMENT;

					float offsetDist = (float)Math.Sin(totalAngle) * segmentRadius;
					Vector2 sectionOffset = new Vector2(0, offsetDist).RotatedBy(segmentRotation);

					Vector2 scale = new Vector2(1, (float)Math.Cos(totalAngle) * scaleMultToMatch / (BASE_TENTACLE_TEXTURE_HEIGHT / DRAWS_PER_TENTACLE_SIDE) * 2);

					float depthColorModifier = (((float)Math.Cos(totalAngle) + 2) / 3) * ((generalDepthFromAngle + 2) / 3);
					Color depthModifiedColor = color.MultiplyRGB(new Color(new Vector3(depthColorModifier)));

					SpriteEffects bodyEffects = SpriteEffects.None;

					if (scale.Y > 0)
					{
						Rectangle frame = new Rectangle(segmentFramePoint, j * BASE_TENTACLE_TEXTURE_HEIGHT / DRAWS_PER_TENTACLE_SIDE, TENTACLE_SEGMENT_SEPARATION * 2, BASE_TENTACLE_TEXTURE_HEIGHT / DRAWS_PER_TENTACLE_SIDE);
						Vector2 origin = frame.Size() / 2;

						drawDatas.Enqueue(new DrawData(TextureAssets.Npc[Type].Value, segmentPosition + sectionOffset - screenPos, frame, depthModifiedColor, segmentRotation, origin, scale, bodyEffects, 0), (float)Math.Cos(totalAngle) * 0.1f + segmentDepthModifier);
					}
				}
			}
		}

        public override void DrawBehind(int index)
        {
            RenderTargetLayer.AddNPC<ConvectiveWandererTarget>(index);
        }
    }
}