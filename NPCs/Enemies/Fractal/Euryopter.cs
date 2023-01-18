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
    public class Euryopter : ModNPC
    {
        private int attackTimer;
        private float attackAngle = 0.1f;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 32;
            NPC.height = 32;
            DrawOffsetY = 14;

            NPC.defense = 8;
            NPC.damage = 36;
            NPC.lifeMax = 240;
            NPC.knockBackResist = 0.01f;
            NPC.npcSlots = 1f;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 750;
            NPC.noTileCollide = false;
            NPC.noGravity = true;

            Banner = NPC.type;
            BannerItem = ItemType<EuryopterBanner>();

            this.SetModBiome<FractalBiome>();
        }

        public override void AI()
        {
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead)
            {
                NPC.TargetClosest();
            }
            float num36 = 4f;
            float num39 = 0.02f;
            Vector2 vector = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
            float num54 = Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2);
            float num53 = Main.player[NPC.target].position.Y + (float)(Main.player[NPC.target].height / 2);
            num54 = (int)(num54 / 8f) * 8;
            num53 = (int)(num53 / 8f) * 8;
            vector.X = (int)(vector.X / 8f) * 8;
            vector.Y = (int)(vector.Y / 8f) * 8;
            num54 -= vector.X;
            num53 -= vector.Y;
            float num48 = (float)Math.Sqrt(num54 * num54 + num53 * num53);
            float num47 = num48;
            if (num48 == 0f)
            {
                num54 = NPC.velocity.X;
                num53 = NPC.velocity.Y;
            }
            else
            {
                num48 = num36 / num48;
                num54 *= num48;
                num53 *= num48;
            }
            if (num47 > 100f)
            {
                NPC.ai[0] += 1f;
                if (NPC.ai[0] > 0f)
                {
                    NPC.velocity.Y += 0.023f;
                }
                else
                {
                    NPC.velocity.Y -= 0.023f;
                }
                if (NPC.ai[0] < -100f || NPC.ai[0] > 100f)
                {
                    NPC.velocity.X += 0.023f;
                }
                else
                {
                    NPC.velocity.X -= 0.023f;
                }
                if (NPC.ai[0] > 200f)
                {
                    NPC.ai[0] = -200f;
                }
            }
            if (num47 < 150f)
            {
                NPC.velocity.X += num54 * 0.007f;
                NPC.velocity.Y += num53 * 0.007f;
            }
            if (Main.player[NPC.target].dead)
            {
                num54 = NPC.direction * num36 / 2f;
                num53 = (0f - num36) / 2f;
            }
            if (NPC.velocity.X < num54)
            {
                NPC.velocity.X += num39;
            }
            else if (NPC.velocity.X > num54)
            {
                NPC.velocity.X -= num39;
            }
            if (NPC.velocity.Y < num53)
            {
                NPC.velocity.Y += num39;
            }
            else if (NPC.velocity.Y > num53)
            {
                NPC.velocity.Y -= num39;
            }
            NPC.rotation = (float)Math.Atan2(num53, num54) - 1.57f;
            float num37 = 0.4f;
            if (NPC.collideX)
            {
                NPC.netUpdate = true;
                NPC.velocity.X = NPC.oldVelocity.X * (0f - num37);
                if (NPC.direction == -1 && NPC.velocity.X > 0f && NPC.velocity.X < 2f)
                {
                    NPC.velocity.X = 2f;
                }
                if (NPC.direction == 1 && NPC.velocity.X < 0f && NPC.velocity.X > -2f)
                {
                    NPC.velocity.X = -2f;
                }
            }
            if (NPC.collideY)
            {
                NPC.netUpdate = true;
                NPC.velocity.Y = NPC.oldVelocity.Y * (0f - num37);
                if (NPC.velocity.Y > 0f && NPC.velocity.Y < 1.5)
                {
                    NPC.velocity.Y = 2f;
                }
                if (NPC.velocity.Y < 0f && NPC.velocity.Y > -1.5)
                {
                    NPC.velocity.Y = -2f;
                }
            }
            if (NPC.wet)
            {
                if (NPC.velocity.Y > 0f)
                {
                    NPC.velocity.Y *= 0.95f;
                }
                NPC.velocity.Y -= 0.3f;
                if (NPC.velocity.Y < -2f)
                {
                    NPC.velocity.Y = -2f;
                }
            }
            if ((NPC.velocity.X > 0f && NPC.oldVelocity.X < 0f || NPC.velocity.X < 0f && NPC.oldVelocity.X > 0f || NPC.velocity.Y > 0f && NPC.oldVelocity.Y < 0f || NPC.velocity.Y < 0f && NPC.oldVelocity.Y > 0f) && !NPC.justHit)
            {
                NPC.netUpdate = true;
            }


            if (attackTimer == 0)
            {
                attackTimer = Main.expertMode ? 4 : 8;

                attackAngle = (0.5f + 2.5f * (1 - (float)NPC.life / NPC.lifeMax)) * ((float)Math.Pow(attackAngle, 3) - attackAngle);
                if (Math.Abs(attackAngle) < 0.01f)
                {
                    attackAngle = 0.01f;
                }

                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(13, 0).RotatedBy(attackAngle * 0.5f + NPC.rotation + MathHelper.PiOver2), ProjectileType<EuryopterBolt>(), 15, 0f, Main.myPlayer);
            }

            attackTimer--;
            NPC.rotation += MathHelper.Pi;
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
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FractalMatter>(), minimumDropped: 1, maximumDropped: 3));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FractalResidue>(), chanceDenominator: 4));
        }
    }

    public class EuryopterBolt : ModProjectile
    {
        public override void SetStaticDefaults()
        {
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.light = 1;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.alpha = Math.Max(0, Projectile.alpha - 16);
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