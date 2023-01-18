using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Polarities.Projectiles;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using ReLogic.Content;
using Terraria.GameContent;

namespace Polarities.Items.Weapons.Magic
{
    public class SeepingDeath : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);

            Item.staff[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(60, 4f, 0);
            Item.DamageType = DamageClass.Magic;
            Item.mana = 15;

            Item.width = 48;
            Item.height = 48;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = 5;
            Item.UseSound = SoundID.Item21;

            Item.noMelee = true;
            Item.knockBack = 4f;

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Pink;

            Item.autoReuse = true;

            Item.shoot = ProjectileType<SeepingDeathProjectile>();
            Item.shootSpeed = 8f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 2; i++)
            {
                position = Main.MouseWorld;

                float angle = Main.rand.NextFloat(MathHelper.PiOver2) + MathHelper.PiOver4;
                Vector2 unit = new Vector2(16, 0).RotatedBy(angle);
                for (int tries = 0; tries < 64; tries++)
                {
                    position += unit;
                    if (!Collision.CanHitLine(Main.MouseWorld, 1, 1, position, 1, 1))
                    {
                        break;
                    }
                }

                Vector2 shootSpeed = -new Vector2(unit.X / 2, unit.Y).SafeNormalize(Vector2.Zero) * velocity.Length();

                Projectile.NewProjectile(source, position, shootSpeed, type, damage, knockback, player.whoAmI);
            }

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Materials.CongealedBrine>(), 6)
                .AddIngredient(ItemType<Placeable.SaltCrystals>(), 6)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class SeepingDeathProjectile : ModProjectile
    {
        static Asset<Texture2D> TrailTexture;

        public override void Load()
        {
            TrailTexture = Request<Texture2D>(Texture + "_Trail");
        }

        public override void Unload()
        {
            TrailTexture = null;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 32;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 360;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            if (Projectile.timeLeft > 240)
                Projectile.velocity = new Vector2(Projectile.velocity.Length(), 0).RotatedBy(Utils.AngleLerp(Projectile.velocity.ToRotation(), (Main.MouseWorld - Projectile.Center).ToRotation(), 0.06f * (Projectile.timeLeft - 240) / 360f));

            Projectile.rotation = Projectile.velocity.ToRotation();

            Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType<Dusts.SaltWaterSplash>(), Scale: 1f)].noGravity = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
            {
                Texture2D altTexture = TrailTexture.Value;
                Main.spriteBatch.Draw(altTexture, Projectile.oldPos[i] + Projectile.Center - Projectile.position - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), lightColor * 0.6f * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length), Projectile.oldRot[i], new Vector2(texture.Width / 2, texture.Height / 2), Projectile.scale * new Vector2(1, ((4 * Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length / 4)), SpriteEffects.None, 0f);
            }

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), lightColor * 0.6f, Projectile.rotation, new Vector2(texture.Width / 2, texture.Height / 2), Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType<Dusts.SaltWaterSplash>(), Scale: 1.5f)].noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffType<Buffs.Desiccating>(), 60);
        }

        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffType<Buffs.Desiccating>(), 60);
        }
    }
}