using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Items.Placeable.Banners;
using Polarities.Items.Weapons.Summon.Sentries;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.NPCs.Enemies.HallowInvasion
{
    public class IlluminantScourer : ModNPC
    {
        public override void SetStaticDefaults()
        {
            NPCID.Sets.TrailCacheLength[Type] = 6;
            NPCID.Sets.TrailingMode[Type] = 3;

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
                Velocity = 2,
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

        private float[] plateAngle = new float[6];
        private float[] plateDistance = new float[6];

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 30;
            NPC.height = 30;

            NPC.defense = 20;
            NPC.damage = 40;
            NPC.lifeMax = 900;
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
            BannerItem = ItemType<IlluminantScourerBanner>();

            SpawnModBiomes = new int[1] { GetInstance<Biomes.HallowInvasion>().Type };
        }

        public override void AI()
        {
            Lighting.AddLight(NPC.Center, 1f, 0.5f, 1f);

            NPC.TargetClosest(false);
            Player player = Main.player[NPC.target];

            if (NPC.localAI[0] == 0)
            {
                NPC.localAI[0] = 1;

                //initialize plate angle and distance
                for (int i = 0; i < plateAngle.Length; i++)
                {
                    plateAngle[i] = i * MathHelper.Pi / plateAngle.Length + MathHelper.Pi + MathHelper.PiOver2 / plateAngle.Length;
                    plateDistance[i] = 32 / (float)Math.Pow(Math.Sin(plateAngle[i] / 2 - MathHelper.PiOver4), 2);
                }
            }

            if (!PolaritiesSystem.hallowInvasion)
            {
                //flee if not in the invasion
                NPC.ai[0] = -1;
            }

            if (NPC.ai[0] >= 0)
            {
                if (NPC.ai[0] < 180)
                {
                    //moving
                    Vector2 goalPosition = player.Center + new Vector2(0, -300);
                    Vector2 goalVelocity = (goalPosition - NPC.Center) / 30f;
                    NPC.velocity += (goalVelocity - NPC.velocity) / (180 - NPC.ai[0]);
                }
                else
                {
                    //stop and shoot laser
                    NPC.velocity *= 0.9f;

                    if (NPC.ai[0] == 180)
                    {
                        //just started stopping, modify timers for other scourers
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            if (i != NPC.whoAmI && Main.npc[i].active && Main.npc[i].type == NPC.type && Main.npc[i].ai[0] <= 180 && Main.npc[i].ai[0] > 150)
                            {
                                Main.npc[i].ai[0] = 117;
                            }
                        }
                    }

                    if (NPC.ai[0] == 190)
                    {
                        NPC.velocity = Vector2.Zero;
                        for (int i = 0; i < plateAngle.Length; i++)
                        {
                            Vector2 bouncePosition = new Vector2(plateDistance[i], 0).RotatedBy(NPC.rotation + plateAngle[i]);
                            Main.projectile[Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<IlluminantScourerLaser>(), 25, 0f, Main.myPlayer, bouncePosition.X, bouncePosition.Y)].localAI[1] = NPC.whoAmI;
                        }
                    }

                    if (NPC.ai[0] >= 250)
                    {
                        if (NPC.ai[0] == 250)
                        {
                            SoundEngine.PlaySound(SoundID.Item122, NPC.Center);
                        }

                        NPC.velocity.Y -= 0.1f;
                    }
                }
                NPC.ai[0]++;
                if (NPC.ai[0] == 370)
                {
                    NPC.ai[0] = 0;
                }
            }
            else
            {
                //flee
                Vector2 goalPosition = NPC.Center + new Vector2(0, -300);
                Vector2 goalVelocity = (goalPosition - NPC.Center) / 30f;
                NPC.velocity += (goalVelocity - NPC.velocity) / 120f;
            }

            //update plate motion
            NPC.localAI[1] += Math.Max(Math.Min(NPC.velocity.X * (Math.Abs(NPC.velocity.Y) + 2) / 2f, 1), -1);
            for (int i = 0; i < plateAngle.Length; i++)
            {
                plateAngle[i] = i * MathHelper.Pi / plateAngle.Length + MathHelper.Pi + MathHelper.PiOver2 / plateAngle.Length + 0.1f * (float)(Math.Sin(NPC.localAI[1] * 0.1f + i - 2.5f));
                plateDistance[i] = (float)(Math.Cos(NPC.localAI[1] * 0.1f + i - 2.5f) + Math.Cos(-NPC.localAI[1] * 0.1f + i - 2.5f) + 10f) / 10f * 32 / (float)Math.Pow(Math.Sin(plateAngle[i] / 2 - MathHelper.PiOver4), 2);
            }
        }

        public override void FindFrame(int frameHeight)
        {
            if (NPC.IsABestiaryIconDummy)
            {
                if (NPC.localAI[0] == 0)
                {
                    NPC.localAI[0] = 1;

                    //initialize plate angle and distance
                    for (int i = 0; i < plateAngle.Length; i++)
                    {
                        plateAngle[i] = i * MathHelper.Pi / plateAngle.Length + MathHelper.Pi + MathHelper.PiOver2 / plateAngle.Length;
                        plateDistance[i] = 32 / (float)Math.Pow(Math.Sin(plateAngle[i] / 2 - MathHelper.PiOver4), 2);
                    }
                }

                NPC.localAI[1] += Math.Max(Math.Min(NPC.velocity.X * (Math.Abs(NPC.velocity.Y) + 2) / 2f, 1), -1);
                for (int i = 0; i < plateAngle.Length; i++)
                {
                    plateAngle[i] = i * MathHelper.Pi / plateAngle.Length + MathHelper.Pi + MathHelper.PiOver2 / plateAngle.Length + 0.1f * (float)(Math.Sin(NPC.localAI[1] * 0.1f + i - 2.5f));
                    plateDistance[i] = (float)(Math.Cos(NPC.localAI[1] * 0.1f + i - 2.5f) + Math.Cos(-NPC.localAI[1] * 0.1f + i - 2.5f) + 10f) / 10f * 32 / (float)Math.Pow(Math.Sin(plateAngle[i] / 2 - MathHelper.PiOver4), 2);
                }
            }
        }

        public static Asset<Texture2D> PlateTexture;

        public override void Load()
        {
            PlateTexture = Request<Texture2D>(Texture + "_Plate");
        }

        public override void Unload()
        {
            PlateTexture = null;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Rectangle frame = texture.Frame();

            spriteBatch.Draw(texture, NPC.Center - screenPos, frame, NPC.GetNPCColorTintedByBuffs(Color.White), NPC.rotation, frame.Size() / 2, NPC.scale, SpriteEffects.None, 0f);

            //Adapted from vanilla illuminant NPC draw code
            for (int num347 = 1; num347 < NPC.oldPos.Length; num347++)
            {
                Color color27 = NPC.GetNPCColorTintedByBuffs(new Color((byte)(150 * (10 - num347) / 15), (byte)(100 * (10 - num347) / 15), (byte)(150 * (10 - num347) / 15), (byte)(50 * (10 - num347) / 15)));
                spriteBatch.Draw(texture, NPC.oldPos[num347] + NPC.Center - NPC.position - screenPos, frame, color27, NPC.rotation, frame.Size() / 2, NPC.scale, SpriteEffects.None, 0f);
            }

            //Draw plates
            Texture2D plateTexture = PlateTexture.Value;
            Rectangle plateFrame = plateTexture.Frame();
            for (int i = 0; i < plateAngle.Length; i++)
            {
                spriteBatch.Draw(plateTexture, NPC.Center - screenPos + new Vector2(plateDistance[i], 0).RotatedBy(NPC.rotation + plateAngle[i]), plateFrame, NPC.GetNPCColorTintedByBuffs(Color.White), NPC.rotation + plateAngle[i] / 2 + MathHelper.PiOver4, new Vector2(plateFrame.Size().X / 2, 0), NPC.scale, SpriteEffects.None, 0f);

                for (int num347 = 1; num347 < NPC.oldPos.Length; num347++)
                {
                    Color color27 = NPC.GetNPCColorTintedByBuffs(new Color((byte)(150 * (10 - num347) / 15), (byte)(100 * (10 - num347) / 15), (byte)(150 * (10 - num347) / 15), (byte)(50 * (10 - num347) / 15)));
                    spriteBatch.Draw(plateTexture, NPC.oldPos[num347] + NPC.Center - NPC.position - screenPos + new Vector2(plateDistance[i], 0).RotatedBy(NPC.rotation + plateAngle[i]), plateFrame, color27, NPC.rotation + plateAngle[i] / 2 + MathHelper.PiOver4, new Vector2(plateFrame.Size().X / 2, 0), NPC.scale, SpriteEffects.None, 0f);
                }
            }

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
                Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Enchanted_Pink, newColor: Color.White, Scale: 2f).noGravity = true;
            }
            for (int i = 0; i < plateAngle.Length; i++)
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center + new Vector2(plateDistance[i], 0).RotatedBy(NPC.rotation + plateAngle[i]), NPC.velocity, Mod.Find<ModGore>("IlluminantScourerGore").Type);

            return true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.CrystalShard, 1, 1, 3));
            npcLoot.Add(ItemDropRule.Common(ItemType<ScouringStaff>(), 8));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            //only spawns during the hallow event
            if (spawnInfo.Player.InModBiome(GetInstance<Biomes.HallowInvasion>()))
            {
                return ModUtils.Lerp(Biomes.HallowInvasion.GetSpawnChance(0), Biomes.HallowInvasion.GetSpawnChance(1), 0.66f);
            }
            return 0f;
        }
    }

    public class IlluminantScourerLaser : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            /*
			Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 1, 32, false, SurfaceFormat.Color);
			List<Color> list = new List<Color>();
			for (int j = 0; j < texture.Height; j++)
			{
				for (int i = 0; i < texture.Width; i++)
				{
					float y = (2 * j / (float)(texture.Height - 1) - 1);

					float distanceSquared = y * y;
					float index = 1 - distanceSquared;

					int r = 255 - (int)(0 * (1 - index));
					int g = 255 - (int)(255 * (1 - index));
					int b = 255 - (int)(13 * (1 - index));
					int alpha = distanceSquared >= 1 ? 0 : (int)(255 * index);

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "IlluminantLaser.png", FileMode.Create), texture.Width, texture.Height);

			texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 32, 32, false, SurfaceFormat.Color);
			list = new List<Color>();
			for (int j = 0; j < texture.Height; j++)
			{
				for (int i = 0; i < texture.Width; i++)
				{
					float x = (2 * i / (float)(texture.Width - 1) - 1);
					float y = (2 * j / (float)(texture.Height - 1) - 1);

					float distanceSquared = y * y;
					float index = (1 + x) / 2 * (1 - y * y);

					int r = 255 - (int)(0 * (1 - index));
					int g = 255 - (int)(255 * (1 - index));
					int b = 255 - (int)(13 * (1 - index));
					int alpha = (int)(255 * index);

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "IlluminantLaserCap.png", FileMode.Create), texture.Width, texture.Height);
			*/
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 2;
            Projectile.height = 2;

            Projectile.timeLeft = 180;
            Projectile.penetrate = -1;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }

        private Vector2 bouncePoint
        {
            get => Projectile.Center + new Vector2(Projectile.ai[0], Projectile.ai[1]);
        }

        public override void AI()
        {
            if (!Main.npc[(int)Projectile.localAI[1]].active)
            {
                Projectile.Kill();
                return;
            }

            //set distance
            Projectile.localAI[0] = 16 - bouncePoint.Y % 16;
            while (Projectile.localAI[0] < 2000)
            {
                Projectile.localAI[0] += 16;
                Tile tile = Main.tile[(bouncePoint + new Vector2(0, Projectile.localAI[0])).ToPoint().X / 16, (bouncePoint + new Vector2(0, Projectile.localAI[0])).ToPoint().Y / 16];
                if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]) break;
            }

            if (Projectile.timeLeft == 120)
            {
                Projectile.hostile = true;
            }

            if (Projectile.timeLeft <= 120)
            {
                Projectile.velocity *= 0.9f;
                Projectile.velocity.Y -= 0.1f;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), bouncePoint, bouncePoint + new Vector2(0, Projectile.localAI[0]));
        }

        public static Asset<Texture2D> CapTexture;

        public override void Load()
        {
            CapTexture = Request<Texture2D>(Texture + "_Cap");
        }

        public override void Unload()
        {
            CapTexture = null;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            //telegraph
            if (Projectile.timeLeft > 120)
            {
                float telegraphAlpha = 1 - (Projectile.timeLeft - 120) / 60f;

                Main.EntitySpriteDraw(Textures.PixelTexture.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.White * telegraphAlpha, (bouncePoint - Projectile.Center).ToRotation(), new Vector2(0, 0.5f), new Vector2((bouncePoint - Projectile.Center).Length(), 1), SpriteEffects.None, 0);
                Main.EntitySpriteDraw(Textures.PixelTexture.Value, bouncePoint - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.White * telegraphAlpha, 0f, new Vector2(0.5f, 0), new Vector2(1, Projectile.localAI[0]), SpriteEffects.None, 0);
            }
            //actual laser
            else
            {
                float scale = (float)(5 + Math.Sin(Main.GlobalTimeWrappedHourly * 60f / 1.5f)) / 5f * 0.5f;

                Texture2D texture = TextureAssets.Projectile[Type].Value;
                Rectangle frame = texture.Frame();

                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White, (bouncePoint - Projectile.Center).ToRotation() - MathHelper.PiOver2, new Vector2(16, 0), new Vector2(scale, (bouncePoint - Projectile.Center).Length()), SpriteEffects.None, 0);
                Main.EntitySpriteDraw(texture, bouncePoint - Main.screenPosition, frame, Color.White, 0f, new Vector2(16, 0), new Vector2(scale, Projectile.localAI[0]), SpriteEffects.None, 0);

                Texture2D laserCapTexture = CapTexture.Value;
                Rectangle laserCapFrame = laserCapTexture.Frame();

                Main.EntitySpriteDraw(laserCapTexture, bouncePoint + new Vector2(0, Projectile.localAI[0]) - Main.screenPosition, laserCapFrame, Color.White, 0f, new Vector2(laserCapFrame.Size().X / 2, 0), scale, SpriteEffects.None, 0);
            }
            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }
}