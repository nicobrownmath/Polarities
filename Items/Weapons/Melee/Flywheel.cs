using System;
using Microsoft.Xna.Framework;
using Polarities.Items.Materials;
using Polarities.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Melee
{
    public class Flywheel : ModItem
    {
        public override void SetStaticDefaults()
        {
            this.SetResearch(1);

            // These are all related to gamepad controls and don't seem to affect anything else
            ItemID.Sets.Yoyo[Type] = true;
            ItemID.Sets.GamepadExtraRange[Type] = 15;
            ItemID.Sets.GamepadSmartQuickReach[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(13, 2.5f, 0);
            Item.DamageType = DamageClass.Melee;

            Item.width = 44;
            Item.height = 38;

            Item.useStyle = 5;
            Item.useAnimation = 25;
            Item.useTime = 25;
            Item.shootSpeed = 16f;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ProjectileType<FlywheelProjectile>();
            Item.UseSound = SoundID.Item1;

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Cobweb, 10)
                .AddIngredient(ItemID.Cloud, 5)
                .AddIngredient(ItemID.RainCloud, 15)
                .AddIngredient(ItemType<StormChunk>(), 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class FlywheelProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("{$Mods.Polarities.ItemName.Flywheel}");

            ProjectileID.Sets.YoyosLifeTimeMultiplier[Projectile.type] = 4.5f;
            ProjectileID.Sets.YoyosMaximumRange[Projectile.type] = 240f;
            ProjectileID.Sets.YoyosTopSpeed[Projectile.type] = 13f;
        }

        public override void SetDefaults()
        {
            Projectile.extraUpdates = 0;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = 99;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.scale = 1f;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            MakeWave(target);
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            MakeWave(target);
        }

        private void MakeWave(Entity target)
        {
            int num = 12;
            for (int i = 0; i < num; i++)
            {
                Projectile.NewProjectile(Projectile.GetSource_OnHit(target), Projectile.Center, new Vector2(2, 0).RotatedBy(i * 2 * Math.PI / num), ProjectileType<FlywheelWave>(), 2 * Projectile.damage / num, Projectile.knockBack, Projectile.owner);
            }
        }
    }

    public class FlywheelWave : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 16;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 4;
            Projectile.alpha += 16;
        }
    }
}
