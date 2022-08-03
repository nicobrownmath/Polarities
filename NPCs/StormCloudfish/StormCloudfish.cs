using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Polarities.Projectiles;
using Polarities.Items;
using Polarities.Items.Placeable;
using Polarities.Items.Accessories;
using Polarities.Items.Armor;
using Polarities.Items.Placeable.Trophies;
using Terraria.Audio;
using Terraria.GameContent;
using ReLogic.Content;
using Polarities.Items.Armor.Vanity;
using Polarities.Items.Consumables.TreasureBags;
using Polarities.Items.Hooks;
using Polarities.Items.Materials;
using Polarities.Items.Placeable.Relics;
using Polarities.Items.Weapons.Ranged;
using Polarities.Items.Weapons.Summon.Minions;
using Terraria.GameContent.ItemDropRules;
using Polarities.Items.Pets;
using Terraria.GameContent.Bestiary;
using Polarities.Buffs;
using Terraria.DataStructures;
using Polarities.Items.Weapons.Magic;

namespace Polarities.NPCs.StormCloudfish
{
    [AutoloadBossHead]
    public class StormCloudfish : ModNPC
    {
        //TODO: 1.4 thunderstorm integration stuff
        //TODO: Rainglow
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 8;
            NPCID.Sets.TrailCacheLength[Type] = 3;
            NPCID.Sets.TrailingMode[Type] = 0;
            
            //group with other bosses
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
            {
                SpecificallyImmuneTo = new int[] {
                    BuffID.Confused,
                }
            };
            NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);

            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                SpriteDirection = 1
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifiers);
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.Rain,
				//flavor text
				this.TranslatedBestiaryEntry()
            });
        }

        private bool rainToggleUsed;

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = Main.getGoodWorld ? 280 : 140;
            NPC.height = 32;
            DrawOffsetY = 2;

            NPC.damage = 20;
            NPC.defense = 0;
            NPC.lifeMax = Main.masterMode ? 3456 / 3 : Main.expertMode ? 2880 / 2 : 1800;
            NPC.knockBackResist = 0f;
            NPC.alpha = 0;
            NPC.value = Item.buyPrice(0, 0, 50, 0);
            NPC.npcSlots = 15f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit30;
            NPC.DeathSound = SoundID.NPCDeath33;

            Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/StormCloudfish");

            if (Main.tenthAnniversaryWorld) NPC.scale *= 0.5f;
        }

        public static int secondStageHeadSlot;

        public override void BossHeadSlot(ref int index)
        {
            float expertHealthFrac = Main.expertMode ? 0.65f : 0.5f;
            if (NPC.lifeMax * expertHealthFrac >= NPC.life)
            {
                index = secondStageHeadSlot;
            }
        }

        public override void AI()
        {
            if (NPC.localAI[0] == 0)
            {
                NPC.localAI[0] = 1;
            }

            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
                if (player.dead)
                {
                    ;
                    if (NPC.timeLeft > 10)
                    {
                        NPC.timeLeft = 10;
                    }
                    if (NPC.velocity.Y > -8)
                    {
                        NPC.velocity.Y -= 0.5f;
                    }
                    return;
                }
            }

            float expertHealthFrac = Main.expertMode ? 0.65f : 0.5f;

            if (NPC.lifeMax * expertHealthFrac >= NPC.life)
            {
                if (!Main.raining)
                {
                    Main.StartRain();
                    rainToggleUsed = true;
                }
            }

            /*
			Storm cloudfish alternates between:
				Attempting to level itself with the player and get some distance away before charging horizontally
					During these charges it can move vertically, but only very slowly
					Each charge has a short acceleration and deceleration period
				Attempting to go above and a bit to the side of the player, before charging a short distance towards them horizontally while emitting short-lived raindrops
				Floating in place while summoning a clump of rainclouds a distance from either side of the player (The cooldown for this attack is longer than it takes for the rainclouds to disappear)
				
			Below a certain fraction of its health, it gains additional attacks
				The leveled dash is replaced with a faster lightning-powered dash that leaves behind short-lived ball lightning
				The short horizontal charge instead shoots down lightning, which is faster and has longer range

				In expert mode, the fish will float around and shoot large numbers of stormcloud orbs at the player, which it will discharge lightning at after a brief time
			The rate at which it uses the upgraded versions of attacks instead of the regular versions starts at 35% and goes to 100% at low health
			*/

            switch (NPC.ai[0])
            {
                case 0:
                    //set up charging the player

                    int goalDirection = player.Center.X < NPC.Center.X ? 1 : -1; //note to self: Can replace with a more sophisticated algorithm that takes existing velocity into account
                    Vector2 goalPosition = player.Center + new Vector2(goalDirection * 600, 0);

                    if (Math.Abs(NPC.velocity.X) > 1)
                    {
                        NPC.direction = NPC.velocity.X > 0 ? -1 : 1;
                        NPC.spriteDirection = NPC.direction;
                    }

                    float maxXAcceleration = 0.2f;
                    float maxXSpeed = 16f;
                    float maxYAcceleration = 0.1f;
                    float maxYSpeed = 4f;

                    Vector2 velDiff = NPC.velocity - player.velocity;

                    //the X coordinate at which we'll halt if we slam on the brakes
                    int haltDirectionX = velDiff.X > 0 ? 1 : -1;
                    float haltPointX = NPC.Center.X + haltDirectionX * (velDiff.X * velDiff.X) / (2 * maxXAcceleration);

                    if (goalPosition.X > haltPointX)
                    {
                        NPC.velocity.X += maxXAcceleration;
                    }
                    else
                    {
                        NPC.velocity.X -= maxXAcceleration;
                    }
                    NPC.velocity.X = Math.Min(maxXSpeed, Math.Max(-maxXSpeed, NPC.velocity.X));

                    //the Y coordinate at which we'll halt if we slam on the brakes
                    int haltDirectionY = velDiff.Y > 0 ? 1 : -1;
                    float haltPointY = NPC.Center.Y + haltDirectionY * (velDiff.Y * velDiff.Y) / (2 * maxYAcceleration);

                    if (goalPosition.Y > haltPointY)
                    {
                        NPC.velocity.Y += maxYAcceleration;
                    }
                    else
                    {
                        NPC.velocity.Y -= maxYAcceleration;
                    }
                    NPC.velocity.Y = Math.Min(maxYSpeed, Math.Max(-maxYSpeed, NPC.velocity.Y));

                    if ((NPC.Center - goalPosition).Length() < 16)
                    {
                        if (NPC.lifeMax * expertHealthFrac < NPC.life)
                        {
                            NPC.ai[0] = 1;
                        }
                        else
                        {
                            if (Main.netMode != 1)
                                NPC.ai[0] = Main.rand.Next(NPC.lifeMax) < NPC.life ? 1 : 2;
                            NPC.netUpdate = true;
                        }
                        NPC.ai[1] = 0;
                    }
                    break;
                case 1:
                    //charge the player
                    NPC.ai[1]++;

                    if (NPC.ai[1] == 1)
                    {
                        SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

                        NPC.direction = player.Center.X < NPC.Center.X ? 1 : -1;
                        NPC.spriteDirection = NPC.direction;

                        NPC.velocity = Vector2.Zero;
                    }

                    if (NPC.ai[1] < 16)
                    {
                        NPC.velocity.X -= NPC.direction;
                    }

                    if (NPC.ai[1] > 75)
                    {
                        NPC.velocity.X += NPC.direction;
                    }

                    if (NPC.ai[1] > 85)
                    {
                        NextAttack();
                    }

                    if (NPC.ai[1] < 45)
                    {
                        velDiff = NPC.velocity - player.velocity;

                        maxYAcceleration = 0.1f;
                        maxYSpeed = 4f;
                        haltDirectionY = velDiff.Y > 0 ? 1 : -1;
                        haltPointY = NPC.Center.Y + haltDirectionY * (velDiff.Y * velDiff.Y) / (2 * maxYAcceleration);

                        if (player.Center.Y > haltPointY)
                        {
                            NPC.velocity.Y += maxYAcceleration;
                        }
                        else
                        {
                            NPC.velocity.Y -= maxYAcceleration;
                        }
                        NPC.velocity.Y = Math.Min(maxYSpeed, Math.Max(-maxYSpeed, NPC.velocity.Y));
                    }
                    else
                    {
                        NPC.velocity.Y *= 0.99f;
                    }

                    break;
                case 2:
                    //empowered charge the player
                    NPC.ai[1]++;

                    if (NPC.ai[1] == 1)
                    {
                        SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

                        NPC.direction = player.Center.X < NPC.Center.X ? 1 : -1;
                        NPC.spriteDirection = NPC.direction;

                        NPC.velocity = Vector2.Zero;
                    }

                    if (NPC.ai[1] < 20)
                    {
                        NPC.velocity.X -= NPC.direction;
                    }

                    if (NPC.ai[1] > 65)
                    {
                        NPC.velocity.X += NPC.direction;
                    }

                    if (NPC.ai[1] > 78)
                    {
                        NextAttack();
                    }

                    if (NPC.ai[1] < 45)
                    {
                        velDiff = NPC.velocity - player.velocity;

                        maxYAcceleration = 0.15f;
                        maxYSpeed = 4f;
                        haltDirectionY = velDiff.Y > 0 ? 1 : -1;
                        haltPointY = NPC.Center.Y + haltDirectionY * (velDiff.Y * velDiff.Y) / (2 * maxYAcceleration);

                        if (player.Center.Y > haltPointY)
                        {
                            NPC.velocity.Y += maxYAcceleration;
                        }
                        else
                        {
                            NPC.velocity.Y -= maxYAcceleration;
                        }
                        NPC.velocity.Y = Math.Min(maxYSpeed, Math.Max(-maxYSpeed, NPC.velocity.Y));
                    }
                    else
                    {
                        NPC.velocity.Y *= 0.99f;
                    }

                    if (NPC.ai[1] >= 20 && NPC.ai[1] % 3 == 0 && Main.netMode != 1)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<StormCloudfishLightningBall>(), 8, 0, Main.myPlayer, 60);
                    }

                    break;
                case 3:
                    //set up charge above the player
                    goalDirection = player.Center.X < NPC.Center.X ? 1 : -1; //note to self: Can replace with a more sophisticated algorithm that takes existing velocity into account
                    goalPosition = player.Center + new Vector2(goalDirection * 300, -300);

                    if (Math.Abs(NPC.velocity.X) > 1)
                    {
                        NPC.direction = NPC.velocity.X > 0 ? -1 : 1;
                        NPC.spriteDirection = NPC.direction;
                    }

                    maxXAcceleration = 0.2f;
                    maxXSpeed = 16f;
                    maxYAcceleration = 0.1f;
                    maxYSpeed = 4f;

                    velDiff = NPC.velocity - player.velocity;

                    //the X coordinate at which we'll halt if we slam on the brakes
                    haltDirectionX = velDiff.X > 0 ? 1 : -1;
                    haltPointX = NPC.Center.X + haltDirectionX * (velDiff.X * velDiff.X) / (2 * maxXAcceleration);

                    if (goalPosition.X > haltPointX)
                    {
                        NPC.velocity.X += maxXAcceleration;
                    }
                    else
                    {
                        NPC.velocity.X -= maxXAcceleration;
                    }
                    NPC.velocity.X = Math.Min(maxXSpeed, Math.Max(-maxXSpeed, NPC.velocity.X));

                    //the Y coordinate at which we'll halt if we slam on the brakes
                    haltDirectionY = velDiff.Y > 0 ? 1 : -1;
                    haltPointY = NPC.Center.Y + haltDirectionY * (velDiff.Y * velDiff.Y) / (2 * maxYAcceleration);

                    if (goalPosition.Y > haltPointY)
                    {
                        NPC.velocity.Y += maxYAcceleration;
                    }
                    else
                    {
                        NPC.velocity.Y -= maxYAcceleration;
                    }
                    NPC.velocity.Y = Math.Min(maxYSpeed, Math.Max(-maxYSpeed, NPC.velocity.Y));

                    if ((NPC.Center - goalPosition).Length() < 16)
                    {
                        if (NPC.lifeMax * expertHealthFrac < NPC.life)
                        {
                            NPC.ai[0] = 4;
                        }
                        else
                        {
                            if (Main.netMode != 1)
                                NPC.ai[0] = Main.rand.Next(NPC.lifeMax) < NPC.life ? 4 : 5;
                            NPC.netUpdate = true;
                        }
                        NPC.ai[1] = 0;
                    }
                    break;
                case 4:
                    //charge above the player
                    NPC.ai[1]++;

                    if (NPC.ai[1] == 1)
                    {
                        NPC.direction = player.Center.X < NPC.Center.X ? 1 : -1;
                        NPC.spriteDirection = NPC.direction;

                        NPC.velocity = Vector2.Zero;
                    }

                    if (NPC.ai[1] < 10)
                    {
                        NPC.velocity.X -= NPC.direction;
                    }
                    if (NPC.ai[1] > 50)
                    {
                        NPC.velocity.X += NPC.direction;
                    }

                    if (NPC.ai[1] > 59)
                    {
                        NextAttack();
                    }

                    if (NPC.ai[1] % 5 == 4 && Main.netMode != 1)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            int shot = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(Main.rand.NextFloat(-50, 50), 0), new Vector2(0, 7), ProjectileType<CloudfishRain>(), 7, 0, Main.myPlayer);
                            //Main.projectile[shot].timeLeft = 60;
                        }
                    }

                    NPC.velocity.Y *= 0.95f;
                    break;
                case 5:
                    //empowered charge above the player
                    NPC.ai[1]++;

                    if (NPC.ai[1] == 1)
                    {
                        NPC.direction = player.Center.X < NPC.Center.X ? 1 : -1;
                        NPC.spriteDirection = NPC.direction;

                        NPC.velocity = Vector2.Zero;
                    }

                    if (NPC.ai[1] < 10)
                    {
                        NPC.velocity.X -= NPC.direction;
                    }
                    if (NPC.ai[1] > 50)
                    {
                        NPC.velocity.X += NPC.direction;
                    }

                    if (NPC.ai[1] > 59)
                    {
                        NextAttack();
                    }

                    if (NPC.ai[1] % 5 == 4)
                    {
                        //client side non-synced projectile because syncing doesn't work for some reason, probably related to it being an extraupdates projectile that calls a bunch of random things?
                        if (Main.netMode != 2)
                        {
                            Vector2 aimPosition = NPC.Center + new Vector2(0, 480);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (aimPosition - NPC.Center).SafeNormalize(Vector2.Zero) * 20, ProjectileType<StormCloudfishLightningArc>(), 10, 0, Main.myPlayer, aimPosition.X, aimPosition.Y);
                        }
                        SoundEngine.PlaySound(SoundID.Item122, NPC.Center);
                    }

                    NPC.velocity.Y *= 0.95f;
                    break;
                case 6:
                    //summon 'raincloud arena'

                    NPC.ai[1]++;

                    if (NPC.ai[1] == 1)
                    {
                        //cooldown of 20 seconds
                        NPC.ai[2] = 1200;
                        NPC.ai[3] = player.Center.X;

                        NPC.direction = player.Center.X < NPC.Center.X ? 1 : -1;
                        NPC.spriteDirection = NPC.direction;
                    }

                    NPC.velocity.X -= NPC.direction * 0.35f;
                    NPC.velocity *= 0.95f;

                    if (NPC.ai[1] > 10 && NPC.ai[1] <= 110 && NPC.ai[1] % 5 == 0)
                    {
                        int direction = NPC.ai[1] % 2 == 0 ? 1 : -1;

                        if (NPC.ai[1] % 10 == 0)
                        {
                            SoundEngine.PlaySound(SoundID.NPCHit30, NPC.Center);
                        }

                        goalPosition = new Vector2(NPC.ai[3] + direction * 720 + Main.rand.Next(-50, 50), NPC.Center.Y - 400 + Main.rand.Next(-400, 400));

                        if (Main.netMode != 1)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (goalPosition - NPC.Center).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(8, 12), ProjectileType<StormCloudfishCloudOrb>(), 0, 0, Main.myPlayer, goalPosition.X, goalPosition.Y);
                        }
                    }

                    if (NPC.ai[1] == 120)
                    {
                        NextAttack();
                    }
                    break;
                case 7:
                    //expert-exclusive stormcloud orbs

                    NPC.ai[1]++;

                    if (NPC.ai[1] == 1)
                    {
                        NPC.direction = player.Center.X < NPC.Center.X ? 1 : -1;
                        NPC.spriteDirection = NPC.direction;
                    }

                    NPC.velocity.X -= NPC.direction * 0.35f;
                    NPC.velocity *= 0.95f;

                    if (NPC.ai[1] > 10 && NPC.ai[1] <= 110 && NPC.ai[1] % 10 == 0)
                    {
                        if (NPC.ai[1] % 20 == 0)
                        {
                            SoundEngine.PlaySound(SoundID.NPCHit30, NPC.Center);
                        }

                        if (Main.netMode != 1)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (player.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedByRandom(0.5f) * Main.rand.NextFloat(16, 24), ProjectileType<StormCloudfishStormcloudOrb>(), 0, 0, Main.myPlayer, NPC.whoAmI);
                        }
                    }

                    if (NPC.ai[1] == 120)
                    {
                        NextAttack();
                    }
                    break;
            }

            if (NPC.ai[2] > 0) NPC.ai[2]--;
        }

        private void NextAttack()
        {
            float expertHealthFrac = Main.expertMode ? 0.65f : 0.5f;

            int[] attacks = new int[4];
            int numAttacks = 2;
            //can include 0, 3, 6, and 7

            attacks[0] = 0;
            attacks[1] = 3;
            if (NPC.ai[2] == 0)
            {
                //this corresponds to if the arena setup attack cooldown has passed
                attacks[numAttacks] = 6;
                numAttacks++;
            }
            if (Main.expertMode && NPC.lifeMax * expertHealthFrac >= NPC.life && NPC.ai[0] != 7)
            {
                attacks[numAttacks] = 7;
                numAttacks++;
            }

            if (Main.netMode != 1)
                NPC.ai[0] = attacks[Main.rand.Next(numAttacks)];
            NPC.ai[1] = 0;
            NPC.netUpdate = true;
        }

        public override void FindFrame(int frameHeight)
        {
            int frameWidth = 164;
            NPC.frame.Width = frameWidth;

            NPC.frameCounter++;
            if (NPC.frameCounter == 5)
            {
                NPC.frameCounter = 0;

                bool dash = NPC.ai[0] == 1 || NPC.ai[0] == 2;

                int frame = NPC.frame.Y / frameHeight + 8 * NPC.frame.X / frameWidth;
                int nextFrame = frame + 1;

                float expertHealthFrac = Main.expertMode ? 0.65f : 0.5f;
                if (NPC.lifeMax * expertHealthFrac >= NPC.life)
                {
                    //in phase 2
                    bool lightning = NPC.ai[0] == 2 || NPC.ai[0] == 5 || NPC.ai[0] == 7;

                    if (dash)
                    {
                        if (nextFrame < 34)
                        {
                            nextFrame = 34;
                        }
                        else if (nextFrame >= 36)
                        {
                            nextFrame = 34;
                        }
                    }
                    else
                    {
                        if (nextFrame < 15)
                        {
                            //still transitioning
                            if (nextFrame >= 7 && nextFrame <= 12)
                            {
                                nextFrame = 13;
                            }
                        }
                        else
                        {
                            //done transitioning
                            if (lightning)
                            {
                                //lightning effects
                                if (nextFrame >= 23)
                                {
                                    nextFrame -= 8;
                                }
                            }
                            else
                            {
                                //no lightning effects
                                if (nextFrame <= 22)
                                {
                                    nextFrame += 8;
                                }
                                else if (nextFrame >= 31)
                                {
                                    nextFrame -= 8;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //in phase 1
                    if (dash)
                    {
                        if (nextFrame < 11)
                        {
                            nextFrame = 11;
                        }
                        else if (nextFrame >= 13)
                        {
                            nextFrame = 11;
                        }
                    }
                    else
                    {
                        if (nextFrame >= 8)
                        {
                            nextFrame = 0;
                        }
                    }
                }

                NPC.frame.X = nextFrame / 8 * frameWidth;
                NPC.frame.Y = nextFrame % 8 * frameHeight;
            }
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return NPC.ai[0] == 1 || NPC.ai[0] == 2;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (NPC.ai[0] == 1 || NPC.ai[0] == 2)
            {
                return null;
            }
            return false;
        }

        public static Asset<Texture2D> GlowTexture;

        public override void Load()
        {
            GlowTexture = Request<Texture2D>(Texture + "_Mask");
        }

        public override void Unload()
        {
            GlowTexture = null;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
        {
            Vector2 drawOrigin = NPC.frame.Size() / 2;
            Vector2 drawPos;
            Color color;

            bool dash = NPC.ai[0] == 1 || NPC.ai[0] == 2;

            Vector2 scale = new Vector2(Main.getGoodWorld ? 2 : 1, 1) * NPC.scale;

            if (dash)
            {
                for (int k = 0; k < NPC.oldPos.Length; k++)
                {
                    drawPos = NPC.Center - NPC.position + NPC.oldPos[k] - screenPos + new Vector2(0f, NPC.gfxOffY);
                    color = NPC.GetAlpha(lightColor) * ((float)(NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);

                    spriteBatch.Draw(TextureAssets.Npc[Type].Value, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
                    spriteBatch.Draw(GlowTexture.Value, drawPos, NPC.frame, Color.White * ((float)(NPC.oldPos.Length - k) / (float)NPC.oldPos.Length), NPC.rotation, drawOrigin, scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
                }
            }
            drawPos = NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY);
            color = NPC.GetAlpha(lightColor);

            spriteBatch.Draw(TextureAssets.Npc[Type].Value, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            spriteBatch.Draw(GlowTexture.Value, drawPos, NPC.frame, Color.White, NPC.rotation, drawOrigin, scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);

            return false;
        }

        public override bool CheckDead()
        {
            for (int i = 0; i < Main.rand.Next(6, 11); i++)
            {
                Main.npc[NPC.NewNPC(NPC.GetSource_Death(), (int)NPC.Center.X + Main.rand.Next(-NPC.width / 4, NPC.width / 4), (int)NPC.Center.Y + Main.rand.Next(-NPC.height / 4, NPC.height / 4), NPCType<NPCs.Critters.StormcloudCichlid>())].velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
            }
            for (int i = 0; i < 40; i++)
            {
                Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, 36, Scale: 1.5f)].noGravity = true;
            }

            if (rainToggleUsed)
            {
                Main.StopRain();
            }

            if (!PolaritiesSystem.downedStormCloudfish)
            {
                PolaritiesSystem.downedStormCloudfish = true;
                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.WorldData); // Immediately inform clients of new world state.
                }
            }
            return true;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(new FlawlessOrRandomDropRule(ItemType<StormCloudfishTrophy>(), 10));
            npcLoot.Add(ItemDropRule.BossBag(ItemType<StormCloudfishBag>()));
            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ItemType<StormCloudfishRelic>()));
            npcLoot.Add(ModUtils.MasterModeDropOnAllPlayersOrFlawless(ItemType<StormCloudfishPetItem>(), 4));

            //normal mode loot
            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemType<StormCloudfishMask>(), 7));
            notExpertRule.OnSuccess(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ItemType<Stormcore>(), ItemType<Skyhook>(), ItemType<StrangeBarometer>()));
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemType<StormChunk>(), 1, 5, 15));
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemID.Cloud, 1, 20, 40));
            notExpertRule.OnSuccess(ItemDropRule.Common(ItemType<GoldfishExplorerPetItem>(), 20));
            npcLoot.Add(notExpertRule);

            npcLoot.Add(ItemDropRule.ByCondition(new FlawlessDropCondition(), ItemType<EyeOfTheStormfish>()));
        }
    }

    public class StormCloudfishCloudOrb : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 1200;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.rotation += 0.05f;

            Projectile.alpha = Math.Max(0, Projectile.alpha - 8);

            Projectile.velocity = (new Vector2(Projectile.ai[0], Projectile.ai[1]) - Projectile.Center).SafeNormalize(Vector2.Zero) * Projectile.velocity.Length();
            if ((new Vector2(Projectile.ai[0], Projectile.ai[1]) - Projectile.Center).Length() < Projectile.velocity.Length())
            {
                Projectile.Kill();
            }
        }

        public override void Kill(int timeLeft)
        {
            if (Main.netMode != 1)
            {
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center.X, Projectile.Center.Y, 0, 0, ProjectileType<StormCloudfishRaincloud>(), 0, 0f, Main.myPlayer, 0, 0);
            }
        }
    }
    public class StormCloudfishRaincloud : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.width = 56;
            Projectile.height = 22;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 1200;
        }

        public override void AI()
        {
            if (Projectile.timeLeft % 40 == 0 && Main.netMode != 1)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center.X + Main.rand.NextFloat(-20, 20), Projectile.Center.Y, 0, 6, ProjectileID.RainNimbus, 7, 1f, Main.myPlayer, 0, 0);
            }
            Projectile.alpha = Math.Max(0, 255 - 4 * Projectile.timeLeft);

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 4;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Color color = Projectile.GetAlpha(lightColor);
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(3, 4, 0, Projectile.frame);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, color, Projectile.rotation, frame.Size() / 2, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }

    public class StormCloudfishStormcloudOrb : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 60;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (!Main.npc[(int)Projectile.ai[0]].active)
            {
                Projectile.ai[1] = 1;
            }

            Projectile.rotation += 0.05f;

            Projectile.alpha = Math.Max(0, Projectile.alpha - 8);

            Projectile.velocity *= 0.98f;
        }

        public override void Kill(int timeLeft)
        {
            if (Projectile.ai[1] == 0)
            {
                SoundEngine.PlaySound(SoundID.Item122, Main.npc[(int)Projectile.ai[0]].Center);

                //see prior non-synced projectile comment
                if (Main.netMode != 2)
                {
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Main.npc[(int)Projectile.ai[0]].Center, (Projectile.Center - Main.npc[(int)Projectile.ai[0]].Center).SafeNormalize(Vector2.Zero) * 20, ProjectileType<StormCloudfishLightningArc>(), 10, 0, Main.myPlayer, Projectile.Center.X, Projectile.Center.Y);
                }

                if (Main.netMode != 1)
                {
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ProjectileType<StormCloudfishLightningBall>(), 8, 0, Main.myPlayer, 240);
                }
            }
        }
    }

    public class StormCloudfishLightningArc : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Pixel";

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.alpha = 0;
            Projectile.timeLeft = 120;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 119;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }

        private Vector2 goalPoint = Vector2.Zero;

        public override void AI()
        {
            if (Main.myPlayer != Projectile.owner)
            {
                Projectile.Kill();
                return;
            }

            if (goalPoint == Vector2.Zero)
            {
                goalPoint = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            }

            //ai[0] is rotation
            //unfortunately I can't really sync these well, so hopefully it doesn't break anything
            Projectile.ai[0] = (goalPoint - Projectile.Center).ToRotation() + Main.rand.NextFloat(-1, 1);

            //don't do random variation if close enough
            if ((goalPoint - Projectile.Center).Length() < Projectile.velocity.Length() * 2)
            {
                Projectile.ai[0] = (goalPoint - Projectile.Center).ToRotation();
            }

            Projectile.velocity = new Vector2(Projectile.velocity.Length(), 0).RotatedBy(Projectile.ai[0]);

            if ((goalPoint - Projectile.Center).Length() < Projectile.velocity.Length())
            {
                Projectile.Kill();
                return;
            }

            for (int i = 0; i < 5; i++)
            {
                Dust.NewDustPerfect(Projectile.Center + i * Projectile.velocity / 5, DustID.Electric, Velocity: Vector2.Zero, Scale: 1f).noGravity = true;
            }
        }
    }

    public class StormCloudfishLightningBall : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Pixel";

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.alpha = 0;
            Projectile.timeLeft = 60;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                Projectile.timeLeft = (int)Projectile.ai[0];
            }

            Vector2 dustPos = Projectile.Center + new Vector2(Main.rand.NextFloat(16), 0).RotatedByRandom(MathHelper.TwoPi);
            Dust dust = Dust.NewDustPerfect(dustPos, DustID.Electric, Velocity: Vector2.Zero, Scale: 1f);
            dust.noGravity = true;
            dust.velocity = (dustPos - Projectile.Center) / 8;
        }
    }

    public class CloudfishRain : ModProjectile
    {
        //literally just nimbus rain but it doesn't last for as long
        //hopefully doing it this way doesn't break everything
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainNimbus;

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.RainNimbus);
            Projectile.timeLeft = 60;
        }

        public override bool PreAI()
        {
            Projectile.type = ProjectileID.RainNimbus;
            return true;
        }
    }
}