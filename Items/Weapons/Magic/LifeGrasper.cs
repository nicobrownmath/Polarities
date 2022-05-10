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
using Polarities.Items.Materials;

namespace Polarities.Items.Weapons.Magic
{
	public class LifeGrasper : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.staff[Type] = true;
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(35, 2.5f, 0);
			Item.DamageType = DamageClass.Magic;
			Item.mana = 40;

			Item.width = 44;
			Item.height = 42;

			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.UseSound = SoundID.Item100;
			Item.autoReuse = true;
			Item.noMelee = true;

			Item.value = Item.sellPrice(gold: 10);
			Item.rare = ItemRarityID.LightPurple;

			Item.shoot = ProjectileType<LifeGrasperAura>();
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
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.ClingerStaff)
				.AddIngredient(ItemID.SoulDrain)
				.AddIngredient(ItemType<EvilDNA>())
				.AddTile(TileID.DemonAltar)
				.Register();
		}
	}

	public class LifeGrasperAura : ModProjectile
	{
		public override string Texture => "Polarities/Textures/Pixel";

		public override void SetDefaults()
		{
			Projectile.width = 360;
			Projectile.height = 360;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.timeLeft = 45 * 60;
			Projectile.tileCollide = false;

			Projectile.hide = true;
		}

        public override void OnSpawn(IEntitySource source)
		{
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				if (Main.projectile[i] != Projectile && Main.projectile[i].active && Main.projectile[i].owner == Projectile.owner && Main.projectile[i].type == ProjectileType<LifeGrasperAura>())
				{
					Main.projectile[i].Kill();
				}
			}

			Projectile.velocity = Vector2.Zero;
		}

        public override void AI()
		{
			if (Projectile.timeLeft % 1 == 0)
			{
				Vector2 offset = new Vector2(1, 0).RotatedByRandom(MathHelper.TwoPi);
				int direction = Main.rand.NextBool() ? 1 : -1;
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center + 180 * offset, 4 * offset.RotatedBy(direction * MathHelper.PiOver2), ProjectileType<LifeGrasperAuraRing>(), Projectile.damage, Projectile.knockBack, Projectile.owner, direction, Projectile.whoAmI);
			}
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			knockback = 0f;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.SoulDrain, 30);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float nearestX = Math.Max(targetHitbox.X, Math.Min(Projectile.Center.X, targetHitbox.X + targetHitbox.Size().X));
			float nearestY = Math.Max(targetHitbox.Y, Math.Min(Projectile.Center.Y, targetHitbox.Y + targetHitbox.Size().Y));
			return (new Vector2(Projectile.Center.X - nearestX, Projectile.Center.Y - nearestY)).Length() < Projectile.width / 2;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}
	}

	public class LifeGrasperAuraRing : ModProjectile
	{
		public override string Texture => "Polarities/Textures/Pixel";

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.timeLeft = 60;
			Projectile.tileCollide = false;

			Projectile.hide = true;
		}

		private Vector2 ownerCenter;

		public override void AI()
		{
			if (Projectile.timeLeft == 60)
			{
				ownerCenter = Main.projectile[(int)Projectile.ai[1]].Center;
			}

			Projectile.Center = (Projectile.Center - ownerCenter).SafeNormalize(Vector2.Zero) * 180 + ownerCenter;

			Projectile.velocity = (Projectile.Center - ownerCenter).SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2 * Projectile.ai[0]) * 4;

			Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.LifeDrain, newColor: Color.Transparent, Scale: 1f)].noGravity = true;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.SoulDrain, 30);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float nearestX = Math.Max(targetHitbox.X, Math.Min(Projectile.Center.X, targetHitbox.X + targetHitbox.Size().X));
			float nearestY = Math.Max(targetHitbox.Y, Math.Min(Projectile.Center.Y, targetHitbox.Y + targetHitbox.Size().Y));
			return (new Vector2(Projectile.Center.X - nearestX, Projectile.Center.Y - nearestY)).Length() < Projectile.width / 2;
		}
	}
}