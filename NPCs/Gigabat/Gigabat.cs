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
using Polarities.Items.Accessories;
using Polarities.Items.Placeable.Trophies;
using Terraria.Audio;
using Terraria.GameContent;
using ReLogic.Content;
using Terraria.GameContent.ItemDropRules;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;

namespace Polarities.NPCs.Gigabat
{
    [AutoloadBossHead]
    //TODO: Better FTW changes?
    //TODO: Make drops crafted from a material? And then have that material drop from bat slimes too
    //TODO: Telegraphs need improvement
    public class Gigabat : ModNPC
    {
        private bool dashing
        {
            get => (NPC.ai[0] == 9 && NPC.ai[1] >= 170 && NPC.ai[1] < 240) || (NPC.ai[0] == 6 && dashCounter > 0 && NPC.ai[1] <= 210);
        }
        private bool rapidWingbeats
        {
            get => NPC.ai[0] == 2 && NPC.ai[1] < 120 && NPC.ai[1] > 30;
        }
        private int maxBats
        {
            get => NPC.life < NPC.lifeMax * 0.65f ? 100 : 64;
        }
        private Vector2 circleCenter
        {
            get => new Vector2(NPC.ai[2], NPC.ai[3]);
            set
            {
                NPC.ai[2] = value.X;
                NPC.ai[3] = value.Y;
            }
        }
        private static float CircleRadius => Main.getGoodWorld ? 450 : 500;

        private static readonly int MAXBATREPLENISHMENTCOOLDOWN = 12000;
        private int batReplenishmentCooldown = MAXBATREPLENISHMENTCOOLDOWN;

        private static readonly int MAXDASHATTACKCOOLDOWN = 1200;
        private int dashAttackCooldown = 0;
        private int dashCounter = 0;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 13;

            //group with other bosses
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                //draw offset on x a bit in the small portrait
                Position = new Vector2(10f, 0f),
                Direction = 1
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

            NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
            {
                SpecificallyImmuneTo = new int[] {
                    BuffID.Confused
                }
            };
            NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);

            Polarities.customNPCGlowMasks[Type] = Request<Texture2D>(Texture + "_Mask");
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

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 54;
            NPC.height = 54;
            DrawOffsetY = 40;

            NPC.defense = 10;
            NPC.damage = Main.expertMode ? 50 / 2 : 30;
            NPC.lifeMax = Main.masterMode ? 5000 / 3 : Main.expertMode ? 4000 / 2 : 3000;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 4, 0, 0);
            NPC.npcSlots = 15f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            if (Main.tenthAnniversaryWorld) NPC.scale *= 0.5f;
            if (Main.getGoodWorld) NPC.scale *= 0.8f;

            Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Gigabat");
        }

        public override void AI()
        {
            if (NPC.localAI[0] == 0)
            {
                NPC.ai[1] = -120;

                for (int i = 0; i < maxBats; i++)
                {
                    NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<GigabatMinion>(), ai0: NPC.whoAmI);
                }

                NPC.localAI[0] = 1;
            }

            Player player = Main.player[NPC.target];
            if (!player.active || player.dead || (!player.ZoneRockLayerHeight && !player.ZoneDirtLayerHeight))
            {
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
                if (player.dead || (!player.ZoneRockLayerHeight && !player.ZoneDirtLayerHeight))
                {
                    if (NPC.timeLeft > 10)
                    {
                        NPC.timeLeft = 10;
                    }
                    if (player.ZoneUnderworldHeight)
                    {
                        NPC.velocity.Y -= 0.1f;
                    }
                    else
                    {
                        NPC.velocity.Y += 0.1f;
                    }
                    return;
                }
            }

            //update circle center
            if ((player.Center - circleCenter).Length() > CircleRadius * 1.05f)
            {
                circleCenter = player.Center;
            }

            int numBats = 0;
            for (int i = 0; i < Main.npc.Length; i++)
            {
                NPC candidateBat = Main.npc[i];
                if (candidateBat.type == NPCType<GigabatMinion>() && candidateBat.ai[0] == NPC.whoAmI && candidateBat.active)
                {
                    numBats++;
                }
            }
            if (numBats < maxBats)
            {
                batReplenishmentCooldown += (numBats - maxBats);
                if (batReplenishmentCooldown < 0) batReplenishmentCooldown = 0;
            }
            if (dashAttackCooldown > 0)
            {
                dashAttackCooldown--;
            }

            switch (NPC.ai[0])
            {
                case 0:
                    //cooldown between attacks

                    NPC.ai[1]++;
                    if (NPC.ai[1] == (Main.expertMode ? 45 : 60))
                    {
                        //replenish bats if available
                        if (batReplenishmentCooldown == 0)
                        {
                            batReplenishmentCooldown = MAXBATREPLENISHMENTCOOLDOWN;
                            NPC.ai[0] = 1;
                            NPC.ai[1] = 0;
                        }
                        else if (dashAttackCooldown == 0)
                        {
                            dashAttackCooldown = MAXDASHATTACKCOOLDOWN;
                            dashCounter = 0;

                            NPC.ai[0] = 6;
                            NPC.ai[1] = 0;
                        }
                        else
                        {
                            if (Main.netMode != 1)
                            {
                                NPC.ai[0] = Main.rand.Next(2, 5);
                                NPC.ai[1] = 0;

                                if (NPC.ai[0] == 3)
                                {
                                    if (NPC.life < NPC.lifeMax * 0.65f)
                                    {
                                        NPC.ai[0] = 8;
                                    }
                                    else if (Main.rand.NextBool(2))
                                    {
                                        NPC.ai[0] = 7;
                                    }
                                }
                                if (NPC.ai[0] == 4)
                                {
                                    if (NPC.life < NPC.lifeMax * 0.65f)
                                    {
                                        NPC.ai[0] = 9;
                                    }
                                }
                            }
                            NPC.netUpdate = true;
                        }
                    }

                    CircleMotion(circleCenter);
                    break;
                case 1:
                    //special bat replenishing

                    if (NPC.ai[1] == 0)
                    {
                        while (numBats < maxBats)
                        {
                            Vector2 spawnPoint = circleCenter + new Vector2(CircleRadius * 2, 0).RotatedByRandom(MathHelper.TwoPi);
                            NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPoint.X, (int)spawnPoint.Y, NPCType<GigabatMinion>(), ai0: NPC.whoAmI);
                            numBats++;
                        }
                    }

                    //circling motion code
                    CircleMotion(circleCenter, 1f);

                    NPC.ai[1]++;
                    if (NPC.ai[1] == 60)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = 0;
                    }
                    break;
                case 2:
                    //dissipating stream attack

                    //telegraph
                    if (NPC.ai[1] == 10 || NPC.ai[1] == 20 || NPC.ai[1] == 30)
                    {
                        Vector2 dustPos = (NPC.Center + circleCenter) / 2f;

                        int numDusts = 20;
                        for (int j = 0; j < numDusts; j++)
                        {
                            int dust = Dust.NewDust(dustPos, 0, 0, 92, Scale: 1.5f); //175, 180-183, 197, 204, 219-223, 226, 228, 229, 235, 258, 259, 261, 263, 264, 269, 270, 
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].noLight = true;
                            Main.dust[dust].velocity = new Vector2(4, 0).RotatedBy(j * MathHelper.TwoPi / numDusts);
                        }
                    }

                    //circling motion code
                    CircleMotion(circleCenter, 0f);

                    NPC.ai[1]++;
                    if (NPC.ai[1] == 180)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = 0;
                    }
                    break;
                case 3:
                    //regularly spaced orthogonal darts towards player

                    //telegraph
                    if (NPC.ai[1] == 0)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 dustPos = circleCenter + new Vector2(CircleRadius, 0).RotatedBy(i * MathHelper.TwoPi / 4f);

                            int numDusts = 20;
                            for (int j = 0; j < numDusts; j++)
                            {
                                int dust = Dust.NewDust(dustPos, 0, 0, 92, Scale: 1.5f); //175, 180-183, 197, 204, 219-223, 226, 228, 229, 235, 258, 259, 261, 263, 264, 269, 270, 
                                Main.dust[dust].noGravity = true;
                                Main.dust[dust].noLight = true;
                                Main.dust[dust].velocity = new Vector2(4, 0).RotatedBy(j * MathHelper.TwoPi / numDusts);
                            }
                        }
                    }

                    //circling motion code
                    CircleMotion(circleCenter);

                    NPC.ai[1]++;
                    if (NPC.ai[1] == 180)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = 0;
                    }
                    break;
                case 7:
                    //regularly spaced diagonal darts towards player

                    //telegraph
                    if (NPC.ai[1] == 0)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 dustPos = circleCenter + new Vector2(CircleRadius, 0).RotatedBy(i * MathHelper.TwoPi / 4f + MathHelper.TwoPi / 8f);

                            int numDusts = 20;
                            for (int j = 0; j < numDusts; j++)
                            {
                                int dust = Dust.NewDust(dustPos, 0, 0, 92, Scale: 1.5f); //175, 180-183, 197, 204, 219-223, 226, 228, 229, 235, 258, 259, 261, 263, 264, 269, 270, 
                                Main.dust[dust].noGravity = true;
                                Main.dust[dust].noLight = true;
                                Main.dust[dust].velocity = new Vector2(4, 0).RotatedBy(j * MathHelper.TwoPi / numDusts);
                            }
                        }
                    }

                    //circling motion code
                    CircleMotion(circleCenter);

                    NPC.ai[1]++;
                    if (NPC.ai[1] == 180)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = 0;
                    }
                    break;
                case 8:
                    //regularly spaced octogonal darts towards player

                    //telegraph
                    if (NPC.ai[1] == 0)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            Vector2 dustPos = circleCenter + new Vector2(CircleRadius, 0).RotatedBy(i * MathHelper.TwoPi / 8f);

                            int numDusts = 20;
                            for (int j = 0; j < numDusts; j++)
                            {
                                int dust = Dust.NewDust(dustPos, 0, 0, 92, Scale: 1.5f); //175, 180-183, 197, 204, 219-223, 226, 228, 229, 235, 258, 259, 261, 263, 264, 269, 270, 
                                Main.dust[dust].noGravity = true;
                                Main.dust[dust].noLight = true;
                                Main.dust[dust].velocity = new Vector2(4, 0).RotatedBy(j * MathHelper.TwoPi / numDusts);
                            }
                        }
                    }

                    //circling motion code
                    CircleMotion(circleCenter);

                    NPC.ai[1]++;
                    if (NPC.ai[1] == 195)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = 0;
                    }
                    break;
                case 4:
                    //random chord darts

                    //circling motion code
                    CircleMotion(circleCenter);

                    NPC.ai[1]++;
                    if (NPC.ai[1] == 180)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = 0;
                    }
                    break;
                case 9:
                    if (NPC.ai[1] < 160)
                    {
                        //circling motion code
                        CircleMotion(circleCenter, 1f);

                        //telegraph
                        if (NPC.ai[1] == 40 || NPC.ai[1] == 70 || NPC.ai[1] == 100 || NPC.ai[1] == 130)
                        {
                            Vector2 dustPos = circleCenter;

                            int numDusts = 20;
                            for (int j = 0; j < numDusts; j++)
                            {
                                int dust = Dust.NewDust(dustPos, 0, 0, 92, Scale: 1.5f); //175, 180-183, 197, 204, 219-223, 226, 228, 229, 235, 258, 259, 261, 263, 264, 269, 270, 
                                Main.dust[dust].noGravity = true;
                                Main.dust[dust].noLight = true;
                                Main.dust[dust].velocity = new Vector2(4, 0).RotatedBy(j * MathHelper.TwoPi / numDusts);
                            }
                        }
                    }
                    //dash through the center after the chord darts if below half health
                    else if (NPC.ai[1] == 170)
                    {
                        SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/NPC_Killed_4")
                        {
                            Volume = 2f,
                            Pitch = -2f
                        }, NPC.Center);

                        NPC.velocity = (circleCenter - NPC.Center).SafeNormalize(Vector2.Zero) * 16f;

                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.velocity.SafeNormalize(Vector2.Zero) * 4, ProjectileType<GigabatEcholocation>(), 1, 0, Main.myPlayer);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.Pi / 6f) * 4, ProjectileType<GigabatEcholocation>(), 1, 0, Main.myPlayer);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.velocity.SafeNormalize(Vector2.Zero).RotatedBy(-MathHelper.Pi / 6f) * 4, ProjectileType<GigabatEcholocation>(), 1, 0, Main.myPlayer);

                        NPC.spriteDirection = (NPC.velocity.X < 0) ? 1 : -1;
                        NPC.rotation = NPC.velocity.ToRotation() + (float)((NPC.velocity.X < 0) ? Math.PI : 0);
                    }
                    if (NPC.ai[1] >= 170)
                    {
                        if (NPC.ai[1] < 240)
                            NPC.velocity *= 0.995f;
                        else
                        {
                            NPC.velocity *= (260 - NPC.ai[1]) / 20f;
                            NPC.rotation = 0;
                        }
                    }

                    NPC.ai[1]++;
                    if (NPC.ai[1] == 260)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = 0;
                    }
                    break;
                case 6:
                    if (NPC.ai[1] < 30 && dashCounter == 0)
                    {
                        CircleMotion(circleCenter, 0f);
                    }
                    else if (NPC.ai[1] == 30)
                    {
                        dashCounter++;

                        //Main.PlaySound(SoundID.ForceRoar, NPC.Center, 0);
                        SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/NPC_Killed_4")
                        {
                            Volume = 2f,
                            Pitch = -1f
                        }, NPC.Center);

                        NPC.velocity = NPC.DirectionTo(player.Center) * 12f;

                        NPC.spriteDirection = (NPC.velocity.X < 0) ? 1 : -1;
                        NPC.rotation = NPC.velocity.ToRotation() + (float)((NPC.velocity.X < 0) ? Math.PI : 0);

                        NPC.ai[1] = (int)(210 - (32f + (NPC.Center - player.Center).Length()) / 12f);

                        if (Main.netMode != 1)
                        {
                            if (dashCounter == 1 || Main.expertMode)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.velocity.SafeNormalize(Vector2.Zero) * 4f, ProjectileType<GigabatEcholocation>(), 1, 0, Main.myPlayer);
                            }
                            if (dashCounter == 1 && NPC.life < NPC.lifeMax * 0.65f)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.Pi / 6f) * 4, ProjectileType<GigabatEcholocation>(), 1, 0, Main.myPlayer);
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.velocity.SafeNormalize(Vector2.Zero).RotatedBy(-MathHelper.Pi / 6f) * 4, ProjectileType<GigabatEcholocation>(), 1, 0, Main.myPlayer);
                            }
                        }
                    }
                    else if (NPC.ai[1] > 210 && NPC.ai[1] <= 230)
                    {
                        NPC.velocity *= (230 - NPC.ai[1]) / 20f;

                        NPC.rotation = 0;
                    }

                    int maxDashes = NPC.life < NPC.lifeMax * 0.65f ? 4 : 3;

                    NPC.ai[1]++;
                    if (NPC.ai[1] == 235 && dashCounter < maxDashes)
                    {
                        NPC.ai[1] = 30;
                    }
                    else if (NPC.ai[1] == 370)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = 0;
                    }
                    break;
                case -1:
                    //death animation
                    if (NPC.ai[1] < 120)
                    {
                        if (NPC.ai[1] == 0)
                        {
                            NPC.spriteDirection = (NPC.velocity.X < 0) ? 1 : -1;
                        }

                        NPC.frameCounter = 0;
                        NPC.frame.Y = NPC.frame.Height * 12;
                        NPC.rotation = NPC.ai[1] * (float)Math.Sin(NPC.ai[1] / 2f) * 0.005f;
                        NPC.velocity *= 0.96f;
                    }
                    else
                    {
                        if (NPC.ai[1] == 120)
                        {
                            NPC.velocity += new Vector2(0, -16);

                            NPC.NPCLoot();
                            if (!PolaritiesSystem.downedGigabat)
                            {
                                NPC.SetEventFlagCleared(ref PolaritiesSystem.downedGigabat, -1);
                            }

                            Music = 0;

                            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/NPC_Killed_4")
                            {
                                Volume = 2f,
                                Pitch = -0.5f
                            }, NPC.Center);

                            SoundEngine.PlaySound(SoundID.NPCDeath14, NPC.Center);
                        }

                        NPC.frameCounter = 0;
                        NPC.frame.Y = NPC.frame.Height * 12;
                        NPC.velocity.Y += 0.2f;
                        NPC.rotation -= NPC.spriteDirection * 0.2f;
                    }

                    NPC.ai[1]++;
                    if (NPC.ai[1] >= 480)
                    {
                        NPC.active = false;
                    }
                    break;
            }

            if (rapidWingbeats && NPC.soundDelay <= 0)
            {
                NPC.soundDelay = 21;

                SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Item_32")
                {
                    Volume = 2f,
                }, NPC.Center);
            }
        }

        private void CircleMotion(Vector2 circleCenter, float speed = 8f, float maxAcceleration = 1f)
        {
            NPC.rotation = 0;
            NPC.spriteDirection = (NPC.velocity.X < 0) ? 1 : -1;

            float goalRadiusWeight = 0.2f;
            float radius = (goalRadiusWeight * CircleRadius + (NPC.Center - circleCenter).Length()) / (1 + goalRadiusWeight);
            Vector2 goalPosition = circleCenter + (NPC.Center - circleCenter).SafeNormalize(Vector2.Zero).RotatedBy(speed / radius) * radius;
            Vector2 goalVelocity = (goalPosition - NPC.Center).SafeNormalize(Vector2.Zero) * speed;
            Vector2 goalAcceleration = goalVelocity - NPC.velocity;
            if (goalAcceleration.Length() > maxAcceleration)
            {
                goalAcceleration.Normalize();
                goalAcceleration *= maxAcceleration;
            }
            NPC.velocity += goalAcceleration;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter == 5 || ((dashing || rapidWingbeats) && NPC.frameCounter >= 3))
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y == 7 * frameHeight && !dashing)
                {
                    NPC.frame.Y = 0;
                }
                else if (NPC.frame.Y == 10 * frameHeight && dashing)
                {
                    NPC.frame.Y = 8 * frameHeight;
                }
                else if (NPC.frame.Y == 13 * frameHeight)
                {
                    NPC.frame.Y = 5 * frameHeight;
                }
            }
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return dashing;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (dashing)
            {
                return null;
            }
            return false;
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.LesserHealingPotion;
        }

        public override bool CheckDead()
        {
            if (NPC.ai[0] != -1)
            {
                NPC.ai[0] = -1;
                NPC.ai[1] = 0;
                NPC.dontTakeDamage = true;
                NPC.life = 1;
            }
            return false;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(new FlawlessOrRandomDropRule(ItemType<GigabatTrophy>(), 10));
            npcLoot.Add(ItemDropRule.BossBag(ItemType<Items.Consumables.TreasureBags.GigabatBag>()));
            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ItemType<Items.Placeable.Relics.GigabatRelic>()));
            npcLoot.Add(ModUtils.MasterModeDropOnAllPlayersOrFlawless(ItemType<Items.Pets.GigabatPetItem>(), 4));

            //normal mode loot
            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemType<Items.Armor.Vanity.GigabatMask>(), 7));
            notExpertRule.OnSuccess(new OneFromOptionsWithCountsNotScaledWithLuckDropRule(1, 1,
                (ItemType<Items.Accessories.BatSigil>(), 1, 1),
                (ItemType<Items.Weapons.Magic.EchoStaff>(), 1, 1),
                (ItemType<Items.Weapons.Melee.ChainSaw>(), 1, 1),
                (ItemType<Items.Weapons.Ranged.Ammo.BatArrow>(), 500, 999)));
            npcLoot.Add(notExpertRule);

            npcLoot.Add(ItemDropRule.ByCondition(new FlawlessDropCondition(), ItemType<Items.Weapons.Summon.Minions.Batastrophe>()));
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);

            if (dashing)
            {
                Texture2D texture = TextureAssets.Npc[Type].Value;
                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height / Main.npcFrameCount[NPC.type] * 0.5f);
                Vector2 drawPos = NPC.Center - screenPos + new Vector2(0, -2f) + new Vector2(0, 8f);

                for (int i = 5; i > 0; i--)
                {
                    spriteBatch.Draw(texture, drawPos - NPC.velocity * (i / 6f) * 3f, NPC.frame, drawColor * (1 - (i / 6f)), NPC.rotation, drawOrigin, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
                }
            }

            return true;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            drawColor = NPC.GetNPCColorTintedByBuffs(Color.White);

            Texture2D mask = Polarities.customNPCGlowMasks[Type].Value;
            Vector2 drawOrigin = new Vector2(mask.Width * 0.5f, mask.Height / Main.npcFrameCount[NPC.type] * 0.5f);
            Vector2 drawPos = NPC.Center - screenPos + new Vector2(0, -2f) + new Vector2(0, 8f);

            if (dashing)
            {
                for (int i = 5; i > 0; i--)
                {
                    spriteBatch.Draw(mask, drawPos - NPC.velocity * (i / 6f) * 3f, NPC.frame, drawColor * (1 - (i / 6f)), NPC.rotation, drawOrigin, NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
                }
            }
        }
    }

    public class GigabatMinion : ModNPC
    {
        private NPC owner
        {
            get => Main.npc[(int)NPC.ai[0]];
        }
        private float speed
        {
            get => NPC.ai[1];
            set => NPC.ai[1] = value;
        }
        private float radiusModifier
        {
            get => NPC.ai[2];
            set => NPC.ai[2] = value;
        }
        private Vector2 circleCenter
        {
            get => new Vector2(owner.ai[2], owner.ai[3]);
        }

        private static float CircleRadius => Main.getGoodWorld ? 450 : 500;
        private bool runAway = false;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;

            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                //draw offset on y a bit in the small portrait
                Position = new Vector2(0f, 10f)
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

            NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
            {
                SpecificallyImmuneTo = new int[] {
                    BuffID.Confused
                }
            };
            NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);

            Polarities.customNPCGlowMasks[Type] = Request<Texture2D>(Texture + "_Mask");
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            int associatedNPCType = NPCType<Gigabat>();
            bestiaryEntry.UIInfoProvider = new CommonEnemyUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[associatedNPCType], quickUnlock: true);

            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                //spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns,
				//flavor text
				this.TranslatedBestiaryEntry()
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 14;
            NPC.height = 12;
            NPC.defense = 0;

            NPC.damage = Main.expertMode ? 40 / 2 : 24;
            NPC.lifeMax = Main.expertMode ? 40 / 2 : 24;

            NPC.knockBackResist = 0f;
            NPC.npcSlots = 1f;
            NPC.dontCountMe = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath4;
            NPC.dontTakeDamage = false;

            NPC.GetGlobalNPC<PolaritiesNPC>().usesProjectileHitCooldowns = true;
            NPC.GetGlobalNPC<PolaritiesNPC>().projectileHitCooldownTime = 10;

            if (Main.getGoodWorld) NPC.scale = 2f;
        }

        public override void AI()
        {
            if (NPC.localAI[0] == 0)
            {
                NPC.TargetClosest(false);

                NPC.localAI[0] = 1;

                if (Main.netMode != 1)
                {
                    speed = Main.rand.NextFloat(12f, 24f);
                    radiusModifier = Main.rand.NextFloat(0.95f, 1.05f);
                }
                NPC.netUpdate = true;
            }

            Player player = Main.player[owner.target];
            if (!player.active || player.dead)
            {
                NPC.netUpdate = true;
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
            }

            if (owner.life <= 0 || owner.ai[0] == -1 || !owner.active)
            {
                runAway = true;
            }
            if (runAway)
            {
                player = Main.player[NPC.target];

                float maxAcceleration = 1f;

                Vector2 goalVelocity = (NPC.Center - player.Center).SafeNormalize(Vector2.Zero) * speed;
                Vector2 goalAcceleration = goalVelocity - NPC.velocity;
                if (goalAcceleration.Length() > maxAcceleration)
                {
                    goalAcceleration.Normalize();
                    goalAcceleration *= maxAcceleration;
                }
                NPC.velocity += goalAcceleration;

                NPC.rotation = (float)(0.5 * Math.Atan(NPC.velocity.X));
                NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;

                return;
            }

            bool doCircling = true;

            //AI stuff (sets doCircling to false if it's doing something other than circling
            switch (owner.ai[0])
            {
                case 0:
                    //reset everything
                    NPC.ai[3] = 0;

                    break;
                case 2:
                    //dissipating stream attack

                    if (NPC.ai[3] == 0 && owner.ai[1] < 120 && owner.ai[1] > 30)
                    {
                        float theta = (NPC.Center - circleCenter).ToRotation() - (owner.Center - circleCenter).ToRotation();

                        while (theta > MathHelper.Pi)
                        {
                            theta -= MathHelper.TwoPi;
                        }
                        while (theta < -MathHelper.Pi)
                        {
                            theta += MathHelper.TwoPi;
                        }
                        if (Math.Abs(theta) < MathHelper.TwoPi / 64f)
                        {
                            if (Main.netMode != 1)
                            {
                                if (Main.rand.NextBool(4))
                                {
                                    NPC.ai[3] = 1;
                                }
                            }
                            NPC.netUpdate = true;
                        }
                    }
                    else if (NPC.ai[3] == 1)
                    {
                        doCircling = false;

                        float speed = owner.life < owner.lifeMax * 0.65f ? 12f : 8f;
                        NPC.velocity = NPC.DirectionTo(circleCenter) * speed;
                        NPC.ai[3] = 2;
                    }
                    else if (NPC.ai[3] == 2)
                    {
                        doCircling = false;

                        if ((NPC.Center - circleCenter).Length() < CircleRadius / 2f)
                        {
                            if (Main.netMode != 1)
                            {
                                float angleVariation = owner.life < owner.lifeMax * 0.65f ? 0.06f : 0.04f;
                                NPC.ai[3] = 3 + Main.rand.NextFloat(-angleVariation, angleVariation);
                            }
                            NPC.netUpdate = true;
                        }
                    }
                    else if (NPC.ai[3] > 2 && NPC.ai[3] < 4)
                    {
                        doCircling = false;

                        NPC.velocity = NPC.velocity.RotatedBy(NPC.ai[3] - 3);

                        if ((NPC.Center - circleCenter).Length() > CircleRadius)
                        {
                            NPC.ai[3] = 0;
                        }
                    }
                    break;
                case 3:
                case 7:
                case 8:
                    //regularly spaced orthogonal/diagonal/octogonal darts towards player

                    if (NPC.ai[3] == 0 && owner.ai[1] < 90 && owner.ai[1] > 30)
                    {
                        float theta = (NPC.Center - circleCenter).ToRotation();
                        if (owner.ai[0] == 7)
                        {
                            theta += MathHelper.TwoPi / 8f;
                        }
                        else if (owner.ai[0] == 8)
                        {
                            theta *= 2;
                        }

                        while (theta > MathHelper.TwoPi / 8f)
                        {
                            theta -= MathHelper.TwoPi / 4f;
                        }
                        while (theta < -MathHelper.TwoPi / 8f)
                        {
                            theta += MathHelper.TwoPi / 4f;
                        }
                        if (Math.Abs(theta) < MathHelper.TwoPi / 64f)
                        {
                            if (Main.netMode != 1)
                            {
                                if (Main.rand.NextBool(16))
                                {
                                    NPC.ai[3] = 1;
                                }
                            }
                            NPC.netUpdate = true;
                        }
                    }
                    else if (NPC.ai[3] == 1)
                    {
                        doCircling = false;
                        NPC.velocity = NPC.DirectionTo(player.Center) * 8f;
                        NPC.ai[3] = 2;
                    }
                    else if (NPC.ai[3] == 2)
                    {
                        doCircling = false;

                        if ((NPC.Center - circleCenter).Length() > CircleRadius && owner.ai[1] > 90)
                        {
                            NPC.ai[3] = 0;
                        }
                    }

                    break;
                case 4:
                case 9:
                    //random chord darts and the followup dash
                    if (NPC.ai[3] == 0 && owner.ai[1] < 105 && owner.ai[1] > 15)
                    {
                        if (Main.netMode != 1)
                        {
                            if (Main.rand.NextBool(80))
                            {
                                NPC.ai[3] = 1;
                            }
                        }
                        NPC.netUpdate = true;
                    }
                    else if (NPC.ai[3] == 1)
                    {
                        doCircling = false;

                        //angle is always just under π
                        float throughAttack = (owner.ai[1] - 15f) / (105f - 15f);
                        float angle = 2.4f + (3f - 2.4f) * throughAttack;

                        NPC.velocity = (NPC.Center - circleCenter).SafeNormalize(Vector2.Zero).RotatedBy(angle) * 8f;
                        NPC.ai[3] = 2;
                    }
                    else if (NPC.ai[3] == 2)
                    {
                        doCircling = false;

                        if ((NPC.Center - circleCenter).Length() > CircleRadius && owner.ai[1] > 105)
                        {
                            NPC.ai[3] = 0;
                        }
                    }
                    break;
            }


            if (doCircling)
            {
                CircleMotion(circleCenter, speed);
            }

            NPC.rotation = (float)(0.5 * Math.Atan(NPC.velocity.X));
            NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
        }

        private void CircleMotion(Vector2 circleCenter, float speed = 12f, float maxAcceleration = 1f)
        {
            float goalRadiusWeight = 0.2f;
            float radius = (goalRadiusWeight * CircleRadius * radiusModifier + (NPC.Center - circleCenter).Length()) / (1 + goalRadiusWeight);
            Vector2 goalPosition = circleCenter + (NPC.Center - circleCenter).SafeNormalize(Vector2.Zero).RotatedBy(speed / radius) * radius;
            Vector2 goalVelocity = (goalPosition - NPC.Center).SafeNormalize(Vector2.Zero) * speed;
            Vector2 goalAcceleration = goalVelocity - NPC.velocity;
            if (goalAcceleration.Length() > maxAcceleration)
            {
                goalAcceleration.Normalize();
                goalAcceleration *= maxAcceleration;
            }
            NPC.velocity += goalAcceleration;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter == 5)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y = (NPC.frame.Y + frameHeight) % (4 * frameHeight);
            }
        }

        public override bool PreKill()
        {
            return false;
        }
    }

    public class GigabatEcholocation : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Pixel";

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.alpha = 0;
            Projectile.timeLeft = 1200;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = false;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Projectile.frameCounter = (Projectile.frameCounter + 1) % 10;
            if (Projectile.frameCounter == 0)
            {
                int numDusts = 20;
                for (int i = 0; i < numDusts; i++)
                {
                    int dust = Dust.NewDust(Projectile.Center, 0, 0, 92, Scale: 1.5f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].noLight = true;
                    Main.dust[dust].velocity = new Vector2(4, 0).RotatedBy(i * MathHelper.TwoPi / numDusts);
                }
            }
        }

        public override bool CanHitPlayer(Player target)
        {
            if (Collision.CheckAABBvAABBCollision(target.position, new Vector2(target.width, target.height), Projectile.position, new Vector2(Projectile.width, Projectile.height)))
            {
                target.AddBuff(BuffType<Pinpointed>(), 720, true);
                target.Hurt(PlayerDeathReason.ByProjectile(target.whoAmI, Projectile.whoAmI), 1, 0);
                Projectile.Kill();
            }
            return false;
        }
    }
}