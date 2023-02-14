using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Items.Materials;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Magic
{
    public class FlailingKraken : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(78, 1f, 0);
            Item.DamageType = DamageClass.Magic;
            Item.mana = 60;

            Item.width = 66;
            Item.height = 38;

            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = false;
            Item.channel = true;

            Item.shoot = ProjectileType<FlailingKrakenProjectile>();
            Item.shootSpeed = 12f;

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Pink;
        }

        public override void HoldItem(Player player)
        {
            if (player.channel)
            {
                player.direction = (Main.MouseWorld.X - player.Center.X > 0) ? 1 : -1;
                if (!player.ItemTimeIsZero) player.itemTime = player.itemTimeMax;
                player.itemAnimation = player.itemAnimationMax;
            }
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.itemRotation = (Main.MouseWorld - player.MountedCenter).ToRotation();
            if (player.direction == -1) { player.itemRotation += MathHelper.Pi; }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 1);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<Tentacle>(), 4)
                .AddIngredient(ItemID.SoulofNight, 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class FlailingKrakenProjectile : ModProjectile
    {
        public override string Texture => "Polarities/Items/Materials/Tentacle";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("{$Mods.Polarities.ItemName.Tentacle}");
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 18;
            Projectile.height = 18;
            DrawOriginOffsetY = -14;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            if ((Main.player[Projectile.owner].Center - Projectile.Center).Length() < 600)
            {
                if ((Projectile.ai[0] == 0 && Projectile.ai[1] == 20) ||
                    (Projectile.ai[0] == 1 && Projectile.ai[1] == 40) ||
                    Projectile.ai[1] == 60
                )
                {
                    //dash at player
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.velocity = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.Zero) * 16f;
                        Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
                    }
                    Projectile.netUpdate = true;
                }
                Projectile.velocity *= 0.97f;
            }
            else
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 goalPos = Main.MouseWorld;
                    Vector2 goalVelocity = Main.player[Projectile.owner].velocity + (goalPos - Projectile.Center) / 10;
                    Projectile.velocity += (goalVelocity - Projectile.velocity) / 10;
                }
            }
            Projectile.ai[1]++;
            if (Projectile.ai[1] >= 80)
            {
                Projectile.ai[1] = 0;
            }

            if (Main.player[Projectile.owner].channel)
            {
                Projectile.timeLeft = 2;
            }
            Projectile.position += Projectile.velocity / 2;

            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (oldVelocity.X != Projectile.velocity.X) Projectile.velocity.X = -oldVelocity.X;
            if (oldVelocity.Y != Projectile.velocity.Y) Projectile.velocity.Y = -oldVelocity.Y;
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 playerCenter = Main.player[Projectile.owner].MountedCenter + new Vector2(Main.player[Projectile.owner].direction * 44, 0).RotatedBy(Main.player[Projectile.owner].itemRotation);

            Vector2[] bezierPoints = { playerCenter, playerCenter + new Vector2(Main.player[Projectile.owner].direction * 80, 0).RotatedBy(Main.player[Projectile.owner].itemRotation), Projectile.Center + new Vector2(0, 80).RotatedBy(Projectile.rotation), Projectile.Center };
            float bezierProgress = 0;
            float bezierIncrement = 6;

            Texture2D texture = NPCs.Enemies.KrakenTentacle.ChainTexture.Value;
            Vector2 textureCenter = new Vector2(4, 4);

            float rotation;

            while (bezierProgress < 1)
            {
                //draw stuff
                Vector2 oldPos = ModUtils.BezierCurve(bezierPoints, bezierProgress);

                //increment progress
                while ((oldPos - ModUtils.BezierCurve(bezierPoints, bezierProgress)).Length() < bezierIncrement)
                {
                    bezierProgress += 0.2f / ModUtils.BezierCurveDerivative(bezierPoints, bezierProgress).Length();
                }

                Vector2 newPos = ModUtils.BezierCurve(bezierPoints, bezierProgress);
                rotation = (newPos - oldPos).ToRotation() + MathHelper.Pi;

                Vector2 drawPos = (oldPos + newPos) / 2;

                Main.EntitySpriteDraw(texture, drawPos - Main.screenPosition, new Rectangle(0, 0, 8, 8), Lighting.GetColor((int)drawPos.X / 16, (int)drawPos.Y / 16), rotation, textureCenter, Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipVertically, 0);
            }

            return true;
        }
    }
}