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
using Terraria.Audio;

namespace Polarities.Items.Weapons.Melee.Warhammers
{
	public class Impactor : WarhammerBase
	{
		public override int HammerLength => 67;
		public override int HammerHeadSize => 15;
		public override int DefenseLoss => 12;
		public override int DebuffTime => 900;

		public override void SetDefaults()
		{
			Item.SetWeaponValues(28, 12, 0);
			Item.DamageType = DamageClass.Melee;

			Item.width = 86;
			Item.height = 82;

			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.useStyle = WarhammerUseStyle;

			Item.value = Item.sellPrice(silver: 40);
			Item.rare = ItemRarityID.Blue;
		}

		static bool hasExploded = false;

        public override void UseAnimation(Player player)
        {
			hasExploded = false;

            base.UseAnimation(player);
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!hasExploded)
			{
				hasExploded = true;
				Projectile.NewProjectile(player.GetSource_OnHit(target), GetHitboxCenter(player), Vector2.Zero, ProjectileType<ImpactorExplosion>(), hit.Damage, 0f, player.whoAmI);
			}

			base.OnHitNPC(player, target, hit, damageDone);
		}

        public override void OnHitTiles(Player player)
		{
			if (!hasExploded)
			{
				hasExploded = true;
				Projectile.NewProjectile(player.GetSource_ItemUse(Item), GetHitboxCenter(player), Vector2.Zero, ProjectileType<ImpactorExplosion>(), Item.damage, 0f, player.whoAmI);
			}
		}

        public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.MeteoriteBar, 20)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}

	public class ImpactorExplosion : ModProjectile
	{
		public override string Texture => "Polarities/Textures/Pixel";

		public override void SetDefaults()
		{
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 1;
			Projectile.tileCollide = false;
			Projectile.hide = true;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			int defenseLoss = 12;
			int time = 900;

			WarhammerBase.Hit(Main.player[Projectile.owner], target, defenseLoss, time);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float nearestX = Math.Max(targetHitbox.X, Math.Min(Projectile.Center.X, targetHitbox.X + targetHitbox.Size().X));
			float nearestY = Math.Max(targetHitbox.Y, Math.Min(Projectile.Center.Y, targetHitbox.Y + targetHitbox.Size().Y));
			return (new Vector2(Projectile.Center.X - nearestX, Projectile.Center.Y - nearestY)).Length() < Projectile.width / 2;
		}

		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			for (int num231 = 0; num231 < 20; num231++)
			{
				int num217 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default(Color), 1.5f);
				Dust dust71 = Main.dust[num217];
				Dust dust362 = dust71;
				dust362.velocity *= 1.4f;
			}
			for (int num230 = 0; num230 < 10; num230++)
			{
				int num220 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 6, 0f, 0f, 100, default(Color), 2.5f);
				Main.dust[num220].noGravity = true;
				Dust dust74 = Main.dust[num220];
				Dust dust362 = dust74;
				dust362.velocity *= 5f;
				num220 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 6, 0f, 0f, 100, default(Color), 1.5f);
				dust74 = Main.dust[num220];
				dust362 = dust74;
				dust362.velocity *= 3f;
			}
			Vector2 position67 = new Vector2(Projectile.position.X, Projectile.position.Y);
			Vector2 val = default(Vector2);
			int num229 = Gore.NewGore(Projectile.GetSource_Death(), position67, val, Main.rand.Next(61, 64));
			Gore gore20 = Main.gore[num229];
			Gore gore76 = gore20;
			gore76.velocity *= 0.4f;
			Main.gore[num229].velocity.X += 1f;
			Main.gore[num229].velocity.Y += 1f;
			Vector2 position68 = new Vector2(Projectile.position.X, Projectile.position.Y);
			val = default(Vector2);
			num229 = Gore.NewGore(Projectile.GetSource_Death(), position68, val, Main.rand.Next(61, 64));
			gore20 = Main.gore[num229];
			gore76 = gore20;
			gore76.velocity *= 0.4f;
			Main.gore[num229].velocity.X -= 1f;
			Main.gore[num229].velocity.Y += 1f;
			Vector2 position69 = new Vector2(Projectile.position.X, Projectile.position.Y);
			val = default(Vector2);
			num229 = Gore.NewGore(Projectile.GetSource_Death(), position69, val, Main.rand.Next(61, 64));
			gore20 = Main.gore[num229];
			gore76 = gore20;
			gore76.velocity *= 0.4f;
			Main.gore[num229].velocity.X += 1f;
			Main.gore[num229].velocity.Y -= 1f;
			Vector2 position70 = new Vector2(Projectile.position.X, Projectile.position.Y);
			val = default(Vector2);
			num229 = Gore.NewGore(Projectile.GetSource_Death(), position70, val, Main.rand.Next(61, 64));
			gore20 = Main.gore[num229];
			gore76 = gore20;
			gore76.velocity *= 0.4f;
			Main.gore[num229].velocity.X -= 1f;
			Main.gore[num229].velocity.Y -= 1f;
		}
	}
}

