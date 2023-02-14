using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Pets
{
    public class SunPixiePetItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.damage = 0;
            Item.useStyle = 1;
            Item.shoot = ProjectileType<SunPixiePet>();
            Item.width = 32;
            Item.height = 24;
            Item.UseSound = SoundID.Item2;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.rare = ItemRarityID.Expert;
            Item.expert = true;
            Item.noMelee = true;
            Item.value = Item.sellPrice(gold: 5);
            Item.buffType = BuffType<SunPixiePetBuff>();
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(Item.buffType, 3600, true);
            }
        }
    }

    public class SunPixiePetBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.lightPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.buffTime[buffIndex] = 18000;
            int projType = ModContent.ProjectileType<SunPixiePet>();
            if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0)
            {
                Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.Center, Vector2.Zero, projType, 0, 0f, player.whoAmI);
            }
        }
    }

    public class SunPixiePet : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.LightPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.light = 1.5f;

            Projectile.width = 28;
            Projectile.height = 24;

            DrawOffsetX = -8;
            DrawOriginOffsetY = -6;
            DrawOriginOffsetX = 4;

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
                player.ClearBuff(BuffType<SunPixiePetBuff>());
            }
            if (player.HasBuff(BuffType<SunPixiePetBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 goalVelocity = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero) * 24;
                if ((Main.MouseWorld - Projectile.Center).Length() > 32)
                {
                    Projectile.velocity += (goalVelocity - Projectile.velocity) / 30;
                }
            }
            Projectile.netUpdate = true;

            if ((Projectile.Center - player.Center).Length() > 1200)
            {
                Projectile.position = player.Center + (Projectile.position - Projectile.Center);
            }

            Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
            Projectile.spriteDirection = Projectile.direction;
            Projectile.rotation = Projectile.velocity.X * 0.05f;

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 2;
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

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = true;
            return true;
        }
    }
}

