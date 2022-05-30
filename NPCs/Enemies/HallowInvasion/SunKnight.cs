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
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Polarities.Biomes;
using Terraria.Audio;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using static Terraria.Graphics.VertexStrip;
using Polarities.Items.Weapons.Melee;
using Polarities.Items.Mounts;
using Terraria.Localization;

namespace Polarities.NPCs.Enemies.HallowInvasion
{
	public class SunKnight : ModNPC
	{
		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[NPC.type] = 9;

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
			NPC.lifeMax = 900;
			NPC.knockBackResist = 0f;
			NPC.npcSlots = 1f;
			NPC.noGravity = false;
			NPC.noTileCollide = true;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath7;
			NPC.value = Item.buyPrice(silver: 25);

			Music = GetInstance<Biomes.HallowInvasion>().Music;
			SceneEffectPriority = SceneEffectPriority.Event;

			Banner = Type;
			BannerItem = ItemType<SunKnightBanner>();

			SpawnModBiomes = new int[1] { GetInstance<Biomes.HallowInvasion>().Type };
		}

		private Vector2 telegraphPosition;

		public override void AI()
		{
			if (NPC.localAI[0] == 0)
			{
				NPC.noGravity = true;

				//possibly spawn pegasus
				if (Biomes.HallowInvasion.GetSpawnChance(6) > Math.Pow(Main.rand.NextFloat(1f), 2))
				{
					int mount = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<Pegasus>());
					NPC.localAI[1] = mount;
					NPC.localAI[2] = 1;
					Main.npc[mount].ai[2] = NPC.whoAmI + 1;
				}

				NPC.localAI[0] = 1;
			}

			NPC.TargetClosest(true);
			Player player = Main.player[NPC.target];

			//non-riding AI
			if (NPC.localAI[2] == 0)
			{
				if (!PolaritiesSystem.hallowInvasion)
				{
					//run away if not in the invasion
					NPC.noTileCollide = true;
					NPC.noGravity = false;
					return;
				}

				bool groundFlag = false;

				int animSpeed = 5;

				if (NPC.velocity.Y == 0)
				{
					//grounded
					if (NPC.ai[0] == 30)
					{
						NPC.frame.Y = 0 * 64;
						NPC.frameCounter = 0;
					}

					NPC.ai[0]++;
					groundFlag = true;

					telegraphPosition = Vector2.Zero;

					if (NPC.ai[0] == 120)
					{
						//jump at player
						NPC.spriteDirection = NPC.direction;

						float jumpHeight = 15;
						float yVelToJumpToAbovePlayer = (NPC.Center.Y - player.Center.Y > 0) ? -(float)Math.Sqrt((NPC.Center.Y - player.Center.Y) / 2.4f + jumpHeight * jumpHeight) : -jumpHeight;

						NPC.velocity = new Vector2(NPC.direction * Math.Min(12, Math.Abs(Main.rand.Next(2, 4) * (player.Center.X - NPC.Center.X) / 150f)), yVelToJumpToAbovePlayer);

						//set goal height
						NPC.ai[2] = player.position.Y;

						//create position telegraph somehow
						Vector2 virtualPosition = NPC.position;
						Vector2 virtualVelocity = NPC.velocity;
						for (int i = 0; i < 1000; i++)
						{
							virtualVelocity.Y += 0.3f;

							if (virtualPosition.Y + NPC.height > NPC.ai[2])
							{
								if (virtualVelocity.Y > 0 && Collision.TileCollision(virtualPosition, new Vector2(0, virtualVelocity.Y), NPC.width, NPC.height) != new Vector2(0, virtualVelocity.Y))
								{
									virtualVelocity = Collision.TileCollision(virtualPosition, new Vector2(0, virtualVelocity.Y), NPC.width, NPC.height);

									virtualPosition += virtualVelocity;

									//set total expected time
									NPC.ai[3] = i;

									break;
								}
							}

							virtualPosition += virtualVelocity;
						}

						telegraphPosition = virtualPosition + NPC.Center - NPC.position;

						//reset timers and stuff going into being in the air
						NPC.ai[0] = 0;
					}
				}
				else
				{
					if (NPC.ai[0] == 0)
					{
						NPC.frame.Y = 5 * 64;
					}

					//not grounded
					NPC.ai[0]++;

					if (NPC.ai[0] == NPC.ai[3] - 60)
					{
						if (Main.netMode != 1)
						{
							//create awesome anime lightsword of awesomeness™
							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<SunKnightLightsword>(), 30, 4f, Main.myPlayer, ai0: NPC.whoAmI, ai1: NPC.spriteDirection);

							//below half health, create a second awesome anime lightsword of awesomeness™
							if (NPC.life * 2 < NPC.lifeMax)
							{
								Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<SunKnightLightsword>(), 30, 4f, Main.myPlayer, ai0: NPC.whoAmI, ai1: -NPC.spriteDirection);
							}
						}

						NPC.frame.Y = 5 * 64;
						NPC.frameCounter = animSpeed * 4;
					}
				}

				NPC.frameCounter--;
				if (NPC.frameCounter % animSpeed == 0 && NPC.frameCounter > 0)
				{
					NPC.frame.Y += 64;
				}

				NPC.velocity.Y += 0.3f;

				if (NPC.position.Y + NPC.height > NPC.ai[2])
				{
					if (NPC.velocity.Y > 0 && Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height) != new Vector2(0, NPC.velocity.Y))
					{
						NPC.position += Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height);
						NPC.velocity = Vector2.Zero;

						if (!groundFlag)
						{
							MakeDusts();

							//reset our timer on landing
							NPC.ai[0] = 0;

							//frame collapse on landing
							NPC.frame.Y -= 4 * 64;
						}
					}
				}
			}
			else
			{
				NPC pegasus = Main.npc[(int)NPC.localAI[1]];

				if (!pegasus.active)
				{
					NPC.localAI[2] = 0;
					return;
				}

				NPC.Center = pegasus.Center + new Vector2(0, -12);
				NPC.velocity = pegasus.velocity;
				NPC.spriteDirection = -pegasus.spriteDirection;
				NPC.frame.Y = 0;

				//only activate the sword if not running away
				if (PolaritiesSystem.hallowInvasion)
				{
					if (NPC.localAI[3] == 0 && pegasus.ai[0] == 1 && (player.Center.X - NPC.Center.X) / NPC.velocity.X > 55 && (player.Center.X - NPC.Center.X) / NPC.velocity.X < 60)
					{
						NPC.localAI[3] = 1;
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<SunKnightLightsword>(), 30, 4f, Main.myPlayer, ai0: NPC.whoAmI, ai1: NPC.spriteDirection);

						if (NPC.life * 2 < NPC.lifeMax)
						{
							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<SunKnightLightsword>(), 30, 4f, Main.myPlayer, ai0: NPC.whoAmI, ai1: -NPC.spriteDirection);
						}
					}
					else if (pegasus.ai[0] == 0)
					{
						NPC.localAI[3] = 0;
					}
				}
			}
		}

		public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if (NPC.localAI[2] != 0)
			{
				damage /= 2;
			}
		}

		public override void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit)
		{
			if (NPC.localAI[2] != 0)
			{
				damage /= 2;
			}
		}

		private void MakeDusts()
		{
			SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/NPC_Death_14")
			{
				Volume = 0.75f,
			}, NPC.Center);

			for (int num231 = 0; num231 < 20; num231++)
			{
				int num217 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y + NPC.height * 7 / 8), NPC.width, NPC.height / 8, DustID.Smoke, 0f, 0f, 100, default(Color), 1.5f);
				Dust dust71 = Main.dust[num217];
				Dust dust362 = dust71;
				dust362.velocity *= 1.4f;
			}
			Vector2 position67 = new Vector2(NPC.position.X + Main.rand.Next(0, NPC.width), NPC.position.Y + NPC.height);
			Vector2 val = default(Vector2);
			int num229 = Gore.NewGore(NPC.GetSource_FromAI(), position67, val, Main.rand.Next(61, 64));
			Gore gore20 = Main.gore[num229];
			Gore gore76 = gore20;
			gore76.velocity *= 0.4f;
			Main.gore[num229].velocity.X += 1f;
			Main.gore[num229].velocity.Y += 1f;
			Vector2 position68 = new Vector2(NPC.position.X + Main.rand.Next(0, NPC.width), NPC.position.Y + NPC.height);
			val = default(Vector2);
			num229 = Gore.NewGore(NPC.GetSource_FromAI(), position68, val, Main.rand.Next(61, 64));
			gore20 = Main.gore[num229];
			gore76 = gore20;
			gore76.velocity *= 0.4f;
			Main.gore[num229].velocity.X -= 1f;
			Main.gore[num229].velocity.Y += 1f;
			Vector2 position69 = new Vector2(NPC.position.X + Main.rand.Next(0, NPC.width), NPC.position.Y + NPC.height);
			val = default(Vector2);
			num229 = Gore.NewGore(NPC.GetSource_FromAI(), position69, val, Main.rand.Next(61, 64));
			gore20 = Main.gore[num229];
			gore76 = gore20;
			gore76.velocity *= 0.4f;
			Main.gore[num229].velocity.X += 1f;
			Main.gore[num229].velocity.Y -= 1f;
			Vector2 position70 = new Vector2(NPC.position.X + Main.rand.Next(0, NPC.width), NPC.position.Y + NPC.height);
			val = default(Vector2);
			num229 = Gore.NewGore(NPC.GetSource_FromAI(), position70, val, Main.rand.Next(61, 64));
			gore20 = Main.gore[num229];
			gore76 = gore20;
			gore76.velocity *= 0.4f;
			Main.gore[num229].velocity.X -= 1f;
			Main.gore[num229].velocity.Y -= 1f;
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
			if (telegraphPosition != Vector2.Zero)
			{
				Texture2D telegraphTexture = TelegraphTexture.Value;
				Rectangle telegraphFrame = telegraphTexture.Frame();

				float timeAlpha = NPC.ai[0] >= NPC.ai[3] ? 0 : 1 - NPC.ai[0] / NPC.ai[3];

				Color mainColor = new Color(255, 245, 168);
				Color endColor = new Color(255, 0, 0);
				Color swordColor = Color.Lerp(mainColor, endColor, ((float)Math.Sin(Main.GlobalTimeWrappedHourly * 12f) + 1) / 4f);

				spriteBatch.Draw(telegraphTexture, telegraphPosition + new Vector2(0, DrawOffsetY) - screenPos, telegraphFrame, swordColor * timeAlpha, NPC.rotation, telegraphFrame.Size() / 2, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);

				for (int i = 0; i < 4; i++)
				{
					Vector2 randomOffset = new Vector2(2, 0).RotatedByRandom(MathHelper.TwoPi);
					spriteBatch.Draw(telegraphTexture, telegraphPosition + randomOffset + new Vector2(0, DrawOffsetY) - screenPos, telegraphFrame, Color.White * (0.25f * timeAlpha), NPC.rotation, telegraphFrame.Size() / 2, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
				}
			}

			Texture2D texture = TextureAssets.Npc[Type].Value;
			Rectangle frame = NPC.frame;

			spriteBatch.Draw(texture, NPC.Center + new Vector2(0, DrawOffsetY) - screenPos, frame, NPC.GetNPCColorTintedByBuffs(Color.White), NPC.rotation, frame.Size() / 2, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
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
			npcLoot.Add(ItemDropRule.Common(ItemType<Sunblade>(), 8));
			npcLoot.Add(ItemDropRule.ByCondition(new SunKnightMountDropCondition(), ItemType<PegasusMountItem>(), 2));
			npcLoot.Add(ItemDropRule.Common(ItemID.HallowedBar, 2));
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			//only spawns during the hallow event
			if (spawnInfo.Player.InModBiome(GetInstance<Biomes.HallowInvasion>()))
			{
				return Biomes.HallowInvasion.GetSpawnChance(4);
			}
			return 0f;
		}
	}

	public class SunKnightLightsword : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("{$Mods.Polarites.ItemName.Sunblade}");

			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;

			/*Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 274, 34, false, SurfaceFormat.Color);
			System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
			for (int i = 0; i < texture.Height; i++)
			{
				for (int j = 0; j < texture.Width; j++)
				{
					float x = (2 * i / (float)(texture.Height - 1) - 1);
					float y = (2 * j / (float)(texture.Width - 1) - 1);

					float distanceSquared = x * x + y * y;
					float theta = new Vector2(x, y).ToRotation();

					int r = 255;
					int g = 255;
					int b = 255;
					int alpha = distanceSquared >= 1 ? 0 : (int)(255 * (1 - distanceSquared * distanceSquared));

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "SunKnightLightswordGlow.png", FileMode.Create), texture.Width, texture.Height);*/
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 2;
			Projectile.height = 2;

			Projectile.timeLeft = 120;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			if (!Main.npc[(int)Projectile.ai[0]].active)
			{
				Projectile.Kill();
				return;
			}
			else
			{
				Projectile.Center = Main.npc[(int)Projectile.ai[0]].Center;
			}

			if (Projectile.localAI[0] == 0)
			{
				Projectile.localAI[0] = 1;

				if (Projectile.ai[1] == 1)
				{
					Projectile.rotation = MathHelper.Pi;
				}

				Projectile.spriteDirection = Main.npc[(int)Projectile.ai[0]].spriteDirection;

				for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
					Projectile.oldPos[i] = Projectile.position;
					Projectile.oldRot[i] = Projectile.rotation;
				}
			}

			Projectile.scale = Projectile.timeLeft * (120 - Projectile.timeLeft) / 3600f;
			Projectile.rotation += 4 * Projectile.scale * Projectile.spriteDirection * MathHelper.TwoPi / 120f;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + new Vector2(257 * Projectile.scale, 0).RotatedBy(Projectile.rotation));
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Color mainColor = new Color(255, 245, 168);
			Color endColor = new Color(255, 0, 0);
			Color swordColor = Color.Lerp(mainColor, endColor, ((float)Math.Sin(Main.GlobalTimeWrappedHourly * 12f) + 1) / 4f);

			float trailWidth = 266 * Projectile.scale;

			MiscShaderData miscShaderData = GameShaders.Misc["FinalFractal"];
			int num = 1;
			int num2 = 0;
			int num3 = 0;
			int num4 = 1;
			miscShaderData.UseShaderSpecificData(new Vector4(num, num2, num3, num4));
			miscShaderData.UseImage0("Images/Extra_" + 201);
			miscShaderData.UseImage1("Images/Extra_" + 195);
			miscShaderData.Apply();

			Vector2 previousInnerPoint = Projectile.Center;
			Vector2[] oldCenters = new Vector2[Projectile.oldPos.Length];
			float[] oldRotInverses = new float[Projectile.oldRot.Length];
			for (int i = 0; i < oldCenters.Length; i++)
            {
				Vector2 arcPoint = Projectile.oldPos[i] + Projectile.Center - Projectile.position + new Vector2(trailWidth, 0).RotatedBy(Projectile.oldRot[i]);
				Vector2 innerPoint = arcPoint + (previousInnerPoint - arcPoint).SafeNormalize(Vector2.Zero) * trailWidth;

				oldCenters[i] = (arcPoint + innerPoint) / 2;
				oldRotInverses[i] = (arcPoint - innerPoint).ToRotation() + MathHelper.PiOver2;

				previousInnerPoint = innerPoint;
			}

			VertexStrip vertexStrip = new VertexStrip();

			Color StripColors(float progressOnStrip)
            {
				Color result = Color.Lerp(mainColor, endColor, progressOnStrip) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip));
				result.A /= 2;
				return result;
            }

			float StripWidth(float progressOnStrip)
            {
				return trailWidth / 2f;
            }

			vertexStrip.PrepareStrip(oldCenters, oldRotInverses, StripColors, StripWidth, -Main.screenPosition + Projectile.Size / 2f, Projectile.oldPos.Length, includeBacksides: true);
			vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();

			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame();
			Vector2 drawCenter = new Vector2(5, 17);

			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, swordColor, Projectile.rotation, drawCenter, Projectile.scale, SpriteEffects.None, 0);
			return false;
		}

        private FinalFractalHelper.FinalFractalProfile GetFinalFractalProfile(int v)
        {
            throw new NotImplementedException();
        }

        public override bool ShouldUpdatePosition()
		{
			return false;
		}
	}

	class SunKnightMountDropCondition : IItemDropRuleCondition
	{
		public bool CanDrop(DropAttemptInfo info)
		{
			return info.npc.localAI[2] != 0;
		}

		public bool CanShowItemDropInUI()
		{
			return true;
		}

		public string GetConditionDescription()
		{
			return Language.GetTextValue("Mods.Polarities.DropConditions.SunKnightMount");
		}
	}
}