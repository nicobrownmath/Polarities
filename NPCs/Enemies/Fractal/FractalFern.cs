using Microsoft.Xna.Framework;
using Polarities.Biomes.Fractal;
using Polarities.Items.Materials;
using Polarities.Items.Placeable.Banners;
using Polarities.Items.Placeable.Blocks.Fractal;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.NPCs.Enemies.Fractal
{
    public class FractalFern : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 24;
            NPC.height = 62;
            DrawOffsetY = -2;
            NPC.defense = 4;
            NPC.damage = 30;
            NPC.lifeMax = 100;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 1f;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 5, 0);

            Banner = NPC.type;
            BannerItem = ItemType<FractalFernBanner>();

            this.SetModBiome<FractalBiome>();
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.lifeMax = 160;
        }

        public override void AI()
        {
            if (NPC.ai[0] == 0)
            {
                NPC.ai[0] = 240;
                //teleport nearby but not on top of player
                NPC.TargetClosest(false);
                Player player = Main.player[NPC.target];

                bool attemptSuccessful = false;

                Teleport(player, ref attemptSuccessful);

                if (!attemptSuccessful)
                {
                    return;
                }
            }
            else if (NPC.ai[0] == 180 || NPC.ai[0] == 150 || NPC.ai[0] == 120)
            {
                //shoot projectile at player
                Player player = Main.player[NPC.target];
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center - new Vector2(0, NPC.height / 2), new Vector2(8, 0).RotatedBy((player.Center - NPC.Center + new Vector2(0, NPC.height / 2)).ToRotation()), ProjectileType<FractalFrond>(), 15, 0, Main.myPlayer);
            }
            NPC.ai[0]--;
        }
        public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit)
        {
            if (Main.expertMode)
            {
                NPC.ai[0] = 240;
                bool attemptSuccessful = false;
                Teleport(player, ref attemptSuccessful);
                if (attemptSuccessful)
                {
                    NPC.ai[0]--;
                }
            }
        }

        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
        {
            if (Main.expertMode)
            {
                NPC.ai[0] = 240;
                bool attemptSuccessful = false;
                Teleport(Main.player[projectile.owner], ref attemptSuccessful);
                if (attemptSuccessful)
                {
                    NPC.ai[0]--;
                }
            }
        }

        private void Teleport(Player player, ref bool attemptSuccessful)
        {
            //try up to 20 times
            for (int i = 0; i < 40; i++)
            {
                Vector2 tryGoalPoint = player.Center + new Vector2(-NPC.width / 2 + Main.rand.NextFloat(100f, 500f) * (Main.rand.Next(2) * 2 - 1), Main.rand.NextFloat(-500f, 500f));
                tryGoalPoint.Y = 16 * (int)(tryGoalPoint.Y / 16);
                tryGoalPoint -= new Vector2(0, NPC.height);

                bool viable = true;

                for (int x = (int)(tryGoalPoint.X / 16); x <= (int)((tryGoalPoint.X + NPC.width) / 16); x++)
                {
                    for (int y = (int)(tryGoalPoint.Y / 16); y <= (int)((tryGoalPoint.Y + NPC.height) / 16); y++)
                    {
                        if (Main.tile[x, y].HasTile)
                        {
                            viable = false;
                            break;
                        }
                    }
                    if (!viable)
                    {
                        break;
                    }
                }

                if (viable)
                {
                    for (int y = (int)((tryGoalPoint.Y + NPC.height) / 16); y < Main.maxTilesY; y++)
                    {
                        int x = (int)((tryGoalPoint.X + NPC.width / 2) / 16);
                        if (Main.tile[x, y].HasTile && (Main.tileSolid[Main.tile[x, y].TileType] || Main.tileSolidTop[Main.tile[x, y].TileType]))
                        {
                            for (int a = 0; a < 12; a++)
                            {
                                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric, newColor: Color.LightBlue, Scale: 1.4f);
                            }

                            NPC.position = new Vector2(tryGoalPoint.X, y * 16 - NPC.height);

                            for (int a = 0; a < 12; a++)
                            {
                                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric, newColor: Color.LightBlue, Scale: 1.4f);
                            }
                            break;
                        }
                    }
                    attemptSuccessful = true;
                    break;
                }
            }
            NPC.spriteDirection = Main.rand.Next(2) * 2 - 1;
            NPC.netUpdate = true;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter == 4)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y = (NPC.frame.Y + frameHeight) % (6 * frameHeight);
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            //if (Subworld.IsActive<FractalSubworld>())
            //{
            //    return 0.2f;
            //}
            return 0f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				//flavor text
				this.TranslatedBestiaryEntry()
            });
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FractalStrands>(), minimumDropped: 1, maximumDropped: 3));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FractalResidue>(), chanceDenominator: 4));
            //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BarnsleyStaff>(), chanceDenominator: 40));
        }
    }

    public class FractalFrond : ModProjectile
    {
        public override void SetStaticDefaults()
        {
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            if (Main.rand.NextBool(3))
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, newColor: Color.LightBlue, Scale: 1f)].noGravity = true;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, newColor: Color.LightBlue, Scale: 1f)].noGravity = true;
            }
        }
    }
}