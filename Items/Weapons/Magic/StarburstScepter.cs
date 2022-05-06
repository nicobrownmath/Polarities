using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Magic
{
	public class StarburstScepter : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(40, 3.5f, 0);
			Item.DamageType = DamageClass.Magic;
			Item.mana = 10;

			Item.width = 44;
			Item.height = 40;

			Item.useTime = 20;
			Item.useAnimation = 19;
			Item.useStyle = 1;
			Item.autoReuse = true;
			Item.noMelee = true;

			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.LightPurple;

			Item.UseSound = SoundID.Item8;
			Item.shoot = ProjectileType<StarburstScepterProjectile>();
			Item.shootSpeed = 1f;
		}

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			while (Collision.CanHitLine(player.position, player.width, player.height, position, 1, 1) && (position - player.Center).Length() < (Main.MouseWorld - player.Center).Length())
			{
				position += velocity * 8;
			}
			if ((position - player.Center).Length() < (Main.MouseWorld - player.Center).Length())
			{
				position -= velocity * 8;
			}
			else
			{
				position = Main.MouseWorld;
			}
			velocity = Vector2.Zero;
		}
	}

	public class StarburstScepterProjectile : ModProjectile
	{
		public override string Texture => "Polarities/Textures/Glow58";

		public override void SetDefaults()
		{
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.timeLeft = 24;
			Projectile.tileCollide = false;
			Projectile.light = 1f;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void AI()
		{
			if (Projectile.ai[0] == 0)
			{
				if (Projectile.timeLeft % 2 == 0 && Main.rand.NextBool())
				{
					Projectile p = Main.projectile[Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center + new Vector2(70 - Projectile.timeLeft * 2, 0).RotatedByRandom(MathHelper.TwoPi), Vector2.Zero, ProjectileType<StarburstScepterProjectile>(), Projectile.damage / 3, Projectile.knockBack / 2, Projectile.owner, ai0: 1)];
					Vector2 oldCenter = p.Center;
					p.scale = Main.rand.NextFloat(0.2f, 0.3f);
					p.width = (int)(p.width * p.scale);
					p.height = (int)(p.height * p.scale);
				}
			}
		}

		public override bool ShouldUpdatePosition()
		{
			return false;
		}

        public override bool PreDraw(ref Color lightColor)
		{
			float rayScale = Math.Max(0, (Projectile.timeLeft - 12) / 6f) * 2;

			Texture2D rayTexture = TextureAssets.Projectile[ProjectileID.RainbowCrystalExplosion].Value;
			Rectangle rayFrame = rayTexture.Frame();
			Main.EntitySpriteDraw(rayTexture, Projectile.Center - Main.screenPosition, rayFrame, Color.White, MathHelper.PiOver4, rayFrame.Size() / 2, new Vector2(1f, rayScale) * Projectile.scale, SpriteEffects.None, 0);
			Main.EntitySpriteDraw(rayTexture, Projectile.Center - Main.screenPosition, rayFrame, Color.White, 3 * MathHelper.PiOver4, rayFrame.Size() / 2, new Vector2(1f, rayScale) * Projectile.scale, SpriteEffects.None, 0);

			float auraScale = (float)Math.Sin(MathHelper.Pi * Projectile.timeLeft / 24f);

			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame();

			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, frame.Size() / 2, Projectile.scale * auraScale, SpriteEffects.None, 0);

			return false;
		}
	}
}