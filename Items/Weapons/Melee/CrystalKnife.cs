using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Melee
{
    public class CrystalKnife : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (16);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(30, 1f, 0);
            Item.DamageType = DamageClass.Melee;
            Item.maxStack = 16;

            Item.width = 34;
            Item.height = 34;

            Item.useTime = 96;
            Item.useAnimation = 96;
            Item.useStyle = 5;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            Item.shoot = ProjectileType<CrystalKnifeProjectile>();
            Item.shootSpeed = 1f;

            Item.value = 5000;
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateInventory(Player player)
        {
            Item.maxStack = 16;
            Item.useTime = (int)Math.Ceiling(96f / Item.stack);
            Item.useAnimation = (int)Math.Ceiling(96f / Item.stack);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.ownedProjectileCounts[Item.shoot] < Item.stack)
            {
                Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI, 0, 100);
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<SaltKnife>())
                .AddIngredient(ItemID.CrystalShard, 10)
                .AddIngredient(ItemID.SoulofLight, 3)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class CrystalKnifeProjectile : ModProjectile
    {
        public override string Texture => "Polarities/Items/Weapons/Melee/CrystalKnife";

        private float rotationOffset;
        private int timer;
        private int atkCooldown;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("{$Mods.Polarities.ItemName.CrystalKnife}");
        }

        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 34;
            DrawOffsetX = -2;
            DrawOriginOffsetX = 1;

            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 96;
            Projectile.tileCollide = true;
            Projectile.light = 0.2f;
        }

        public override void AI()
        {
            if (Main.player[Projectile.owner].dead || !Main.player[Projectile.owner].active)
            {
                Projectile.active = false;
            }

            timer++;
            atkCooldown--;
            Projectile.friendly = atkCooldown < 0;

            Projectile.spriteDirection = Main.player[Projectile.owner].direction;

            float rotationIndex = Projectile.ai[0] + timer * 2 * (float)Math.PI / Projectile.ai[1];

            rotationOffset = -Main.player[Projectile.owner].direction * 0.2f * (float)Math.Cos(rotationIndex);
            Projectile.rotation = rotationOffset + (Main.MouseWorld - Main.player[Projectile.owner].Center).ToRotation() + MathHelper.Pi / 4;

            float targetLength = (Main.MouseWorld - Main.player[Projectile.owner].Center).Length();

            Projectile.velocity = -(new Vector2(Projectile.width / 2, Projectile.height / 2)) + Main.player[Projectile.owner].Center
                + (new Vector2(1, -1)).RotatedBy(Projectile.rotation) *
                (Projectile.width * 0.5f + targetLength *
                (float)Math.Sqrt(2) * 0.5f *
                (float)Math.Pow(0.5 * (1 + Math.Sin(rotationIndex)), 8)
                )
                - Projectile.position;
            Projectile.netUpdate = true;

            if (Projectile.spriteDirection == -1)
            {
                Projectile.rotation += MathHelper.PiOver2;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.immune[Projectile.owner] = 0;
            atkCooldown = (int)Projectile.ai[1] / 4;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = 2;
            height = 2;
            return true;
        }
    }
}