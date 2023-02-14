using Microsoft.Xna.Framework;
using Polarities.Items.Weapons.Ranged.Ammo;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Ranged.Atlatls
{
    public class BoneSlinger : AtlatlBase
    {
        public override Vector2[] ShotDistances => new Vector2[] { new Vector2(30), new Vector2(30) };

        public override void SetDefaults()
        {
            Item.SetWeaponValues(20, 3, 4);
            Item.DamageType = DamageClass.Ranged;

            Item.width = 38;
            Item.height = 38;

            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;

            Item.shoot = 10;
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Dart;

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Orange;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            base.Shoot(player, source, position, velocity, type, damage, knockback);

            mostRecentShotTypes[1] = ProjectileType<BoneDartProjectile>();

            return false;
        }

        public override bool RealShoot(Player player, EntitySource_ItemUse_WithAmmo source, int index, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (index == 1)
            {
                velocity = velocity.RotatedByRandom(0.1f);
            }

            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Bone, 24)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
