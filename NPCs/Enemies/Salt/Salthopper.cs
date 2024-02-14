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
using Polarities.Tiles;
using Polarities.Items.Weapons;
using Polarities.Items.Armor;
using Polarities.Items.Placeable.Banners;
using Terraria.GameContent.Bestiary;
using Terraria.Audio;
using Polarities.Items.Placeable.Blocks;
using Terraria.GameContent.ItemDropRules;
using Polarities.Items.Consumables;
using Polarities.Biomes;
using Polarities.Items.Weapons.Summon.Sentries;
using Polarities.Items.Accessories;

namespace Polarities.NPCs.Enemies.Salt
{
    public class Salthopper : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 3;

            PolaritiesNPC.customSlimes.Add(Type);
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
            NPC.aiStyle = 1;
            NPC.width = 36;
            NPC.height = 28;
            NPC.defense = 8;
            NPC.damage = 16;
            NPC.lifeMax = 80;
            NPC.knockBackResist = 0.3f;
            NPC.npcSlots = 1f;
            NPC.noGravity = false;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 5, 0);

            Banner = NPC.type;
            BannerItem = ItemType<SalthopperBanner>();

            SpawnModBiomes = new int[1] { GetInstance<SaltCave>().Type };
        }

        public override bool PreAI()
        {
            NPC.noGravity = NPC.wet && NPC.velocity.Y < 0;
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
                int num19 = 0;
                if (NPC.ai[0] >= 0f)
                {
                    num19 = 1;
                }
                if (NPC.ai[0] >= -1000f && NPC.ai[0] <= -500f)
                {
                    num19 = 2;
                }
                if (NPC.ai[0] >= -2000f && NPC.ai[0] <= -1500f)
                {
                    num19 = 3;
                }
                if (num19 > 0)
                {
                    NPC.netUpdate = true;
                    if (NPC.ai[2] == 1f)
                    {
                        NPC.TargetClosest();
                    }
                    if (num19 == 3)
                    {
                        NPC.velocity.Y = -8f;
                        NPC.velocity.X += (float)(2 * NPC.direction);
                        NPC.ai[0] = -200f;
                        NPC.ai[3] = NPC.position.X;
                    }
                    else
                    {
                        NPC.velocity.Y = -4f;
                        NPC.velocity.X += (float)(8 * NPC.direction);
                        NPC.ai[0] = -120f;
                        if (num19 == 1)
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
            NPC.spriteDirection = -NPC.direction;
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

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (Main.expertMode)
            {
                target.AddBuff(BuffType<Desiccating>(), 3 * 60);
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            Tile playerTile = Main.tile[(int)(spawnInfo.Player.Center.X / 16), (int)((spawnInfo.Player.Center.Y + 1 + spawnInfo.Player.height / 2) / 16)];
            if (spawnInfo.Player.InModBiome(GetInstance<SaltCave>()) && (spawnInfo.SpawnTileType == TileType<SaltTile>() || spawnInfo.SpawnTileType == TileType<RockSaltTile>() || playerTile.TileType == TileType<SaltTile>() || playerTile.TileType == TileType<RockSaltTile>()))
            {
                return 1f;
            }
            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemType<Items.Placeable.Blocks.Salt>(), 1, 5, 10));
            npcLoot.Add(ItemDropRule.Common(ItemType<SaltCrystals>(), 1, 2, 4));
            npcLoot.Add(ItemDropRule.Common(ItemType<HopperCrystal>(), 10));
            npcLoot.Add(ItemDropRule.Common(ItemType<Pretzel>(), 50));
        }
    }
}