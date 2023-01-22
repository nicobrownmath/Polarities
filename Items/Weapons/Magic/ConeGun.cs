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

namespace Polarities.Items.Weapons.Magic
{
    public class ConeGun : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 900;
            Item.DamageType = DamageClass.Magic;

            Item.width = 48;
            Item.height = 26;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 12f;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item61;
            Item.autoReuse = false;
            Item.shoot = ProjectileType<ConeGunProjectile>();
            Item.shootSpeed = 12f;
        }

        public override bool CanUseItem(Player player)
        {
            return player.statMana > 0;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            damage = (int)(damage * player.statMana * player.statMana / 40000f);

            if (player.HasBuff(BuffID.ManaSickness))
            {
                damage /= 4;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.statMana = 0;

            player.GetModPlayer<PolaritiesPlayer>().AddScreenShake(15, 15);
            return true;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-20, 4);
        }
    }

    public class ConeGunProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Giant Cone");
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 46;
            Projectile.height = 46;

            DrawOffsetX = -146;
            DrawOriginOffsetY = 0;
            DrawOriginOffsetX = 73;

            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.penetrate = -1;

            Projectile.timeLeft = 3600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                SoundEngine.PlaySound(SoundID.Item38, Projectile.Center);
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            Projectile.ai[1] += Projectile.velocity.Length();
            if (Projectile.ai[1] > 169)
            {
                Projectile.ai[1] = 169;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 unit = new Vector2(-1, 0).RotatedBy(Projectile.rotation);
            float Distance = Math.Min(Projectile.ai[1], 146);

            float point = 0f;
            // Run an AABB versus Line check to look for collisions, look up AABB collision first to see how it works
            // It will look for collisions on the given line using AABB
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
                Projectile.Center + unit * Distance, 46, ref point);
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            lightColor = Lighting.GetColor((int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16));

            Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(169 - (int)Projectile.ai[1], Projectile.frame * 46, (int)Projectile.ai[1] + 23, 46), lightColor, Projectile.rotation, new Vector2(Projectile.ai[1], 23), 1f, SpriteEffects.None, 0f);

            return false;
        }
    }
}