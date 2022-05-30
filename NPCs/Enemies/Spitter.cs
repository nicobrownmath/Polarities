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
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Bestiary;

namespace Polarities.NPCs.Enemies
{
    public class Spitter : ModNPC
    {
        private int attackCooldown
        {
            get => attackCooldown = (int)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 12;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                //spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
				//flavor text
				this.TranslatedBestiaryEntry()
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 54;
            NPC.height = 58;
            NPC.defense = 10;
            NPC.lifeMax = 400;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 1f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 10, 0);

            Banner = NPC.type;
            BannerItem = ItemType<SpitterBanner>();
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

            if (attackCooldown > 0)
            {
                attackCooldown--;
            }
            else
            {
                attackCooldown = 60;
            }
            if (NPC.Distance(player.Center) < 720f && attackCooldown <= 20 && attackCooldown % 5 == 0 && Main.netMode != 1)
            {
                float a = 0.1f;
                float v = 16;
                float x = player.Center.X - NPC.Center.X;
                float y = player.Center.Y - NPC.position.Y;

                double theta = (new Vector2(x, y)).ToRotation();
                theta += Math.PI / 2;
                if (theta > Math.PI) { theta -= Math.PI * 2; }
                theta *= 0.5;
                theta -= Math.PI / 2;

                double num0 = -4 * Math.Pow(a, 2) * Math.Pow(x, 4) + 4 * a * Math.Pow(v, 2) * Math.Pow(x, 2) * y + Math.Pow(v, 4) * Math.Pow(x, 2);
                if (num0 > 0)
                {
                    num0 = -NPC.direction * Math.Sqrt(num0);
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
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, -NPC.height / 2), 16 * (new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta))), ProjectileType<SpitterVenom>(), 25, 1f, Main.myPlayer);
                if (attackCooldown == 20)
                {
                    SoundEngine.PlaySound(SoundID.Item13, NPC.Center);
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter == 4)
            {
                NPC.frameCounter = 0;
                if (NPC.frame.Y <= 0)
                {
                    NPC.frame.Y += 6 * frameHeight;
                }
                if (NPC.frame.Y >= 11 * frameHeight)
                {
                    NPC.frame.Y -= 6 * frameHeight;
                }
                if (NPC.frame.Y >= 5 * frameHeight && NPC.direction == -1)
                {
                    NPC.frame.Y -= 6 * frameHeight;
                }
                if (NPC.frame.Y <= 6 * frameHeight && NPC.direction == 1)
                {
                    NPC.frame.Y += 6 * frameHeight;
                }
                if (NPC.direction == 1)
                {
                    NPC.frame.Y -= frameHeight;
                }
                else
                {
                    NPC.frame.Y += frameHeight;
                }
            }
        }

        public override bool CheckDead()
        {
            for (int i = 1; i <= 4; i++)
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("SpitterGore" + i).Type);
            return true;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.SpawnTileY <= Main.worldSurface && spawnInfo.SpawnTileType == TileID.Sand && !spawnInfo.Water && Main.hardMode ? 0.1f : 0f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemType<Items.Materials.VenomGland>(), 3, 1, 2));
        }
    }

    public class SpitterVenom : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Pixel";

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.alpha = 0;
            Projectile.timeLeft = 3600;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ToxicBubble, Scale: 1.5f)].noGravity = true;
            Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ToxicBubble, Scale: 1.5f)].noGravity = true;
            Projectile.velocity.Y += 0.2f;
        }
        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Venom, 300);
        }
    }
}