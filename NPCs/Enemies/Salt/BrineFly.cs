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
using Terraria.GameContent.ItemDropRules;
using Polarities.Items.Placeable.Blocks;
using Polarities.Items.Consumables;
using Terraria.GameContent.Bestiary;
using Polarities.Biomes;

namespace Polarities.NPCs.Enemies.Salt
{
    public class BrineFly : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 2;

            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                SpriteDirection = 1
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifiers);
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
            NPC.stairFall = true;
            NPC.width = 10;
            NPC.height = 10;
            NPC.defense = 0;
            NPC.damage = 1;
            NPC.lifeMax = 6;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 0.05f;
            NPC.noGravity = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            NPC.dontCountMe = true;

            Banner = NPC.type;
            BannerItem = ItemType<BrineFlyBanner>();

            NPC.GetGlobalNPC<PolaritiesNPC>().usesProjectileHitCooldowns = true;
            NPC.GetGlobalNPC<PolaritiesNPC>().projectileHitCooldownTime = 10;

            SpawnModBiomes = new int[1] { GetInstance<SaltCave>().Type };
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.lifeMax = 6;
        }

        public override int SpawnNPC(int tileX, int tileY)
        {
            for (int i = 0; i < (Main.expertMode ? 19 : 9); i++)
            {
                NPC.NewNPC(NPC.GetSource_NaturalSpawn(), tileX * 16 + 8 - i, tileY * 16, NPC.type);
            }

            return NPC.NewNPC(NPC.GetSource_NaturalSpawn(), tileX * 16 + 8, tileY * 16, NPC.type);
        }

        public override bool PreAI()
        {
            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.netUpdate = true;
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
            }

            if (NPC.collideX)
            {
                NPC.velocity.X = -NPC.velocity.X;
            }
            if (NPC.collideY)
            {
                NPC.velocity.Y = -NPC.velocity.Y;
            }

            Boids(player.Center);

            if (NPC.wet)
            {
                NPC.velocity.Y -= 0.15f;
                if (NPC.velocity.Y > 0)
                {
                    NPC.velocity.Y *= 0.9f;
                }
            }

            NPC.spriteDirection = NPC.velocity.X > 0 ? -1 : 1;
            NPC.rotation = NPC.velocity.X / 10f;

            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter == 3)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y == 2 * frameHeight)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        private void Boids(Vector2 goalPosition)
        {
            //boids
            Vector2 separation = Vector2.Zero;
            Vector2 alignment = Vector2.Zero;
            Vector2 cohesion = Vector2.Zero;
            int count = 0;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC otherNPC = Main.npc[i];

                if (i != NPC.whoAmI && otherNPC.type == NPC.type && otherNPC.active && (NPC.Center - otherNPC.Center).Length() < 128)
                {
                    count++;

                    //separation component
                    separation += 32f * (NPC.Center - otherNPC.Center).SafeNormalize(Vector2.Zero) / (NPC.Center - otherNPC.Center).Length();

                    //alignment component
                    alignment += 1 / 6f * (otherNPC.velocity - NPC.velocity);

                    //cohesion component
                    cohesion += 1 / 24f * (otherNPC.Center - NPC.Center);
                }

            }

            if (count > 0)
            {
                alignment /= count;
                cohesion /= count;
            }

            Vector2 goalVelocity = NPC.velocity + separation + alignment + cohesion + (goalPosition - NPC.Center).SafeNormalize(Vector2.Zero);
            if (goalVelocity.Length() > 5)
            {
                goalVelocity.Normalize();
                goalVelocity *= 5;
            }
            NPC.velocity += (goalVelocity - NPC.velocity) / 20;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            if (Collision.CheckAABBvAABBCollision(target.position, new Vector2(target.width, target.height), NPC.position, new Vector2(NPC.width, NPC.height)))
            {
                if (Main.rand.NextBool(15))
                {
                    target.AddBuff(BuffID.Confused, 10, true);
                }
            }
            return false;
        }

        public override bool CheckDead()
        {
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("BrineFlyGore").Type);
            return true;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            Tile playerTile = Main.tile[(int)(spawnInfo.Player.Center.X / 16), (int)((spawnInfo.Player.Center.Y + 1 + spawnInfo.Player.height / 2) / 16)];
            if (spawnInfo.Player.InModBiome(GetInstance<SaltCave>()) && (spawnInfo.SpawnTileType == TileType<SaltTile>() || spawnInfo.SpawnTileType == TileType<RockSaltTile>() || playerTile.TileType == TileType<SaltTile>() || playerTile.TileType == TileType<RockSaltTile>()))
            {
                if (Main.hardMode) return 0.25f;
                return 0.375f;
            }
            return 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemType<Pretzel>(), 1000));
        }
    }
}