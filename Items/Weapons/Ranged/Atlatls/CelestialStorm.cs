using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Ranged.Atlatls
{
    public class CelestialStorm : AtlatlBase
    {
        public override Vector2[] ShotDistances => new Vector2[] { new Vector2(52) };
        public override float BaseShotDistance => 52;

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(4, 3));

            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(82, 3, 0);
            Item.DamageType = DamageClass.Ranged;

            Item.width = 66;
            Item.height = 66;

            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            Item.shoot = 10;
            Item.shootSpeed = 6f;
            Item.useAmmo = AmmoID.Dart;

            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Red;
        }

        public override bool RealShoot(Player player, EntitySource_ItemUse_WithAmmo source, int index, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ProjectileType<CelestialStormProjectile>(), damage, knockback, player.whoAmI, ai0: type);

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FragmentVortex, 18)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override bool DoVanillaDraw() => false;

        public override void DrawHeldItem(ref PlayerDrawSet drawInfo)
        {
            //custom animated draw
            int textureFrames = 3;
            int frame = (PolaritiesSystem.timer / 4) % 3;

            Texture2D texture = TextureAssets.Item[Type].Value;

            SpriteEffects spriteEffects = (SpriteEffects)((drawInfo.drawPlayer.gravDir != 1f) ? ((drawInfo.drawPlayer.direction != 1) ? 3 : 2) : ((drawInfo.drawPlayer.direction != 1) ? 1 : 0));

            if (drawInfo.drawPlayer.gravDir == -1)
            {
                DrawData drawData = new DrawData(texture, new Vector2((int)(drawInfo.drawPlayer.itemLocation.X - Main.screenPosition.X), (int)(drawInfo.drawPlayer.itemLocation.Y - Main.screenPosition.Y)), (Rectangle?)new Rectangle(0, frame * texture.Height / textureFrames, texture.Width, texture.Height / textureFrames), Color.White, drawInfo.drawPlayer.itemRotation, new Vector2(texture.Width * 0.5f - texture.Width * 0.5f * drawInfo.drawPlayer.direction, 0f), drawInfo.drawPlayer.inventory[drawInfo.drawPlayer.selectedItem].scale, spriteEffects, 0);
                drawInfo.DrawDataCache.Add(drawData);
            }
            else
            {
                Vector2 value21 = Vector2.Zero;
                int type6 = drawInfo.drawPlayer.inventory[drawInfo.drawPlayer.selectedItem].type;
                DrawData drawData = new DrawData(texture, new Vector2((int)(drawInfo.drawPlayer.itemLocation.X - Main.screenPosition.X), (int)(drawInfo.drawPlayer.itemLocation.Y - Main.screenPosition.Y)), (Rectangle?)new Rectangle(0, frame * texture.Height / textureFrames, texture.Width, texture.Height / textureFrames), Color.White, drawInfo.drawPlayer.itemRotation, new Vector2(texture.Width * 0.5f - (float)texture.Height / textureFrames * 0.5f * drawInfo.drawPlayer.direction, (float)texture.Height / textureFrames) + value21, drawInfo.drawPlayer.inventory[drawInfo.drawPlayer.selectedItem].scale, spriteEffects, 0);
                drawInfo.DrawDataCache.Add(drawData);
            }

            base.DrawHeldItem(ref drawInfo);
        }

        public override bool DoDartDraw(int index, ref PlayerDrawSet drawInfo)
        {
            return base.DoDartDraw(index, ref drawInfo);
        }
    }

    public class CelestialStormProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_641";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.width = 60;
            Projectile.height = 60;

            Projectile.alpha = 255;

            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.timeLeft = 180;
        }

        public override void AI()
        {
            if ((Projectile.timeLeft == 60 || Projectile.timeLeft == 80 || Projectile.timeLeft == 100) && Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero) * 20, (int)Projectile.ai[0], Projectile.damage, Projectile.knockBack, Projectile.owner);
            }

            Projectile.velocity *= 0.98f;
            Projectile.velocity += (Main.player[Projectile.owner].Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 0.2f;

            if (Projectile.timeLeft > 50)
            {
                Projectile.alpha -= 5;
            }
            else
            {
                Projectile.alpha += 5;
            }
            if (Projectile.alpha < 0)
            {
                Projectile.alpha = 0;
            }

            Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
            Projectile.spriteDirection = Projectile.direction;

            Projectile.rotation -= Projectile.direction * (MathHelper.Pi * 2f) / 120f;
            Projectile.scale = Projectile.Opacity;
            Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.9f, 0.7f) * Projectile.Opacity);
            if (Main.rand.NextBool(2))
            {
                Vector2 vector152 = Vector2.UnitY.RotatedByRandom(6.2831854820251465);
                Dust[] dust366 = Main.dust;
                Vector2 position240 = Projectile.Center - vector152 * 30f;
                Color newColor4 = default(Color);
                Dust dust59 = dust366[Dust.NewDust(position240, 0, 0, DustID.Vortex, 0f, 0f, 0, newColor4)];
                dust59.noGravity = true;
                dust59.position = Projectile.Center - vector152 * Main.rand.Next(10, 21);
                Vector2 val4 = default(Vector2);
                dust59.velocity = vector152.RotatedBy(1.5707963705062866, val4) * 6f;
                dust59.scale = 0.5f + Main.rand.NextFloat();
                dust59.fadeIn = 0.5f;
                dust59.customData = Projectile.Center;
            }
            if (Main.rand.NextBool(2))
            {
                Vector2 vector151 = Vector2.UnitY.RotatedByRandom(6.2831854820251465);
                Dust[] dust367 = Main.dust;
                Vector2 position241 = Projectile.Center - vector151 * 30f;
                Color newColor4 = default(Color);
                Dust dust58 = dust367[Dust.NewDust(position241, 0, 0, DustID.Granite, 0f, 0f, 0, newColor4)];
                dust58.noGravity = true;
                dust58.position = Projectile.Center - vector151 * 30f;
                Vector2 val4 = default(Vector2);
                dust58.velocity = vector151.RotatedBy(-1.5707963705062866, val4) * 3f;
                dust58.scale = 0.5f + Main.rand.NextFloat();
                dust58.fadeIn = 0.5f;
                dust58.customData = Projectile.Center;
            }
            Projectile.ai[1] -= Projectile.direction * (MathHelper.Pi / 8f) / 50f;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }

        public override bool CanHitPvp(Player target)
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = 0;
            if (Projectile.spriteDirection == -1)
            {
                spriteEffects = (SpriteEffects)1;
            }

            Color color78 = Lighting.GetColor((int)(Projectile.position.X + Projectile.width * 0.5) / 16, (int)((Projectile.position.Y + Projectile.height * 0.5) / 16.0));

            Vector2 vector64 = Projectile.position + new Vector2(Projectile.width, Projectile.height) / 2f + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
            Texture2D texture2D20 = TextureAssets.Projectile[Type].Value;
            Color alpha7 = Projectile.GetAlpha(color78);
            Vector2 origin14 = new Vector2(texture2D20.Width, texture2D20.Height) / 2f;

            Color color72 = alpha7 * 0.8f;
            color72.A = (byte)(color72.A / 2);
            Color color73 = Color.Lerp(alpha7, Color.Black, 0.5f);
            color73.A = alpha7.A;
            float num391 = 0.95f + (Projectile.rotation * 0.75f).ToRotationVector2().Y * 0.1f;
            color73 *= num391;
            float scale6 = 0.6f + Projectile.scale * 0.6f * num391;
            Main.EntitySpriteDraw(TextureAssets.Extra[50].Value, vector64, null, color73, 0f - Projectile.rotation + 0.35f, origin14, scale6, (SpriteEffects)((int)spriteEffects ^ 1), 0);
            Main.EntitySpriteDraw(TextureAssets.Extra[50].Value, vector64, null, alpha7, 0f - Projectile.rotation, origin14, Projectile.scale, (SpriteEffects)((int)spriteEffects ^ 1), 0);
            Main.EntitySpriteDraw(texture2D20, vector64, null, color72, (0f - Projectile.rotation) * 0.7f, origin14, Projectile.scale, (SpriteEffects)((int)spriteEffects ^ 1), 0);
            Main.EntitySpriteDraw(TextureAssets.Extra[50].Value, vector64, null, alpha7 * 0.8f, Projectile.rotation * 0.5f, origin14, Projectile.scale * 0.9f, spriteEffects, 0);
            alpha7.A = 0;

            Main.EntitySpriteDraw(texture2D20, vector64, null, alpha7, Projectile.rotation, origin14, Projectile.scale, spriteEffects, 0);

            return false;
        }
    }
}