using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Ranged
{
    public class GliderGun : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 400;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 50;
            Item.height = 20;
            Item.useTime = 29;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item115;
            Item.autoReuse = true;
            Item.shoot = ProjectileType<GliderGunProjectile>();
            Item.shootSpeed = 8f;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-10, 3);
        }
    }

    public class GliderGunProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Glider");
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 24;
            Projectile.height = 24;

            Projectile.light = 0.5f;

            Projectile.alpha = 0;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;

            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                Projectile.position += Projectile.velocity.RotatedBy(MathHelper.PiOver2) / 2;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver4;

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 1)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame == 4)
                {
                    Projectile.frame = 0;
                }

                if (Projectile.frame == 0)
                {
                    Projectile.position += Projectile.velocity + Projectile.velocity.RotatedBy(MathHelper.PiOver2);
                }
                else if (Projectile.frame == 2)
                {
                    Projectile.position += Projectile.velocity + Projectile.velocity.RotatedBy(-MathHelper.PiOver2);
                }
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (!target.immortal)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (i != Projectile.whoAmI && Main.projectile[i].active && Main.projectile[i].type == Projectile.type && Main.projectile[i].owner == Projectile.owner)
                    {
                        Main.projectile[i].velocity = (target.Center - Main.projectile[i].Center).SafeNormalize(Vector2.Zero) * Main.projectile[i].velocity.Length();
                    }
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 134, newColor: Color.Pink, Scale: 1f)].noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * 24, 24, 24), Color.White, Projectile.rotation, new Vector2(12, 12), Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool ShouldUpdatePosition()
        {
            return false;
        }
    }
}