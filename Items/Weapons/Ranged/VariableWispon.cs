using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Polarities.Projectiles;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Polarities.Effects;
using Terraria.Audio;
using Terraria.GameContent;
using ReLogic.Content;

namespace Polarities.Items.Weapons.Ranged
{
    public class VariableWispon : ModItem, IDrawHeldItem
    {
        public override void SetStaticDefaults()
        {
            this.SetResearch(1);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(67, 3f);
            Item.DamageType = DamageClass.Ranged;

            Item.width = 62;
            Item.height = 22;

            Item.useTime = 24;
            Item.useAnimation = 24;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Cyan;

            Item.UseSound = SoundID.Item33;
            Item.autoReuse = true;

            Item.useAmmo = AmmoID.Bullet;
            Item.shoot = 10;
            Item.shootSpeed = 8f;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.UseSound = SoundID.Item15;

                if (Item.useAmmo == AmmoID.Bullet)
                {
                    Item.useAmmo = AmmoID.Dart;
                    Item.useTime = 28;
                    Item.useAnimation = 28;
                }
                else if (Item.useAmmo == AmmoID.Dart)
                {
                    Item.useAmmo = AmmoID.Rocket;
                    Item.useTime = 36;
                    Item.useAnimation = 36;
                }
                else if (Item.useAmmo == AmmoID.Rocket)
                {
                    Item.useAmmo = AmmoID.Bullet;
                    Item.useTime = 24;
                    Item.useAnimation = 24;
                }
                return true;
            }

            Item.UseSound = SoundID.Item33;
            return true;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return player.altFunctionUse != 2;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (Item.useAmmo == AmmoID.Bullet)
            {
                type = ProjectileType<VariableWisponLaser>();
            }
            else if (Item.useAmmo == AmmoID.Dart)
            {
                type = ProjectileType<VariableWisponSawblade>();
                velocity *= 1.25f;
            }
            else if (Item.useAmmo == AmmoID.Rocket)
            {
                type = ProjectileType<VariableWisponRocket>();
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return player.altFunctionUse != 2;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(0, 0);
        }

        public static Asset<Texture2D> MaskTexture;
        public static Asset<Texture2D>[] MaskTextures;

        public override void Load()
        {
            MaskTexture = Request<Texture2D>(Texture + "_Mask");

            MaskTextures = new Asset<Texture2D>[6]; //in retrospect I should have made this one thing with frames but whatever
            for (int i = 1; i <= 6; i++)
            {
                MaskTextures[i - 1] = Request<Texture2D>(Texture + "_Mask" + i);
            }
        }

        public override void Unload()
        {
            MaskTexture = null;
            MaskTextures = null;
        }

        public void DrawHeldItem(ref PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;

            Texture2D texture = TextureAssets.Item[Type].Value;
            SpriteEffects spriteEffects = (SpriteEffects)((drawPlayer.gravDir != 1f) ? ((drawPlayer.direction != 1) ? 3 : 2) : ((drawPlayer.direction != 1) ? 1 : 0));

            Color drawColor = Color.White;
            if (drawPlayer.HeldItem.useAmmo == AmmoID.Bullet)
            {
                drawColor = new Color(128, 255, 255);
            }
            if (drawPlayer.HeldItem.useAmmo == AmmoID.Dart)
            {
                drawColor = new Color(255, 161, 209);
            }
            else if (drawPlayer.HeldItem.useAmmo == AmmoID.Rocket)
            {
                drawColor = new Color(255, 198, 98);
            }
            Color currentColor = Lighting.GetColor((int)((double)drawPlayer.position.X + (double)drawPlayer.width * 0.5) / 16, (int)(((double)drawPlayer.position.Y + (double)drawPlayer.height * 0.5) / 16.0), drawColor);
            Vector2 vector12 = new Vector2((float)(texture.Width / 2), (float)(texture.Height / 2));
            Vector2 vector10 = (Vector2)HoldoutOffset();
            vector10.Y *= drawPlayer.gravDir;
            int num76 = (int)vector10.X;
            vector12.Y += vector10.Y;
            texture = MaskTexture.Value;
            Vector2 origin6 = new Vector2((float)(-num76), (float)(texture.Height / 2));
            if (drawPlayer.direction == -1)
            {
                origin6 = new Vector2((float)(texture.Width + num76), (float)(texture.Height / 2));
            }
            DrawData drawData = new DrawData(texture, new Vector2((float)(int)(drawPlayer.itemLocation.X - Main.screenPosition.X + vector12.X), (float)(int)(drawPlayer.itemLocation.Y - Main.screenPosition.Y + vector12.Y)), (Rectangle?)new Rectangle(0, 0, texture.Width, texture.Height), drawPlayer.inventory[drawPlayer.selectedItem].GetAlpha(currentColor), drawPlayer.itemRotation, origin6, drawPlayer.inventory[drawPlayer.selectedItem].scale, spriteEffects, 0);
            drawInfo.DrawDataCache.Add(drawData);
            for (int i = 1; i <= 6; i++)
            {
                float alpha = i / 6f;
                texture = MaskTextures[i - 1].Value;
                drawData = new DrawData(texture, new Vector2((float)(int)(drawPlayer.itemLocation.X - Main.screenPosition.X + vector12.X), (float)(int)(drawPlayer.itemLocation.Y - Main.screenPosition.Y + vector12.Y)), (Rectangle?)new Rectangle(0, 0, texture.Width, texture.Height), drawColor * alpha, drawPlayer.itemRotation, origin6, drawPlayer.inventory[drawPlayer.selectedItem].scale, spriteEffects, 0);
                drawInfo.DrawDataCache.Add(drawData);
            }
        }
    }

    public class VariableWisponLaser : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            DrawOffsetX = -16;
            DrawOriginOffsetY = 0;
            DrawOriginOffsetX = 8;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 6;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.light = 1f;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                for (int k = 0; k < Projectile.oldPos.Length; k++)
                {
                    Projectile.oldPos[k] = Projectile.position;
                }
            }
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Projectile.penetrate == 1)
            {
                Projectile.ai[1] = 3;
            }
            if (Projectile.ai[1] > 2)
            {
                Projectile.friendly = false;
                Projectile.alpha += 8;
                if (Projectile.alpha > 255)
                {
                    Projectile.Kill();
                    return;
                }
            }

            //oldPos trail diffusion thing
            for (int k = 1; k < Projectile.oldPos.Length - 1; k++)
            {
                Projectile.oldPos[k] = (Projectile.oldPos[k - 1] + Projectile.oldPos[k + 1]) / 2f;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (oldVelocity.X != Projectile.velocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (oldVelocity.Y != Projectile.velocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            Projectile.ai[1]++;

            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Color mainColor = Color.White * ((255f - Projectile.alpha) / 255f);

            for (int k = 1; k < Projectile.oldPos.Length; k++)
            {
                Color color = mainColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                float scale = Projectile.scale;

                float rotation = (Projectile.oldPos[k - 1] - Projectile.oldPos[k]).ToRotation();

                Main.EntitySpriteDraw(texture, Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), color, rotation, new Vector2(21, 5), scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), mainColor, Projectile.rotation, new Vector2(21, 5), Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            return false;
        }
    }

    public class VariableWisponSawblade : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;

            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.light = 1f;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;

                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
                for (int k = 0; k < Projectile.oldPos.Length; k++)
                {
                    Projectile.oldPos[k] = Projectile.position;
                }
            }

            if (Main.rand.NextBool(2))
            {
                Vector2 velocity = new Vector2(5, 0).RotatedByRandom(MathHelper.TwoPi);
                Main.dust[Dust.NewDust(Projectile.Center - new Vector2(0.5f, 0.5f), 1, 1, DustID.FireworkFountain_Pink, SpeedX: velocity.X, SpeedY: velocity.Y, Scale: 0.4f)].noGravity = true;
            }

            Projectile.rotation += Projectile.spriteDirection * 0.4f;

            if (Projectile.ai[0] == 0)
                Projectile.velocity.Y += 0.2f;

            if (Projectile.timeLeft < 32)
            {
                Projectile.friendly = false;
                Projectile.alpha += 8;
                if (Projectile.alpha > 255)
                {
                    Projectile.Kill();
                    return;
                }
            }

            //oldPos trail diffusion thing
            for (int k = 1; k < Projectile.oldPos.Length - 1; k++)
            {
                Projectile.oldPos[k] = (Projectile.oldPos[k - 1] + Projectile.oldPos[k + 1]) / 2f;
            }
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = 2;
            height = 2;

            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity = Vector2.Zero;
            Projectile.ai[0] = 1;

            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Color mainColor = Color.White * ((255f - Projectile.alpha) / 255f);

            for (int k = 1; k < Projectile.oldPos.Length; k++)
            {
                Color color = mainColor * 0.75f * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                float scale = Projectile.scale * ((float)(Projectile.oldPos.Length * 2 - k) / (float)(Projectile.oldPos.Length * 2));

                float rotationMultiplier = Projectile.ai[0] == 1 ? 0 : 0.4f;
                float rotation = Projectile.rotation - Projectile.spriteDirection * k * rotationMultiplier;

                Main.EntitySpriteDraw(texture, Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), color, rotation, new Vector2(16, 16), scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), mainColor * 0.75f, Projectile.rotation, new Vector2(16, 16), Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            return false;
        }
    }

    public class VariableWisponRocket : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;

            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.light = 1f;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;

                for (int k = 0; k < Projectile.oldPos.Length; k++)
                {
                    Projectile.oldPos[k] = Projectile.position;
                }
            }

            if (Projectile.timeLeft == 1)
            {
                Projectile.width = 100;
                Projectile.height = 100;
                Projectile.position -= new Vector2((100 - 32) / 2, (100 - 32) / 2);
                Projectile.hide = true;
                return;
            }

            int targetID = Projectile.FindTargetWithLineOfSight(400f);
            if (targetID != -1)
            {
                NPC target = Main.npc[targetID];

                Projectile.velocity = (Projectile.velocity + (target.Center - Projectile.Center) / 1024f).SafeNormalize(Vector2.Zero) * Projectile.velocity.Length();
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Projectile.timeLeft < 40)
            {
                Projectile.friendly = false;
                Projectile.alpha += 8;
                if (Projectile.alpha > 255)
                {
                    Projectile.Kill();
                    return;
                }
            }

            //oldPos trail diffusion thing
            for (int k = 1; k < Projectile.oldPos.Length - 1; k++)
            {
                Projectile.oldPos[k] = (Projectile.oldPos[k - 1] + Projectile.oldPos[k + 1]) / 2f;
            }
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = 2;
            height = 2;

            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.timeLeft > 1)
            {
                Projectile.timeLeft = 2;
                Projectile.tileCollide = false;
            }
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath14, Projectile.Center);
            for (int i = 0; i < 80; i++)
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Pixie, newColor: new Color(255, 240, 210), Scale: 2f)].noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.timeLeft > 1)
            {
                target.immune[Projectile.owner] = 0;
                Projectile.timeLeft = 2;
                Projectile.tileCollide = false;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Color mainColor = Color.White * ((255f - Projectile.alpha) / 255f);

            for (int k = 1; k < Projectile.oldPos.Length; k++)
            {
                Color color = mainColor * 0.75f * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                float scale = Projectile.scale * ((float)(Projectile.oldPos.Length * 2 - k) / (float)(Projectile.oldPos.Length * 2));

                float rotation = (Projectile.oldPos[k - 1] - Projectile.oldPos[k]).ToRotation();

                Main.EntitySpriteDraw(texture, Projectile.Center - Projectile.position + Projectile.oldPos[k] - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), color, rotation, new Vector2(16, 16), scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * texture.Height / Main.projFrames[Projectile.type], texture.Width, texture.Height / Main.projFrames[Projectile.type]), mainColor * 0.75f, Projectile.rotation, new Vector2(16, 16), Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            return false;
        }
    }
}