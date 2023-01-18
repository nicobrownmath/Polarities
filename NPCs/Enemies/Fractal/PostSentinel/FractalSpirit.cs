using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Biomes.Fractal;
using Polarities.Items.Placeable.Banners;
using Polarities.Items.Placeable.Blocks.Fractal;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.NPCs.Enemies.Fractal.PostSentinel
{
    public class FractalSpirit : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;

            NPCID.Sets.TrailCacheLength[NPC.type] = 9;    //The length of old position to be recorded
            NPCID.Sets.TrailingMode[NPC.type] = 0;        //The recording mode

            NPCID.Sets.DebuffImmunitySets[Type] = new NPCDebuffImmunityData()
            {
                SpecificallyImmuneTo = new int[]
                {
                    BuffID.Confused,
                    BuffID.OnFire,
                    BuffID.OnFire3,
                    BuffID.Frostburn,
                    BuffID.Frostburn2,
                    BuffID.CursedInferno,
                    BuffID.ShadowFlame,
                    BuffID.Poisoned,
                    BuffID.Venom,
                }
            };
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers(0) { Position = new Vector2(0f, -40f), PortraitPositionYOverride = -24f, };

            /*
			Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 166, 166, false, SurfaceFormat.Color);
			System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
			for (int i = 0; i < texture.Width; i++)
			{
				for (int j = 0; j < texture.Height; j++)
				{
					double x = 2 * (2 * i / (double)(texture.Width - 1) - 1) - 0.75;
					double y = 2 * (2 * j / (double)(texture.Width - 1) - 1);
					double varX = x;
					double varY = y;

					int MAXITERATIONS = 8;

					for (int iterations = 0; iterations < MAXITERATIONS; iterations++)
					{
						double newX = x * x - y * y + varX;
						double newY = 2 * x * y + varY;
						x = newX;
						y = newY;
					}

					double distance = x * x + y * y;

					double value = Math.Pow(distance, -1/Math.Pow(2, MAXITERATIONS - 4));

					int r = 255;
					int g = 255;
					int b = 255;
					double alpha = value;
					//list.Add(new Color(r, g, b, (int)(255 * alpha)));
					list.Add(new Color((int)(r * alpha), (int)(g * alpha), (int)(b * alpha), (int)(255 * alpha)));
				}
			}
			for (int i = 0; i < texture.Width/2; i++)
			{
				for (int j = 0; j < texture.Height/2; j++)
				{
					double x = 2 * (2 * i / (double)(texture.Width/2 - 1) - 1) - 0.75;
					double y = 2 * (2 * j / (double)(texture.Width/2 - 1) - 1);
					double varX = x;
					double varY = y;

					int MAXITERATIONS = 8;

					for (int iterations = 0; iterations < MAXITERATIONS; iterations++)
					{
						double newX = x * x - y * y + varX;
						double newY = 2 * x * y + varY;
						x = newX;
						y = newY;
					}

					double distance = Math.Sqrt(x * x + y * y);

					if (distance <= 2)
					{
						int r = 82;
						int g = 179;
						int b = 203;
						int alpha = 255;

						int index = 2 * i * texture.Width + 2 * j;
						list.RemoveAt(index);
						list.Insert(index, new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
						list.RemoveAt(index + 1);
						list.Insert(index + 1, new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
						list.RemoveAt(index + texture.Width);
						list.Insert(index + texture.Width, new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
						list.RemoveAt(index + 1 + texture.Width);
						list.Insert(index + 1 + texture.Width, new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
					}
				}
			}

			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "FractalSpirit.png", FileMode.Create), texture.Width, texture.Height);
			*/
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 54;
            NPC.height = 54;

            NPC.defense = 24;
            NPC.damage = 50;
            NPC.lifeMax = 5000;
            NPC.knockBackResist = 0.25f;
            NPC.value = 5000;
            NPC.npcSlots = 1f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit36;
            NPC.DeathSound = SoundID.NPCDeath39;
            NPC.rarity = 1;

            this.SetModBiome<FractalBiome>();

            Banner = NPC.type;
            BannerItem = ItemType<FractalSpiritBanner>();
        }

        public override void AI()
        {
            Lighting.AddLight(NPC.Center, 0.5f, 0.5f, 1f);

            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.netUpdate = true;
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
            }

            if (NPC.ai[1] == 0)
            {
                if (Main.netMode != 1)
                {
                    NPC.ai[0] = (NPC.ai[0] + 1 + Main.rand.Next(2)) % 3;
                }
                NPC.netUpdate = true;
            }

            switch (NPC.ai[0])
            {
                case 0:
                    //zoom to above player and fall on them
                    if (NPC.ai[1] == 0)
                    {
                        NPC.ai[1] = Main.rand.Next(120, 180);
                    }
                    if (NPC.ai[1] > 90)
                    {
                        NPC.velocity = (player.Center + new Vector2(0, -300) - NPC.Center) / 6;
                        if (NPC.velocity.Length() > 24)
                        {
                            NPC.velocity.Normalize();
                            NPC.velocity *= 24;
                        }
                        NPC.rotation = (player.Center - NPC.Center).ToRotation() + MathHelper.PiOver2;
                    }
                    else
                    {
                        NPC.velocity.Y += 0.3f;
                        NPC.velocity.Y *= 0.99f;
                    }
                    break;
                case 1:
                    //zoom to above the side of player and fire a ring of weakly homing downwards-going bolts
                    if (NPC.ai[1] == 0)
                    {
                        NPC.ai[1] = Main.rand.Next(120, 180);
                        if (Main.netMode != 1)
                        {
                            NPC.ai[2] = Main.rand.NextFloat(-MathHelper.Pi / 3, MathHelper.Pi / 3);
                            NPC.ai[3] = Main.rand.Next(200, 400);
                        }
                        NPC.netUpdate = true;
                    }
                    else if (NPC.ai[1] > 60)
                    {
                        NPC.velocity = (player.Center + new Vector2(0, -NPC.ai[3]).RotatedBy(NPC.ai[2]) - NPC.Center) / 6;
                        if (NPC.velocity.Length() > 24)
                        {
                            NPC.velocity.Normalize();
                            NPC.velocity *= 24;
                        }
                        NPC.rotation = (NPC.ai[2] > 0 ? 1 : -1) * MathHelper.PiOver2;
                    }
                    else
                    {
                        NPC.velocity = Vector2.Zero;
                        NPC.rotation += (NPC.ai[2] > 0 ? 1 : -1) * MathHelper.TwoPi / 120;
                        if (NPC.ai[1] % 10 == 0)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 8).RotatedBy(NPC.rotation), ProjectileType<FractalSpiritSpark>(), 14, 0, Main.myPlayer, player.whoAmI);
                            SoundEngine.PlaySound(SoundID.Item93, NPC.Center);
                        }
                    }
                    break;
                case 2:
                    //zoom to somewhere around the player and LASERBEAM them
                    if (NPC.ai[1] == 0)
                    {
                        NPC.ai[1] = Main.rand.Next(120, 180);
                        if (Main.netMode != 1)
                        {
                            NPC.ai[2] = Main.rand.NextFloat(-MathHelper.Pi / 3, MathHelper.Pi / 3);
                            NPC.ai[3] = Main.rand.Next(200, 400);
                        }
                        NPC.netUpdate = true;
                    }
                    else if (NPC.ai[1] > 90)
                    {
                        NPC.velocity = (player.Center + new Vector2(0, -NPC.ai[3]).RotatedBy(NPC.ai[2]) - NPC.Center) / 6;
                        if (NPC.velocity.Length() > 24)
                        {
                            NPC.velocity.Normalize();
                            NPC.velocity *= 24;
                        }
                        NPC.rotation = NPC.ai[2];
                    }
                    else
                    {
                        NPC.velocity = Vector2.Zero;
                        if (NPC.ai[1] <= 60 && NPC.ai[1] % 5 == 0)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, 42).RotatedBy(NPC.rotation), new Vector2(0, 4).RotatedBy(NPC.ai[2]).RotatedByRandom(0.1f), ProjectileType<FractalSpiritLightning>(), 20, 0, Main.myPlayer);
                            SoundEngine.PlaySound(SoundID.Item122, NPC.Center);
                        }
                    }
                    break;
            }

            NPC.ai[1]--;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            //if (Subworld.IsActive<FractalSubworld>() && spawnInfo.Player.GetModPlayer<PolaritiesPlayer>().GetFractalization() > FractalSubworld.POST_GOLEM_TIME)
            //{
            //    return 0.15f;
            //}
            return 0f;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return NPC.ai[0] == 0 && NPC.ai[1] <= 90;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return NPC.ai[0] == 0 && NPC.ai[1] <= 90;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				//flavor text
				this.TranslatedBestiaryEntry()
            });
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DendriticEnergy>(), minimumDropped: 4, maximumDropped: 6));
            //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FractalKey>(), chanceDenominator: 3));
            //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MandelbrotOrb>(), chanceDenominator: 20));
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            /*Vector2 drawOrigin = new Vector2(Main.npcTexture[npc.type].Width * 0.5f, npc.height * 0.5f);
			Vector2 drawPos = npc.Center - Main.screenPosition;
			Color color = Color.White;

			for (int k = 0; k < npc.oldPos.Length; k++)
			{
				drawPos = npc.Center - npc.position + (npc.oldPos[k] + npc.position) / 2 - Main.screenPosition;
				color = Color.White * ((float)(npc.oldPos.Length - k) / (float)npc.oldPos.Length);
				if (npc.spriteDirection == -1)
				{
					spriteBatch.Draw(Main.npcTexture[npc.type], drawPos, npc.frame, color, npc.rotation, drawOrigin, npc.scale, SpriteEffects.None, 0f);
				}
				else
				{
					spriteBatch.Draw(Main.npcTexture[npc.type], drawPos, npc.frame, color, npc.rotation, drawOrigin, npc.scale, SpriteEffects.FlipHorizontally, 0f);
				}
			}*/

            Vector2 drawOrigin = new Vector2(82, 61); //new Vector2(Main.npcTexture[npc.type].Width * 0.5f, npc.height * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos;
            Color color = Color.White;
            if (NPC.spriteDirection == -1)
            {
                spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
            }
            else
            {
                spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.FlipHorizontally, 0f);
            }
            return false;
        }
    }

    public class FractalSpiritLightning : ModProjectile
    {
        public override string Texture => "Polarities/Projectiles/CallShootProjectile";

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.alpha = 0;
            Projectile.timeLeft = 1023;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1023;
            Projectile.tileCollide = true;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Dust.NewDustPerfect(Projectile.Center, DustID.Electric, Velocity: Vector2.Zero, Scale: 1f).noGravity = true;
        }
    }

    public class FractalSpiritSpark : ModProjectile
    {
        public override string Texture => "Polarities/Projectiles/CallShootProjectile";
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.alpha = 0;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Player player = Main.player[(int)Projectile.ai[0]];

            Projectile.velocity.Y += 0.25f;
            if (Projectile.velocity.Y > 2)
            {
                Projectile.velocity.Y = 2;
            }

            if (player.Center.X > Projectile.Center.X)
            {
                Projectile.velocity.X += 0.1f;
                if (Projectile.velocity.X > 6)
                {
                    Projectile.velocity.X = 6;
                }
            }
            else
            {
                Projectile.velocity.X -= 0.1f;
                if (Projectile.velocity.X < -6)
                {
                    Projectile.velocity.X = -6;
                }
            }

            Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, Scale: 1f)].noGravity = true;
        }
    }
}
