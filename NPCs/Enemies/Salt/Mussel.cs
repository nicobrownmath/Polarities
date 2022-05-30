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
using Terraria.Audio;
using Polarities.Items.Placeable.Blocks;
using Terraria.GameContent.ItemDropRules;
using Polarities.Items.Consumables;
using Terraria.GameContent.Bestiary;
using Polarities.Biomes;
using Polarities.Items.Weapons.Summon.Sentries;

namespace Polarities.NPCs.Enemies.Salt
{
    public class Mussel : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
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
            NPC.width = 46;
            NPC.height = 42;
            NPC.defense = 10;
            NPC.damage = 40;
            NPC.lifeMax = 90;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 1f;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 5, 0);

            Banner = NPC.type;
            BannerItem = ItemType<MusselBanner>();

            SpawnModBiomes = new int[1] { GetInstance<SaltCave>().Type };
        }

        public override void AI()
        {
            if (NPC.localAI[0] == 0)
            {
                NPC.localAI[0] = 1;
                NPC.netUpdate = true;
            }

            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.netUpdate = true;
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
            }
            NPC.direction = player.Center.X > NPC.Center.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;

            NPC.ai[0]++;
            if (NPC.ai[0] >= 240)
            {
                NPC.ai[0] = 0;
                NPC.ai[1] = (NPC.ai[1] + 1) % 2;
            }
            switch (NPC.ai[1])
            {
                case 0:
                    NPC.defense = 50;
                    break;
                case 1:
                    NPC.defense = 10;
                    if (NPC.ai[0] % 60 == 0 && NPC.ai[0] != 0 && Main.netMode != 1)
                    {
                        //shoot pearl
                        //a = 0.2 units per tick per tick
                        //v = 8 ticks per second
                        //we want the path to intersect playerX, playerY
                        //x = 8cos(theta)*t
                        //y = 8sin(theta)*t+0.1*t^2
                        float a = 0.1f;
                        float v = 8;
                        float x = player.Center.X - NPC.Center.X;
                        float y = player.Center.Y - NPC.Center.Y;

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
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, 8 * (new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta))), ProjectileType<MusselProjectile>(), 10, 5f, Main.myPlayer);
                        SoundEngine.PlaySound(SoundID.Item61, NPC.Center);
                    }
                    break;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            //closing if NPC.ai[1] = 0, opening if 1
            if (NPC.frame.Y != NPC.ai[1] * 2 * frameHeight)
            {
                NPC.frameCounter++;
                //closes faster than it opens
                if (NPC.frameCounter >= 2 + NPC.ai[1])
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y = (NPC.frame.Y + frameHeight) % (Main.npcFrameCount[NPC.type] * frameHeight);
                }
            }
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            //snap closed on player
            if (NPC.ai[1] == 1)
            {
                NPC.ai[1] = 0;
                NPC.ai[0] = 240 - 59;
            }
            else
            {
                damage /= 3;
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            Tile playerTile = Main.tile[(int)(spawnInfo.Player.Center.X / 16), (int)((spawnInfo.Player.Center.Y + 1 + spawnInfo.Player.height / 2) / 16)];
            if (spawnInfo.Player.InModBiome(GetInstance<SaltCave>()) && (spawnInfo.SpawnTileType == TileType<SaltTile>() || spawnInfo.SpawnTileType == TileType<RockSaltTile>() || playerTile.TileType == TileType<SaltTile>() || playerTile.TileType == TileType<RockSaltTile>()))
            {
                return 0.85f;
            }
            return 0f;
        }

        public override bool CheckDead()
        {
            for (int i = 1; i <= 3; i++)
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("MusselGore" + i).Type);
            return true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemType<MolluscStaff>(), 10));
            npcLoot.Add(ItemDropRule.Common(ItemType<Pretzel>(), 50));
        }
    }

    public class MusselProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.timeLeft = 1200;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 0.3f;
        }

        public override void AI()
        {
            Projectile.rotation += Projectile.velocity.X / Projectile.width * 2;
            Projectile.velocity.Y += 0.2f;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
                Projectile.velocity *= 0.9f;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
                Projectile.velocity *= 0.9f;
            }
            if (Projectile.timeLeft < 600)
            {
                Projectile.Kill();
            }
            return false;
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
            for (int i = 0; i < 3; i++)
            {
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Projectile.velocity, Mod.Find<ModGore>("PearlShard").Type);
            }
        }
    }
}