using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Melee
{
	public class GlowYo : ModItem
	{
		public override void SetStaticDefaults()
		{
			// These are all related to gamepad controls and don't seem to affect anything else
			ItemID.Sets.Yoyo[Item.type] = true;
			ItemID.Sets.GamepadExtraRange[Item.type] = 15;
			ItemID.Sets.GamepadSmartQuickReach[Item.type] = true;

			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.useStyle = 5;
			Item.width = 24;
			Item.height = 32;
			Item.useAnimation = 25;
			Item.useTime = 25;
			Item.shootSpeed = 16f;
			Item.knockBack = 2f;
			Item.damage = 14;
			Item.rare = ItemRarityID.Blue;

			Item.DamageType = DamageClass.Melee;
			Item.channel = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;

			Item.UseSound = SoundID.Item1;
			Item.value = Item.sellPrice(silver: 50);
			Item.shoot = ProjectileType<GlowYoProjectile>();
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.Cobweb, 10)
				.AddIngredient(ItemType<Materials.AlkalineFluid>(), 5)
				.AddIngredient(ItemType<Placeable.Blocks.Limestone>(), 15)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}

	public class GlowYoProjectile : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Glow-Yo");

			// The following sets are only applicable to yoyo that use aiStyle 99.
			// YoyosLifeTimeMultiplier is how long in seconds the yoyo will stay out before automatically returning to the player. 
			// Vanilla values range from 3f(Wood) to 16f(Chik), and defaults to -1f. Leaving as -1 will make the time infinite.
			ProjectileID.Sets.YoyosLifeTimeMultiplier[Projectile.type] = 8f;
			// YoyosMaximumRange is the maximum distance the yoyo sleep away from the player. 
			// Vanilla values range from 130f(Wood) to 400f(Terrarian), and defaults to 200f
			ProjectileID.Sets.YoyosMaximumRange[Projectile.type] = 160f;
			// YoyosTopSpeed is top speed of the yoyo Projectile. 
			// Vanilla values range from 9f(Wood) to 17.5f(Terrarian), and defaults to 10f
			ProjectileID.Sets.YoyosTopSpeed[Projectile.type] = 14f;
		}

		public override void SetDefaults()
		{
			Projectile.extraUpdates = 0;
			Projectile.width = 16;
			Projectile.height = 16;
			DrawOffsetX = -1;
			DrawOriginOffsetY = -1;
			DrawOriginOffsetX = 0;

			// aiStyle 99 is used for all yoyos, and is Extremely suggested, as yoyo are extremely difficult without them
			Projectile.aiStyle = 99;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.scale = 1f;
		}
		// notes for aiStyle 99: 
		// localAI[0] is used for timing up to YoyosLifeTimeMultiplier
		// localAI[1] can be used freely by specific types
		// ai[0] and ai[1] usually point towards the x and y world coordinate hover point
		// ai[0] is -1f once YoyosLifeTimeMultiplier is reached, when the player is stoned/frozen, when the yoyo is too far away, or the player is no longer clicking the shoot button.
		// ai[0] being negative makes the yoyo move back towards the player
		// Any AI method can be used for dust, spawning Projectiles, etc specific to your yoyo.

		public override void PostAI()
		{
			Projectile.localAI[1]++;

			if (Projectile.localAI[1] == 30)
			{
				Projectile.localAI[1] = 0;

				if (Main.player[Projectile.owner].channel)
					Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(0, 8), ProjectileType<GlowYoDroplet>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
			}
		}
	}

	public class GlowYoDroplet : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.extraUpdates = 0;
			Projectile.width = 16;
			Projectile.height = 16;
			DrawOffsetX = -1;
			DrawOriginOffsetY = -1;
			DrawOriginOffsetX = 0;

			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.scale = 1f;
		}

		public override void AI()
		{
			Projectile.rotation += 0.1f;
		}

        public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

			Color mainColor = Color.White;

			for (int k = 0; k < Projectile.oldPos.Length; k++)
			{
				Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				float scale = Projectile.scale * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

				float rotation = Projectile.rotation;

				Main.spriteBatch.Draw(texture, Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), color, rotation, new Vector2(texture.Width / 2, texture.Height / Main.projFrames[Projectile.type] / 2), scale, SpriteEffects.None, 0f);
			}

			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), mainColor, Projectile.rotation, new Vector2(texture.Width / 2, texture.Height / Main.projFrames[Projectile.type] / 2), Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 10; i++)
			{
				Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 74, Scale: 1.5f)].noGravity = true;
			}
		}
	}
}
