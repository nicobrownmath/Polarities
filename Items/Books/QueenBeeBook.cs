using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Books
{
    public class QueenBeeBook : BookBase
    {
        public override int BuffType => BuffType<QueenBeeBookBuff>();
        public override int BookIndex => 9;
    }

    public class QueenBeeBookBuff : BookBuffBase
    {
        public override int ItemType => ItemType<QueenBeeBook>();

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ProjectileType<QueenBeeBookBee>()] + player.ownedProjectileCounts[ProjectileType<QueenBeeBookBeeLarge>()] < 6 && player.GetModPlayer<PolaritiesPlayer>().beeRingTimer == 0)
            {
                if (player.strongBees && Main.rand.NextBool())
                {
                    Projectile.NewProjectile(player.GetSource_Buff(buffIndex), Main.MouseWorld, Vector2.Zero, ProjectileType<QueenBeeBookBeeLarge>(), 8, 0.5f, player.whoAmI, Main.rand.NextFloat(0, 2 * MathHelper.Pi));
                }
                else
                {
                    Projectile.NewProjectile(player.GetSource_Buff(buffIndex), Main.MouseWorld, Vector2.Zero, ProjectileType<QueenBeeBookBee>(), 5, 0, player.whoAmI, Main.rand.NextFloat(0, 2 * MathHelper.Pi));
                }
                player.GetModPlayer<PolaritiesPlayer>().beeRingTimer = 5;
            }

            base.Update(player, ref buffIndex);
        }
    }

    public class QueenBeeBookBee : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bee;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bee");
            Main.projFrames[Projectile.type] = Main.projFrames[ProjectileID.Bee];
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.penetrate = 2;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.spriteDirection = ((Main.MouseWorld + new Vector2(32, 0).RotatedBy(Projectile.ai[0]) - Projectile.Hitbox.Size() / 2) - Projectile.position).X > 0 ? 1 : -1;
            Projectile.rotation = (Projectile.spriteDirection - 1) * MathHelper.Pi / 2 + ((Main.MouseWorld + new Vector2(32, 0).RotatedBy(Projectile.ai[0]) - Projectile.Hitbox.Size() / 2) - Projectile.position).ToRotation();

            Projectile.position = Main.MouseWorld + new Vector2(32, 0).RotatedBy(Projectile.ai[0]) - Projectile.Hitbox.Size() / 2;
            Projectile.ai[0] += 0.1f;

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            Projectile.netUpdate = true;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }
    }

    public class QueenBeeBookBeeLarge : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GiantBee;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bee");
            Main.projFrames[Projectile.type] = Main.projFrames[ProjectileID.GiantBee];
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = 2;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.spriteDirection = ((Main.MouseWorld + new Vector2(32, 0).RotatedBy(Projectile.ai[0]) - Projectile.Hitbox.Size() / 2) - Projectile.position).X > 0 ? 1 : -1;
            Projectile.rotation = (Projectile.spriteDirection - 1) * MathHelper.Pi / 2 + ((Main.MouseWorld + new Vector2(32, 0).RotatedBy(Projectile.ai[0]) - Projectile.Hitbox.Size() / 2) - Projectile.position).ToRotation();

            Projectile.position = Main.MouseWorld + new Vector2(32, 0).RotatedBy(Projectile.ai[0]) - Projectile.Hitbox.Size() / 2;
            Projectile.ai[0] += 0.15f;

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            Projectile.netUpdate = true;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }
    }
}