using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Melee.Warhammers
{
    public class SkullMelter : WarhammerBase
    {
        public override int HammerLength => 81;
        public override int HammerHeadSize => 15;
        public override int DefenseLoss => 18;
        public override int DebuffTime => 1200;

        public override void SetDefaults()
        {
            Item.SetWeaponValues(48, 12, 0);
            Item.DamageType = DamageClass.Melee;

            Item.width = 96;
            Item.height = 96;

            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = WarhammerUseStyle;

            Item.value = Item.sellPrice(silver: 60);
            Item.rare = ItemRarityID.Orange;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ProjectileType<SkullMelterProjectile>()] < 1;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useStyle = ItemUseStyleID.Thrust;
                Item.shoot = ProjectileType<SkullMelterProjectile>();
                Item.shootSpeed = 2.5f;
                Item.noMelee = true;
                Item.noUseGraphic = true;
            }
            else
            {
                Item.useStyle = WarhammerUseStyle;
                Item.shoot = 0;
                Item.shootSpeed = 0f;
                Item.noMelee = false;
                Item.noUseGraphic = false;
            }
            return null;
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            target.AddBuff(BuffID.OnFire, DebuffTime);

            base.OnHitNPC(player, target, damage, knockBack, crit);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (player.itemAnimation < player.itemAnimationMax)
            {
                Main.dust[Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 6, player.velocity.X / 2, player.velocity.Y / 2, 0, Color.White, 2)].noGravity = true;
                Main.dust[Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 6, player.velocity.X / 2, player.velocity.Y / 2, 0, Color.White, 2)].noGravity = true;
            }
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            damage /= 2;
            knockback /= 2;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return player.altFunctionUse == 2;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HellstoneBar, 20)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class SkullMelterProjectile : ModProjectile
    {
        public override string Texture => "Polarities/Items/Weapons/Melee/Warhammers/SkullMelter";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("{$Mods.Polarities.ItemName.SkullMelter}");
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            DrawOffsetX = 15 * 2 - 48 * 2;
            DrawOriginOffsetY = 0;
            DrawOriginOffsetX = 48 - 15;

            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.scale = 1f;
            Projectile.alpha = 0;

            Projectile.hide = true;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
        }

        public float movementFactor
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override void AI()
        {
            Player projOwner = Main.player[Projectile.owner];
            Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
            Projectile.direction = projOwner.direction;
            projOwner.heldProj = Projectile.whoAmI;
            projOwner.itemTime = projOwner.itemAnimation;
            Projectile.Center = ownerMountedCenter;
            if (!projOwner.frozen)
            {
                if (movementFactor == 0f)
                {
                    movementFactor = 3f;
                }
                if (projOwner.itemAnimation < projOwner.itemAnimationMax / 3)
                {
                    movementFactor -= 1.9f;
                }
                else
                {
                    movementFactor += 1.7f;
                }
            }

            Projectile.position += Projectile.velocity * movementFactor;
            if (projOwner.itemAnimation == 0)
            {
                Projectile.Kill();
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(45f);
            if (Projectile.spriteDirection == -1)
            {
                Projectile.rotation -= MathHelper.ToRadians(90f);
            }

            Main.dust[Dust.NewDust(new Vector2(Projectile.Hitbox.X, Projectile.Hitbox.Y), Projectile.Hitbox.Width, Projectile.Hitbox.Height, 6, projOwner.velocity.X / 2, projOwner.velocity.Y / 2, 0, Color.White, 2)].noGravity = true;
            Main.dust[Dust.NewDust(new Vector2(Projectile.Hitbox.X, Projectile.Hitbox.Y), Projectile.Hitbox.Width, Projectile.Hitbox.Height, 6, projOwner.velocity.X / 2, projOwner.velocity.Y / 2, 0, Color.White, 2)].noGravity = true;
        }

        public override void OnHitNPC(NPC target, int damage, float knockBack, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 1200);
        }
    }
}

