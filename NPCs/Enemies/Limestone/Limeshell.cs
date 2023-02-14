using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Biomes;
using Polarities.Items.Accessories;
using Polarities.Items.Consumables;
using Polarities.Items.Materials;
using Polarities.Items.Placeable.Banners;
using Polarities.Items.Placeable.Blocks;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.NPCs.Enemies.Limestone
{
    public class Limeshell : ModNPC
    {
        private int attackPattern
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }
        private int attackTimer
        {
            get => (int)NPC.ai[1];
            set => NPC.ai[1] = value;
        }


        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;

            NPCID.Sets.TrailCacheLength[NPC.type] = 3;
            NPCID.Sets.TrailingMode[NPC.type] = 0;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				//flavor text
				this.TranslatedBestiaryEntry()
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = 14;
            NPC.width = 48;
            NPC.height = 48;
            DrawOffsetY = 4;

            NPC.damage = 80;
            NPC.lifeMax = 400;
            NPC.defense = 10;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 1f;
            NPC.value = 1000;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath52;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = false;

            Banner = NPC.type;
            BannerItem = ItemType<LimeshellBanner>();

            SpawnModBiomes = new int[1] { GetInstance<LimestoneCave>().Type };
        }

        public override bool PreAI()
        {
            switch (attackPattern)
            {
                case 0:
                    NPC.defense = 9999;
                    NPC.knockBackResist = 0f;
                    NPC.velocity.Y += 0.3f;

                    NPC.rotation += NPC.velocity.X * 0.1f;

                    attackTimer++;
                    if (NPC.collideY || attackTimer > 240)
                    {
                        if (NPC.collideY) { SoundEngine.PlaySound(SoundID.Tink, NPC.Center); }

                        attackTimer = 0;
                        attackPattern = 1;

                        NPC.TargetClosest(false);
                    }
                    break;

                case 1:
                    Lighting.AddLight(NPC.Center, 173f / 255, 217f / 255, 160f / 255);
                    NPC.defense = 10;
                    NPC.knockBackResist = 0.5f;
                    NPC.noGravity = true;

                    NPC.velocity *= 0.99f;
                    if (NPC.Center.Y > Main.player[NPC.target].Center.Y)
                    {
                        NPC.velocity.Y *= 1.01f;
                    }
                    else
                    {
                        NPC.velocity.Y *= 0.99f;
                    }

                    attackTimer++;
                    if (attackTimer == 80 || attackTimer == 100)
                    {
                        //shoot projectile
                        SoundEngine.PlaySound(SoundID.Item33, NPC.position);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (Main.player[NPC.target].Center - NPC.Center).SafeNormalize(Vector2.Zero) * 12, ProjectileType<LimeshellProjectile>(), 25, 1, Main.myPlayer);

                    }
                    else if (attackTimer > 120)
                    {
                        attackTimer = 0;
                        attackPattern = 2;
                    }
                    NPC.rotation = 0;

                    break;

                case 2:
                    Lighting.AddLight(NPC.Center, 173f / 512, 217f / 512, 160f / 512);
                    NPC.defense = 20;
                    NPC.knockBackResist = 0.25f;
                    NPC.noGravity = true;

                    NPC.velocity *= 0.98f;
                    NPC.velocity.Y *= 0.98f;

                    attackTimer++;
                    if (attackTimer > 60)
                    {
                        NPC.TargetClosest(false);
                        Player player = Main.player[NPC.target];

                        float a = 0.15f;
                        float v = 8;
                        float x = player.Center.X - NPC.Center.X;
                        float y = player.Center.Y - NPC.Center.Y;

                        NPC.direction = x < 0 ? -1 : 1;

                        double theta = (new Vector2(x, y)).ToRotation();
                        theta += Math.PI / 2;
                        if (theta > Math.PI) { theta -= Math.PI * 2; }
                        theta *= 0.5;
                        theta -= Math.PI / 2;

                        double num0 = -4 * Math.Pow(a, 2) * Math.Pow(x, 4) + 4 * a * Math.Pow(v, 2) * Math.Pow(x, 2) * y + Math.Pow(v, 4) * Math.Pow(x, 2);
                        if (num0 > 0)
                        {
                            num0 = NPC.direction * Math.Sqrt(num0);
                            double num1 = a * x * x - v * v * y;

                            theta = -2 * Math.Atan(
                                num0 / (2 * num1) +
                                0.5 * Math.Sqrt(Math.Max(0,
                                    -(
                                        (num1 * (-16 * Math.Pow(v, 2) * x * (a * x * x + v * v * y) / Math.Pow(num1, 2) - 16 * Math.Pow(v, 2) * x / num1 + 8 * Math.Pow(v, 6) * Math.Pow(x, 3) / Math.Pow(num1, 3))) /
                                        (4 * num0)
                                    )
                                    - 2 * (a * x * x + v * v * y) / num1 + 2 + 2 * Math.Pow(v, 4) * Math.Pow(x, 2) / (num1 * num1)
                                )) -
                                Math.Pow(v, 2) * x / (2 * num1)
                            ); //some magic thingy idk

                            int thetaDir = Math.Cos(theta) < 0 ? -1 : 1;
                            if (thetaDir != NPC.direction) { theta -= Math.PI; }
                        }

                        NPC.velocity = v * (new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)));

                        attackPattern = 0;
                        attackTimer = 0;
                    }

                    break;
            }

            if (NPC.collideX)
            {
                NPC.velocity.X = -NPC.oldVelocity.X / 2;
            }
            if (NPC.collideY)
            {
                NPC.velocity.Y = -NPC.oldVelocity.Y / 2;
            }

            return false;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!Main.hardMode) { return 0f; }
            Tile playerTile = Main.tile[(int)(spawnInfo.Player.Center.X / 16), (int)((spawnInfo.Player.Center.Y + 1 + spawnInfo.Player.height / 2) / 16)];
            if (spawnInfo.Player.InModBiome(GetInstance<LimestoneCave>()) && (spawnInfo.SpawnTileType == TileType<LimestoneTile>() || playerTile.TileType == TileType<LimestoneTile>()))
            {
                return 1f;
            }
            return 0f;
        }

        public override void FindFrame(int frameHeight)
        {
            if (Main.player[NPC.target].Center.X > NPC.Center.X)
            {
                NPC.spriteDirection = 1;
            }
            else
            {
                NPC.spriteDirection = -1;
            }
        }

        public override bool CheckDead()
        {
            for (int i = 1; i <= 2; i++)
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("LimeshellGore" + i).Type);
            return true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemType<Items.Placeable.Blocks.Limestone>(), 1, 5, 10));
            npcLoot.Add(ItemDropRule.Common(ItemType<LimestoneCarapace>(), 1, 1, 2));
            npcLoot.Add(ItemDropRule.Common(ItemType<LimestoneShield>(), 10));
            npcLoot.Add(ItemDropRule.Common(ItemType<KeyLimePie>(), 50));
        }

        private static Asset<Texture2D> BackTexture;
        private static Asset<Texture2D> MaskTexture;
        private static Asset<Texture2D> TopTexture;
        private static Asset<Texture2D> TrailTexture;
        private static Asset<Texture2D> BottomTexture;

        public override void Load()
        {
            BackTexture = Request<Texture2D>(Texture + "_Back");
            MaskTexture = Request<Texture2D>(Texture + "_Mask");
            TopTexture = Request<Texture2D>(Texture + "_Top");
            TrailTexture = Request<Texture2D>(Texture + "_Trail");
            BottomTexture = Request<Texture2D>(Texture + "_Bottom");
        }

        public override void Unload()
        {
            BackTexture = null;
            MaskTexture = null;
            TopTexture = null;
            TrailTexture = null;
            BottomTexture = null;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
            {
                attackPattern = 1;
            }

            Texture2D texture0 = BackTexture.Value;
            Texture2D texture1 = TextureAssets.Npc[Type].Value;
            Texture2D texture2 = MaskTexture.Value;
            Texture2D texture3 = TopTexture.Value;
            Texture2D texture4 = TrailTexture.Value;
            Texture2D texture5 = BottomTexture.Value;
            Rectangle frame = texture1.Frame();
            Vector2 origin = frame.Size() / 2;
            Vector2 drawPos = NPC.Center - screenPos;
            Color color = NPC.GetAlpha(NPC.GetNPCColorTintedByBuffs(drawColor));
            Color glowColor = NPC.GetAlpha(NPC.GetNPCColorTintedByBuffs(Color.White));
            SpriteEffects spriteEffects = (NPC.spriteDirection == 1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Vector2 capVector = new Vector2(0, 9);

            if (attackPattern == 2)
            {
                capVector.Y = 9 * (60 - attackTimer) / 60f;
            }

            if (attackPattern == 0)
            {
                for (int i = 1; i < NPC.oldPos.Length; i++)
                {
                    spriteBatch.Draw(texture4, NPC.oldPos[i] - NPC.position + drawPos, frame, color * ((NPC.oldPos.Length - i) / (float)NPC.oldPos.Length), NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
                }

                capVector.Y = 0;
            }

            spriteBatch.Draw(texture0, drawPos, frame, color, NPC.rotation, origin - capVector, NPC.scale, spriteEffects, 0f);

            if (attackPattern != 0)
            {
                spriteBatch.Draw(texture1, drawPos, frame, color, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);

                if (attackPattern == 1 && ((attackTimer >= 80 && attackTimer < 90) || (attackTimer >= 100 && attackTimer < 110)))
                    spriteBatch.Draw(texture2, drawPos, frame, glowColor, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
            }

            spriteBatch.Draw(texture5, drawPos, frame, color, NPC.rotation, origin - capVector, NPC.scale, spriteEffects, 0f);
            spriteBatch.Draw(texture3, drawPos, frame, color, NPC.rotation, origin + capVector, NPC.scale, spriteEffects, 0f);

            return false;
        }
    }

    public class LimeshellProjectile : ModProjectile
    {
        public override string Texture => "Polarities/Items/Weapons/Ranged/AlkalineRainProjectile";

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            DrawOffsetX = -46;
            DrawOriginOffsetY = 0;
            DrawOriginOffsetX = 23;
            Projectile.aiStyle = -1;
            Projectile.alpha = 255;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 173f / 255, 217f / 255, 160f / 255);
            Projectile.rotation = Projectile.velocity.ToRotation();

            Projectile.alpha = Math.Max(0, Projectile.alpha - 16);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            return true;
        }

        public override void Kill(int timeLeft)
        {
            Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 74, Scale: 1f)].noGravity = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.Draw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 48, 2), Color.White * ((255f - Projectile.alpha) / 255f), Projectile.rotation, new Vector2(47, 1), Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}