using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Books
{
    public class PlanteraBook : BookBase
    {
        public override int BuffType => BuffType<PlanteraBookBuff>();
        public override int BookIndex => 20;
    }

    public class PlanteraBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<PlanteraBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ProjectileType<PlanteraBookProjectile>()] < 10)
            {
                for (int i = 0; i < 10 - player.ownedProjectileCounts[ProjectileType<PlanteraBookProjectile>()]; i++)
                {
                    Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.Center, player.velocity, ProjectileType<PlanteraBookProjectile>(), 30, 2, player.whoAmI, Main.rand.NextFloat(MathHelper.TwoPi));
                }
            }

            base.Update(player, ref buffIndex);
        }
    }

    public class PlanteraBookProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/NPC_" + NPCID.PlanterasTentacle;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hook");
            Main.projFrames[Projectile.type] = Main.npcFrameCount[NPCID.PlanterasTentacle];
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 29;
            Projectile.height = 29;
            DrawOffsetX = 0;
            DrawOriginOffsetY = -6;
            DrawOriginOffsetX = 0;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.tileCollide = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (player.HasBuff(BuffType<PlanteraBookBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            Projectile.ai[0] += Main.rand.NextFloat(-0.05f, 0.05f);

            float avgDistance = 130;

            NPC target = Projectile.FindTargetWithinRange(80);
            if (target != null)
            {
                Projectile.ai[0] = Projectile.ai[0].AngleLerp((target.Center - player.Center).ToRotation(), 0.05f);
                avgDistance = MathHelper.Min(avgDistance, (target.Center - player.Center).Length());
            }

            Vector2 goalPosition = player.Center + new Vector2(avgDistance + Main.rand.Next(-30, 30), 0).RotatedBy(Projectile.ai[0]);
            Vector2 goalVelocity = goalPosition - Projectile.Center;
            if (goalVelocity.Length() > 6)
            {
                goalVelocity.Normalize();
                goalVelocity *= 6;
            }

            goalVelocity += player.velocity;
            Projectile.velocity += (goalVelocity - Projectile.velocity) / 10;

            Projectile.rotation = (player.Center - Projectile.Center).ToRotation();

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            if ((Projectile.Center - player.Center).Length() > 400) { Projectile.Kill(); }
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 playerCenter = Main.player[Projectile.owner].MountedCenter;
            Vector2 center = Projectile.Center;
            Vector2 distToProj = playerCenter - Projectile.Center;
            float projRotation = distToProj.ToRotation();
            float distance = distToProj.Length();
            while (distance > 16f && !float.IsNaN(distance))
            {
                distToProj.Normalize();                 //get unit vector
                distToProj *= 16f;                      //speed
                center += distToProj;                   //update draw position
                distToProj = playerCenter - center;    //update distance
                distance = distToProj.Length();
                Color drawColor = lightColor;

                //Draw chain
                Main.EntitySpriteDraw(TextureAssets.Chain27.Value, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                    new Rectangle(0, 0, 16, 16), drawColor, projRotation + MathHelper.PiOver2,
                    new Vector2(16 * 0.5f, 16 * 0.5f), 1f, SpriteEffects.None, 0);
            }
            return true;
        }
    }

    public class PlanteraBookHookProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/NPC_" + NPCID.PlanterasTentacle;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hook");
            Main.projFrames[Projectile.type] = Main.npcFrameCount[NPCID.PlanterasTentacle];
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }

        private Projectile hook;

        public override void SetDefaults()
        {
            Projectile.width = 29;
            Projectile.height = 29;
            DrawOffsetX = 0;
            DrawOriginOffsetY = -6;
            DrawOriginOffsetX = 0;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.tileCollide = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void OnSpawn(IEntitySource source)
        {
            hook = Main.projectile[(int)Projectile.ai[1]];
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (player.HasBuff(BuffType<PlanteraBookBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            Projectile.ai[0] += Main.rand.NextFloat(-0.05f, 0.05f);

            float avgDistance = 130;

            NPC target = Projectile.FindTargetWithinRange(80);
            if (target != null)
            {
                Projectile.ai[0] = Projectile.ai[0].AngleLerp((target.Center - hook.Center).ToRotation(), 0.05f);
                avgDistance = MathHelper.Min(avgDistance, (target.Center - hook.Center).Length());
            }

            Vector2 goalPosition = hook.Center + new Vector2(avgDistance + Main.rand.Next(-30, 30), 0).RotatedBy(Projectile.ai[0]);
            Vector2 goalVelocity = goalPosition - Projectile.Center;
            if (goalVelocity.Length() > 6)
            {
                goalVelocity.Normalize();
                goalVelocity *= 6;
            }

            goalVelocity += hook.velocity;
            Projectile.velocity += (goalVelocity - Projectile.velocity) / 10;

            Projectile.rotation = (hook.Center - Projectile.Center).ToRotation();

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            if ((Projectile.Center - hook.Center).Length() > 400 || !hook.active || hook.aiStyle != 7) { Projectile.Kill(); }
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 hookCenter = hook.Center;
            Vector2 center = Projectile.Center;
            Vector2 distToProj = hookCenter - Projectile.Center;
            float projRotation = distToProj.ToRotation();
            float distance = distToProj.Length();
            while (distance > 16f && !float.IsNaN(distance))
            {
                distToProj.Normalize();                 //get unit vector
                distToProj *= 16f;                      //speed
                center += distToProj;                   //update draw position
                distToProj = hookCenter - center;    //update distance
                distance = distToProj.Length();
                Color drawColor = lightColor;

                //Draw chain
                Main.EntitySpriteDraw(TextureAssets.Chain27.Value, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                    new Rectangle(0, 0, 16, 16), drawColor, projRotation + MathHelper.PiOver2,
                    new Vector2(16 * 0.5f, 16 * 0.5f), 1f, SpriteEffects.None, 0);
            }
            return true;
        }
    }
}