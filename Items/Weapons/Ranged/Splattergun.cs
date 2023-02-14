using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Weapons.Ranged
{
    public class Splattergun : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(28, 4, 0);
            Item.DamageType = DamageClass.Ranged;

            Item.width = 70;
            Item.height = 30;

            Item.useTime = 48;
            Item.useAnimation = 48;

            Item.useStyle = 5;
            Item.noMelee = true;

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.LightPurple;

            Item.UseSound = SoundID.Item36;
            Item.autoReuse = true;

            Item.shoot = ProjectileID.GoldenShowerFriendly;
            Item.shootSpeed = 8f;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-8, 6);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < Main.rand.Next(6, 10); i++)
            {
                int shot = Projectile.NewProjectile(source, position + new Vector2(Main.rand.NextFloat(10)).RotatedByRandom(MathHelper.TwoPi), velocity * Main.rand.NextFloat(0.9f, 1.1f), type, damage, knockback, player.whoAmI);
                Main.projectile[shot].DamageType = DamageClass.Ranged;
                Main.projectile[shot].penetrate = 1;
                Main.projectile[shot].maxPenetrate = 1;
            }
            return false;
        }
    }
}