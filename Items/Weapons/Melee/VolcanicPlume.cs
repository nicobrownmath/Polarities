using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Effects;
using Polarities.Items.Placeable.Bars;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Melee
{
    public class VolcanicPlume : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Volcanic Plume");
            Tooltip.SetDefault("Flies to the cursor and produces an explosion of volcanic fireballs");

            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(250, 3, 0);
            Item.DamageType = DamageClass.Melee;

            Item.width = 26;
            Item.height = 48;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = 1;
            Item.noMelee = true;
            Item.UseSound = SoundID.Item74;
            Item.autoReuse = true;
            Item.noUseGraphic = true;

            Item.shoot = ProjectileType<VolcanicPlumeProjectile>();
            Item.shootSpeed = 16f;

            Item.value = 100000;
            Item.rare = 8;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ProjectileType<VolcanicPlumeProjectile>()] <= 0;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<MantellarBar>(), 12)
                .AddIngredient(ItemID.Flamarang)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            velocity *= player.GetTotalAttackSpeed(DamageClass.Melee);
        }
    }

    public class VolcanicPlumeProjectile : ModProjectile
    {
        public override string Texture => "Polarities/Items/Weapons/Melee/VolcanicPlume";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Volcanic Plume");

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            DrawOffsetX = 0;
            DrawOriginOffsetY = -11;
            DrawOriginOffsetX = 0;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 3600;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = Main.MouseWorld.X;
                Projectile.localAI[1] = Main.MouseWorld.Y;

                Projectile.spriteDirection = (Projectile.localAI[0] > Projectile.Center.X) ? 1 : -1;
            }
            if (Projectile.ai[0] == 0)
            {
                float speed = Projectile.velocity.Length();
                Projectile.velocity = new Vector2(Projectile.localAI[0], Projectile.localAI[1]) - Projectile.Center;
                if (Projectile.velocity.Length() > speed)
                {
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * speed;
                }
                else
                {
                    Projectile.ai[0] = 1;
                }
            }
            else if (Projectile.ai[0] == 1)
            {
                Projectile.tileCollide = false;

                Projectile.velocity = Vector2.Zero;

                if (Projectile.ai[1] == 0)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ProjectileType<VolcanicPlumeExplosion>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    SoundEngine.PlaySound(SoundID.NPCDeath14, Projectile.Center);

                    for (int i = 0; i < 12; i++)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(Main.rand.NextFloat(8, 16), 0).RotatedBy(i * MathHelper.TwoPi / 12).RotatedByRandom(MathHelper.PiOver4), ProjectileType<VolcanicPlumeFireball>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
                    }
                }

                Projectile.ai[1]++;
                if (Projectile.ai[1] >= 20)
                {
                    Projectile.ai[1] = 0;
                    Projectile.ai[0] = 2;
                }

                Projectile.rotation += 0.3f * Projectile.spriteDirection;
            }
            else
            {
                Vector2 goalPosition = Main.player[Projectile.owner].Center;
                Projectile.ai[1] += 1f;

                float speed = Projectile.ai[1] * Main.player[Projectile.owner].GetTotalAttackSpeed(DamageClass.Melee);
                Projectile.velocity = goalPosition - Projectile.Center;
                if (Projectile.velocity.Length() > speed)
                {
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * speed;
                }
                else
                {
                    Projectile.Kill();
                }
            }

            Projectile.rotation += 0.5f * Projectile.spriteDirection;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (Projectile.ai[0] == 0)
            {
                Projectile.ai[0] = 1f;
            }
            target.AddBuff(BuffID.OnFire, 1200);
        }

        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            if (Projectile.ai[0] == 0)
            {
                Projectile.ai[0] = 1f;
            }
            target.AddBuff(BuffID.OnFire, 1200);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.ai[0] == 0)
            {
                Projectile.ai[0] = 1f;
            }
            Projectile.velocity = Vector2.Zero;
            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            DrawLayer.AddProjectile<DrawLayerAdditiveAfterProjectiles>(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float drawAlpha = 0.75f;

            Texture2D texture = Textures.Glow256.Value;
            Rectangle frame = texture.Frame();
            Vector2 center = frame.Center();

            Texture2D mainTexture = TextureAssets.Projectile[Type].Value;
            Rectangle mainFrame = mainTexture.Frame();
            Vector2 mainCenter = mainFrame.Size() / 2;

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] != Vector2.Zero)
                {
                    float progress = 1 - i / (float)Projectile.oldPos.Length;
                    Vector2 scale = new Vector2((Projectile.oldPos[i] - Projectile.oldPos[i - 1]).Length() * 0.02f, progress * 0.15f);
                    float rotation = (Projectile.oldPos[i] - Projectile.oldPos[i - 1]).ToRotation();
                    Color color = ModUtils.ConvectiveFlameColor(progress * progress / 2f) * drawAlpha;
                    Main.EntitySpriteDraw(texture, Projectile.oldPos[i] + Projectile.Center - Projectile.position - Main.screenPosition, frame, color, rotation, center, scale, SpriteEffects.None, 0);
                }
            }

            Main.EntitySpriteDraw(mainTexture, Projectile.Center - Main.screenPosition, mainFrame, Color.White * drawAlpha, Projectile.rotation + MathHelper.PiOver2, mainCenter, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }

    public class VolcanicPlumeExplosion : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Glow256";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Volcanic Blast");
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 128;
            Projectile.height = 128;
            Projectile.scale = 0f;
            Projectile.timeLeft = 20;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
            Projectile.DamageType = DamageClass.Ranged;
        }

        public override void AI()
        {
            Vector2 oldCenter = Projectile.Center;

            Projectile.scale = (1 - Projectile.timeLeft / 20f) * 2;
            Projectile.width = (int)(128 * Projectile.scale);
            Projectile.height = (int)(128 * Projectile.scale);

            Projectile.Center = oldCenter;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return CustomCollision.CheckAABBvDisc(targetHitbox, new Circle(Projectile.Center, Projectile.width / 2));
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            DrawLayer.AddProjectile<DrawLayerAdditiveAfterProjectiles>(index);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 1200);
        }

        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 1200);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float progress = Projectile.timeLeft / 20f;

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame();
            Vector2 center = frame.Center();

            for (int i = 1; i <= 4; i++)
            {
                Color color = ModUtils.ConvectiveFlameColor(progress * progress * i / 4f) * (progress * 2f);
                float drawScale = Projectile.width / 128f * i / 4f;
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, color, 0f, center, drawScale, SpriteEffects.None, 0);
            }

            return false;
        }
    }

    public class VolcanicPlumeFireball : ModProjectile
    {
        public override string Texture => "Polarities/Items/Weapons/Ranged/DraconianFireball";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Volcanic Fireball");

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.timeLeft = 120;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
            Projectile.DamageType = DamageClass.Melee;

            Projectile.extraUpdates = 1;
        }

        private const int FADE_TIME = 10;

        public override void AI()
        {
            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.timeLeft == FADE_TIME)
            {
                TryExplode();
            }
        }

        private void TryExplode()
        {
            if (Projectile.localAI[1] == 0)
            {
                Projectile.localAI[1] = 1;

                Projectile.timeLeft = FADE_TIME;
                Projectile.velocity = Vector2.Zero;
                Projectile.damage = 0;
                Projectile.penetrate = -1;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 1200);
            TryExplode();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            TryExplode();

            return false;
        }

        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.OnFire, 1200);
            TryExplode();
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            DrawLayer.AddProjectile<DrawLayerAdditiveAfterProjectiles>(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float drawAlpha = 0.5f;
            if (Projectile.timeLeft < FADE_TIME) drawAlpha = 0.5f * Projectile.timeLeft / FADE_TIME;

            Texture2D texture = Textures.Glow256.Value;
            Rectangle frame = texture.Frame();
            Vector2 center = frame.Center();

            Texture2D mainTexture = TextureAssets.Projectile[Type].Value;
            Rectangle mainFrame = mainTexture.Frame();
            Vector2 mainCenter = new Vector2(18, 18);

            Main.EntitySpriteDraw(mainTexture, Projectile.Center - Main.screenPosition, mainFrame, Color.White * drawAlpha, Projectile.rotation + MathHelper.PiOver2, mainCenter, Projectile.scale, SpriteEffects.None, 0);

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] != Vector2.Zero)
                {
                    float progress = 1 - i / (float)Projectile.oldPos.Length;
                    Vector2 scale = new Vector2(progress * 0.15f, (Projectile.oldPos[i] - Projectile.oldPos[i - 1]).Length() * 0.02f);
                    Color color = ModUtils.ConvectiveFlameColor(progress * progress / 2f) * drawAlpha;
                    Main.EntitySpriteDraw(texture, Projectile.oldPos[i] + Projectile.Center - Projectile.position - Main.screenPosition, frame, color, Projectile.oldRot[i], center, scale, SpriteEffects.None, 0);
                }
            }

            return false;
        }
    }
}