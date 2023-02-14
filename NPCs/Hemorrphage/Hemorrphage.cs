using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
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

namespace Polarities.NPCs.Hemorrphage
{
    [AutoloadBossHead]
    public class Hemorrphage : ModNPC
    {
        private int LEGCOUNT = Main.expertMode ? 12 : 8;
        private float VELOCITYMULTIPLIER = Main.expertMode ? 30 : 60;
        private int[] legs = new int[20];

        private int ARMCOUNT = Main.expertMode ? 8 : 6;
        private int[] arms = new int[8];

        private float angleGoal;
        private int mostRecentAttack;

        private float attackCooldown
        {
            get => attackCooldown = NPC.ai[0];
            set => NPC.ai[0] = value;
        }
        private int attackPattern
        {
            get => attackPattern = (int)NPC.ai[1];
            set => NPC.ai[1] = (float)value;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hemorrphage");
            Main.npcFrameCount[NPC.type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 56;
            NPC.height = 56;
            DrawOffsetY = 26;

            NPC.defense = 60;
            NPC.damage = 100;
            NPC.lifeMax = 80000;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 15, 0, 0);
            NPC.npcSlots = 15f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            NPC.buffImmune[BuffID.Confused] = true;

            Music = MusicLoader.GetMusicSlot("Sounds/Music/Hemorrphage");
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.lifeMax = (int)(100000 * bossLifeScale);
        }

        public override void AI()
        {
            //always a blood moon
            if (Main.dayTime || (!Main.dayTime && Main.time == Main.nightLength - 1))
            {
                Main.dayTime = false;
                Main.time = Main.nightLength - 2;
            }
            if (!Main.bloodMoon)
            {
                Main.bloodMoon = true;
            }

            Lighting.AddLight(NPC.Center, 2f, 2f, 2f);

            if (NPC.localAI[0] == 0)
            {
                NPC.localAI[0] = 1;
                for (int i = 0; i < LEGCOUNT; i++)
                {
                    legs[i] = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y + 100, NPCType<HemorrphageLeg>(), ai0: NPC.whoAmI, ai1: (240 / LEGCOUNT) * i, ai3: i % 2);
                    Main.npc[legs[i]].velocity.X = 2 * i - (LEGCOUNT - 1);
                    Main.npc[legs[i]].velocity.Y = 1;
                }
                for (int i = 0; i < ARMCOUNT; i++)
                {
                    arms[i] = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y + 100, NPCType<HemorrphageTentacle>(), ai0: NPC.whoAmI, ai2: MathHelper.TwoPi / ARMCOUNT * i);
                    Main.npc[legs[i]].velocity.X = 2 * i - (ARMCOUNT - 1);
                    Main.npc[legs[i]].velocity.Y = 1;
                }

                attackCooldown = -60;
            }

            Player player = Main.player[NPC.target];
            if (!player.active || player.dead || !Main.bloodMoon)
            {
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
                if (player.dead || !Main.bloodMoon)
                {
                    if (NPC.timeLeft > 10)
                    {
                        NPC.timeLeft = 10;
                    }
                    NPC.velocity.Y += 0.1f;
                    return;
                }
            }

            int livingLegs = 0;
            Vector2 targetPosition = Vector2.Zero;
            for (int i = 0; i < 4; i++)
            {
                if (true)
                {//!Main.npc[legs[i]].noTileCollide) {
                    targetPosition += Main.npc[legs[i]].Center + new Vector2(0, -540);
                    livingLegs++;
                }
            }
            if (livingLegs != 0)
            {
                targetPosition /= livingLegs;
            }
            else
            {
                targetPosition = player.Center;
            }

            float playerWeight = 0.75f * (1 - 0.8f * (float)NPC.life / (float)NPC.lifeMax) + (player.Center - NPC.Center).Length() / 1800f;

            targetPosition += player.Center * playerWeight;
            targetPosition /= (1 + playerWeight);

            Vector2 targetVelocity = (targetPosition - NPC.Center) / VELOCITYMULTIPLIER / 1.5f;
            NPC.velocity += (targetVelocity - NPC.velocity) / 60;

            //attack code
            switch (attackPattern)
            {
                case 0:
                    angleGoal = 0;

                    attackCooldown++;
                    if (attackCooldown == (Main.expertMode ? 30 : 45))
                    {
                        attackCooldown = 0;

                        //maximum number of attacks
                        int maxNumAttacks = 12;

                        int[] allowedAttacks = new int[maxNumAttacks];
                        int numAttacks = 0;

                        for (int i = 1; i < maxNumAttacks + 1; i++)
                        {
                            bool allowI = mostRecentAttack != i;
                            if (allowI)
                            {
                                switch(i)
                                {
                                    case 1:
                                        allowI = NPC.life > 2 * NPC.lifeMax / 3 || NPC.ai[3] > 420; //only occurs in phase 1 or in phase 2 with bloodsaws
                                        break;
                                    case 3:
                                        allowI = NPC.ai[3] == 0 && NPC.life > 2 * NPC.lifeMax / 3; //bloodsaw cooldown
                                        break;
                                    case 4:
                                        allowI = NPC.ai[2] == 0; //teleport cooldown under normal circumstances
                                        break;
                                    case 5:
                                    case 6:
                                    case 11:
                                        allowI = NPC.life > 2 * NPC.lifeMax / 3 && NPC.ai[3] <= 420; //cannot occur while bloodsaws are active
                                        break;
                                    case 7:
                                        allowI = NPC.ai[3] == 0 && NPC.life <= 2 * NPC.lifeMax / 3; //bloodsaw cooldown
                                        break;
                                    case 8:
                                    case 9:
                                    case 10:
                                    case 12:
                                        allowI = NPC.life <= 2 * NPC.lifeMax / 3 && NPC.ai[3] <= 420; //phase 2, cannot occur while bloodsaws are active
                                        break;
                                }
                            }

                            if (allowI)
                            {
                                allowedAttacks[numAttacks] = i;
                                numAttacks++;
                            }
                        }

                        attackPattern = allowedAttacks[Main.rand.Next(numAttacks)];

                        if ((player.Center - NPC.Center).Length() > 1600)
                        {
                            attackPattern = 4; //teleport if the player gets too far
                            attackCooldown = 180; //do so immediately
                        }

                        NPC.netUpdate = true;
                        mostRecentAttack = attackPattern;
                    }
                    break;
                case 1:
                    //upwards blood vomit (phase 1)
                    if (attackCooldown <= 180)
                        angleGoal = MathHelper.Pi;
                    else
                    {
                        angleGoal = 0;
                    }

                    if (attackCooldown >= 30 && attackCooldown < 120)
                    {
                        Main.projectile[Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -Main.rand.NextFloat(22, 26)).RotatedBy(0.25f).RotatedByRandom(0.15f), ProjectileType<BloodVomit>(), 0, 0, Main.myPlayer)].timeLeft = 20;
                        Main.projectile[Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -Main.rand.NextFloat(22, 26)).RotatedBy(-0.25f).RotatedByRandom(0.15f), ProjectileType<BloodVomit>(), 0, 0, Main.myPlayer)].timeLeft = 20;
                    }

                    if (attackCooldown == 120)
                    {
                        SoundEngine.PlaySound(SoundID.NPCDeath13.WithPitchOffset(Main.rand.NextFloat(-0.25f, -0.125f)), NPC.Center);

                        int numProjectiles = Main.expertMode ? 80 : 60;

                        for (int i = 0; i < numProjectiles; i++)
                        {
                            if (Main.netMode != 1)
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -Main.rand.NextFloat(22, 26)).RotatedBy(0.25f).RotatedByRandom(0.15f), ProjectileType<BloodVomit>(), 35, 0, Main.myPlayer);
                        }
                        for (int i = 0; i < numProjectiles; i++)
                        {
                            if (Main.netMode != 1)
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -Main.rand.NextFloat(22, 26)).RotatedBy(-0.25f).RotatedByRandom(0.15f), ProjectileType<BloodVomit>(), 35, 0, Main.myPlayer);
                        }
                    }

                    attackCooldown++;
                    if (attackCooldown == 240)
                    {
                        attackPattern = 0;
                        attackCooldown = 0;
                    }
                    break;
                case 2:
                    //ya-yeet
                    angleGoal = (player.Center - NPC.Center).ToRotation() - MathHelper.PiOver2;

                    if (attackCooldown == 0)
                    {
                        SoundEngine.PlaySound(SoundID.NPCDeath10.WithVolumeScale(1.2f).WithPitchOffset(-0.5f), NPC.Center);
                        SoundEngine.PlaySound(SoundID.Roar.WithVolumeScale(1.2f).WithPitchOffset(-0.5f), NPC.Center);
                        for (int i = 0; i < Main.maxPlayers; i++)
                        {
                            if (Main.player[i].active && (Main.player[i].Center - NPC.Center).Length() < 6000)
                            {
                                Main.player[i].GetModPlayer<PolaritiesPlayer>().AddScreenShake(12, 90);
                            }
                        }
                    }
                    else if (attackCooldown >= 15)
                    {
                        for (int i = 0; i < Main.maxPlayers; i++)
                        {
                            if (Main.player[i].active && (Main.player[i].Center - NPC.Center).Length() < 6000)
                            {
                                Main.player[i].wingTime = 0;
                                Main.player[i].mount.Dismount(Main.player[i]);
                            }
                        }
                    }

                    float chargeSpeed = Main.expertMode ? 0.3f : 0.2f;
                    NPC.velocity += (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * chargeSpeed;

                    attackCooldown++;
                    if (attackCooldown == 90)
                    {
                        attackPattern = 0;
                        attackCooldown = 0;
                    }
                    break;
                case 3:
                    //bloodsaws
                    angleGoal = attackCooldown * 0.1f;

                    if (attackCooldown > 30 && attackCooldown % 30 == 0 && Main.netMode != 1)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(6, 0).RotatedBy(angleGoal + MathHelper.PiOver2), ProjectileType<HomingClot>(), 40, 3, Main.myPlayer, player.whoAmI);
                    }

                    attackCooldown++;
                    if (attackCooldown == 180)
                    {
                        NPC.ai[3] = 900;

                        attackPattern = 0;
                        attackCooldown = 0;
                    }
                    break;
                case 4:
                    //teleports above you: 'nothing personal, kid'

                    if (attackCooldown >= 10 && attackCooldown < 30)
                    {
                        NPC.velocity.Y += 0.875f;
                    }
                    else if (attackCooldown >= 40 && attackCooldown < 100)
                    {
                        NPC.velocity.Y -= 1.75f;
                    }

                    if (attackCooldown == 180)
                    {
                        for (int i = 0; i < Main.maxPlayers; i++)
                        {
                            if (Main.player[i].active && (Main.player[i].Center - NPC.Center).Length() < 6000)
                            {
                                Main.player[i].GetModPlayer<PolaritiesPlayer>().AddScreenShake(12, 90);
                            }
                        }

                        NPC.position = (NPC.position - NPC.Center) + player.Center + new Vector2(0, -720);
                        NPC.velocity = Vector2.Zero;

                        int legDirection = player.velocity.X <= 0 ? ((player.velocity.X == 0 && Main.rand.NextBool()) ? -1 : 1) : -1;

                        for (int i = 0; i < LEGCOUNT; i++)
                        {
                            Main.npc[legs[i]].position = (Main.npc[legs[i]].position - Main.npc[legs[i]].Center) + player.Center + new Vector2(0, -720);
                            Main.npc[legs[i]].velocity.X = legDirection * (2 * i - (LEGCOUNT - 1));
                            Main.npc[legs[i]].velocity.Y = 1;
                            Main.npc[legs[i]].netUpdate = true;
                        }

                        for (int i = 0; i < ARMCOUNT; i++)
                        {
                            Main.npc[arms[i]].position = (Main.npc[arms[i]].position - Main.npc[arms[i]].Center) + player.Center + new Vector2(0, -720);
                            Main.npc[arms[i]].velocity.X = legDirection * (2 * i - (ARMCOUNT - 1));
                            Main.npc[arms[i]].velocity.Y = 1;
                            Main.npc[arms[i]].netUpdate = true;
                        }

                        SoundEngine.PlaySound(SoundID.NPCDeath10.WithVolumeScale(1.2f).WithPitchOffset(-0.5f), NPC.Center);
                        SoundEngine.PlaySound(SoundID.Roar.WithVolumeScale(1.2f).WithPitchOffset(-0.5f), NPC.Center);
                    }

                    attackCooldown++;
                    if (attackCooldown == 360)
                    {
                        NPC.ai[2] = 600;

                        attackPattern = 0;
                        attackCooldown = 0;
                    }
                    break;
                case 5:
                    //continuous blood streams
                    angleGoal = (player.Center - NPC.Center).ToRotation() - MathHelper.PiOver2;

                    if (attackCooldown < 90)
                    {
                        if (attackCooldown % 5 == 0)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, Main.rand.NextFloat(22, 26)).RotatedBy(angleGoal).RotatedByRandom(0.05f), ProjectileType<BloodVomit>(), 0, 0, Main.myPlayer, 20);
                        }
                    }

                    if (attackCooldown % 5 == 0 && attackCooldown >= 90 && Main.netMode != 1)
                    {
                        if (!Main.expertMode)
                        {
                            if (attackCooldown % 10 == 0)
                            {
                                SoundEngine.PlaySound(SoundID.Item13, NPC.Center);
                            }

                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, Main.rand.NextFloat(22, 26)).RotatedBy(angleGoal).RotatedByRandom(0.05f), ProjectileType<BloodVomit>(), 35, 0, Main.myPlayer, 180);
                        }
                        else
                        {
                            float a = 0.15f;
                            float v = 30;
                            float x = player.Center.X - NPC.Center.X;
                            float y = player.Center.Y - NPC.Center.Y;

                            float direction = x < 0 ? -1 : 1;

                            double theta = (new Vector2(x, y)).ToRotation();
                            theta += Math.PI / 2;
                            if (theta > Math.PI) { theta -= Math.PI * 2; }
                            theta *= 0.5;
                            theta -= Math.PI / 2;

                            double num0 = -4 * Math.Pow(a, 2) * Math.Pow(x, 4) + 4 * a * Math.Pow(v, 2) * Math.Pow(x, 2) * y + Math.Pow(v, 4) * Math.Pow(x, 2);
                            if (num0 > 0)
                            {
                                num0 = -direction * Math.Sqrt(num0);
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
                                if (thetaDir != direction) { theta -= Math.PI; }
                            }

                            if (attackCooldown % 10 == 0)
                            {
                                SoundEngine.PlaySound(SoundID.Item13, NPC.Center);
                            }

                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(Main.rand.NextFloat(22, 26), 0).RotatedBy(theta).RotatedByRandom(0.05f), ProjectileType<BloodVomit>(), 35, 0, Main.myPlayer, 180);
                        }
                    }

                    attackCooldown++;
                    if (attackCooldown == 270)
                    {
                        attackPattern = 0;
                        attackCooldown = 0;
                    }
                    break;
                case 6:
                    //tentacles stick outwards and shoot projectiles

                    if (attackCooldown == 0)
                    {
                        SoundEngine.PlaySound(SoundID.ForceRoar.WithPitchOffset(Main.rand.NextFloat(-0.25f, -0.125f)), NPC.Center);
                    }

                    if (attackCooldown > 120)
                    {
                        NPC.velocity *= 0.95f;

                        if (attackCooldown % 10 == 0)
                        {
                            SoundEngine.PlaySound(SoundID.Item13, NPC.Center);
                        }
                    }

                    attackCooldown++;
                    if (attackCooldown == 240)
                    {
                        attackPattern = 0;
                        attackCooldown = 0;
                    }
                    break;
                case 7:
                    //more bloodsaws
                    angleGoal = attackCooldown * 0.1f;

                    if (attackCooldown > 30 && attackCooldown % 22 == 8 && Main.netMode != 1)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(6, 0).RotatedBy(angleGoal + MathHelper.PiOver2), ProjectileType<HomingClot>(), 40, 3, Main.myPlayer, player.whoAmI);
                    }

                    attackCooldown++;
                    if (attackCooldown == 180)
                    {
                        NPC.ai[3] = 900;

                        attackPattern = 0;
                        attackCooldown = 0;
                    }
                    break;
                case 8:
                    //upwards blood vomit (phase 2)
                    if (attackCooldown <= 210)
                        angleGoal = MathHelper.Pi;
                    else
                    {
                        angleGoal = 0;
                    }

                    if (attackCooldown >= 30 && attackCooldown < 120)
                    {
                        Main.projectile[Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -Main.rand.NextFloat(22, 26)).RotatedBy(0.25f).RotatedByRandom(0.15f), ProjectileType<BloodVomit>(), 0, 0, Main.myPlayer)].timeLeft = 20;
                        Main.projectile[Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -Main.rand.NextFloat(22, 26)).RotatedBy(-0.25f).RotatedByRandom(0.15f), ProjectileType<BloodVomit>(), 0, 0, Main.myPlayer)].timeLeft = 20;
                    }

                    if (attackCooldown == 120)
                    {
                        SoundEngine.PlaySound(SoundID.NPCDeath13.WithPitchOffset(Main.rand.NextFloat(-0.25f, -0.125f)), NPC.Center);

                        int numProjectiles = Main.expertMode ? 80 : 60;

                        for (int i = 0; i < numProjectiles; i++)
                        {
                            if (Main.netMode != 1)
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -Main.rand.NextFloat(22, 26)).RotatedBy(0.25f).RotatedByRandom(0.15f), ProjectileType<BloodVomit>(), 35, 0, Main.myPlayer);
                        }
                        for (int i = 0; i < numProjectiles; i++)
                        {
                            if (Main.netMode != 1)
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -Main.rand.NextFloat(22, 26)).RotatedBy(-0.25f).RotatedByRandom(0.15f), ProjectileType<BloodVomit>(), 35, 0, Main.myPlayer);
                        }
                    }

                    if (attackCooldown > 120 && attackCooldown < 180)
                    {
                        if (attackCooldown % 10 == 0)
                        {
                            SoundEngine.PlaySound(SoundID.Item13, NPC.Center);
                        }

                        if (attackCooldown % 2 == 0)
                        {
                            float angle = (190 - attackCooldown) / 400;

                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -24).RotatedBy(angle), ProjectileType<BloodVomit>(), 35, 0, Main.myPlayer);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -24).RotatedBy(-angle), ProjectileType<BloodVomit>(), 35, 0, Main.myPlayer);
                        }
                    }

                    attackCooldown++;
                    if (attackCooldown == 270)
                    {
                        attackPattern = 0;
                        attackCooldown = 0;
                    }
                    break;
                case 9:
                    //tentacles stick outwards, wave slightly and shoot projectiles

                    if (attackCooldown == 0)
                    {
                        SoundEngine.PlaySound(SoundID.ForceRoar.WithPitchOffset(Main.rand.NextFloat(-0.25f, -0.125f)), NPC.Center);
                    }

                    if (attackCooldown > 120)
                    {
                        NPC.velocity *= 0.95f;

                        if (attackCooldown % 10 == 0)
                        {
                            SoundEngine.PlaySound(SoundID.Item13, NPC.Center);
                        }
                    }

                    attackCooldown++;
                    if (attackCooldown == 360)
                    {
                        attackPattern = 0;
                        attackCooldown = 0;
                    }
                    break;
                case 10:
                    //continuous blood streams
                    angleGoal = (player.Center - NPC.Center).ToRotation() - MathHelper.PiOver2;

                    if (attackCooldown < 90)
                    {
                        if (attackCooldown % 5 == 0)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, Main.rand.NextFloat(22, 26)).RotatedBy(angleGoal).RotatedByRandom(0.05f), ProjectileType<BloodVomit>(), 0, 0, Main.myPlayer, 20);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, Main.rand.NextFloat(22, 26)).RotatedBy(0.3f + angleGoal).RotatedByRandom(0.05f), ProjectileType<BloodVomit>(), 0, 0, Main.myPlayer, 20);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, Main.rand.NextFloat(22, 26)).RotatedBy(-0.3f + angleGoal).RotatedByRandom(0.05f), ProjectileType<BloodVomit>(), 0, 0, Main.myPlayer, 20);
                        }
                    }

                    if (attackCooldown % 5 == 0 && attackCooldown >= 90 && Main.netMode != 1)
                    {
                        if (!Main.expertMode)
                        {
                            if (attackCooldown % 10 == 0)
                            {
                                SoundEngine.PlaySound(SoundID.Item13, NPC.Center);
                            }

                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, Main.rand.NextFloat(22, 26)).RotatedBy(angleGoal).RotatedByRandom(0.05f), ProjectileType<BloodVomit>(), 35, 0, Main.myPlayer, 180);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, Main.rand.NextFloat(22, 26)).RotatedBy(0.3f + angleGoal).RotatedByRandom(0.05f), ProjectileType<BloodVomit>(), 35, 0, Main.myPlayer, 180);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, Main.rand.NextFloat(22, 26)).RotatedBy(-0.3f + angleGoal).RotatedByRandom(0.05f), ProjectileType<BloodVomit>(), 35, 0, Main.myPlayer, 180);
                        }
                        else
                        {
                            float a = 0.15f;
                            float v = 30;
                            float x = player.Center.X - NPC.Center.X;
                            float y = player.Center.Y - NPC.Center.Y;

                            float direction = x < 0 ? -1 : 1;

                            double theta = (new Vector2(x, y)).ToRotation();
                            theta += Math.PI / 2;
                            if (theta > Math.PI) { theta -= Math.PI * 2; }
                            theta *= 0.5;
                            theta -= Math.PI / 2;

                            double num0 = -4 * Math.Pow(a, 2) * Math.Pow(x, 4) + 4 * a * Math.Pow(v, 2) * Math.Pow(x, 2) * y + Math.Pow(v, 4) * Math.Pow(x, 2);
                            if (num0 > 0)
                            {
                                num0 = -direction * Math.Sqrt(num0);
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
                                if (thetaDir != direction) { theta -= Math.PI; }
                            }

                            if (attackCooldown % 10 == 0)
                            {
                                SoundEngine.PlaySound(SoundID.Item13, NPC.Center);
                            }

                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(Main.rand.NextFloat(22, 26), 0).RotatedBy(theta).RotatedByRandom(0.05f), ProjectileType<BloodVomit>(), 35, 0, Main.myPlayer, 180);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(Main.rand.NextFloat(22, 26), 0).RotatedBy(0.3f + theta).RotatedByRandom(0.05f), ProjectileType<BloodVomit>(), 35, 0, Main.myPlayer, 180);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(Main.rand.NextFloat(22, 26), 0).RotatedBy(-0.3f + theta).RotatedByRandom(0.05f), ProjectileType<BloodVomit>(), 35, 0, Main.myPlayer, 180);
                        }
                    }

                    attackCooldown++;
                    if (attackCooldown == 270)
                    {
                        attackPattern = 0;
                        attackCooldown = 0;
                    }
                    break;
                case 11:
                    //blood orbiters

                    if (attackCooldown == 0)
                    {
                        SoundEngine.PlaySound(SoundID.NPCDeath19, NPC.Center);

                        if (Main.netMode != 1)
                        {
                            int direction = Main.rand.NextBool() ? 1 : -1;
                            int numPlates = 10;
                            float rotationOffset = Main.rand.NextFloat(MathHelper.TwoPi);

                            for (int i = 0; i < numPlates; i++)
                            {
                                Vector2 speed = new Vector2(8f, 0).RotatedBy(NPC.rotation - MathHelper.PiOver2);

                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, speed, ProjectileType<Platelet>(), 35, 0f, player.whoAmI, 480, direction * (i + 1) * MathHelper.TwoPi / numPlates + direction * rotationOffset);
                            }
                        }
                    }

                    attackCooldown++;
                    if (attackCooldown == 300)
                    {
                        attackPattern = 0;
                        attackCooldown = 0;
                    }
                    break;
                case 12:
                    //blood orbiters 2

                    if (attackCooldown % 60 == 0 && attackCooldown < 240)
                    {
                        SoundEngine.PlaySound(SoundID.NPCDeath19, NPC.Center);

                        if (Main.netMode != 1)
                        {
                            int direction = attackCooldown % 120 == 0 ? 1 : -1;
                            int numPlates = 10;
                            float rotationOffset = Main.rand.NextFloat(MathHelper.TwoPi);

                            for (int i = 0; i < numPlates; i++)
                            {
                                Vector2 speed = new Vector2(8f, 0).RotatedBy(NPC.rotation - MathHelper.PiOver2);

                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, speed, ProjectileType<Platelet>(), 35, 0f, player.whoAmI, 480, direction * (i + 1) * MathHelper.TwoPi / numPlates + direction * rotationOffset);
                            }
                        }
                    }

                    attackCooldown++;
                    if (attackCooldown == 360)
                    {
                        attackPattern = 0;
                        attackCooldown = 0;
                    }
                    break;
            }

            if (NPC.ai[2] > 0)
                NPC.ai[2]--; //teleport cooldown
            if (NPC.ai[3] > 0)
                NPC.ai[3]--; //are bloodsaws alive

            //rotation code
            if (angleGoal > MathHelper.TwoPi) angleGoal -= MathHelper.TwoPi;
            else if (angleGoal < 0) angleGoal += MathHelper.TwoPi;
            if (NPC.rotation > MathHelper.TwoPi) NPC.rotation -= MathHelper.TwoPi;
            else if (NPC.rotation < 0) NPC.rotation += MathHelper.TwoPi;

            float angleOffset = angleGoal - NPC.rotation;
            if (angleOffset > MathHelper.Pi) angleOffset -= MathHelper.TwoPi;
            else if (angleOffset < -MathHelper.Pi) angleOffset += MathHelper.TwoPi;

            float maxTurn = 0.2f;

            if (Math.Abs(angleOffset) < maxTurn) { NPC.rotation = angleGoal; }
            else if (angleOffset > 0)
            {
                NPC.rotation += maxTurn;
            }
            else
            {
                NPC.rotation -= maxTurn;
            }
        }

        public override bool CheckDead()
        {
            if (!PolaritiesSystem.downedHemorrphage)
            {
                PolaritiesSystem.downedHemorrphage = true;
                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.WorldData); // Immediately inform clients of new world state.
                }
            }

            SoundEngine.PlaySound(SoundID.NPCDeath10.WithVolumeScale(1.2f).WithPitchOffset(-0.75f), NPC.Center);
            SoundEngine.PlaySound(SoundID.Roar.WithVolumeScale(1.2f).WithPitchOffset(-0.75f), NPC.Center);

            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 1; i <= 2; i++)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity, GoreHelper.GoreType($"HemorrphageGore{i}"));
                }
            }

            for (int i = 0; i < 128; i++)
            {
                Vector2 dustPos = NPC.Center + new Vector2(Main.rand.NextFloat(64), 0).RotatedByRandom(MathHelper.TwoPi);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Blood, Velocity: (dustPos - NPC.Center) / 12, Scale: 2.4f);
                dust.noGravity = true;
            }
            return true;
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }

        public override void OnKill()
        {
            //if (Main.rand.NextBool(10) || NPC.GetGlobalNPC<PolaritiesNPC>().noHit)
            //{
            //    Item.NewItem(NPC.getRect(), ItemType<HemorrphageTrophy>());
            //}

            //if (Main.expertMode)
            //{
            //    NPC.DropBossBags();
            //    if (Main.rand.NextBool(4))
            //    {
            //        Item.NewItem(NPC.getRect(), ItemType<BloodyBloodCell>());
            //    }
            //}
            //else
            //{
            //    if (Main.rand.NextBool(7))
            //    {
            //        Item.NewItem(NPC.getRect(), ItemType<HemorrphageMask>());
            //    }
            //    Item.NewItem(NPC.getRect(), ItemType<HemorrhagicFluid>(), Main.rand.Next(10, 21));
            //    if (Main.rand.NextBool(5))
            //    {
            //        Item.NewItem(NPC.getRect(), ItemType<VolatileHeart>());
            //    }
            //    switch (Main.rand.Next(4))
            //    {
            //        case 0:
            //            Item.NewItem(NPC.getRect(), ItemType<BloodDicer>());
            //            break;
            //        case 1:
            //            Item.NewItem(NPC.getRect(), ItemType<Hemophobia>());
            //            break;
            //        case 2:
            //            Item.NewItem(NPC.getRect(), ItemType<Phagocyte>());
            //            break;
            //        case 3:
            //            Item.NewItem(NPC.getRect(), ItemType<BleedingSky>());
            //            break;
            //    }
            //}
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            for (int i = 0; i < LEGCOUNT; i++)
            {
                writer.Write(legs[i]);
            }
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            for (int i = 0; i < LEGCOUNT; i++)
            {
                legs[i] = reader.ReadInt32();
            }
        }
    }

    internal class HemorrphageLeg : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hemorrphage Claw");
        }

        private int jumpCooldown;

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 14;
            NPC.height = 14;
            DrawOffsetY = 4;

            NPC.defense = 5;
            NPC.damage = 64;
            NPC.lifeMax = 1000;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 0f;
            NPC.lavaImmune = true;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.dontTakeDamage = true;
            NPC.buffImmune[BuffID.Confused] = true;
        }

        public override void AI()
        {
            NPC owner = Main.npc[(int)NPC.ai[0]];
            if (!owner.active)
            {
                if (Main.netMode != NetmodeID.Server)
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity, GoreHelper.GoreType("HemorrphageLeg"));

                NPC.active = false;
                return;
            }

            bool leaving = Main.player[owner.target].dead || !Main.bloodMoon;

            NPC.ai[1]++;
            if (NPC.ai[1] == 240)
            {
                NPC.ai[1] = 0;
            }

            if (NPC.ai[2] > 0)
            {
                NPC.ai[2]--;
            }
            else
            {
                NPC.noTileCollide = false;
            }

            if (NPC.velocity.Y != 0 && (NPC.position.Y + NPC.height) < Main.player[owner.target].position.Y || leaving)
            {
                NPC.noTileCollide = true;
            }

            if ((NPC.velocity.Y == 0 || (NPC.Center + new Vector2(0, -360) - owner.Center).Length() > 600) && !leaving)
            {
                if (NPC.velocity.Y == 0) { NPC.velocity.X = 0; }

                if (jumpCooldown == 0 && (NPC.ai[1] == 0 || ((NPC.Center + new Vector2(0, -360) - owner.Center).Length() > 600 && NPC.Center.Y > Main.player[owner.target].Center.Y + 256)))
                {
                    //leap at the player
                    Player player = Main.player[owner.target];

                    float a = 0.15f;
                    float v = 16;
                    float x = player.Center.X - NPC.Center.X;
                    float y = player.Center.Y - NPC.Center.Y;

                    //go faster if you're doing a midair desparation catchup jump
                    if (NPC.velocity.Y != 0)
                    {
                        v = 20;
                    }

                    NPC.direction = x < 0 ? -1 : 1;

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

                    NPC.velocity = (v * (new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)))).RotatedByRandom(0.1f);
                    jumpCooldown = 60;

                    if ((NPC.Center - owner.Center).Length() > 480)
                    {
                        owner.velocity -= (NPC.velocity - NPC.oldVelocity) / 9;
                    }

                    NPC.ai[2] = 30;
                    NPC.noTileCollide = true;
                }
            }

            if ((NPC.Center + new Vector2(0, -360) - owner.Center).Length() > 600)
            {
                NPC.noTileCollide = true;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float dotProduct = (NPC.velocity - owner.velocity).X * (owner.Center - NPC.Center).X + (NPC.velocity - owner.velocity).Y * (owner.Center - NPC.Center).Y;
                    if (dotProduct < 0)
                    {
                        NPC.velocity -= dotProduct / (owner.Center - NPC.Center).Length() * (owner.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                    }

                    NPC.velocity += (owner.Center - NPC.Center).SafeNormalize(new Vector2(0, -1)) * 4 + new Vector2(1, 0).RotatedByRandom(Math.PI * 2);
                    owner.velocity -= ((owner.Center - NPC.Center).SafeNormalize(new Vector2(0, -1)) * 4 + new Vector2(1, 0).RotatedByRandom(Math.PI * 2)) / 12;
                }
                NPC.netUpdate = true;
            }

            if (jumpCooldown > 0)
            {
                jumpCooldown--;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D legTexture = ModContent.Request<Texture2D>($"{Texture}Chain").Value;

            Vector2 mainCenter = Main.npc[(int)NPC.ai[0]].Center;
            Vector2 center = NPC.Center + new Vector2(0, -240 - 11);
            Vector2 intermediateCenter = center + (mainCenter - NPC.Center) / 2;//+ new Vector2(240, 0).RotatedBy((mainCenter-npc.Center).ToRotation());


            Vector2 distToNPC = intermediateCenter - center;
            float projRotation = distToNPC.ToRotation() + MathHelper.PiOver2;
            float distance = distToNPC.Length();
            int tries = 100;
            while (distance > 12f && !float.IsNaN(distance) && tries > 0)
            {
                tries--;
                //Draw chain
                spriteBatch.Draw(legTexture, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                    new Rectangle(0, 0, 18, 12), Lighting.GetColor((int)(center.X / 16), (int)(center.Y / 16)), projRotation,
                    new Vector2(18 * 0.5f, 12 * 0.5f), 1f, SpriteEffects.None, 0f);

                distToNPC.Normalize();                 //get unit vector
                distToNPC *= 12f;                      //speed = 24
                center += distToNPC;                   //update draw position
                distToNPC = intermediateCenter - center;    //update distance
                distance = distToNPC.Length();
            }
            spriteBatch.Draw(legTexture, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                new Rectangle(0, 0, 18, 12), Lighting.GetColor((int)(center.X / 16), (int)(center.Y / 16)), projRotation,
                new Vector2(18 * 0.5f, 12 * 0.5f), 1f, SpriteEffects.None, 0f);

            distToNPC.Normalize();                 //get unit vector
            distToNPC *= 12f;                      //speed = 24
            center += distToNPC;                   //update draw position
            distToNPC = intermediateCenter - center;    //update distance

            distToNPC = mainCenter - intermediateCenter;
            projRotation = distToNPC.ToRotation() + MathHelper.PiOver2;
            distance = distToNPC.Length();
            tries = 100;
            while (distance > 12f && !float.IsNaN(distance) && tries > 0)
            {
                tries--;
                //Draw chain
                spriteBatch.Draw(legTexture, new Vector2(intermediateCenter.X - Main.screenPosition.X, intermediateCenter.Y - Main.screenPosition.Y),
                    new Rectangle(0, 0, 18, 12), Lighting.GetColor((int)(center.X / 16), (int)(center.Y / 16)), projRotation,
                    new Vector2(18 * 0.5f, 12 * 0.5f), 1f, SpriteEffects.None, 0f);

                distToNPC.Normalize();                 //get unit vector
                distToNPC *= 12f;                      //speed = 24
                intermediateCenter += distToNPC;                   //update draw position
                distToNPC = mainCenter - intermediateCenter;    //update distance
                distance = distToNPC.Length();
            }

            for (int i = 0; i <= 20; i++)
            {
                center = NPC.Center + new Vector2(0, -11 - i * 12);
                spriteBatch.Draw(legTexture, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                    new Rectangle(0, 0, 18, 12), Lighting.GetColor((int)(center.X / 16), (int)(center.Y / 16)), 0,
                    new Vector2(18 * 0.5f, 12 * 0.5f), 1f, SpriteEffects.None, 0f);
            }
            return true;
        }

        public override bool CheckActive()
        {
            return !Main.npc[(int)NPC.ai[0]].active;
        }
    }

    internal class HemorrphageTentacle : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hemorrphage Tentacle");
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 14;
            NPC.height = 14;
            DrawOffsetY = -1;

            NPC.defense = 30;
            NPC.damage = 64;
            NPC.lifeMax = 1200;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 0f;
            NPC.lavaImmune = true;
            NPC.noGravity = false;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = false;

            NPC.buffImmune[BuffID.Confused] = true;
        }

        public override void AI()
        {
            NPC owner = Main.npc[(int)NPC.ai[0]];
            if (!owner.active)
            {
                if (Main.netMode != NetmodeID.Server)
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity, GoreHelper.GoreType("HemorrphageTentacle"));

                NPC.active = false;
                return;
            }

            bool leaving = Main.player[owner.target].dead || !Main.bloodMoon;

            //death/damage cooldown
            if (NPC.ai[3] > 0)
            {
                NPC.ai[3]--;
            }
            NPC.dontTakeDamage = NPC.ai[3] > 0;

            float goalRotMultiplier = 0.5f;

            bool stickOutwardsFully = (owner.ai[1] == 6 || owner.ai[1] == 9) && !leaving;
            bool stickOutwardsPartial = NPC.ai[3] == 0;

            if (stickOutwardsFully)
            {
                Lighting.AddLight(NPC.Center, 2f, 2f, 2f);

                goalRotMultiplier = 1;
                NPC.noGravity = true;

                Vector2 goalPosition = new Vector2(120, 0).RotatedBy(NPC.ai[2]);

                if (owner.ai[1] == 9)
                {
                    goalPosition = goalPosition.RotatedBy(Math.Sin(owner.ai[0] / 45f) / 5f);
                }

                Vector2 goalVelocity = (owner.Center + goalPosition - NPC.Center).SafeNormalize(Vector2.Zero) * 16;
                NPC.velocity += (goalVelocity - NPC.velocity) / 10;

                //radial attack
                if (owner.ai[1] == 6 || owner.ai[1] == 9)
                {
                    if (owner.ai[0] > 120 && owner.ai[0] % 2 == 0 && Main.netMode != 1)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (NPC.Center - owner.Center).SafeNormalize(Vector2.Zero) * 16, ProjectileType<CirclingBloodShot>(), 35, 1f, Main.myPlayer);
                    }
                    else if (owner.ai[0] % 2 == 0)
                    {
                        Main.projectile[Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (NPC.Center - owner.Center).SafeNormalize(Vector2.Zero) * 16, ProjectileType<CirclingBloodShot>(), 0, 1f, Main.myPlayer)].timeLeft = 20;
                    }
                }
            }
            else if (stickOutwardsPartial)
            {
                goalRotMultiplier = 1 - (float)Math.Pow(Math.Cos(NPC.ai[2]), 2) / 3;
                NPC.noGravity = true;

                Vector2 goalVelocity = (owner.Center + new Vector2(90, 0).RotatedBy(NPC.ai[2]) - NPC.Center).SafeNormalize(Vector2.Zero) * 16;
                NPC.velocity += (goalVelocity - NPC.velocity) / 10;
            }
            else
            {
                NPC.noGravity = false;
                if ((NPC.Center - owner.Center).Length() > 120)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        NPC.velocity += (owner.Center - NPC.Center).SafeNormalize(new Vector2(0, -1)) * 4 + new Vector2(1, 0).RotatedByRandom(Math.PI * 2);
                        if ((NPC.velocity - owner.velocity).Length() > 12)
                        {
                            NPC.velocity = (NPC.velocity - owner.velocity).SafeNormalize(Vector2.Zero) * 12 + owner.velocity;
                        }
                    }
                }
            }

            NPC.ai[1] += (goalRotMultiplier - NPC.ai[1]) / 20;

            NPC.rotation = (owner.Center - NPC.Center).RotatedBy(MathHelper.PiOver2).ToRotation();
            NPC.rotation *= NPC.ai[1];
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            if (NPC.ai[3] == 0)
            {
                return null;
            }
            return false;
        }

        public override bool CheckDead()
        {
            NPC.life = NPC.lifeMax;
            NPC.ai[3] = 600;

            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D legTexture = ModContent.Request<Texture2D>($"{ModContent.GetInstance<HemorrphageLeg>().Texture}Chain").Value;

            Vector2 mainCenter = Main.npc[(int)NPC.ai[0]].Center;
            Vector2 center = NPC.Center;
            Vector2 intermediateCenter = center + new Vector2(0, -60).RotatedBy(NPC.rotation);


            Vector2 distToNPC = intermediateCenter - center;

            float projRotation = distToNPC.ToRotation() + MathHelper.PiOver2;
            float distance = distToNPC.Length();

            distToNPC.Normalize();
            distToNPC *= 4f;
            center += distToNPC;

            int tries = 100;
            while (distance > 12f && !float.IsNaN(distance) && tries > 0)
            {
                tries--;
                //Draw chain
                spriteBatch.Draw(legTexture, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                    new Rectangle(0, 0, 18, 12), Lighting.GetColor((int)(center.X / 16), (int)(center.Y / 16)), projRotation,
                    new Vector2(18 * 0.5f, 12 * 0.5f), 1f, SpriteEffects.None, 0f);

                distToNPC.Normalize();                 //get unit vector
                distToNPC *= 12f;                      //speed = 24
                center += distToNPC;                   //update draw position
                distToNPC = intermediateCenter - center;    //update distance
                distance = distToNPC.Length();
            }
            spriteBatch.Draw(legTexture, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                new Rectangle(0, 0, 18, 12), Lighting.GetColor((int)(center.X / 16), (int)(center.Y / 16)), projRotation,
                new Vector2(18 * 0.5f, 12 * 0.5f), 1f, SpriteEffects.None, 0f);

            distToNPC.Normalize();                 //get unit vector
            distToNPC *= 12f;                      //speed = 24
            center += distToNPC;                   //update draw position
            distToNPC = intermediateCenter - center;    //update distance

            distToNPC = mainCenter - intermediateCenter;
            projRotation = distToNPC.ToRotation() + MathHelper.PiOver2;
            distance = distToNPC.Length();
            tries = 100;
            while (distance > 12f && !float.IsNaN(distance) && tries > 0)
            {
                tries--;
                //Draw chain
                spriteBatch.Draw(legTexture, new Vector2(intermediateCenter.X - Main.screenPosition.X, intermediateCenter.Y - Main.screenPosition.Y),
                    new Rectangle(0, 0, 18, 12), Lighting.GetColor((int)(center.X / 16), (int)(center.Y / 16)), projRotation,
                    new Vector2(18 * 0.5f, 12 * 0.5f), 1f, SpriteEffects.None, 0f);

                distToNPC.Normalize();                 //get unit vector
                distToNPC *= 12f;                      //speed = 24
                intermediateCenter += distToNPC;                   //update draw position
                distToNPC = mainCenter - intermediateCenter;    //update distance
                distance = distToNPC.Length();
            }
            return true;
        }

        public override bool CheckActive()
        {
            return !Main.npc[(int)NPC.ai[0]].active;
        }
    }

    public class BloodVomit : ModProjectile
    {
        public override string Texture => "Polarities/NPCs/Hemorrphage/BloodSpray";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Blood Spray");

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 48;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.width = 16;
            Projectile.height = 16;
            DrawOffsetX = 3;
            DrawOriginOffsetY = -2;

            Projectile.alpha = 0;
            Projectile.timeLeft = 1200;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                for (int k = 0; k < Projectile.oldPos.Length; k++)
                {
                    Projectile.oldPos[k] = Projectile.position;
                }
                Projectile.localAI[0] = 1;
            }

            if (Projectile.ai[0] != 0)
            {
                Projectile.timeLeft = (int)Projectile.ai[0];
                Projectile.ai[0] = 0;
            }

            if (Projectile.timeLeft < 20)
            {
                Projectile.alpha = (int)(255 - 255 * (Projectile.timeLeft / 20f));
            }

            Projectile.velocity.Y += 0.15f;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Slow, 60 * 5);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Polarities.CallShootProjectile).Value;

            Color mainColor = new Color(168, 0, 0);

            for (int k = 0; k < Projectile.oldPos.Length - 1; k++)
            {
                float amount = ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

                Color color = mainColor * (1 - Projectile.alpha / 255f);
                float scale = 2f * Projectile.scale * amount;

                float rotation = (Projectile.oldPos[k + 1] - Projectile.oldPos[k]).ToRotation();

                Main.spriteBatch.Draw(texture, Projectile.Center - (Projectile.position - Projectile.oldPos[k]) * 0.9f - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, rotation, new Vector2(0, 0.5f), new Vector2((Projectile.oldPos[k + 1] - Projectile.oldPos[k]).Length(), scale), SpriteEffects.None, 0f);
            }

            return false;
        }
    }

    public class CirclingBloodShot : ModProjectile
    {
        public override string Texture => "Polarities/NPCs/Hemorrphage/BloodSpray";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Blood Spray");

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 48;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.width = 16;
            Projectile.height = 16;
            DrawOffsetX = 3;
            DrawOriginOffsetY = -2;

            Projectile.alpha = 0;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                for (int k = 0; k < Projectile.oldPos.Length; k++)
                {
                    Projectile.oldPos[k] = Projectile.position;
                }

                Projectile.localAI[0] = 1;
                Projectile.ai[0] = Main.rand.NextFloat(-1, 1) * 0.015f;
            }

            if (Projectile.timeLeft < 20)
            {
                Projectile.alpha = (int)(255 - 255 * (Projectile.timeLeft / 20f));
            }

            Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[0]) * 1.015f;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Slow, 60 * 5);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Polarities.CallShootProjectile).Value;

            Color mainColor = new Color(168, 0, 0);

            for (int k = 0; k < Projectile.oldPos.Length - 1; k++)
            {
                float amount = ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

                Color color = mainColor * (1 - Projectile.alpha / 255f);
                float scale = 2f * Projectile.scale * amount;

                float rotation = (Projectile.oldPos[k + 1] - Projectile.oldPos[k]).ToRotation();

                Main.spriteBatch.Draw(texture, Projectile.Center - (Projectile.position - Projectile.oldPos[k]) * 0.9f - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, rotation, new Vector2(0, 0.5f), new Vector2((Projectile.oldPos[k + 1] - Projectile.oldPos[k]).Length(), scale), SpriteEffects.None, 0f);
            }

            return false;
        }
    }

    public class HomingClot : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bloodsaw");
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.width = 52;
            Projectile.height = 52;
            Projectile.alpha = 0;
            Projectile.timeLeft = 540;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.light = 1f;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                SoundEngine.PlaySound(SoundID.Item17, Projectile.Center);
            }

            Vector2 dustPos = Projectile.Center + new Vector2(Main.rand.NextFloat(26), 0).RotatedByRandom(MathHelper.TwoPi);
            Dust dust = Dust.NewDustPerfect(dustPos, DustID.Blood, Velocity: Projectile.velocity, Scale: 1.75f);
            dust.noGravity = true;

            if (Projectile.timeLeft > 420)
            {
                Projectile.velocity.Y += 0.15f;
            }
            else
            {
                Player player = Main.player[(int)Projectile.ai[0]];

                Vector2 goalVelocity = (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 24;
                Projectile.velocity += (goalVelocity - Projectile.velocity) / 90;
            }

            Projectile.rotation += 0.25f;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Slow, 60 * 5);
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 16; i++)
            {
                Vector2 dustPos = Projectile.Center + new Vector2(Main.rand.NextFloat(26), 0).RotatedByRandom(MathHelper.TwoPi);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Blood, Velocity: Projectile.velocity, Scale: 1.75f);
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, (TextureAssets.Projectile[Projectile.type].Value.Height * Projectile.frame) / Main.projFrames[Projectile.type], TextureAssets.Projectile[Projectile.type].Value.Width, TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]), Color.White, Projectile.rotation, new Vector2(TextureAssets.Projectile[Projectile.type].Value.Width / 2 + DrawOriginOffsetX, Projectile.height / 2), Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }

    public class Platelet : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Platelet");
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.alpha = 0;

            Projectile.timeLeft = 405;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;

            Projectile.light = 1f;
        }

        public override void AI()
        {
            Vector2 dustPos = Projectile.Center + new Vector2(Main.rand.NextFloat(26), 0).RotatedByRandom(MathHelper.TwoPi);
            Dust dust = Dust.NewDustPerfect(dustPos, DustID.Blood, Velocity: Projectile.velocity, Scale: 1.75f);
            dust.noGravity = true;

            Projectile.ai[1] += 0.01f * (Projectile.ai[1] > 0 ? 1 : -1);

            if (Projectile.timeLeft > 300)
            {
                Player player = Main.player[Projectile.owner];

                Vector2 goalPosition = player.Center + new Vector2(Projectile.ai[0], 0).RotatedBy(Projectile.ai[1]);

                Vector2 goalVelocity = GoalVelocityRadial(goalPosition, player.Center, (Projectile.timeLeft - 300), 1000f);
                Projectile.velocity += (goalVelocity - Projectile.velocity) / (float)Math.Sqrt(Projectile.timeLeft - 300);
            }
            else
            {
                if (Projectile.timeLeft == 300)
                {
                    Player player = Main.player[Projectile.owner];

                    Projectile.localAI[0] = player.Center.X;
                    Projectile.localAI[1] = player.Center.Y;
                }

                Vector2 goalCenter = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
                float length = Projectile.timeLeft * Projectile.ai[0] / 300f;
                Projectile.velocity = goalCenter + new Vector2(length, 0).RotatedBy(Projectile.ai[1]) - Projectile.Center;
            }

            Projectile.rotation += 0.1f * (Projectile.ai[1] > 0 ? 1 : -1);
        }

        private Vector2 GoalVelocityRadial(Vector2 goalPosition, Vector2 orbitPoint, float timeLeft, float maxSpeed = 24f)
        {
            float dRadial = (goalPosition - orbitPoint).Length() - (Projectile.Center - orbitPoint).Length();
            float dAngle = (goalPosition - orbitPoint).ToRotation() - (Projectile.Center - orbitPoint).ToRotation();
            while (dAngle > MathHelper.Pi)
            {
                dAngle -= MathHelper.TwoPi;
            }
            while (dAngle < -MathHelper.Pi)
            {
                dAngle += MathHelper.TwoPi;
            }

            Vector2 output = (new Vector2(dRadial, dAngle * (Projectile.Center - orbitPoint).Length()).RotatedBy((Projectile.Center - orbitPoint).ToRotation())) / timeLeft;

            if (output.Length() > maxSpeed)
            {
                output.Normalize();
                output *= maxSpeed;
            }

            return output;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, (TextureAssets.Projectile[Projectile.type].Value.Height * Projectile.frame) / Main.projFrames[Projectile.type], TextureAssets.Projectile[Projectile.type].Value.Width, TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]), Color.White, Projectile.rotation, new Vector2(TextureAssets.Projectile[Projectile.type].Value.Width / 2 + DrawOriginOffsetX, Projectile.height / 2), Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}