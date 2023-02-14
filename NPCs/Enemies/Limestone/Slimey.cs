using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Biomes;
using Polarities.Effects;
using Polarities.Items.Consumables;
using Polarities.Items.Materials;
using Polarities.Items.Placeable.Banners;
using Polarities.Items.Placeable.Blocks;
using Polarities.Items.Weapons.Ranged.Atlatls;
using ReLogic.Content;
using System;
using System.Collections.Generic;
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
    public class Slimey : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;

            NPCID.Sets.TrailCacheLength[NPC.type] = 3;
            NPCID.Sets.TrailingMode[NPC.type] = 3;
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
            NPC.width = 32;
            NPC.height = 32;
            DrawOffsetY = 0;

            NPC.damage = 64;
            NPC.lifeMax = 400;
            NPC.defense = 40;
            NPC.knockBackResist = 0.01f;
            NPC.npcSlots = 1f;
            NPC.value = 1000;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath52;
            NPC.lavaImmune = true;
            NPC.noGravity = false;
            NPC.noTileCollide = false;

            Banner = NPC.type;
            BannerItem = ItemType<SlimeyBanner>();

            SpawnModBiomes = new int[1] { GetInstance<LimestoneCave>().Type };
        }

        private bool hostile => NPC.life < NPC.lifeMax;

        public override bool PreAI()
        {
            Lighting.AddLight(NPC.Center, 173f / 255, 217f / 255, 160f / 255);

            NPC.TargetClosest(false);
            Player player = Main.player[NPC.target];

            if (NPC.direction == 0) NPC.direction = Main.rand.Next(2) * 2 - 1;

            bool collidePlatforms = true;

            NPC.noTileCollide = false;
            NPC.noGravity = false;

            switch (NPC.ai[0])
            {
                //default behavior
                case 0:
                    float goalXVelocity = NPC.direction;
                    if (hostile) goalXVelocity *= 3;

                    NPC.velocity.X += (goalXVelocity - NPC.velocity.X) / 10f;

                    collidePlatforms = player.Hitbox.Top < NPC.Hitbox.Bottom;
                    if (!hostile) collidePlatforms = true;

                    if (NPC.collideX || NPC.velocity.X == 0)
                    {
                        NPC.direction *= -1;
                        NPC.spriteDirection *= -1;
                        NPC.velocity.X *= -1;
                    }

                    if (NPC.collideY)
                    {
                        NPC.rotation = 0;
                    }
                    else
                    {
                        NPC.rotation = NPC.velocity.ToRotation() + (NPC.spriteDirection == 1 ? 0 : MathHelper.Pi);
                    }

                    if (hostile)
                    {
                        NPC.ai[1]++;
                    }
                    if (NPC.ai[1] >= 240 && NPC.collideY || NPC.ai[1] >= 600)
                    {
                        NPC.ai[1] = 0;

                        NPC.ai[0] = Main.rand.Next(1, 3);

                        if (NPC.ai[1] >= 600) NPC.ai[0] = 1;
                    }
                    break;
                //leap!
                case 1:
                    if (NPC.ai[1] == 0)
                    {
                        NPC.direction = (player.Center.X > NPC.Center.X) ? 1 : -1;
                        NPC.ai[1]++;

                        for (int i = 0; i < 5; i++)
                            Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 74, Scale: 1f)].noGravity = true;

                        SoundEngine.PlaySound(SoundID.NPCHit54, NPC.Center);
                    }
                    if (NPC.ai[1] == 1)
                    {
                        NPC.noGravity = true;
                        NPC.noTileCollide = true;

                        Vector2 goalPosition = player.Center + new Vector2(0, -360);
                        Vector2 goalVelocity = (goalPosition - NPC.Center).SafeNormalize(Vector2.Zero) * 8f + (goalPosition - NPC.Center) / 120f;
                        goalVelocity.Y *= 2;
                        NPC.velocity = goalVelocity;

                        NPC.rotation = NPC.velocity.ToRotation() + (NPC.spriteDirection == 1 ? 0 : MathHelper.Pi);

                        if ((NPC.Center - goalPosition).Length() < 16)
                        {
                            NPC.ai[1] = 2;
                            NPC.Center = goalPosition;
                        }
                    }
                    else if (NPC.ai[1] < 32)
                    {
                        NPC.noGravity = true;
                        NPC.noTileCollide = true;

                        NPC.rotation += NPC.ai[1] * 0.035f * NPC.direction;
                        NPC.velocity = new Vector2(0, 0.0001f);

                        NPC.ai[1]++;
                    }
                    else
                    {
                        if (NPC.ai[1] == 32)
                        {
                            for (int i = 0; i < 5; i++)
                                Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 74, Scale: 1f)].noGravity = true;

                            SoundEngine.PlaySound(SoundID.NPCHit54, NPC.Center);
                        }

                        NPC.noGravity = true;

                        float oldYVel = NPC.velocity.Y;
                        NPC.velocity = new Vector2(0, 20);

                        NPC.noTileCollide = player.Hitbox.Top >= NPC.Hitbox.Bottom && Math.Abs(player.Center.X - NPC.Center.X) < (player.Hitbox.Top - NPC.Hitbox.Bottom) / 2f;
                        collidePlatforms = player.Hitbox.Top < NPC.Hitbox.Bottom;

                        if (!NPC.noTileCollide)
                        {
                            NPC.position += Collision.TileCollision(NPC.position, NPC.velocity / 2, NPC.width, NPC.height, fallThrough: !collidePlatforms, fall2: !collidePlatforms);
                            NPC.velocity = new Vector2(0, 10);
                        }

                        NPC.rotation = NPC.velocity.ToRotation() + (NPC.spriteDirection == 1 ? 0 : MathHelper.Pi);

                        NPC.ai[1]++;
                        if (oldYVel == 0 || NPC.ai[1] >= 240)
                        {
                            NPC.ai[1] = 0;
                            NPC.ai[0] = 0;

                            NPC.direction = Main.rand.Next(2) * 2 - 1;

                            SoundEngine.PlaySound(SoundID.NPCDeath14, NPC.Center);

                            for (int i = 0; i < 10; i++)
                                Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 74, Scale: 1f)].noGravity = true;
                        }
                    }
                    break;
                case 2:
                    NPC.velocity.X *= 0.95f;

                    if (NPC.collideY)
                    {
                        NPC.rotation = 0;
                    }
                    else
                    {
                        NPC.rotation = NPC.velocity.ToRotation() + (NPC.spriteDirection == 1 ? 0 : MathHelper.Pi);
                    }

                    if (NPC.ai[1] == 30)
                    {
                        for (int i = -1; i <= 1; i++)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<SlimeyProjectile>(), 24, 2f, player.whoAmI, i);
                        }

                        SoundEngine.PlaySound(SoundID.NPCHit54, NPC.Center);
                    }

                    NPC.ai[1]++;
                    if (NPC.ai[1] >= 120)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = 0;

                        NPC.direction = Main.rand.Next(2) * 2 - 1;
                    }
                    break;
            }

            NPC.spriteDirection = NPC.direction;
            NPC.aiStyle = collidePlatforms ? -1 : 14;
            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            int animSpeed = 8;
            if (hostile) animSpeed = 4;
            if (NPC.frameCounter >= animSpeed)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += NPC.frame.Height;
                if (NPC.frame.Y >= NPC.frame.Height * 4)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return NPC.ai[0] != 1 || NPC.ai[1] >= 32;
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

        public override bool CheckDead()
        {
            for (int i = 1; i <= 1; i++)
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("SlimeyGore" + i).Type);
            for (int i = 0; i < 10; i++)
                Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 74, Scale: 1f)].noGravity = true;

            return true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemType<AlkalineFluid>(), 1, 2, 3));
            npcLoot.Add(ItemDropRule.Common(ItemType<Items.Placeable.Blocks.Limestone>(), 1, 3, 6));
            npcLoot.Add(ItemDropRule.Common(ItemType<LimestoneCarapace>(), 1, 1, 2));
            npcLoot.Add(ItemDropRule.Common(ItemType<CausticSlug>(), 10));
            npcLoot.Add(ItemDropRule.Common(ItemType<KeyLimePie>(), 50));
        }

        private static Asset<Texture2D> MaskTexture;

        public override void Load()
        {
            MaskTexture = Request<Texture2D>(Texture + "_Mask");
        }

        public override void Unload()
        {
            MaskTexture = null;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Rectangle frame = NPC.frame;
            Vector2 center = frame.Size() / 2;

            Texture2D glowTexture = MaskTexture.Value;

            if (hostile)
            {
                Texture2D trailTexture = TextureAssets.Projectile[ProjectileType<SlimeyProjectile>()].Value;

                for (int i = 1; i < NPC.oldPos.Length; i++)
                {
                    spriteBatch.Draw(trailTexture, NPC.oldPos[i] - NPC.position + NPC.Center - screenPos, frame, NPC.GetAlpha(NPC.GetNPCColorTintedByBuffs(Color.White)) * (1 - i / (float)NPC.oldPos.Length), NPC.oldRot[i], center, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
                }
            }

            spriteBatch.Draw(texture, NPC.Center - screenPos, frame, NPC.GetAlpha(NPC.GetNPCColorTintedByBuffs(drawColor)), NPC.rotation, center, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            spriteBatch.Draw(glowTexture, NPC.Center - screenPos, frame, NPC.GetAlpha(NPC.GetNPCColorTintedByBuffs(Color.White)), NPC.rotation, center, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);

            return false;
        }
    }

    public class SlimeyProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;

            Projectile.aiStyle = -1;
            Projectile.alpha = 128;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;

            Projectile.timeLeft = 600;

            //Projectile.hide = true;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 173f / 255, 217f / 255, 160f / 255);

            Player player = Main.player[Projectile.owner];

            if (Projectile.ai[1] < 60)
            {
                if (Projectile.ai[1] == 0)
                {
                    Projectile.spriteDirection = (player.Center.X > Projectile.Center.X) ? -1 : 1;
                }

                Projectile.tileCollide = false;

                Vector2 goalPosition = player.Center + new Vector2(Projectile.ai[0] * Projectile.spriteDirection * 64, -360);
                Vector2 goalVelocity = (goalPosition - Projectile.Center) / (60 - Projectile.ai[1]);
                goalVelocity.Y *= 2;
                Projectile.velocity = goalVelocity;

                Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == -1 ? 0 : MathHelper.Pi);

                Projectile.ai[1]++;
                if (Projectile.ai[1] == 60)
                {
                    Projectile.Center = goalPosition;
                    Projectile.velocity = Vector2.Zero;
                }
            }
            else if (Projectile.ai[1] < 100 + Projectile.ai[0] * 8)
            {
                Projectile.tileCollide = false;

                Projectile.rotation -= Projectile.ai[1] * (1.12f / (100 + Projectile.ai[0] * 8)) * Projectile.spriteDirection;
                Projectile.velocity = Vector2.Zero;

                Projectile.ai[1]++;
            }
            else
            {
                if (Projectile.ai[1] == 100 + Projectile.ai[0] * 8)
                {
                    for (int i = 0; i < 5; i++)
                        Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 74, Scale: 1f)].noGravity = true;

                    SoundEngine.PlaySound(SoundID.NPCHit54, Projectile.Center);
                }

                Projectile.tileCollide = false;
                Projectile.velocity = new Vector2(0, 16);

                Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == -1 ? 0 : MathHelper.Pi);

                Projectile.ai[1]++;
            }

            if (Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }

            Projectile.hostile = (Projectile.ai[1] >= 100 + Projectile.ai[0] * 8);
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 74, Scale: 1f)].noGravity = true;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            DrawLayer.AddProjectile<DrawLayerAdditiveAfterProjectiles>(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, 4, 0, Projectile.frame);
            Vector2 center = frame.Size() / 2;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Main.spriteBatch.Draw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, Color.White * 0.5f, Projectile.oldRot[i], center, Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            }

            return false;
        }
    }
}
