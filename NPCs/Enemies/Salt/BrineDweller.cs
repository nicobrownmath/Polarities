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
using Terraria.GameContent;
using Polarities.Items.Placeable.Blocks;
using Polarities.Biomes;
using Terraria.GameContent.ItemDropRules;
using Polarities.Items.Materials;
using Polarities.Items.Consumables;
using Terraria.GameContent.Bestiary;

namespace Polarities.NPCs.Enemies.Salt
{
    public class BrineDweller : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 8;

            NPCID.Sets.TrailCacheLength[NPC.type] = 5;
            NPCID.Sets.TrailingMode[NPC.type] = 3;

            NPCID.Sets.CantTakeLunchMoney[Type] = true;
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
            NPC.aiStyle = -1;
            NPC.width = 44;
            NPC.height = 20;
            NPC.defense = 20;
            NPC.damage = 60;
            NPC.lifeMax = 360;
            NPC.knockBackResist = 0.5f;
            NPC.npcSlots = 1f;
            NPC.noGravity = false;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(silver: 10);

            Banner = NPC.type;
            BannerItem = ItemType<BrineDwellerBanner>();

            SpawnModBiomes = new int[1] { GetInstance<SaltCave>().Type };
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            if (NPC.alpha != 0) return false;
            return null;
        }

        public override void AI()
        {
            if (NPC.localAI[0] == 0)
            {
                NPC.localAI[0] = 1;

                //1/4 chance to spawn as pink or brown variant
                if (Main.rand.NextBool(4))
                {
                    NPC.frame.X = 76 * Main.rand.Next(1, 3);
                }
            }

            NPC.TargetClosest(false);
            Player target = Main.player[NPC.target];

            NPC.alpha = 0;

            if (NPC.wet)
            {
                NPC.noGravity = true;
                if (NPC.Distance(target.Center) < 600 && Collision.CanHitLine(NPC.position, NPC.width, NPC.height, target.position, target.width, target.height))
                {
                    NPC.ai[0]++;
                    if (NPC.ai[0] >= 30)
                    {
                        NPC.velocity = NPC.DirectionTo(target.Center) * 14f;

                        //stagger jump initialization with other brine dwellers
                        if (NPC.ai[0] == 30)
                        {
                            for (int i = 0; i < Main.maxNPCs; i++)
                            {
                                if (Main.npc[i].active && Main.npc[i].type == NPC.type && Main.npc[i].wet && Main.npc[i].ai[0] > 15 && Main.npc[i].ai[0] <= 30)
                                {
                                    Main.npc[i].ai[0] = 15;
                                }
                            }
                        }

                        if (NPC.ai[0] >= 60)
                        {
                            NPC.ai[0] = 0;
                        }
                    }
                    else
                    {
                        NPC.alpha = 160;
                    }
                }
                else
                {
                    NPC.velocity.Y += 0.05f;
                    NPC.velocity.Y *= 0.9f;
                    if (NPC.velocity.X > 0)
                    {
                        NPC.velocity.X = 2f;
                    }
                    else
                    {
                        NPC.velocity.X = -2f;
                    }
                    if (NPC.collideX)
                    {
                        NPC.velocity.X = -NPC.oldVelocity.X;
                    }

                    NPC.alpha = 160;

                    if (NPC.ai[0] < 0)
                    {
                        NPC.ai[0]++;
                    }
                    else
                    {
                        NPC.ai[0] = 0;
                    }
                }

                NPC.ai[1] = 0;
            }
            else
            {
                NPC.noGravity = false;

                if (NPC.velocity.Y == 0)
                {
                    NPC.velocity.X = 0;
                    NPC.ai[1]++;
                    if (NPC.ai[1] >= 15)
                    {
                        //random flopping
                        if (Main.rand.NextBool())
                        {
                            NPC.velocity = new Vector2(Main.rand.NextFloat(2f, 6f) * NPC.direction, -4f);
                        }
                        else
                        {
                            NPC.velocity = new Vector2(Main.rand.NextFloat(1f, 2f) * NPC.direction, -8f);
                        }
                    }
                }
                else
                {
                    NPC.ai[1] = 0;
                }

                if (NPC.collideX)
                {
                    NPC.velocity.X = (NPC.velocity.X > 0 ? -0.1f : 0.1f);
                }
                if (Math.Abs(NPC.velocity.X) < 0.1f)
                {
                    NPC.velocity.X = NPC.direction * 0.1f;
                }

                if (NPC.ai[0] >= 30)
                {
                    NPC.ai[0] = -90;
                }

                if (NPC.ai[0] < 0)
                {
                    NPC.ai[0]++;
                }
            }

            if (NPC.velocity.X > 0)
            {
                NPC.direction = 1;
            }
            else if (NPC.velocity.X < 0)
            {
                NPC.direction = -1;
            }
            NPC.spriteDirection = NPC.direction;

            NPC.rotation = NPC.velocity.ToRotation() + ((NPC.direction == 1) ? 0 : MathHelper.Pi);
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Width = 76;

            NPC.frameCounter++;
            if (NPC.frameCounter >= 6)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= frameHeight * 8)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Color color = NPC.GetAlpha(NPC.GetNPCColorTintedByBuffs(drawColor));
            Vector2 origin = new Vector2(38, 42);
            SpriteEffects spriteEffects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            for (int i = 1; i < NPC.oldPos.Length; i++)
            {
                float alpha = 0.5f * (1 - i / (float)NPC.oldPos.Length);
                spriteBatch.Draw(texture, NPC.oldPos[i] - NPC.position + NPC.Center - screenPos, NPC.frame, color * alpha, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
            }
            spriteBatch.Draw(texture, NPC.Center - screenPos, NPC.frame, color, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);

            return false;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (Main.expertMode)
            {
                target.AddBuff(BuffType<Desiccating>(), 4 * 60);
                target.AddBuff(BuffID.Confused, 1 * 60);
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!Main.hardMode)
            {
                return 0f;
            }

            Tile playerTile = Main.tile[(int)(spawnInfo.Player.Center.X / 16), (int)((spawnInfo.Player.Center.Y + 1 + spawnInfo.Player.height / 2) / 16)];
            if (spawnInfo.Player.InModBiome(GetInstance<SaltCave>()) && (spawnInfo.SpawnTileType == TileType<SaltTile>() || spawnInfo.SpawnTileType == TileType<RockSaltTile>() || playerTile.TileType == TileType<SaltTile>() || playerTile.TileType == TileType<RockSaltTile>()))
            {
                if (spawnInfo.Water)
                    return 4f;
            }
            return 0f;
        }

        public override int SpawnNPC(int tileX, int tileY)
        {
            if (Main.rand.NextBool())
                NPC.NewNPC(NPC.GetSource_NaturalSpawn(), tileX * 16 + 8, tileY * 16 + 8, NPC.type);
            if (Main.rand.NextBool())
                NPC.NewNPC(NPC.GetSource_NaturalSpawn(), tileX * 16 + 8, tileY * 16 - 8, NPC.type);

            return NPC.NewNPC(NPC.GetSource_NaturalSpawn(), tileX * 16 + 8, tileY * 16, NPC.type);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemType<CongealedBrine>(), 1, 2, 3));
            npcLoot.Add(ItemDropRule.Common(ItemType<SaltCrystals>(), 1, 3, 6));
            npcLoot.Add(ItemDropRule.Common(ItemType<Pretzel>(), 50));
        }
    }
}