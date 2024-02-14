using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Polarities.Projectiles;
using System;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;

namespace Polarities.Items.Weapons.Magic
{
	public class OceanSky : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(35, 0f, 0);
			Item.DamageType = DamageClass.Magic;
			Item.mana = 20;

			Item.width = 30;
			Item.height = 34;

			Item.useTime = 60;
			Item.useAnimation = 60;
			Item.useStyle = 5;
			Item.noMelee = true;
			Item.UseSound = SoundID.Item9;
			Item.autoReuse = false;

			Item.shoot = ProjectileType<OceanSkyProjectile>();
			Item.shootSpeed = 8f;

			Item.value = 10000;
			Item.rare = ItemRarityID.Blue;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			velocity = new Vector2(player.direction, velocity.Length());
			for (int i = 0; i < 48; i++)
			{
				position.X = player.Center.X + player.direction * i * 50;
				position.Y = player.Center.Y - 1000 - i * 100;
				Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			}
			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.Book)
				.AddIngredient(ItemType<Placeable.Bars.SunplateBar>(), 5)
				.AddIngredient(ItemID.FallenStar, 20)
				.AddTile(TileID.Bookcases)
				.Register();
		}
	}

    public class OceanSkyProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_"+ProjectileID.FallingStar;

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 3600;
            Projectile.tileCollide = true;
            Projectile.light = 0.9f;
            Projectile.scale = 1.2f;
        }

        public override void AI()
        {
            if (Projectile.ai[1] == 0f && !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
            {
                Projectile.ai[1] = 1f;
            }
            if (Projectile.ai[1] != 0f)
            {
                Projectile.tileCollide = true;
            }
            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = 20 + Main.rand.Next(40);
                SoundEngine.PlaySound(SoundID.Item9, Projectile.position);
            }
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
            }
            Projectile.alpha += (int)(25f * Projectile.localAI[0]);
            if (Projectile.alpha > 200)
            {
                Projectile.alpha = 200;
                Projectile.localAI[0] = -1f;
            }
            if (Projectile.alpha < 0)
            {
                Projectile.alpha = 0;
                Projectile.localAI[0] = 1f;
            }
            Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.01f * (float)Projectile.direction;

            Vector2 value34 = new Vector2((float)Main.screenWidth, (float)Main.screenHeight);
            if (Projectile.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + value34 / 2f, value34 + new Vector2(400f))) && Main.rand.Next(6) == 0)
            {
                int[] array6 = new int[4] { 16, 17, 17, 17 };
                int num855 = Utils.SelectRandom(Main.rand, array6);
                if (Main.tenthAnniversaryWorld)
                {
                    int[] array7 = new int[4] { 16, 16, 16, 17 };
                    num855 = Utils.SelectRandom(Main.rand, array7);
                }
                Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.position, Projectile.velocity * 0.2f, num855);
            }
            Projectile.light = 0.9f;
            if (Main.rand.Next(20) == 0 || (Main.tenthAnniversaryWorld && Main.rand.Next(15) == 0))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 58, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 150, default(Color), 1.2f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            Color newColor7 = Color.CornflowerBlue;
            if (Main.tenthAnniversaryWorld)
            {
                newColor7 = Color.HotPink;
                newColor7.A = (byte)(newColor7.A / 2);
            }
            for (int num573 = 0; num573 < 7; num573++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 58, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 150, default(Color), 0.8f);
            }
            for (float num574 = 0f; num574 < 1f; num574 += 0.125f)
            {
                Vector2 center25 = Projectile.Center;
                Vector2 unitY11 = Vector2.UnitY;
                double radians36 = num574 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f;
                Vector2 center2 = default(Vector2);
                Dust.NewDustPerfect(center25, 278, unitY11.RotatedBy(radians36, center2) * (4f + Main.rand.NextFloat() * 4f), 150, newColor7).noGravity = true;
            }
            for (float num575 = 0f; num575 < 1f; num575 += 0.25f)
            {
                Vector2 center26 = Projectile.Center;
                Vector2 unitY12 = Vector2.UnitY;
                double radians37 = num575 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f;
                Vector2 center2 = default(Vector2);
                Dust.NewDustPerfect(center26, 278, unitY12.RotatedBy(radians37, center2) * (2f + Main.rand.NextFloat() * 3f), 150, Color.Gold).noGravity = true;
            }
            Vector2 value15 = new Vector2((float)Main.screenWidth, (float)Main.screenHeight);
            if (Projectile.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + value15 / 2f, value15 + new Vector2(400f))))
            {
                for (int num576 = 0; num576 < 7; num576++)
                {
                    Vector2 val29 = Projectile.position;
                    Vector2 val30 = Main.rand.NextVector2CircularEdge(0.5f, 0.5f) * Projectile.velocity.Length();
                    int[] array18 = new int[8] { 16, 17, 17, 17, 17, 17, 17, 17 };
                    Gore.NewGore(Projectile.GetSource_Death(), val29, val30, Utils.SelectRandom(Main.rand, array18));
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            lightColor = Color.White;

            SpriteEffects spriteEffects = (SpriteEffects)0;
            if (Projectile.spriteDirection == -1)
            {
                spriteEffects = (SpriteEffects)1;
            }

            Texture2D value174 = TextureAssets.Projectile[Type].Value;
            Rectangle rectangle24 = value174.Frame();
            Vector2 origin33 = rectangle24.Size() / 2f;
            Color alpha13 = Projectile.GetAlpha(lightColor);
            Texture2D value175 = TextureAssets.Extra[91].Value;
            Rectangle value176 = value175.Frame();
            Vector2 origin10 = new Vector2((float)value176.Width / 2f, 10f);
            Vector2 value177 = new Vector2(0f, Projectile.gfxOffY);
            Vector2 spinningpoint = new Vector2(0f, -10f);
            float num184 = (float)Main.timeForVisualEffects / 60f;
            Vector2 value178 = Projectile.Center + Projectile.velocity;
            Color color42 = Color.Blue * 0.2f;
            Color value179 = Color.White * 0.5f;
            value179.A = 0;
            float num185 = 0f;
            if (Main.tenthAnniversaryWorld)
            {
                color42 = Color.HotPink * 0.3f;
                value179 = Color.White * 0.75f;
                value179.A = 0;
                num185 = -0.1f;
            }
            Color color43 = color42;
            color43.A = 0;
            Color color44 = color42;
            color44.A = 0;
            Color color45 = color42;
            color45.A = 0;
            Vector2 val8 = value178 - Main.screenPosition + value177;
            Vector2 spinningpoint17 = spinningpoint;
            double radians6 = (float)Math.PI * 2f * num184;
            Vector2 val2 = default(Vector2);
            Main.EntitySpriteDraw(value175, val8 + spinningpoint17.RotatedBy(radians6, val2), value176, color43, Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin10, 1.5f + num185, (SpriteEffects)0, 0);
            Vector2 val9 = value178 - Main.screenPosition + value177;
            Vector2 spinningpoint18 = spinningpoint;
            double radians7 = (float)Math.PI * 2f * num184 + (float)Math.PI * 2f / 3f;
            val2 = default(Vector2);
            Main.EntitySpriteDraw(value175, val9 + spinningpoint18.RotatedBy(radians7, val2), value176, color44, Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin10, 1.1f + num185, (SpriteEffects)0, 0);
            Vector2 val10 = value178 - Main.screenPosition + value177;
            Vector2 spinningpoint19 = spinningpoint;
            double radians8 = (float)Math.PI * 2f * num184 + 4.1887903f;
            val2 = default(Vector2);
            Main.EntitySpriteDraw(value175, val10 + spinningpoint19.RotatedBy(radians8, val2), value176, color45, Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin10, 1.3f + num185, (SpriteEffects)0, 0);
            Vector2 value180 = Projectile.Center - Projectile.velocity * 0.5f;
            for (float num186 = 0f; num186 < 1f; num186 += 0.5f)
            {
                float num187 = num184 % 0.5f / 0.5f;
                num187 = (num187 + num186) % 1f;
                float num188 = num187 * 2f;
                if (num188 > 1f)
                {
                    num188 = 2f - num188;
                }
                Main.EntitySpriteDraw(value175, value180 - Main.screenPosition + value177, value176, value179 * num188, Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin10, 0.3f + num187 * 0.5f, (SpriteEffects)0, 0);
            }
            Main.EntitySpriteDraw(value174, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), rectangle24, alpha13, Projectile.rotation, origin33, Projectile.scale + 0.1f, spriteEffects, 0);

            return false;
        }
    }
}