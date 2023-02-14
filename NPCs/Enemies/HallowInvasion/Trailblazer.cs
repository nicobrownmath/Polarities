using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Effects;
using Polarities.Items.Placeable.Banners;
using Polarities.Items.Weapons.Magic;
using Polarities.Projectiles;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.NPCs.Enemies.HallowInvasion
{
    public class Trailblazer : ModNPC
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
            NPC.width = 30;
            NPC.height = 30;

            NPC.defense = 20;
            NPC.damage = 50;
            NPC.lifeMax = 1000;
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
            BannerItem = ItemType<TrailblazerBanner>();

            SpawnModBiomes = new int[1] { GetInstance<Biomes.HallowInvasion>().Type };
        }

        public override void AI()
        {
            Lighting.AddLight(NPC.Center, 1f, 1f, 1f);

            NPC.TargetClosest(false);
            Player player = Main.player[NPC.target];

            if (!PolaritiesSystem.hallowInvasion)
            {
                //run away if not in the invasion
                NPC.ai[0] = -1;
            }

            //create my trail projectile if it doesn't exist
            if ((NPC.ai[2] == 0 || !Main.projectile[(int)NPC.ai[2] - 1].active || Main.projectile[(int)NPC.ai[2] - 1].type != ProjectileType<TrailblazerHitbox>() || Main.projectile[(int)NPC.ai[2] - 1].ai[0] != NPC.whoAmI) && Main.netMode != 1)
            {
                NPC.ai[2] = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<TrailblazerHitbox>(), 25, 0f, Main.myPlayer, ai0: NPC.whoAmI) + 1;
            }

            switch (NPC.ai[0])
            {
                case 0:
                    //standard worminess I guess
                    Vector2 goalVelocity = (player.Center - NPC.Center) / 60f;
                    NPC.velocity += (goalVelocity - NPC.velocity) / 60f;

                    if (NPC.velocity.Length() > 8)
                    {
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 8;
                    }

                    NPC.ai[1]++;
                    if (NPC.ai[1] >= 240)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = Main.rand.Next(new int[] { 1, 2 });
                    }
                    break;
                case 1:
                    //jumpy worminess I guess

                    //random deceleration thing
                    NPC.velocity += (-4f * NPC.velocity.SafeNormalize(Vector2.Zero)).RotatedByRandom(MathHelper.PiOver2);

                    //occasionally go super fast
                    if (NPC.ai[1] % 60 == 0 && NPC.ai[1] > 0)
                    {
                        float speed = 64 - 16 * (NPC.life / (float)NPC.lifeMax);

                        NPC.velocity += new Vector2(speed, 0).RotatedBy(NPC.ai[3]);
                    }
                    if (NPC.ai[1] < 180)
                    {
                        //adjust goal direction
                        Vector2 goalDirection = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                        NPC.ai[3] = (new Vector2(NPC.ai[1] % 60, 0).RotatedBy(NPC.ai[3]) + goalDirection).ToRotation();
                    }

                    NPC.ai[1]++;
                    if (NPC.ai[1] >= 240)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = Main.rand.Next(new int[] { 0, 2 });
                    }
                    break;
                case 2:
                    //move slower than normal and do some sort of flashbang thing
                    NPC.velocity *= 0.95f;

                    goalVelocity = (player.Center - NPC.Center) / 60f;
                    NPC.velocity += (goalVelocity - NPC.velocity) / 60f;

                    if (NPC.velocity.Length() > 8)
                    {
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 8;
                    }

                    //do aforementioned flashbang thing
                    if (Main.netMode != 1 && NPC.ai[1] == 0)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<TrailblazerFlash>(), 25, 0f, Main.myPlayer, NPC.whoAmI);
                    }

                    NPC.ai[1]++;
                    if (NPC.ai[1] >= 180)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = Main.rand.Next(new int[] { 0, 1 });
                    }
                    break;
                case -1:
                    //flee, flee for your lives
                    goalVelocity = (NPC.Center - player.Center) / 60f;
                    NPC.velocity += (goalVelocity - NPC.velocity) / 60f;

                    if (NPC.velocity.Length() > 8)
                    {
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 8;
                    }
                    break;
            }

            NPC.rotation = NPC.velocity.X * 0.1f;
        }

        public override void FindFrame(int frameHeight)
        {
            //wingbeats
            NPC.localAI[0] += 0.5f;
        }

        public static Asset<Texture2D> WingTexture;

        public override void Load()
        {
            WingTexture = Request<Texture2D>(Texture + "_Wing");
        }

        public override void Unload()
        {
            WingTexture = null;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            //draw telegraph for dashes
            if (NPC.ai[0] == 1 && NPC.ai[1] < 180)
            {
                Texture2D telegraphTexture = Textures.Glow58.Value;
                Rectangle telegraphFrame = telegraphTexture.Frame(2, 1, 1, 0);

                spriteBatch.Draw(telegraphTexture, NPC.Center - screenPos, telegraphFrame, new Color(255, 240, 168), NPC.ai[3], new Vector2(0, telegraphFrame.Size().Y / 2), new Vector2((NPC.ai[1] % 60) / 4f, Math.Min(1f, 4f / (NPC.ai[1] % 60))), SpriteEffects.None, 0f);
            }

            Texture2D wingTexture = WingTexture.Value;
            Rectangle wingFrame = wingTexture.Frame();

            spriteBatch.Draw(wingTexture, NPC.Center - screenPos, wingFrame, NPC.GetNPCColorTintedByBuffs(Color.White), NPC.rotation + (float)Math.Sin(NPC.localAI[0]) * 0.75f - 0.1f, new Vector2(47, 13), new Vector2(1, ((float)Math.Cos(NPC.localAI[0]) + 2) / 3) * NPC.scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(wingTexture, NPC.Center - screenPos, wingFrame, NPC.GetNPCColorTintedByBuffs(Color.White), NPC.rotation - (float)Math.Sin(NPC.localAI[0]) * 0.75f + 0.1f, new Vector2(-13, 13), new Vector2(1, ((float)Math.Cos(NPC.localAI[0]) + 2) / 3) * NPC.scale, SpriteEffects.FlipHorizontally, 0f);

            Texture2D texture = TextureAssets.Npc[Type].Value;
            Rectangle frame = texture.Frame();

            spriteBatch.Draw(texture, NPC.Center - screenPos, frame, NPC.GetNPCColorTintedByBuffs(Color.White), NPC.rotation, frame.Size() / 2, NPC.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override bool CheckDead()
        {
            if (PolaritiesSystem.hallowInvasion)
            {
                //counts for 4 points
                PolaritiesSystem.hallowInvasionSize -= 4;
            }

            for (int i = 1; i <= 2; i++)
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("TrailblazerWingGore").Type);
            for (int a = 0; a < 12; a++)
            {
                Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, 6, newColor: Color.White, Scale: 2f).noGravity = true;
            }

            return true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.PixieDust, 1, 2, 5));
            npcLoot.Add(ItemDropRule.Common(ItemType<HeatFlare>(), 8));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            //only spawns during the hallow event
            if (spawnInfo.Player.InModBiome(GetInstance<Biomes.HallowInvasion>()))
            {
                return Biomes.HallowInvasion.GetSpawnChance(5);
            }
            return 0f;
        }
    }

    public class TrailblazerHitbox : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_644";

        private Vector2[] trailPositions;
        private float[] trailScale;

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

            Projectile.GetGlobalProjectile<PolaritiesProjectile>().ForceDraw = true;

            trailPositions = new Vector2[128];
            trailScale = new float[128];
        }

        public override void AI()
        {
            Projectile.Center = Main.npc[(int)Projectile.ai[0]].Center;

            //don't die
            Projectile.timeLeft = 2;

            if (!Main.npc[(int)Projectile.ai[0]].active)
            {
                Projectile.Kill();
                return;
            }

            //initialize trail
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                for (int i = 0; i < trailPositions.Length; i++)
                {
                    trailPositions[i] = Projectile.Center;
                    trailScale[i] = 1 - i / (float)trailPositions.Length;
                }
            }

            //shift trail by a certain amount
            float trailShift = Math.Min(((Projectile.Center - trailPositions[0]).Length() / 16), trailPositions.Length);

            for (int i = trailPositions.Length - 1; i >= (int)trailShift; i--)
            {
                trailPositions[i] = trailPositions[i - (int)trailShift];
                trailScale[i] = Math.Max(0f, trailScale[i - (int)trailShift] - (int)trailShift / (float)trailPositions.Length);
            }
            for (int i = (int)trailShift - 1; i >= 0; i--)
            {
                trailPositions[i] = (Projectile.Center * ((int)trailShift - i) + trailPositions[0] * i) / (int)trailShift;
                trailScale[i] = 1f;
            }

            //trail homogenization
            for (int i = trailPositions.Length - 2; i > 0; i--)
            {
                trailPositions[i] = (trailPositions[i + 1] + 2 * trailPositions[i] + (2 + trailShift) * trailPositions[i - 1]) / (5 + trailShift);
            }
            trailPositions[0] = (trailPositions[1] + 2 * trailPositions[0] + (2 + trailShift) * Projectile.Center) / (5 + trailShift);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (int i = 0; i < trailPositions.Length - 1; i++)
            {
                if (trailScale[i] <= 0.1f)
                {
                    //trailScale is nonincreasing
                    return false;
                }

                if (Collision.CheckAABBvAABBCollision(targetHitbox.TopLeft(), targetHitbox.Size(), trailPositions[i] - new Vector2(trailScale[i] * 15), new Vector2(trailScale[i] * 30)))
                {
                    return true;
                }
            }

            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame();

            float alpha = DrawLayer.IsActive<DrawLayerAdditiveBeforeNPCs>() ? 0.25f : 0.75f;

            for (int i = trailPositions.Length - 2; i >= 0; i--)
            {
                if (trailScale[i] >= 0.05f)
                {
                    Main.EntitySpriteDraw(texture, trailPositions[i] - Main.screenPosition, frame, new Color(255, 240, 168) * alpha, (trailPositions[i + 1] - trailPositions[i]).ToRotation() + MathHelper.PiOver2, frame.Size() / 2, trailScale[i], SpriteEffects.None, 0);
                }
            }

            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
            DrawLayer.AddProjectile<DrawLayerAdditiveBeforeNPCs>(index);
        }

        public override bool ShouldUpdatePosition() => false;
    }

    public class TrailblazerFlash : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Glow256";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Pixie Aura");

            /*Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 256, 256, false, SurfaceFormat.Color);
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
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "TrailblazerFlash.png", FileMode.Create), texture.Width, texture.Height);*/
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 150;
            Projectile.height = 150;

            Projectile.timeLeft = 180;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Projectile.scale = 0.23f * ((180 - Projectile.timeLeft) * (180 - Projectile.timeLeft) / 2025f + 1f) * (float)Math.Abs(Math.Sin(Projectile.timeLeft * MathHelper.Pi / 60f));
            Projectile.width = (int)(150 * Projectile.scale);
            Projectile.height = (int)(150 * Projectile.scale);

            Projectile.Center = Main.npc[(int)Projectile.ai[0]].Center;

            if (!Main.npc[(int)Projectile.ai[0]].active)
            {
                Projectile.Kill();
                return;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float nearestX = Math.Max(targetHitbox.X, Math.Min(Projectile.Center.X, targetHitbox.X + targetHitbox.Size().X));
            float nearestY = Math.Max(targetHitbox.Y, Math.Min(Projectile.Center.Y, targetHitbox.Y + targetHitbox.Size().Y));
            return (new Vector2(Projectile.Center.X - nearestX, Projectile.Center.Y - nearestY)).Length() < Projectile.width / 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame();

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, new Color(255, 240, 168), Projectile.rotation, frame.Size() / 2, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }

        public override bool ShouldUpdatePosition() => false;
    }
}