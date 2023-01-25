using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Polarities.Projectiles;
using System;
using Terraria.DataStructures;
using Terraria.Audio;

namespace Polarities.Items.Weapons.Magic
{
	public class Shatterslash : ModItem
	{
		public override void SetStaticDefaults()
		{
			SacrificeTotal = (1);
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(40, 5f, 0);
			Item.DamageType = DamageClass.Magic;
			Item.mana = 12;

			Item.width = 32;
			Item.height = 40;

			Item.useTime = 60;
			Item.useAnimation = 60;
			Item.useStyle = 5;
			Item.noMelee = true;
			Item.UseSound = SoundID.Item8;
			Item.autoReuse = true;

			Item.shoot = ProjectileType<ShatterslashProjectile>();
			Item.shootSpeed = 6f;

			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.LightPurple;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			double rotation = Main.rand.NextDouble() * 2 * Math.PI;
			for (int i = 0; i < 6; i++)
			{
				Projectile.NewProjectile(source, position, (new Vector2(Item.shootSpeed, 0)).RotatedBy(rotation + Math.PI * i / 3), type, damage, knockback, player.whoAmI);
			}
			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.SpellTome)
				.AddIngredient(ItemID.FrostCore)
				.AddIngredient(ItemID.IceBlock, 20)
				.AddIngredient(ItemID.SoulofNight, 5)
				.AddTile(TileID.Bookcases)
				.Register();
		}
	}

    public class ShatterslashProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;

            //wth is this
            Projectile.coldDamage = true;
        }
        public override void AI()
        {
            if (Main.rand.NextBool(2))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, Scale: 2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.5f;
            }

            int targetID = Projectile.FindTargetWithLineOfSight(2000);
            NPC target = null;
            if (targetID != -1)
            {
                target = Main.npc[targetID];
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2;

            if (Projectile.timeLeft > 540)
            {
                Projectile.velocity *= 0.95f;
            }
            else
            {
                if (target != null)
                {
                    Vector2 a = target.Center - Projectile.Center;
                    if (a.Length() > 1) { a.Normalize(); }
                    a *= 0.2f;
                    Projectile.velocity += a;
                }

                Projectile.velocity *= 1.01f;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Frostburn, 300);
        }

        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Frostburn, 300);
        }

        public override void Kill(int timeLeft)
        {
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);
            for (int i = 0; i < 4; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, Projectile.velocity.X / 2, Projectile.velocity.Y / 2, Scale: 2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.5f;
            }
        }
    }
}