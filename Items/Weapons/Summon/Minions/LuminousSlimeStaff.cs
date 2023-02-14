using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Summon.Minions
{
    public class LuminousSlimeStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);

            ItemID.Sets.StaffMinionSlotsRequired[Type] = 0;
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(36, 1f, 0);
            Item.DamageType = DamageClass.Summon;
            Item.mana = 10;

            Item.width = 26;
            Item.height = 34;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = 1;
            Item.noMelee = true;
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = true;

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.LightPurple;

            Item.buffType = BuffType<LuminousSlimeMinionBuff>();
            Item.shoot = ProjectileType<LuminousSlimeMinion>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.ownedProjectileCounts[type] == 0)
            {
                player.AddBuff(Item.buffType, 18000, true);
                player.SpawnMinionOnCursor(source, player.whoAmI, type, damage, knockback);
                return false;
            }
            else
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].type == type && Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI)
                    {
                        Main.projectile[i].ai[1]++;
                        return false;
                    }
                }
            }
            return false;
        }
    }

    public class LuminousSlimeMinionBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ProjectileType<LuminousSlimeMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }

    public class LuminousSlimeMinion : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 42;
            Projectile.height = 42;
            Projectile.penetrate = -1;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.friendly = true;
            Projectile.tileCollide = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        private const float yAcceleration = 0.3f;
        private const int groundTime = 30;
        private const int jumpTime = 30;
        private const float extraHeight = 128f;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            //custom minion slot code
            if (player.slotsMinions + Projectile.ai[1] > player.maxMinions)
            {
                Projectile.ai[1] = player.maxMinions - player.slotsMinions;
                if (Projectile.ai[1] < 0f)
                {
                    Projectile.Kill();
                    return;
                }
            }
            player.slotsMinions += Projectile.ai[1];

            if (!player.active)
            {
                Projectile.active = false;
                return;
            }
            if (player.dead)
            {
                player.ClearBuff(BuffType<LuminousSlimeMinionBuff>());
            }
            if (player.HasBuff(BuffType<LuminousSlimeMinionBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                Projectile.ai[0] = groundTime + jumpTime + 1000;
            }
            else if (Projectile.velocity.Y == 0)
            {
                if (Projectile.oldVelocity.Y >= 0 && Projectile.ai[0] >= groundTime + jumpTime)
                {
                    Projectile.velocity.X = 0;
                    Projectile.ai[0] = 0;
                }
            }

            //if too far away, go to player and reset target and AI
            if ((Projectile.Center - player.Center).Length() > 2000f)
            {
                Projectile.Center = player.Center;
                Projectile.localAI[1] = 0;
                Projectile.ai[0] = 0;
            }

            //update the scale to be bigger the more minion slots are used
            Vector2 oldCenter = Projectile.Center + new Vector2(0, Projectile.height / 2f);
            Projectile.scale = (float)Math.Sqrt(1 + Projectile.ai[1]) * 0.5f;
            Projectile.width = (int)(42 * Projectile.scale);
            Projectile.height = (int)(42 * Projectile.scale);
            Projectile.Center = oldCenter - new Vector2(0, Projectile.height / 2f);

            Projectile.tileCollide = true;

            if (Projectile.localAI[1] == 0 || !Main.npc[(int)Projectile.localAI[1] - 1].CanBeChasedBy(Projectile))
            {
                //find new target
                int targetID = -1;
                Projectile.Minion_FindTargetInRange(750, ref targetID, true);
                Projectile.localAI[1] = targetID + 1;
            }
            Vector2 targetPos;
            Vector2 targetVel;
            bool alwaysJump = false;
            float timeLeftEstimateForEyes = 0f;
            squishing = Vector2.One;
            if (Projectile.localAI[1] == 0)
            {
                targetPos = player.Center + new Vector2(-100 * player.direction, 0);
                targetVel = player.velocity;
            }
            else
            {
                //attack target
                NPC target = Main.npc[(int)Projectile.localAI[1] - 1];
                targetPos = target.Center;
                targetVel = target.velocity;
                alwaysJump = true;
            }
            //ai[0] is grounded time
            if (Projectile.ai[0] < groundTime)
            {
                if (Projectile.velocity.Y != 0)
                {
                    timeLeftEstimateForEyes = groundTime;
                }

                Projectile.velocity.X *= 0.9f;
                Projectile.velocity.Y += yAcceleration;

                if (alwaysJump || Projectile.ai[0] > 0 || ((Projectile.Center - targetPos) * new Vector2(1, 0.25f)).Length() > Projectile.width / 2)
                    Projectile.ai[0]++;

                if (Projectile.ai[0] == groundTime)
                {
                    //retarget
                    int targetID = -1;
                    Projectile.Minion_FindTargetInRange(750, ref targetID, true);
                    Projectile.localAI[1] = targetID + 1;
                }

                squishing += new Vector2(-1, 1) * (0.1f * (float)Math.Sin(Projectile.ai[0] * Projectile.ai[0] * Projectile.ai[0] / 30f / 30f));

                Projectile.rotation = 0f;
            }
            else if (Projectile.ai[0] < groundTime + jumpTime)
            {
                if (Projectile.ai[0] == groundTime && player.HasMinionAttackTargetNPC)
                {
                    //retarget
                    int targetID = -1;
                    Projectile.Minion_FindTargetInRange(750, ref targetID, true);
                    if (targetID == player.MinionAttackTargetNPC)
                        Projectile.localAI[1] = targetID + 1;
                }

                //predict target's motion and attempt to get above them
                float timeLeft = jumpTime + groundTime - Projectile.ai[0];
                Vector2 estimatedGoalOffset = targetPos + targetVel * timeLeft - new Vector2(0, extraHeight) - Projectile.Center;
                float requiredXVelocity = estimatedGoalOffset.X / timeLeft;
                float requiredYVelocity = estimatedGoalOffset.Y / timeLeft - timeLeft / 2 * yAcceleration;
                Vector2 goalVelocity = new Vector2(Math.Clamp(requiredXVelocity, -16, 16), requiredYVelocity);

                timeLeftEstimateForEyes = timeLeft;
                squishing += new Vector2(-1, 1) * 0.05f;

                Projectile.velocity.Y += yAcceleration / 2;

                Projectile.velocity += (goalVelocity - Projectile.velocity) / (Projectile.ai[0] - groundTime + 1);

                Projectile.velocity.Y += yAcceleration / 2;

                Projectile.ai[0]++;

                Projectile.tileCollide = false;

                Projectile.rotation = -(float)Math.Atan(Projectile.velocity.X * 0.2f) * 0.2f;
            }
            else
            {
                if (Projectile.ai[0] == groundTime + jumpTime)
                {
                    if (Projectile.velocity.Y < Math.Min(0, targetVel.Y))
                    {
                        Projectile.velocity.Y = Math.Min(0, targetVel.Y);
                    }
                }

                //predict target's motion and attempt to fall on them
                //estimated fall time
                //myHeight(t) = yAcceleration * t^2 / 2 + velocity.Y * t + startHeight
                //targetHeight(t) = target.velocity.Y * t + targetStartHeight
                //yAcceleration * t^2 / 2 + (velocity.Y - target.velocity.Y) * t + (startHeight - targetStartHeight) = 0
                //solve for t
                float yVelDiff = Projectile.velocity.Y - targetVel.Y;
                float yPosDiff = Projectile.Center.Y - targetPos.Y;
                float discriminant = yVelDiff * yVelDiff - 2 * yPosDiff * yAcceleration;
                if (discriminant > 0)
                {
                    float timeLeft = (-yVelDiff + (float)Math.Sqrt(discriminant)) / yAcceleration;
                    float goalXVelocity = (targetPos.X + targetVel.X * timeLeft - Projectile.Center.X) / timeLeft;
                    goalXVelocity = Math.Clamp(goalXVelocity, -16, 16);
                    Projectile.velocity.X += (goalXVelocity - Projectile.velocity.X) / (Projectile.ai[0] - groundTime - jumpTime + 1);

                    timeLeftEstimateForEyes = timeLeft;
                    squishing += new Vector2(-1, 1) * 0.1f;
                }
                else
                {
                    squishing += new Vector2(-1, 1) * 0.15f;
                }

                Projectile.velocity.Y += yAcceleration;

                Projectile.ai[0]++;

                Projectile.rotation = -(float)Math.Atan(Projectile.velocity.X * 0.2f) * 0.2f;
            }

            Vector2 targetEyePos;
            if (Projectile.localAI[1] == 0)
            {
                targetEyePos = player.Center;
            }
            else
            {
                targetEyePos = targetPos + targetVel * timeLeftEstimateForEyes;
            }

            eyeOffset = (targetEyePos - Projectile.Center);
            float eyeDistMultiplier = (3 + (float)Math.Cos(2 * eyeOffset.ToRotation())) * 0.25f;
            eyeOffset = eyeOffset.SafeNormalize(Vector2.Zero) * eyeDistMultiplier * 10;

            Projectile.light = 0.5f * (float)Math.Sqrt(1 + Projectile.ai[1]);
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            //if not grounded, bounce
            if (Projectile.ai[0] >= groundTime)
            {
                Projectile.ai[0] = groundTime;
                Projectile.localAI[1] = target.whoAmI + 1;
            }
        }

        public override bool MinionContactDamage()
        {
            return Projectile.localAI[1] != 0 && Projectile.ai[0] >= groundTime;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            damage = (int)(damage * (float)(Math.Sqrt(1 + Projectile.ai[1]) + (1 + Projectile.ai[1])) / 2f);
        }

        private Vector2 eyeOffset;
        private Vector2 squishing;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = TextureAssets.Projectile[Type].Frame(1, 2, 0, 0);
            Vector2 center = frame.Size() / 2;
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Color componentColor = new Color(255, 240, 168) * 0.3f;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, componentColor, Projectile.rotation, center, squishing * Projectile.scale * 0.8f, effects, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, componentColor, Projectile.rotation, center, squishing * Projectile.scale * 0.9f, effects, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, componentColor, Projectile.rotation, center, squishing * Projectile.scale, effects, 0);

            for (int i = 0; i < 3; i++)
            {
                float scaleMult = 1 + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f + i * MathHelper.TwoPi / 3f) * 0.1f;

                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, componentColor, Projectile.rotation, center, squishing * Projectile.scale * scaleMult, effects, 0);
            }

            for (int i = 0; i < 3; i++)
            {
                float rotPhase = Main.GlobalTimeWrappedHourly * 2f;
                Vector2 offset = (squishing * Projectile.scale) * Vector2.UnitX.RotatedBy(Projectile.rotation + i * MathHelper.TwoPi / 3f + rotPhase) * 3f;
                Main.EntitySpriteDraw(texture, Projectile.Center + offset - Main.screenPosition, frame, componentColor, Projectile.rotation, center, squishing * Projectile.scale, effects, 0);
            }

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                float scaleMult = 1 - i / (float)Projectile.oldPos.Length;
                Main.EntitySpriteDraw(texture, Projectile.Center + Projectile.oldPos[i] - Projectile.position - Main.screenPosition, frame, componentColor, Projectile.rotation, center, squishing * Projectile.scale * scaleMult, effects, 0);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(0, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, componentColor, Projectile.rotation, center, squishing * Projectile.scale * 0.8f, effects, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, componentColor, Projectile.rotation, center, squishing * Projectile.scale * 0.9f, effects, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, componentColor, Projectile.rotation, center, squishing * Projectile.scale, effects, 0);

            for (int i = 0; i < 3; i++)
            {
                float scaleMult = 1 + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f + i * MathHelper.TwoPi / 3f) * 0.1f;

                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, componentColor, Projectile.rotation, center, squishing * Projectile.scale * scaleMult, effects, 0);
            }

            for (int i = 0; i < 3; i++)
            {
                float rotPhase = Main.GlobalTimeWrappedHourly * 2f;
                Vector2 offset = (squishing * Projectile.scale) * Vector2.UnitX.RotatedBy(Projectile.rotation + i * MathHelper.TwoPi / 3f + rotPhase) * 3f;
                Main.EntitySpriteDraw(texture, Projectile.Center + offset - Main.screenPosition, frame, componentColor, Projectile.rotation, center, squishing * Projectile.scale, effects, 0);
            }

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                float scaleMult = 1 - i / (float)Projectile.oldPos.Length;
                Main.EntitySpriteDraw(texture, Projectile.Center + Projectile.oldPos[i] - Projectile.position - Main.screenPosition, frame, componentColor, Projectile.rotation, center, squishing * Projectile.scale * scaleMult, effects, 0);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            //eye draw
            Color eyeColor = new Color(255, 240, 168).MultiplyRGBA(new Color(0.5f, 0.5f, 0.5f, 0.6f));
            Main.EntitySpriteDraw(texture, Projectile.Center + (squishing * Projectile.scale) * eyeOffset - Main.screenPosition, TextureAssets.Projectile[Type].Frame(1, 2, 0, 1), eyeColor, -Projectile.rotation, center, squishing * Projectile.scale, effects, 0);

            return false;
        }
    }
}