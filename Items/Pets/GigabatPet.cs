using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Pets
{
    public class GigabatPetItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.DefaultToVanitypet(ProjectileType<GigabatPetLarge>(), BuffType<GigabatPetBuff>());

            Item.width = 26;
            Item.height = 48;

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Master;
            Item.master = true;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(Item.buffType, 3600, true);
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ProjectileType<GigabatPetMedium>(), 0, 0, player.whoAmI);
            Projectile.NewProjectile(source, position, velocity, ProjectileType<GigabatPetSmall>(), 0, 0, player.whoAmI);
            return true;
        }
    }

    public class GigabatPetBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.vanityPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.buffTime[buffIndex] = 18000;
            bool petProjectileNotSpawned = player.ownedProjectileCounts[ProjectileType<GigabatPetLarge>()] <= 0;
            if (petProjectileNotSpawned && player.whoAmI == Main.myPlayer)
            {
                Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.position.X + player.width / 2, player.position.Y + player.height / 2, 0f, 0f, ProjectileType<GigabatPetLarge>(), 0, 0f, player.whoAmI, 0f, 0f);
            }
            petProjectileNotSpawned = player.ownedProjectileCounts[ProjectileType<GigabatPetMedium>()] <= 0;
            if (petProjectileNotSpawned && player.whoAmI == Main.myPlayer)
            {
                Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.position.X + player.width / 2, player.position.Y + player.height / 2, 0f, 0f, ProjectileType<GigabatPetMedium>(), 0, 0f, player.whoAmI, 0f, 0f);
            }
            petProjectileNotSpawned = player.ownedProjectileCounts[ProjectileType<GigabatPetSmall>()] <= 0;
            if (petProjectileNotSpawned && player.whoAmI == Main.myPlayer)
            {
                Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.position.X + player.width / 2, player.position.Y + player.height / 2, 0f, 0f, ProjectileType<GigabatPetSmall>(), 0, 0f, player.whoAmI, 0f, 0f);
            }
        }
    }

    public class GigabatPetLarge : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 70;
            Projectile.height = 58;

            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active)
            {
                Projectile.active = false;
                return;
            }
            if (player.dead)
            {
                player.ClearBuff(BuffType<GigabatPetBuff>());
            }
            if (player.HasBuff(BuffType<GigabatPetBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            Projectile.ai[0]++;
            Vector2 goalPosition = player.MountedCenter + new Vector2(-player.direction * 64 + 32 * (float)Math.Cos(0.1f * Projectile.ai[0]), -64 + 32 * (float)Math.Sin(0.1f * Projectile.ai[0]));
            Vector2 goalVelocity = player.velocity / 2f + (goalPosition - Projectile.Center) / 16f;
            Projectile.velocity += (goalVelocity - Projectile.velocity) / 24f;

            if (Projectile.Distance(player.MountedCenter) > 1000)
            {
                Projectile.Center = player.MountedCenter;
                Projectile.velocity = player.velocity;
            }

            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 3)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X / 2;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y / 2;
            }
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = true;
            return true;
        }
    }

    public class GigabatPetMedium : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;

            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active)
            {
                Projectile.active = false;
                return;
            }
            if (player.dead)
            {
                player.ClearBuff(BuffType<GigabatPetBuff>());
            }
            if (player.HasBuff(BuffType<GigabatPetBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            Projectile.ai[0]++;
            Vector2 goalPosition = player.MountedCenter + new Vector2(-player.direction * 64 + 64 * (float)Math.Cos(0.075f * Projectile.ai[0]), -64 + 64 * (float)Math.Sin(0.075f * Projectile.ai[0]));
            Vector2 goalVelocity = player.velocity / 2f + (goalPosition - Projectile.Center) / 16f;
            Projectile.velocity += (goalVelocity - Projectile.velocity) / 24f;

            if (Projectile.Distance(player.MountedCenter) > 1000)
            {
                Projectile.Center = player.MountedCenter;
                Projectile.velocity = player.velocity;
            }

            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 3)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X / 2;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y / 2;
            }
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = true;
            return true;
        }
    }

    public class GigabatPetSmall : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 28;

            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active)
            {
                Projectile.active = false;
                return;
            }
            if (player.dead)
            {
                player.ClearBuff(BuffType<GigabatPetBuff>());
            }
            if (player.HasBuff(BuffType<GigabatPetBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            Projectile.ai[0]++;
            Vector2 goalPosition = player.MountedCenter + new Vector2(-player.direction * 64 + 64 * (float)Math.Cos(0.05f * Projectile.ai[0]), -64 + 64 * (float)Math.Sin(0.05f * Projectile.ai[0]));
            Vector2 goalVelocity = player.velocity / 2f + (goalPosition - Projectile.Center) / 16f;
            Projectile.velocity += (goalVelocity - Projectile.velocity) / 24f;

            if (Projectile.Distance(player.MountedCenter) > 1000)
            {
                Projectile.Center = player.MountedCenter;
                Projectile.velocity = player.velocity;
            }

            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 3)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X / 2;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y / 2;
            }
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = true;
            return true;
        }
    }
}

