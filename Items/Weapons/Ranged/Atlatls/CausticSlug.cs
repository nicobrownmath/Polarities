using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.NPCs;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Audio;
using Polarities.Items.Weapons.Ranged.Ammo;
using Terraria.GameContent;

namespace Polarities.Items.Weapons.Ranged.Atlatls
{
	public class CausticSlug : AtlatlBase
	{
        public override Vector2[] ShotDistances => new Vector2[] { new Vector2(38), new Vector2(38), new Vector2(38) };
        public override float BaseShotDistance => 30;

        public override void SetDefaults()
        {
            Item.SetWeaponValues(23, 6f, 0);
			Item.DamageType = DamageClass.Ranged;

			Item.width = 48;
			Item.height = 50;

			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true;
			Item.autoReuse = true;

			Item.shoot = 10;
			Item.shootSpeed = 17f;
			Item.useAmmo = AmmoID.Dart;

			Item.value = Item.sellPrice(gold: 2, silver: 50);
			Item.rare = ItemRarityID.Pink;
		}

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			base.Shoot(player, source, position, velocity, type, damage, knockback);

			for (int i = 0; i < mostRecentShotTypes.Length; i++)
            {
				mostRecentShotTypes[i] = ProjectileType<CausticSlugProjectile>();
            }

			return false;
        }

        public override bool RealShoot(Player player, EntitySource_ItemUse_WithAmmo source, int index, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
			Vector2 goalPos = Main.MouseWorld + new Vector2(Main.rand.NextFloat(64f)).RotatedByRandom(MathHelper.TwoPi);
			float ai0 = goalPos.X;
			float ai1 = goalPos.Y;
			velocity = velocity.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.9f, 1.1f);

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai0: ai0, ai1: ai1);

			return false;
        }

        public override bool DoDartDraw(int index)
        {
            return index == 0;
        }
    }

    public class CausticSlugProjectile : ModProjectile, ICustomAtlatlDart
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;

            Projectile.aiStyle = -1;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 6;
            Projectile.tileCollide = true;

            Projectile.timeLeft = 600;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (Projectile.frameCounter == 0)
            {
                Projectile.frameCounter = 1;
                Projectile.frame = Main.rand.Next(2);
                Projectile.localAI[0] = 60 - (int)((new Vector2(Projectile.ai[0], Projectile.ai[1]) - Projectile.Center).Length() / Projectile.velocity.Length());

                Projectile.localAI[1] = Main.rand.Next(15, 30);
            }

            if (Projectile.localAI[0] < 60)
            {
                Vector2 goalPosition = new Vector2(Projectile.ai[0], Projectile.ai[1]);

                Vector2 goalVelocity = (goalPosition - Projectile.Center) / (60 - Projectile.localAI[0]);
                goalVelocity.Y *= 2;
                Projectile.velocity += (goalVelocity - Projectile.velocity) / (60 - Projectile.localAI[0]);

                Projectile.spriteDirection = (Projectile.velocity.X < 0 ? 1 : -1);
                Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == -1 ? 0 : MathHelper.Pi);

                Projectile.localAI[0]++;
                if (Projectile.localAI[0] == 60)
                {
                    Projectile.Center = goalPosition;
                    Projectile.velocity = Vector2.Zero;
                }
            }
            else if (Projectile.localAI[0] < 60 + Projectile.localAI[1])
            {
                Projectile.rotation -= Projectile.localAI[0] * (1.12f / (60 + Projectile.localAI[1])) * Projectile.spriteDirection;
                Projectile.velocity = Vector2.Zero;

                Projectile.localAI[0]++;
            }
            else
            {
                if (Projectile.localAI[0] == 60 + Projectile.localAI[1])
                {
                    for (int i = 0; i < 4; i++)
                        Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 74, Scale: 1f)].noGravity = true;

                    SoundEngine.PlaySound(SoundID.NPCHit54, Projectile.Center);
                }

                Projectile.velocity = new Vector2(0, 16);

                Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == -1 ? 0 : MathHelper.Pi);

                Projectile.localAI[0]++;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffType<Buffs.Corroding>(), 60);
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 74, Scale: 1f)].noGravity = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, 2, 0, Projectile.frame);
            Vector2 center = frame.Size() / 2;

            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                float alpha = 1 - i / (float)Projectile.oldPos.Length;
                Main.spriteBatch.Draw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, Color.White * alpha, Projectile.oldRot[i], center, Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            }

            return false;
        }

        public void DrawDart(PlayerDrawSet drawInfo, Vector2 position, float rotation)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            SpriteEffects spriteEffects = (drawInfo.drawPlayer.direction * drawInfo.drawPlayer.gravDir) == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            drawInfo.DrawDataCache.Add(new DrawData(texture, position, texture.Frame(1, 2, 0, 0), Color.White, rotation - MathHelper.PiOver2, texture.Frame(1, 2, 0, 0).Size() / 2, 1f, spriteEffects, 0));
        }
    }
}

