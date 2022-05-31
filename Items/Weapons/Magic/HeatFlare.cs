using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using System;
using Polarities.Projectiles;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.Audio;
using System.Collections.Generic;
using Polarities.Effects;
using Terraria.DataStructures;

namespace Polarities.Items.Weapons.Magic
{
    public class HeatFlare : ModItem
    {
        public override void SetStaticDefaults()
        {
            this.SetResearch(1);

            Tooltip.SetDefault("Creates tendrils of heat");
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(38, 4f, 0);
            Item.DamageType = DamageClass.Magic;
            Item.mana = 4;
            Item.noMelee = true;

            Item.width = 30;
            Item.height = 32;

            Item.useTime = 0;
            Item.useAnimation = 0;
            Item.channel = true;
            Item.useStyle = 5;
            Item.autoReuse = false;

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.LightPurple;

            Item.UseSound = SoundID.Item34;
            Item.shoot = ProjectileType<HeatFlareProjectile>();
            Item.shootSpeed = 0.01f;
        }

        private int time;

        public override void HoldItem(Player player)
        {
            if (player.channel)
            {
                player.direction = (Main.MouseWorld.X - player.Center.X > 0) ? 1 : -1;
                time++;
                if (!player.ItemTimeIsZero) player.itemTime = player.itemTimeMax;
                player.itemAnimation = player.itemAnimationMax;
                player.manaRegen = Math.Min(player.manaRegen, 0);
                if (time % 10 == 0)
                {
                    if (!player.CheckMana(player.inventory[player.selectedItem].mana, true))
                    {
                        player.channel = false;
                    }
                }
                if (time % 20 == 0 && Item.UseSound != null)
                {
                    SoundEngine.PlaySound((SoundStyle)Item.UseSound, player.position);
                }
            }
        }
    }

    public class HeatFlareProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_644";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("{$Mods.Polarities.ItemName.HeatFlare}");
        }

        //trail stuff
        const int trailLength = 128;
        Vector2[] trailPositions;
        Vector2[] trailVelocities;
        float[] trailScale;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = false;

            //uses local npc hit cooldown
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
        }

        public override void OnSpawn(IEntitySource source)
        {
            trailPositions = new Vector2[trailLength];
            trailVelocities = new Vector2[trailLength];
            trailScale = new float[trailLength];
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            //initialize trail
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                for (int i = 0; i < trailPositions.Length; i++)
                {
                    trailPositions[i] = Projectile.Center;
                    trailScale[i] = 1 - i / (float)trailPositions.Length;
                }
            }

            if (!player.dead && player.active && player.channel)
            {
                //attached state
                Projectile.Center = player.Center;
                Projectile.timeLeft = 2;
            }
            else
            {
                Projectile.Kill();
                return;
            }

            const int iterations = 2;
            for (int k = 0; k < iterations; k++)
            {
                //update velocities
                for (int i = 0; i < trailPositions.Length; i++)
                {
                    trailPositions[i] += trailVelocities[i];
                }

                //shift trail by a certain amount
                float trailShift = 1;

                for (int i = trailPositions.Length - 1; i >= (int)trailShift; i--)
                {
                    trailPositions[i] = trailPositions[i - (int)trailShift];
                    trailScale[i] = Math.Max(0f, trailScale[i - (int)trailShift] - (int)trailShift / (float)trailPositions.Length);
                }
                for (int i = (int)trailShift - 1; i >= 0; i--)
                {
                    trailPositions[i] = (Projectile.Center * ((int)trailShift - i) + trailPositions[0] * i) / (int)trailShift;
                    trailScale[i] = 1f;
                }

                Projectile.ai[0] += Main.rand.NextFloat(-1f, 1f) * 0.05f;
                Projectile.ai[0] *= 0.95f;

                //trail homogenization
                for (int i = trailPositions.Length - 2; i > 0; i--)
                {
                    trailPositions[i] = (trailPositions[i + 1] + 2 * trailPositions[i] + (2 + trailShift) * trailPositions[i - 1]) / (5 + trailShift);
                    trailVelocities[i] = (trailVelocities[i + 1] + 2 * trailVelocities[i] + (2 + trailShift) * trailVelocities[i - 1]) / (5 + trailShift);
                }
                trailPositions[0] = (trailPositions[1] + 2 * trailPositions[0] + (2 + trailShift) * Projectile.Center) / (5 + trailShift);
                trailVelocities[0] = (trailVelocities[1] + 2 * trailVelocities[0] + (2 + trailShift) * ((player.position - player.oldPosition) / iterations + (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero) * 16f / iterations)).RotatedBy(Projectile.ai[0]) / (5 + trailShift);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (int i = 0; i < trailPositions.Length - 1; i++)
            {
                if (trailScale[i] <= 0.1f)
                {
                    //trailScale is nonincreasing
                    return false;
                }

                if (Collision.CheckAABBvAABBCollision(targetHitbox.TopLeft(), targetHitbox.Size(), trailPositions[i] - new Vector2(trailScale[i] * 15), new Vector2(trailScale[i] * 30)))
                {
                    return true;
                }
                if (trailScale[i] <= 0.35f)
                {
                    if (Collision.CheckAABBvAABBCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Main.player[Projectile.owner].Center + (trailPositions[i] - Main.player[Projectile.owner].Center).RotatedBy(0.1f) - new Vector2((trailScale[i] - 0.25f) * 15), new Vector2((trailScale[i] - 0.25f) * 30)))
                    {
                        return true;
                    }
                    if (Collision.CheckAABBvAABBCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Main.player[Projectile.owner].Center + (trailPositions[i] - Main.player[Projectile.owner].Center).RotatedBy(-0.1f) - new Vector2((trailScale[i] - 0.25f) * 15), new Vector2((trailScale[i] - 0.25f) * 30)))
                    {
                        return true;
                    }
                }
                if (trailScale[i] <= 0.6f)
                {
                    if (Collision.CheckAABBvAABBCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Main.player[Projectile.owner].Center + (trailPositions[i] - Main.player[Projectile.owner].Center).RotatedBy(0.2f) - new Vector2((trailScale[i] - 0.5f) * 15), new Vector2((trailScale[i] - 0.5f) * 30)))
                    {
                        return true;
                    }
                    if (Collision.CheckAABBvAABBCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Main.player[Projectile.owner].Center + (trailPositions[i] - Main.player[Projectile.owner].Center).RotatedBy(-0.2f) - new Vector2((trailScale[i] - 0.5f) * 15), new Vector2((trailScale[i] - 0.5f) * 30)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame();

            float alphaMult = DrawLayer.IsActive<DrawLayerAdditiveAfterProjectiles>() ? 0.5f : 0.75f;

            for (int i = trailPositions.Length - 2; i >= 0; i--)
            {
                if (trailScale[i] >= 0.05f)
                {
                    Color drawColor = Main.hslToRgb(trailScale[i] * 0.2f, 1f, 0.5f + trailScale[i] * 0.4f) * alphaMult;
                    Main.EntitySpriteDraw(texture, trailPositions[i] - Main.screenPosition, frame, drawColor, (trailPositions[i + 1] - trailPositions[i]).ToRotation() + MathHelper.PiOver2, frame.Size() / 2, trailScale[i], SpriteEffects.None, 0);
                }
                if (trailScale[i] >= 0.3f)
                {
                    Color drawColor = Main.hslToRgb((trailScale[i] - 0.25f) * 0.2f, 1f, 0.5f + (trailScale[i] - 0.25f) * 0.6f) * alphaMult;
                    Main.EntitySpriteDraw(texture, Main.player[Projectile.owner].Center + (trailPositions[i] - Main.player[Projectile.owner].Center).RotatedBy(0.1f) - Main.screenPosition, frame, drawColor, (trailPositions[i + 1] - trailPositions[i]).ToRotation() + MathHelper.PiOver2 + 0.1f, frame.Size() / 2, (trailScale[i] - 0.25f), SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(texture, Main.player[Projectile.owner].Center + (trailPositions[i] - Main.player[Projectile.owner].Center).RotatedBy(-0.1f) - Main.screenPosition, frame, drawColor, (trailPositions[i + 1] - trailPositions[i]).ToRotation() + MathHelper.PiOver2 - 0.1f, frame.Size() / 2, (trailScale[i] - 0.25f), SpriteEffects.None, 0);
                }
                if (trailScale[i] >= 0.55f)
                {
                    Color drawColor = Main.hslToRgb((trailScale[i] - 0.5f) * 0.2f, 1f, 0.5f + (trailScale[i] - 0.5f) * 0.8f) * alphaMult;
                    Main.EntitySpriteDraw(texture, Main.player[Projectile.owner].Center + (trailPositions[i] - Main.player[Projectile.owner].Center).RotatedBy(0.2f) - Main.screenPosition, frame, drawColor, (trailPositions[i + 1] - trailPositions[i]).ToRotation() + MathHelper.PiOver2 + 0.2f, frame.Size() / 2, (trailScale[i] - 0.5f), SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(texture, Main.player[Projectile.owner].Center + (trailPositions[i] - Main.player[Projectile.owner].Center).RotatedBy(-0.2f) - Main.screenPosition, frame, drawColor, (trailPositions[i + 1] - trailPositions[i]).ToRotation() + MathHelper.PiOver2 - 0.2f, frame.Size() / 2, (trailScale[i] - 0.5f), SpriteEffects.None, 0);
                }
            }

            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            DrawLayer.AddProjectile<DrawLayerAdditiveAfterProjectiles>(index);
        }

        public override bool ShouldUpdatePosition() => false;
    }
}