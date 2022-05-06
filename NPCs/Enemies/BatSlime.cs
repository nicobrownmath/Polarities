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
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader.Utilities;
using Terraria.GameContent.ItemDropRules;

namespace Polarities.NPCs.Enemies
{
    public class BatSlime : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 6;

            PolaritiesNPC.customSlimes.Add(Type);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                //spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns,
				//flavor text
				this.TranslatedBestiaryEntry()
            });
        }

        private float[] slimeAI = new float[4];
        private int batAItimer;

        public override void SetDefaults()
        {
            NPC.aiStyle = 1;
            NPC.width = 32;
            NPC.height = 24;
            DrawOffsetY = 4;
            NPC.defense = 0;
            NPC.damage = 25;
            NPC.lifeMax = 100;
            NPC.knockBackResist = 0.5f;
            NPC.npcSlots = 1f;
            NPC.noGravity = false;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath4;
            NPC.value = Item.buyPrice(silver: 10);

            Banner = NPC.type;
            BannerItem = ItemType<BatSlimeBanner>();
        }

        public override bool PreAI()
        {
            if (NPC.aiStyle == 1)
            {
                if (slimeAI[2] > 1f)
                {
                    slimeAI[2] -= 1f;
                }
                NPC.aiAction = 0;
                if (slimeAI[2] == 0f)
                {
                    slimeAI[0] = -100f;
                    slimeAI[2] = 1f;
                    NPC.TargetClosest();
                }
                if (NPC.velocity.Y == 0f)
                {
                    if (NPC.collideY && NPC.oldVelocity.Y != 0f && Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                    {
                        NPC.position.X -= NPC.velocity.X + (float)NPC.direction;
                    }
                    if (slimeAI[3] == NPC.position.X)
                    {
                        NPC.direction *= -1;
                        slimeAI[2] = 200f;
                    }
                    slimeAI[3] = 0f;
                    NPC.velocity.X *= 0.8f;
                    if ((double)NPC.velocity.X > -0.1 && (double)NPC.velocity.X < 0.1)
                    {
                        NPC.velocity.X = 0f;
                    }
                    slimeAI[0] += 2f;
                    int jumpState = 0;
                    if (slimeAI[0] >= 0f)
                    {
                        jumpState = 1;
                    }
                    if (slimeAI[0] >= -1000f && slimeAI[0] <= -500f)
                    {
                        jumpState = 2;
                    }
                    if (slimeAI[0] >= -2000f && slimeAI[0] <= -1500f)
                    {
                        jumpState = 3;
                    }
                    if (jumpState > 0)
                    {
                        NPC.netUpdate = true;
                        if (slimeAI[2] == 1f)
                        {
                            NPC.TargetClosest();
                        }
                        if (jumpState == 3)
                        {
                            NPC.aiStyle = 14;
                            batAItimer = 0;
                            NPC.velocity.Y = -8f;
                            NPC.velocity.X += (float)(3 * NPC.direction);
                            slimeAI[0] = -200f;
                            slimeAI[3] = NPC.position.X;
                        }
                        else
                        {
                            NPC.velocity.Y = -6f;
                            NPC.velocity.X += (float)(3 * NPC.direction);
                            slimeAI[0] = -120f;
                            if (jumpState == 1)
                            {
                                slimeAI[0] -= 1000f;
                            }
                            else
                            {
                                slimeAI[0] -= 2000f;
                            }
                        }
                    }
                    else if (slimeAI[0] >= -30f)
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
                NPC.rotation = 0;

            }
            else if (NPC.aiStyle == 14)
            {
                batAItimer++;
                NPC.rotation = NPC.velocity.X / 8f;
                if (batAItimer == 240)
                {
                    NPC.aiStyle = 1;
                }
            }
            NPC.noGravity = (NPC.aiStyle == 14);
            return NPC.aiStyle == 14;
        }

        public override void FindFrame(int frameHeight)
        {
            if (NPC.aiStyle == 1)
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
                NPC.spriteDirection = NPC.direction;
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
                if (NPC.frame.Y >= frameHeight * 2)
                {
                    NPC.frame.Y = 0;
                }
            }
            else
            {
                NPC.spriteDirection = NPC.direction;
                if (batAItimer % 5 == 0)
                {
                    NPC.frame.Y += frameHeight;
                }
                if (NPC.frame.Y >= frameHeight * 6)
                {
                    NPC.frame.Y = frameHeight * 2;
                }
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (Main.expertMode && Main.rand.NextBool(10))
                target.AddBuff(BuffID.Rabies, Main.rand.Next(1800, 5400));
        }

        public override bool CheckDead()
        {
            for (int i = 1; i <= 2; i++)
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("BatSlimeGore").Type);
            return true;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneTowerNebula || spawnInfo.Player.ZoneTowerSolar || spawnInfo.Player.ZoneTowerStardust || spawnInfo.Player.ZoneTowerVortex)
            {
                return 0f;
            }

            if (PolaritiesSystem.downedGigabat)
            {
                return SpawnCondition.Cavern.Chance * 0.1f;
            }
            else
            {
                return 0f;
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemType<GigabatSummonItem>(), 10));
        }
    }
}