using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Biomes.Fractal;
using Polarities.Items.Materials;
using Polarities.Items.Placeable.Banners;
using Polarities.Items.Placeable.Furniture;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.NPCs.Enemies.Fractal
{
    public class SeaAnomaly : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers(0) { CustomTexturePath = "Polarities/Textures/Bestiary/SeaAnomaly", Position = new Vector2(0f, 16f), };
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 34;
            NPC.height = 34;

            NPC.defense = 10;
            NPC.damage = 35;
            NPC.lifeMax = 300;
            NPC.knockBackResist = 0f;
            NPC.value = Item.sellPrice(silver: 1);
            NPC.npcSlots = 1f;
            NPC.behindTiles = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath1;

            NPC.buffImmune[BuffID.Confused] = true;
            NPC.buffImmune[BuffID.OnFire] = true;
            NPC.buffImmune[BuffID.Frostburn] = true;
            NPC.buffImmune[BuffID.CursedInferno] = true;
            NPC.buffImmune[BuffID.ShadowFlame] = true;
            NPC.buffImmune[BuffID.Poisoned] = true;
            NPC.buffImmune[BuffID.Venom] = true;

            Banner = NPC.type;
            BannerItem = ItemType<SeaAnomalyBanner>();

            this.SetModBiome<FractalBiome, FractalOceanBiome>();

            NPC.GetGlobalNPC<PolaritiesNPC>().usesProjectileHitCooldowns = true;
            NPC.GetGlobalNPC<PolaritiesNPC>().projectileHitCooldownTime = 10;
        }

        private Vector2[] segmentPositions = new Vector2[46];
        private int[] hitBoxSegmentIds = { -1, -1 };

        public override void AI()
        {
            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.netUpdate = true;
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
            }

            if (NPC.localAI[0] == 0)
            {
                NPC.rotation = (player.Center - NPC.Center).ToRotation() + MathHelper.PiOver2;
                NPC.localAI[0] = 1;

                segmentPositions[0] = NPC.Center - new Vector2(0, NPC.height / 2 - 2).RotatedBy(NPC.rotation);
                for (int i = 1; i < segmentPositions.Length; i++)
                {
                    segmentPositions[i] = segmentPositions[i - 1] + new Vector2(0, NPC.height).RotatedBy(NPC.rotation);
                }
            }

            //changeable ai values
            float rotationFade = 6f;
            float rotationAmount = 1f;

            if (NPC.ai[0] == 0)
            {
                //circle player
                Vector2 goalPosition = player.Center + (NPC.Center + NPC.velocity * 8 - player.Center).SafeNormalize(Vector2.Zero) * 160;
                Vector2 goalVelocity = (goalPosition - NPC.Center).SafeNormalize(Vector2.Zero) * 12;
                NPC.velocity += (goalVelocity - NPC.velocity) / 30f;
                NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;

                NPC.ai[1]++;
                if (NPC.ai[1] == 120)
                {
                    NPC.ai[1] = 0;
                    NPC.ai[0] = Main.rand.Next(1, 3);
                }
            }
            else if (NPC.ai[0] == 1)
            {
                //stop and shoot lasers
                NPC.velocity *= 0.97f;

                if (NPC.ai[1] == 20 || NPC.ai[1] == 30 || NPC.ai[1] == 40)
                {
                    SoundEngine.PlaySound(SoundID.Item91, NPC.Center);

                    for (int i = 0; i < 3; i++)
                    {
                        int index = 8 + 46 * i / 3;

                        if (Main.netMode != 1)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), segmentPositions[index], (player.Center - segmentPositions[index]).SafeNormalize(Vector2.Zero) * 8, ProjectileType<ShockflakeBolt>(), 17, 1f, Main.myPlayer);
                        }
                    }
                }

                NPC.ai[1]++;
                if (NPC.ai[1] == 60)
                {
                    NPC.ai[1] = 0;
                    NPC.ai[0] = 0;
                }
            }
            else
            {
                //dash
                if (NPC.ai[1] < 10)
                {
                    NPC.velocity += ((player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 12 - NPC.velocity) / 5;
                    NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                }
                else if (NPC.ai[1] >= 50)
                {
                    NPC.velocity *= 0.95f;
                }

                NPC.ai[1]++;
                if (NPC.ai[1] == 60)
                {
                    NPC.ai[1] = 0;
                    NPC.ai[0] = 0;
                }
            }

            //update segment positions
            segmentPositions[0] = NPC.Center + NPC.velocity - new Vector2(0, NPC.height / 2 - 2).RotatedBy(NPC.rotation);
            Vector2 rotationGoal = Vector2.Zero;

            for (int i = 1; i < segmentPositions.Length; i++)
            {
                if (i > 1)
                {
                    rotationGoal = ((rotationFade - 1) * rotationGoal + (segmentPositions[i - 1] - segmentPositions[i - 2])) / rotationFade;
                }

                segmentPositions[i] = segmentPositions[i - 1] + (rotationAmount * rotationGoal + (segmentPositions[i] - segmentPositions[i - 1]).SafeNormalize(Vector2.Zero)).SafeNormalize(Vector2.Zero) * 2;
            }

            //position hitbox segments
            NPC.realLife = NPC.whoAmI;
            for (int h = 0; h < hitBoxSegmentIds.Length; h++)
            {
                int index = 8 + 46 * (h + 1) / 3;

                Vector2 spot = segmentPositions[index];

                if (hitBoxSegmentIds[h] == -1 || !Main.npc[hitBoxSegmentIds[h]].active || Main.npc[hitBoxSegmentIds[h]].type != NPCType<SeaAnomalyHitbox>())
                {
                    if (Main.netMode != 1)
                    {
                        hitBoxSegmentIds[h] = NPC.NewNPC(NPC.GetSource_FromThis(), (int)spot.X, (int)spot.Y, NPCType<SeaAnomalyHitbox>(), ai0: NPC.whoAmI, ai1: index);
                        NPC.netUpdate = true;
                    }
                }
                else
                {
                    Main.npc[hitBoxSegmentIds[h]].Center = spot;
                    Main.npc[hitBoxSegmentIds[h]].timeLeft = 10;
                    Main.npc[hitBoxSegmentIds[h]].dontTakeDamage = NPC.dontTakeDamage;
                    Main.npc[hitBoxSegmentIds[h]].defense = NPC.defense;
                    Main.npc[hitBoxSegmentIds[h]].defDefense = NPC.defDefense;
                    Main.npc[hitBoxSegmentIds[h]].HitSound = NPC.HitSound;
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (FractalSubworld.Active)
            {
                return 0.3f * FractalSubworld.SpawnConditionFractalWaters(spawnInfo) * (1 - FractalSubworld.SpawnConditionFractalSky(spawnInfo));
            }
            return 0f;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (NPC.life <= 0)
            {
                GoreHelper.DeathGore(NPC, "SeaAnomalyGore1");
                GoreHelper.DeathGore(NPC, segmentPositions[23], "SeaAnomalyGore2");
                GoreHelper.DeathGore(NPC, segmentPositions[39], "SeaAnomalyGore3");
            }
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
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Stromatolight>(), chanceDenominator: 2));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FractalResidue>(), chanceDenominator: 2));
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
                return false;
            //draw the thing
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            for (int i = segmentPositions.Length - 1; i > 0; i--)
            {
                Vector2 drawPosition = (segmentPositions[i] + segmentPositions[i - 1]) / 2;
                float rotation = (segmentPositions[i - 1] - segmentPositions[i]).ToRotation() + MathHelper.PiOver2;

                spriteBatch.Draw(texture, drawPosition - screenPos, new Rectangle(0, (i - 1) * 2, 38, 4), Lighting.GetColor((int)(drawPosition.X / 16), (int)(drawPosition.Y / 16)), rotation, new Vector2(19, 2), new Vector2(1, 1), SpriteEffects.None, 0f);
                spriteBatch.Draw(ModContent.Request<Texture2D>($"{Texture}_Mask").Value, drawPosition - screenPos, new Rectangle(0, (i - 1) * 2, 38, 4), Color.White, rotation, new Vector2(19, 2), new Vector2(1, 1), SpriteEffects.None, 0f);
            }

            return false;
        }
    }

    public class SeaAnomalyHitbox : ModNPC
    {
        public override string Texture => "Polarities/Projectiles/CallShootProjectile";

        public override void SetStaticDefaults()
        {
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers(0) { Hide = true, };
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 34;
            NPC.height = 34;

            NPC.defense = 10;
            NPC.damage = 35;
            NPC.lifeMax = 300;
            NPC.knockBackResist = 0f;
            NPC.value = Item.sellPrice(silver: 1);
            NPC.npcSlots = 1f;
            NPC.behindTiles = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.dontCountMe = true;

            NPC.buffImmune[BuffID.Confused] = true;
            NPC.buffImmune[BuffID.OnFire] = true;
            NPC.buffImmune[BuffID.Frostburn] = true;
            NPC.buffImmune[BuffID.CursedInferno] = true;
            NPC.buffImmune[BuffID.ShadowFlame] = true;
            NPC.buffImmune[BuffID.Poisoned] = true;
            NPC.buffImmune[BuffID.Venom] = true;

            //Banner = NPCType<SeaAnomaly>();
            //BannerItem = ItemType<SeaAnomalyBanner>();

            NPC.GetGlobalNPC<PolaritiesNPC>().usesProjectileHitCooldowns = true;
            NPC.GetGlobalNPC<PolaritiesNPC>().projectileHitCooldownTime = 10;
        }

        public override bool CheckActive()
        {
            return Main.npc[NPC.realLife].active;
        }

        public override void AI()
        {
            NPC.realLife = (int)NPC.ai[0];
            NPC.active = Main.npc[NPC.realLife].active;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return false;
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return false;
        }
    }
}
