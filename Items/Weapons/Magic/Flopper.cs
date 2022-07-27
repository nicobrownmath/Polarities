using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using System;
using Terraria.DataStructures;
using Terraria.Audio;
using Polarities.Items.Materials;

namespace Polarities.Items.Weapons.Magic
{
    public class Flopper : ModItem
    {
        public override void SetStaticDefaults()
        {
            this.SetResearch(1);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(12, 3, 0);
            Item.DamageType = DamageClass.Magic;
            Item.mana = 20;

            Item.width = 62;
            Item.height = 36;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = 5;
            Item.noMelee = true;
            Item.shoot = ProjectileType<FlopperProjectile>();
            Item.shootSpeed = 8f;

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Green;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundEngine.PlaySound(SoundID.DoubleJump, position);
            return true;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-20, 0);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Cloud, 20)
                .AddIngredient(ItemID.RainCloud, 10)
                .AddIngredient(ItemType<StormChunk>(), 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class FlopperProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.width = 28;
            Projectile.height = 18;
            Projectile.alpha = 0;
            Projectile.timeLeft = 1800;
            Projectile.penetrate = 3;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.hide = false;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == 1800)
            {
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
            }

            Projectile.velocity.Y += 0.6f / (Math.Abs(Projectile.velocity.X) + 2f);
            Projectile.velocity.X *= 0.97f;

            if (Projectile.frame > 0)
            {
                Projectile.frameCounter++;
                if (Projectile.frameCounter == 4)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame = (Projectile.frame + 1) % 3;
                }
            }

            Projectile.rotation = Projectile.velocity.Y * 0.1f;

            if (Projectile.wet)
            {
                Projectile.Kill();
            }
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            height = 2;
            fallThrough = false;
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.Y != oldVelocity.Y && oldVelocity.Y > 0)
            {
                Projectile.velocity.Y = -2f;
                int numIncreasesIndex = Main.rand.Next(32);
                while (numIncreasesIndex % 2 == 1)
                {
                    numIncreasesIndex /= 2;
                    Projectile.velocity.Y -= 0.5f;
                }
                if (Projectile.velocity.X == oldVelocity.X)
                    Projectile.velocity.X = Main.rand.NextFloat(-1f, 1f);
                Projectile.frame = 1;
                Projectile.frameCounter = 0;
            }

            return false;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 16, Scale: 1.5f, newColor: Color.DarkGray);
                Main.dust[dust].velocity *= 0.5f;
            }

            SoundEngine.PlaySound(SoundID.DoubleJump, Projectile.position);
        }
    }
}