using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Magic
{
    public class ShadeStorm : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(49, 4, 0);
            Item.DamageType = DamageClass.Magic;
            Item.mana = 8;

            Item.width = 30;
            Item.height = 32;

            Item.useTime = 18;
            Item.useAnimation = 18;

            Item.useStyle = 5;
            Item.noMelee = true;

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.LightPurple;

            Item.UseSound = SoundID.NPCHit54;
            Item.autoReuse = true;

            Item.shoot = ProjectileType<ShadeStormProjectile>();
            Item.shootSpeed = 8f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 1);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, -1);
            return false;
        }
    }

    public class ShadeStormProjectile : ModProjectile
    {
        public override string Texture => "Polarities/NPCs/SunPixie/SunPixieArena";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shade Vortex");

            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.alpha = 32;
            Projectile.timeLeft = 600;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;

            Projectile.scale = 0.9f;
        }

        public override void AI()
        {
            float switchChance = 0.01f;

            int targetID = Projectile.FindTargetWithLineOfSight(1000);
            if (targetID != -1)
            {
                float val = Projectile.ai[0] * (Projectile.velocity.X * (Main.npc[targetID].Center.Y - Projectile.Center.Y) - Projectile.velocity.Y * (Main.npc[targetID].Center.X - Projectile.Center.X));

                switchChance = val > 0 ? 0.005f : 0.02f;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextFloat(1) < switchChance)
            {
                Projectile.ai[0] *= -1;
            }
            Projectile.netUpdate = true;

            Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[0] * 0.1f);

            Projectile.frameCounter++;

            Projectile.spriteDirection = (int)Projectile.ai[0];

            if (Projectile.timeLeft < 30)
            {
                Projectile.friendly = false;
                Projectile.velocity *= 0.97f;
            }
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width /= 2;
            height /= 2;
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (oldVelocity.X != Projectile.velocity.X) Projectile.velocity.X = -oldVelocity.X;
            if (oldVelocity.Y != Projectile.velocity.Y) Projectile.velocity.Y = -oldVelocity.Y;
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[644].Value;

            Color mainColor = new Color(16, 16, 16);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Color color = mainColor * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                float scale = Projectile.scale * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

                if (Projectile.timeLeft < 30)
                {
                    scale *= Projectile.timeLeft / 30f;
                }

                float rotation;
                if (k + 1 >= Projectile.oldPos.Length)
                {
                    rotation = (Projectile.position - Projectile.oldPos[k]).ToRotation() + MathHelper.PiOver2;
                }
                else
                {
                    rotation = (Projectile.oldPos[k + 1] - Projectile.oldPos[k]).ToRotation() + MathHelper.PiOver2;
                }

                Main.EntitySpriteDraw(texture, Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), color, rotation, new Vector2(36, 36), new Vector2(scale, Projectile.scale), SpriteEffects.None, 0);
            }


            int numDraws = 12;
            for (int i = 0; i < numDraws; i++)
            {
                float scale = (1 + (float)Math.Sin(Projectile.frameCounter * 0.1f + (MathHelper.TwoPi * i) / numDraws)) / 2f;
                Color color = new Color((int)(lightColor.R / 4 * scale + (-64) * (1 - scale)), (int)(lightColor.G / 4 * scale + (-64) * (1 - scale)), (int)(lightColor.B / 4 * scale + (-64) * (1 - scale)));
                float alpha = 0.2f;
                float rotation = Projectile.frameCounter * 0.2f;

                if (Projectile.timeLeft < 30)
                {
                    scale *= Projectile.timeLeft / 30f;
                }

                Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 64), color * alpha, rotation, new Vector2(32, 32), Projectile.scale * scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            }
            for (int i = 0; i < numDraws; i++)
            {
                float scale = (1 + (float)Math.Sin(Projectile.frameCounter * 0.1f + (MathHelper.TwoPi * i) / numDraws)) / 2f;
                scale *= 0.75f;
                Color color = new Color((int)(lightColor.R / 4 * scale + (-64) * (1 - scale)), (int)(lightColor.G / 4 * scale + (-64) * (1 - scale)), (int)(lightColor.B / 4 * scale + (-64) * (1 - scale)));
                float alpha = 0.2f;
                float rotation = Projectile.frameCounter * 0.3f;

                if (Projectile.timeLeft < 45)
                {
                    scale *= (Projectile.timeLeft - 15) / 30f;
                }
                if (Projectile.timeLeft > 15)
                {
                    Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 64), color * alpha, rotation, new Vector2(32, 32), Projectile.scale * scale, Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
                }
            }

            return false;
        }

        public override void Kill(int timeLeft)
        {
            if (timeLeft > 0)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath52, Projectile.Center);

                for (int i = 0; i < 16; i++)
                {
                    Dust.NewDustPerfect(Projectile.Center, DustID.Smoke, new Vector2(1, 0).RotatedBy(i * MathHelper.TwoPi / 16), 0, Color.Black, 1);
                }
            }
        }
    }
}