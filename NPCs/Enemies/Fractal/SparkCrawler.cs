using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Biomes.Fractal;
using Polarities.Items.Materials;
using Polarities.Items.Placeable.Banners;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.NPCs.Enemies.Fractal
{
    public class SparkCrawler : ModNPC
    {
        private Vector2[] feet = new Vector2[3];
        private Vector2[] feetGoals = new Vector2[3];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 8;
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers(0) { CustomTexturePath = "Polarities/Textures/Bestiary/SparkCrawler", };
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = 14;
            NPC.stairFall = true;
            NPC.width = 30;
            NPC.height = 30;
            DrawOffsetY = 15;

            NPC.defense = 8;
            NPC.damage = 40;
            NPC.lifeMax = 300;
            NPC.knockBackResist = 0.01f;
            NPC.npcSlots = 1f;
            NPC.noGravity = true;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.value = 750;

            NPC.buffImmune[BuffID.Confused] = true;
            NPC.buffImmune[BuffID.OnFire] = true;
            NPC.buffImmune[BuffID.Frostburn] = true;
            NPC.buffImmune[BuffID.CursedInferno] = true;
            NPC.buffImmune[BuffID.ShadowFlame] = true;
            NPC.buffImmune[BuffID.Poisoned] = true;
            NPC.buffImmune[BuffID.Venom] = true;

            Banner = NPC.type;
            BannerItem = ItemType<SparkCrawlerBanner>();

            this.SetModBiome<FractalBiome, FractalUGBiome>();
        }

        public override bool PreAI()
        {
            Lighting.AddLight(NPC.Center, 0.8f, 0.8f, 1f);

            if (NPC.localAI[0] == 0)
            {
                NPC.localAI[0] = 1;
                for (int i = 0; i < 3; i++)
                {
                    feet[i] = NPC.Center + new Vector2(64, 0).RotatedBy(i * MathHelper.PiOver2);
                    feetGoals[i] = feet[i];
                }
            }

            for (int i = 0; i < 3; i++)
            {
                if ((NPC.Center - feet[i]).Length() > 256)
                {
                    feet[i] = NPC.Center;
                }
                if ((NPC.Center - feet[i]).Length() > 128)
                {
                    feetGoals[i] = NPC.Center + NPC.velocity * 64 + new Vector2(64, 0).RotatedByRandom(Math.PI);
                    NPC.netUpdate = true;
                }
                Vector2 goalDir = feetGoals[i] - feet[i];
                if (goalDir.Length() > 8)
                {
                    goalDir.Normalize();
                    goalDir *= 8;
                }
                if (Main.tile[(int)(feet[i].X / 16), (int)(feet[i].Y / 16)].WallType != 0 || Main.tile[(int)(feet[i].X / 16), (int)(feet[i].Y / 16)].HasTile)
                {
                    feet[i] += goalDir;
                }
                else
                {
                    feet[i] += new Vector2(0, 4);
                    feetGoals[i] = feet[i];
                }

                NPC.velocity += (feet[i] - NPC.Center).SafeNormalize(Vector2.Zero) * 0.03f;
            }

            NPC.TargetClosest(false);
            Player player = Main.player[NPC.target];

            if (NPC.collideX)
            {
                NPC.velocity.X = -NPC.oldVelocity.X;
                NPC.velocity.Y *= 1.5f;
            }
            if (NPC.collideY)
            {
                NPC.velocity.Y = -NPC.oldVelocity.Y;
                NPC.velocity.X *= 1.5f;
            }

            NPC.velocity += (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 0.2f;
            if (NPC.velocity.Length() > 2f)
            {
                NPC.velocity.Normalize();
                NPC.velocity *= 2;
            }

            NPC.rotation = (player.Center - NPC.Center).ToRotation() - MathHelper.PiOver2;

            NPC.noGravity = Main.tile[(int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16)].WallType != 0;

            if (NPC.ai[0] == 0 && Collision.CanHit(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height))
            {
                if (Main.netMode != 1)
                {
                    if (Main.rand.NextBool())
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(20, 0).RotatedBy((player.Center - NPC.Center).ToRotation()), ProjectileType<SparkCrawlerLightningHead>(), 0, 0, Main.myPlayer, NPC.Center.X, NPC.Center.Y);
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), feet[i], new Vector2(6, 0).RotatedBy((player.Center - NPC.Center).ToRotation()), ProjectileType<SparkCrawlerTriangle>(), 20, 0, Main.myPlayer);
                        }
                    }
                }
                NPC.ai[0] = 120;

                NPC.frameCounter = 0;
                NPC.frame.Y += TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type];
                if (NPC.frame.Y == TextureAssets.Npc[NPC.type].Value.Height) NPC.frame.Y = 0;

            }
            if (NPC.ai[0] > 0)
            {
                NPC.ai[0]--;
            }

            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            if (NPC.frame.Y == 0 || NPC.frame.Y == 4 * frameHeight)
            {
                return;
            }
            NPC.frameCounter++;
            if (NPC.frameCounter == 3)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y = (NPC.frame.Y + frameHeight) % TextureAssets.Npc[NPC.type].Value.Height;
            }
        }

        public override bool CheckDead()
        {
            //Gore.NewGore(NPC.position, NPC.velocity, Mod.GetGoreSlot("Gores/SparkCrawlerGore1"));
            //Gore.NewGore(NPC.position, NPC.velocity, Mod.GetGoreSlot("Gores/SparkCrawlerGore2"));
            return true;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            //if (Subworld.IsActive<FractalSubworld>())
            //{
            //    return FractalSubworld.SpawnConditionFractalUnderground(spawnInfo);
            //}
            return 0f;
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
            //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Lightslate>(), minimumDropped: 1, maximumDropped: 2));
            //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SparkOfSimilarity>(), chanceDenominator: 50));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FractalResidue>(), chanceDenominator: 4));
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return false;
            for (int i = 0; i < 3; i++)
            {
                Texture2D legTexture = ModContent.Request<Texture2D>($"{Texture}Leg").Value;

                Vector2 constructCenter = NPC.Center;
                Vector2 center = feet[i];
                Vector2 distToNPC = constructCenter - center;
                float projRotation = distToNPC.ToRotation() + MathHelper.PiOver2;
                float distance = distToNPC.Length();
                while (distance > 16f && !float.IsNaN(distance))
                {
                    //Draw chain
                    spriteBatch.Draw(legTexture, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                        new Rectangle(0, 0, 12, 16), Lighting.GetColor((int)(center.X / 16), (int)(center.Y / 16)), projRotation,
                        new Vector2(12 * 0.5f, 16 * 0.5f), 1f, SpriteEffects.None, 0f);

                    distToNPC.Normalize();                 //get unit vector
                    distToNPC *= 16f;                      //speed = 24
                    center += distToNPC;                   //update draw position
                    distToNPC = constructCenter - center;    //update distance
                    distance = distToNPC.Length();
                }

                Texture2D texture = ModContent.Request<Texture2D>($"{Texture}Foot").Value;

                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.33f);
                Vector2 drawPos = feet[i] - Main.screenPosition;
                spriteBatch.Draw(texture, drawPos, new Rectangle(0, 0, texture.Width, texture.Height), Lighting.GetColor((int)(feet[i].X / 16), (int)(feet[i].Y / 16)), NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
            }
            return true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            for (int i = 0; i < 3; i++)
            {
                writer.WriteVector2(feet[i]);
                writer.WriteVector2(feetGoals[i]);
            }
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            for (int i = 0; i < 3; i++)
            {
                feet[i] = reader.ReadVector2();
                feetGoals[i] = reader.ReadVector2();
            }
        }
    }

    public class SparkCrawlerTriangle : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.light = 1;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;

                SoundEngine.PlaySound(SoundID.Item109, Projectile.Center);
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, newColor: Color.LightBlue, Scale: 1f)].noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Color mainColor = Color.White;

            for (int k = 1; k < Projectile.oldPos.Length; k++)
            {
                Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                float scale = Projectile.scale * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

                float rotation = (Projectile.oldPos[k - 1] - Projectile.oldPos[k]).ToRotation();

                Main.spriteBatch.Draw(texture, Projectile.Center + Projectile.oldPos[k] - Projectile.position - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), color, rotation, new Vector2(13, 13), scale, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), mainColor, Projectile.rotation, new Vector2(13, 13), Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    }

    public class SparkCrawlerLightning : ModProjectile
    {
        public override void SetStaticDefaults()
        {
        }

        private float angle
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private int owner
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        private int length;

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 8;
            Projectile.height = 8;
            DrawOffsetX = 0;
            DrawOriginOffsetY = 0;
            DrawOriginOffsetX = -12;
            Projectile.alpha = 0;
            Projectile.timeLeft = 30;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
            Projectile.hide = true;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;

                for (length = 0; length < 32; length += 8)
                {
                    if (!Collision.CanHit(Projectile.Center, 1, 1, Projectile.Center + (new Vector2(length - Projectile.width / 2, 0)).RotatedBy(Projectile.rotation), 1, 1))
                    {
                        Main.projectile[owner].Kill();
                        break;
                    }
                }
            }
            if (length < 32)
            {
                int dust = Dust.NewDust(Projectile.Center + (new Vector2(length - Projectile.width / 2, 0)).RotatedBy(Projectile.rotation), 0, 0, DustID.Electric, 0f, 0f, 0, new Color(63, 63, 255), 1.2f);
                Main.dust[dust].noGravity = true;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            return Collision.CheckAABBvLineCollision(new Vector2(targetHitbox.X, targetHitbox.Y), targetHitbox.Size(), Projectile.Center, Projectile.Center + (new Vector2(length, 0)).RotatedBy(Projectile.rotation), 8, ref point);
        }

        public override bool PreDraw(ref Color lightColor)
        {

            Vector2 drawOrigin = new Vector2(Projectile.width / 2, Projectile.height / 2);
            Vector2 drawPos = Projectile.position - Main.screenPosition + drawOrigin;
            Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, drawPos, new Rectangle(0, 0, length, 8), Color.White, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }

        /*public override void OnHitPlayer(Player target, int damage, bool crit) {
			Main.projectile[owner].Kill();
		}*/
    }

    public class SparkCrawlerLightningHead : ModProjectile
    {
        private int timer;

        private float lightningPointX
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private float lightningPointY
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.alpha = 0;
            Projectile.timeLeft = 240;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;

                SoundEngine.PlaySound(SoundID.Item122, Projectile.Center);
            }

            timer = 0;

            if (timer == 0)
            {
                //need to find angles such that the point of distance c from lightningPoint is within distance r of position: Cos(2theta)=(c^2+d^2-r^2)/2cd

                float segmentLength = 24;
                float variance = 32;
                float dist = (float)Math.Sqrt((lightningPointX - Projectile.position.X) * (lightningPointX - Projectile.position.X) + (lightningPointY - Projectile.position.Y) * (lightningPointY - Projectile.position.Y));

                float theta = 0;

                if (dist > variance + segmentLength || segmentLength > variance + dist)
                {
                    theta = 0;
                }
                else if (variance > dist + segmentLength)
                {
                    theta = (float)Math.PI;
                }
                else
                {
                    theta = (float)Math.Acos((segmentLength * segmentLength + dist * dist - variance * variance) / (2 * segmentLength * dist));
                }

                float dirToProjectile = (float)Math.Atan((Projectile.position.Y - lightningPointY) / (Projectile.position.X - lightningPointX));

                if (Projectile.position.X - lightningPointX < 0)
                {
                    dirToProjectile += (float)Math.PI;
                }

                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.netUpdate = true;

                    float angle = dirToProjectile + 2 * theta * (float)Main.rand.NextDouble() - theta;

                    int segment = Projectile.NewProjectile(Projectile.GetSource_FromAI(), lightningPointX, lightningPointY, 0, 0, ProjectileType<SparkCrawlerLightning>(), 30, Projectile.knockBack, Projectile.owner, angle, Projectile.whoAmI);
                    Main.projectile[segment].rotation = angle;

                    lightningPointX += (float)Math.Cos(angle) * segmentLength;
                    lightningPointY += (float)Math.Sin(angle) * segmentLength;
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.timeLeft > 4)
            {
                Projectile.timeLeft = 4;
                SoundEngine.PlaySound(SoundID.Item122, Projectile.position);
                for (int i = 0; i < 128; i++)
                {
                    Dust dust = Main.dust[Dust.NewDust(Projectile.position - new Vector2(32, 32), 64, 64, 226, 0f, 0f, 0, new Color(63, 63, 255), 2f)];
                    dust.noGravity = true;
                    dust.scale = 1.2f;
                }
            }
            return false;
        }
    }
}