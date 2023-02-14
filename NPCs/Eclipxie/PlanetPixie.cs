using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.NPCs.Eclipxie
{
    public class PlanetPixie : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Planet Pixie");
            Main.npcFrameCount[NPC.type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 24;
            NPC.height = 24;
            DrawOffsetY = -4;

            NPC.defense = 45;
            NPC.lifeMax = 1500;
            NPC.knockBackResist = 0f;
            NPC.npcSlots = 1f;
            NPC.dontCountMe = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath7;
            NPC.buffImmune[BuffID.Confused] = true;
            NPC.buffImmune[BuffID.OnFire] = true;
            NPC.buffImmune[BuffID.Frostburn] = true;
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.damage = 60;
            NPC.lifeMax = 2000;
        }

        private int currentAttack;
        private int currentAttackTime;

        public override void AI()
        {
            if (NPC.localAI[0] == 0)
            {
                NPC.localAI[0] = 1;
            }

            NPC owner = Main.npc[(int)NPC.ai[0]];

            if (!owner.active || owner.hide)// || owner.ai[3] > 0f)
            {
                NPC.active = false;
            }

            float resummonTimeMultiplier = 60f;
            float orbitTimeMultiplier = 20f;
            float distanceMultiplier = 1 - (float)Math.Exp(-NPC.ai[2] / resummonTimeMultiplier);
            NPC.velocity = -NPC.Center + owner.Center + new Vector2(240 * distanceMultiplier, 0).RotatedBy(NPC.ai[3] * (Main.GameUpdateCount / orbitTimeMultiplier - distanceMultiplier) + NPC.ai[1] * MathHelper.TwoPi / 5);
            NPC.ai[2]++;

            Player player = Main.player[owner.target];

            currentAttackTime++;
            if (currentAttack != (int)owner.ai[0])
            {
                currentAttackTime = 0;
                currentAttack = (int)owner.ai[0];
            }

            if (owner.ai[0] == 2)
            {
                if (currentAttackTime == 20)
                {
                    //scythes
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (player.Center - owner.Center).SafeNormalize(Vector2.Zero) * 0.005f, ProjectileType<PlanetPixieScythe>(), Main.expertMode ? 10 : 15, 2f, Main.myPlayer);
                }
            }
            else if (owner.ai[0] == 3)
            {
                //laser barrage
                if (currentAttackTime % (5 * (int)(5 + 15 * (owner.life / (float)owner.lifeMax))) == NPC.ai[1] * (int)(5 + 15 * (owner.life / (float)owner.lifeMax)) && currentAttackTime > 0)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 12, ProjectileType<PlanetPixieRay>(), Main.expertMode ? 10 : 15, 2f, Main.myPlayer);
                }
            }
        }

        public override bool PreKill()
        {
            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Vector2 drawOrigin = new Vector2(TextureAssets.Npc[NPC.type].Value.Width * 0.5f, TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type] * 0.5f);
            Vector2 drawPos = NPC.Center - Main.screenPosition;
            spriteBatch.Draw(TextureAssets.Npc[NPC.type].Value, drawPos, NPC.frame, Color.White, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter == 3)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y = (NPC.frame.Y + frameHeight) % (4 * frameHeight);
            }
        }

        public override bool CheckActive()
        {
            return Main.npc[(int)NPC.ai[0]].active;
        }
    }

    public class PlanetPixieRay : ModProjectile
    {
        public override string Texture => "Polarities/NPCs/Eclipxie/AncientStarlight";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Planet Ray");
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 2;
            Projectile.height = 2;
            DrawOffsetX = -28;
            DrawOriginOffsetX = 15;
            DrawOriginOffsetY = -2;
            Projectile.alpha = 0;
            Projectile.timeLeft = 600;
            Projectile.penetrate = 1;
            Projectile.hostile = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 0.8f;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float trailLength = 4f;
            float trailSize = 4;
            for (int i = (int)trailLength - 1; i >= 0; i--)
            {
                Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Projectile.velocity * (i / trailLength) * trailSize - Main.screenPosition, new Rectangle(0, TextureAssets.Projectile[Projectile.type].Value.Height * Projectile.frame / Main.projFrames[Projectile.type], TextureAssets.Projectile[Projectile.type].Value.Width, TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]), lightColor * (1 - Projectile.alpha / 256f) * (1 - i / trailLength), Projectile.rotation, new Vector2(TextureAssets.Projectile[Projectile.type].Value.Width / 2 + DrawOriginOffsetX, Projectile.height / 2), Projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }

    public class PlanetPixieScythe : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Planet Scythe");
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 16;
            Projectile.height = 16;

            Projectile.alpha = 0;
            Projectile.timeLeft = 600;
            Projectile.penetrate = 1;
            Projectile.hostile = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 0.8f;
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

        public override bool PreDraw(ref Color lightColor)
        {
            float trailLength = 4f;
            float trailSize = 4;
            for (int i = (int)trailLength - 1; i >= 0; i--)
            {
                Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Projectile.velocity * (i / trailLength) * trailSize - Main.screenPosition, new Rectangle(0, TextureAssets.Projectile[Projectile.type].Value.Height * Projectile.frame / Main.projFrames[Projectile.type], TextureAssets.Projectile[Projectile.type].Value.Width, TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]), lightColor * (1 - Projectile.alpha / 256f) * (1 - i / trailLength), Projectile.rotation, new Vector2(TextureAssets.Projectile[Projectile.type].Value.Width / 2 + DrawOriginOffsetX, Projectile.height / 2), Projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
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
    }
}