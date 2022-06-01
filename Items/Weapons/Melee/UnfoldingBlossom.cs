using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Projectiles;
using System;
using System.Collections.Generic;

namespace Polarities.Items.Weapons
{
	public class UnfoldingBlossom : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Unfolding Blossom");
			Tooltip.SetDefault("Flawless");
		}

		public override void SetDefaults()
		{
			item.damage = 160;
			item.knockBack = 3;
			item.melee = true;
			item.width = 42;
			item.height = 40;
			item.useTime = 1;
			item.useAnimation = 1;
			item.noUseGraphic = true;
			item.useStyle = 5;
			item.noMelee = true;
			item.value = Item.sellPrice(gold: 7, silver: 50);
			item.autoReuse = false;
			item.channel = true;
			item.shoot = ProjectileType<UnfoldingBlossomProjectile>();
			item.shootSpeed = 1f;

			item.rare = ItemRarityID.LightPurple;
			item.GetGlobalItem<PolaritiesItem>().customRarity = PolaritiesItemRarityID.PlanteraFlawless;
		}

		private int time;
		private float startRotation;

		public override void HoldItem(Player player)
		{
			if (player.channel)
			{
				player.direction = (Main.MouseWorld.X - player.Center.X > 0) ? 1 : -1;

				if (time % 10 == 0)
				{
					float angleOffsetIncrement = MathHelper.Pi * (3 - (float)Math.Sqrt(5));
					Projectile.NewProjectile(player.Center, new Vector2(item.shootSpeed, 0).RotatedBy(time / 10f * angleOffsetIncrement + startRotation), item.shoot, item.damage, item.knockBack, player.whoAmI);
				}

				player.itemTime = item.useTime;
				player.itemAnimation = item.useAnimation;
				time++;
			}
			else
			{
				startRotation = (Main.MouseWorld - player.MountedCenter).ToRotation();
				time = 0;
			}
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			return false;
		}
	}

	public class UnfoldingBlossomProjectile : ModProjectile
	{
		public override void SetDefaults()
		{
			projectile.aiStyle = -1;
			projectile.friendly = true;
			projectile.melee = true;
			projectile.width = 2;
			projectile.height = 2;
			projectile.alpha = 0;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.ignoreWater = false;

			projectile.usesIDStaticNPCImmunity = true;
			projectile.idStaticNPCHitCooldown = 10;

			projectile.timeLeft = 900;

			projectile.hide = true;
		}

		public override void AI()
		{
			Player player = Main.player[projectile.owner];

			if (projectile.ai[1] == 0)
			{
				projectile.ai[0]++;
				projectile.scale = 0.025f * (float)Math.Sqrt(projectile.ai[0]);

				projectile.Center = player.Center + projectile.velocity * 255 * projectile.scale;
				projectile.rotation = projectile.velocity.ToRotation();

				if (!player.channel || (Main.rand.NextBool() && player.GetModPlayer<PolaritiesPlayer>().justHit) || projectile.timeLeft == 1)
				{
					projectile.ai[1] = 1;
					projectile.timeLeft = 1800;
					projectile.velocity = player.velocity;

					//this becomes angular momentum
					projectile.ai[0] = 0;
				}
			}
			if (projectile.ai[1] == 1)
			{
				Vector2 direction = new Vector2(1, 0).RotatedBy(projectile.rotation);

				//gravity
				projectile.velocity.Y += 0.3f;

				//drag:
				float drag = (float)Math.Abs(Math.Sin(projectile.velocity.ToRotation() - direction.ToRotation())) / (projectile.scale * 4);
				float sidewaysForce = (float)Math.Sin(2 * (projectile.velocity.ToRotation() - direction.ToRotation())) / (projectile.scale * 4);

				projectile.velocity -= 0.1f * (projectile.velocity * drag + projectile.velocity.RotatedBy(MathHelper.PiOver2) * sidewaysForce);

				//you're probably hitting the front more or somehting which affects angular momentum
				projectile.ai[0] -= 0.001f * projectile.velocity.Length() * sidewaysForce;

				//angular momentum drag:
				projectile.ai[0] -= 0.1f * projectile.ai[0];

				projectile.rotation += projectile.ai[0];
				//update angular momentum
			}

			DelegateMethods.v3_1 = new Vector3(0.1f);
			Utils.PlotTileLine(projectile.Center + new Vector2(1, 0).RotatedBy(projectile.rotation) * 255 * projectile.scale, projectile.Center - new Vector2(1, 0).RotatedBy(projectile.rotation) * 255 * projectile.scale, 16, DelegateMethods.CastLight);
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			damage = (int)(damage * projectile.scale / 0.75f);
		}

		public override void ModifyHitPvp(Player target, ref int damage, ref bool crit)
		{
			damage = (int)(damage * projectile.scale / 0.75f);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 lineStart = projectile.Center + new Vector2(1, 0).RotatedBy(projectile.rotation) * 255 * projectile.scale;
			Vector2 lineEnd = projectile.Center - new Vector2(1, 0).RotatedBy(projectile.rotation) * 255 * projectile.scale;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			lightColor = Lighting.GetColor((int)(projectile.Center.X / 16), (int)(projectile.Center.Y / 16));

			Texture2D texture = Main.projectileTexture[projectile.type];
			spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), lightColor, projectile.rotation, new Vector2(texture.Width / 2f, texture.Height / 2f), projectile.scale, SpriteEffects.None, 0f);

			return false;
		}

		public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
		{
			drawCacheProjsBehindNPCsAndTiles.Add(index);
		}

		public override bool ShouldUpdatePosition()
		{
			return projectile.ai[1] == 1;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}
	}
}