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
using Terraria.GameContent;
using ReLogic.Content;

namespace Polarities.Items.Weapons.Melee
{
    public class Snakebite : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(180, 1f, 0);
            Item.DamageType = DamageClass.Melee;

            Item.width = 38;
            Item.height = 38;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = 5;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.channel = true;

            Item.shoot = ProjectileType<SnakebiteProjectile>();
            Item.shootSpeed = 24f;

            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.LightPurple;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Materials.VenomGland>(), 4)
                .AddRecipeGroup("AdamantiteBar", 12)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class SnakebiteProjectile : ModProjectile
    {
        //texture cacheing
        public static Asset<Texture2D> ChainTexture;
        public static Asset<Texture2D> JawTexture;

        public override void Load()
        {
            ChainTexture = Request<Texture2D>(Texture + "_Chain");
            JawTexture = Request<Texture2D>(Texture + "_Jaw");
        }

        public override void Unload()
        {
            ChainTexture = null;
            JawTexture = null;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 3600;
            Projectile.tileCollide = true;

            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            player.direction = (Projectile.Center.X - player.Center.X > 0) ? 1 : -1;
            player.itemAnimation = 10;
            player.itemTime = 10;

            Projectile.rotation = (Projectile.Center.X - player.Center.X > 0) ? (Projectile.Center - player.Center).ToRotation() : (Projectile.Center - player.Center).ToRotation() + MathHelper.Pi;
            Projectile.direction = (Projectile.Center.X - player.Center.X > 0) ? 1 : -1;
            Projectile.spriteDirection = Projectile.direction;

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

            if (dToPlayer > 500)
            {
                Projectile.ai[0] = 1;
            }

            if (Projectile.ai[0] != 0)
            {
                //retracting (adapted from vanilla flail code)
                float num213 = 14f / Main.player[Projectile.owner].GetTotalAttackSpeed(DamageClass.Melee);
                float num214 = 0.9f / Main.player[Projectile.owner].GetTotalAttackSpeed(DamageClass.Melee);
                Math.Abs(num209);
                Math.Abs(num210);
                Projectile.tileCollide = false;
                Projectile.damage = 0;
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
            }
            else
            {
                float speed = Projectile.velocity.Length();
                Projectile.velocity += (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero) * 2f;
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * speed;
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
                Projectile.damage = 0;
            }

            target.AddBuff(BuffID.Venom, 300);
        }

        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            if (Projectile.ai[0] == 0)
            {
                Projectile.ai[0] = 1;
                Projectile.damage = 0;
            }

            target.AddBuff(BuffID.Venom, 300);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            Projectile.ai[0] = 1;
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 playerCenter = Main.player[Projectile.owner].MountedCenter;
            Vector2 center = Projectile.Center;
            Vector2 distToProj = playerCenter - Projectile.Center;
            float projRotation = Projectile.rotation;
            float distance = distToProj.Length();

            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            int segments = 0;
            while (distance > 12f && !float.IsNaN(distance))
            {
                distToProj.Normalize();
                distToProj *= 12f;
                center += distToProj;
                distToProj = playerCenter - center;
                distance = distToProj.Length();
                Color drawColor = Projectile.GetAlpha(lightColor);

                //Draw chain
                Main.spriteBatch.Draw(ChainTexture.Value, new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                    new Rectangle(0, 0, 12, 20), drawColor, projRotation,
                    new Vector2(12 * 0.5f, 20 * 0.5f), new Vector2(1f, 20f / (segments + 20f)), spriteEffects, 0);

                segments++;
            }

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D lowerJawTexture = JawTexture.Value;
            Rectangle frame = texture.Frame();
            Vector2 realOrigin = frame.Size() / 2;
            Vector2 origin = new Vector2(12 - Projectile.spriteDirection * 10, 20);
            Vector2 drawOffset = (origin - realOrigin).RotatedBy(Projectile.rotation);
            Vector2 drawPos = Projectile.Center - Main.screenPosition + drawOffset;

            float jawAngle = Projectile.ai[0] == 0 ? 0.5f : 0f;

            Main.spriteBatch.Draw(lowerJawTexture, drawPos, frame, Projectile.GetAlpha(lightColor), Projectile.rotation + jawAngle * Projectile.spriteDirection, origin, Projectile.scale, spriteEffects, 0);
            Main.spriteBatch.Draw(texture, drawPos, frame, Projectile.GetAlpha(lightColor), Projectile.rotation - jawAngle * Projectile.spriteDirection, origin, Projectile.scale, spriteEffects, 0);

            return false;
        }
    }
}