using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using System;
using Polarities.Projectiles;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace Polarities.Items.Weapons.Melee
{
	public class Sunblade : ModItem
	{
        public override void SetStaticDefaults()
        {
			this.SetResearch(1);
        }

        public override void SetDefaults()
		{
			Item.SetWeaponValues(80, 4f, 0);
			Item.DamageType = DamageClass.Melee;

			Item.width = 54;
			Item.height = 54;

			Item.useTime = 10;
			Item.useAnimation = 10;
			Item.channel = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noUseGraphic = true;
			Item.autoReuse = true;
			Item.noMelee = true;

			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.LightPurple;

			Item.UseSound = SoundID.Item15;
			Item.shoot = ProjectileType<SunbladeProjectile>();
			Item.shootSpeed = 0.01f;
		}

		private int time;

		public override void HoldItem(Player player)
		{
			if (player.channel)
			{
				player.direction = (Main.MouseWorld.X - player.MountedCenter.X > 0) ? 1 : -1;
				time++;

                if (!player.ItemTimeIsZero) player.itemTime = player.itemTimeMax;
                player.itemAnimation = player.itemAnimationMax;

                if (time % 20 == 0 && Item.UseSound != null)
				{
					SoundEngine.PlaySound((SoundStyle)Item.UseSound, player.position);
				}
            }
            else
            {
                time = 0;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI, ai1: (Main.MouseWorld.X > player.MountedCenter.X) ? 1 : -1);
			return false;
		}
	}

	public class SunbladeProjectile : ModProjectile
	{
		public override string Texture => "Polarities/NPCs/Enemies/HallowInvasion/SunKnightLightsword";

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("{$Mods.Polarities.ItemName.Sunblade}");

			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.aiStyle = -1;
			Projectile.width = 2;
			Projectile.height = 2;

			Projectile.timeLeft = 2;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;

			//uses local npc hit cooldown
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			if (!player.dead && player.active && player.channel)
			{
				Projectile.Center = player.MountedCenter;
				Projectile.timeLeft = 2;
			}
			else
			{
				Projectile.Kill();
				return;
			}

			if (Projectile.localAI[0] == 0)
			{
				player.heldProj = Projectile.whoAmI;

                Projectile.localAI[0] = 1;

				Projectile.scale = 0f;

				Projectile.spriteDirection = (int)Projectile.ai[1];

				for (int i = 0; i < Projectile.oldPos.Length; i++)
				{
					Projectile.oldPos[i] = Projectile.position;
					Projectile.oldRot[i] = Projectile.rotation;
				}

				if (Projectile.ai[1] == 1)
				{
					Projectile.rotation = MathHelper.Pi;
				}
			}

			Projectile.scale = Projectile.scale * 0.98f + 0.02f;
			Projectile.rotation += 4 * Projectile.scale * Projectile.spriteDirection * MathHelper.TwoPi / 120f;

			player.itemLocation = player.MountedCenter + new Vector2(10, 0).RotatedBy(Projectile.rotation);
			player.itemRotation = (float)Math.IEEERemainder(Projectile.rotation + (player.direction == 1 ? 0 : MathHelper.Pi), MathHelper.TwoPi);
        }

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			//since we're a player projectile we get a little extra collision length compared to the enemy version because I'm merciful
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + new Vector2(266 * Projectile.scale, 0).RotatedBy(Projectile.rotation));
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Color mainColor = new Color(255, 245, 168);
			Color endColor = new Color(255, 0, 0);
			Color swordColor = Color.Lerp(mainColor, endColor, ((float)Math.Sin(Main.GlobalTimeWrappedHourly * 12f) + 1) / 4f);

			float trailWidth = 266 * Projectile.scale;

			MiscShaderData miscShaderData = GameShaders.Misc["FinalFractal"];
			int num = 1;
			int num2 = 0;
			int num3 = 0;
			int num4 = 1;
			miscShaderData.UseShaderSpecificData(new Vector4(num, num2, num3, num4));
			miscShaderData.UseImage0("Images/Extra_" + 201);
			miscShaderData.UseImage1("Images/Extra_" + 195);
			miscShaderData.Apply();

			Vector2 previousInnerPoint = Projectile.Center;
			Vector2[] oldCenters = new Vector2[Projectile.oldPos.Length];
			float[] oldRotInverses = new float[Projectile.oldRot.Length];
			for (int i = 0; i < oldCenters.Length; i++)
			{
				Vector2 arcPoint = Projectile.oldPos[i] + Projectile.Center - Projectile.position + new Vector2(trailWidth, 0).RotatedBy(Projectile.oldRot[i]);
				Vector2 innerPoint = arcPoint + (previousInnerPoint - arcPoint).SafeNormalize(Vector2.Zero) * trailWidth;

				oldCenters[i] = (arcPoint + innerPoint) / 2;
				oldRotInverses[i] = (arcPoint - innerPoint).ToRotation() + MathHelper.PiOver2;

				previousInnerPoint = innerPoint;
			}

			VertexStrip vertexStrip = new VertexStrip();

			Color StripColors(float progressOnStrip)
			{
				Color result = Color.Lerp(mainColor, endColor, progressOnStrip) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip));
				result.A /= 2;
				return result;
			}

			float StripWidth(float progressOnStrip)
			{
				return trailWidth / 2f;
			}

			vertexStrip.PrepareStrip(oldCenters, oldRotInverses, StripColors, StripWidth, -Main.screenPosition + Projectile.Size / 2f, Projectile.oldPos.Length, includeBacksides: true);
			vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();

			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame();
			Vector2 drawCenter = new Vector2(5, 17);

			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, swordColor, Projectile.rotation, drawCenter, Projectile.scale, SpriteEffects.None, 0);
			return false;
		}

		public override bool ShouldUpdatePosition() => false;
	}
}