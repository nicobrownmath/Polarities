using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Polarities.Projectiles;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace Polarities.Items.Weapons.Magic
{
    public class Halocline : ModItem
    {
        public override void SetStaticDefaults()
        {
            this.SetResearch(1);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(22, 0f, 0);
            Item.DamageType = DamageClass.Magic;
            Item.mana = 10;

            Item.width = 30;
            Item.height = 32;

            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = 5;
            Item.noMelee = true;
            Item.UseSound = SoundID.Item61;

            Item.shoot = ProjectileType<HaloclineBubbleLarge>();
            Item.shootSpeed = 8f;

            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ItemRarityID.Blue;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Book)
                .AddIngredient(ItemType<Items.Placeable.SaltCrystals>(), 15)
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }

    public class HaloclineBubbleLarge : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.velocity.Y += 0.15f;
            Projectile.rotation = Projectile.velocity.ToRotation();

            for (int i = 0; i < 2; i++)
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType<Dusts.SaltWaterSplash>(), Scale: 1.5f)].noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                Main.spriteBatch.Draw(texture, Projectile.oldPos[i] + Projectile.Center - Projectile.position - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), lightColor * 0.6f * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length), Projectile.rotation, new Vector2(texture.Width / 2, texture.Height / 2), Projectile.scale * ((4 * Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length / 4), SpriteEffects.None, 0f);
            }

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float nearestX = Math.Max(targetHitbox.X, Math.Min(Projectile.Center.X, targetHitbox.X + targetHitbox.Size().X));
            float nearestY = Math.Max(targetHitbox.Y, Math.Min(Projectile.Center.Y, targetHitbox.Y + targetHitbox.Size().Y));
            return (new Vector2(Projectile.Center.X - nearestX, Projectile.Center.Y - nearestY)).Length() < Projectile.width / 2;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = 34;
            height = 34;
            fallThrough = true;
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Projectile.velocity + new Vector2(4, 0).RotatedByRandom(MathHelper.TwoPi), ProjectileType<HaloclineBubbleSmall>(), Projectile.damage / 2, Projectile.knockBack / 2, Projectile.owner);
            }
            for (int i = 0; i < 24; i++)
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType<Dusts.SaltWaterSplash>(), Scale: 1.5f)].noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffType<Buffs.Desiccating>(), 3 * 60);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffType<Buffs.Desiccating>(), 3 * 60);
        }
    }

    public class HaloclineBubbleSmall : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("{$Mods.Polarities.ProjectileName.HaloclineBubbleLarge}");

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.velocity.Y += 0.15f;
            Projectile.rotation = Projectile.velocity.ToRotation();

            Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType<Dusts.SaltWaterSplash>(), Scale: 1.5f * Projectile.scale)].noGravity = true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X * 0.8f;
                Projectile.velocity *= 0.9f;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y * 0.8f;
                Projectile.velocity *= 0.9f;
            }

            Projectile.scale -= 0.1f;
            for (int i = 0; i < 6 * Projectile.scale; i++)
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType<Dusts.SaltWaterSplash>(), Scale: 1.5f * Projectile.scale)].noGravity = true; ;
            }

            if (Projectile.scale <= 0)
            {
                Projectile.Kill();
                return false;
            }

            Vector2 oldCenter = Projectile.Center;
            Projectile.width = (int)(32 * Projectile.scale);
            Projectile.height = (int)(32 * Projectile.scale);
            Projectile.Center = oldCenter;
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                Main.spriteBatch.Draw(texture, Projectile.oldPos[i] + Projectile.Center - Projectile.position - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), lightColor * 0.6f * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length), Projectile.rotation, new Vector2(texture.Width / 2, texture.Height / 2), Projectile.scale * ((2 * Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length / 2), SpriteEffects.None, 0f);
            }

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float nearestX = Math.Max(targetHitbox.X, Math.Min(Projectile.Center.X, targetHitbox.X + targetHitbox.Size().X));
            float nearestY = Math.Max(targetHitbox.Y, Math.Min(Projectile.Center.Y, targetHitbox.Y + targetHitbox.Size().Y));
            return (new Vector2(Projectile.Center.X - nearestX, Projectile.Center.Y - nearestY)).Length() < Projectile.width / 2;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = (int)(22 * Projectile.scale);
            height = (int)(22 * Projectile.scale);
            fallThrough = true;
            return true;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.SourceDamage *= Projectile.scale;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            OnHitAnything();
        }

        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            modifiers.SourceDamage *= Projectile.scale;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            OnHitAnything();
        }

        private void OnHitAnything()
        {
            Projectile.scale -= 0.1f;
            for (int i = 0; i < 6 * Projectile.scale; i++)
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType<Dusts.SaltWaterSplash>(), Scale: 1.5f * Projectile.scale)].noGravity = true; ;
            }

            if (Projectile.scale <= 0)
            {
                Projectile.Kill();
                return;
            }

            Vector2 oldCenter = Projectile.Center;
            Projectile.width = (int)(32 * Projectile.scale);
            Projectile.height = (int)(32 * Projectile.scale);
            Projectile.Center = oldCenter;
        }
    }
}
