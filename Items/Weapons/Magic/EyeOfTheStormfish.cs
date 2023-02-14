using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Magic
{
    public class EyeOfTheStormfish : ModItem, IDrawHeldItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
            PolaritiesItem.IsFlawless.Add(Type);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(19, 1, 0);
            Item.DamageType = DamageClass.Magic;
            Item.mana = 10;
            Item.noMelee = true;

            Item.width = 48;
            Item.height = 42;

            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.Thrust;
            Item.noUseGraphic = false;
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = true;
            Item.shoot = ProjectileType<StormEyeProjectile>();
            Item.shootSpeed = 1f;

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = RarityType<StormCloudfishFlawlessRarity>();
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.itemRotation = -player.direction * 2 * MathHelper.TwoPi * player.itemTime / Item.useTime * player.gravDir;
            player.itemLocation = player.Center + new Vector2(-player.direction * 24, 24).RotatedBy(player.itemRotation) + new Vector2(player.direction * 10, 0f);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            while (Collision.CanHitLine(player.position, player.width, player.height, position, 1, 1) && (position - player.Center).Length() < (Main.MouseWorld - player.Center).Length())
            {
                position += velocity * 8;
            }
            if ((position - player.Center).Length() < (Main.MouseWorld - player.Center).Length())
            {
                position -= velocity * 8;
            }
            else
            {
                position = Main.MouseWorld;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 20; i++)
            {
                int p = Projectile.NewProjectile(source, position + new Vector2(120 / MathHelper.PiOver2, 0).RotatedBy(i * MathHelper.TwoPi / 20), new Vector2(0, 4).RotatedBy(i * MathHelper.TwoPi / 20) * (velocity.X > 0 ? 1 : -1), type, damage, knockback, player.whoAmI, i, (velocity.X > 0 ? 1 : -1));

                if (i == 0)
                {
                    (Main.projectile[p].ModProjectile as StormEyeProjectile).shotPosition = position;
                }
            }
            return false;
        }

        public static Asset<Texture2D> HeldTexture;

        public override void Load()
        {
            HeldTexture = Request<Texture2D>(Texture + "_Held");
        }

        public override void Unload()
        {
            HeldTexture = null;
        }

        public void DrawHeldItem(ref PlayerDrawSet drawInfo)
        {
            Color currentColor = Lighting.GetColor((int)(drawInfo.drawPlayer.position.X + drawInfo.drawPlayer.width * 0.5) / 16, (int)((drawInfo.drawPlayer.position.Y + drawInfo.drawPlayer.height * 0.5) / 16.0));
            Texture2D texture = HeldTexture.Value;
            SpriteEffects spriteEffects = (SpriteEffects)((drawInfo.drawPlayer.gravDir != 1f) ? ((drawInfo.drawPlayer.direction != 1) ? 3 : 2) : ((drawInfo.drawPlayer.direction != 1) ? 1 : 0));

            Vector2 value21 = Vector2.Zero;
            int type6 = drawInfo.drawPlayer.inventory[drawInfo.drawPlayer.selectedItem].type;
            DrawData drawData = new DrawData(texture, new Vector2((int)(drawInfo.drawPlayer.itemLocation.X - Main.screenPosition.X), (int)(drawInfo.drawPlayer.itemLocation.Y - Main.screenPosition.Y)), (Rectangle?)new Rectangle(0, 0, texture.Width, texture.Height), currentColor, drawInfo.drawPlayer.itemRotation, new Vector2(texture.Width * 0.5f - texture.Height * 0.5f * drawInfo.drawPlayer.direction, texture.Height) + value21, drawInfo.drawPlayer.inventory[drawInfo.drawPlayer.selectedItem].scale, spriteEffects, 0);
            drawInfo.DrawDataCache.Add(drawData);
        }

        public bool DoVanillaDraw()
        {
            return false;
        }
    }

    public class StormEyeProjectile : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Pixel";

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.hide = true;
        }
        public Vector2 shotPosition;
        public override void AI()
        {
            Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[1] * MathHelper.Pi / 30);
            Dust.NewDustPerfect(Projectile.Center, DustID.Smoke, Velocity: Vector2.Zero, newColor: new Color(96, 96, 128), Scale: 1.5f).noGravity = true;
        }
        public override void Kill(int timeLeft)
        {
            if (Projectile.ai[0] == 0 && Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_Death(), shotPosition, (Main.MouseWorld - shotPosition).SafeNormalize(Vector2.Zero) * 20, ProjectileType<FriendlyLightning>(), Projectile.damage * 4 / 3, Projectile.knockBack, Projectile.owner);
                SoundEngine.PlaySound(SoundID.Item122, Projectile.position);
            }
        }
    }

    public class FriendlyLightning : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Pixel";

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.alpha = 0;
            Projectile.timeLeft = 511;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 511;
            Projectile.tileCollide = true;
            Projectile.hide = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 100000;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;

                Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.ai[0] = Projectile.Center.X;
                Projectile.ai[1] = Projectile.Center.Y;
            }

            Vector2 leadVelocity = new Vector2(Projectile.velocity.Length(), 0).RotatedBy(Projectile.rotation);
            Projectile.ai[0] += leadVelocity.X;
            Projectile.ai[1] += leadVelocity.Y;

            Vector2 goalPoint = new Vector2(Projectile.ai[0], Projectile.ai[1]);

            float randomAngle = (goalPoint - Projectile.Center).ToRotation() + Main.rand.NextFloat(-1, 1);
            Projectile.velocity = new Vector2(Projectile.velocity.Length(), 0).RotatedBy(randomAngle);

            for (int i = 0; i < 5; i++)
            {
                Dust.NewDustPerfect(Projectile.Center + i * Projectile.velocity / 5, DustID.Electric, Velocity: Vector2.Zero, Scale: 1f).noGravity = true;
            }
        }
    }
}