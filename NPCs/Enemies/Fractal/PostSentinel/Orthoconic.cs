using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Biomes.Fractal;
using Polarities.Items;
using Polarities.Items.Placeable.Banners;
using Polarities.Items.Placeable.Blocks.Fractal;
using Polarities.Items.Weapons.Magic;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.NPCs.Enemies.Fractal.PostSentinel
{
    public class Orthoconic : ModNPC
    {
        //custom hitbox code is how it's done in Imperious from QWERTY's mod
        private int[] hitBoxSegmentIds = { -1, -1, -1, -1, -1, -1, -1, -1 };

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 42;
            NPC.height = 42;

            DrawOffsetY = 174;

            NPC.defense = 20;
            NPC.damage = 50;
            NPC.lifeMax = 7200;
            NPC.knockBackResist = 0f;

            NPC.value = 5000;
            NPC.npcSlots = 1f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.rarity = 1;

            NPC.chaseable = false;

            NPC.buffImmune[BuffID.Confused] = true;
            NPC.buffImmune[BuffID.OnFire] = true;
            NPC.buffImmune[BuffID.Frostburn] = true;
            NPC.buffImmune[BuffID.CursedInferno] = true;
            NPC.buffImmune[BuffID.ShadowFlame] = true;
            NPC.buffImmune[BuffID.Poisoned] = true;
            NPC.buffImmune[BuffID.Venom] = true;

            Banner = NPC.type;
            BannerItem = ItemType<OrthoconicBanner>();

            this.SetModBiome<FractalBiome, FractalOceanBiome>();
        }

        public override bool? CanBeHitByItem(Player player, Item item)
        {
            return false;
        }

        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            return false;
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            position.Y -= 178;
            return true;
        }

        public override void AI()
        {
            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.netUpdate = true;
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
            }

            float angleGoal = NPC.rotation;
            float maxTurn = 0.1f;
            float angleOffset;

            //randomize ai if ai[0] is set to 0
            if (NPC.ai[0] == 0)
            {
                NPC.ai[0] = Main.rand.Next(1, 5);
                NPC.ai[1] = 0;
            }
            switch (NPC.ai[0])
            {
                case 1:
                    //parabolic arcs attack
                    if (NPC.ai[1] < 60)
                    {
                        NPC.velocity = new Vector2(0, (player.Center - NPC.Center).Length() / 120).RotatedBy(NPC.rotation);
                        angleGoal = (player.Center - NPC.Center).RotatedBy(-MathHelper.PiOver2).ToRotation();

                        NPC.ai[1]++;
                    }
                    else if (NPC.ai[1] == 60)
                    {
                        //point away from player

                        NPC.velocity = new Vector2(0, (player.Center - NPC.Center).Length() / 120).RotatedBy(NPC.rotation);

                        angleGoal = (player.Center - NPC.Center).RotatedBy(-MathHelper.PiOver2).ToRotation();

                        if (angleGoal > MathHelper.TwoPi) angleGoal -= MathHelper.TwoPi;
                        else if (angleGoal < 0) angleGoal += MathHelper.TwoPi;
                        if (NPC.rotation > MathHelper.TwoPi) NPC.rotation -= MathHelper.TwoPi;
                        else if (NPC.rotation < 0) NPC.rotation += MathHelper.TwoPi;

                        angleOffset = angleGoal - NPC.rotation;
                        if (angleOffset > MathHelper.Pi) angleOffset -= MathHelper.TwoPi;
                        else if (angleOffset < -MathHelper.Pi) angleOffset += MathHelper.TwoPi;

                        if (Math.Abs(angleOffset) < 0.1f)
                        {
                            NPC.ai[1]++;

                            NPC.velocity = new Vector2(0, -8).RotatedBy(angleGoal);

                            //spawn projectiles
                            if (Main.netMode != 1)
                            {
                                for (int i = 0; i < 7; i++)
                                {
                                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.velocity.RotatedBy((i - 3f) / MathHelper.TwoPi), ProjectileType<OrthoconicParabola>(), 20, 3f, Main.myPlayer, -NPC.velocity.X, -NPC.velocity.Y);
                                }
                            }
                        }
                    }
                    else if (NPC.ai[1] > 60)
                    {
                        angleGoal = NPC.velocity.RotatedBy(MathHelper.PiOver2).ToRotation();

                        NPC.velocity *= 0.95f;

                        NPC.ai[1]++;
                        if (NPC.ai[1] >= 120)
                        {
                            NPC.ai[0] = 0;
                        }
                    }
                    break;
                case 2:
                    //ellipse rings attack
                    if (NPC.ai[1] <= 30)
                    {
                        angleGoal = (player.Center - NPC.Center).RotatedBy(-MathHelper.PiOver2).ToRotation();
                    }
                    NPC.velocity *= 0.93f;

                    if (NPC.ai[1] % 6 == 0 && NPC.ai[1] > 30 && NPC.ai[1] <= 120 && Main.netMode != 1)
                    {
                        float rotation = NPC.rotation - MathHelper.PiOver2;
                        float speed = Main.rand.NextFloat(10, 12);
                        float distanceMultiplier = 1 / Main.rand.NextFloat(1.01f, 1.33f);
                        int direction = Main.rand.NextBool() ? 1 : -1;

                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + new Vector2(distanceMultiplier * 12000f / (speed * speed), 0).RotatedBy(rotation), new Vector2(0, direction * speed).RotatedBy(rotation), ProjectileType<OrthoconicEllipse>(), 20, 3f, Main.myPlayer, NPC.Center.X, NPC.Center.Y);
                    }

                    NPC.ai[1]++;
                    if (NPC.ai[1] >= 240)
                    {
                        //can't repeat this attack
                        NPC.ai[0] = Main.rand.Next(new int[] { 1, 3, 4 });
                        NPC.ai[1] = 0;
                    }
                    break;
                case 3:
                    //circle charge circle attack
                    if (NPC.ai[1] == 0)
                    {
                        if (Main.netMode != 1)
                        {
                            NPC.ai[2] = Main.rand.NextBool() ? 1 : -1;
                        }
                        NPC.netUpdate = true;
                    }
                    if (NPC.ai[1] < 90)
                    {
                        NPC.velocity = new Vector2(0, -8).RotatedBy(NPC.rotation);
                        angleGoal = NPC.rotation + NPC.ai[2] * 0.075f;

                        NPC.ai[1]++;
                    }
                    else if (NPC.ai[1] < 180)
                    {
                        NPC.velocity = new Vector2(0, -8).RotatedBy(NPC.rotation);
                        angleGoal = NPC.rotation + NPC.ai[2] * 0.075f;

                        angleOffset = (player.Center - NPC.Center).ToRotation() - (NPC.rotation - MathHelper.PiOver2);
                        if (angleOffset > MathHelper.Pi) angleOffset -= MathHelper.TwoPi;
                        else if (angleOffset < -MathHelper.Pi) angleOffset += MathHelper.TwoPi;

                        NPC.ai[1]++;

                        if (Math.Abs(angleOffset) < 0.1f)
                        {
                            NPC.ai[1] = 180;
                            NPC.velocity *= 1.5f;
                        }
                    }
                    else if (NPC.ai[1] == 180)
                    {
                        angleGoal = NPC.velocity.RotatedBy(MathHelper.PiOver2).ToRotation();

                        Vector2 playerDistance = player.Center - NPC.Center;
                        if (NPC.velocity.X * playerDistance.X + NPC.velocity.Y * playerDistance.Y < 0)
                        {
                            NPC.ai[1]++;
                            NPC.ai[2] = NPC.velocity.X * playerDistance.Y - NPC.velocity.Y * playerDistance.X > 0 ? 1 : -1;
                        }
                    }
                    else
                    {
                        NPC.velocity = new Vector2(0, -8f).RotatedBy(NPC.rotation);
                        angleGoal = NPC.rotation + NPC.ai[2] * 0.075f;

                        NPC.ai[1]++;
                        if (NPC.ai[1] >= 270)
                        {
                            //can't repeat this attack
                            NPC.ai[0] = Main.rand.Next(new int[] { 1, 2, 4 });
                            NPC.ai[1] = 0;
                        }
                    }
                    break;
                case 4:
                    //approach player and attempt to bash them with the cone
                    if (NPC.ai[1] < 90)
                    {
                        NPC.velocity = new Vector2(0, (player.Center - NPC.Center).Length() / 75).RotatedBy(NPC.rotation);
                        angleGoal = (player.Center - NPC.Center).RotatedBy(-MathHelper.PiOver2).ToRotation();

                        NPC.ai[1]++;
                    }
                    else if (NPC.ai[1] < 120)
                    {
                        angleGoal = (player.Center - NPC.Center).RotatedBy(MathHelper.PiOver2).ToRotation();
                        maxTurn = 0.15f;

                        NPC.ai[1]++;
                    }
                    else
                    {
                        NPC.velocity = new Vector2(0, -(player.Center - NPC.Center).Length() / 120).RotatedBy(NPC.rotation);
                        angleGoal = (player.Center - NPC.Center).RotatedBy(-MathHelper.PiOver2).ToRotation();
                        maxTurn = 0.15f;

                        NPC.ai[1]++;
                        if (NPC.ai[1] >= 150)
                        {
                            NPC.ai[0] = 0;
                        }
                    }
                    break;
            }

            if (angleGoal > MathHelper.TwoPi) angleGoal -= MathHelper.TwoPi;
            else if (angleGoal < 0) angleGoal += MathHelper.TwoPi;
            if (NPC.rotation > MathHelper.TwoPi) NPC.rotation -= MathHelper.TwoPi;
            else if (NPC.rotation < 0) NPC.rotation += MathHelper.TwoPi;

            angleOffset = angleGoal - NPC.rotation;
            if (angleOffset > MathHelper.Pi) angleOffset -= MathHelper.TwoPi;
            else if (angleOffset < -MathHelper.Pi) angleOffset += MathHelper.TwoPi;

            if (Math.Abs(angleOffset) < maxTurn) { NPC.rotation = angleGoal; }
            else if (angleOffset > 0)
            {
                NPC.rotation += maxTurn;
            }
            else
            {
                NPC.rotation -= maxTurn;
            }

            //Custom hitbox based on Qwerty's mod
            //position hitbox segments
            NPC.realLife = NPC.whoAmI;
            for (int h = 0; h < hitBoxSegmentIds.Length; h++)
            {
                Vector2 spot = NPC.Center + NPC.velocity + new Vector2(0, -h * (178 / hitBoxSegmentIds.Length)).RotatedBy(NPC.rotation); //QwertyMethods.PolarVector((totalLength - bladeLength - 18) + h * (bladeLength / (hitBoxSegmentIds.Length + 1)) + bladeWidth / 2, npc.rotation);
                if (hitBoxSegmentIds[h] == -1 || !Main.npc[hitBoxSegmentIds[h]].active || Main.npc[hitBoxSegmentIds[h]].type != NPCType<OrthoconicHitbox>())
                {
                    if (Main.netMode != 1)
                    {
                        hitBoxSegmentIds[h] = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spot.X, (int)spot.Y, NPCType<OrthoconicHitbox>(), ai0: NPC.whoAmI);
                        NPC.netUpdate = true;
                    }
                }
                else
                {
                    Main.npc[hitBoxSegmentIds[h]].Center = spot;
                    Main.npc[hitBoxSegmentIds[h]].timeLeft = 10;
                    Main.npc[hitBoxSegmentIds[h]].dontTakeDamage = NPC.dontTakeDamage;
                    if (h > 0)
                    {
                        Main.npc[hitBoxSegmentIds[h]].defense = 9999;
                        Main.npc[hitBoxSegmentIds[h]].defDefense = 9999;
                        Main.npc[hitBoxSegmentIds[h]].HitSound = SoundID.NPCHit2;
                    }
                    else
                    {
                        Main.npc[hitBoxSegmentIds[h]].defense = NPC.defense;
                        Main.npc[hitBoxSegmentIds[h]].defDefense = NPC.defDefense;
                        Main.npc[hitBoxSegmentIds[h]].HitSound = SoundID.NPCHit1;
                    }
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter == 5)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y = (NPC.frame.Y + frameHeight) % (frameHeight * Main.npcFrameCount[NPC.type]);

            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            for (int i = 0; i < hitBoxSegmentIds.Length; i++)
            {
                writer.Write(hitBoxSegmentIds[i]);
            }
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            for (int i = 0; i < hitBoxSegmentIds.Length; i++)
            {
                hitBoxSegmentIds[i] = reader.ReadInt32();
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            //if (Subworld.IsActive<FractalSubworld>() && spawnInfo.Player.GetModPlayer<PolaritiesPlayer>().GetFractalization() > FractalSubworld.POST_GOLEM_TIME)
            //{
            //    return 0.15f * FractalSubworld.SpawnConditionFractalWaters(spawnInfo) * (1 - FractalSubworld.SpawnConditionFractalSky(spawnInfo));
            //}
            return 0f;
        }

        public override bool CheckDead()
        {
            //Gore.NewGore(NPC.position, NPC.velocity, Mod.GetGoreSlot("Gores/OrthoconicGore"));
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
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FractalOre>(), minimumDropped: 1, maximumDropped: 3));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FractalKey>(), chanceDenominator: 3));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ConeGun>(), chanceDenominator: 20));
        }
    }

    public class OrthoconicHitbox : ModNPC
    {
        public override string Texture => Polarities.CallShootProjectile;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers(0) { Hide = true, };
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 42;
            NPC.height = 42;
            DrawOffsetY = 36;

            NPC.defense = 9999;
            NPC.damage = 50;
            NPC.lifeMax = 7200;
            NPC.knockBackResist = 0f;

            NPC.value = 5000;
            NPC.npcSlots = 0f;
            NPC.dontCountMe = true;
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

            //Banner = NPCType<Orthoconic>();
            //BannerItem = ItemType<OrthoconicBanner>();
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

        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
        {
            List<int> hitboxIds = new List<int>();
            for (int i = 0; i < Main.npc.Length; i++)
            {
                if (Main.npc[i].type == NPC.type && Main.npc[i].realLife == NPC.realLife)
                {
                    hitboxIds.Add(i);
                }
            }
            if (projectile.usesLocalNPCImmunity || projectile.localNPCImmunity[NPC.whoAmI] != 0)
            {
                foreach (int who in hitboxIds)
                {
                    projectile.localNPCImmunity[who] = projectile.localNPCImmunity[NPC.whoAmI];
                    Main.npc[who].immune[projectile.owner] = NPC.immune[projectile.owner];
                }
            }
            else if (projectile.usesIDStaticNPCImmunity)
            {
                foreach (int who in hitboxIds)
                {

                    Projectile.perIDStaticNPCImmunity[projectile.type][who] = Projectile.perIDStaticNPCImmunity[projectile.type][NPC.whoAmI];
                }
            }
            else
            {
                foreach (int who in hitboxIds)
                {
                    Main.npc[who].immune[projectile.owner] = NPC.immune[projectile.owner];
                }
            }
        }

        public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit)
        {
            List<int> hitboxIds = new List<int>();
            for (int i = 0; i < Main.npc.Length; i++)
            {
                if (Main.npc[i].type == NPC.type && Main.npc[i].realLife == NPC.realLife)
                {
                    hitboxIds.Add(i);
                }
            }
            foreach (int who in hitboxIds)
            {
                Main.npc[who].immune[player.whoAmI] = NPC.immune[player.whoAmI];
            }
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

    public class OrthoconicParabola : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_644";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.hostile = true;

            Projectile.width = 10;
            Projectile.height = 10;

            DrawOffsetX = -31;
            DrawOriginOffsetY = -31;
            DrawOriginOffsetX = 0;

            Projectile.alpha = 0;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Projectile.velocity += new Vector2(Projectile.ai[0], Projectile.ai[1]).SafeNormalize(Vector2.Zero) * 0.085f;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.timeLeft < 30)
            {
                Projectile.scale -= 1 / 30f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Color mainColor = new Color(181, 248, 255);

            for (int k = 0; k < Math.Min(Projectile.oldPos.Length, 598 - Projectile.timeLeft); k++)
            {
                Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                float scale = Projectile.scale * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

                float rotation;
                if (k + 1 >= Projectile.oldPos.Length)
                {
                    rotation = (Projectile.position - Projectile.oldPos[k]).ToRotation() + MathHelper.PiOver2;
                }
                else
                {
                    rotation = (Projectile.oldPos[k + 1] - Projectile.oldPos[k]).ToRotation() + MathHelper.PiOver2;
                }

                Main.spriteBatch.Draw(texture, Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), color, rotation, new Vector2(36, 36), new Vector2(scale, Projectile.scale), SpriteEffects.None, 0f);
            }

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), mainColor, Projectile.rotation, new Vector2(36, 36), Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }

    public class OrthoconicEllipse : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_644";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.hostile = true;

            Projectile.width = 10;
            Projectile.height = 10;

            DrawOffsetX = -31;
            DrawOriginOffsetY = -31;
            DrawOriginOffsetX = 0;

            Projectile.alpha = 0;
            Projectile.timeLeft = 270;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Vector2 displacement = new Vector2(Projectile.ai[0], Projectile.ai[1]) - Projectile.Center;

            Projectile.velocity += 6000f * displacement / (float)Math.Pow(displacement.Length(), 3);

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.timeLeft < 30)
            {
                Projectile.scale -= 1 / 30f;
            }
        }

        public override bool? CanDamage()/* tModPorter Suggestion: Return null instead of true */
        {
            return Projectile.timeLeft <= 240;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Color mainColor = new Color(181, 248, 255);

            for (int k = 0; k < Math.Min(Projectile.oldPos.Length, 268 - Projectile.timeLeft); k++)
            {
                Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                float scale = Projectile.scale * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

                float rotation;
                if (k + 1 >= Projectile.oldPos.Length)
                {
                    rotation = (Projectile.position - Projectile.oldPos[k]).ToRotation() + MathHelper.PiOver2;
                }
                else
                {
                    rotation = (Projectile.oldPos[k + 1] - Projectile.oldPos[k]).ToRotation() + MathHelper.PiOver2;
                }

                Main.spriteBatch.Draw(texture, Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), color, rotation, new Vector2(36, 36), new Vector2(scale, Projectile.scale), SpriteEffects.None, 0f);
            }

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), mainColor, Projectile.rotation, new Vector2(36, 36), Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
