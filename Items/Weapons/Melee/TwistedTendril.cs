using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Polarities.Projectiles;
using System;
using Terraria.DataStructures;

namespace Polarities.Items.Weapons.Melee
{
	public class TwistedTendril : ModItem
	{
		public override void SetStaticDefaults()
		{
			SacrificeTotal = (3);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(40, 1, 0);
			Item.DamageType = DamageClass.Melee;

			Item.width = 42;
			Item.height = 42;

			Item.useTime = 18;
			Item.useAnimation = 18;
			Item.useStyle = 1;
			Item.noMelee = true;
			Item.noUseGraphic = true;

			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.LightPurple;

			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.shoot = ProjectileType<TwistedTendrilProjectile>();
			Item.shootSpeed = 16f;
			Item.maxStack = 3;
		}

		public override void UpdateInventory(Player player)
		{
			Item.maxStack = 3;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			float angle = Main.rand.NextFloat(MathHelper.TwoPi);
			for (int i = 0; i < Item.stack; i++)
			{
				Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, angle + MathHelper.TwoPi * i / Item.stack, 0);
			}
			return false;
		}
	}

	public class TwistedTendrilProjectile : ModProjectile
	{
		public override string Texture => "Polarities/Items/Weapons/Melee/TwistedTendril";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("{$Mods.Polarities.ItemName.TwistedTendril}");
		}
		public override void SetDefaults()
		{
			Projectile.width = 42;
			Projectile.height = 42;

			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 30;
			Projectile.tileCollide = true;
		}

		private Vector2 startPosition;
		private Vector2 startVelocity;

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			if (player.dead || !player.active)
			{
				Projectile.active = false;
			}

			if (Projectile.timeLeft == 30)
			{
				startPosition = Projectile.Center;
				startVelocity = Projectile.velocity;
			}

			Vector2 goalPosition = startPosition + startVelocity.SafeNormalize(Vector2.Zero) * 16 * Projectile.ai[1] + startVelocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * 64 * (float)Math.Sin(Projectile.ai[0] + Projectile.ai[1] / 10f) * (float)Math.Sin(MathHelper.Pi * Projectile.ai[1] / 30f);

			Projectile.velocity = goalPosition - Projectile.Center;

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

			Projectile.ai[1]++;
		}

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			width = 2;
			height = 2;
			return true;
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 3; i++)
			{
				Vector2 dustPos = Projectile.Center + new Vector2(Main.rand.NextFloat(16), 0).RotatedByRandom(MathHelper.TwoPi);
				Dust dust = Dust.NewDustPerfect(dustPos, 18, Velocity: (dustPos - Projectile.Center) / 6, Alpha: 32, Scale: 1.4f);
				dust.noGravity = true;
			}
		}
	}
}