using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;

namespace Polarities.Items.Weapons.Melee
{
	public class FangtipSpear : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(58, 6f, 0);
			Item.DamageType = DamageClass.Melee;

			Item.width = 66;
			Item.height = 66;

			Item.useTime = 28;
			Item.useAnimation = 28;
			Item.useStyle = 3;
			Item.knockBack = 6f;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.channel = true;

			Item.shootSpeed = 2.5f;
			Item.shoot = ProjectileType<FangtipSpearProjectile>();

			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightPurple;
		}

		public override bool CanUseItem(Player player)
		{
			return player.ownedProjectileCounts[ProjectileType<FangtipSpearProjectile>()] < 1 && base.CanUseItem(player);
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemType<Materials.VenomGland>(), 4)
				.AddRecipeGroup("Polarities:AdamantiteBar", 12)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}

	public class FangtipSpearProjectile : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 24;
			Projectile.height = 24;
			DrawOffsetX = 12 * 2 - 64 * 2;
			DrawOriginOffsetY = 0;
			DrawOriginOffsetX = 64 - 12;

			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.scale = 1f;
			Projectile.alpha = 0;

			Projectile.hide = true;
			Projectile.ownerHitCheck = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
		}

		// In here the AI uses this example, to make the code more organized and readable
		// Also showcased in ExampleJavelinProjectile.cs
		public float movementFactor // Change this value to alter how fast the spear moves
		{
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void AI()
		{
			Player projOwner = Main.player[Projectile.owner];

			if (Projectile.ai[1] == 0)
			{
				//Vanilla spear AI

				// Since we access the owner player instance so much, it's useful to create a helper local variable for this
				// Here we set some of the projectile's owner properties, such as held item and itemtime, along with projectile direction and position based on the player
				Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
				Projectile.direction = projOwner.direction;
				projOwner.heldProj = Projectile.whoAmI;
				projOwner.itemTime = projOwner.itemAnimation;
				Projectile.position.X = ownerMountedCenter.X - (float)(Projectile.width / 2);
				Projectile.position.Y = ownerMountedCenter.Y - (float)(Projectile.height / 2);
				// As long as the player isn't frozen, the spear can move
				if (!projOwner.frozen)
				{
					if (movementFactor == 0f) // When initially thrown out, the ai0 will be 0f
					{
						movementFactor = 3f; // Make sure the spear moves forward when initially thrown out
						Projectile.netUpdate = true; // Make sure to netUpdate this spear
					}
					if (projOwner.itemAnimation < projOwner.itemAnimationMax / 3) // Somewhere along the item animation, make sure the spear moves back
					{
						movementFactor -= 2.9f;
					}
					else // Otherwise, increase the movement factor
					{
						movementFactor += 2.5f;
					}
				}
				// Change the spear position based off of the velocity and the movementFactor
				Projectile.position += Projectile.velocity * movementFactor;
				// When we reach the end of the animation, we can kill the spear projectile
				if (projOwner.itemAnimation == 0)
				{
					Projectile.Kill();
				}
				// Apply proper rotation, with an offset of 135 degrees due to the sprite's rotation, notice the usage of MathHelper, use this class!
				// MathHelper.ToRadians(xx degrees here)
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(45f);
				// Offset by 90 degrees here
				if (Projectile.spriteDirection == -1)
				{
					Projectile.rotation -= MathHelper.ToRadians(90f);
				}
			}
			else
			{
				NPC target = Main.npc[(int)Projectile.ai[1] - 1];

				projOwner.heldProj = Projectile.whoAmI;
				projOwner.itemTime = projOwner.itemAnimation;

				if (target.CanBeChasedBy(Projectile) && projOwner.channel && Projectile.timeLeft > 3000)
				{
					Projectile.Center = target.Center;
					Projectile.velocity = Vector2.Zero;
				}
				else
				{
					projOwner.channel = false;

					Projectile.velocity = (projOwner.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * Math.Max(24, projOwner.velocity.Length());
					if ((projOwner.Center - Projectile.Center).Length() < 24)
					{
						Projectile.Kill();
					}
				}

				Projectile.rotation = (Projectile.Center - projOwner.Center).ToRotation() + MathHelper.ToRadians(45f);
				// Offset by 90 degrees here
				if (Projectile.spriteDirection == -1)
				{
					Projectile.rotation -= MathHelper.ToRadians(90f);
				}
			}

		}

		public override void OnHitNPC(NPC target, int damage, float knockBack, bool crit)
		{
			target.AddBuff(BuffID.Venom, 300);

			if (Projectile.ai[1] == 0 && !target.immortal)
				Projectile.ai[1] = target.whoAmI + 1;
		}

		public override void OnHitPvp(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffID.Venom, 300);
		}
	}
}
