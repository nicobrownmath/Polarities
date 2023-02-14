using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Hooks
{
    public class Starhook : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.noUseGraphic = true;
            Item.damage = 0;
            Item.knockBack = 7f;
            Item.useStyle = 5;
            Item.shootSpeed = 13f;
            Item.shoot = ProjectileType<StarhookProjectile>();
            Item.width = 30;
            Item.height = 26;
            Item.UseSound = SoundID.Item1;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.rare = ItemRarityID.Green;
            Item.noMelee = true;
            Item.value = 5000;
        }
    }

    public class StarhookProjectile : ModProjectile
    {
        //texture cacheing
        public static Asset<Texture2D> ChainTexture;

        public override void Load()
        {
            ChainTexture = Request<Texture2D>(Texture + "_Chain");
        }

        public override void Unload()
        {
            ChainTexture = null;

        }

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 26;
            Projectile.height = 26;

            Projectile.aiStyle = 7;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft *= 10;
        }

        public override bool PreAI()
        {
            Lighting.AddLight(Projectile.Center, 1f, 1f, 1f);

            if (Projectile.ai[1] == 0)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedBy((i * 2 - 3) / 18f), Projectile.type, 0, 0, Projectile.owner, ai1: 1);
                    }
                }
                Projectile.Kill();
                Projectile.active = false;
                return false;
            }
            return true;
        }

        // Amethyst Hook is 300, Static Hook is 600
        public override float GrappleRange()
        {
            return 500f;
        }

        public override void NumGrappleHooks(Player player, ref int numHooks)
        {
            numHooks = 4;
        }

        // default is 11, Lunar is 24
        public override void GrappleRetreatSpeed(Player player, ref float speed)
        {
            speed = 12f;
        }

        public override void GrapplePullSpeed(Player player, ref float speed)
        {
            speed = 10;
        }

        public override bool PreDrawExtras()
        {
            Vector2 playerCenter = Main.player[Projectile.owner].MountedCenter;
            Vector2 center = Projectile.Center;
            Vector2 distToProj = playerCenter - Projectile.Center;
            float projRotation = distToProj.ToRotation() - 1.57f;
            float distance = distToProj.Length();
            while (distance > 16f && !float.IsNaN(distance))
            {
                distToProj.Normalize();
                distToProj *= 16f;
                center += distToProj;
                distToProj = playerCenter - center;
                distance = distToProj.Length();

                //Draw chain
                Main.spriteBatch.Draw(ChainTexture.Value, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                    new Rectangle(0, 0, 10, 16), Color.White, projRotation,
                    new Vector2(10 * 0.5f, 16 * 0.5f), 1f, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}
