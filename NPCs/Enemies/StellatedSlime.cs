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
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;

namespace Polarities.NPCs.Enemies
{
    public class StellatedSlime : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 2;

            PolaritiesNPC.customSlimes.Add(Type);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                //spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				//flavor text
				this.TranslatedBestiaryEntry()
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = 1;
            NPC.width = 32;
            NPC.height = 24;
            NPC.defense = 0;
            NPC.damage = 25;
            NPC.lifeMax = 100;
            NPC.knockBackResist = 0.5f;
            NPC.npcSlots = 1f;
            NPC.noGravity = false;
            NPC.HitSound = SoundID.Item9;
            NPC.DeathSound = SoundID.Item10;
            NPC.value = Item.buyPrice(0, 0, 5, 0);

            NPC.alpha = 96;

            Banner = NPC.type;
            BannerItem = ItemType<StellatedSlimeBanner>();
        }

        public override bool PreAI()
        {
            NPC.noGravity = true;
            if (NPC.ai[2] > 1f)
            {
                NPC.ai[2] -= 1f;
            }
            NPC.aiAction = 0;
            if (NPC.ai[2] == 0f)
            {
                NPC.ai[0] = -100f;
                NPC.ai[2] = 1f;
                NPC.TargetClosest();
            }
            if (NPC.velocity.Y == 0f)
            {
                if (NPC.collideY && NPC.oldVelocity.Y != 0f && Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                {
                    NPC.position.X -= NPC.velocity.X + (float)NPC.direction;
                }
                if (NPC.ai[3] == NPC.position.X)
                {
                    NPC.direction *= -1;
                    NPC.ai[2] = 200f;
                }
                NPC.ai[3] = 0f;
                NPC.velocity.X *= 0.8f;
                if ((double)NPC.velocity.X > -0.1 && (double)NPC.velocity.X < 0.1)
                {
                    NPC.velocity.X = 0f;
                }
                NPC.ai[0] += 2f;
                int jumpState = 0;
                if (NPC.ai[0] >= 0f)
                {
                    jumpState = 1;
                }
                if (NPC.ai[0] >= -1000f && NPC.ai[0] <= -500f)
                {
                    jumpState = 2;
                }
                if (NPC.ai[0] >= -2000f && NPC.ai[0] <= -1500f)
                {
                    jumpState = 3;
                }
                if (jumpState > 0)
                {
                    NPC.netUpdate = true;
                    if (NPC.ai[2] == 1f)
                    {
                        NPC.TargetClosest();
                    }
                    if (jumpState == 3)
                    {
                        NPC.velocity.Y = -16f;
                        NPC.velocity.X += (float)(3 * NPC.direction);
                        NPC.ai[0] = -200f;
                        NPC.ai[3] = NPC.position.X;
                    }
                    else
                    {
                        NPC.velocity.Y = -12f;
                        NPC.velocity.X += (float)(3 * NPC.direction);
                        NPC.ai[0] = -120f;
                        if (jumpState == 1)
                        {
                            NPC.ai[0] -= 1000f;
                        }
                        else
                        {
                            NPC.ai[0] -= 2000f;
                        }
                    }
                }
                else if (NPC.ai[0] >= -30f)
                {
                    NPC.aiAction = 1;
                }
            }
            else if (NPC.target < 255 && ((NPC.direction == 1 && NPC.velocity.X < 3f) || (NPC.direction == -1 && NPC.velocity.X > -3f)))
            {
                if (NPC.collideX && Math.Abs(NPC.velocity.X) == 0.2f)
                {
                    NPC.position.X -= 1.4f * (float)NPC.direction;
                }
                if (NPC.collideY && NPC.oldVelocity.Y != 0f && Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                {
                    NPC.position.X -= NPC.velocity.X + (float)NPC.direction;
                }
                if ((NPC.direction == -1 && (double)NPC.velocity.X < 0.01) || (NPC.direction == 1 && (double)NPC.velocity.X > -0.01))
                {
                    NPC.velocity.X += 0.2f * (float)NPC.direction;
                }
                else
                {
                    NPC.velocity.X *= 0.93f;
                }
            }

            NPC.velocity.Y += 0.50f;

            if (Main.dayTime)
            {
                //spawn dusts and despawn
                for (int i = 0; i < 30; i++)
                {
                    Vector2 position30 = NPC.position;
                    int width27 = NPC.width;
                    int height27 = NPC.height;
                    float speedX13 = NPC.velocity.X * 0.5f;
                    float speedY13 = NPC.velocity.Y * 0.5f;
                    Color newColor = default(Color);
                    Dust.NewDust(position30, width27, height27, 58, speedX13, speedY13, 150, newColor, 1.2f);
                }
                for (int i = 0; i < 15; i++)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center + new Vector2(-8, NPC.height / 2), new Vector2(NPC.velocity.X * 0.2f, NPC.velocity.Y * 0.2f), Main.rand.Next(16, 18));
                }

                NPC.active = false;
            }

            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            int num2 = 0;
            if (NPC.aiAction == 0)
            {
                num2 = ((NPC.velocity.Y < 0f) ? 2 : ((NPC.velocity.Y > 0f) ? 3 : ((NPC.velocity.X != 0f) ? 1 : 0)));
            }
            else if (NPC.aiAction == 1)
            {
                num2 = 4;
            }

            NPC.frameCounter++;
            if (num2 > 0)
            {
                NPC.frameCounter++;
            }
            if (num2 == 4)
            {
                NPC.frameCounter++;
            }
            if (NPC.frameCounter >= 8.0)
            {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0.0;
            }
            if (NPC.frame.Y >= frameHeight * Main.npcFrameCount[NPC.type])
            {
                NPC.frame.Y = 0;
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneTowerNebula || spawnInfo.Player.ZoneTowerSolar || spawnInfo.Player.ZoneTowerStardust || spawnInfo.Player.ZoneTowerVortex)
            {
                return 0f;
            }

            if (PolaritiesSystem.downedStarConstruct)
            {
                return Terraria.ModLoader.Utilities.SpawnCondition.OverworldNightMonster.Chance * 0.125f;
            }
            else
            {
                return 0f;
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.FallenStar));
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D star = TextureAssets.Item[ItemID.FallenStar].Value;
            Rectangle frame = star.Frame(1, 8, 0, (int)(Main.timeForVisualEffects / Main.itemAnimations[ItemID.FallenStar].TicksPerFrame) % 8);
            Vector2 drawOrigin = frame.Size() / 2;
            float scale = NPC.scale * 0.5f;

            Vector2 drawPos = NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY) + new Vector2(0, 2);

            float num10 = (float)Main.timeForVisualEffects / 240f + Main.GlobalTimeWrappedHourly * 0.04f;
            float globalTimeWrappedHourly2 = Main.GlobalTimeWrappedHourly;
            globalTimeWrappedHourly2 %= 5f;
            globalTimeWrappedHourly2 /= 2.5f;
            if (globalTimeWrappedHourly2 >= 1f)
            {
                globalTimeWrappedHourly2 = 2f - globalTimeWrappedHourly2;
            }
            globalTimeWrappedHourly2 = globalTimeWrappedHourly2 * 0.5f + 0.5f;
            for (float num11 = 0f; num11 < 1f; num11 += 0.25f)
            {
                spriteBatch.Draw(star, drawPos + Utils.RotatedBy(new Vector2(0f, 8f), (num11 + num10) * ((float)Math.PI * 2f)) * scale * globalTimeWrappedHourly2, frame, new Color(50, 50, 255, 50), 0f, drawOrigin, scale, (SpriteEffects)0, 0f);
            }
            for (float num12 = 0f; num12 < 1f; num12 += 0.34f)
            {
                spriteBatch.Draw(star, drawPos + Utils.RotatedBy(new Vector2(0f, 4f), (num12 + num10) * ((float)Math.PI * 2f)) * scale * globalTimeWrappedHourly2, frame, new Color(120, 120, 255, 127), 0f, drawOrigin, scale, (SpriteEffects)0, 0f);
            }

            spriteBatch.Draw(star, drawPos, frame, Color.White, NPC.rotation, drawOrigin, NPC.scale * 0.5f, SpriteEffects.None, 0f);

            return true;
        }
    }
}