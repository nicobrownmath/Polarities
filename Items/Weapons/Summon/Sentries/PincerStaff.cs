using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Summon.Sentries
{
    public class PincerStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(29, 2, 0);
            Item.DamageType = DamageClass.Summon;
            Item.sentry = true;
            Item.mana = 5;

            Item.width = 48;
            Item.height = 48;
            Item.useTime = 30;
            Item.useAnimation = 30;

            Item.useStyle = 1;
            Item.noMelee = true;

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.LightPurple;

            Item.UseSound = SoundID.Item44;
            Item.autoReuse = true;
            Item.shoot = ProjectileType<PincerStaffSentry>();
            Item.shootSpeed = 0f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.SpawnSentry(source, player.whoAmI, type, damage, knockback);
            return false;
        }
    }

    public class PincerStaffSentry : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 46;
            Projectile.height = 36;

            DrawOffsetX = -14;
            DrawOriginOffsetY = -30;
            DrawOriginOffsetX = 0;

            Projectile.penetrate = -1;
            Projectile.sentry = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.timeLeft = Projectile.SentryLifeTime;
        }

        private int scytheLeftFrame;
        private int scytheLeftFrameCounter;
        private NPC scytheTargetLeft;
        private int scytheRightFrame;
        private int scytheRightFrameCounter;
        private NPC scytheTargetRight;

        public override void AI()
        {
            Projectile.velocity.Y++;

            int leftTargetID = FindMinionTarget(Projectile, -1, 750f);
            if (leftTargetID != -1)
            {
                ScytheLeft(Main.npc[leftTargetID]);
            }
            int rightTargetID = FindMinionTarget(Projectile, 1, 750f);
            if (rightTargetID != -1)
            {
                ScytheRight(Main.npc[rightTargetID]);
            }

            UpdateScythes();
        }

        private void ScytheLeft(NPC target)
        {
            scytheTargetLeft = target;

            if (scytheLeftFrameCounter == 0)
            {
                scytheLeftFrameCounter++;
            }
        }

        private void ScytheRight(NPC target)
        {
            scytheTargetRight = target;

            if (scytheRightFrameCounter == 0)
            {
                scytheRightFrameCounter++;
            }
        }

        private void UpdateScythes()
        {
            int speed = 12;

            if (scytheLeftFrameCounter != 0)
            {
                scytheLeftFrameCounter++;
                if (scytheLeftFrameCounter == 6)
                {
                    scytheLeftFrameCounter = 1;
                    scytheLeftFrame++;
                    if (scytheLeftFrame == 4)
                    {
                        scytheLeftFrame = 0;
                        scytheLeftFrameCounter = 0;

                        //shoot scythe
                        Vector2 shotPosition = Projectile.Center + new Vector2(-24, -20);

                        if (scytheTargetLeft != null)
                        {
                            if (scytheTargetLeft.active)
                            {

                                float shotRotation = (scytheTargetLeft.Center - shotPosition).ToRotation();

                                Projectile.NewProjectile(Projectile.GetSource_FromAI(), shotPosition, new Vector2(speed, 0).RotatedBy(shotRotation), ProjectileType<PincerStaffSlash>(), Projectile.damage, 2f, Main.myPlayer);
                                Projectile.NewProjectile(Projectile.GetSource_FromAI(), shotPosition, new Vector2(speed, 0).RotatedBy(shotRotation + 0.05f), ProjectileType<PincerStaffSlash>(), Projectile.damage, 2f, Main.myPlayer);
                                Projectile.NewProjectile(Projectile.GetSource_FromAI(), shotPosition, new Vector2(speed, 0).RotatedBy(shotRotation - 0.05f), ProjectileType<PincerStaffSlash>(), Projectile.damage, 2f, Main.myPlayer);

                                SoundEngine.PlaySound(SoundID.Item71, shotPosition);
                            }
                        }
                    }
                }
            }
            if (scytheRightFrameCounter != 0)
            {
                scytheRightFrameCounter++;
                if (scytheRightFrameCounter == 6)
                {
                    scytheRightFrameCounter = 1;
                    scytheRightFrame++;
                    if (scytheRightFrame == 4)
                    {
                        scytheRightFrame = 0;
                        scytheRightFrameCounter = 0;

                        //shoot scythe
                        Vector2 shotPosition = Projectile.Center + new Vector2(24, -20);

                        if (scytheTargetRight != null)
                        {
                            if (scytheTargetRight.active)
                            {

                                float shotRotation = (scytheTargetRight.Center - shotPosition).ToRotation();

                                Projectile.NewProjectile(Projectile.GetSource_FromAI(), shotPosition, new Vector2(speed, 0).RotatedBy(shotRotation), ProjectileType<PincerStaffSlash>(), Projectile.damage, 2f, Main.myPlayer);
                                Projectile.NewProjectile(Projectile.GetSource_FromAI(), shotPosition, new Vector2(speed, 0).RotatedBy(shotRotation + 0.05f), ProjectileType<PincerStaffSlash>(), Projectile.damage, 2f, Main.myPlayer);
                                Projectile.NewProjectile(Projectile.GetSource_FromAI(), shotPosition, new Vector2(speed, 0).RotatedBy(shotRotation - 0.05f), ProjectileType<PincerStaffSlash>(), Projectile.damage, 2f, Main.myPlayer);

                                SoundEngine.PlaySound(SoundID.Item71, shotPosition);
                            }
                        }
                    }
                }
            }
        }

        //direction -1 is the left scythe
        //direction 1 is the right scythe
        private int FindMinionTarget(Projectile projectile, int direction, float maxDistance = 400f, bool usePlayerDistance = false, bool requireLineOfSight = true, bool strictMaxDistance = false, bool respectTarget = true)
        {
            int targetID = -1;

            bool hasTarget = false;

            Vector2 center = Projectile.Center;
            if (usePlayerDistance)
            {
                center = Main.player[Projectile.owner].Center;
            }

            NPC ownerMinionAttackTargetNPC = Projectile.OwnerMinionAttackTargetNPC;
            if (respectTarget && ownerMinionAttackTargetNPC != null && ownerMinionAttackTargetNPC.CanBeChasedBy(projectile) && (Projectile.Center.X - ownerMinionAttackTargetNPC.Center.X > 0 ? -1 : 1) == direction)
            {
                float num63 = Vector2.Distance(ownerMinionAttackTargetNPC.Center, center);
                if ((num63 < maxDistance || (!hasTarget && !strictMaxDistance)) && (!requireLineOfSight || Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, ownerMinionAttackTargetNPC.position, ownerMinionAttackTargetNPC.width, ownerMinionAttackTargetNPC.height)))
                {
                    maxDistance = num63;
                    hasTarget = true;
                    targetID = ownerMinionAttackTargetNPC.whoAmI;
                }
            }
            if (!hasTarget)
            {
                for (int k = 0; k < Main.maxNPCs; k++)
                {
                    NPC nPC = Main.npc[k];
                    if (nPC.CanBeChasedBy(projectile) && (Projectile.Center.X - nPC.Center.X > 0 ? -1 : 1) == direction)
                    {
                        float num62 = Vector2.Distance(nPC.Center, center);
                        if ((num62 < maxDistance || (!hasTarget && !strictMaxDistance)) && (!requireLineOfSight || Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, nPC.position, nPC.width, nPC.height)))
                        {
                            maxDistance = num62;
                            hasTarget = true;
                            targetID = k;
                        }
                    }
                }
            }

            return targetID;
        }

        public static Asset<Texture2D> ClawTexture;

        public override void Load()
        {
            ClawTexture = Request<Texture2D>(Texture + "_Claw");
        }

        public override void Unload()
        {
            ClawTexture = null;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Rectangle leftFrame = ClawTexture.Frame(1, 4, 0, scytheLeftFrame);
            Rectangle rightFrame = ClawTexture.Frame(1, 4, 0, scytheRightFrame);

            Main.EntitySpriteDraw(ClawTexture.Value, Projectile.Center - Main.screenPosition + new Vector2(0, -13), leftFrame, lightColor, Projectile.rotation, leftFrame.Size() / 2, Projectile.scale, SpriteEffects.FlipHorizontally, 0);
            Main.EntitySpriteDraw(ClawTexture.Value, Projectile.Center - Main.screenPosition + new Vector2(0, -13), rightFrame, lightColor, Projectile.rotation, rightFrame.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            return true;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool MinionContactDamage()
        {
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }
    }

    public class PincerStaffSlash : ModProjectile
    {
        public override string Texture => "Polarities/NPCs/Enemies/WorldEvilInvasion/CrimagoSlash";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.SentryShot[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 20;
            Projectile.height = 20;
            DrawOffsetX = -6;
            Projectile.DamageType = DamageClass.Summon;
            DrawOriginOffsetY = 0;
            DrawOriginOffsetX = 0;
            Projectile.alpha = 0;
            Projectile.timeLeft = 70;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.timeLeft < 256 / 8)
            {
                Projectile.alpha += 8;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float trailLength = 4f;
            float trailSize = 4;
            for (int i = (int)trailLength - 1; i >= 0; i--)
            {
                Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Projectile.velocity * (i / trailLength) * trailSize - Main.screenPosition, TextureAssets.Projectile[Type].Frame(), lightColor * (1 - Projectile.alpha / 256f) * (1 - i / trailLength), Projectile.rotation, TextureAssets.Projectile[Type].Size() / 2, Projectile.scale * (1 - i / trailLength), SpriteEffects.None, 0);
            }
            return true;
        }
    }
}

