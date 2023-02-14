using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Ranged.Ammo
{
    public class VenomDart : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (99);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(24, 2, 0);
            Item.DamageType = DamageClass.Ranged;

            Item.width = 14;
            Item.height = 24;

            Item.maxStack = 9999;
            Item.consumable = true;

            Item.value = 1;
            Item.rare = ItemRarityID.Lime;

            Item.shoot = ProjectileType<VenomDartProjectile>();
            Item.shootSpeed = 1f;
            Item.ammo = AmmoID.Dart;
        }

        public override void AddRecipes()
        {
            CreateRecipe(100)
                .AddIngredient(ItemID.VialofVenom)
                .AddIngredient(ItemID.PoisonDart, 100)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    public class VenomDartProjectile : ModProjectile
    {
        public override string Texture => "Polarities/Items/Weapons/Ranged/Ammo/VenomDart";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("{$Mods.Polarities.ItemName.VenomDart}");
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.alpha = 0;
            Projectile.timeLeft = 3600;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;

            Projectile.extraUpdates = 1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            while (Projectile.velocity.X >= 16f || Projectile.velocity.X <= -16f || Projectile.velocity.Y >= 16f || Projectile.velocity.Y < -16f)
            {
                Projectile.velocity.X *= 0.97f;
                Projectile.velocity.Y *= 0.97f;
            }
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity.Y += 0.075f;

            if (Projectile.velocity.Y > 16) Projectile.velocity.Y = 16;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 16; i++)
            {
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, new Vector2(Main.rand.NextFloat(0.25f, 2f), 0).RotatedByRandom(0.1f).RotatedBy(MathHelper.TwoPi / 16 * i), ProjectileType<VenomDartCloud>(), Projectile.damage / 2, 0, Projectile.owner);
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Venom, 300);
        }

        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Venom, 300);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            return true;
        }
    }

    public class VenomDartCloud : ModProjectile
    {
        public override string Texture => "Terraria/Images/Gore_13";

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.alpha = 0;
            Projectile.timeLeft = 255;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = false;
        }

        public override void AI()
        {
            Projectile.rotation += Projectile.velocity.X / 5;
            Projectile.alpha++;
            Projectile.velocity *= 0.98f;

            if (Projectile.timeLeft < 128)
            {
                Projectile.friendly = false;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Venom, 120);
        }

        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Venom, 120);
        }
    }
}
