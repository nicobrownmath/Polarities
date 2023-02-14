using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Ranged
{
    public class Eyeruption : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
            PolaritiesItem.IsFlawless.Add(Type);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(11, 2.1f, 0);
            Item.DamageType = DamageClass.Ranged;

            Item.width = 36;
            Item.height = 26;

            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            Item.shoot = ProjectileType<EyeruptionProjectile>();
            Item.shootSpeed = 22f;

            Item.value = Item.sellPrice(gold: 1, silver: 50);
            Item.rare = RarityType<EyeOfCthulhuFlawlessRarity>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float rot = Main.rand.NextFloat(0.1f, 0.4f);
            for (int i = -1; i < 2; i++)
            {
                Projectile.NewProjectile(source, position, new Vector2(0, -velocity.Length()).RotatedBy(i * rot), type, damage, knockback, player.whoAmI);
            }
            return false;
        }
    }

    public class EyeruptionProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/NPC_5";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            DrawOriginOffsetY = -10;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 3600;
        }

        public override void AI()
        {
            NPC target = Projectile.FindTargetWithinRange(750);

            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            if (target != null)
            {
                Vector2 targetVelocity = target.Center - Projectile.Center;
                Projectile.velocity.X += Math.Max(-0.25f, Math.Min(0.25f, (targetVelocity.X - Projectile.velocity.X) / 60));
                Projectile.velocity.Y += Math.Min(0, (targetVelocity.SafeNormalize(Vector2.Zero)).Y * 0.1f);
            }
            Projectile.velocity.Y += 0.35f;
            Projectile.velocity *= 0.975f;

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 5)
            {
                Projectile.frame = (Projectile.frame + 1) % 2;
                Projectile.frameCounter = 0;
            }
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = true;
            return true;
        }
    }
}