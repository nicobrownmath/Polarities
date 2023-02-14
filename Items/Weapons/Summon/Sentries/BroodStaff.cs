using Microsoft.Xna.Framework;
using Polarities.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Summon.Sentries
{
    public class BroodStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(20, 7.5f, 0);
            Item.DamageType = DamageClass.Summon;
            Item.sentry = true;
            Item.mana = 5;

            Item.width = 36;
            Item.height = 32;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = 1;
            Item.noMelee = true;
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = true;

            Item.shoot = ProjectileType<BroodStaffSentry>();
            Item.shootSpeed = 0f;

            Item.value = 5000;
            Item.rare = 2;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.SpawnSentry(source, player.whoAmI, type, damage, knockback);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Materials.Rattle>())
                .AddRecipeGroup(RecipeGroupID.Wood, 12)
                .AddIngredient(ItemID.SandBlock, 6)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    public class BroodStaffSentry : ModProjectile
    {
        private const int MaxAtkCooldown = 40;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 12;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 18;

            DrawOriginOffsetY = -2;
            DrawOffsetX = -23;

            Projectile.penetrate = -1;
            Projectile.sentry = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;

            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.DamageType = DamageClass.Summon;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = MaxAtkCooldown - 10;

            Projectile.GetGlobalProjectile<PolaritiesProjectile>().turretSlots = 0.5f;
        }

        public override void AI()
        {
            int index = 0;
            for (int i = 0; i < Projectile.whoAmI; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type == Projectile.type && Main.projectile[i].owner == Projectile.owner)
                {
                    index++;
                }
            }

            //we don't respect assigned targets in order to ensure we're strictly measuring distance from the projectile, but the range is so low it really doesn't matter
            NPC target = Projectile.FindTargetWithinRange(50, false);

            if (Projectile.ai[0] > 0)
            {
                Projectile.ai[0]--;
            }
            else if (target != null && Projectile.velocity.Y == 0)
            {
                Projectile.spriteDirection = (target.Center.X > Projectile.Center.X) ? 1 : -1;
                SoundEngine.PlaySound(Sounds.Rattle, Projectile.Center);
                Projectile.ai[0] = MaxAtkCooldown;
                Projectile.frame = 1; //this frame is skipped because we always add 1 to it after, but we want this to happen
            }

            Projectile.velocity.Y++;

            if (Projectile.frame > 0)
            {
                Projectile.frame++;
            }
            if (Projectile.ai[0] < MaxAtkCooldown - 9)
            {
                Projectile.frame = 0;
            }
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox.X += Projectile.spriteDirection * 26;
            hitbox.Y -= 13;
            hitbox.Height += 26;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            hitDirection = Projectile.spriteDirection;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool? CanDamage()
        {
            if (Projectile.ai[0] < MaxAtkCooldown - 9) return false;
            return null;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Poisoned, 300);
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }
    }
}