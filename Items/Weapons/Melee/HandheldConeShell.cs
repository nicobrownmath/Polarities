using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Polarities.Projectiles;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using ReLogic.Content;

namespace Polarities.Items.Weapons.Melee
{
    public class HandheldConeShell : ModItem
    {
        public override void SetStaticDefaults()
        {
            this.SetResearch(1);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(1, 0, 0);
            Item.DamageType = DamageClass.Melee;

            Item.width = 40;
            Item.height = 18;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = 5;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.channel = false;

            Item.shoot = ProjectileType<HandheldConeShellProjectile>();
            Item.shootSpeed = 12f;

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Pink;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-20, 0);
        }
    }
    public class HandheldConeShellProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 3600;
            Projectile.tileCollide = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (Main.player[Projectile.owner].dead)
            {
                Projectile.Kill();
                return;
            }
            Main.player[Projectile.owner].itemAnimation = 5;
            Main.player[Projectile.owner].itemTime = 5;
            if (Projectile.alpha == 0)
            {
                if (Projectile.position.X + (float)(Projectile.width / 2) > Main.player[Projectile.owner].position.X + (float)(Main.player[Projectile.owner].width / 2))
                {
                    Main.player[Projectile.owner].ChangeDir(1);
                }
                else
                {
                    Main.player[Projectile.owner].ChangeDir(-1);
                }
            }
            Vector2 vector140 = Projectile.Center;
            float num2390 = Main.player[Projectile.owner].position.X + (float)(Main.player[Projectile.owner].width / 2) - vector140.X;
            float num2389 = Main.player[Projectile.owner].position.Y + (float)(Main.player[Projectile.owner].height / 2) - vector140.Y;
            float num2388 = (float)Math.Sqrt(num2390 * num2390 + num2389 * num2389);
            if (Projectile.ai[0] == 0f)
            {
                if (num2388 > 64f)
                {
                    Projectile.ai[0] = 1f;
                }
                Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + 1.57f;
                Projectile.ai[1] += 1f;
                if (Projectile.ai[1] > 5f)
                {
                    Projectile.alpha = 0;
                }
                if (Projectile.ai[1] > 8f)
                {
                    Projectile.ai[1] = 8f;
                }
            }
            else if (Projectile.ai[0] == 1f)
            {
                Projectile.tileCollide = false;
                Projectile.rotation = (float)Math.Atan2(num2389, num2390) - 1.57f;
                float num2387 = 20f;
                if (num2388 < 20f)
                {
                    Projectile.Kill();
                }
                num2388 = num2387 / num2388;
                num2390 *= num2388;
                num2389 *= num2388;
                Projectile.velocity.X = num2390;
                Projectile.velocity.Y = num2389;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Projectile.ai[0] = 1;
            target.AddBuff(BuffType<Buffs.ConeVenom>(), 60 * 20);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            Projectile.ai[0] = 1;
            return false;
        }

        static Asset<Texture2D> ChainTexture;

        public override void Load()
        {
            ChainTexture = Request<Texture2D>(Texture + "_Chain");
        }

        public override void Unload()
        {
            ChainTexture = null;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 playerCenter = Main.player[Projectile.owner].MountedCenter;
            Vector2 center = Projectile.Center;
            Vector2 distToProj = playerCenter - Projectile.Center;
            float projRotation = distToProj.ToRotation();
            float distance = distToProj.Length();
            while (distance > 2f && !float.IsNaN(distance))
            {
                distToProj.Normalize();                 //get unit vector
                distToProj *= 2f;                      //speed = 24
                center += distToProj;                   //update draw Projectile.position
                distToProj = playerCenter - center;    //update distance
                distance = distToProj.Length();
                Color drawColor = lightColor;

                //Draw chain
                Main.EntitySpriteDraw(ChainTexture.Value, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                    new Rectangle(0, 0, 2, 2), drawColor, projRotation,
                    new Vector2(2 * 0.5f, 2 * 0.5f), 1f, SpriteEffects.None, 0);
            }
            return true;
        }
    }
}