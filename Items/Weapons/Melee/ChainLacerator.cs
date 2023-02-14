using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Items.Materials;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Melee
{
    public class ChainLacerator : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(48, 3f, 0);
            Item.DamageType = DamageClass.Melee;

            Item.width = 42;
            Item.height = 42;

            Item.useTime = 8;
            Item.useAnimation = 8;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.LightPurple;

            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.channel = false;
            Item.shoot = ProjectileType<ChainLaceratorProjectile>();
            Item.shootSpeed = 12f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity.RotatedBy(0.8f).RotatedByRandom(0.8f), type, damage, knockback, player.whoAmI, ai0: 1f);
            Projectile.NewProjectile(source, position, velocity.RotatedBy(-0.8f).RotatedByRandom(0.8f), type, damage, knockback, player.whoAmI, ai0: -1f);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ChainGuillotines)
                .AddIngredient(ItemID.FetidBaghnakhs)
                .AddIngredient(ItemType<EvilDNA>())
                .AddTile(TileID.DemonAltar)
                .Register();
        }
    }

    public class ChainLaceratorProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 3600;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = (int)(Projectile.ai[0] + 1) / 2;
            Projectile.spriteDirection = (int)Projectile.ai[0];

            Projectile.ai[0] = 0;
        }

        public override void AI()
        {
            if (Main.player[Projectile.owner].dead)
            {
                Projectile.Kill();
                return;
            }
            if (Projectile.alpha == 0)
            {
                if (Projectile.position.X + Projectile.width / 2 > Main.player[Projectile.owner].position.X + Main.player[Projectile.owner].width / 2)
                {
                    Main.player[Projectile.owner].ChangeDir(1);
                }
                else
                {
                    Main.player[Projectile.owner].ChangeDir(-1);
                }
            }
            Vector2 vector140 = Projectile.Center;
            float num2390 = Main.player[Projectile.owner].position.X + Main.player[Projectile.owner].width / 2 - vector140.X;
            float num2389 = Main.player[Projectile.owner].position.Y + Main.player[Projectile.owner].height / 2 - vector140.Y;
            float num2388 = (float)Math.Sqrt(num2390 * num2390 + num2389 * num2389);
            if (Projectile.ai[0] == 0f)
            {
                if (Projectile.timeLeft < 3600 - 30)
                {
                    Projectile.ai[0] = 1f;
                }
                Projectile.rotation = Utils.AngleLerp(Projectile.rotation, (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + 1.57f, 0.25f);
                Projectile.ai[1] += 1f;
                if (Projectile.ai[1] > 5f)
                {
                    Projectile.alpha = 0;
                }
                if (Projectile.ai[1] > 8f)
                {
                    Projectile.ai[1] = 8f;
                }

                Projectile.velocity = Projectile.velocity.RotatedBy(-Projectile.spriteDirection * 0.075f);
            }
            else if (Projectile.ai[0] == 1f)
            {
                Projectile.tileCollide = false;
                Projectile.rotation = Utils.AngleLerp(Projectile.rotation, (float)Math.Atan2(num2389, num2390) - 1.57f, 0.25f);
                float num2387 = 20f;
                if (num2388 < 20f)
                {
                    Projectile.Kill();
                }
                num2388 = num2387 / num2388;
                num2390 *= num2388;
                num2389 *= num2388;
                Projectile.velocity.X = num2390;
                Projectile.velocity.Y = num2389;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            Projectile.ai[0] = 1;
            return false;
        }

        public static Asset<Texture2D> ChainTexture;

        public override void Load()
        {
            ChainTexture = Request<Texture2D>(Texture + "_Chain");
        }

        public override void Unload()
        {
            ChainTexture = null;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 startPoint = Projectile.Center;
            Vector2 endPoint = Main.player[Projectile.owner].Center;

            Vector2 midPoint = (startPoint + endPoint) / 2;

            midPoint += (startPoint - endPoint).RotatedBy(MathHelper.PiOver2) * (float)Math.Sin((startPoint - endPoint).ToRotation() - Projectile.rotation + MathHelper.PiOver2);

            Vector2[] bezierPoints = { endPoint, midPoint, startPoint };
            float bezierProgress = 0;
            float bezierIncrement = 10;

            Texture2D chainTexture = ChainTexture.Value;
            Rectangle textureFrame = ChainTexture.Frame();
            Vector2 textureCenter = ChainTexture.Size() / 2;

            while (bezierProgress < 1)
            {
                //draw stuff
                Vector2 oldPos = ModUtils.BezierCurve(bezierPoints, bezierProgress);

                //increment progress
                while ((oldPos - ModUtils.BezierCurve(bezierPoints, bezierProgress)).Length() < bezierIncrement)
                {
                    bezierProgress += 0.1f / ModUtils.BezierCurveDerivative(bezierPoints, bezierProgress).Length();
                }

                Vector2 newPos = ModUtils.BezierCurve(bezierPoints, bezierProgress);
                float rotation = (newPos - oldPos).ToRotation();

                Vector2 drawingPos = (oldPos + newPos) / 2;

                Color drawColor = Lighting.GetColor(drawingPos.ToTileCoordinates());

                Main.EntitySpriteDraw(chainTexture, drawingPos - Main.screenPosition, new Rectangle(0, 0, 10, 10), drawColor, rotation, textureCenter, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }
    }
}