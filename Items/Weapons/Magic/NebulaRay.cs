using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace Polarities.Items.Weapons.Magic
{
    public class NebulaRay : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(150, 4, 0);
            Item.DamageType = DamageClass.Magic;
            Item.mana = 16;

            Item.width = 28;
            Item.height = 30;
            Item.useTime = 14;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.noMelee = true;

            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item29;
            Item.shoot = ProjectileType<NebulaRayProjectile>();
            Item.shootSpeed = 32f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //this is done here instead of in modifyshootstats to prevent the weapon from looking weird in the player's hand
            position = player.MountedCenter + (player.MountedCenter - Main.MouseWorld).SafeNormalize(Vector2.Zero).RotatedByRandom(MathHelper.PiOver4) * 1200f;
            velocity = (Main.MouseWorld - position).SafeNormalize(Vector2.Zero) * velocity.Length();

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FragmentNebula, 18)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class NebulaRayProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_644";

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 2;
            Projectile.height = 2;

            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.penetrate = -1;

            Projectile.timeLeft = 3600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center + new Vector2(36 * 20, 0).RotatedBy(Projectile.rotation), Projectile.Center - new Vector2(36 * 20, 0).RotatedBy(Projectile.rotation));
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector3 color1 = new Vector3(255f / 255f, 255f / 255f, 255f / 255f);
            Vector3 color2 = new Vector3(254f / 255f, 190f / 255f, 243f / 255f);
            Vector3 drawVector = (color1 * (float)(1 + Math.Sin(Main.GlobalTimeWrappedHourly * 2f)) + color2 * (float)(1 - Math.Sin(Main.GlobalTimeWrappedHourly * 2f))) / 2;
            lightColor = new Color(drawVector);

            float heightScale = Math.Min(1, (3600 - Projectile.timeLeft) / 10f);
            Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 72, 72), lightColor, Projectile.rotation + MathHelper.PiOver2, new Vector2(36, 36), new Vector2(heightScale, 20), SpriteEffects.None, 0);

            return false;
        }
    }
}