using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Pets
{
    public class SparkOfSimilarity : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 0;
            Item.useStyle = 1;
            Item.shoot = ModContent.ProjectileType<BabySparkCrawler>();
            Item.width = 14;
            Item.height = 26;
            Item.UseSound = SoundID.Item2;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.rare = 3;
            Item.noMelee = true;
            Item.value = Item.sellPrice(0, 0, 25, 0);
            Item.buffType = ModContent.BuffType<BabySparkCrawlerBuff>();
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(Item.buffType, 3600, true);
            }
        }
    }

    public class BabySparkCrawlerBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.vanityPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.buffTime[buffIndex] = 18000;
            bool petProjectileNotSpawned = player.ownedProjectileCounts[ModContent.ProjectileType<BabySparkCrawler>()] <= 0;
            if (petProjectileNotSpawned && player.whoAmI == Main.myPlayer)
            {
                Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.position.X + player.width / 2, player.position.Y + player.height / 2, 0f, 0f, ModContent.ProjectileType<BabySparkCrawler>(), 0, 0f, player.whoAmI, 0f, 0f);
            }
        }
    }

    public class BabySparkCrawler : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Baby Spark Crawler");
            Main.projFrames[Type] = 1;
            Main.projPet[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            DrawOriginOffsetY = -6;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
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
                player.ClearBuff(ModContent.BuffType<BabySparkCrawlerBuff>());
            }
            if (player.HasBuff(ModContent.BuffType<BabySparkCrawlerBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            if ((player.Center + new Vector2(0, -100) - Projectile.Center).Length() > 800)
            {
                Projectile.position.X = player.position.X;
                Projectile.position.Y = player.position.Y;
                Projectile.velocity.X = 0;
                Projectile.velocity.Y = 0;
            }
            else if ((player.Center + new Vector2(0, -100) - Projectile.Center).Length() > 100)
            {
                Projectile.velocity += (player.Center + new Vector2(0, -100) - Projectile.Center).SafeNormalize(Vector2.Zero) * 0.2f;
                if (Projectile.velocity.Length() > (player.Center + new Vector2(0, -100) - Projectile.Center).Length() / 50)
                {
                    Projectile.velocity.Normalize();
                    Projectile.velocity *= (player.Center + new Vector2(0, -100) - Projectile.Center).Length() / 50;
                }
            }

            float angleGoal = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (angleGoal > MathHelper.TwoPi) angleGoal -= MathHelper.TwoPi;
            else if (angleGoal < 0) angleGoal += MathHelper.TwoPi;
            if (Projectile.rotation > MathHelper.TwoPi) Projectile.rotation -= MathHelper.TwoPi;
            else if (Projectile.rotation < 0) Projectile.rotation += MathHelper.TwoPi;

            float angleOffset = angleGoal - Projectile.rotation;
            if (angleOffset > MathHelper.Pi) angleOffset -= MathHelper.TwoPi;
            else if (angleOffset < -MathHelper.Pi) angleOffset += MathHelper.TwoPi;

            float maxTurn = 0.2f;

            if (Math.Abs(angleOffset) < maxTurn) { Projectile.rotation = angleGoal; }
            else if (angleOffset > 0)
            {
                Projectile.rotation += maxTurn;
            }
            else
            {
                Projectile.rotation -= maxTurn;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            return false;
        }
    }
}