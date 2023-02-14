using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Magic
{
    public class DesolateWinds : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(35, 5f, 0);
            Item.DamageType = DamageClass.Magic;
            Item.mana = 10;

            Item.width = 30;
            Item.height = 32;

            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = 5;
            Item.noMelee = true;
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = true;

            Item.shoot = ProjectileType<DesolateWindsProjectile>();
            Item.shootSpeed = 12f;

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.LightPurple;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 3; i++)
            {
                velocity = new Vector2(velocity.Length() * player.direction * Main.rand.NextFloat(0.8f, 1.2f), Main.rand.NextFloat(-0.1f, 0.1f));
                position.X -= 1000 * player.direction;
                position.Y += Main.rand.Next(-600, 600);
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SpellTome)
                .AddIngredient(ItemID.AncientBattleArmorMaterial)
                .AddIngredient(ItemID.SandBlock, 20)
                .AddIngredient(ItemID.SoulofLight, 5)
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }

    //TODO: Dusts
    //TODO: Maybe wave up and down instead of circling
    public class DesolateWindsProjectile : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Pixel";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.alpha = 0;
            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;

            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 0 && Main.rand.NextBool(120))
            {
                Projectile.ai[0] = Main.rand.Next(30, 90);
                Projectile.ai[1] = (Projectile.velocity.X < 0 ? 1 : -1) / Projectile.ai[0];
            }
            if (Projectile.ai[0] > 0)
            {
                Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.TwoPi * Projectile.ai[1]);
                Projectile.ai[0]--;
            }
            else
            {
                //are we about to hit something
                if (!Collision.CanHitLine(Projectile.position, 2, 2, Projectile.position + Projectile.velocity * 80, 2, 2))
                {
                    //turn away from tiles
                    float turnDirection = 0;
                    const float rotAmount = 0.1f;
                    for (int i = -5; i < 5; i++)
                    {
                        if (i == 0) continue;
                        Vector2 testVelocity = Projectile.velocity.RotatedBy(i * rotAmount).SafeNormalize(Vector2.Zero) * 8;
                        for (int j = 0; j < 16 * (4 - Math.Abs(i)); j++)
                        {
                            if (!Collision.CanHitLine(Projectile.position + testVelocity * j, 2, 2, Projectile.position + testVelocity * (j + 1), 2, 2))
                            {
                                turnDirection -= 1f / (i * (j + 1));
                                break;
                            }
                        }
                    }
                    Projectile.velocity = Projectile.velocity.RotatedBy(0.5f * Math.Clamp(turnDirection, -0.1f, 0.1f));
                }
                else
                {
                    float length = Projectile.velocity.Length();
                    Projectile.velocity.Y *= 0.95f;
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * length;
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.width -= 16;
            Projectile.height -= 16;
            Projectile.position.X += 8;
            Projectile.position.Y += 8;
            Projectile.velocity = oldVelocity;
            if (Projectile.width <= 0)
            {
                return true;
            }
            return false;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (targetHitbox.Contains((Projectile.oldPos[i] + Projectile.Center - Projectile.position).ToPoint())) return true;
            }
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float drawThickness = 1f;
            if (Projectile.timeLeft < 10)
            {
                drawThickness = Projectile.timeLeft / 10f;
            }

            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i + 1] == Vector2.Zero) continue;

                Vector2 position = Projectile.oldPos[i] - Main.screenPosition;
                float scaleMult = 4 * (i + 1) * (Projectile.oldPos.Length - i + 1) / (float)(Projectile.oldPos.Length * Projectile.oldPos.Length);
                Vector2 scale = new Vector2((Projectile.oldPos[i + 1] - Projectile.oldPos[i]).Length(), 2 * scaleMult);
                float rotation = (Projectile.oldPos[i + 1] - Projectile.oldPos[i]).ToRotation();
                Color color = Projectile.GetAlpha(lightColor).MultiplyRGBA(new Color(255, 224, 192)) * scaleMult;
                Main.spriteBatch.Draw(TextureAssets.Projectile[Type].Value, position, new Rectangle(0, 0, 1, 1), color, rotation, new Vector2(0, 0.5f), new Vector2(1, drawThickness) * scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}