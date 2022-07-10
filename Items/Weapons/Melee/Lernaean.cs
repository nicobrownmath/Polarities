using Polarities.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace Polarities.Items.Weapons.Melee
{
    public class Lernaean : ModItem
    {
        public override void SetStaticDefaults()
        {
            this.SetResearch(1);

            // These are all related to gamepad controls and don't seem to affect anything else
            ItemID.Sets.Yoyo[Item.type] = true;
            ItemID.Sets.GamepadExtraRange[Item.type] = 15;
            ItemID.Sets.GamepadSmartQuickReach[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(35, 4, 0);
            Item.DamageType = DamageClass.Melee;

            Item.useStyle = 5;
            Item.width = 34;
            Item.height = 38;
            Item.useAnimation = 25;
            Item.useTime = 25;
            Item.shootSpeed = 16f;
            Item.UseSound = SoundID.Item1;

            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.shoot = ProjectileType<LernaeanProjectile>();

            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 1);
        }
    }

    public class LernaeanProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("{$Mods.Polarities.ItemName.Lernaean}");

            // The following sets are only applicable to yoyo that use aiStyle 99.
            // YoyosLifeTimeMultiplier is how long in seconds the yoyo will stay out before automatically returning to the player. 
            // Vanilla values range from 3f(Wood) to 16f(Chik), and defaults to -1f. Leaving as -1 will make the time infinite.
            ProjectileID.Sets.YoyosLifeTimeMultiplier[Projectile.type] = 16f;
            // YoyosMaximumRange is the maximum distance the yoyo sleep away from the player. 
            // Vanilla values range from 130f(Wood) to 400f(Terrarian), and defaults to 200f
            ProjectileID.Sets.YoyosMaximumRange[Projectile.type] = 300f;
            // YoyosTopSpeed is top speed of the yoyo Projectile. 
            // Vanilla values range from 9f(Wood) to 17.5f(Terrarian), and defaults to 10f
            ProjectileID.Sets.YoyosTopSpeed[Projectile.type] = 15f;
        }

        public override void SetDefaults()
        {
            Projectile.extraUpdates = 0;
            Projectile.width = 16;
            Projectile.height = 16;
            DrawOffsetX = -1;
            DrawOriginOffsetY = -1;
            DrawOriginOffsetX = 0;

            // aiStyle 99 is used for all yoyos, and is Extremely suggested, as yoyo are extremely difficult without them
            Projectile.aiStyle = 99;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.scale = 1f;
        }
        // notes for aiStyle 99: 
        // localAI[0] is used for timing up to YoyosLifeTimeMultiplier
        // localAI[1] can be used freely by specific types
        // ai[0] and ai[1] usually point towards the x and y world coordinate hover point
        // ai[0] is -1f once YoyosLifeTimeMultiplier is reached, when the player is stoned/frozen, when the yoyo is too far away, or the player is no longer clicking the shoot button.
        // ai[0] being negative makes the yoyo move back towards the player
        // Any AI method can be used for dust, spawning projectiles, etc specific to your yoyo.

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Projectile.NewProjectile(Projectile.GetSource_OnHit(target), Projectile.Center, Projectile.velocity, ProjectileType<LernaeanOrbiter>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.whoAmI);
        }

        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            Projectile.NewProjectile(Projectile.GetSource_OnHit(target), Projectile.Center, Projectile.velocity, ProjectileType<LernaeanOrbiter>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.whoAmI);
        }
    }

    public class LernaeanOrbiter : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.alpha = 0;
            Projectile.timeLeft = 3600;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;

            Projectile.hide = true;
        }

        public override void AI()
        {
            if (Projectile.ai[1] == 0)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    Projectile.ai[1] = Main.rand.NextBool() ? 1 : -1;
                }
                Projectile.netUpdate = true;
            }

            Projectile owner = Main.projectile[(int)Projectile.ai[0]];
            if (!owner.active)
            {
                Projectile.active = false;
            }

            Vector2 goalPosition = owner.Center + 50 * (Projectile.Center - owner.Center).SafeNormalize(Vector2.Zero).RotatedBy(Projectile.ai[1] * MathHelper.Pi / 3);
            Projectile.velocity = (goalPosition - Projectile.Center).SafeNormalize(Vector2.Zero) * 15f;

            Projectile.rotation += Projectile.ai[1] * 0.1f;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return Projectile.timeLeft < (3600 - 60);
        }

        public override bool CanHitPvp(Player target)
        {
            return Projectile.timeLeft < (3600 - 60);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Projectile owner = Main.projectile[(int)Projectile.ai[0]];

            Vector2 vector48 = new Vector2(owner.Center.X, owner.Center.Y - 6);

            vector48.Y += Main.player[Projectile.owner].gfxOffY;
            float num354 = Projectile.Center.X - vector48.X;
            float num361 = Projectile.Center.Y - vector48.Y;
            Math.Sqrt(num354 * num354 + num361 * num361);
            float num369 = (float)Math.Atan2(num361, num354) - 1.57f;
            if (!Projectile.counterweight)
            {
                int num371 = -1;
                if (Projectile.position.X + (float)(Projectile.width / 2) < Main.player[Projectile.owner].position.X + (float)(Main.player[Projectile.owner].width / 2))
                {
                    num371 = 1;
                }
                num371 *= -1;
                Main.player[Projectile.owner].itemRotation = (float)Math.Atan2(num361 * (float)num371, num354 * (float)num371);
            }
            bool flag = true;
            if (num354 == 0f && num361 == 0f)
            {
                flag = false;
            }
            else
            {
                float num373 = (float)Math.Sqrt(num354 * num354 + num361 * num361);
                num373 = 12f / num373;
                num354 *= num373;
                num361 *= num373;
                vector48.X -= num354 * 0.1f;
                vector48.Y -= num361 * 0.1f;
                num354 = Projectile.position.X + (float)Projectile.width * 0.5f - vector48.X;
                num361 = Projectile.position.Y + (float)Projectile.height * 0.5f - vector48.Y;
            }
            while (flag)
            {
                float num377 = 12f;
                float num380 = (float)Math.Sqrt(num354 * num354 + num361 * num361);
                float num381 = num380;
                if (float.IsNaN(num380) || float.IsNaN(num381))
                {
                    flag = false;
                    continue;
                }
                if (num380 < 14f)
                {
                    num377 = num380 - 8f;
                    flag = false;
                }
                num380 = 12f / num380;
                num354 *= num380;
                num361 *= num380;
                vector48.X += num354;
                vector48.Y += num361;
                num354 = Projectile.position.X + (float)Projectile.width * 0.5f - vector48.X;
                num361 = Projectile.position.Y + (float)Projectile.height * 0.1f - vector48.Y;
                if (num381 > 12f)
                {
                    float num385 = 0.3f;
                    float num387 = Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y);
                    if (num387 > 16f)
                    {
                        num387 = 16f;
                    }
                    num387 = 1f - num387 / 16f;
                    num385 *= num387;
                    num387 = num381 / 80f;
                    if (num387 > 1f)
                    {
                        num387 = 1f;
                    }
                    num385 *= num387;
                    if (num385 < 0f)
                    {
                        num385 = 0f;
                    }
                    num385 *= num387;
                    num385 *= 0.5f;
                    if (num361 > 0f)
                    {
                        num361 *= 1f + num385;
                        num354 *= 1f - num385;
                    }
                    else
                    {
                        num387 = Math.Abs(Projectile.velocity.X) / 3f;
                        if (num387 > 1f)
                        {
                            num387 = 1f;
                        }
                        num387 -= 0.5f;
                        num385 *= num387;
                        if (num385 > 0f)
                        {
                            num385 *= 2f;
                        }
                        num361 *= 1f + num385;
                        num354 *= 1f - num385;
                    }
                }
                num369 = (float)Math.Atan2(num361, num354) - 1.57f;
                int stringColor = Main.player[Projectile.owner].stringColor;
                Color oldColor = WorldGen.paintColor(stringColor);
                if (oldColor.R < 75)
                {
                    oldColor.R = 75;
                }
                if (oldColor.G < 75)
                {
                    oldColor.G = 75;
                }
                if (oldColor.B < 75)
                {
                    oldColor.B = 75;
                }
                switch (stringColor)
                {
                    case 13:
                        oldColor = new Color(20, 20, 20);
                        break;
                    case 0:
                    case 14:
                        oldColor = new Color(200, 200, 200);
                        break;
                    case 28:
                        oldColor = new Color(163, 116, 91);
                        break;
                    case 27:
                        oldColor = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB);
                        break;
                }
                oldColor.A = (byte)((float)(int)oldColor.A * 0.4f);
                float num409 = 0.5f;
                oldColor = Lighting.GetColor((int)vector48.X / 16, (int)(vector48.Y / 16f), oldColor);
                oldColor = new Color((int)(byte)((float)(int)oldColor.R * num409), (int)(byte)((float)(int)oldColor.G * num409), (int)(byte)((float)(int)oldColor.B * num409), (int)(byte)((float)(int)oldColor.A * num409));
                Main.EntitySpriteDraw(TextureAssets.FishingLine.Value, new Vector2(vector48.X - Main.screenPosition.X + (float)TextureAssets.FishingLine.Value.Width * 0.5f, vector48.Y - Main.screenPosition.Y + (float)TextureAssets.FishingLine.Value.Height * 0.5f) - new Vector2(6f, 0f), (Rectangle?)new Rectangle(0, 0, TextureAssets.FishingLine.Value.Width, (int)num377), oldColor, num369, new Vector2((float)TextureAssets.FishingLine.Value.Width * 0.5f, 0f), 1f, (SpriteEffects)0, 0);
            }

            return true;
        }
    }
}
