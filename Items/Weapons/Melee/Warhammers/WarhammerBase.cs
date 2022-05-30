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

namespace Polarities.Items.Weapons.Melee.Warhammers
{
	public abstract class WarhammerBase : ModItem
	{
        public abstract int HammerLength { get; }
        public abstract int HammerHeadSize { get; }
        public abstract int DefenseLoss { get; }
        public abstract int DebuffTime { get; }

        //the total time spent on the actual swing
        public virtual float SwingTime => 10f;

        public virtual SoundStyle? SwingSound => SoundID.Item1;

        static Vector2 oldHitboxPosition;
        static bool hasHitTile;
        static float oldRot;
        static float mostRecentRotation;

        public const int WarhammerUseStyle = 1728;

        //TODO: Sound effect on hammering enemies (very small chance to be replaced with the funny bonk sound)
        //TODO: Adjust anim's rotation so it doesn't snap back when autoswinging

        public override void SetStaticDefaults()
        {
            this.SetResearch(1);
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            Hit(player, target, DefenseLoss, DebuffTime);
        }

        //gets the total amount of the swing spent on windup, from 0 to 1
        public float SwingWindup(Player player)
        {
            //this ensures we always spend SwingTime in the actual swing, unless the item animation is shorter than SwingTime
            return 1 - SwingTime / Math.Max(player.itemAnimationMax, SwingTime);
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (Item.useStyle != WarhammerUseStyle)
            {
                return;
            }

            float animationProgress = 1 - (player.itemAnimation - 1) / (float)player.itemAnimationMax;
            float swingWindup = SwingWindup(player);

            if (animationProgress < swingWindup)
            {
                hasHitTile = false;
            }

            if (!hasHitTile)
            {
                if (SwingSound != null && animationProgress >= swingWindup && 1 - player.itemAnimation / (float)player.itemAnimationMax < swingWindup)
                {
                    SoundEngine.PlaySound((SoundStyle)SwingSound, player.Center);
                }

                if (animationProgress < swingWindup)
                {
                    float motionProgress = 1 - (1 - animationProgress / swingWindup) * (1 - animationProgress / swingWindup);
                    player.itemRotation = -MathHelper.PiOver4 - MathHelper.PiOver2 * motionProgress;

                    if (!player.mount.Active)
                        player.fullRotation = -player.direction * player.gravDir * 0.1f * motionProgress;
                }
                else
                {
                    float motionProgress = (animationProgress - swingWindup) / (1 - swingWindup);
                    player.itemRotation = -3 * MathHelper.PiOver4 + MathHelper.Pi * 1.25f * motionProgress;

                    if (!player.mount.Active)
                        player.fullRotation = player.direction * player.gravDir * (-0.1f + 0.5f * motionProgress);
                }

                float fullRot = player.fullRotation;
                player.fullRotation = 0;

                player.itemRotation *= player.direction * player.gravDir;

                bool goodRotation = player.itemRotation * player.direction * player.gravDir <= -MathHelper.PiOver4;

                Rectangle hitbox = GetHitbox(player);

                if (!hasHitTile && oldHitboxPosition != Vector2.Zero && !goodRotation)
                {
                    int steps = Math.Max(1, (int)(hitbox.TopLeft() - oldHitboxPosition).Length() / 4);
                    Vector2 velocity = (hitbox.TopLeft() - oldHitboxPosition) / steps;
                    for (int i = 0; i < steps; i++)
                    {
                        Vector2 testPos = Vector2.Lerp(oldHitboxPosition, hitbox.TopLeft(), i / (float)steps);

                        bool goodYPosition = (testPos.Y + hitbox.Height / 2f) * player.gravDir + hitbox.Height / 2f > player.Center.Y * player.gravDir;

                        Vector2 colVelocity = Collision.noSlopeCollision(testPos, velocity, hitbox.Width, hitbox.Height, true, true);

                        if (goodYPosition && colVelocity != velocity)
                        {
                            Collision.HitTiles(testPos - new Vector2(1, 1), colVelocity, hitbox.Width + 1, hitbox.Height + 1);
                            TileSound(testPos - new Vector2(1, 1), colVelocity, hitbox.Width + 1, hitbox.Height + 1);
                            hasHitTile = true;
                            mostRecentRotation = Utils.AngleLerp(oldRot, player.itemRotation, (i + colVelocity.Y / velocity.Y) / steps);

                            player.itemRotation = mostRecentRotation;

                            OnHitTiles(player);
                            break;
                        }
                    }
                }

                player.fullRotation = fullRot;
            }
            else
            {
                player.itemRotation = mostRecentRotation;
            }

            if (player.itemRotation * player.direction * player.gravDir <= -MathHelper.PiOver4 && !hasHitTile)
            {
                player.itemLocation = player.MountedCenter.Floor() + new Vector2(player.direction * -6, player.gravDir * -10);
            }
            else
            {
                player.itemLocation = player.MountedCenter.Floor() + new Vector2(player.direction * 6, -player.gravDir * 6).RotatedBy(player.itemRotation);
            }

            ModifyItemPosition(player);

            oldRot = player.itemRotation;

            //adjust for player fullRotation
            player.itemRotation -= player.fullRotation;
            player.itemLocation = (player.position + player.fullRotationOrigin) + (player.itemLocation - (player.position + player.fullRotationOrigin)).RotatedBy(-player.fullRotation);
        }

        public override void UseItemFrame(Player player)
        {
            if (Item.useStyle != WarhammerUseStyle)
            {
                return;
            }

            float num23 = (player.itemRotation - player.fullRotation) * player.direction * player.gravDir - MathHelper.PiOver4;
            player.bodyFrame.Y = player.bodyFrame.Height * 3;
            if ((double)num23 < -0.75)
            {
                player.bodyFrame.Y = player.bodyFrame.Height * 2;
            }
            if ((double)num23 > 0.6)
            {
                player.bodyFrame.Y = player.bodyFrame.Height * 4;
            }
            if ((double)num23 < -1.5)
            {
                player.bodyFrame.Y = player.bodyFrame.Height;
            }
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if (Item.useStyle != WarhammerUseStyle)
            {
                return;
            }

            hitbox = GetHitbox(player);
            oldHitboxPosition = hitbox.TopLeft();
        }

        public override bool? CanHitNPC(Player player, NPC target)
        {
            if (Item.useStyle != WarhammerUseStyle)
            {
                return null;
            }

            float animationProgress = 1 - (player.itemAnimation - 1) / (float)player.itemAnimationMax;

            if (animationProgress < SwingWindup(player)) return false;

            return null;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            foreach (TooltipLine tooltip in tooltips)
            {
                if (tooltip.Name.StartsWith("Tooltip"))
                {
                    int boostedDefenseLoss = DefenseLoss + Main.LocalPlayer.GetModPlayer<PolaritiesPlayer>().warhammerDefenseBoost;
                    int boostedDebuffTime = DebuffTime + Main.LocalPlayer.GetModPlayer<PolaritiesPlayer>().warhammerTimeBoost;
                    tooltip.Text = tooltip.Text.Replace("{DefenseLoss}", (boostedDefenseLoss).ToString()).Replace("{DebuffTime}", (boostedDebuffTime / 60).ToString());
                }
            }
        }

        public static void TileSound(Vector2 Position, Vector2 Velocity, int Width, int Height)
        {
            Vector2 vector = Position + Velocity;
            int num = (int)(Position.X / 16f) - 1;
            int num2 = (int)((Position.X + (float)Width) / 16f) + 2;
            int num3 = (int)(Position.Y / 16f) - 1;
            int num4 = (int)((Position.Y + (float)Height) / 16f) + 2;
            if (num < 0)
            {
                num = 0;
            }
            if (num2 > Main.maxTilesX)
            {
                num2 = Main.maxTilesX;
            }
            if (num3 < 0)
            {
                num3 = 0;
            }
            if (num4 > Main.maxTilesY)
            {
                num4 = Main.maxTilesY;
            }
            Vector2 vector2 = default(Vector2);
            for (int i = num; i < num2; i++)
            {
                for (int j = num3; j < num4; j++)
                {
                    if (!Main.tile[i, j].IsActuated && Main.tile[i, j].HasTile && (Main.tileSolid[Main.tile[i, j].TileType] || (Main.tileSolidTop[Main.tile[i, j].TileType] && Main.tile[i, j].TileFrameY == 0)))
                    {
                        vector2.X = i * 16;
                        vector2.Y = j * 16;
                        int num5 = 16;
                        if (Main.tile[i, j].IsHalfBlock)
                        {
                            vector2.Y += 8f;
                            num5 -= 8;
                        }
                        if (vector.X + (float)Width >= vector2.X && vector.X <= vector2.X + 16f && vector.Y + (float)Height >= vector2.Y && vector.Y <= vector2.Y + (float)num5)
                        {
                            WorldGen.KillTile_PlaySounds(i, j, true, Main.tile[i, j]);
                        }
                    }
                }
            }
        }

        public static void Hit(Player player, NPC target, int DefenseLoss, int DebuffTime)
        {
            int boostedDefLoss = DefenseLoss + player.GetModPlayer<PolaritiesPlayer>().warhammerDefenseBoost;
            int boostedDefTime = DebuffTime + player.GetModPlayer<PolaritiesPlayer>().warhammerTimeBoost;

            if (target.GetGlobalNPC<PolaritiesNPC>().hammerTimes.ContainsKey(boostedDefLoss))
            {
                target.GetGlobalNPC<PolaritiesNPC>().hammerTimes[boostedDefLoss] = Math.Max(target.GetGlobalNPC<PolaritiesNPC>().hammerTimes[boostedDefLoss], boostedDefTime);
            }
            else
            {
                target.GetGlobalNPC<PolaritiesNPC>().hammerTimes.Add(boostedDefLoss, boostedDefTime);
            }
        }

        public Vector2 GetHitboxCenter(Player player)
        {
            return player.position + player.fullRotationOrigin + (player.itemLocation - (player.position + player.fullRotationOrigin) + new Vector2(player.direction * HammerLength, -HammerLength).RotatedBy(player.itemRotation).RotatedBy(player.gravDir == 1 ? 0 : player.direction * MathHelper.PiOver2)).RotatedBy(player.fullRotation);
        }

        public Rectangle GetHitbox(Player player)
        {
            Vector2 hitboxDisplacement = GetHitboxCenter(player);
            return new Rectangle((int)hitboxDisplacement.X - HammerHeadSize, (int)hitboxDisplacement.Y - HammerHeadSize, HammerHeadSize * 2, HammerHeadSize * 2);
        }

        public virtual void OnHitTiles(Player player) { }

        public virtual void ModifyItemPosition(Player player) { }
    }
}

