using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.NPCs;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Polarities.Items.Materials;
using Terraria.Audio;

namespace Polarities.Items.Weapons.Melee.Warhammers
{
	public class TentacleClub : WarhammerBase
	{
		public override int HammerLength => 79;
		public override int HammerHeadSize => 11;
		public override int DefenseLoss => 24;
		public override int DebuffTime => 1200;

		public override void SetDefaults()
		{
			Item.SetWeaponValues(90, 12, 0);
			Item.DamageType = DamageClass.Melee;

			Item.width = 90;
			Item.height = 90;

			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.useStyle = WarhammerUseStyle;

			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Pink;
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override bool? UseItem(Player player)
		{
			if (player.altFunctionUse == 2)
			{
				Item.useStyle = ItemUseStyleID.Swing;
				Item.shoot = ProjectileType<TentacleClubProjectile>();
				Item.shootSpeed = 16f;
				Item.noMelee = true;
				Item.noUseGraphic = true;
			}
			else
			{
				Item.useStyle = WarhammerUseStyle;
				Item.shoot = ProjectileID.None;
				Item.shootSpeed = 0f;
				Item.noMelee = false;
				Item.noUseGraphic = false;
			}
			return null;
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			damage /= 2;
			knockback /= 2;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			return player.altFunctionUse == 2;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemType<Tentacle>(), 4)
				.AddIngredient(ItemID.SoulofLight, 10)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}

	public class TentacleClubProjectile : ModProjectile
	{
		public override string Texture => "Polarities/Items/Weapons/Melee/Warhammers/TentacleClub";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("{$Mods.Polarities.ItemName.TentacleClub}");
		}

		public override void SetDefaults()
		{
			Projectile.width = 22;
			Projectile.height = 22;
			DrawOffsetX = -68;
			DrawOriginOffsetY = 0;
			DrawOriginOffsetX = 34;

			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = 3;
			Projectile.hide = true;
			Projectile.ignoreWater = true;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
			if (IsStickingToTarget)
			{
				int npcIndex = (int)Projectile.ai[1];
				if (npcIndex >= 0 && npcIndex < 200 && Main.npc[npcIndex].active)
				{
					if (Main.npc[npcIndex].behindTiles)
					{
						behindNPCsAndTiles.Add(index);
					}
					else
					{
						behindNPCs.Add(index);
					}

					return;
				}
			}
			behindProjectiles.Add(index);
		}

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
			width = height = 10;
			return true;
        }

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (targetHitbox.Width > 8 && targetHitbox.Height > 8)
			{
				targetHitbox.Inflate(-targetHitbox.Width / 8, -targetHitbox.Height / 8);
			}
			return projHitbox.Intersects(targetHitbox);
		}

		public override void Kill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
			Vector2 usePos = Projectile.position;

			Vector2 rotVector = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2();
			usePos += rotVector * 16f;

			const int NUM_DUSTS = 20;
			for (int i = 0; i < NUM_DUSTS; i++)
			{
				Dust dust = Dust.NewDustDirect(usePos, Projectile.width, Projectile.height, DustID.SomethingRed);
				dust.position = (dust.position + Projectile.Center) / 2f;
				dust.velocity += rotVector * 2f;
				dust.velocity *= 0.5f;
				dust.noGravity = true;
				usePos -= rotVector * 8f;
			}
		}

		public bool IsStickingToTarget
		{
			get => Projectile.ai[0] == 1f;
			set => Projectile.ai[0] = value ? 1f : 0f;
		}

		public int TargetWhoAmI
		{
			get => (int)Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}

		private const int MAX_STICKY_JAVELINS = 6;
		private readonly Point[] _stickingJavelins = new Point[MAX_STICKY_JAVELINS];

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			IsStickingToTarget = true;
			TargetWhoAmI = target.whoAmI;
			Projectile.velocity =
				(target.Center - Projectile.Center) *
				0.75f;
			Projectile.netUpdate = true;

			Projectile.damage = 0;

			UpdateStickyJavelins(target);
		}

		private void UpdateStickyJavelins(NPC target)
		{
			int currentJavelinIndex = 0;

			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile currentProjectile = Main.projectile[i];
				if (i != Projectile.whoAmI
					&& currentProjectile.active
					&& currentProjectile.owner == Main.myPlayer
					&& currentProjectile.type == Projectile.type
					&& currentProjectile.ModProjectile is TentacleClubProjectile javelinProjectile
					&& javelinProjectile.IsStickingToTarget
					&& javelinProjectile.TargetWhoAmI == target.whoAmI)
				{
					_stickingJavelins[currentJavelinIndex++] = new Point(i, currentProjectile.timeLeft);
					if (currentJavelinIndex >= _stickingJavelins.Length)
						break;
				}
			}

			if (currentJavelinIndex >= MAX_STICKY_JAVELINS)
			{
				int oldJavelinIndex = 0;
				for (int i = 1; i < MAX_STICKY_JAVELINS; i++)
				{
					if (_stickingJavelins[i].Y < _stickingJavelins[oldJavelinIndex].Y)
					{
						oldJavelinIndex = i;
					}
				}
				Main.projectile[_stickingJavelins[oldJavelinIndex].X].Kill();
			}
		}

		private const int MAX_TICKS = 45;
		private const int ALPHA_REDUCTION = 25;

		public override void AI()
		{
			UpdateAlpha();
			if (IsStickingToTarget) StickyAI();
			else NormalAI();
		}

		private void UpdateAlpha()
		{
			if (Projectile.alpha > 0)
			{
				Projectile.alpha -= ALPHA_REDUCTION;
			}
			if (Projectile.alpha < 0)
			{
				Projectile.alpha = 0;
			}
		}

		private void NormalAI()
		{
			TargetWhoAmI++;
			if (TargetWhoAmI >= MAX_TICKS)
			{
				const float velXmult = 0.98f;
				const float velYmult = 0.35f;
				TargetWhoAmI = MAX_TICKS;
				Projectile.velocity.X *= velXmult;
				Projectile.velocity.Y += velYmult;
			}
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
		}

		private void StickyAI()
		{
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			const int aiFactor = 15;
			Projectile.localAI[0] += 1f;

			bool hitEffect = Projectile.localAI[0] % 30f == 0f;
			int projTargetIndex = (int)TargetWhoAmI;
			if (Projectile.localAI[0] >= 60 * aiFactor || projTargetIndex < 0 || projTargetIndex >= 200)
			{
				Projectile.Kill();
			}
			else if (Main.npc[projTargetIndex].active && !Main.npc[projTargetIndex].dontTakeDamage)
			{
				Main.npc[projTargetIndex].GetGlobalNPC<PolaritiesNPC>().tentacleClubs++;
				Projectile.Center = Main.npc[projTargetIndex].Center - Projectile.velocity * 2f;
				Projectile.gfxOffY = Main.npc[projTargetIndex].gfxOffY;
				if (hitEffect)
				{
					Main.npc[projTargetIndex].HitEffect(0, 1.0);
				}
			}
			else
			{
				Projectile.Kill();
			}
		}
	}
}