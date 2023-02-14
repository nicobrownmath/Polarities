using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.NPCs.Enemies.HallowInvasion;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Summon.Sentries
{
    public class ScouringStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(40, 2f, 0);
            Item.DamageType = DamageClass.Summon;
            Item.sentry = true;
            Item.mana = 5;
            Item.knockBack = 2f;

            Item.width = 50;
            Item.height = 50;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = 1;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.UseSound = SoundID.Item44;

            Item.shoot = ProjectileType<ScouringStaffSentry>();
            Item.shootSpeed = 0f;

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.LightPurple;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.SpawnSentry(source, player.whoAmI, type, damage, knockback, spawnWithGravity: false);
            return false;
        }
    }

    public class ScouringStaffSentry : ModProjectile
    {
        public override string Texture => "Polarities/NPCs/Enemies/HallowInvasion/IlluminantScourer";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        private float[] plateAngle = new float[6];
        private float[] plateDistance = new float[6];

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.penetrate = -1;
            Projectile.sentry = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = Projectile.SentryLifeTime;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            int targetID = -1;
            Projectile.Minion_FindTargetInRange(750, ref targetID, true);
            NPC target = null;
            if (targetID != -1)
            {
                target = Main.npc[targetID];
            }

            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;

                //initialize plate angle and distance
                for (int i = 0; i < plateAngle.Length; i++)
                {
                    plateAngle[i] = i * MathHelper.TwoPi / plateAngle.Length;
                    plateDistance[i] = 32;
                }
            }

            if (target == null)
            {
                //no target, stop the whole timer thing if we reach 0
                if (Projectile.ai[0] < 120 && Projectile.ai[0] != 0)
                {
                    Projectile.ai[0]++;
                    if (Projectile.ai[0] == 120)
                    {
                        Projectile.ai[0] = 0;
                    }
                }
            }
            else
            {
                if (Projectile.ai[0] <= 30)
                {
                    Projectile.rotation = (target.Center - Projectile.Center).ToRotation();

                    if (Projectile.ai[0] == 30)
                    {
                        for (int i = 0; i < plateAngle.Length; i++)
                        {
                            Vector2 bouncePosition = new Vector2(plateDistance[i], 0).RotatedBy(plateAngle[i]);
                            Main.projectile[Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ProjectileType<IlluminantLaserFriendly>(), Projectile.damage, Projectile.knockBack, Main.myPlayer, bouncePosition.X, bouncePosition.Y)].localAI[1] = Projectile.whoAmI;
                        }
                    }
                }

                //timer thing behaves normally
                if (Projectile.ai[0] < 120)
                {
                    Projectile.ai[0]++;
                    if (Projectile.ai[0] == 120)
                    {
                        Projectile.ai[0] = 0;
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame();

            float randomOffset = (Projectile.ai[0] < 30 ? 0 : (120 - Projectile.ai[0]) / 10f);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White, 0f, frame.Size() / 2, Projectile.scale, SpriteEffects.None, 0);

            //Adapted from vanilla illuminant NPC draw code
            for (int num347 = 1; num347 < Projectile.oldPos.Length; num347++)
            {
                Color color27 = new Color((byte)(150 * (10 - num347) / 15), (byte)(100 * (10 - num347) / 15), (byte)(150 * (10 - num347) / 15), (byte)(50 * (10 - num347) / 15));
                Main.EntitySpriteDraw(texture, new Vector2(Main.rand.NextFloat(randomOffset)).RotatedByRandom(MathHelper.TwoPi) + Projectile.oldPos[num347] + Projectile.Center - Projectile.position - Main.screenPosition, frame, color27, 0f, frame.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            }

            //Draw plates
            Texture2D plateTexture = IlluminantScourer.PlateTexture.Value;
            Rectangle plateFrame = plateTexture.Frame();
            for (int i = 0; i < plateAngle.Length; i++)
            {
                Main.EntitySpriteDraw(plateTexture, Projectile.Center - Main.screenPosition + new Vector2(plateDistance[i] + 4, 0).RotatedBy(plateAngle[i]), plateFrame, Color.White, (Projectile.rotation + plateAngle[i]) / 2, plateFrame.Size() / 2, Projectile.scale, SpriteEffects.None, 0);

                for (int num347 = 1; num347 < Projectile.oldPos.Length; num347++)
                {
                    Color color27 = new Color((byte)(150 * (10 - num347) / 15), (byte)(100 * (10 - num347) / 15), (byte)(150 * (10 - num347) / 15), (byte)(50 * (10 - num347) / 15));
                    Main.EntitySpriteDraw(plateTexture, new Vector2(Main.rand.NextFloat(randomOffset)).RotatedByRandom(MathHelper.TwoPi) + Projectile.oldPos[num347] + Projectile.Center - Projectile.position - Main.screenPosition + new Vector2(plateDistance[i] + 4, 0).RotatedBy(plateAngle[i]), plateFrame, color27, (Projectile.rotation + plateAngle[i]) / 2, plateFrame.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
                }
            }

            return false;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool MinionContactDamage()
        {
            return false;
        }
    }

    public class IlluminantLaserFriendly : ModProjectile
    {
        public override string Texture => "Polarities/NPCs/Enemies/HallowInvasion/IlluminantScourerLaser";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("{Mods.Polarities.ProjectileName.IlluminantScourerLaser");

            ProjectileID.Sets.SentryShot[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.DamageType = DamageClass.Summon;

            Projectile.timeLeft = 90;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }

        private Vector2 bouncePoint
        {
            get => Projectile.Center + new Vector2(Projectile.ai[0], Projectile.ai[1]);
        }

        public override void AI()
        {
            if (!Main.projectile[(int)Projectile.localAI[1]].active || Main.projectile[(int)Projectile.localAI[1]].ai[0] < 30)
            {
                Projectile.Kill();
                return;
            }
            Projectile.rotation = Main.projectile[(int)Projectile.localAI[1]].rotation;

            //set distance
            Projectile.localAI[0] = 16 - bouncePoint.Y % 16;
            while (Projectile.localAI[0] < 2000)
            {
                Projectile.localAI[0] += 16;
                Tile tile = Main.tile[(bouncePoint + new Vector2(Projectile.localAI[0], 0).RotatedBy(Projectile.rotation)).ToPoint().X / 16, (bouncePoint + new Vector2(Projectile.localAI[0], 0).RotatedBy(Projectile.rotation)).ToPoint().Y / 16];
                if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]) break;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), bouncePoint, bouncePoint + new Vector2(Projectile.localAI[0], 0).RotatedBy(Projectile.rotation));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float scale = (float)(5 + Math.Sin(Main.GlobalTimeWrappedHourly * 60f / 1.5f)) / 5f * 0.5f;

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame();

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White, (bouncePoint - Projectile.Center).ToRotation() - MathHelper.PiOver2, new Vector2(16, 0), new Vector2(scale, (bouncePoint - Projectile.Center).Length()), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, bouncePoint - Main.screenPosition, frame, Color.White, Projectile.rotation - MathHelper.PiOver2, new Vector2(16, 0), new Vector2(scale, Projectile.localAI[0]), SpriteEffects.None, 0);

            Texture2D laserCapTexture = IlluminantScourerLaser.CapTexture.Value;
            Rectangle laserCapFrame = laserCapTexture.Frame();

            Main.EntitySpriteDraw(laserCapTexture, bouncePoint + new Vector2(Projectile.localAI[0], 0).RotatedBy(Projectile.rotation) - Main.screenPosition, laserCapFrame, Color.White, Projectile.rotation - MathHelper.PiOver2, new Vector2(laserCapFrame.Size().X / 2, 0), scale, SpriteEffects.None, 0);

            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
        }
    }
}