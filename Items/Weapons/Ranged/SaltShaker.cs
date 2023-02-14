using Microsoft.Xna.Framework;
using Polarities.Items.Placeable;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Ranged
{
    public class SaltShaker : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(10, 1, 0);
            Item.DamageType = DamageClass.Ranged;

            Item.width = 36;
            Item.height = 28;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.UseSound = SoundID.Item36;

            Item.shoot = ProjectileType<SaltShakerProjectile>();
            Item.shootSpeed = 10f;
            Item.useAmmo = ItemType<Items.Placeable.Blocks.Salt>();

            Item.value = Item.sellPrice(silver: 20);
            Item.rare = ItemRarityID.Blue;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < Main.rand.Next(7, 10); i++)
            {
                Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.779f, 1.28f), type, damage, knockback, player.whoAmI, 0, 0);
            }
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return Vector2.Zero;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.IllegalGunParts)
                .AddIngredient(ItemID.Glass, 6)
                .AddRecipeGroup(RecipeGroupID.IronBar, 6)
                .AddIngredient(ItemType<SaltCrystals>(), 15)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class SaltShakerProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.rotation += 0.5f;

            Projectile.velocity.Y += 0.2f;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return true;
        }
    }
}