using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.NPCs.Enemies.Fractal.PostSentinel;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Summon.Orbs
{
    public class MandelbrotOrb : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 80;
            Item.DamageType = DamageClass.Summon;
            Item.width = 34;
            Item.height = 44;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.channel = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.knockBack = 0;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Yellow;
            Item.shoot = ModContent.ProjectileType<MandelbrotOrbMinion>();
            Item.shootSpeed = 2f;
        }

        public override void HoldItem(Player player)
        {
            if (player.channel)
            {
                player.direction = (Main.MouseWorld.X - player.Center.X > 0) ? 1 : -1;
                if (!player.ItemTimeIsZero) player.itemTime = player.itemTimeMax;
                player.itemAnimation = player.itemAnimationMax;
            }

            player.itemLocation += new Vector2(-player.direction * 8, 8 - Item.height / 2);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < player.GetModPlayer<PolaritiesPlayer>().orbMinionSlots; i++)
            {
                var p = Projectile.NewProjectileDirect(source, position, velocity.RotatedBy(i * MathHelper.TwoPi / player.GetModPlayer<PolaritiesPlayer>().orbMinionSlots), type, damage, knockback, player.whoAmI, ai0: -1 + 960 * i / player.GetModPlayer<PolaritiesPlayer>().orbMinionSlots / 240, ai1: 960 * i / player.GetModPlayer<PolaritiesPlayer>().orbMinionSlots % 240);
                p.originalDamage = damage;
            }
            return false;
        }
    }

    public class MandelbrotOrbMinion : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Antibrot");
            Main.projFrames[Type] = 10;
        }

        public override void SetDefaults()
        {
            Projectile.width = 94;
            Projectile.height = 94;
            Projectile.penetrate = -1;
            Projectile.minion = true;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 3600;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.channel || !player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }
            else
            {
                Projectile.timeLeft = 2;
            }

            int index = 0;
            int ownedProjectiles = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type == Projectile.type && Main.projectile[i].owner == Projectile.owner)
                {
                    ownedProjectiles++;
                    if (i < Projectile.whoAmI)
                    {
                        index++;
                    }
                }
            }

            Projectile.friendly = false;
            Projectile.alpha = 0;

            int targetID = -1;
            Projectile.Minion_FindTargetInRange(2000, ref targetID, skipIfCannotHitWithOwnBody: false);
            if (targetID != -1 && Projectile.ai[0] >= 0)
            {
                NPC target = Main.npc[targetID];

                switch (Projectile.ai[0])
                {
                    case 0:
                        //charges

                        Projectile.frame = 0;
                        Projectile.frameCounter = 0;

                        Projectile.friendly = true;

                        if (Projectile.ai[1] % 30 == 0)
                        {
                            Vector2 velocityPart = target.position - target.oldPosition;
                            if (velocityPart.Length() > 24)
                            {
                                velocityPart = velocityPart.SafeNormalize(Vector2.Zero) * 24;
                            }
                            Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 24 + velocityPart;
                            Projectile.rotation = Projectile.velocity.ToRotation();
                        }
                        else
                        {
                            Projectile.velocity *= 0.975f;
                        }

                        Projectile.ai[1]++;
                        if (Projectile.ai[1] == 240)
                        {
                            Projectile.ai[0]++;
                            Projectile.ai[1] = 0;
                        }
                        break;
                    case 1:
                        //homing barrage

                        Projectile.frameCounter++;
                        if (Projectile.frameCounter == 3)
                        {
                            Projectile.frame = (Projectile.frame + 1) % 10;
                            Projectile.frameCounter = 0;
                        }

                        Projectile.velocity *= 0.95f;
                        Projectile.velocity += (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 0.05f;
                        Projectile.rotation = Projectile.velocity.ToRotation();

                        if (Projectile.ai[1] % 32 == 0)
                        {
                            SoundEngine.PlaySound(SoundID.Item34, Projectile.Center);
                        }

                        if (Projectile.ai[1] % (48 / player.maxMinions) == 0)
                        {
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(0, 4).RotatedBy(Projectile.rotation).RotatedByRandom(MathHelper.PiOver4), ProjectileType<MandelbrotOrbMinionHomingShot>(), Projectile.damage / 3, Projectile.knockBack, Projectile.owner);
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(0, -4).RotatedBy(Projectile.rotation).RotatedByRandom(MathHelper.PiOver4), ProjectileType<MandelbrotOrbMinionHomingShot>(), Projectile.damage / 3, Projectile.knockBack, Projectile.owner);
                        }

                        Projectile.ai[1]++;
                        if (Projectile.ai[1] == 240)
                        {
                            Projectile.ai[0] = 2;
                            Projectile.ai[1] = 0;
                        }
                        break;
                    case 2:
                        //splitting charges

                        //longer dash, main body fades out and back in, not doing damage during that time, summons a bunch of images (number of minion slots) to do damage instead

                        Projectile.frame = 0;
                        Projectile.frameCounter = 0;

                        float alphaFactor = 1 - (Projectile.ai[1] % 60) * (60f - (Projectile.ai[1] % 60)) / (30f * 30f);
                        Projectile.alpha = (int)(255 - 255 * alphaFactor);

                        if (Projectile.ai[1] % 60 == 0)
                        {
                            Vector2 velocityPart = target.position - target.oldPosition;
                            if (velocityPart.Length() > 24)
                            {
                                velocityPart = velocityPart.SafeNormalize(Vector2.Zero) * 24;
                            }
                            Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 24 + velocityPart;
                            Projectile.rotation = Projectile.velocity.ToRotation();

                            for (int i = 0; i < player.maxMinions; i++)
                            {
                                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity, ProjectileType<MandelbrotOrbMinionClone>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
                            }
                        }
                        else
                        {
                            Projectile.velocity *= 0.98f;
                        }

                        Projectile.ai[1]++;
                        if (Projectile.ai[1] == 240)
                        {
                            Projectile.ai[0]++;
                            Projectile.ai[1] = 0;
                        }
                        break;
                    case 3:
                        //oscillating gaze

                        Projectile.frame = 0;
                        Projectile.frameCounter = 0;

                        if (Projectile.ai[1] == 0)
                        {
                            Projectile.velocity = Vector2.Zero;
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ProjectileType<MandelbrotOrbMinionOscillatingRay>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner, Projectile.whoAmI);
                        }

                        Projectile.velocity *= 0.99f;
                        Projectile.velocity += -(target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 0.01f;
                        Projectile.rotation = (-Projectile.velocity).ToRotation();

                        Projectile.ai[1]++;
                        if (Projectile.ai[1] == 240)
                        {
                            Projectile.ai[0] = 0;
                            Projectile.ai[1] = 0;
                        }
                        break;
                }
            }
            else if (Projectile.ai[0] != 3)
            {

                Projectile.velocity *= 0.99f;
                Projectile.velocity += (player.Center + new Vector2(0, -240) + new Vector2((ownedProjectiles - 1) * 60, 0).RotatedBy(index * MathHelper.TwoPi / ownedProjectiles) - Projectile.Center) / 480f;
                Projectile.rotation = Projectile.velocity.ToRotation();

                Projectile.frame = 0;
                Projectile.frameCounter = 0;

                Projectile.ai[1]++;
                if (Projectile.ai[1] == 240)
                {
                    Projectile.ai[1] = 0;
                    Projectile.ai[0]++;
                    if (Projectile.ai[0] == 3)
                    {
                        Projectile.ai[0] = -1;
                    }
                }
            }
            else
            {
                Projectile.velocity *= 0.99f;
                Projectile.rotation = (-Projectile.velocity).ToRotation();

                Projectile.ai[1]++;
                if (Projectile.ai[1] == 240)
                {
                    Projectile.ai[0] = 0;
                    Projectile.ai[1] = 0;
                }

                Projectile.frame = 0;
                Projectile.frameCounter = 0;
            }
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float projectileAlpha = (255 - Projectile.alpha) / 255f;

            //bezier tail
            Vector2 playerCenter = Main.player[Projectile.owner].Center + new Vector2(Main.player[Projectile.owner].direction * 13, -24);

            Vector2[] bezierPoints = { playerCenter, playerCenter + new Vector2(0, -160), Projectile.Center + new Vector2(-270, 0).RotatedBy(Projectile.rotation), Projectile.Center + new Vector2(-110, 0).RotatedBy(Projectile.rotation) };
            float bezierProgress = 0;
            float bezierIncrement = 0.1f;

            Texture2D texture = ModContent.Request<Texture2D>($"{Texture}Chain").Value;
            Vector2 textureCenter = new Vector2(1, 4);

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

                Main.spriteBatch.Draw(texture, drawPos - Main.screenPosition, new Rectangle(0, 0, 2, 8), Color.White * 0.15f * projectileAlpha, rotation, textureCenter, Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipVertically, 0f);
            }

            texture = TextureAssets.Projectile[Projectile.type].Value;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16) * projectileAlpha, Projectile.rotation, new Vector2(127, 55), Projectile.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(ModContent.Request<Texture2D>($"{Texture}_Mask").Value, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), Color.White * projectileAlpha, Projectile.rotation, new Vector2(127, 55), Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    }

    public class MandelbrotOrbMinionHomingShot : ModProjectile
    {
        public override string Texture => $"{ModContent.GetInstance<ChaosBolt>().Texture}";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Homing Energy");
            ProjectileID.Sets.MinionShot[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = false;

            ProjectileID.Sets.TrailCacheLength[Type] = 24;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 1200;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            if (Projectile.timeLeft > 1080)
            {
                Projectile.velocity *= 0.97f;
            }
            else if (Projectile.timeLeft == 1080)
            {
                int targetID = -1;
                Projectile.Minion_FindTargetInRange(2000, ref targetID, skipIfCannotHitWithOwnBody: false);
                if (targetID != -1)
                {
                    NPC target = Main.npc[targetID];

                    Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 12f;
                }
                else
                {
                    Projectile.Kill();
                }
            }
            else
            {
                int targetID = -1;
                Projectile.Minion_FindTargetInRange(2000, ref targetID, skipIfCannotHitWithOwnBody: false);
                if (targetID != -1)
                {
                    NPC target = Main.npc[targetID];

                    Vector2 targetVelocityComponent = target.position - target.oldPosition;
                    if (targetVelocityComponent.Length() > 24)
                    {
                        targetVelocityComponent = targetVelocityComponent.SafeNormalize(Vector2.Zero) * 24;
                    }
                    Vector2 goalVelocity = targetVelocityComponent / 2 + (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 12f;
                    Projectile.velocity = (Projectile.velocity + goalVelocity / (1080f - Projectile.timeLeft)).SafeNormalize(Vector2.Zero) * 12f;
                }
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Projectile.Kill();
        }
        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            Projectile.Kill();
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 134, newColor: Color.Pink, Scale: 1f)].noGravity = true;
            }
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Color mainColor = Color.White;

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), mainColor, Projectile.rotation, new Vector2(5, 5), Projectile.scale, SpriteEffects.None, 0f);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                float scale = Projectile.scale * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

                float rotation;
                if (k + 1 >= Projectile.oldPos.Length)
                {
                    rotation = (Projectile.oldPos[k] - Projectile.position).ToRotation() + MathHelper.PiOver2;
                }
                else
                {
                    rotation = (Projectile.oldPos[k] - Projectile.oldPos[k + 1]).ToRotation() + MathHelper.PiOver2;
                }

                Main.spriteBatch.Draw(texture, Projectile.Center - Projectile.position + (Projectile.oldPos[k] + Projectile.position) / 2 - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), color, rotation, new Vector2(5, 5), scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }

    public class MandelbrotOrbMinionOscillatingRay : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Oscillating Ray");
            ProjectileID.Sets.MinionShot[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = false;
            /*
			Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 4096, 4096, false, SurfaceFormat.Color);
			System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();

			for (int i = 0; i < texture.Height; i++)
			{
				for (int j = 0; j < texture.Width; j++)
				{
					float x = j / (float)texture.Width;
					float y = i / (float)texture.Height;

					float floatAlpha = 0;
					if (x > Math.Abs(y))
					{
						floatAlpha = Math.Min(1, 1f / (float)Math.Sqrt(x * x - y * y) / 48f);
					}

					int r = 255;
					int g = 255;
					int b = 255;
					int alpha = (int)(255 * floatAlpha);

					list.Add(new Color((int)(r * floatAlpha), (int)(g * floatAlpha), (int)(b * floatAlpha), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "MandelbrotOrbMinionOscillatingRay.png", FileMode.Create), texture.Width, texture.Height);
			
			Main.projectileTexture[projectile.type] = texture;
			*/
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (!Main.projectile[(int)Projectile.ai[0]].active)
            {
                Projectile.Kill();
                return;
            }

            Projectile.rotation = Main.projectile[(int)Projectile.ai[0]].rotation;
            Projectile.Center = Main.projectile[(int)Projectile.ai[0]].Center + new Vector2(42, 0).RotatedBy(Projectile.rotation);
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            //need to check if the hitbox either has an edge that intersects an edge ray, or if every point falls within the sector
            float scaleFactor = Projectile.timeLeft * (240f - Projectile.timeLeft) / (120f * 120f);
            float rotationFactor = (float)Math.Abs(Math.Sin(5 * MathHelper.Pi * Projectile.timeLeft / 240f)) * 0.5f * Main.player[Projectile.owner].maxMinions / 6;
            float angleOffset = (float)Math.Atan(scaleFactor * rotationFactor);

            //every point checking
            float targetAngle = (targetHitbox.TopLeft() - Projectile.Center).ToRotation() - Projectile.rotation;
            while (targetAngle > MathHelper.Pi)
            {
                targetAngle -= MathHelper.TwoPi;
            }
            while (targetAngle < -MathHelper.Pi)
            {
                targetAngle += MathHelper.TwoPi;
            }
            if (Math.Abs(targetAngle) < angleOffset)
            {
                return true;
            }

            //edges checking
            if (
                Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + new Vector2(10000, 0).RotatedBy(Projectile.rotation + angleOffset)) ||
                Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + new Vector2(10000, 0).RotatedBy(Projectile.rotation - angleOffset))
                )
            {
                return true;
            }

            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float scaleFactor = Projectile.timeLeft * (240f - Projectile.timeLeft) / (120f * 120f);
            float rotationFactor = (float)Math.Abs(Math.Sin(5 * MathHelper.Pi * Projectile.timeLeft / 240f)) * 0.5f * Main.player[Projectile.owner].maxMinions / 6;
            float angleOffset = (float)Math.Atan(scaleFactor * rotationFactor);

            Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 4096, 4096), Color.White, Projectile.rotation, new Vector2(0, 0), new Vector2(1, scaleFactor * rotationFactor), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 4096, 4096), Color.White, Projectile.rotation, new Vector2(0, 4096), new Vector2(1, scaleFactor * rotationFactor), SpriteEffects.FlipVertically, 0f);

            var texture = ModContent.Request<Texture2D>(Polarities.CallShootProjectile);
            Main.spriteBatch.Draw(texture.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 2, 2), new Color(192, 192, 255), Projectile.rotation + angleOffset, new Vector2(0, 1), new Vector2(10000, 1), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 2, 2), new Color(192, 192, 255), Projectile.rotation - angleOffset, new Vector2(0, 1), new Vector2(10000, 1), SpriteEffects.None, 0f);

            Main.instance.LoadProjectile(ProjectileID.DD2FlameBurstTowerT1Shot);
            texture = TextureAssets.Projectile[ProjectileID.DD2FlameBurstTowerT1Shot];
            Main.spriteBatch.Draw(texture.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 72, 72), new Color(192, 192, 255), 5 * MathHelper.Pi * Projectile.timeLeft / 240f, new Vector2(36, 36), new Vector2(0.5f, 3f) * scaleFactor * rotationFactor, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 72, 72), Color.White, 5 * MathHelper.Pi * Projectile.timeLeft / 240f + MathHelper.PiOver2, new Vector2(36, 36), new Vector2(0.5f, 5f) * scaleFactor * rotationFactor, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 72, 72), new Color(192, 192, 255), -5 * MathHelper.Pi * Projectile.timeLeft / 240f, new Vector2(36, 36), new Vector2(0.5f, 5f) * scaleFactor * rotationFactor, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 72, 72), Color.White, -5 * MathHelper.Pi * Projectile.timeLeft / 240f + MathHelper.PiOver2, new Vector2(36, 36), new Vector2(0.5f, 3f) * scaleFactor * rotationFactor, SpriteEffects.None, 0f);

            return false;
        }
    }

    public class MandelbrotOrbMinionClone : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Antibrot Image");

            ProjectileID.Sets.MinionShot[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = false;
        }

        public override void SetDefaults()
        {
            Projectile.width = 94;
            Projectile.height = 94;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 60;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            float alphaFactor = (Projectile.timeLeft % 60) * (60f - (Projectile.timeLeft % 60)) / (30f * 30f) / 2f;
            Projectile.alpha = (int)(255 - 255 * alphaFactor);
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;

                Vector2 displacementVector = new Vector2(Main.rand.NextFloat(0, 8), 0).RotatedByRandom(MathHelper.TwoPi);
                displacementVector.Y *= 2;
                displacementVector = displacementVector.RotatedBy(Projectile.rotation);

                Projectile.ai[0] = displacementVector.X;
                Projectile.ai[1] = displacementVector.Y;
            }

            Projectile.position.X += Projectile.ai[0] * (float)Math.Sin(MathHelper.TwoPi * Projectile.timeLeft / 60f);
            Projectile.position.Y += Projectile.ai[1] * (float)Math.Sin(MathHelper.TwoPi * Projectile.timeLeft / 60f);

            Projectile.velocity *= 0.98f;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float projectileAlpha = (255 - Projectile.alpha) / 255f;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), Color.White * projectileAlpha, Projectile.rotation, new Vector2(127, 55), Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}