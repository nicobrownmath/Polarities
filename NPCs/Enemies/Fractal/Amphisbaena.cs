using Microsoft.Xna.Framework;
using Polarities.Biomes.Fractal;
using Polarities.Items.Armor.Vanity;
using Polarities.Items.Materials;
using Polarities.Items.Placeable.Banners;
using Polarities.Items.Placeable.Blocks.Fractal;
using System.IO;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.NPCs.Enemies.Fractal
{
    public class Amphisbaena : ModNPC
    {
        private int attackCooldown
        {
            get => attackCooldown = (int)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 9;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 26;
            NPC.height = 18;
            NPC.defense = 14;
            NPC.damage = 70;
            NPC.lifeMax = 100;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 1f;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(silver: 20);

            Banner = NPC.type;
            BannerItem = ItemType<AmphisbaenaBanner>();

            this.SetModBiome<FractalBiome, FractalWastesBiome>();
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.damage = 100;
            NPC.lifeMax = 200;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
            {
                for (int i = 1; i <= 4; i++)
                    GoreHelper.DeathGore(NPC, $"AmphisbaenaGore{i}", Main.rand.NextVector2Unit() * NPC.Size / 2f);
            }
        }

        public override void AI()
        {
            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
            }

            if (NPC.life < NPC.lifeMax / 2)
            {
                NPC.catchItem = (short)ModContent.ItemType<AmphisnekHat>();
                NPC.netUpdate = true;
            }

            if (attackCooldown > 0)
            {
                attackCooldown--;
                NPC.damage = Main.expertMode ? 100 : 70;
                if (NPC.frame.Y < NPC.height)
                {
                    NPC.frame.Y = 0;
                }
            }
            else
            {
                NPC.damage = 0;

                if ((NPC.Center - player.Center).Length() > 50)
                {
                    NPC.frame.Y = 0;
                    NPC.noGravity = false;
                    NPC.noTileCollide = false;
                }
                else
                {
                    NPC.noGravity = true;
                    NPC.noTileCollide = true;
                    NPC.frame.Y = 2 * NPC.height;
                    NPC.width = 60 * 2 - 26;
                    NPC.position.X -= 60 - 26;
                    attackCooldown = 60;
                }
            }

            if (attackCooldown < 60 - 7)
            {
                NPC.width = 26;
            }

            if (attackCooldown == 60 - 8)
            {
                NPC.position.X += 60 - 26;

                //teleport spam
                SpawnTeleportDusts();
                for (int i = 0; i < 5; i++)
                    Teleport(player);
                Teleport(player, Main.rand.NextBool());
                attackCooldown = 60 - 9;
            }
        }

        public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit)
        {
            //teleport
            SpawnTeleportDusts();
            for (int i = 0; i < 5; i++)
                Teleport(Main.player[NPC.target]);
            Teleport(Main.player[NPC.target], Main.rand.NextBool());
            attackCooldown = 60 - 9;
        }

        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
        {
            //teleport
            SpawnTeleportDusts();
            for (int i = 0; i < 5; i++)
                Teleport(Main.player[NPC.target]);
            Teleport(Main.player[NPC.target], Main.rand.NextBool());
            attackCooldown = 60 - 9;
        }

        private void Teleport(Player player, bool teleportNextToPlayer = false)
        {
            //try up to 20 times
            for (int i = 0; i < 40; i++)
            {
                Vector2 tryGoalPoint;
                if (!teleportNextToPlayer)
                {
                    tryGoalPoint = player.Center + new Vector2(-NPC.width / 2 + Main.rand.NextFloat(100f, 500f) * (Main.rand.Next(2) * 2 - 1), Main.rand.NextFloat(-500f, 500f));
                }
                else
                {
                    tryGoalPoint = player.Center + new Vector2(-NPC.width / 2 + Main.rand.NextFloat(10f, 50f) * (Main.rand.Next(2) * 2 - 1), Main.rand.NextFloat(-500f, 500f));
                }

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
                        if (Main.tile[x, y].HasTile && (Main.tileSolid[Main.tile[x, y].TileType] || Main.tileSolid[Main.tile[x, y].TileType]))
                        {
                            NPC.position = new Vector2(tryGoalPoint.X, y * 16 - NPC.height);

                            SpawnTeleportDusts();
                            break;
                        }
                    }
                    break;
                }
            }
            NPC.spriteDirection = Main.rand.Next(2) * 2 - 1;
            NPC.netUpdate = true;
        }

        public void SpawnTeleportDusts()
        {
            for (int a = 0; a < 12; a++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Electric, newColor: Color.LightBlue, Scale: 1f);
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter == 1)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y == 1 * frameHeight || NPC.frame.Y == 9 * frameHeight)
                {
                    NPC.frame.Y = 0;
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            //if (Subworld.IsActive<FractalSubworld>())
            //{
            //    return FractalSubworld.SpawnConditionFractalWastes(spawnInfo) * (1f - FractalSubworld.SpawnConditionFractalSky(spawnInfo));
            //}
            return 0f;
        }

        public override bool CheckDead()
        {
            return true;
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
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FractalDust>(), minimumDropped: 1, maximumDropped: 3));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FractalResidue>(), chanceDenominator: 2));
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Electrified, 240);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.catchItem);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.catchItem = reader.ReadInt16();
        }
    }
}
