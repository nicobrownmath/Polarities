using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Projectiles;
using System;
using Polarities.Buffs;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Audio;
using Polarities.NPCs;
using MultiHitboxNPCLibrary;

namespace Polarities.Items.Weapons.Ranged
{
	public class Contagun : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(40, 4f, 0);
			Item.DamageType = DamageClass.Ranged;
			Item.noMelee = true;

			Item.width = 116;
			Item.height = 48;

			Item.useTime = 10;
			Item.useAnimation = 10;
			Item.channel = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.UseSound = SoundID.Item34;
			Item.autoReuse = true;

			Item.shoot = ProjectileType<ContagunProjectile>();
			Item.shootSpeed = 10f;

			Item.value = Item.sellPrice(gold: 7);
			Item.rare = RarityType<EsophageFlawlessRarity>();
			Item.GetGlobalItem<PolaritiesItem>().flawless = true;
		}

		int soundTime;
		int shotTime;

		public override void HoldItem(Player player)
		{
			if (player.channel)
			{
				Vector2 velocity = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero) * Item.shootSpeed;
				Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.MountedCenter + velocity.SafeNormalize(Vector2.Zero) * 70, velocity, Item.shoot, Item.damage, Item.knockBack, player.whoAmI, ai0: Main.GlobalTimeWrappedHourly * 4f, ai1: 60f);

				if (!player.ItemTimeIsZero) player.itemTime = player.itemTimeMax;
				player.itemAnimation = player.itemAnimationMax;

				soundTime++;
				if (soundTime >= 20 && Item.UseSound != null)
				{
					soundTime = 0;
					SoundEngine.PlaySound((SoundStyle)Item.UseSound, player.MountedCenter);
				}

				shotTime++;
				if (shotTime >= Item.useTime)
				{
					shotTime = 0;

					Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.MountedCenter + velocity.SafeNormalize(Vector2.Zero) * 60, velocity.RotatedByRandom(0.1f) * Main.rand.NextFloat(0.5f, 2f), ProjectileType<ContagunVirusProjectile>(), Item.damage, Item.knockBack, player.whoAmI);
				}
			}
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame)
		{
			player.direction = Main.MouseWorld.X > player.MountedCenter.X ? 1 : -1;
			player.itemRotation = (Main.MouseWorld - player.MountedCenter).ToRotation();
			if (player.direction == -1) { player.itemRotation += (float)Math.PI; }
		}

        public override Vector2? HoldoutOffset()
        {
			return new Vector2(-20, 0);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			soundTime = 0;
			shotTime = 0;
			return false;
		}
	}

	public class ContagunProjectile : ModProjectile
	{
		/*public override void Load()
        {
            IL.Terraria.Main.UpdateMenu += Main_UpdateMenu;
		}

        private void Main_UpdateMenu(MonoMod.Cil.ILContext il)
        {
            MonoMod.Cil.ILCursor c = new MonoMod.Cil.ILCursor(il);

			c.EmitDelegate<Action>(() =>
			{
				if (!(bool)(typeof(ModLoader).GetField("isLoading", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null)))
				{
					String filePath = Main.SavePath + System.IO.Path.DirectorySeparatorChar + "ContagunProjectile.png";

					if (!System.IO.File.Exists(filePath))
					{
						Terraria.Utilities.UnifiedRandom rand = new Terraria.Utilities.UnifiedRandom(521);
						const int textureSize = 512;

						float[,] fractalNoise = rand.FractalNoise(textureSize, 8);

						Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, textureSize, textureSize, false, SurfaceFormat.Color);
						System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
						for (int i = 0; i < texture.Width; i++)
						{
							for (int j = 0; j < texture.Height; j++)
							{
								float x = (2 * i / (float)(texture.Width - 1) - 1);
								float y = (2 * j / (float)(texture.Height - 1) - 1);

								float distanceSquared = x * x + y * y;

								int r = 255;
								int g = 255;
								int b = 255;
								int alpha = distanceSquared >= 1 ? 0 : (int)(255 * Math.Exp(1 + 1 / (distanceSquared - 1)));

								alpha = Math.Max(0, (int)(alpha - (fractalNoise[i, j] + 0.5f) * 255));

								list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
							}
						}
						texture.SetData(list.ToArray());
						texture.SaveAsPng(new System.IO.FileStream(filePath, System.IO.FileMode.Create), texture.Width, texture.Height);
					}
				}
			});
        }*/

		public override void SetDefaults()
		{
			Projectile.width = 256;
			Projectile.height = 256;
			Projectile.scale = 0.1f;

			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 60;

			Projectile.extraUpdates = 2;

			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
			Projectile.localAI[0] = Main.rand.NextFloat(-0.1f, 0.1f);
			Projectile.spriteDirection = Main.rand.NextBool() ? 1 : -1;

			Projectile.timeLeft = (int)Projectile.ai[1];

			Projectile.localAI[1] = 1f;

			if (Projectile.ai[1] > 60f)
			{
				Projectile.usesIDStaticNPCImmunity = false;
				Projectile.usesLocalNPCImmunity = true;
				Projectile.localNPCHitCooldown = 10;

				Projectile.tileCollide = false;
			}
		}

		public override void AI()
		{
			if (Projectile.ai[1] > 60f)
            {
				//decelerate if long-lasting
				Projectile.velocity *= 0.98f;
				Projectile.localAI[0] *= 0.98f;
            }

			Vector2 oldCenter = Projectile.Center;

			if (Projectile.ai[1] <= 60f)
				Projectile.scale += 0.45f / Projectile.ai[1];
			else
				Projectile.scale = 0.5f;

			Projectile.width = (int)(256 * Projectile.scale);
			Projectile.height = (int)(256 * Projectile.scale);
			Projectile.Center = oldCenter;

			Projectile.rotation += Projectile.localAI[0];

			if (Projectile.timeLeft < Projectile.ai[1] / 2f)
			{
				Projectile.alpha = (int)(255 * (1 - (Projectile.timeLeft / (Projectile.ai[1] / 2f))));
			}
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			damage = (int)(damage * Projectile.localAI[1]);
			Projectile.localAI[1] *= 0.95f;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			width = 2;
			height = 2;
			return true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.velocity = oldVelocity;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 10;
			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Color color = Color.Lerp(Color.Red, Color.Purple, (float)(Math.Sin(Projectile.ai[0]) + 1) * 0.5f).MultiplyRGB(Lighting.GetColor(Projectile.Center.ToTileCoordinates())) * (1 - Projectile.alpha / 255f);
			Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), color, Projectile.rotation, TextureAssets.Projectile[Type].Size() / 2, Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

			return false;
		}
	}

	public class ContagunVirusProjectile : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.scale = 0.8f;

			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 600;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = int.MaxValue;

            Projectile.GetGlobalProjectile<MultiHitboxNPCLibraryProjectile>().badCollision = true;
            Projectile.GetGlobalProjectile<MultiHitboxNPCLibraryProjectile>().javelinSticking = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
			SoundEngine.PlaySound(SoundID.Item17, Projectile.Center);
        }

        public override void AI()
		{
			if (Projectile.ai[0] == 0)
			{
				Projectile.hide = false;

				Projectile.rotation = Projectile.velocity.ToRotation();

				Projectile.velocity *= 1.02f;
			}
			else
            {
				Projectile.hide = true;

				NPC target = Main.npc[(int)Projectile.ai[0] - 1];

				if (!target.active || target.dontTakeDamage)
                {
					Projectile.Kill();
					return;
                }

				target.GetGlobalNPC<PolaritiesNPC>().contagunPhages++;

				Projectile.velocity = Vector2.Zero;
				Projectile.Center = target.Center;

				if (Main.rand.NextBool(120) && !target.immortal)
                {
					Projectile newProj = Main.projectile[Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(10f, 0).RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(0.5f, 2f), Type, Projectile.damage, Projectile.knockBack, Projectile.owner)];

					newProj.localNPCImmunity = (int[])Projectile.localNPCImmunity.Clone();
				}
            }
		}

        public override bool? CanDamage()
        {
            return Projectile.ai[0] == 0 ? null : false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Projectile.ai[0] = target.whoAmI + 1;

			for (int i = 0; i < Main.maxNPCs; i++)
            {
				if (Main.npc[i].active && target.realLife != -1 && Main.npc[i].realLife == target.realLife)
                {
					Projectile.localNPCImmunity[i] = Projectile.localNPCHitCooldown;
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.soundDelay <= 0)
			{
				Projectile.soundDelay = 20;
				Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
				SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
			}

			if (oldVelocity.X != Projectile.velocity.X) Projectile.velocity.X = -oldVelocity.X;
			if (oldVelocity.Y != Projectile.velocity.Y) Projectile.velocity.Y = -oldVelocity.Y;

			return Projectile.velocity.Length() > 64;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Color color = Lighting.GetColor(Projectile.Center.ToTileCoordinates());
			Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), color, Projectile.rotation, new Vector2(23, 9), Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

			return false;
		}
	}
}

