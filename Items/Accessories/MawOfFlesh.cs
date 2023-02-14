using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Projectiles;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Accessories
{
    public class MawOfFlesh : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
            PolaritiesItem.IsFlawless.Add(Type);
        }

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 56;
            Item.accessory = true;
            Item.defense = 4;

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = RarityType<WallOfFleshFlawlessRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PolaritiesPlayer>().dashIndex = GetInstance<MawOfFleshDash>().Index;
        }
    }

    public class MawOfFleshDash : Dash
    {
        public override int Index => 2;

        public override float Speed => 12f;
        public override int Cooldown => 60;
        public override int Duration => 40;

        public override void OnDash(Player player)
        {
            Projectile.NewProjectile(null, player.Center, new Vector2(Speed * 0.33f * (player.velocity.X > 0 ? 1 : -1), 0), ProjectileType<MawOfFleshDashProjectile>(), 25, 8, player.whoAmI);
        }
    }

    public class MawOfFleshDashProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = true;
            Projectile.timeLeft = 3600;

            Projectile.GetGlobalProjectile<PolaritiesProjectile>().ForceDraw = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            if (Math.Abs(Projectile.Center.X - Main.screenPosition.X - Main.screenWidth / 2) > Main.screenWidth) return false;
            const int stepSize = 8;
            for (int i = stepSize * (int)(Main.screenPosition.Y / stepSize); i < Main.screenPosition.Y + Main.screenHeight; i += stepSize)
            {
                const int frameIndex = 0;
                Rectangle frame = new Rectangle(0, stepSize * ((frameIndex + (64 - (i % 64)) / stepSize) % (64 / stepSize)), 36, stepSize);
                Vector2 origin = new Vector2(Projectile.Center.X, i);
                Color cTop = Lighting.GetColor((int)(origin.X / 16), (int)((origin.Y - stepSize / 2) / 16));
                Color cBottom = Lighting.GetColor((int)(origin.X / 16), (int)((origin.Y + stepSize / 2) / 16));
                Color c = new Color(Math.Max(cTop.R, cBottom.R), Math.Max(cTop.G, cBottom.G), Math.Max(cTop.B, cBottom.B));
                Main.spriteBatch.Draw(texture, new Vector2(Projectile.Center.X, i) - Main.screenPosition, frame, c, MathHelper.Pi, new Vector2(36 * 0.5f, stepSize * 0.5f), 1f, Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            }
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                new Vector2(Projectile.Center.X, 0), new Vector2(Projectile.Center.X, Main.maxTilesY * 16),
                36, ref point);
        }

        public override void AI()
        {
            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
            Projectile.velocity *= 1.002f;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
    }
}

