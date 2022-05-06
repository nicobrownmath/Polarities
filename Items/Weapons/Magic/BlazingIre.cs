using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Polarities.Projectiles;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent;
using ReLogic.Content;
using Terraria.DataStructures;

namespace Polarities.Items.Weapons.Magic
{
    public class BlazingIre : ModItem
    {
        public override void SetStaticDefaults()
        {
            this.SetResearch(1);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(38, 5, 15);

            Item.DamageType = DamageClass.Magic;
            Item.mana = 16;

            Item.width = 64;
            Item.height = 64;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = 1;
            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = 6;

            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ProjectileType<BlazingIreJavelin>();
            Item.shootSpeed = 20f;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useStyle = 3;
                Item.shootSpeed = 2.5f;
                Item.shoot = ProjectileType<BlazingIreSpear>();
            }
            else
            {
                Item.useStyle = 1;
                Item.shootSpeed = 20f;
                Item.shoot = ProjectileType<BlazingIreJavelin>();
            }
            return player.ownedProjectileCounts[ProjectileType<BlazingIreSpear>()] < 1 && base.CanUseItem(player);
        }
    }

    public class BlazingIreJavelin : ModProjectile
    {
        public override string Texture => "Polarities/Items/Weapons/Magic/BlazingIre";

        public static Asset<Texture2D> TrailTexture;

        public override void Load()
        {
            TrailTexture = Request<Texture2D>(Texture + "_Trail");
        }

        public override void Unload()
        {
            TrailTexture = null;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            DrawOffsetX = -52;
            DrawOriginOffsetY = 0;
            DrawOriginOffsetX = 26;

            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.light = 1f;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (Projectile.ai[0] > 0)
            {
                Projectile.tileCollide = false;
                Projectile.ai[0]--;
            }
            else
            {
                Projectile.tileCollide = true;
            }

            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
            DrawOriginOffsetX = 26 * Projectile.spriteDirection;
            Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 4 + (Projectile.spriteDirection == 1 ? 0 : MathHelper.PiOver2);

            if (Projectile.timeLeft < 585)
            {
                Projectile.velocity.X *= 0.98f;
                Projectile.velocity.Y += 0.6f;
            }
            if (!Projectile.friendly)
            {
                Projectile.alpha += 26;
                if (Projectile.alpha >= 255)
                {
                    Projectile.Kill();
                }
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Projectile.friendly = false;

            target.AddBuff(BuffID.OnFire, 120);
            if ((crit && Main.rand.NextBool(2) || Main.rand.NextBool(2)) && Main.myPlayer == Projectile.owner)
            {
                TryShot(Projectile.GetSource_OnHit(target), target.Center, target.velocity, Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
        }

        public static void TryShot(IEntitySource source, Vector2 targetPos, Vector2 targetVelocity, int damage, float knockback, int playerIndex)
        {
            for (int j = 0; j < 50; j++)
            {
                Vector2 direction = new Vector2(1, 0).RotatedByRandom(MathHelper.TwoPi);
                Vector2 tryPosition = targetPos + direction * 282;
                Vector2 tryVelocity = -20 * direction + targetVelocity;
                if (j == 49 || Collision.CanHitLine(tryPosition - new Vector2(6), 12, 12, targetPos - new Vector2(6), 12, 12))
                {
                    Projectile shot = Main.projectile[Projectile.NewProjectile(source, tryPosition, tryVelocity, ProjectileType<BlazingIreJavelin>(), damage, knockback, playerIndex, ai0: 14)];
                    shot.rotation = direction.ToRotation() + (float)Math.PI / 4;
                    break;
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

            Vector2 dustPos = Projectile.Center;

            for (int i = 0; i < 7; ++i)
            {
                float r = Main.rand.NextFloat(6f);
                float theta = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 dustVel = new Vector2((float)Math.Cos(theta) * r, -(float)Math.Sin(theta) * r);
                Dust dust = Main.dust[Dust.NewDust(dustPos, 0, 0, 133, dustVel.X, dustVel.Y)];
                dust.noGravity = true;
                dust.scale = 1.2f;
            }

            return true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D trailTexture = TrailTexture.Value;

            float alpha = ((255 - Projectile.alpha) / 255f);

            Color mainColor;
            Color mainColorA = new Color(231, 181, 65);
            Color mainColorB = Color.White;

            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                if (k >= 599 - Projectile.timeLeft)
                {
                    continue;
                }

                float gradient = ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                mainColor = new Color((int)((int)mainColorA.R * gradient + (int)mainColorB.R * (1 - gradient)), (int)((int)mainColorA.G * gradient + (int)mainColorB.G * (1 - gradient)), (int)((int)mainColorA.B * gradient + (int)mainColorB.B * (1 - gradient)));

                Color color = mainColor * gradient * alpha;
                float scale = Projectile.scale * gradient * alpha;

                Main.EntitySpriteDraw(trailTexture, new Vector2(0, DrawOriginOffsetY) + Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), color, Projectile.oldRot[k], new Vector2(texture.Width / 2 + DrawOriginOffsetX, Projectile.height / 2), scale, spriteEffects, 0);
            }

            mainColor = Color.White;
            mainColor *= alpha;
            Main.EntitySpriteDraw(texture, new Vector2(0, DrawOriginOffsetY) + Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), mainColor, Projectile.rotation, new Vector2(texture.Width / 2 + DrawOriginOffsetX, Projectile.height / 2), Projectile.scale, spriteEffects, 0);
            return false;
        }
    }

    public class BlazingIreSpear : ModProjectile
    {
        public static Asset<Texture2D> TrailTexture;

        public override void Load()
        {
            TrailTexture = Request<Texture2D>(Texture + "_Trail");
        }

        public override void Unload()
        {
            TrailTexture = null;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            DrawOffsetX = 16 * 2 - 64 * 2;
            DrawOriginOffsetY = 0;
            DrawOriginOffsetX = 64 - 16;

            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.scale = 1f;
            Projectile.alpha = 0;

            Projectile.hide = true;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;
            Projectile.friendly = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        // In here the AI uses this example, to make the code more organized and readable
        // Also showcased in ExampleJavelinProjectile.cs
        public float movementFactor // Change this value to alter how fast the spear moves
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        // It appears that for this AI, only the ai0 field is used!
        public override void AI()
        {
            // Since we access the owner player instance so much, it's useful to create a helper local variable for this
            // Sadly, Projectile/ModProjectile does not have its own
            Player projOwner = Main.player[Projectile.owner];
            // Here we set some of the projectile's owner properties, such as held item and itemtime, along with projectile direction and position based on the player
            Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
            Projectile.direction = projOwner.direction;
            projOwner.heldProj = Projectile.whoAmI;
            projOwner.itemTime = projOwner.itemAnimation;
            Projectile.position.X = ownerMountedCenter.X - (float)(Projectile.width / 2);
            Projectile.position.Y = ownerMountedCenter.Y - (float)(Projectile.height / 2);
            // As long as the player isn't frozen, the spear can move
            if (!projOwner.frozen)
            {
                if (movementFactor == 0f) // When initially thrown out, the ai0 will be 0f
                {
                    movementFactor = 3f; // Make sure the spear moves forward when initially thrown out
                    Projectile.netUpdate = true; // Make sure to netUpdate this spear
                }
                if (projOwner.itemAnimation < projOwner.itemAnimationMax / 3) // Somewhere along the item animation, make sure the spear moves back
                {
                    movementFactor -= 2.6f;
                }
                else // Otherwise, increase the movement factor
                {
                    movementFactor += 2.3f;
                }
            }
            // Change the spear position based off of the velocity and the movementFactor
            Projectile.position += Projectile.velocity * movementFactor;
            // When we reach the end of the animation, we can kill the spear projectile
            if (projOwner.itemAnimation == 0)
            {
                Projectile.Kill();
            }
            // Apply proper rotation, with an offset of 135 degrees due to the sprite's rotation, notice the usage of MathHelper, use this class!
            // MathHelper.ToRadians(xx degrees here)
            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
            DrawOriginOffsetX = 48 * Projectile.spriteDirection;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(45f) + (Projectile.spriteDirection == 1 ? 0 : MathHelper.PiOver2);

            // do whatever custom stuff here
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Projectile.friendly = false;

            target.AddBuff(BuffID.OnFire, 120);
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 2; i++)
                {
                    BlazingIreJavelin.TryShot(Projectile.GetSource_OnHit(target), target.Center, target.velocity, Projectile.damage, Projectile.knockBack, Projectile.owner);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D trailTexture = TrailTexture.Value;
            float alpha = ((255 - Projectile.alpha) / 255f);

            Color mainColor;
            Color mainColorA = new Color(231, 181, 65);
            Color mainColorB = Color.White;

            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                if (k >= 3599 - Projectile.timeLeft)
                {
                    continue;
                }

                float gradient = ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                mainColor = new Color((int)((int)mainColorA.R * gradient + (int)mainColorB.R * (1 - gradient)), (int)((int)mainColorA.G * gradient + (int)mainColorB.G * (1 - gradient)), (int)((int)mainColorA.B * gradient + (int)mainColorB.B * (1 - gradient)));

                Color color = mainColor * gradient * alpha;
                float scale = Projectile.scale * gradient * alpha;

                Main.EntitySpriteDraw(trailTexture, new Vector2(0, DrawOriginOffsetY) + Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), color, Projectile.rotation, new Vector2(texture.Width / 2 + DrawOriginOffsetX, Projectile.height / 2), scale, spriteEffects, 0);
            }

            mainColor = Color.White;
            mainColor *= alpha;
            Main.EntitySpriteDraw(texture, new Vector2(0, DrawOriginOffsetY) + Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), mainColor, Projectile.rotation, new Vector2(texture.Width / 2 + DrawOriginOffsetX, Projectile.height / 2), Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
}