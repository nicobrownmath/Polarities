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
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.GameContent;
using ReLogic.Content;
using Terraria.Localization;
using Terraria.GameContent.Bestiary;
using Polarities.Biomes;
using Terraria.GameContent.ItemDropRules;
using Polarities.Effects;
using Polarities.Items.Placeable.Relics;
using Polarities.Items.Consumables.TreasureBags;
using Polarities.Items.Materials;
using Polarities.Items.Weapons.Summon.Minions;
using Polarities.Items.Hooks;
using Polarities.Items.Armor.Vanity;
using Polarities.Items.Weapons.Ranged;
using MultiHitboxNPCLibrary;

namespace Polarities.NPCs.Esophage
{
    [AutoloadBossHead]
    public class Esophage : ModNPC, IMultiHitboxSegmentUpdate
    {
        private int LEGCOUNT
        {
            get => (Main.expertMode ? 6 : 4) * (Main.getGoodWorld ? 2 : 1);
        }
        private float VELOCITYMULTIPLIER
        {
            get => Main.expertMode ? 30 : 60;
        }

        private float corruptAttackType
        {
            get => NPC.ai[0];
            set => NPC.ai[0] = value;
        }
        private float corruptAttackCooldown
        {
            get => NPC.ai[1];
            set => NPC.ai[1] = value;
        }

        private float crimsonAttackType
        {
            get => NPC.ai[2];
            set => NPC.ai[2] = value;
        }
        private float crimsonAttackCooldown
        {
            get => NPC.ai[3];
            set => NPC.ai[3] = value;
        }

        private int corruptAttackPrevious;
        private int crimsonAttackPrevious;

        private bool doCorruptAttacks;
        private bool doCrimsonAttacks;

        private int[] legs;

        private float angleGoal;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;

            //group with other bosses
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
            {
                SpecificallyImmuneTo = new int[] {
                    BuffID.Confused
                }
            };
            NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);

            Main.npcFrameCount[NPC.type] = 6;
            NPCID.Sets.TrailCacheLength[NPC.type] = 5;
            NPCID.Sets.TrailingMode[NPC.type] = 0;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                //spawn conditions
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCorruption,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson,
                new BestiaryPortraitBackgroundProviderPreferenceInfoElement(GetInstance<WorldEvilInvasion>().ModBiomeBestiaryInfoElement),
				//flavor text
				this.TranslatedBestiaryEntry()
            });
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 60;
            NPC.height = 60;

            NPC.defense = 30;
            NPC.damage = 60;
            NPC.lifeMax = Main.masterMode ? 63000 / 3 : Main.expertMode ? 50000 / 2 : 40000;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 5, 0, 0);
            NPC.npcSlots = 15f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            Music = MusicID.Boss2;
            if (ModLoader.HasMod("PolaritiesMusic"))
            {
                Music = MusicLoader.GetMusicSlot(ModLoader.GetMod("PolaritiesMusic"), "Sounds/Music/Esophage");
            }

            legs = new int[12];

            if (Main.getGoodWorld)
            {
                NPC.scale = 0.5f;
            }

            SpawnModBiomes = new int[1] { GetInstance<WorldEvilInvasion>().Type };
        }

        public static void SpawnOn(Player player)
        {
            float r = (float)Main.rand.NextDouble();
            float theta = Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4) + Main.rand.Next(2) * MathHelper.Pi;

            int boss = NPC.NewNPC(NPC.GetBossSpawnSource(player.whoAmI), (int)(player.Center.X + (500 * r + 1000) * (float)Math.Cos(theta)), (int)(player.Center.Y - (500 * r + 1000) * (float)Math.Sin(theta)), NPCType<NPCs.Esophage.Esophage>());
            Main.NewText(Language.GetTextValue("Announcement.HasAwoken", Main.npc[boss].TypeName), 171, 64, 255);

            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/NPC_Death_10")
            {
                Volume = 1.2f,
                Pitch = -0.5f
            }, player.position);
            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Roar_0")
            {
                Volume = 1.2f,
                Pitch = -0.5f
            }, player.position);
        }

        const int numSegments = 2;

        public override void AI()
        {
            if (Main.getGoodWorld)
            {
                //for the worthy evil spreading
                if (WorldGen.AllowedToSpreadInfections && !PolaritiesSystem.disabledEvilSpread)
                {
                    Point p = (NPC.Center + new Vector2(Main.rand.NextFloat(800), 0).RotatedByRandom(MathHelper.TwoPi)).ToTileCoordinates();
                    WorldGen.Convert(p.X, p.Y, Main.rand.Next(new int[] { 1, 4 }), 0);
                }
            }

            if (NPC.localAI[0] == 0)
            {
                NPC.localAI[0] = 1;
                for (int i = 0; i < LEGCOUNT; i++)
                {
                    legs[i] = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y + 100, NPCType<EsophageClaw>(), ai0: NPC.whoAmI, ai1: (240 / LEGCOUNT) * i, ai3: i % 2);
                    Main.npc[legs[i]].velocity.X = 2 * i - (LEGCOUNT - 1);
                    Main.npc[legs[i]].velocity.Y = 1;
                }

                if (Main.netMode != 1)
                {
                    doCorruptAttacks = Main.rand.NextBool();
                    doCrimsonAttacks = !doCorruptAttacks;
                }
                NPC.netUpdate = true;
            }

            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
                if (player.dead)
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
                {
                    targetPosition += Main.npc[legs[i]].Center + new Vector2(0, -180);
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

            Vector2 targetVelocity = (targetPosition - NPC.Center) / VELOCITYMULTIPLIER;
            NPC.velocity += (targetVelocity - NPC.velocity) / 60;

            //default angle goal position
            angleGoal = 0;

            if (NPC.life < NPC.lifeMax * (2f / 3f))
            {
                doCorruptAttacks = true;
                doCrimsonAttacks = true;
            }

            bool capsidOpen = false;
            bool jawsOpen = false;
            bool blinking = false;

            //attacking AI: Separate crimson and corruption attacks
            bool crimsonAttacksStarting = doCrimsonAttacks;
            if (doCorruptAttacks)
            {
                switch (corruptAttackType)
                {
                    case 0:
                        //attack switching stuff
                        corruptAttackCooldown++;

                        int maxAttackCooldown = doCrimsonAttacks ? 80 : 40;

                        if (Main.getGoodWorld) maxAttackCooldown = 0;

                        if (corruptAttackCooldown >= maxAttackCooldown)
                        {
                            if (doCrimsonAttacks)
                            {
                                corruptAttackCooldown = 0;

                                if (corruptAttackPrevious == 0)
                                {
                                    corruptAttackType = Main.rand.Next(1, 4);
                                    corruptAttackPrevious = (int)corruptAttackType;
                                }
                                else
                                {
                                    corruptAttackType = (Main.rand.Next(0, 2) + corruptAttackPrevious) % 3 + 1;
                                    corruptAttackPrevious = (int)corruptAttackType;
                                }
                            }
                            else
                            {
                                doCorruptAttacks = false;
                                doCrimsonAttacks = true;
                                corruptAttackCooldown = 0;
                                crimsonAttackCooldown = 0;
                                corruptAttackType = 0;

                                if (crimsonAttackPrevious == 0)
                                {
                                    if (Math.Abs((player.Center - NPC.Center).RotatedBy(MathHelper.PiOver2).ToRotation()) < MathHelper.Pi / 6)
                                    {
                                        //don't do ichor rain if player is above
                                        crimsonAttackType = Main.rand.Next(2, 4);
                                    }
                                    else
                                    {
                                        crimsonAttackType = Main.rand.Next(1, 4);
                                    }

                                    crimsonAttackPrevious = (int)crimsonAttackType;
                                }
                                else
                                {
                                    if (Math.Abs((player.Center - NPC.Center).RotatedBy(MathHelper.PiOver2).ToRotation()) < MathHelper.Pi / 6)
                                    {
                                        //don't do ichor rain if player is above
                                        if (crimsonAttackPrevious == 1)
                                            crimsonAttackType = Main.rand.Next(2, 4);
                                        else
                                            crimsonAttackType = 5 - crimsonAttackPrevious;
                                    }
                                    else
                                    {
                                        crimsonAttackType = (Main.rand.Next(0, 2) + crimsonAttackPrevious) % 3 + 1;
                                    }

                                    crimsonAttackPrevious = (int)crimsonAttackType;
                                }
                            }
                        }
                        break;
                    case 1:
                        //fireballs
                        capsidOpen = true;

                        if (corruptAttackCooldown % 45 == 44 && Main.netMode != 1)
                        {
                            Vector2 displacement = new Vector2(128f, 0).RotatedBy(NPC.rotation - MathHelper.PiOver2) * NPC.scale;
                            Vector2 speed = new Vector2(8f, 0).RotatedBy(NPC.rotation - MathHelper.PiOver2);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + displacement, speed, ProjectileType<EsophageFireball>(), 27, 2f, Main.myPlayer, player.whoAmI);
                        }

                        corruptAttackCooldown++;
                        if (corruptAttackCooldown == 180)
                        {
                            corruptAttackCooldown = 0;
                            corruptAttackType = 0;
                        }
                        break;
                    case 2:
                        //capsid attack
                        capsidOpen = true;

                        if (corruptAttackCooldown == 120)
                        {
                            SoundEngine.PlaySound(SoundID.Item117, NPC.Center);

                            if (Main.netMode != 1)
                            {
                                Vector2 displacement = new Vector2(91f, 0).RotatedBy(NPC.rotation - MathHelper.PiOver2) * NPC.scale;

                                int numRealPlates = 10;
                                int numTotalPlates = 20;

                                for (int i = 0; i < numTotalPlates; i++)
                                {
                                    Vector2 speed = new Vector2(8f, 0).RotatedBy(NPC.rotation - MathHelper.PiOver2); // i * MathHelper.TwoPi / numTotalPlates + MathHelper.Pi);

                                    if (Main.rand.Next(20 - i) < numRealPlates)
                                    {
                                        numRealPlates--;
                                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + displacement, speed, ProjectileType<EsophageCapsidPlate>(), 30, 4f, Main.myPlayer, player.whoAmI, i * MathHelper.TwoPi / numTotalPlates);
                                    }
                                    else
                                    {
                                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + displacement, speed, ProjectileType<EsophageCapsidPlate>(), 0, 4f, Main.myPlayer, player.whoAmI, i * MathHelper.TwoPi / numTotalPlates);
                                    }
                                }
                            }
                        }

                        corruptAttackCooldown++;
                        if (corruptAttackCooldown == 240)
                        {
                            corruptAttackCooldown = 0;
                            corruptAttackType = 0;
                        }
                        break;
                    case 3:
                        //tendril spikes

                        if (corruptAttackCooldown == 60)
                        {
                            SoundEngine.PlaySound(SoundID.Item8, NPC.Center);

                            if (Main.netMode != 1)
                            {
                                for (int i = 0; i < 6; i++)
                                {
                                    Vector2 displacement = new Vector2(91f, 0).RotatedBy(NPC.rotation - MathHelper.PiOver2) * NPC.scale;
                                    Vector2 speed = new Vector2(18f, 0).RotatedBy((player.Center - (NPC.Center + displacement)).ToRotation() + (i + 0.5f) * MathHelper.TwoPi / 6);

                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + displacement, speed, ProjectileType<EsophageTendril>(), 27, 2f, Main.myPlayer, player.whoAmI);
                                }
                            }
                        }

                        corruptAttackCooldown++;
                        if (corruptAttackCooldown == 120)
                        {
                            corruptAttackCooldown = 0;
                            corruptAttackType = 0;
                        }
                        break;
                }
            }
            if (crimsonAttacksStarting)
            {
                switch (crimsonAttackType)
                {
                    case 0:
                        //attack switching stuff
                        crimsonAttackCooldown++;

                        int maxAttackCooldown = doCorruptAttacks ? 80 : 40;

                        if (Main.getGoodWorld) maxAttackCooldown = 0;

                        if (crimsonAttackCooldown >= maxAttackCooldown)
                        {
                            if (doCorruptAttacks)
                            {
                                crimsonAttackCooldown = 0;

                                if (crimsonAttackPrevious == 0)
                                {
                                    if (Math.Abs((player.Center - NPC.Center).RotatedBy(MathHelper.PiOver2).ToRotation()) < MathHelper.Pi / 6)
                                    {
                                        //don't do ichor rain if player is above
                                        crimsonAttackType = Main.rand.Next(2, 4);
                                    }
                                    else
                                    {
                                        crimsonAttackType = Main.rand.Next(1, 4);
                                    }

                                    crimsonAttackPrevious = (int)crimsonAttackType;
                                }
                                else
                                {
                                    if (Math.Abs((player.Center - NPC.Center).RotatedBy(MathHelper.PiOver2).ToRotation()) < MathHelper.Pi / 6)
                                    {
                                        //don't do ichor rain if player is above
                                        if (crimsonAttackPrevious == 1)
                                            crimsonAttackType = Main.rand.Next(2, 4);
                                        else
                                            crimsonAttackType = 5 - crimsonAttackPrevious;
                                    }
                                    else
                                    {
                                        crimsonAttackType = (Main.rand.Next(0, 2) + crimsonAttackPrevious) % 3 + 1;
                                    }

                                    crimsonAttackPrevious = (int)crimsonAttackType;
                                }
                            }
                            else
                            {
                                doCrimsonAttacks = false;
                                doCorruptAttacks = true;
                                crimsonAttackCooldown = 0;
                                corruptAttackCooldown = 0;
                                crimsonAttackType = 0;

                                if (corruptAttackPrevious == 0)
                                {
                                    corruptAttackType = Main.rand.Next(1, 4);
                                    corruptAttackPrevious = (int)corruptAttackType;
                                }
                                else
                                {
                                    corruptAttackType = (Main.rand.Next(0, 2) + corruptAttackPrevious) % 3 + 1;
                                    corruptAttackPrevious = (int)corruptAttackType;
                                }
                            }
                        }
                        break;
                    case 1:
                        //ichor rain
                        jawsOpen = true;
                        blinking = true;

                        int setupTime = 28;

                        int ichorSprayPeriod = 4;
                        float ichorSprayVelocity = 16f;

                        if (crimsonAttackCooldown < setupTime)
                        {
                            NPC.direction = player.Center.X > NPC.Center.X ? 1 : -1;
                        }
                        angleGoal = MathHelper.Pi + ((NPC.direction - NPC.direction * (crimsonAttackCooldown - setupTime) / 240f) / 3f);

                        if (crimsonAttackCooldown >= setupTime && crimsonAttackCooldown % ichorSprayPeriod == ichorSprayPeriod - 1 && Main.netMode != 1)
                        {
                            Vector2 speed = new Vector2(ichorSprayVelocity, 0).RotatedBy(angleGoal + MathHelper.PiOver2);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + speed.SafeNormalize(Vector2.Zero) * 24f * NPC.scale, speed, ProjectileType<EsophageIchorSpray>(), 16, 3, Main.myPlayer);
                        }
                        else if (crimsonAttackCooldown < setupTime)
                        {
                            Vector2 displacement = new Vector2(24f, 0).RotatedBy(NPC.rotation + MathHelper.PiOver2);
                            Vector2 speed = new Vector2(8f, 0).RotatedBy(angleGoal + MathHelper.PiOver2);
                            Dust.NewDustPerfect(NPC.Center + displacement, 170, speed.RotatedByRandom(0.05f), Scale: 1f);
                        }

                        crimsonAttackCooldown++;
                        if (crimsonAttackCooldown == 240 + setupTime)
                        {
                            crimsonAttackCooldown = 0;
                            crimsonAttackType = 0;
                        }
                        break;
                    case 2:
                        //scythes everywhere

                        angleGoal = (player.Center - NPC.Center).ToRotation() - MathHelper.PiOver2;

                        if (crimsonAttackCooldown % 36 == 0)
                        {
                            scytheLeftSwinging = true;
                        }
                        else if (crimsonAttackCooldown % 36 == 18)
                        {
                            scytheRightSwinging = true;
                        }

                        if (crimsonAttackCooldown % 18 == 17)
                        {
                            SoundEngine.PlaySound(SoundID.Item71, NPC.Center);

                            if (Main.netMode != 1)
                            {
                                if (crimsonAttackCooldown % 36 == 17)
                                {
                                    Vector2 shotPosition = NPC.Center + new Vector2(0, 32).RotatedBy(angleGoal + MathHelper.PiOver2) * NPC.scale;
                                    for (int i = -5; i <= 5; i++)
                                    {
                                        Vector2 speed = new Vector2(0.1f, 0).RotatedBy((player.Center - shotPosition).ToRotation() + i * MathHelper.PiOver2 / 75f);
                                        Vector2 trueShotPosition = shotPosition + new Vector2(32, 0).RotatedBy(speed.ToRotation()) * NPC.scale;

                                        Projectile.NewProjectile(NPC.GetSource_FromAI(), trueShotPosition, speed, ProjectileType<EsophageSlash>(), 27, 2f, Main.myPlayer);
                                    }
                                }
                                else
                                {
                                    Vector2 shotPosition = NPC.Center + new Vector2(0, -32).RotatedBy(angleGoal + MathHelper.PiOver2) * NPC.scale;
                                    for (int i = -5; i <= 5; i++)
                                    {
                                        Vector2 speed = new Vector2(0.1f, 0).RotatedBy((player.Center - shotPosition).ToRotation() + i * MathHelper.PiOver2 / 75f);
                                        Vector2 trueShotPosition = shotPosition + new Vector2(32, 0).RotatedBy(speed.ToRotation()) * NPC.scale;

                                        Projectile.NewProjectile(NPC.GetSource_FromAI(), trueShotPosition, speed, ProjectileType<EsophageSlash>(), 27, 2f, Main.myPlayer);
                                    }
                                }
                            }
                        }

                        crimsonAttackCooldown++;
                        if (crimsonAttackCooldown == 72)
                        {
                            crimsonAttackCooldown = 0;
                            crimsonAttackType = 0;
                        }
                        break;
                    case 3:
                        //flesh chunks

                        if (crimsonAttackCooldown < 180)
                        {
                            jawsOpen = true;
                        }

                        if (crimsonAttackCooldown % 30 == 29 && crimsonAttackCooldown < 180)
                        {
                            if (Main.netMode != 1)
                            {
                                Vector2 displacement = new Vector2(24f, 0).RotatedBy(NPC.rotation + MathHelper.PiOver2) * NPC.scale;
                                Vector2 speed = new Vector2(NPC.velocity.X, 2f);

                                float direction = (crimsonAttackCooldown % 90 == 44) ? 1f : -1f;

                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + displacement, speed, ProjectileType<EsophageFleshChunk>(), 27, 2f, Main.myPlayer, ai0: direction, ai1: player.whoAmI);
                            }
                        }

                        crimsonAttackCooldown++;
                        if (crimsonAttackCooldown == 240)
                        {
                            crimsonAttackCooldown = 0;
                            crimsonAttackType = 0;
                        }
                        break;
                }
            }

            //rotation!
            float maxTurn = 0.1f;

            float angleOffset = angleGoal - NPC.rotation;
            while (angleOffset > MathHelper.Pi)
            {
                angleOffset -= MathHelper.TwoPi;
            }
            while (angleOffset < -MathHelper.Pi)
            {
                angleOffset += MathHelper.TwoPi;
            }

            if (Math.Abs(angleOffset) < maxTurn) { NPC.rotation = angleGoal; }
            else if (angleOffset > 0)
            {
                NPC.rotation += maxTurn;
            }
            else
            {
                NPC.rotation -= maxTurn;
            }

            //Animations!
            NPC.frameCounter++;
            if (NPC.frameCounter == 8)
            {
                NPC.frameCounter = 0;

                //capsid animations
                if (capsidOpen)
                {
                    if (capsidFrameY < 182 * 2)
                    {
                        capsidFrameY += 182;
                    }
                }
                else
                {
                    if (capsidFrameY > 0)
                    {
                        capsidFrameY -= 182;
                    }
                }
                capsidFrameX += 136;
                if (capsidFrameX == 136 * 4)
                {
                    capsidFrameX = 0;
                }

                //mouth animations
                if (jawsOpen)
                {
                    if (headFrameY < 182 * 2)
                    {
                        headFrameY += 182;
                    }
                }
                else
                {
                    if (headFrameY > 0)
                    {
                        headFrameY -= 182;
                    }
                }
                headFrameX += 136;
                if (headFrameX == 136 * 4)
                {
                    headFrameX = 0;
                }

                //blinking
                if (blinking || eyesFrameY != 0)
                {
                    eyesFrameY += 182;
                    if (eyesFrameY == 182 * 4)
                    {
                        eyesFrameY = 0;
                    }
                }

                //scythe swinging
                if (!scytheLeftSwinging)
                {
                    if (jawsOpen)
                    {
                        if (scytheLeftFrameY < 182 * 2)
                        {
                            scytheLeftFrameY += 182;
                        }
                    }
                    else
                    {
                        if (scytheLeftFrameY > 0)
                        {
                            scytheLeftFrameY -= 182;
                        }
                    }
                }
                if (!scytheRightSwinging)
                {
                    if (jawsOpen)
                    {
                        if (scytheRightFrameY < 182 * 2)
                        {
                            scytheRightFrameY += 182;
                        }
                    }
                    else
                    {
                        if (scytheRightFrameY > 0)
                        {
                            scytheRightFrameY -= 182;
                        }
                    }
                }
            }
            //more scythe swinging
            scytheFrameCounter++;
            if (scytheFrameCounter == 6)
            {
                scytheFrameCounter = 0;
                if (scytheLeftSwinging)
                {
                    scytheLeftFrameY += 182;
                    if (scytheLeftFrameY == 182 * 4)
                    {
                        scytheLeftFrameY = 0;
                        scytheLeftSwinging = false;
                    }
                }
                if (scytheRightSwinging)
                {
                    scytheRightFrameY += 182;
                    if (scytheRightFrameY == 182 * 4)
                    {
                        scytheRightFrameY = 0;
                        scytheRightSwinging = false;
                    }
                }
            }

            //position hitbox segments
            List<RectangleHitboxData> hitboxes = new List<RectangleHitboxData>();
            for (int h = 0; h < numSegments; h++)
            {
                Vector2 spot = NPC.Center + NPC.velocity + new Vector2(0, -h * (136 / numSegments)).RotatedBy(NPC.rotation) * NPC.scale;
                hitboxes.Add(new RectangleHitboxData(new Rectangle((int)spot.X - NPC.width / 2, (int)spot.Y - NPC.height / 2, NPC.width, NPC.height)));
            }
            NPC.GetGlobalNPC<MultiHitboxNPC>().AssignHitboxFrom(hitboxes);
        }

        public override void FindFrame(int frameHeight)
        {
            if (NPC.IsABestiaryIconDummy)
            {
                NPC.frameCounter++;
                if (NPC.frameCounter == 8)
                {
                    NPC.frameCounter = 0;

                    capsidFrameX += 136;
                    if (capsidFrameX == 136 * 4)
                    {
                        capsidFrameX = 0;
                    }
                    headFrameX += 136;
                    if (headFrameX == 136 * 4)
                    {
                        headFrameX = 0;
                    }
                }
            }
        }

        public void MultiHitboxSegmentUpdate(NPC npc, RectangleHitbox mostRecentHitbox)
        {
            if (mostRecentHitbox.index == 0)
            {
                npc.defense = 30;
                npc.HitSound = SoundID.NPCHit1;
            }
            else
            {
                npc.defense = 60;
                npc.HitSound = SoundID.NPCHit2;
            }
        }

        private bool scytheLeftSwinging;
        private bool scytheRightSwinging;
        private int scytheFrameCounter;

        private int capsidFrameX;
        private int capsidFrameY;
        private int scytheLeftFrameY;
        private int scytheRightFrameY;
        private int headFrameX;
        private int headFrameY;
        private int eyesFrameY;

        public static Asset<Texture2D> CapsidTexture;
        public static Asset<Texture2D> ScytheTexture;
        public static Asset<Texture2D> EyesTexture;

        public override void Load()
        {
            CapsidTexture = Request<Texture2D>(Texture + "_Capsid");
            ScytheTexture = Request<Texture2D>(Texture + "_Scythe");
            EyesTexture = Request<Texture2D>(Texture + "_Eyes");

            /*IL.Terraria.Main.UpdateMenu += Main_UpdateMenu;
		}

		private void Main_UpdateMenu(MonoMod.Cil.ILContext il)
		{
			MonoMod.Cil.ILCursor c = new MonoMod.Cil.ILCursor(il);

			c.EmitDelegate<Action>(() =>
			{
				if (!(bool)(typeof(ModLoader).GetField("isLoading", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null)))
				{
					String filePath = Main.SavePath + Path.DirectorySeparatorChar + "Esophage.png";

					if (!File.Exists(filePath))
					{
						Main.spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);

						var capture = new RenderTarget2D(Main.spriteBatch.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, Main.spriteBatch.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

						Main.spriteBatch.GraphicsDevice.SetRenderTarget(capture);
						Main.spriteBatch.GraphicsDevice.Clear(Color.Transparent);

						NPC me = new NPC();
						me.SetDefaults(NPCType<Esophage>());
						me.IsABestiaryIconDummy = true;
						me.Center = Vector2.Zero;

						Main.instance.DrawNPCDirect(Main.spriteBatch, me, false, -capture.Size() / 2);

						Main.spriteBatch.End();
						Main.spriteBatch.GraphicsDevice.SetRenderTarget(null);

						var stream = File.Create(filePath);
						capture.SaveAsPng(stream, capture.Width, capture.Height);
						stream.Dispose();
						capture.Dispose();
					}
				}
			});*/
        }

        public override void Unload()
        {
            CapsidTexture = null;
            ScytheTexture = null;
            EyesTexture = null;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
            {
                EsophageClaw legDummy = GetModNPC(NPCType<EsophageClaw>()) as EsophageClaw;
                for (int i = -LEGCOUNT / 2; i <= LEGCOUNT / 2; i++)
                {
                    if (i != 0)
                        legDummy.DrawAt(NPC, NPC.Center + new Vector2((float)i / LEGCOUNT * 300, 240), spriteBatch, screenPos, drawColor, true);
                }
            }

            spriteBatch.Draw(CapsidTexture.Value, NPC.Center - screenPos, new Rectangle(capsidFrameX, capsidFrameY, 136, 182), NPC.GetNPCColorTintedByBuffs(drawColor), NPC.rotation, new Vector2(68, 136), NPC.scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(ScytheTexture.Value, NPC.Center - screenPos, new Rectangle(0, scytheLeftFrameY, 136, 182), NPC.GetNPCColorTintedByBuffs(drawColor), NPC.rotation, new Vector2(68, 136), NPC.scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(ScytheTexture.Value, NPC.Center - screenPos, new Rectangle(0, scytheRightFrameY, 136, 182), NPC.GetNPCColorTintedByBuffs(drawColor), NPC.rotation, new Vector2(68, 136), NPC.scale, SpriteEffects.FlipHorizontally, 0f);
            spriteBatch.Draw(TextureAssets.Npc[Type].Value, NPC.Center - screenPos, new Rectangle(headFrameX, headFrameY, 136, 182), NPC.GetNPCColorTintedByBuffs(drawColor), NPC.rotation, new Vector2(68, 136), NPC.scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(EyesTexture.Value, NPC.Center - screenPos, new Rectangle(0, eyesFrameY, 136, 182), NPC.GetNPCColorTintedByBuffs(Color.White), NPC.rotation, new Vector2(68, 136), NPC.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }

        public override bool CheckDead()
        {
            if (!PolaritiesSystem.downedEsophage)
            {
                NPC.SetEventFlagCleared(ref PolaritiesSystem.downedEsophage, -1);
            }

            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/NPC_Death_10")
            {
                Volume = 1.2f,
                Pitch = -0.5f
            }, NPC.Center);
            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Roar_0")
            {
                Volume = 1.2f,
                Pitch = -0.5f
            }, NPC.Center);

            Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity, Mod.Find<ModGore>("EsophageHeadGore").Type);
            Gore.NewGore(NPC.GetSource_Death(), NPC.Center + new Vector2(0, -91f).RotatedBy(NPC.rotation), NPC.velocity, Mod.Find<ModGore>("EsophageCapsidGore").Type);
            return true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(new FlawlessOrRandomDropRule(ItemType<EsophageTrophy>(), 10));
            npcLoot.Add(ItemDropRule.BossBag(ItemType<EsophageBag>()));
            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ItemType<EsophageRelic>()));
            //TODO: npcLoot.Add(ModUtils.MasterModeDropOnAllPlayersOrFlawless(ItemType<EsophagePetItem>(), 4));

            //normal mode loot
            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemType<EsophageMask>(), 7));
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemType<EvilDNA>(), 1, 3, 4));
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.SoulofNight, 1, 12, 18));
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemType<StrangeSamples>(), 2));
            notExpertRule.OnSuccess(new OneFromOptionsWithCountsNotScaledWithLuckDropRule(1, 1,
                (ItemID.Ichor, 20, 40),
                (ItemID.CursedFlame, 20, 40)));
            //TODO: Should probably drop evil seeds still
            notExpertRule.OnSuccess(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemType<EsophageousStaff>(), ItemType<PhagefootHook>()));
            npcLoot.Add(notExpertRule);

            npcLoot.Add(ItemDropRule.ByCondition(new FlawlessDropCondition(), ItemType<Contagun>()));
        }
    }

    public class EsophageClaw : ModNPC
    {
        public override void SetStaticDefaults()
        {
            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                //don't show up in bestiary
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifier);
        }

        private int jumpCooldown;

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 12;
            NPC.height = 12;
            DrawOffsetY = 6;

            NPC.defense = 5;
            NPC.damage = 30;
            NPC.lifeMax = 500;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 0f;
            NPC.lavaImmune = true;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.dontTakeDamage = true;
        }

        public override void AI()
        {
            NPC owner = Main.npc[(int)NPC.ai[0]];
            if (!owner.active)
            {
                Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity, Mod.Find<ModGore>("EsophageClawGore").Type);

                NPC.active = false;
                return;
            }

            if (Main.player[owner.target].dead)
            {
                if (NPC.timeLeft > 10)
                {
                    NPC.timeLeft = 10;
                }
                NPC.noTileCollide = true;
                NPC.velocity.Y += 0.1f;
                return;
            }

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

            if (NPC.velocity.Y != 0 && NPC.Center.Y < Main.player[owner.target].Center.Y)
            {
                NPC.noTileCollide = true;
            }

            if (NPC.velocity.Y == 0 || (NPC.Center - owner.Center).Length() > 480)
            {
                if (NPC.velocity.Y == 0) { NPC.velocity.X = 0; }

                if (jumpCooldown == 0 && (NPC.ai[1] == 0 || ((NPC.Center - owner.Center).Length() > 480 && NPC.Center.Y > Main.player[owner.target].Center.Y + 256)))
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

                    NPC.velocity = v * (new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta))).RotatedByRandom(0.05f);
                    jumpCooldown = 120;

                    if ((NPC.Center - owner.Center).Length() > 480)
                    {
                        owner.velocity -= (NPC.velocity - NPC.oldVelocity) / 9;
                    }

                    NPC.ai[2] = 30;
                    NPC.noTileCollide = true;
                }
            }

            if ((NPC.Center + new Vector2(0, -60) - owner.Center).Length() > 480)
            {
                NPC.noTileCollide = true;
                if (Main.netMode != 1)
                {
                    NPC.velocity += (owner.Center - NPC.Center).SafeNormalize(new Vector2(0, -1)) * 4 + new Vector2(1, 0).RotatedByRandom(Math.PI * 2);
                    owner.velocity -= ((owner.Center - NPC.Center).SafeNormalize(new Vector2(0, -1)) * 4 + new Vector2(1, 0).RotatedByRandom(Math.PI * 2)) / 6;
                }
                NPC.netUpdate = true;
            }

            if (jumpCooldown > 0)
            {
                jumpCooldown--;
            }
        }

        public static Asset<Texture2D> ChainTexture;

        public override void Load()
        {
            ChainTexture = Request<Texture2D>(Texture + "_Chain");
        }

        public override void Unload()
        {
            ChainTexture = null;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            NPC owner = Main.npc[(int)NPC.ai[0]];

            DrawAt(owner, NPC.Center, spriteBatch, screenPos, drawColor);

            return false;
        }

        public void DrawAt(NPC owner, Vector2 center, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor, bool bestiaryDummy = false)
        {
            Vector2[] points = { center + new Vector2(0, -8), center + (owner.Center - center) * 0.34f - new Vector2(0, 200), center + (owner.Center - center) * 0.66f, owner.Center };

            for (int i = 0; i < points.Length - 1; i++)
            {
                DrawChain(owner, points[i], points[i + 1], spriteBatch, screenPos, drawColor, bestiaryDummy);
            }

            float num246 = Main.NPCAddHeight(NPC);
            SpriteEffects spriteEffects = (SpriteEffects)0;
            if (NPC.spriteDirection == 1)
            {
                spriteEffects = (SpriteEffects)1;
            }
            Vector2 halfSize = new Vector2((float)(TextureAssets.Npc[Type].Width() / 2), (float)(TextureAssets.Npc[Type].Height() / Main.npcFrameCount[NPC.type] / 2));

            Color color = owner.GetNPCColorTintedByBuffs(bestiaryDummy ? Color.White : Lighting.GetColor(NPC.Center.ToTileCoordinates()));

            spriteBatch.Draw(TextureAssets.Npc[Type].Value, center + new Vector2(0, NPC.height / 2) - screenPos + new Vector2((float)(-TextureAssets.Npc[Type].Width()) * NPC.scale / 2f + halfSize.X * NPC.scale, (float)(-TextureAssets.Npc[Type].Height()) * NPC.scale / (float)Main.npcFrameCount[Type] + 4f + halfSize.Y * NPC.scale + num246 + NPC.gfxOffY), TextureAssets.Npc[Type].Frame(), color, NPC.rotation, halfSize, NPC.scale, spriteEffects, 0f);
        }

        public void DrawChain(NPC owner, Vector2 startPoint, Vector2 endPoint, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor, bool bestiaryDummy)
        {
            Texture2D chainTexture = ChainTexture.Value;
            Rectangle chainFrame = ChainTexture.Frame();
            Vector2 chainOrigin = chainFrame.Size() / 2;

            int stepSize = chainFrame.Width;

            int parity = 1;

            for (int i = 0; i < (endPoint - startPoint).Length() / stepSize; i++)
            {
                parity *= -1;

                Vector2 drawingPos = startPoint + (endPoint - startPoint).SafeNormalize(Vector2.Zero) * i * stepSize;
                spriteBatch.Draw(chainTexture, drawingPos - screenPos, chainFrame, owner.GetNPCColorTintedByBuffs(bestiaryDummy ? Color.White : Lighting.GetColor(drawingPos.ToTileCoordinates())), (endPoint - startPoint).ToRotation(), chainOrigin, NPC.scale, NPC.spriteDirection * parity == -1 ? SpriteEffects.None : SpriteEffects.FlipVertically, 0f);
            }
        }

        public override bool CheckActive()
        {
            return !Main.npc[(int)NPC.ai[0]].active;
        }
    }

    public class EsophageIchorSpray : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Pixel";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 48;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 16;
            Projectile.height = 16;

            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = false;
            Projectile.timeLeft = 600;

            Projectile.extraUpdates = 2;
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

                SoundEngine.PlaySound(SoundID.Item17, Projectile.position);
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

            Projectile.velocity.Y += 0.075f;

            //adapted from vanilla golden shower dusts
            for (int num68 = 0; num68 < 3; num68++)
            {
                if (Main.rand.NextBool(3))
                {
                    float num69 = Projectile.velocity.X / 3f * (float)num68;
                    float num70 = Projectile.velocity.Y / 3f * (float)num68;
                    int num71 = 2;
                    int num72 = Dust.NewDust(new Vector2(Projectile.position.X + (float)num71, Projectile.position.Y + (float)num71), Projectile.width - num71 * 2, Projectile.height - num71 * 2, 170, 0f, 0f, 100);
                    Main.dust[num72].noGravity = true;
                    Dust dust79 = Main.dust[num72];
                    Dust dust195 = dust79;
                    dust195.velocity *= 0.1f;
                    dust79 = Main.dust[num72];
                    dust195 = dust79;
                    dust195.velocity += Projectile.velocity * 0.5f;
                    Main.dust[num72].position.X -= num69;
                    Main.dust[num72].position.Y -= num70;
                }
            }
            if (Main.rand.NextBool(24))
            {
                int num73 = 4;
                int num75 = Dust.NewDust(new Vector2(Projectile.position.X + (float)num73, Projectile.position.Y + (float)num73), Projectile.width - num73 * 2, Projectile.height - num73 * 2, 170, 0f, 0f, 100, default(Color), 0.5f);
                Dust dust78 = Main.dust[num75];
                Dust dust195 = dust78;
                dust195.velocity *= 0.25f;
                dust78 = Main.dust[num75];
                dust195 = dust78;
                dust195.velocity += Projectile.velocity * 0.5f;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Ichor, 60 * 15);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Textures.PixelTexture.Value;

            Color mainColor = Color.Gold * 0.8f;

            for (int k = 0; k < Projectile.oldPos.Length - 1; k++)
            {
                float amount = ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

                Color color = mainColor * (1 - Projectile.alpha / 255f);
                float scale = 2f * Projectile.scale * amount;

                float rotation = (Projectile.oldPos[k + 1] - Projectile.oldPos[k]).ToRotation();

                Main.EntitySpriteDraw(texture, Projectile.Center - (Projectile.position - Projectile.oldPos[k]) * 0.9f - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, rotation, new Vector2(0, 0.5f), new Vector2((Projectile.oldPos[k + 1] - Projectile.oldPos[k]).Length(), scale), SpriteEffects.None, 0);
            }

            return false;
        }
    }

    public class EsophageFireball : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CursedFlameHostile;

        private Vector2 goalPosition;

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 16;
            Projectile.height = 16;

            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = false;
            Projectile.timeLeft = 90;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(goalPosition);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            goalPosition = reader.ReadVector2();
        }

        public override void AI()
        {
            //vanilla AI for hostile cursed flames
            Vector2 position58 = new Vector2(Projectile.position.X + Projectile.velocity.X, Projectile.position.Y + Projectile.velocity.Y);
            int width31 = Projectile.width;
            int height31 = Projectile.height;
            float x13 = Projectile.velocity.X;
            float y9 = Projectile.velocity.Y;
            Color newColor4 = default(Color);
            int num2467 = Dust.NewDust(position58, width31, height31, 75, x13, y9, 100, newColor4, 3f * Projectile.scale);
            Main.dust[num2467].noGravity = true;

            Projectile.rotation += 0.3f * Projectile.direction;

            //custom stuff
            Player player = Main.player[(int)Projectile.ai[0]];

            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                SoundEngine.PlaySound(SoundID.Item20, Projectile.position);
            }

            float maxTime = Math.Min(60, 60 * 540f / Projectile.Distance(player.Center));

            if (Projectile.timeLeft > maxTime)
            {
                goalPosition = player.Center;
            }

            //custom stuff
            Vector2 goalVelocity = (goalPosition - Projectile.Center) / Projectile.timeLeft;
            Projectile.velocity += (goalVelocity - Projectile.velocity) / Projectile.timeLeft;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.CursedInferno, 60 * 4);
            Projectile.Kill();
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            if (Main.netMode != 1)
            {
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ProjectileType<CursedFlameExplosion>(), 27, 0f, Main.myPlayer);
                //for (int i = 0; i < 32; i++)
                //Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, new Vector2(Main.rand.NextFloat(15, 17), 0).RotatedBy(i * MathHelper.TwoPi / 32f).RotatedByRandom(MathHelper.TwoPi / 32f), ProjectileType<CursedFlameExplosionShot>(), 27, 0f, Main.myPlayer);
            }
        }
    }

    public class CursedFlameExplosion : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Glow256";

        public override void SetDefaults()
        {
            Projectile.width = 270;
            Projectile.height = 270;

            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 20;
        }

        public override void AI()
        {
            Vector2 oldCenter = Projectile.Center;
            Projectile.scale = 270 / 128f * Projectile.timeLeft / 20f;
            Projectile.width = (int)(128 * Projectile.scale);
            Projectile.height = (int)(128 * Projectile.scale);
            Projectile.Center = oldCenter;

            if (Projectile.timeLeft == 20)
            {
                for (int i = 0; i < Projectile.timeLeft; i++)
                {
                    Dust.NewDustPerfect(Projectile.Center, DustID.CursedTorch, new Vector2(Main.rand.NextFloat(Projectile.timeLeft / 2f), 0).RotatedByRandom(MathHelper.TwoPi), Scale: 3f);
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return CustomCollision.CheckAABBvDisc(targetHitbox, new Circle(Projectile.Center, Projectile.width / 2));
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.CursedInferno, 60 * 4);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> TextureAsset = Textures.Glow256;
            Main.EntitySpriteDraw(TextureAsset.Value, Projectile.Center - Main.screenPosition, TextureAsset.Frame(), new Color(64, 255, 64) * 0.9f, 0f, TextureAsset.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            DrawLayer.AddProjectile<DrawLayerAdditiveAfterProjectiles>(index);
        }
    }

    public class EsophageSlash : ModProjectile
    {
        public override string Texture => "Polarities/NPCs/Enemies/WorldEvilInvasion/CrimagoSlash";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("{$Mods.Polarities.ProjectileName.CrimagoSlash}");
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 20;
            Projectile.height = 20;
            DrawOffsetX = -6;

            DrawOriginOffsetY = 0;
            DrawOriginOffsetX = 0;
            Projectile.alpha = 0;
            Projectile.timeLeft = 1200;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Projectile.velocity *= 1.08f;
            if (Projectile.velocity.Length() > 40)
            {
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero);
                Projectile.velocity *= 40;
            }

            if (Projectile.timeLeft < 256 / 8)
            {
                Projectile.alpha += 8;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float trailLength = 12f;
            float trailSize = 4;
            for (int i = (int)trailLength - 1; i >= 0; i--)
            {
                Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Projectile.velocity * (i / trailLength) * trailSize - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), lightColor * (1 - Projectile.alpha / 256f) * (1 - i / trailLength), Projectile.rotation, TextureAssets.Projectile[Type].Size() / 2, Projectile.scale * (1 - i / trailLength), SpriteEffects.None, 0);
            }
            return false;
        }
    }

    public class EsophageCapsidPlate : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 30;
            Projectile.height = 30;
            DrawOffsetX = 0;
            DrawOriginOffsetY = -6;
            DrawOriginOffsetX = -6;
            Projectile.alpha = 0;

            Projectile.timeLeft = 405;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }

        public override void AI()
        {
            if (Projectile.damage > 0)
            {
                Projectile.frame = 0;
            }
            else
            {
                Projectile.frame = 1;

                Color lightColor = Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16);
                Projectile.alpha = 16 + 128 - (lightColor.R + lightColor.G + lightColor.B) / 6;
            }

            if (Projectile.timeLeft > 300)
            {
                Player player = Main.player[(int)Projectile.ai[0]];

                Projectile.rotation = (player.Center - Projectile.Center).ToRotation();

                Vector2 goalPosition = player.Center + new Vector2(300, 0).RotatedBy(Projectile.ai[1]);

                Vector2 goalVelocity = GoalVelocityRadial(goalPosition, player.Center, (Projectile.timeLeft - 300), 1000f);
                Projectile.velocity += (goalVelocity - Projectile.velocity) / (float)Math.Sqrt(Projectile.timeLeft - 300);
            }
            else if (Projectile.timeLeft == 300)
            {
                Player player = Main.player[(int)Projectile.ai[0]];

                Projectile.Center = player.Center + new Vector2(300, 0).RotatedBy(Projectile.ai[1]);
                Projectile.velocity = (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * (300f / 300f);

                Projectile.rotation = Projectile.velocity.ToRotation();
            }
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

        public override bool PreDraw(ref Color lightColor)
        {
            lightColor = Lighting.GetColor(Projectile.Center.ToTileCoordinates());

            return true;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }

        public override void Kill(int timeLeft)
        {
            Dust.NewDustPerfect(Projectile.Center + new Vector2(Projectile.width / 2, 0).RotatedBy(Projectile.rotation), 14, Vector2.Zero, Scale: 1.5f).noGravity = true;
        }
    }

    public class EsophageTendril : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 30;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 18;
            Projectile.height = 18;
            DrawOffsetX = 0;
            DrawOriginOffsetY = 0;
            DrawOriginOffsetX = 0;

            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }

        public override void AI()
        {
            if (Projectile.timeLeft > 30)
            {
                Player player = Main.player[(int)Projectile.ai[0]];

                int age = 600 - Projectile.timeLeft;
                float homingFactor = 1.5f / (float)Math.Max(age / 1.95f, 48f - age / 1.95f);

                float velocityFactor = Math.Min(1 + age / 4f, 18f);

                Vector2 goalVelocity = (player.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * velocityFactor;
                Projectile.velocity += (goalVelocity - Projectile.velocity) * homingFactor;
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * velocityFactor;

                Projectile.rotation = Projectile.velocity.ToRotation();

                /*if (Main.netMode != 1)
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ProjectileType<EsophageTendrilTrail>(), 27, 2f, Main.myPlayer, Projectile.velocity.ToRotation());*/
            }
            else
            {
                Projectile.velocity = Vector2.Zero;

                Projectile.alpha = 255 - (255 * Projectile.timeLeft / 30);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (int i = 0; i < 10; i++)
            {
                Rectangle segmentHitbox = new Rectangle((int)Projectile.oldPos[i].X, (int)Projectile.oldPos[i].Y, Projectile.width, Projectile.height);
                if (segmentHitbox.Intersects(targetHitbox))
                {
                    return true;
                }
            }
            return false;
        }

        public static Asset<Texture2D> TrailTexture;

        public override void Load()
        {
            TrailTexture = Request<Texture2D>(Texture + "_Trail");
        }

        public override void Unload()
        {
            TrailTexture = null;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                lightColor = Lighting.GetColor((Projectile.oldPos[i] - Projectile.position + Projectile.Center).ToTileCoordinates());

                Main.EntitySpriteDraw(TrailTexture.Value, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, TrailTexture.Frame(), lightColor * (1 - (float)i / Projectile.oldPos.Length) * 0.4f, Projectile.oldRot[i], TrailTexture.Size() / 2, 1.33f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(TrailTexture.Value, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, TrailTexture.Frame(), lightColor * (1 - (float)i / Projectile.oldPos.Length), Projectile.oldRot[i], TrailTexture.Size() / 2, 1f, SpriteEffects.None, 0);
            }

            lightColor = Lighting.GetColor(Projectile.Center.ToTileCoordinates());

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), lightColor * 0.4f, Projectile.rotation, new Vector2(texture.Width / 2, texture.Height / Main.projFrames[Projectile.type] / 2), 1.33f, SpriteEffects.None, 0);

            return true;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }
    }

    public class EsophageFleshChunk : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 42;
            Projectile.height = 42;
            DrawOffsetX = -2;
            DrawOriginOffsetY = -2;
            DrawOriginOffsetX = 0;
            Projectile.alpha = 0;

            Projectile.timeLeft = 240;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;

                SoundEngine.PlaySound(SoundID.NPCDeath13, Projectile.Center);
            }

            Player player = Main.player[(int)Projectile.ai[1]];

            Projectile.velocity.X += (((player.Center.X + player.velocity.X * 60 - Projectile.Center.X) / 70f) - Projectile.velocity.X) / 70f;

            Projectile.rotation += Projectile.velocity.X / Projectile.width * 2f;
            Projectile.velocity.Y += 0.2f;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (oldVelocity.X != Projectile.velocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
                Projectile.velocity.X *= 0.8f;
            }
            if (oldVelocity.Y != Projectile.velocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
                Projectile.velocity.Y *= 0.8f;
            }
            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath19, Projectile.Center);

            if (Main.netMode != 1)
            {
                for (int i = 0; i < 10; i++)
                {
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, new Vector2(8f, 0).RotatedBy(i * MathHelper.TwoPi / 10f), ProjectileType<EsophageTooth>(), 27, 1f, Main.myPlayer, ai0: Projectile.ai[0], ai1: Projectile.ai[1]);
                }
            }

            for (int i = 0; i < 16; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Scale: 1.5f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            lightColor = Lighting.GetColor(Projectile.Center.ToTileCoordinates());

            Color mainColor = lightColor;

            for (int k = 1; k < Projectile.oldPos.Length; k++)
            {
                Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                float scale = Projectile.scale;

                float rotation = Projectile.rotation;

                Main.EntitySpriteDraw(texture, Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), color, rotation, new Vector2(texture.Width / 2, texture.Height / Main.projFrames[Projectile.type] / 2), scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), mainColor, Projectile.rotation, new Vector2(texture.Width / 2, texture.Height / Main.projFrames[Projectile.type] / 2), Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }

    public class EsophageTooth : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 12;
            Projectile.height = 12;
            DrawOffsetX = -2;
            DrawOriginOffsetY = -2;
            DrawOriginOffsetX = 0;
            Projectile.alpha = 0;

            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;

            Projectile.velocity.Y += 0.3f;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            lightColor = Lighting.GetColor(Projectile.Center.ToTileCoordinates());

            Color mainColor = lightColor;

            for (int k = 1; k < Projectile.oldPos.Length; k++)
            {
                Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                float scale = Projectile.scale;

                float rotation = (Projectile.oldPos[k - 1] - Projectile.oldPos[k]).ToRotation() + MathHelper.PiOver2;

                Main.EntitySpriteDraw(texture, Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), color, rotation, new Vector2(6, 6), scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), mainColor, Projectile.rotation, new Vector2(6, 6), Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            return false;
        }
    }
}