using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.NPCs.Enemies.Fractal.PostSentinel
{
    public class ChaosCrawler : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 3;
            NPCID.Sets.DebuffImmunitySets[Type] = new NPCDebuffImmunityData()
            {
                SpecificallyImmuneTo = new int[]
                {
                    BuffID.Confused,
                    BuffID.OnFire,
                    BuffID.OnFire3,
                    BuffID.Frostburn,
                    BuffID.Frostburn2,
                    BuffID.CursedInferno,
                    BuffID.ShadowFlame,
                    BuffID.Poisoned,
                    BuffID.Venom,
                }
            };
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 34;
            NPC.height = 34;

            NPC.defense = 36;
            NPC.damage = 50;
            NPC.lifeMax = 20000;
            NPC.knockBackResist = 0f;
            NPC.value = 5000;
            NPC.npcSlots = 1f;
            NPC.behindTiles = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.rarity = 1;

            //Banner = NPC.type;
            //BannerItem = ItemType<ChaosCrawlerBanner>();

            NPC.GetGlobalNPC<PolaritiesNPC>().usesProjectileHitCooldowns = true;
            NPC.GetGlobalNPC<PolaritiesNPC>().projectileHitCooldownTime = 10;
        }

        private Vector2[] segmentPositions = new Vector2[20 * 17];
        private int[] hitBoxSegmentIds = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        private bool[] segmentActive = new bool[20];

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

                segmentPositions[0] = NPC.Center + new Vector2(0, NPC.height / 2 - 2).RotatedBy(NPC.rotation);
                for (int i = 1; i < segmentPositions.Length; i++)
                {
                    segmentPositions[i] = segmentPositions[i - 1] + new Vector2(0, NPC.height).RotatedBy(NPC.rotation);
                }

                for (int i = 0; i < 8; i++)
                {
                    legPositions[i] = NPC.Center;
                }
            }

            //changeable ai values
            bool priorActive = false;
            float rotationFade = 17f;
            float rotationAmount = 0.5f;

            //overall AI
            switch (NPC.ai[0])
            {
                case 0:
                    //basic placeholder motion code
                    NPC.velocity += ((player.Center - NPC.Center) / 30 - NPC.velocity) / 30;
                    NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;

                    NPC.ai[1]++;
                    if (NPC.ai[1] == 128)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = Main.rand.Next(new int[] { 1, 2, 3, 4 });
                    }
                    break;
                case 1:
                    //cellular automaton
                    priorActive = true;

                    NPC.velocity *= 0.95f;

                    if (NPC.ai[1] % 32 == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item34, NPC.Center);
                    }

                    NPC.ai[1]++;
                    if (NPC.ai[1] == 128)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = Main.rand.Next(new int[] { 0, 2, 3, 4 });
                    }
                    break;
                case 2:
                    //spin attack
                    NPC.velocity = (player.Center - NPC.Center) / 240 + new Vector2(4, 0).RotatedBy(NPC.rotation - MathHelper.PiOver2).RotatedBy(MathHelper.TwoPi / 64);
                    NPC.rotation += MathHelper.TwoPi / 64;

                    rotationAmount = 0.75f + (player.Center - NPC.Center).Length() / 720;

                    NPC.ai[1]++;
                    if (NPC.ai[1] == 128)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = Main.rand.Next(new int[] { 0, 1, 3, 4 });
                    }
                    break;
                case 3:
                    //charge
                    if (NPC.ai[1] < 16 || NPC.ai[1] >= 64 && NPC.ai[1] < 80)
                    {
                        NPC.velocity += ((player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 16 - NPC.velocity) / 10;
                        NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                    }
                    else if (NPC.ai[1] >= 48 && NPC.ai[1] < 64 || NPC.ai[1] >= 112)
                    {
                        NPC.velocity *= 0.9f;
                    }

                    NPC.ai[1]++;
                    if (NPC.ai[1] == 128)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = Main.rand.Next(new int[] { 0, 1, 2, 4 });
                    }
                    break;
                case 4:
                    //omg 1.4 summoner
                    NPC.velocity = new Vector2(0, -0.5f).RotatedBy(NPC.rotation);

                    float angleGoal = (NPC.Center - player.Center).ToRotation();
                    float maxTurn = 0.1f * (128 - NPC.ai[1]) / 128;
                    rotationAmount = 0.5f + 16 * (NPC.ai[1] * NPC.ai[1] * NPC.ai[1]) / (128 * 128 * 128);

                    float angleDiff = angleGoal - (NPC.rotation - MathHelper.PiOver2);
                    while (angleDiff > MathHelper.Pi)
                    {
                        angleDiff -= MathHelper.TwoPi;
                    }
                    while (angleDiff < -MathHelper.Pi)
                    {
                        angleDiff += MathHelper.TwoPi;
                    }
                    if (angleDiff > maxTurn)
                    {
                        NPC.rotation += maxTurn;
                    }
                    else if (angleDiff < -maxTurn)
                    {
                        NPC.rotation -= maxTurn;
                    }
                    else
                    {
                        NPC.rotation = angleGoal + MathHelper.PiOver2;
                    }

                    NPC.ai[1]++;
                    if (NPC.ai[1] == 128)
                    {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = Main.rand.Next(new int[] { 0, 1, 2, 3 });
                    }
                    break;
            }

            //update segment cellular automaton
            NPC.frameCounter++;
            if (NPC.frameCounter == 4)
            {
                NPC.frameCounter = 0;

                for (int i = segmentActive.Length - 1; i >= 0; i--)
                {
                    if (i == 0)
                    {
                        segmentActive[i] = priorActive ^ segmentActive[i];
                    }
                    else
                    {
                        segmentActive[i] = segmentActive[i - 1] ^ segmentActive[i];
                    }

                    if (segmentActive[i] && Main.netMode != 1)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), segmentPositions[17 * i + 8], (player.Center - segmentPositions[17 * i + 8]).SafeNormalize(Vector2.Zero) * 2f, ProjectileType<ChaosBolt>(), 24, 3f, Main.myPlayer);
                    }
                }
            }

            //update segment positions
            segmentPositions[0] = NPC.Center + NPC.velocity + new Vector2(0, NPC.height / 2 - 2).RotatedBy(NPC.rotation);
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
                Vector2 spot = segmentPositions[h * 17 + 8];

                if (hitBoxSegmentIds[h] == -1 || !Main.npc[hitBoxSegmentIds[h]].active || Main.npc[hitBoxSegmentIds[h]].type != NPCType<ChaosCrawlerHitbox>())
                {
                    if (Main.netMode != 1)
                    {
                        hitBoxSegmentIds[h] = NPC.NewNPC(NPC.GetSource_FromThis(), (int)spot.X, (int)spot.Y, NPCType<ChaosCrawlerHitbox>(), ai0: NPC.whoAmI, ai1: h * 17 + 8);
                        NPC.netUpdate = true;
                    }
                }
                else
                {
                    Main.npc[hitBoxSegmentIds[h]].scale = (segmentPositions.Length - (h * 17 + 8)) / (float)(segmentPositions.Length - 1);
                    Main.npc[hitBoxSegmentIds[h]].width = (int)(34 * Main.npc[hitBoxSegmentIds[h]].scale);
                    Main.npc[hitBoxSegmentIds[h]].height = (int)(34 * Main.npc[hitBoxSegmentIds[h]].scale);

                    Main.npc[hitBoxSegmentIds[h]].Center = spot;
                    Main.npc[hitBoxSegmentIds[h]].timeLeft = 10;
                    Main.npc[hitBoxSegmentIds[h]].dontTakeDamage = NPC.dontTakeDamage;
                    Main.npc[hitBoxSegmentIds[h]].defense = NPC.defense;
                    Main.npc[hitBoxSegmentIds[h]].defDefense = NPC.defDefense;
                    Main.npc[hitBoxSegmentIds[h]].HitSound = NPC.HitSound;
                }
            }

            UpdateLegData();
        }

        private Vector2[] legPositions = new Vector2[8];
        private bool[] legStates = new bool[8];

        private void UpdateLegData()
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 center;
                if (i % 2 == 0)
                {
                    center = NPC.Center + new Vector2(0, 17).RotatedBy(NPC.rotation) + NPC.velocity;
                }
                else
                {
                    center = NPC.Center + new Vector2(0, 17).RotatedBy(NPC.rotation) + NPC.velocity;
                }

                int xOffset = i % 2 == 0 ? 72 : -72;
                int yOffset = i / 2 * 28 - 36;

                Vector2 legBasePosition = NPC.Center + new Vector2(xOffset, yOffset).RotatedBy(NPC.rotation);

                if (legStates[i]) //this is if the leg is in its stationary state
                {
                    if ((legBasePosition - legPositions[i]).Length() > 64)
                    {
                        legStates[i] = false;
                    }
                }
                else //this is if the leg is in its moving state
                {
                    if ((legBasePosition - legPositions[i]).Length() < 8)
                    {
                        legPositions[i] = legBasePosition;
                        legStates[i] = true;
                    }
                    else
                    {

                        float dRadius = (legBasePosition - center).Length() - (legPositions[i] - center).Length();
                        float dAngle = (legBasePosition - center).ToRotation() - (legPositions[i] - center).ToRotation();

                        while (dAngle > MathHelper.Pi)
                        {
                            dAngle -= MathHelper.TwoPi;
                        }
                        while (dAngle < -MathHelper.Pi)
                        {
                            dAngle += MathHelper.TwoPi;
                        }

                        legPositions[i] += new Vector2(dRadius, dAngle * (legPositions[i] - center).Length()).RotatedBy((legPositions[i] - center).ToRotation()).SafeNormalize(Vector2.Zero) * 8;

                        //legPositions[i] += (legBasePosition - legPositions[i]).SafeNormalize(Vector2.Zero) * 4;
                    }
                    legPositions[i] += NPC.velocity;
                }

                if ((legPositions[i] - center).Length() >= 100)
                {
                    legPositions[i] = center + (legPositions[i] - center).SafeNormalize(Vector2.Zero) * 100;
                    legStates[i] = false;
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            //if (Subworld.IsActive<FractalSubworld>() && spawnInfo.Player.GetModPlayer<PolaritiesPlayer>().GetFractalization() > FractalSubworld.POST_GOLEM_TIME)
            //{
            //    return 0.15f * FractalSubworld.SpawnConditionFractalWastes(spawnInfo) * FractalSubworld.SpawnConditionFractalUnderground(spawnInfo);
            //}
            return 0f;
        }

        public override bool CheckDead()
        {
            //Gore.NewGore(NPC.Center, NPC.velocity, Mod.GetGoreSlot("Gores/ChaosCrawlerGore"));
            for (int i = 0; i < segmentPositions.Length; i++)
            {
                if (Main.rand.NextFloat() < 1 - i / (float)segmentPositions.Length)
                {
                    Dust.NewDustDirect(segmentPositions[i], 1, 1, 134, newColor: Color.Pink, Scale: 1f).noGravity = true;
                }
            }

            return true;
        }

        public override void OnKill()
        {
            //Item.NewItem(NPC.Hitbox, ItemType<SelfsimilarOre>(), Main.rand.Next(1, 3));
            //if (Main.rand.NextBool(3))
            //{
            //    Item.NewItem(NPC.getRect(), ItemType<Items.FractalKey>());
            //}
            //if (Main.rand.NextBool(20))
            //{
            //    Item.NewItem(NPC.getRect(), ItemType<Items.Weapons.GliderGun>());
            //}
            //if (Main.rand.NextBool(20))
            //{
            //    Item.NewItem(NPC.getRect(), ItemID.RodofDiscord);
            //}
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var legTexture = ModContent.Request<Texture2D>($"{Texture}Leg");
            //draw legs
            Vector2 center;

            for (int i = 0; i < 8; i++)
            {
                if (i % 2 == 0)
                {
                    Vector2 drawPosition = legPositions[i] - Main.screenPosition;

                    center = NPC.Center - Main.screenPosition + new Vector2(0, 17).RotatedBy(NPC.rotation);

                    int segmentLength = 54;

                    float rotation = (center - drawPosition).ToRotation() + MathHelper.PiOver2 + (float)Math.Acos(((drawPosition - center) / 2).Length() / segmentLength);
                    float rotation2 = (center - drawPosition).ToRotation() + MathHelper.PiOver2 - (float)Math.Acos(((drawPosition - center) / 2).Length() / segmentLength);

                    spriteBatch.Draw(legTexture.Value, drawPosition,
                        new Rectangle(0, 0, 10, 64), drawColor, rotation,
                        new Vector2(5, 59), 1f, SpriteEffects.FlipHorizontally, 0f);

                    spriteBatch.Draw(legTexture.Value, center,
                        new Rectangle(0, 0, 10, 64), drawColor, rotation2,
                        new Vector2(5, 5), 1f, SpriteEffects.FlipHorizontally, 0f);
                }
                else
                {
                    Vector2 drawPosition = legPositions[i] - Main.screenPosition;

                    center = NPC.Center - Main.screenPosition + new Vector2(0, 17).RotatedBy(NPC.rotation);

                    int segmentLength = 54;

                    float rotation = (drawPosition - center).ToRotation() - MathHelper.PiOver2 - (float)Math.Acos(((drawPosition - center) / 2).Length() / segmentLength);
                    float rotation2 = (drawPosition - center).ToRotation() - MathHelper.PiOver2 + (float)Math.Acos(((drawPosition - center) / 2).Length() / segmentLength);

                    spriteBatch.Draw(legTexture.Value, drawPosition,
                        new Rectangle(0, 0, 10, 64), drawColor, rotation,
                        new Vector2(5, 59), 1f, SpriteEffects.None, 0f);

                    spriteBatch.Draw(legTexture.Value, center,
                        new Rectangle(0, 0, 10, 64), drawColor, rotation2,
                        new Vector2(5, 5), 1f, SpriteEffects.None, 0f);
                }
            }

            //draw tail
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            for (int i = segmentPositions.Length - 1; i > 0; i--)
            {
                Vector2 drawPosition = (segmentPositions[i] + segmentPositions[i - 1]) / 2;
                float rotation = (segmentPositions[i - 1] - segmentPositions[i]).ToRotation() + MathHelper.PiOver2;
                float scale = (segmentPositions.Length - i) / (float)(segmentPositions.Length - 1);

                int frame = segmentActive[i / 17] ? 36 : 0;

                spriteBatch.Draw(texture, drawPosition - Main.screenPosition, new Rectangle(0, frame + 34 + (i - 1) % 17 * 2, 34, 4), Lighting.GetColor((int)(drawPosition.X / 16), (int)(drawPosition.Y / 16)), rotation, new Vector2(17, 2), new Vector2(scale, 1), SpriteEffects.None, 0f);
            }

            //draw body
            spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, new Rectangle(0, 0, 34, 34), Lighting.GetColor((int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16)), NPC.rotation, new Vector2(17, 17), new Vector2(1, 1), SpriteEffects.None, 0f);

            return false;
        }
    }

    public class ChaosCrawlerHitbox : ModNPC
    {
        public override string Texture => Polarities.BlankTexture;

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 34;
            NPC.height = 34;

            NPC.defense = 36;
            NPC.damage = 50;
            NPC.lifeMax = 20000;
            NPC.knockBackResist = 0f;
            NPC.value = 5000;
            NPC.npcSlots = 0f;
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

            //Banner = NPCType<ChaosCrawler>();
            //BannerItem = ItemType<ChaosCrawlerBanner>();

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

    public class ChaosBolt : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 24;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.light = 1;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.velocity *= 1.02f;
            if (Projectile.velocity.Length() > 12)
            {
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 12;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 134, newColor: Color.Pink, Scale: 1f)].noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Color mainColor = Color.White;

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), mainColor, Projectile.rotation, new Vector2(5, 5), Projectile.scale, SpriteEffects.None, 0f);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                float scale = Projectile.scale * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

                float rotation;
                if (k + 1 >= Projectile.oldPos.Length)
                {
                    rotation = (Projectile.oldPos[k] - Projectile.position).ToRotation() + MathHelper.PiOver2;
                }
                else
                {
                    rotation = (Projectile.oldPos[k] - Projectile.oldPos[k + 1]).ToRotation() + MathHelper.PiOver2;
                }

                Main.spriteBatch.Draw(texture, Projectile.Center - Projectile.position + (Projectile.oldPos[k] + Projectile.position) / 2 - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), color, rotation, new Vector2(5, 5), scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}
