using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Polarities.Projectiles;
using System;
using Terraria.Audio;
using ReLogic.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Polarities.Items.Weapons.Melee
{
	public class ChainSaw : ModItem
	{
		public override void SetStaticDefaults()
		{
            this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(15, 1f, 0);
			Item.DamageType = DamageClass.Melee;

			Item.width = 40;
			Item.height = 36;

			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = 5;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item23;
			Item.autoReuse = true;
			Item.channel = true;

			Item.shoot = ProjectileType<ChainSawProjectile>();
			Item.shootSpeed = 16f;

			Item.value = 10000;
			Item.rare = ItemRarityID.Green;
		}
	}

    public class ChainSawProjectile : ModProjectile
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
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 3600;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.rotation += 1f;

            if (Projectile.localAI[0] >= 30)
            {
                Projectile.localAI[0] = 0;
            }
            if (Projectile.localAI[0] == 0)
            {
                SoundEngine.PlaySound(SoundID.Item, Projectile.Center, 22);
            }
            Projectile.localAI[0]++;

            Player player = Main.player[Projectile.owner];
            player.direction = (Projectile.Center.X - player.Center.X > 0) ? 1 : -1;
            player.itemAnimation = 10;
            player.itemTime = 10;

            if (player.dead)
            {
                Projectile.Kill();
                return;
            }

            if ((Projectile.Center - player.Center).Length() > 600)
            {
                Projectile.ai[0] = 2;
            }

            Vector2 mountedCenter2 = Main.player[Projectile.owner].MountedCenter;
            float num209 = mountedCenter2.X - Projectile.Center.X;
            float num210 = mountedCenter2.Y - Projectile.Center.Y;
            float dToPlayer = (Projectile.Center - Main.player[Projectile.owner].MountedCenter).Length();

            if (dToPlayer > 400)
            {
                Projectile.ai[0] = 2;
            }

            switch (Projectile.ai[0])
            {
                case 0:
                    //flying out
                    break;
                case 1:
                    //embedded
                    NPC target = Main.npc[(int)Projectile.ai[1]];
                    Projectile.position = target.Center + Projectile.position - Projectile.Center;
                    Projectile.velocity = target.velocity;
                    if (!player.channel || !target.active || target.dontTakeDamage)
                    {
                        Projectile.ai[0] = 2;
                    }

                    break;
                case 2:
                    //retracting (adapted from vanilla flail code)
                    float num213 = 14f / Main.player[Projectile.owner].GetTotalAttackSpeed(DamageClass.Melee);
                    float num214 = 0.9f / Main.player[Projectile.owner].GetTotalAttackSpeed(DamageClass.Melee);
                    Math.Abs(num209);
                    Math.Abs(num210);
                    Projectile.tileCollide = false;
                    if (dToPlayer < 20f)
                    {
                        Projectile.Kill();
                    }
                    num214 *= 2f;
                    dToPlayer = num213 / dToPlayer;
                    num209 *= dToPlayer;
                    num210 *= dToPlayer;
                    new Vector2(Projectile.velocity.X, Projectile.velocity.Y);
                    float num217 = num209 - Projectile.velocity.X;
                    float num218 = num210 - Projectile.velocity.Y;
                    float num219 = (float)Math.Sqrt(num217 * num217 + num218 * num218);
                    num219 = num214 / num219;
                    num217 *= num219;
                    num218 *= num219;
                    Projectile.velocity.X = Projectile.velocity.X * 0.98f;
                    Projectile.velocity.Y = Projectile.velocity.Y * 0.98f;
                    Projectile.velocity.X = Projectile.velocity.X + num217;
                    Projectile.velocity.Y = Projectile.velocity.Y + num218;

                    break;
            }
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = 2;
            height = 2;
            return true;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (Projectile.ai[0] == 0)
            {
                Projectile.ai[0] = 1;
                Projectile.ai[1] = target.whoAmI;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            Projectile.ai[0] = 2;
            return false;
        }

        public override bool PreDrawExtras()
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
            while (distance > 14f && !float.IsNaN(distance))
            {
                distToProj.Normalize();                 //get unit vector
                distToProj *= 14f;                      //speed = 24
                center += distToProj;                   //update draw position
                distToProj = playerCenter - center;    //update distance
                distance = distToProj.Length();
                Color drawColor = lightColor;

                //Draw chain
                Main.spriteBatch.Draw(ChainTexture.Value, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                    new Rectangle(0, 0, 14, 10), drawColor, projRotation,
                    new Vector2(14 * 0.5f, 10 * 0.5f), 1f, SpriteEffects.None, 0f);
            }
            return true;
        }
    }
}