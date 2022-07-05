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
using Polarities.Items.Weapons;
using Polarities.Items.Armor;
using Polarities.Items.Placeable.Banners;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using System.Collections.Generic;
using MultiHitboxNPCLibrary;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using ReLogic.Content;
using Polarities.Items.Materials;
using Terraria.Audio;
using Polarities.Effects;

namespace Polarities.NPCs.Enemies.Granite
{
    public class GraniteStomper : ModNPC
    {
        public override void SetStaticDefaults()
        {
            NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
            {
                SpecificallyImmuneTo = new int[] {
                    BuffID.Confused
                }
            };
            NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Granite,
				//flavor text
				this.TranslatedBestiaryEntry()
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 40;
            NPC.height = 40;

            NPC.defense = 15;
            NPC.damage = 60;
            NPC.lifeMax = 400;

            NPC.knockBackResist = 0f;
            NPC.value = Item.sellPrice(gold: 1);
            NPC.npcSlots = 2f;
            NPC.behindTiles = true;
            NPC.noGravity = false;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit7;
            NPC.DeathSound = SoundID.NPCDeath6;

            Banner = Type;
            //TODO: BannerItem = ItemType<GraniteStomperBanner>();
        }

        public override void OnSpawn(IEntitySource source)
        {
            NPC.ai[0] = 2;
            NPC.localAI[2] = 0f;
            NPC.localAI[3] = 20f;
            NPC.localAI[0] = 0.25f;
        }

        public override void AI()
        {
            NPC.noGravity = true;

            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.netUpdate = true;
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
            }

            bool groundFlag = NPC.velocity.Y == 0;
            NPC.dontTakeDamage = false;

            if (groundFlag)
            {
                switch (NPC.ai[0])
                {
                    case 0:
                        {
                            //jump at player
                            Vector2 jumpVelocity = new Vector2(12, 0).RotatedBy(Utils.AngleLerp((player.Center - NPC.Center).ToRotation(), -MathHelper.PiOver2, 0.66f));

                            float progress = NPC.ai[1] / 60f;
                            float offsetMultiplier = progress * progress * (3 - 2 * progress);
                            Vector2 bodyOffset = -jumpVelocity.SafeNormalize(Vector2.Zero) * offsetMultiplier * 16f;
                            NPC.localAI[2] = ModUtils.Lerp(NPC.localAI[2], bodyOffset.X, 0.05f);
                            NPC.localAI[3] = ModUtils.Lerp(NPC.localAI[3], bodyOffset.Y, 0.05f);

                            NPC.localAI[0] = ModUtils.Lerp(NPC.localAI[0], 1f, 0.05f);

                            if (NPC.ai[1] == 58)
                            {
                                NPC.velocity = jumpVelocity;
                            }

                            NPC.ai[1]++;
                            if (NPC.ai[1] == 60)
                            {
                                NPC.ai[0] = Collision.CanHit(NPC, player) ? Main.rand.Next(new int[] { 0, 1, 3 }) : 2;
                                NPC.ai[1] = 0;

                                if (NPC.ai[0] == 3 && player.Center.Y > NPC.Center.Y) NPC.ai[0] = Main.rand.Next(new int[] { 0, 1 });
                            }
                        }
                        break;
                    case 1:
                        {
                            //jump up before hiding
                            Vector2 jumpVelocity = new Vector2(0, -6);

                            float progress = NPC.ai[1] / 60f;
                            float offsetMultiplier = progress * progress * (3 - 2 * progress);
                            Vector2 bodyOffset = -jumpVelocity.SafeNormalize(Vector2.Zero) * offsetMultiplier * 16f;
                            NPC.localAI[2] = ModUtils.Lerp(NPC.localAI[2], bodyOffset.X, 0.05f);
                            NPC.localAI[3] = ModUtils.Lerp(NPC.localAI[3], bodyOffset.Y, 0.05f);

                            NPC.localAI[0] = ModUtils.Lerp(NPC.localAI[0], 1f, 0.05f);

                            if (NPC.ai[1] == 58)
                            {
                                NPC.velocity = jumpVelocity;
                            }

                            NPC.ai[1]++;
                            if (NPC.ai[1] == 60)
                            {
                                NPC.ai[0] = 2;
                                NPC.ai[1] = 0;

                                //produce homing crystal projectiles
                                int numProjectiles = Main.rand.Next(6, 10);
                                for (int i = 0; i < numProjectiles; i++)
                                {
                                    float shotProgress = Main.rand.NextFloat();
                                    Vector2 shotPosition = new Vector2(NPC.position.X + shotProgress * NPC.width, NPC.position.Y + NPC.height - 6);
                                    Vector2 shotVelocity = new Vector2(0, -Main.rand.NextFloat(1f, 6f)).RotatedBy((shotProgress - 0.5f) * 2f);
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), shotPosition, shotVelocity, ProjectileType<GraniteStomperHomingCrystal>(), 30, 2f, Main.myPlayer, ai0: player.whoAmI);
                                }
                            }
                        }
                        break;
                    case 2:
                        {
                            NPC.localAI[2] = ModUtils.Lerp(NPC.localAI[2], 0f, 0.05f);
                            NPC.localAI[3] = ModUtils.Lerp(NPC.localAI[3], 20f, 0.05f);
                            NPC.localAI[0] = ModUtils.Lerp(NPC.localAI[0], 0.25f, 0.05f);
                            NPC.dontTakeDamage = true;

                            //requires player to be in line of sight
                            NPC.ai[1]++;
                            if (NPC.ai[1] >= 180 && Collision.CanHit(NPC, player))
                            {
                                NPC.ai[0] = Main.rand.Next(new int[] { 0, 3 });
                                NPC.ai[1] = 0;

                                if (NPC.ai[0] == 3 && player.Center.Y > NPC.Center.Y) NPC.ai[0] = 0;
                            }
                        }
                        break;
                    case 3:
                        {
                            //laser attack
                            if (NPC.ai[1] == 60)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, -30), (player.Center - NPC.Center).SafeNormalize(Vector2.Zero), ProjectileType<GraniteStomperLaser>(), 30, 2f, Main.myPlayer, ai0: NPC.whoAmI);
                            }

                            NPC.localAI[2] *= 0.95f;
                            NPC.localAI[3] *= 0.95f;
                            NPC.localAI[0] = ModUtils.Lerp(NPC.localAI[0], 1f, 0.05f);

                            NPC.ai[1]++;
                            if (NPC.ai[1] >= 180)
                            {
                                NPC.ai[0] = Main.rand.Next(new int[] { 0, 1 });
                                NPC.ai[1] = 0;
                            }
                        }
                        break;
                }
            }
            else
            {
                NPC.rotation = -(float)Math.Atan(0.05f * NPC.velocity.Y * NPC.velocity.X) / MathHelper.TwoPi;

                NPC.velocity *= 0.99f;

                switch (NPC.ai[0])
                {
                    case 0:
                    case 3:
                        NPC.velocity.X += (player.Center.X > NPC.Center.X ? 1 : -1) * 0.01f;

                        NPC.localAI[2] *= 0.95f;
                        NPC.localAI[3] *= 0.95f;
                        NPC.localAI[0] = ModUtils.Lerp(NPC.localAI[0], 1f, 0.05f);
                        break;
                    case 1:
                    case 2:
                        NPC.localAI[2] = ModUtils.Lerp(NPC.localAI[2], 0f, 0.05f);
                        NPC.localAI[3] = ModUtils.Lerp(NPC.localAI[3], 20f, 0.05f);
                        NPC.localAI[0] = ModUtils.Lerp(NPC.localAI[0], 0.25f, 0.05f);
                        break;
                }
            }

            NPC.velocity.Y += 0.3f;

            if (NPC.velocity.X != 0 && Collision.TileCollision(NPC.position, new Vector2(NPC.velocity.X, 0), NPC.width, NPC.height) != new Vector2(NPC.velocity.X, 0))
            {
                NPC.velocity.X = Collision.TileCollision(NPC.position, new Vector2(NPC.velocity.X, 0), NPC.width, NPC.height).X;
                NPC.position.X += NPC.velocity.X;
                NPC.velocity.X = 0;
            }
            if (NPC.velocity.Y < 0 && Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height) != new Vector2(0, NPC.velocity.Y))
            {
                NPC.velocity.Y = Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height).Y;
                NPC.position.Y += NPC.velocity.Y;
                NPC.velocity.Y = 0;
            }
            if (NPC.velocity.Y > 0 && Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height) != new Vector2(0, NPC.velocity.Y))
            {
                NPC.velocity = Collision.TileCollision(NPC.position, new Vector2(0, NPC.velocity.Y), NPC.width, NPC.height);
                NPC.position.Y += NPC.velocity.Y;
                NPC.velocity.Y = 0;

                if (!groundFlag)
                {
                    MakeDusts();
                    NPC.frame.Y = NPC.frame.Height * 5;
                    NPC.rotation = 0f;
                }
            }
        }

        private void MakeDusts()
        {
            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/NPC_Killed_14")
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

        public override bool? CanFallThroughPlatforms()
        {
            return Main.player[NPC.target].Top.Y > NPC.Bottom.Y;
        }

        public static Asset<Texture2D> Leg1Texture;
        public static Asset<Texture2D> Leg2Texture;
        public static Asset<Texture2D> GlowTexture;

        public override void Load()
        {
            Leg1Texture = Request<Texture2D>(Texture + "_Leg1");
            Leg2Texture = Request<Texture2D>(Texture + "_Leg2");
            GlowTexture = Request<Texture2D>(Texture + "_Glow");
        }

        public override void Unload()
        {
            Leg1Texture = null;
            Leg2Texture = null;
            GlowTexture = null;
        }

        //TODO: Probably make it do something where the legs move more independently
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
        {
            //draw legs
            Texture2D leg1Texture = Leg1Texture.Value;
            Texture2D leg2Texture = Leg2Texture.Value;
            Texture2D texture = TextureAssets.Npc[Type].Value;

            Color drawColor = NPC.GetNPCColorTintedByBuffs(Lighting.GetColor(NPC.Center.ToTileCoordinates()));

            if (NPC.IsABestiaryIconDummy)
            {
                drawColor = Color.White;
                NPC.localAI[0] = 1f;
                NPC.localAI[2] = 0f;
                NPC.localAI[3] = 0f;
            }

            //draw back legs
            spriteBatch.Draw(leg1Texture, NPC.Center + new Vector2(-20 * NPC.scale * (0.5f + NPC.localAI[0]), 2).RotatedBy(NPC.rotation) - screenPos, leg1Texture.Frame(), drawColor, NPC.rotation, leg1Texture.Frame().Size() / 2, NPC.scale, SpriteEffects.None, 0);
            spriteBatch.Draw(leg1Texture, NPC.Center + new Vector2(20 * NPC.scale * (0.5f + NPC.localAI[0]), 2).RotatedBy(NPC.rotation) - screenPos, leg1Texture.Frame(), drawColor, NPC.rotation, leg1Texture.Frame().Size() / 2, NPC.scale, SpriteEffects.FlipHorizontally, 0);

            //draw body
            Vector2 bodyOffset = new Vector2(NPC.localAI[2], NPC.localAI[3] - 10);
            spriteBatch.Draw(texture, NPC.Center + bodyOffset - screenPos, texture.Frame(), drawColor, NPC.rotation, texture.Frame().Size() / 2, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            spriteBatch.Draw(GlowTexture.Value, NPC.Center + bodyOffset - screenPos, texture.Frame(), NPC.GetNPCColorTintedByBuffs(Color.White), NPC.rotation, texture.Frame().Size() / 2, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

            //draw front legs
            spriteBatch.Draw(leg2Texture, NPC.Center + new Vector2(-20 * NPC.scale * NPC.localAI[0], 2).RotatedBy(NPC.rotation) - screenPos, leg2Texture.Frame(), drawColor, NPC.rotation, leg2Texture.Frame().Size() / 2, NPC.scale, SpriteEffects.None, 0);
            spriteBatch.Draw(leg2Texture, NPC.Center + new Vector2(20 * NPC.scale * NPC.localAI[0], 2).RotatedBy(NPC.rotation) - screenPos, leg2Texture.Frame(), drawColor, NPC.rotation, leg2Texture.Frame().Size() / 2, NPC.scale, SpriteEffects.FlipHorizontally, 0);

            return false;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!Main.hardMode) return 0f;
            if (!spawnInfo.Granite) return 0f;

            return 0.1f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.Spaghetti, 50));
            npcLoot.Add(ItemDropRule.Common(ItemID.Granite, 1, 5, 10));
            npcLoot.Add(ItemDropRule.Common(ItemID.NightVisionHelmet, 30));
            npcLoot.Add(ItemDropRule.Common(ItemID.Geode, 20));
            //TODO: npcLoot.Add(ItemDropRule.Common(ItemType<RainbowQuartz>(), 2, 1, 2));
        }
    }

    public class GraniteStomperHomingCrystal : ModProjectile
    {
        public override string Texture => "Polarities/Items/Materials/BlueQuartz";

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = -1;
            Projectile.alpha = 0;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 600;
        }

        public override void AI()
        {
            Player player = Main.player[(int)Projectile.ai[0]];

            Projectile.ai[1]++;
            if (Projectile.ai[1] < 60)
            {
                Projectile.velocity *= 0.97f;
            }
            else
            {
                float progress = Projectile.ai[1] - 60;

                Vector2 goalVelocity = (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 12f;
                Projectile.velocity += (goalVelocity - Projectile.velocity) / (progress + 1);

                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 12f;
            }

            if (Projectile.ai[1] % 3 == 0)
            {
                Dust.NewDustPerfect(Projectile.Center, DustID.Electric, Vector2.Zero, Scale: 0.75f).noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 159f / 255f, 233f / 255f, 1f);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.ai[1] >= 60)
            {
                SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
                return true;
            }
            else
            {
                if (oldVelocity.X != Projectile.velocity.X) Projectile.velocity.X = -oldVelocity.X;
                if (oldVelocity.Y != Projectile.velocity.Y) Projectile.velocity.Y = -oldVelocity.Y;

                return false;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.Draw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), Color.White * ((255f - Projectile.alpha) / 255f), Projectile.rotation, new Vector2(6, 6), Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }

    public class GraniteStomperLaser : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 12;
            Projectile.height = 12;

            Projectile.alpha = 0;
            Projectile.timeLeft = 120;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = false;

            Projectile.GetGlobalProjectile<PolaritiesProjectile>().ForceDraw = true;
        }

        const float MAX_LENGTH = 4000;

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            if (!Main.npc[(int)Projectile.ai[0]].active)
            {
                Projectile.Kill();
                return;
            }

            Projectile.rotation += Projectile.ai[1] * 0.01f;

            if (Projectile.timeLeft == 60)
            {
                Projectile.ai[1] = Vector2.Dot(Main.player[Main.npc[(int)Projectile.ai[0]].target].Center - Projectile.Center, new Vector2(0, 1).RotatedBy(Projectile.rotation)) > 0 ? 1 : -1;
                SoundEngine.PlaySound(SoundID.Item122, Projectile.Center);
            }

            Projectile.Center = Main.npc[(int)Projectile.ai[0]].Center + new Vector2(0, -30);

            Vector2 step = new Vector2(1, 0).RotatedBy(Projectile.rotation);

            Projectile.velocity = Vector2.Zero;
            while (Collision.CanHitLine(Projectile.Center - new Vector2(0.5f) + Projectile.velocity, 1, 1, Projectile.Center - new Vector2(0.5f) + Projectile.velocity + step * 4, 1, 1) && Projectile.velocity.Length() < MAX_LENGTH)
            {
                Projectile.velocity += step * 4;
            }

            if (Projectile.timeLeft <= 60)
            {
                Vector2 speed = new Vector2(4, 0).RotatedBy(Projectile.rotation);
                Color dustColor = Color.Lerp(Main.rand.Next(new Color[] { Color.Red, Color.Lime, Color.Blue }), Color.White, Main.rand.NextFloat());
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.FireworksRGB, speed.X, speed.Y, newColor: dustColor, Scale: 1f);
                Main.dust[dust].noGravity = true;

                for (int i = 0; i < 2; i++)
                {
                    speed = new Vector2(-4, 0).RotatedBy(Projectile.rotation);
                    dustColor = Color.Lerp(Main.rand.Next(new Color[] { Color.Red, Color.Lime, Color.Blue }), Color.White, Main.rand.NextFloat());
                    dust = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.FireworksRGB, speed.X, speed.Y, newColor: dustColor, Scale: 1f);
                    Main.dust[dust].noGravity = true;
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.timeLeft <= 60)
                return Collision.CheckAABBvLineCollision(targetHitbox.TopRight(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity);
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.timeLeft <= 60)
            {
                float scaleMult = Math.Min(1f, Projectile.timeLeft / 10f);

                for (int i = 0; i < 3; i++)
                {
                    Color color;
                    switch (i)
                    {
                        case 0:
                            color = new Color(255, 0, 0, 127);
                            break;
                        case 1:
                            color = new Color(0, 255, 0, 127);
                            break;
                        default:
                            color = new Color(0, 0, 255, 127);
                            break;
                    }

                    Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition + new Vector2(0, 4f * scaleMult).RotatedBy(Projectile.rotation + i * MathHelper.TwoPi / 3f + PolaritiesSystem.timer * 0.1f), new Rectangle(0, 0, 12, 20), color, Projectile.rotation, new Vector2(0, 10), new Vector2(1, scaleMult), SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 12f - Main.screenPosition + new Vector2(0, 4f * scaleMult).RotatedBy(Projectile.rotation + i * MathHelper.TwoPi / 3f + PolaritiesSystem.timer * 0.1f), new Rectangle(12, 0, 12, 20), color, Projectile.rotation, new Vector2(0, 10), new Vector2((Projectile.velocity.Length() - 12) / 12f, scaleMult), SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center + Projectile.velocity - Main.screenPosition + new Vector2(0, 4f * scaleMult).RotatedBy(Projectile.rotation + i * MathHelper.TwoPi / 3f + PolaritiesSystem.timer * 0.1f), new Rectangle(24, 0, 12, 20), color, Projectile.rotation, new Vector2(0, 10), new Vector2(1, scaleMult), SpriteEffects.None, 0);
                }
                Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 12, 20), Color.White * 0.5f, Projectile.rotation, new Vector2(0, 10), new Vector2(1, scaleMult), SpriteEffects.None, 0);
                Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 12f - Main.screenPosition, new Rectangle(12, 0, 12, 20), Color.White * 0.5f, Projectile.rotation, new Vector2(0, 10), new Vector2((Projectile.velocity.Length() - 12) / 12f, scaleMult), SpriteEffects.None, 0);
                Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center + Projectile.velocity - Main.screenPosition, new Rectangle(24, 0, 12, 20), Color.White * 0.5f, Projectile.rotation, new Vector2(0, 10), new Vector2(1, scaleMult), SpriteEffects.None, 0);
            }
            else
            {
                float progressLeft = (Projectile.timeLeft - 60) / 60f;
                Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, new Rectangle(12, 0, 12, 20), Color.White * (1 - progressLeft), Projectile.rotation, new Vector2(0, 10), new Vector2(Projectile.velocity.Length() / 12f, progressLeft), SpriteEffects.None, 0);
            }

            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            DrawLayer.AddProjectile<DrawLayerAdditiveAfterProjectiles>(index);
        }

        public override bool ShouldUpdatePosition() => false;
    }
}
