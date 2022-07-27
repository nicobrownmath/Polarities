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
using Terraria.Audio;

namespace Polarities.Items.Weapons.Magic
{
	public class Proteolysis : ModItem
	{
		public override void SetStaticDefaults()
		{
			this.SetResearch(1);

			Item.staff[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.SetWeaponValues(45, 2.5f, 0);
			Item.DamageType = DamageClass.Magic;
			Item.mana = 8;

			Item.width = 46;
			Item.height = 48;

			Item.useTime = 14;
			Item.useAnimation = 14;
			Item.useStyle = 5;
			Item.noMelee = true;
			Item.autoReuse = true;

			Item.shoot = ProjectileType<ProteolysisProjectile>();
			Item.shootSpeed = 16f;

			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightPurple;
		}

		public override void ModifyManaCost(Player player, ref float reduce, ref float mult)
		{
			mult = 0;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			int numShots = 0;

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				if (Main.npc[i].CanBeChasedBy() && (Main.npc[i].Center - Main.MouseWorld).Length() < 256)
				{
					if (player.CheckMana(player.inventory[player.selectedItem].mana, true))
					{
						for (int j = 0; j < 50; j++) {
							Vector2 direction = new Vector2(1, 0).RotatedByRandom(MathHelper.TwoPi);
							Vector2 tryPosition = Main.npc[i].Center + direction * 240;
							Vector2 tryVelocity = -direction * velocity.Length() + Main.npc[i].velocity;
							if (j == 49 || Collision.CanHitLine(tryPosition - new Vector2(8), 16, 16, Main.npc[i].Center - new Vector2(8), 16, 16))
							{
								Projectile.NewProjectile(source, tryPosition, tryVelocity, type, damage, knockback, player.whoAmI);
								break;
							}
						}
						numShots++;
						if (numShots == 1) SoundEngine.PlaySound(SoundID.Item17, player.position);
						if (numShots == 5) break;
					}
					else
					{
						break;
					}
				}
			}
			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemType<Materials.VenomGland>(), 4)
				.AddRecipeGroup("AdamantiteBar", 12)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}

	public class ProteolysisProjectile : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.timeLeft = 120;
			Projectile.tileCollide = true;
		}

		public override void AI()
		{
			//special go through walls projectile
			if (Projectile.ai[0] < 15)
			{
				Projectile.ai[0]++;
				Projectile.tileCollide = false;
			}
			else
            {
				Projectile.tileCollide = true;
            }

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
		}

		public override void OnHitNPC(NPC target, int damage, float knockBack, bool crit)
		{
			target.AddBuff(BuffID.Venom, 300);
		}

		public override void OnHitPvp(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffID.Venom, 300);
		}

        public override bool? CanCutTiles()
        {
			//can only cut tiles if unable to go through walls
			return Projectile.ai[0] == 15;
        }
    }
}