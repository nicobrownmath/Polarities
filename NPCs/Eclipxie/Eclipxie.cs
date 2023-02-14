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
using Polarities.Buffs;
using Polarities.Items;
using Polarities.Items.Accessories;
using Polarities.Items.Placeable;
using Polarities.Items.Weapons;
using Polarities.Items.Armor;
using Polarities.Items.Placeable.Trophies;

namespace Polarities.NPCs.Eclipxie
{
    [AutoloadBossHead]
    public class Eclipxie : ModNPC
    {
        private int attackPattern
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }

        private int attackSubPattern;
        private int attackCooldown;
        private float patience;
        private int orbiterCooldown;
        private int orbiterDirection = -1;

        private Vector2 targetDiff;
        private Vector2 targetPosition;
        private Vector2 moonOffset;
        private Vector2 moonOffsetDelta;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Eclipxie");
            Main.npcFrameCount[NPC.type] = 8;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 64;
            NPC.height = 64;

            NPC.defense = 45;
            NPC.damage = 70;
            NPC.lifeMax = 60000;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 15, 0, 0);
            NPC.npcSlots = 15f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit5;

            NPC.buffImmune[BuffID.Confused] = true;
            NPC.buffImmune[BuffID.OnFire] = true;
            NPC.buffImmune[BuffID.Frostburn] = true;

            Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Eclipxie");
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.6f * bossLifeScale);
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            if (Main.expertMode && (attackCooldown == 0 || NPC.hide) && (NPC.life * 4 < NPC.lifeMax * 3 && numSplitPhases == 0 || NPC.life * 2 < NPC.lifeMax && numSplitPhases == 1 || NPC.life * 4 < NPC.lifeMax && numSplitPhases == 2))
            {
                position = Vector2.Zero;
                return true;
            }
            return null;
        }

        private int numSplitPhases;

        public override void AI()
        {
            //always an eclipse
            if (!Main.dayTime || Main.dayTime && Main.time == Main.dayLength - 1)
            {
                Main.dayTime = true;
                Main.time = Main.dayLength - 2;
            }
            if (!Main.eclipse)
            {
                Main.eclipse = true;
            }

            NPC.realLife = NPC.whoAmI;

            if (reformAnimationTimeLeft > 0)
            {
                reformAnimationTimeLeft--;
            }

            if (NPC.localAI[0] < spawnAnimTime)
            {
                NPC.localAI[0]++;
                NPC.dontTakeDamage = true;
                NPC.chaseable = false;
                return;
            }

            Player player = Main.player[NPC.target];
            if (!player.active || player.dead || !Main.eclipse)
            {
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
                if (player.dead || !Main.eclipse)
                {
                    if (NPC.timeLeft > 10)
                    {
                        NPC.timeLeft = 10;
                    }
                    NPC.velocity.Y -= 0.1f;
                    return;
                }
            }

            //AI for when the NPC is hidden during sol moth/luna butterfly phases (expert only)
            //only *start* if attackCooldown is 0
            if (Main.expertMode && (attackCooldown == 0 || NPC.hide))
            {
                if (NPC.life * 4 < NPC.lifeMax * 3 && numSplitPhases == 0 || NPC.life * 2 < NPC.lifeMax && numSplitPhases == 1 || NPC.life * 4 < NPC.lifeMax && numSplitPhases == 2)
                {
                    //cancel all buffs
                    for (int i = 0; i < NPC.buffTime.Length; i++)
                    {
                        NPC.buffTime[i] = 0;
                    }

                    //initialization
                    if (!NPC.hide)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int solMoth = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<SolMoth>());
                            Main.npc[solMoth].realLife = NPC.whoAmI;
                            Main.npc[solMoth].netUpdate = true;
                            int lunaButterfly = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<LunaButterfly>());
                            Main.npc[lunaButterfly].realLife = NPC.whoAmI;
                            Main.npc[lunaButterfly].netUpdate = true;
                        }

                        //this phase always lasts exactly 21 seconds
                        attackCooldown = 1260;

                        SoundEngine.PlaySound(SoundID.NPCDeath14, NPC.Center);
                    }
                    NPC.hide = true;
                    NPC.damage = 0;
                    NPC.chaseable = false;

                    NPC.velocity = Vector2.Zero;
                    NPC.position = player.Center + new Vector2(0, -400) + (NPC.position - NPC.Center);

                    attackCooldown--;

                    if (attackCooldown == 0)
                    {
                        //attack finished, show, teleport above the player, and make sunbeams attack the inaccessible one
                        numSplitPhases++;
                        NPC.hide = false;
                        NPC.damage = NPC.defDamage;
                        NPC.chaseable = true;
                        attackPattern = -1;
                        attackCooldown = 60;

                        reformAnimationTimeLeft = reformAnimationTime;

                        SoundEngine.PlaySound(SoundID.Item29.WithPitchOffset(-0.25f), NPC.Center);
                        //Main.PlaySound(SoundID.Item, npc.Center, 29);
                    }

                    return;
                }
            }

            NPC.dontTakeDamage = false;
            NPC.chaseable = true;

            //summon in planet pixies
            if (!Main.expertMode && 2 * NPC.life < NPC.lifeMax || Main.expertMode && numSplitPhases >= 2)
            {
                bool orbiters = false;
                for (int j = 0; j < Main.maxNPCs; j++)
                {
                    if (Main.npc[j].active && Main.npc[j].type == NPCType<PlanetPixie>() && Main.npc[j].ai[0] == NPC.whoAmI)
                    {
                        orbiters = true;
                        break;
                    }
                }
                if (!orbiters && orbiterCooldown > 0)
                {
                    orbiterCooldown--;
                }
                if (orbiterCooldown == 0)
                {
                    orbiterCooldown = 60;
                    orbiterDirection *= -1;

                    if (Main.netMode != 1)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            int orbiter = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCType<PlanetPixie>(), ai0: NPC.whoAmI, ai1: i, ai3: orbiterDirection);
                            Main.npc[orbiter].netUpdate = true;
                        }
                    }
                }
            }

            if (attackCooldown == 0)
            {
                if (Main.netMode != 1)
                {
                    attackPattern = (attackPattern + 1 + Main.rand.Next(4)) % 5;
                }
                NPC.netUpdate = true;
            }
            switch (attackPattern)
            {
                case -1:
                    //just reformed
                    NPC.velocity = Vector2.Zero;
                    //do not use standard motion code
                    NPC.ai[1] = 0;
                    attackCooldown--;

                    //sunbeams is the inaccessible attack after this
                    if (attackCooldown == 0) attackPattern = 0;
                    break;
                case 1:
                    //Ancient Starlight
                    if (attackCooldown == 0)
                    {
                        attackCooldown = -1;

                        attackSubPattern = 0;
                        patience = 1;
                        if (Main.netMode != 1)
                        {
                            float r = Main.rand.NextFloat(400, 500);
                            float theta = Main.rand.NextFloat() * (float)Math.PI;
                            targetDiff = new Vector2(r * (float)Math.Cos(theta), -r * (float)Math.Sin(theta));
                        }
                        NPC.netUpdate = true;
                    }
                    if (attackSubPattern == 0)
                    {
                        //moving into position
                        //patience acts as an accumulating speed multiplier to ensure we get into position
                        //do not use standard motion code
                        NPC.ai[1] = 0;

                        //npc.velocity = player.Center + targetDiff - npc.Center;

                        /*float speed = 8 * patience;
                        if (npc.velocity.Length() > speed)
                        {
                            npc.velocity.Normalize();
                            npc.velocity *= speed;
                            patience *= 1.01f;
                        }*/
                        if (attackCooldown > -120)
                        {
                            GoTowardsRadial(player.Center + targetDiff, player.Center, attackCooldown - -120, maxSpeed: float.PositiveInfinity);
                            attackCooldown--;
                        }
                        else
                        {
                            NPC.velocity = player.Center + targetDiff - NPC.Center;

                            //position reached, do starlight attack and remain still for it
                            attackSubPattern = 1;
                            attackCooldown = 90;

                            Vector2 spread = (NPC.Center - player.Center).RotatedBy(Math.PI / 2);
                            spread.Normalize();
                            spread *= 250;
                            for (int i = -31; i < 32; i += 2)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + 3 * spread.RotatedBy(-Math.PI / 2) + i * spread / 4, spread.RotatedBy(Math.PI / 2) / 16, ProjectileType<AncientStarlight>(), 20, 1, Main.myPlayer, NPC.Center.X, NPC.Center.Y);
                            }
                        }

                        moonOffsetDelta = NPC.velocity;
                    }
                    else if (attackSubPattern == 1)
                    {
                        //do not use standard motion code
                        NPC.ai[1] = 0;

                        NPC.velocity = Vector2.Zero;

                        if (attackCooldown == 90 - 30)
                        {
                            SoundEngine.PlaySound(SoundID.Item29, NPC.Center);
                        }

                        attackCooldown--;
                        moonOffsetDelta = -moonOffset;
                    }
                    break;
                //case 0 can't go first: we never start with this attack
                case 0:
                    //Sunbeams
                    if (attackCooldown == 0)
                    {
                        attackCooldown = 60 + 2 * teleportAnimTime;
                        attackSubPattern = 0;
                    }
                    if (attackCooldown >= 60 + teleportAnimTime)
                    {
                        //dissipation
                        //do not use standard motion code
                        NPC.ai[1] = 0;
                        NPC.velocity *= 0.975f;

                        //can't hit player or be hit while dissipating
                        NPC.damage = 0;
                        NPC.dontTakeDamage = true;
                        NPC.chaseable = false;

                        if (attackCooldown == 60 + teleportAnimTime)
                        {
                            //teleport above the player
                            NPC.position = player.Center + new Vector2(0, -400) + (NPC.position - NPC.Center);
                            NPC.velocity = Vector2.Zero;
                        }

                        moonOffsetDelta = -moonOffset;
                    }
                    else if (attackCooldown >= 60)
                    {
                        //reforming
                        //do not use standard motion code
                        NPC.ai[1] = 0;
                        NPC.velocity = Vector2.Zero;

                        //can't hit player or be hit while dissipating
                        NPC.damage = 0;
                        NPC.dontTakeDamage = true;
                        NPC.chaseable = false;

                        moonOffsetDelta = -moonOffset;

                        if (attackCooldown == 60)
                        {
                            //launch flares
                            targetPosition = NPC.Center + new Vector2(0, -1000);
                            SoundEngine.PlaySound(SoundID.Item109, NPC.Center);
                            if (Main.netMode != 1)
                            {
                                for (int i = -4; i < 5; i++)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 2).RotatedBy(i / 2.5f), ProjectileType<EclipseFlareBolt>(), 1, 0, Main.myPlayer, 0, 0);
                                }
                            }

                            //restore damage
                            NPC.damage = NPC.defDamage;
                            NPC.dontTakeDamage = false;
                            NPC.chaseable = true;
                        }
                    }
                    else
                    {
                        moonOffsetDelta = new Vector2(0, 1);
                    }

                    attackCooldown--;

                    break;
                case 2:
                    //Solar Scythes
                    if (attackCooldown == 0)
                    {
                        attackCooldown = 120;
                    }
                    targetPosition = player.Center;
                    attackCooldown--;
                    if (attackCooldown % 20 == 0 && Main.netMode != 1)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 0.005f).RotatedBy((player.Center - NPC.Center).ToRotation() - Math.PI / 2), ProjectileType<SolarScythe>(), 30, 0, Main.myPlayer, 0, 0);
                    }
                    moonOffsetDelta = targetPosition - NPC.Center;
                    break;
                case 3:
                    //circling barrage
                    if (attackCooldown == 0)
                    {
                        attackCooldown = 240;
                        if (Main.netMode != 1)
                        {
                            attackSubPattern = 2 * Main.rand.Next() - 1;
                        }
                        NPC.netUpdate = true;
                    }
                    targetDiff = (NPC.Center - player.Center).RotatedBy(attackSubPattern * Math.PI / 2);
                    targetDiff.Normalize();
                    targetDiff *= 150;
                    targetPosition = player.Center + targetDiff;
                    attackCooldown--;
                    if (attackCooldown % (int)(5 + 15 * (NPC.life / (float)NPC.lifeMax)) == 0)
                    {
                        if (Main.netMode != 1)
                        {
                            int laser = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 12).RotatedBy((player.Center - NPC.Center).ToRotation() - Math.PI / 2), ProjectileType<EclipxieRay>(), 25, 0, Main.myPlayer, 1, 0);
                            Main.projectile[laser].tileCollide = false;
                        }
                        SoundEngine.PlaySound(SoundID.Item33, NPC.position);
                    }
                    moonOffsetDelta = new Vector2(1, 0).RotatedBy((player.Center - NPC.Center).ToRotation() + attackSubPattern * Math.PI / 2);
                    break;
                case 4:
                    //sweeping deathray
                    if (attackCooldown == 0)
                    {
                        attackCooldown = -1;

                        attackSubPattern = 0;
                        patience = 1;

                        if (Main.netMode != 1)
                        {
                            NPC.direction = Main.rand.NextBool() ? 1 : -1;
                            targetDiff = new Vector2(-NPC.direction * 600, -400);
                        }
                        NPC.netUpdate = true;
                    }
                    if (attackSubPattern == 0)
                    {
                        //moving into position
                        //patience acts as an accumulating speed multiplier to ensure we get into position
                        //do not use standard motion code
                        NPC.ai[1] = 0;

                        /*float speed = 8 * patience;
                        if (npc.velocity.Length() > speed)
                        {
                            npc.velocity.Normalize();
                            npc.velocity *= speed;
                            patience *= 1.01f;
                        }*/
                        if (attackCooldown > -120)
                        {
                            GoTowardsRadial(player.Center + targetDiff, player.Center, attackCooldown - -120, maxSpeed: float.PositiveInfinity);
                            attackCooldown--;
                        }
                        else
                        {
                            NPC.velocity = player.Center + targetDiff - NPC.Center;

                            //position reached, do deathray attack and begin to move
                            attackSubPattern = 1;
                            attackCooldown = 150;

                            //make deathray
                            SoundEngine.PlaySound(SoundID.Item109, NPC.Center);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 1), ProjectileType<EclipxieDeathray>(), 40, 2f, Main.myPlayer, ai1: NPC.whoAmI);
                        }

                        //npc.velocity = player.Center + targetDiff - npc.Center;
                        moonOffsetDelta = NPC.velocity;
                    }
                    else if (attackSubPattern == 1)
                    {
                        //do not use standard motion code
                        NPC.ai[1] = 0;

                        if (attackCooldown >= 120)
                        {
                            NPC.velocity = Vector2.Zero;
                        }
                        else
                        {
                            NPC.velocity.X += NPC.direction * 0.34f;
                        }

                        attackCooldown--;
                        moonOffsetDelta = -moonOffset;
                    }
                    break;
            }

            if (NPC.ai[1] == 1)
            {
                Target(targetPosition);
                if (moonOffsetDelta.Length() > 0.1f)
                {
                    moonOffsetDelta.Normalize();
                    moonOffsetDelta *= 0.1f;
                }
            }
            else
            {
                NPC.ai[1] = 1;
            }
            moonOffset += moonOffsetDelta;
            if (moonOffset.Length() > 3)
            {
                moonOffset.Normalize();
                moonOffset *= 3;
            }
        }

        private void Target(Vector2 position)
        {
            float speed = (float)Math.Max(0.1f, Math.Sqrt(NPC.velocity.X * NPC.velocity.X + NPC.velocity.Y * NPC.velocity.Y));
            float dist = (float)Math.Max(0.1f, Math.Sqrt((position.X - NPC.Center.X) * (position.X - NPC.Center.X) + (position.Y - NPC.Center.Y) * (position.Y - NPC.Center.Y)));

            float vectorAngle = 0.01f * (1 - NPC.life / NPC.lifeMax) + (float)Math.Acos(Math.Min(1, Math.Max(-1, NPC.velocity.X / speed * (position.X - NPC.Center.X) / dist + NPC.velocity.Y / speed * (position.Y - NPC.Center.Y) / dist)));

            NPC.velocity.X *= 0.99f;
            NPC.velocity.Y *= 0.99f;

            NPC.velocity.X += 0.02f * vectorAngle * (position.X - NPC.Center.X) / (1 + speed);
            NPC.velocity.Y += 0.02f * vectorAngle * (position.Y - NPC.Center.Y) / (1 + speed);
        }

        private void GoTowardsRadial(Vector2 goalPosition, Vector2 orbitPoint, float timeLeft, float maxSpeed = 24f)
        {
            float dRadial = (goalPosition - orbitPoint).Length() - (NPC.Center - orbitPoint).Length();
            float dAngle = (goalPosition - orbitPoint).ToRotation() - (NPC.Center - orbitPoint).ToRotation();
            while (dAngle > MathHelper.Pi)
            {
                dAngle -= MathHelper.TwoPi;
            }
            while (dAngle < -MathHelper.Pi)
            {
                dAngle += MathHelper.TwoPi;
            }

            NPC.velocity = (new Vector2(dRadial, dAngle * (NPC.Center - orbitPoint).Length()).RotatedBy((NPC.Center - orbitPoint).ToRotation()) + (goalPosition - NPC.Center)) / 2 / timeLeft;

            if (NPC.velocity.Length() > maxSpeed)
            {
                NPC.velocity.Normalize();
                NPC.velocity *= maxSpeed;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter = (NPC.frameCounter + 1) % 5;
            if (NPC.frameCounter == 0)
            {
                NPC.frame.Y = (NPC.frame.Y + frameHeight) % (8 * frameHeight);
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Frostburn, 150, true);
            target.AddBuff(BuffID.OnFire, 150, true);
        }

        private static readonly int spawnAnimTime = 120;
        private static readonly int teleportAnimTime = 60;
        private static readonly int reformAnimationTime = 40;

        private int reformAnimationTimeLeft = 0;

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.localAI[0] < spawnAnimTime)
            {
                for (int i = 0; i <= 100; i++)
                {
                    float rotationAmount = spawnAnimTime / (spawnAnimTime - NPC.localAI[0]);
                    Vector2 imageDrawOffsetA = new Vector2(spawnAnimTime - NPC.localAI[0], 0).RotatedBy(i * MathHelper.TwoPi / 5f + rotationAmount);
                    Vector2 imageDrawOffsetB = new Vector2(spawnAnimTime - NPC.localAI[0], 0).RotatedBy(-(i * MathHelper.TwoPi / 5f + rotationAmount));
                    float imageDrawAlphaA = NPC.localAI[0] / (spawnAnimTime * 20f);
                    float imageDrawAlphaB = NPC.localAI[0] / (spawnAnimTime * 10f);

                    Texture2D subTexture = Request<Texture2D>(GetInstance<SolMoth>().Texture).Value;
                    Vector2 subDrawOrigin = new Vector2(subTexture.Width * 0.5f, subTexture.Height * 0.5f / Main.npcFrameCount[NPC.type]);
                    Vector2 subDrawPos = imageDrawOffsetA + NPC.position - Main.screenPosition + subDrawOrigin + new Vector2(0f, NPC.gfxOffY) + new Vector2((NPC.width - subTexture.Width) / 2, -80);
                    spriteBatch.Draw(subTexture, subDrawPos, new Rectangle(0, subTexture.Height * NPC.frame.Y / TextureAssets.Npc[NPC.type].Value.Height, subTexture.Width, subTexture.Height / 8), Color.White * imageDrawAlphaA, NPC.rotation, subDrawOrigin, NPC.scale, SpriteEffects.None, 0f);

                    subTexture = Request<Texture2D>(GetInstance<LunaButterfly>().Texture).Value;
                    subDrawOrigin = new Vector2(subTexture.Width * 0.5f, subTexture.Height * 0.5f / Main.npcFrameCount[NPC.type]);
                    subDrawPos = imageDrawOffsetB + NPC.position - Main.screenPosition + subDrawOrigin + new Vector2(0f, NPC.gfxOffY) + new Vector2((NPC.width - subTexture.Width) / 2, -80);
                    spriteBatch.Draw(subTexture, subDrawPos, new Rectangle(0, subTexture.Height * NPC.frame.Y / TextureAssets.Npc[NPC.type].Value.Height, subTexture.Width, subTexture.Height / 8), Color.White * imageDrawAlphaB, NPC.rotation, subDrawOrigin, NPC.scale, SpriteEffects.None, 0f);
                }

                float mainDrawAlpha = NPC.localAI[0] / spawnAnimTime;

                Texture2D texture = TextureAssets.Npc[NPC.type].Value;

                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f / Main.npcFrameCount[NPC.type]);
                Vector2 drawPos = NPC.position - Main.screenPosition + drawOrigin + new Vector2(0f, NPC.gfxOffY) + new Vector2((NPC.width - texture.Width) / 2, -80);
                spriteBatch.Draw(texture, drawPos, NPC.frame, Color.White * mainDrawAlpha, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);

                Texture2D moon = Request<Texture2D>($"{Texture}Moon").Value;
                Vector2 newDrawOrigin = new Vector2(moon.Width / 2, moon.Height / 2);
                Vector2 newDrawPos = NPC.Center - Main.screenPosition + new Vector2(0f, NPC.gfxOffY);
                spriteBatch.Draw(moon, newDrawPos + moonOffset, new Rectangle(0, 0, moon.Width, moon.Height), Color.White * mainDrawAlpha, NPC.rotation, newDrawOrigin, NPC.scale, SpriteEffects.None, 0f);
            }
            else if (attackPattern == 0 && attackCooldown > 60)
            {
                float animationProgress = attackCooldown - (60 + teleportAnimTime);
                if (attackCooldown < 60 + teleportAnimTime)
                {
                    animationProgress = 60 + teleportAnimTime - attackCooldown;
                }

                for (int i = 0; i <= 100; i++)
                {
                    float rotationAmount = teleportAnimTime / (teleportAnimTime - animationProgress);
                    Vector2 imageDrawOffsetA = new Vector2(2 * (teleportAnimTime - animationProgress), 0).RotatedBy(i * MathHelper.TwoPi / 5f + rotationAmount);
                    Vector2 imageDrawOffsetB = new Vector2(2 * (teleportAnimTime - animationProgress), 0).RotatedBy(-(i * MathHelper.TwoPi / 5f + rotationAmount));
                    float imageDrawAlphaA = animationProgress / (teleportAnimTime * 20f);
                    float imageDrawAlphaB = animationProgress / (teleportAnimTime * 10f);

                    Texture2D subTexture = Request<Texture2D>(GetInstance<SolMoth>().Texture).Value;
                    Vector2 subDrawOrigin = new Vector2(subTexture.Width * 0.5f, subTexture.Height * 0.5f / Main.npcFrameCount[NPC.type]);
                    Vector2 subDrawPos = imageDrawOffsetA + NPC.position - Main.screenPosition + subDrawOrigin + new Vector2(0f, NPC.gfxOffY) + new Vector2((NPC.width - subTexture.Width) / 2, -80);
                    spriteBatch.Draw(subTexture, subDrawPos, new Rectangle(0, subTexture.Height * NPC.frame.Y / TextureAssets.Npc[NPC.type].Value.Height, subTexture.Width, subTexture.Height / 8), Color.White * imageDrawAlphaA, NPC.rotation, subDrawOrigin, NPC.scale, SpriteEffects.None, 0f);

                    subTexture = Request<Texture2D>(GetInstance<LunaButterfly>().Texture).Value;
                    subDrawOrigin = new Vector2(subTexture.Width * 0.5f, subTexture.Height * 0.5f / Main.npcFrameCount[NPC.type]);
                    subDrawPos = imageDrawOffsetB + NPC.position - Main.screenPosition + subDrawOrigin + new Vector2(0f, NPC.gfxOffY) + new Vector2((NPC.width - subTexture.Width) / 2, -80);
                    spriteBatch.Draw(subTexture, subDrawPos, new Rectangle(0, subTexture.Height * NPC.frame.Y / TextureAssets.Npc[NPC.type].Value.Height, subTexture.Width, subTexture.Height / 8), Color.White * imageDrawAlphaB, NPC.rotation, subDrawOrigin, NPC.scale, SpriteEffects.None, 0f);
                }

                float mainDrawAlpha = animationProgress / teleportAnimTime;

                Texture2D texture = TextureAssets.Npc[NPC.type].Value;

                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f / Main.npcFrameCount[NPC.type]);
                Vector2 drawPos = NPC.position - Main.screenPosition + drawOrigin + new Vector2(0f, NPC.gfxOffY) + new Vector2((NPC.width - texture.Width) / 2, -80);
                spriteBatch.Draw(texture, drawPos, NPC.frame, Color.White * mainDrawAlpha, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);

                Texture2D moon = Request<Texture2D>($"{Texture}Moon").Value;
                Vector2 newDrawOrigin = new Vector2(moon.Width / 2, moon.Height / 2);
                Vector2 newDrawPos = NPC.Center - Main.screenPosition + new Vector2(0f, NPC.gfxOffY);
                spriteBatch.Draw(moon, newDrawPos + moonOffset, new Rectangle(0, 0, moon.Width, moon.Height), Color.White * mainDrawAlpha, NPC.rotation, newDrawOrigin, NPC.scale, SpriteEffects.None, 0f);
            }
            else
            {
                if (reformAnimationTimeLeft > 0)
                {
                    Texture2D reformTexture = Request<Texture2D>($"{Texture}ReformingSpark").Value;
                    Vector2 reformDrawOrigin = new Vector2(reformTexture.Width * 0.5f, reformTexture.Height * 0.5f);
                    Vector2 reformDrawPos = NPC.Center - Main.screenPosition + new Vector2(0f, NPC.gfxOffY);

                    float rotation = (reformAnimationTime - reformAnimationTimeLeft) * MathHelper.Pi / reformAnimationTime;
                    float scale = (float)Math.Sin((reformAnimationTime - reformAnimationTimeLeft) * MathHelper.Pi / reformAnimationTime);

                    spriteBatch.Draw(reformTexture, reformDrawPos, new Rectangle(0, 0, reformTexture.Width, reformTexture.Height), Color.White, rotation, reformDrawOrigin, NPC.scale * scale, SpriteEffects.None, 0f);
                    //spriteBatch.Draw(reformTexture, reformDrawPos, new Rectangle(0, 0, reformTexture.Width, reformTexture.Height), Color.White, rotation + MathHelper.PiOver4, reformDrawOrigin, npc.scale * scale, SpriteEffects.None, 0f);
                    spriteBatch.Draw(reformTexture, reformDrawPos, new Rectangle(0, 0, reformTexture.Width, reformTexture.Height), Color.White, -rotation, reformDrawOrigin, NPC.scale * scale, SpriteEffects.None, 0f);
                    //spriteBatch.Draw(reformTexture, reformDrawPos, new Rectangle(0, 0, reformTexture.Width, reformTexture.Height), Color.White, -rotation + MathHelper.PiOver4, reformDrawOrigin, npc.scale * scale, SpriteEffects.None, 0f);
                }

                Texture2D texture = TextureAssets.Npc[NPC.type].Value;

                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f / Main.npcFrameCount[NPC.type]);
                Vector2 drawPos = NPC.position - Main.screenPosition + drawOrigin + new Vector2(0f, NPC.gfxOffY) + new Vector2((NPC.width - texture.Width) / 2, -80);
                spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, drawPos, NPC.frame, Color.White, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);

                Texture2D moon = Request<Texture2D>($"{Texture}Moon").Value;

                Vector2 newDrawOrigin = new Vector2(moon.Width / 2, moon.Height / 2);
                Vector2 newDrawPos = NPC.Center - Main.screenPosition + new Vector2(0f, NPC.gfxOffY);
                spriteBatch.Draw(moon, newDrawPos + moonOffset, new Rectangle(0, 0, moon.Width, moon.Height), Color.White, NPC.rotation, newDrawOrigin, NPC.scale, SpriteEffects.None, 0f);
            }

            return false;
        }

        public override void BossHeadSlot(ref int index)
        {
            if (NPC.hide)
            {
                index = -1;
            }
        }

        public override bool CheckActive()
        {
            return true;
        }

        public override bool CheckDead()
        {
            SoundEngine.PlaySound(SoundID.Item29.WithVolumeScale(1.2f).WithPitchOffset(-0.5f), NPC.Center);
            PolaritiesSystem.downedEclipxie = true;
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.WorldData); // Immediately inform clients of new world state.
            }

            if (Main.expertMode && (attackCooldown == 0 || NPC.hide) && (NPC.life * 4 < NPC.lifeMax * 3 && numSplitPhases == 0 || NPC.life * 2 < NPC.lifeMax && numSplitPhases == 1 || NPC.life * 4 < NPC.lifeMax && numSplitPhases == 2))
            {
                NPC.Center = Vector2.Zero;
                //set the NPC center to whichever of sol moth or luna butterfly is closer to the player
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && Main.npc[i].realLife == NPC.whoAmI && (Main.npc[i].type == NPCType<SolMoth>() || Main.npc[i].type == NPCType<LunaButterfly>()))
                    {
                        if ((Main.npc[i].Center - Main.LocalPlayer.Center).Length() < (NPC.Center - Main.LocalPlayer.Center).Length())
                            NPC.Center = Main.npc[i].Center;
                    }
                }
            }
            else
            {
                for (int i = 1; i <= 2; i++)
                {
                    NPC.DeathGore($"LunaButterfly{i}");
                }
            }

            return true;
        }

        public override void UpdateLifeRegen(ref int damage)
        {
            if (Main.expertMode && (attackCooldown == 0 || NPC.hide) && (NPC.life * 4 < NPC.lifeMax * 3 && numSplitPhases == 0 || NPC.life * 2 < NPC.lifeMax && numSplitPhases == 1 || NPC.life * 4 < NPC.lifeMax && numSplitPhases == 2))
                damage = 0;
        }

        public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
        {
            if (Main.expertMode && (attackCooldown == 0 || NPC.hide) && (NPC.life * 4 < NPC.lifeMax * 3 && numSplitPhases == 0 || NPC.life * 2 < NPC.lifeMax && numSplitPhases == 1 || NPC.life * 4 < NPC.lifeMax && numSplitPhases == 2))
                return false;
            return true;
        }

        public override bool? CanBeHitByItem(Player player, Item item)
        {
            if (Main.expertMode && (attackCooldown == 0 || NPC.hide) && (NPC.life * 4 < NPC.lifeMax * 3 && numSplitPhases == 0 || NPC.life * 2 < NPC.lifeMax && numSplitPhases == 1 || NPC.life * 4 < NPC.lifeMax && numSplitPhases == 2))
                return false;
            return null;
        }

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if (Main.expertMode && (attackCooldown == 0 || NPC.hide) && (NPC.life * 4 < NPC.lifeMax * 3 && numSplitPhases == 0 || NPC.life * 2 < NPC.lifeMax && numSplitPhases == 1 || NPC.life * 4 < NPC.lifeMax && numSplitPhases == 2))
                return false;
            return null;
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(attackPattern);
            writer.Write(attackSubPattern);
            writer.Write(attackCooldown);
            writer.Write(patience);
            writer.WriteVector2(targetDiff);
            writer.WriteVector2(targetPosition);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            attackPattern = reader.ReadInt32();
            attackSubPattern = reader.ReadInt32();
            attackCooldown = reader.ReadInt32();
            patience = reader.ReadInt32();
            targetDiff = reader.ReadVector2();
            targetPosition = reader.ReadVector2();
        }

        public override void OnKill()
        {
            //if(Main.rand.NextBool(10) || NPC.GetGlobalNPC<PolaritiesNPC>().noHit) {
            //	Item.NewItem(NPC.getRect(), ItemType<EclipsePixieTrophy>());
            //}
            //         if (Main.expertMode)
            //         {
            //             NPC.DropBossBags();
            //         }
            //         else
            //         {
            //             if (Main.rand.NextBool(7))
            //             {
            //                 Item.NewItem(NPC.getRect(), ItemType<EclipxieMask>());
            //             }
            //             Item.NewItem(NPC.getRect(), ItemType<EclipxieDust>(), Main.rand.Next(10, 21));
            //             if (Main.rand.NextBool(5))
            //             {
            //                 Item.NewItem(NPC.getRect(), ItemType<SolarPendant>());
            //             }
            //             switch (Main.rand.Next(4))
            //             {
            //                 case 0:
            //                     Item.NewItem(NPC.getRect(), ItemType<BlackLight>());
            //                     break;
            //                 case 1:
            //                     Item.NewItem(NPC.getRect(), ItemType<Sunsliver>());
            //                     break;
            //                 case 2:
            //                     Item.NewItem(NPC.getRect(), ItemType<SolarEyeStaff>());
            //                     break;
            //                 case 3:
            //                     if (Main.rand.NextBool(2))
            //                     {
            //                         Item.NewItem(NPC.getRect(), ItemType<Items.Weapons.SunDisc>());
            //                     }
            //                     else
            //                     {
            //                         Item.NewItem(NPC.getRect(), ItemType<Items.Weapons.Lunarang>());
            //                     }
            //                     break;
            //             }
            //             if (Main.rand.NextBool(15))
            //             {
            //                 Item.NewItem(NPC.getRect(), ItemType<Items.Accessories.Wings.EclipxieWings>());
            //             }
            //         }
        }
    }

    [AutoloadBossHead]
    public class SolMoth : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sol Moth");
            Main.npcFrameCount[NPC.type] = 8;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 64;
            NPC.height = 64;
            NPC.defense = 45;
            NPC.damage = 70;
            NPC.lifeMax = 60000;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 5, 0, 0);
            NPC.npcSlots = 15f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit5;

            NPC.dontTakeDamage = false;
            NPC.chaseable = true;

            NPC.buffImmune[BuffID.Confused] = true;
            NPC.buffImmune[BuffID.OnFire] = true;

            Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Eclipxie");
        }

        public override void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit)
        {
            damage /= 2;
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            damage /= 2;
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.6f * bossLifeScale);
        }

        private Vector2 orbitCenter;
        private float orbitRotation;

        private void GoTowardsRadial(Vector2 goalPosition, Vector2 orbitPoint, float timeLeft, float maxSpeed = 24f)
        {
            float dRadial = (goalPosition - orbitPoint).Length() - (NPC.Center - orbitPoint).Length();
            float dAngle = (goalPosition - orbitPoint).ToRotation() - (NPC.Center - orbitPoint).ToRotation();
            while (dAngle > MathHelper.Pi)
            {
                dAngle -= MathHelper.TwoPi;
            }
            while (dAngle < -MathHelper.Pi)
            {
                dAngle += MathHelper.TwoPi;
            }

            NPC.velocity = (new Vector2(dRadial, dAngle * (NPC.Center - orbitPoint).Length()).RotatedBy((NPC.Center - orbitPoint).ToRotation()) + (goalPosition - NPC.Center)) / 2 / timeLeft;

            if (NPC.velocity.Length() > maxSpeed)
            {
                NPC.velocity.Normalize();
                NPC.velocity *= maxSpeed;
            }
        }

        public override void AI()
        {
            if (NPC.localAI[0] == 0)
            {
                NPC.localAI[0] = 1;
                NPC.netUpdate = true;
            }
            Player player = Main.player[NPC.target];
            if (!player.active || player.dead || !Main.eclipse)
            {
                NPC.TargetClosest(false);
                NPC.netUpdate = true;
                player = Main.player[NPC.target];
                if (player.dead || !Main.eclipse)
                {
                    if (NPC.timeLeft > 10)
                    {
                        NPC.timeLeft = 10;
                    }
                    NPC.velocity.Y -= 0.1f;
                    return;
                }
            }

            if (!Main.npc[NPC.realLife].active)
            {
                NPC.life = 0;
                NPC.checkDead();
                return;
            }

            if (NPC.ai[0] < 300)
            {
                //spin attack
                if (NPC.ai[0] == 0)
                {
                    //initialize to spin around this point
                    orbitCenter = NPC.Center;
                    orbitRotation = (orbitCenter - player.Center).ToRotation();
                }

                if ((orbitCenter - player.Center).Length() > 1200)
                {
                    orbitCenter = player.Center + (orbitCenter - player.Center).SafeNormalize(Vector2.Zero) * 1200;
                }

                float angle = orbitRotation + (float)Math.Cos(MathHelper.Pi * NPC.ai[0] / 300f) * MathHelper.Pi * 5;
                float distance = (float)Math.Sin(MathHelper.PiOver2 * NPC.ai[0] / 300f) * 160;

                NPC.velocity = orbitCenter + new Vector2(distance, 0).RotatedBy(angle) - NPC.Center;
                if (NPC.ai[0] % 2 == 1)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(12, 0).RotatedBy(angle), ProjectileType<EclipxieRay>(), 25, 0, Main.myPlayer, 1, 0);
                    SoundEngine.PlaySound(SoundID.Item33, NPC.position);
                }
            }
            else if (NPC.ai[0] < 780)
            {
                //hover over player and shoot flares
                Vector2 goalPosition = player.Center + new Vector2(0, -500);
                //Vector2 goalVelocity = (goalPosition - npc.Center) / 15; //(goalPosition - npc.Center).SafeNormalize(Vector2.Zero) * 16;
                //npc.velocity += (goalVelocity - npc.velocity) / 15;

                Vector2 currentVelocity = NPC.velocity;
                GoTowardsRadial(goalPosition, player.Center, 15f, maxSpeed: float.PositiveInfinity);
                Vector2 goalVelocity = NPC.velocity;
                NPC.velocity = currentVelocity;
                NPC.velocity += (goalVelocity - NPC.velocity) / 15;

                if (NPC.ai[0] > 420)
                {
                    if (NPC.ai[0] % 30 == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item109, NPC.Center);
                        int shot = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 4).RotatedBy(MathHelper.PiOver4), ProjectileType<EclipseFlareBolt>(), 1, 0, Main.myPlayer, 0, 0);
                        Main.projectile[shot].timeLeft = 60;
                        Main.projectile[shot].tileCollide = false;
                    }
                    else if (NPC.ai[0] % 30 == 15)
                    {
                        SoundEngine.PlaySound(SoundID.Item109, NPC.Center);
                        int shot = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 4).RotatedBy(-MathHelper.PiOver4), ProjectileType<EclipseFlareBolt>(), 1, 0, Main.myPlayer, 0, 0);
                        Main.projectile[shot].timeLeft = 60;
                        Main.projectile[shot].tileCollide = false;
                    }
                }
            }
            else if (NPC.ai[0] < 840)
            {
                //move to position for deathray
                Vector2 goalPosition = player.Center + new Vector2(-600, -200);
                //npc.velocity = (goalPosition - npc.Center) / (840 - npc.ai[0]);

                GoTowardsRadial(goalPosition, player.Center, 840 - NPC.ai[0], maxSpeed: float.PositiveInfinity);
            }
            else if (NPC.ai[0] < 870)
            {
                NPC.velocity = Vector2.Zero;

                if (NPC.ai[0] == 840)
                {
                    SoundEngine.PlaySound(SoundID.Item109, NPC.Center);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, 1), ProjectileType<SunMothDeathray>(), 40, 2f, Main.myPlayer, ai1: NPC.whoAmI);
                }
            }
            else if (NPC.ai[0] < 1110)
            {
                NPC.velocity.X += 0.1f;
            }
            else if (NPC.ai[0] < 1260)
            {
                //move to home
                Vector2 goalPosition = player.Center + new Vector2(0, -400);
                //npc.velocity = (goalPosition - npc.Center) / (1260 - npc.ai[0]);
                GoTowardsRadial(goalPosition, player.Center, 1260 - NPC.ai[0], maxSpeed: float.PositiveInfinity);
            }
            else
            {
                NPC.active = false;
            }

            //continuously increase this timer
            NPC.ai[0]++;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter = (NPC.frameCounter + 1) % 5;
            if (NPC.frameCounter == 0)
            {
                NPC.frame.Y = (NPC.frame.Y + frameHeight) % (8 * frameHeight);
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 300, true);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f / Main.npcFrameCount[NPC.type]);
            Vector2 drawPos = NPC.position - Main.screenPosition + drawOrigin + new Vector2(0f, NPC.gfxOffY) + new Vector2((NPC.width - texture.Width) / 2, -80);
            spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, drawPos, NPC.frame, Color.White, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override bool CheckActive()
        {
            return true;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (NPC.life <= 0)
            {
                for (int i = 1; i <= 2; i++)
                {
                    NPC.DeathGore($"SolMothGore{i}");
                }
            }
        }
    }

    [AutoloadBossHead]
    public class LunaButterfly : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 8;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 64;
            NPC.height = 64;
            NPC.defense = 45;
            NPC.damage = 70;
            NPC.lifeMax = 60000;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(0, 5, 0, 0);
            NPC.npcSlots = 15f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit5;

            NPC.dontTakeDamage = false;
            NPC.chaseable = true;

            NPC.buffImmune[BuffID.Confused] = true;
            NPC.buffImmune[BuffID.Frostburn] = true;

            Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Eclipxie");
        }

        public override void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit)
        {
            damage /= 2;
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            damage /= 2;
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.6f * bossLifeScale);
        }

        private Vector2 orbitCenter;
        private float orbitRotation;

        private void GoTowardsRadial(Vector2 goalPosition, Vector2 orbitPoint, float timeLeft, float maxSpeed = 24f)
        {
            float dRadial = (goalPosition - orbitPoint).Length() - (NPC.Center - orbitPoint).Length();
            float dAngle = (goalPosition - orbitPoint).ToRotation() - (NPC.Center - orbitPoint).ToRotation();
            while (dAngle > MathHelper.Pi)
            {
                dAngle -= MathHelper.TwoPi;
            }
            while (dAngle < -MathHelper.Pi)
            {
                dAngle += MathHelper.TwoPi;
            }

            NPC.velocity = (new Vector2(dRadial, dAngle * (NPC.Center - orbitPoint).Length()).RotatedBy((NPC.Center - orbitPoint).ToRotation()) + (goalPosition - NPC.Center)) / 2 / timeLeft;

            if (NPC.velocity.Length() > maxSpeed)
            {
                NPC.velocity.Normalize();
                NPC.velocity *= maxSpeed;
            }
        }

        public override void AI()
        {
            if (NPC.localAI[0] == 0)
            {
                NPC.localAI[0] = 1;
                NPC.netUpdate = true;
            }
            Player player = Main.player[NPC.target];
            if (!player.active || player.dead || !Main.eclipse)
            {
                NPC.TargetClosest(false);
                NPC.netUpdate = true;
                player = Main.player[NPC.target];
                if (player.dead || !Main.eclipse)
                {
                    if (NPC.timeLeft > 10)
                    {
                        NPC.timeLeft = 10;
                    }
                    NPC.velocity.Y -= 0.1f;
                    return;
                }
            }

            if (!Main.npc[NPC.realLife].active)
            {
                NPC.life = 0;
                NPC.checkDead();
                return;
            }

            if (NPC.ai[0] < 300)
            {
                //spin attack
                if (NPC.ai[0] == 0)
                {
                    //initialize to spin around this point
                    orbitCenter = NPC.Center;
                    orbitRotation = (orbitCenter - player.Center).ToRotation();
                }

                if ((orbitCenter - player.Center).Length() > 1200)
                {
                    orbitCenter = player.Center + (orbitCenter - player.Center).SafeNormalize(Vector2.Zero) * 1200;
                }

                float angle = orbitRotation + MathHelper.Pi + (float)Math.Cos(MathHelper.Pi * NPC.ai[0] / 300f) * MathHelper.Pi * 5;
                float distance = (float)Math.Sin(MathHelper.PiOver2 * NPC.ai[0] / 300f) * 160;

                NPC.velocity = orbitCenter + new Vector2(distance, 0).RotatedBy(angle) - NPC.Center;
                if (NPC.ai[0] % 20 == 1)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 0.005f, ProjectileType<MoonButterflyScythe>(), 25, 0, Main.myPlayer, 1, 0);
                }
            }
            else if (NPC.ai[0] < 780)
            {
                if (NPC.ai[0] == 420 || NPC.ai[0] == 540 || NPC.ai[0] == 660)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<LunarCorona>(), 40, 0f, Main.myPlayer, ai0: NPC.whoAmI, ai1: 1);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<LunarCorona>(), 40, 0f, Main.myPlayer, ai0: NPC.whoAmI, ai1: -1);
                }
                Target(player.Center);
            }
            else if (NPC.ai[0] < 840)
            {
                //move to position for deathray
                Vector2 goalPosition = player.Center + new Vector2(600, 200);
                //npc.velocity = (goalPosition - npc.Center) / (840 - npc.ai[0]);
                GoTowardsRadial(goalPosition, player.Center, 840 - NPC.ai[0], maxSpeed: float.PositiveInfinity);
            }
            else if (NPC.ai[0] < 870)
            {
                NPC.velocity = Vector2.Zero;

                if (NPC.ai[0] == 840)
                {
                    SoundEngine.PlaySound(SoundID.Item109, NPC.Center);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -1), ProjectileType<MoonButterflyDeathray>(), 40, 2f, Main.myPlayer, ai1: NPC.whoAmI);
                }
            }
            else if (NPC.ai[0] < 1110)
            {
                NPC.velocity.X -= 0.1f;
            }
            else if (NPC.ai[0] < 1260)
            {
                //move to home
                Vector2 goalPosition = player.Center + new Vector2(0, -400);
                //npc.velocity = (goalPosition - npc.Center) / (1260 - npc.ai[0]);
                GoTowardsRadial(goalPosition, player.Center, 1260 - NPC.ai[0], maxSpeed: float.PositiveInfinity);
            }
            else
            {
                NPC.active = false;
            }

            //continuously increase this timer
            NPC.ai[0]++;
        }

        private void Target(Vector2 position)
        {
            float speed = (float)Math.Max(0.1f, Math.Sqrt(NPC.velocity.X * NPC.velocity.X + NPC.velocity.Y * NPC.velocity.Y));
            float dist = (float)Math.Max(0.1f, Math.Sqrt((position.X - NPC.Center.X) * (position.X - NPC.Center.X) + (position.Y - NPC.Center.Y) * (position.Y - NPC.Center.Y)));

            float vectorAngle = 0.1f + (float)Math.Acos(Math.Min(1, Math.Max(-1, NPC.velocity.X / speed * (position.X - NPC.Center.X) / dist + NPC.velocity.Y / speed * (position.Y - NPC.Center.Y) / dist)));

            NPC.velocity.X *= 0.99f;
            NPC.velocity.Y *= 0.99f;

            NPC.velocity.X += 0.02f * vectorAngle * (position.X - NPC.Center.X) / (1 + speed);
            NPC.velocity.Y += 0.02f * vectorAngle * (position.Y - NPC.Center.Y) / (1 + speed);
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter = (NPC.frameCounter + 1) % 5;
            if (NPC.frameCounter == 0)
            {
                NPC.frame.Y = (NPC.frame.Y + frameHeight) % (8 * frameHeight);
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Frostburn, 300, true);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f / Main.npcFrameCount[NPC.type]);
            Vector2 drawPos = NPC.position - Main.screenPosition + drawOrigin + new Vector2(0f, NPC.gfxOffY) + new Vector2((NPC.width - texture.Width) / 2, -80);
            spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, drawPos, NPC.frame, Color.White, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override bool CheckActive()
        {
            return true;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (NPC.life <= 0)
            {
                for (int i = 1; i <= 2; i++)
                {
                    NPC.DeathGore($"LunaButterflyGore{i}");
                }
            }
        }
    }

    public class EclipxieDeathray : ModProjectile
    {
        private float Distance;
        private int frame = 9;
        private int timer;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Eclipse Deathray");
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = false;
            Projectile.timeLeft = 150;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawLaser(Main.spriteBatch, TextureAssets.Projectile[Projectile.type].Value, Projectile.Center, new Vector2(0, 1), 32, Projectile.damage, (float)Math.PI / 2);
            return false;
        }

        // The core function of drawing a laser
        public void DrawLaser(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 unit, float step, int damage, float rotation = 0f, float scale = 1f, float maxDist = 2000f, Color color = default(Color), int transDist = 0)
        {
            float r = unit.ToRotation() + rotation;

            // Draws the laser 'body'
            for (float i = transDist; i <= Distance; i += step)
            {
                Color c = Color.White;
                var origin = start + i * unit;
                spriteBatch.Draw(texture, origin - Main.screenPosition,
                    new Rectangle(0, 32 * frame, 64, 32), i < transDist ? Color.Transparent : c, r,
                    new Vector2(64 * .5f, 32), scale, 0, 0);
            }
        }

        // Change the way of collision check of the projectile
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.timeLeft >= 120)
            {
                return false;
            }
            Vector2 unit = new Vector2(0, 1);
            float point = 0f;
            // Run an AABB versus Line check to look for collisions, look up AABB collision first to see how it works
            // It will look for collisions on the given line using AABB
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
                Projectile.Center + unit * Distance, 64, ref point);
        }

        // The AI of the projectile
        public override void AI()
        {
            Distance = 16 * Main.maxTilesY - Projectile.position.Y;
            CastLights();
            if (Projectile.timeLeft >= 120)
            {
                frame = 4;
                if (Projectile.timeLeft == 120)
                {
                    SoundEngine.PlaySound(SoundID.Item122, Projectile.position);
                }
            }
            else
            {
                if (timer == 0)
                {
                    frame = (frame + 1) % 4;
                }
                timer = (timer + 1) % 3;
            }

            NPC npc = Main.npc[(int)Projectile.ai[1]];

            if (!npc.active)
            {
                Projectile.active = false;
            }

            Projectile.position = npc.Center + (Projectile.position - Projectile.Center);
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 300, true);
            target.AddBuff(BuffID.Frostburn, 300, true);
        }

        private void CastLights()
        {
            // Cast a light along the line of the laser
            DelegateMethods.v3_1 = new Vector3(0.8f, 0.8f, 1f);
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * (Distance - 0), 26, DelegateMethods.CastLight);
        }

        public override bool ShouldUpdatePosition() => false;
    }

    public class SunMothDeathray : ModProjectile
    {
        private float Distance;
        private int frame = 9;
        private int timer;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Solar Deathray");
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = false;
            Projectile.timeLeft = 270;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawLaser(Main.spriteBatch, TextureAssets.Projectile[Projectile.type].Value, Projectile.Center, new Vector2(0, 1), 32, Projectile.damage, (float)Math.PI / 2);
            return false;
        }

        // The core function of drawing a laser
        public void DrawLaser(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 unit, float step, int damage, float rotation = 0f, float scale = 1f, float maxDist = 2000f, Color color = default(Color), int transDist = 0)
        {
            float r = unit.ToRotation() + rotation;

            // Draws the laser 'body'
            for (float i = transDist; i <= Distance; i += step)
            {
                Color c = Color.White;
                var origin = start + i * unit;
                spriteBatch.Draw(texture, origin - Main.screenPosition,
                    new Rectangle(0, 32 * frame, 64, 32), i < transDist ? Color.Transparent : c, r,
                    new Vector2(64 * .5f, 32), scale, 0, 0);
            }
        }

        // Change the way of collision check of the projectile
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.timeLeft >= 240)
            {
                return false;
            }
            Vector2 unit = new Vector2(0, 1);
            float point = 0f;
            // Run an AABB versus Line check to look for collisions, look up AABB collision first to see how it works
            // It will look for collisions on the given line using AABB
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
                Projectile.Center + unit * Distance, 64, ref point);
        }

        // The AI of the projectile
        public override void AI()
        {
            Distance = 16 * Main.maxTilesY - Projectile.position.Y;
            CastLights();
            if (Projectile.timeLeft >= 240)
            {
                frame = 4;
                if (Projectile.timeLeft == 240)
                {
                    SoundEngine.PlaySound(SoundID.Item122, Projectile.position);
                }
            }
            else
            {
                if (timer == 0)
                {
                    frame = (frame + 1) % 4;
                }
                timer = (timer + 1) % 3;
            }

            NPC npc = Main.npc[(int)Projectile.ai[1]];

            if (!npc.active)
            {
                Projectile.active = false;
            }

            Projectile.position = npc.Center + (Projectile.position - Projectile.Center);
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 600, true);
        }

        private void CastLights()
        {
            // Cast a light along the line of the laser
            DelegateMethods.v3_1 = new Vector3(0.8f, 0.8f, 1f);
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * (Distance - 0), 26, DelegateMethods.CastLight);
        }

        public override bool ShouldUpdatePosition() => false;
    }

    public class MoonButterflyDeathray : ModProjectile
    {
        private float Distance;
        private int frame = 9;
        private int timer;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lunar Deathray");
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = false;
            Projectile.timeLeft = 270;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawLaser(Main.spriteBatch, TextureAssets.Projectile[Projectile.type].Value, Projectile.Center, new Vector2(0, -1), 32, Projectile.damage, (float)Math.PI / 2);
            return false;
        }

        // The core function of drawing a laser
        public void DrawLaser(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 unit, float step, int damage, float rotation = 0f, float scale = 1f, float maxDist = 2000f, Color color = default(Color), int transDist = 0)
        {
            float r = unit.ToRotation() + rotation;

            // Draws the laser 'body'
            for (float i = transDist; i <= Distance; i += step)
            {
                Color c = Color.White;
                var origin = start + i * unit;
                spriteBatch.Draw(texture, origin - Main.screenPosition,
                    new Rectangle(0, 32 * frame, 64, 32), i < transDist ? Color.Transparent : c, r,
                    new Vector2(64 * .5f, 32), scale, 0, 0);
            }
        }

        // Change the way of collision check of the projectile
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.timeLeft >= 240)
            {
                return false;
            }
            Vector2 unit = new Vector2(0, -1);
            float point = 0f;
            // Run an AABB versus Line check to look for collisions, look up AABB collision first to see how it works
            // It will look for collisions on the given line using AABB
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
                Projectile.Center + unit * Distance, 64, ref point);
        }

        // The AI of the projectile
        public override void AI()
        {
            Distance = 16 * Main.maxTilesY - Projectile.position.Y;
            CastLights();
            if (Projectile.timeLeft >= 240)
            {
                frame = 4;
                if (Projectile.timeLeft == 240)
                {
                    SoundEngine.PlaySound(SoundID.Item122, Projectile.position);
                }
            }
            else
            {
                if (timer == 0)
                {
                    frame = (frame + 1) % 4;
                }
                timer = (timer + 1) % 3;
            }

            NPC npc = Main.npc[(int)Projectile.ai[1]];

            if (!npc.active)
            {
                Projectile.active = false;
            }

            Projectile.position = npc.Center + (Projectile.position - Projectile.Center);
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Frostburn, 600, true);
        }

        private void CastLights()
        {
            // Cast a light along the line of the laser
            DelegateMethods.v3_1 = new Vector3(0.8f, 0.8f, 1f);
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * (Distance - 0), 26, DelegateMethods.CastLight);
        }

        public override bool ShouldUpdatePosition() => false;
    }

    public class MoonButterflyScythe : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Moon Scythe");
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.aiStyle = -1;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 3600;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.light = 1f;
        }

        public override void AI()
        {
            Projectile.rotation += 0.2f + Projectile.velocity.Length() / 4;
            Projectile.velocity *= 1.05f;
            if (Projectile.velocity.Length() > 32)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= 32;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            return true;
        }
        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Color mainColor = Color.White;

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), mainColor, Projectile.rotation, new Vector2(texture.Width / 2, texture.Height / Main.projFrames[Projectile.type] / 2), Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            return false;
        }
    }
}