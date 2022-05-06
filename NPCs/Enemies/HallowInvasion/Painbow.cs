using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Polarities.Items.Placeable.Banners;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Polarities.Biomes;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Polarities.Effects;
using Terraria.GameContent.ItemDropRules;
using Polarities.Items.Weapons.Ranged;

namespace Polarities.NPCs.Enemies.HallowInvasion
{
	public class Painbow : ModNPC
	{
		public override string Texture => "Terraria/Images/Projectile_644";

		public override void SetStaticDefaults()
		{
			NPCID.Sets.TrailCacheLength[NPC.type] = 11;
			NPCID.Sets.TrailingMode[NPC.type] = 3;

			NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
			{
				SpecificallyImmuneTo = new int[] {
					BuffID.Confused,
					BuffID.OnFire
				}
			};
			NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);

			PolaritiesNPC.hallowInvasionNPC.Add(Type);

			/*Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 58, 58, false, SurfaceFormat.Color);
			System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
			for (int i = 0; i < texture.Width; i++)
			{
				for (int j = 0; j < texture.Height; j++)
				{
					float x = (2 * i / (float)(texture.Width - 1) - 1);
					float y = (2 * j / (float)(texture.Width - 1) - 1);

					float distanceSquared = x * x + y * y;
					float theta = new Vector2(x, y).ToRotation();

					int r = 255;
					int g = 255;
					int b = 255;
					int alpha = distanceSquared >= 1 ? 0 : (int)(255 * Math.Exp(1 + 1 / (distanceSquared - 1)));

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "PainbowGlow.png", FileMode.Create), texture.Width, texture.Height);*/
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
			NPC.width = 58;
			NPC.height = 58;

			NPC.defense = 30;
			NPC.damage = 50;
			NPC.lifeMax = 1500;
			NPC.knockBackResist = 0f;
			NPC.npcSlots = 1f;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.HitSound = SoundID.NPCHit5;
			NPC.DeathSound = SoundID.NPCDeath7;
			NPC.value = Item.buyPrice(silver: 25);

			Music = GetInstance<Biomes.HallowInvasion>().Music;
			SceneEffectPriority = SceneEffectPriority.Event;

			Banner = Type;
			BannerItem = ItemType<PainbowBanner>();

			SpawnModBiomes = new int[1] { GetInstance<Biomes.HallowInvasion>().Type };
		}

		public override void AI()
		{
			Lighting.AddLight(NPC.Center, 1f, 1f, 1f);

			NPC.TargetClosest(false);
			Player player = Main.player[NPC.target];

			if (!PolaritiesSystem.hallowInvasion)
			{
				//flee
				if (NPC.ai[0] == 0)
				{
					NPC.ai[0] = -1;
				}
			}

			switch (NPC.ai[0])
			{
				case 0:
					//idle behavior, try to stay a fixed distance from the player
					Vector2 goalPosition = player.Center + (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * 400f;
					Vector2 goalVelocity = (goalPosition - NPC.Center) / 20f;
					NPC.velocity += (goalVelocity - NPC.velocity) / 20f;

					NPC.rotation += NPC.velocity.X * 0.02f;

					if (NPC.ai[1] == 180 && (player.Center - NPC.Center).Length() < 600f)
					{
						//just started attack, modify timers for other viable painbows
						for (int i = 0; i < Main.maxNPCs; i++)
						{
							if (i != NPC.whoAmI && Main.npc[i].active && Main.npc[i].type == NPC.type && Main.npc[i].ai[0] == 0 && Main.npc[i].ai[1] <= 180)
							{
								Main.npc[i].ai[1] = 180;
							}
						}
					}

					NPC.ai[1]++;
					if (NPC.ai[1] > 180 && (player.Center - NPC.Center).Length() < 600f)
					{
						NPC.ai[1] = 0;
						NPC.ai[0] = Main.rand.Next(1, 3);
						NPC.netUpdate = true;
					}
					break;
				case 1:
					//charge
					if (NPC.ai[1] == 0)
					{
						NPC.direction = player.Center.X > NPC.Center.X ? 1 : -1;
					}
					if (NPC.ai[1] == 90)
					{
						NPC.velocity = (player.Center - NPC.Center) / 24f;
					}

					if (NPC.ai[1] < 90)
					{
						NPC.velocity *= 0.95f;
						NPC.rotation += NPC.direction * 0.6f * NPC.ai[1] / 90f;
					}
					else
					{
						NPC.rotation += NPC.direction * 0.2f;
					}

					NPC.ai[1]++;
					if (NPC.ai[1] == 120)
					{
						NPC.ai[1] = 0;
						NPC.ai[0] = 0;
					}
					break;
				case 2:
					if (NPC.ai[1] == 0)
					{
						NPC.direction = player.Center.X > NPC.Center.X ? 1 : -1;
					}

					if (NPC.ai[1] < 120)
					{
						if (NPC.ai[1] < 30)
							NPC.rotation += NPC.direction * 0.2f * (30 - NPC.ai[1]) / 30f;

						NPC.velocity *= 0.95f;
						NPC.ai[2] = -5 * (float)Math.Sin(NPC.ai[1] / 120 * MathHelper.Pi);
					}
					else
					{
						NPC.ai[2] = 128 * (float)Math.Sin((NPC.ai[1] - 120) / 30 * MathHelper.Pi);
					}
					if (NPC.ai[1] == 120 && Main.netMode != 1)
					{
						for (int i = 0; i < 3; i++)
						{
							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.UnitY.RotatedBy(NPC.rotation + i * MathHelper.Pi / 3), ProjectileType<PainbowHitbox>(), 25, 0f, Main.myPlayer, ai0: NPC.whoAmI);
						}
					}

					NPC.ai[1]++;
					if (NPC.ai[1] == 150)
					{
						NPC.ai[1] = 0;
						NPC.ai[0] = 0;
						NPC.ai[2] = 0;
					}
					break;
				case -1:
					goalPosition = NPC.Center + (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * 400f;
					goalVelocity = (goalPosition - NPC.Center) / 20f;
					NPC.velocity += (goalVelocity - NPC.velocity) / 20f;

					NPC.rotation += NPC.velocity.X * 0.02f;
					break;
			}

			NPC.localAI[0] += 0.05f;
		}

        public override void FindFrame(int frameHeight)
        {
            if (NPC.IsABestiaryIconDummy)
			{
				NPC.localAI[0] += 0.05f;
			}
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
		{
			//telegraphs
			if (!DrawLayer.IsActive<DrawLayerAdditiveAfterNPCs>())
			{
				switch (NPC.ai[0])
				{
					case 1:
						//dash
						if (NPC.ai[1] <= 90)
						{
							Player player = Main.player[NPC.target];

							float alphaModifier = (NPC.ai[1] < 54 ? 1 : (float)(Math.Cos(NPC.ai[1] * MathHelper.TwoPi / 12f) + 1.5f) / 2.5f);
							float alpha = (NPC.ai[1] / 180f) * alphaModifier;

							spriteBatch.Draw(Textures.PixelTexture.Value, NPC.Center - screenPos, new Rectangle(0, 0, 1, 1), Color.White * alpha, (player.Center - NPC.Center).ToRotation(), new Vector2(0, 0.5f), new Vector2(2000, 1), SpriteEffects.None, 0f);
						}
						break;
					case 2:
						//rays
						if (NPC.ai[1] >= 30 && NPC.ai[1] < 120)
						{
							float alphaModifier = (NPC.ai[1] < 84 ? 1 : (float)(Math.Cos(NPC.ai[1] * MathHelper.TwoPi / 12f) + 1.5f) / 2.5f);
							float alpha = ((NPC.ai[1] - 30) / 180f) * alphaModifier;

							for (int i = 0; i < 3; i++)
							{
								spriteBatch.Draw(Textures.PixelTexture.Value, NPC.Center - screenPos, new Rectangle(0, 0, 1, 1), Color.White * alpha, NPC.rotation + i * MathHelper.Pi / 3f, new Vector2(0.5f, 0.5f), new Vector2(1, 4000), SpriteEffects.None, 0f);
							}
						}
						break;
				}
			}

			if (!NPC.IsABestiaryIconDummy)
			{
				float alphaMult = DrawLayer.IsActive<DrawLayerAdditiveAfterNPCs>() ? 0.25f : 0.75f;
				MainDraw(spriteBatch, screenPos, lightColor, alphaMult);
			}
			else
            {
				MainDraw(spriteBatch, screenPos, lightColor, 0.75f);

				NPC.SetBlendState(spriteBatch, BlendState.Additive);

				MainDraw(spriteBatch, screenPos, lightColor, 0.25f);

				NPC.SetBlendState(spriteBatch, BlendState.AlphaBlend);
			}

			return false;
		}

		void MainDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor, float alphaMult)
		{
			Texture2D texture = TextureAssets.Npc[Type].Value;
			Rectangle frame = texture.Frame();

			spriteBatch.Draw(texture, NPC.Center - screenPos, frame, Color.White * alphaMult, NPC.localAI[0], frame.Size() / 2, new Vector2(0.5f, 3), SpriteEffects.None, 0f);
			spriteBatch.Draw(texture, NPC.Center - screenPos, frame, Color.White * alphaMult, -NPC.localAI[0], frame.Size() / 2, new Vector2(0.5f, 3), SpriteEffects.None, 0f);
			spriteBatch.Draw(texture, NPC.Center - screenPos, frame, Color.White * alphaMult, NPC.localAI[0] + MathHelper.PiOver2, frame.Size() / 2, new Vector2(0.5f, 3), SpriteEffects.None, 0f);
			spriteBatch.Draw(texture, NPC.Center - screenPos, frame, Color.White * alphaMult, -NPC.localAI[0] + MathHelper.PiOver2, frame.Size() / 2, new Vector2(0.5f, 3), SpriteEffects.None, 0f);

			DrawLightTentacle(spriteBatch, screenPos, Color.Red * alphaMult, 0f);
			DrawLightTentacle(spriteBatch, screenPos, Color.OrangeRed * alphaMult, MathHelper.Pi / 3);
			DrawLightTentacle(spriteBatch, screenPos, Color.Yellow * alphaMult, 2 * MathHelper.Pi / 3);
			DrawLightTentacle(spriteBatch, screenPos, Color.Green * alphaMult, 3 * MathHelper.Pi / 3);
			DrawLightTentacle(spriteBatch, screenPos, Color.Blue * alphaMult, 4 * MathHelper.Pi / 3);
			DrawLightTentacle(spriteBatch, screenPos, Color.Purple * alphaMult, 5 * MathHelper.Pi / 3);

			Texture2D texture2 = Textures.Glow58.Value;
			Rectangle frame2 = texture2.Frame();

			spriteBatch.Draw(texture2, NPC.Center - screenPos, frame2, Color.White * (alphaMult * 2f), 0f, frame2.Size() / 2, 1f, SpriteEffects.None, 0f);
		}

		private void DrawLightTentacle(SpriteBatch spriteBatch, Vector2 screenPos, Color color, float rotation)
		{
			Texture2D texture = TextureAssets.Npc[Type].Value;
			Rectangle frame = texture.Frame();

			for (int i = 0; i < NPC.oldPos.Length - 1; i++)
			{
				float drawRotation = MathHelper.PiOver2 + (NPC.oldPos[i] - NPC.oldPos[i + 1] + new Vector2(0, 32 + (6 + NPC.ai[2]) * i).RotatedBy(NPC.oldRot[i] + rotation) - new Vector2(0, 32 + (6 + NPC.ai[2]) * (i + 1)).RotatedBy(NPC.oldRot[i + 1] + rotation)).ToRotation();
				float drawLength = (NPC.oldPos[i] - NPC.oldPos[i + 1] + new Vector2(0, 32 + (6 + NPC.ai[2]) * i).RotatedBy(NPC.oldRot[i] + rotation) - new Vector2(0, 32 + (6 + NPC.ai[2]) * (i + 1)).RotatedBy(NPC.oldRot[i + 1] + rotation)).Length() / 20;
				drawLength = Math.Min(Math.Max((10 - i) / 10f, drawLength), Math.Max(6 + NPC.ai[2], 6) * (10 - i) / 60f);

				spriteBatch.Draw(texture, NPC.oldPos[i] + NPC.Center - NPC.position - screenPos + new Vector2(0, 32 + (6 + NPC.ai[2]) * i).RotatedBy(NPC.oldRot[i] + rotation), frame, new Color(color.ToVector4() + new Vector4((11 - i) / 20f)), drawRotation, frame.Size() / 2, new Vector2((10 - i) / 10f, drawLength), SpriteEffects.None, 0f);

				float avgRot = (NPC.oldRot[i] + NPC.oldRot[i + 1]) / 2f;
				if (Math.Abs(NPC.oldRot[i] - NPC.oldRot[i + 1]) > MathHelper.Pi)
				{
					avgRot += MathHelper.PiOver2;
				}
				spriteBatch.Draw(texture, (NPC.oldPos[i] + NPC.oldPos[i + 1]) / 2f + NPC.Center - NPC.position - screenPos + new Vector2(0, 32 + (6 + NPC.ai[2]) * (i + 0.5f)).RotatedBy(avgRot + rotation), frame, new Color(color.ToVector4() + new Vector4((11 - i - 0.5f) / 20f)), drawRotation, frame.Size() / 2, new Vector2((10 - i - 0.5f) / 10f, drawLength), SpriteEffects.None, 0f);
			}
		}

		public override bool CheckDead()
		{
			if (PolaritiesSystem.hallowInvasion)
			{
				//counts for 4 points
				PolaritiesSystem.hallowInvasionSize -= 4;
			}

			//rainbow death dusts
			for (int j = 0; j < 6; j++)
            {
				float rotation = j * MathHelper.Pi / 3;
				Color dustColor;
				switch (j) {
					case 0:
						dustColor = Color.Red;
						break;
					case 1:
						dustColor = Color.OrangeRed;
						break;
					case 2:
						dustColor = Color.Yellow;
						break;
					case 3:
						dustColor = Color.Green;
						break;
					case 4:
						dustColor = Color.Blue;
						break;
					default:
						dustColor = Color.Purple;
						break;
				}

				for (int i = 0; i < NPC.oldPos.Length - 1; i++)
				{
					float avgRot = (NPC.oldRot[i] + NPC.oldRot[i + 1]) / 2f;
					if (Math.Abs(NPC.oldRot[i] - NPC.oldRot[i + 1]) > MathHelper.Pi)
					{
						avgRot += MathHelper.PiOver2;
					}
					Vector2 dustPos = (NPC.oldPos[i] + NPC.oldPos[i + 1]) / 2f + NPC.Center - NPC.position + new Vector2(0, 32 + (6 + NPC.ai[2]) * (i + 0.5f)).RotatedBy(avgRot + rotation);
					Dust.NewDustPerfect(dustPos, DustID.TintableDustLighted, newColor: dustColor, Scale: 2f).noGravity = true;
				}
            }
			for (int a = 0; a < 24; a++)
			{
				Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.TintableDustLighted, newColor: Color.White, Scale: 3f).noGravity = true;
			}

			return true;
		}

        public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ItemType<PainBow>(), 8));
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			if (spawnInfo.Player.ZoneTowerNebula || spawnInfo.Player.ZoneTowerSolar || spawnInfo.Player.ZoneTowerStardust || spawnInfo.Player.ZoneTowerVortex)
			{
				return 0f;
			}

			//only spawns during the hallow event
			if (spawnInfo.Player.InModBiome(GetInstance<Biomes.HallowInvasion>()) && spawnInfo.Player.ZoneHallow && spawnInfo.Player.ZoneOverworldHeight)
			{
				return Biomes.HallowInvasion.GetSpawnChance(6);
			}
			return 0f;
		}

        public override void DrawBehind(int index)
        {
			DrawLayer.AddNPC<DrawLayerAdditiveAfterNPCs>(index);
        }
    }

	public class PainbowHitbox : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_644";

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 2;
			Projectile.height = 2;

			Projectile.timeLeft = 30;
			Projectile.penetrate = -1;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.hide = true;
		}

		public override void AI()
		{
			if (Projectile.localAI[0] == 0)
			{
				Projectile.localAI[0] = 1;

				Projectile.rotation = Projectile.velocity.ToRotation();
			}

			Projectile.Center = Main.npc[(int)Projectile.ai[0]].Center;

			if (!Main.npc[(int)Projectile.ai[0]].active)
			{
				Projectile.Kill();
				return;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float radius = 1280 * (float)Math.Sin(Projectile.timeLeft / 30f * MathHelper.Pi);

			return Collision.CheckAABBvLineCollision(targetHitbox.TopRight(), targetHitbox.Size(), Projectile.Center + new Vector2(radius, 0).RotatedBy(Projectile.rotation), Projectile.Center - new Vector2(radius, 0).RotatedBy(Projectile.rotation));
		}

		public override bool ShouldUpdatePosition()
		{
			return false;
		}
	}
}
