using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Effects;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Ranged
{
    public class PainBow : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(14, 3.5f, 0);
            Item.DamageType = DamageClass.Ranged;
            Item.useAmmo = AmmoID.Arrow;
            Item.noMelee = true;

            Item.width = 16;
            Item.height = 40;

            Item.useTime = 40;
            Item.useAnimation = 39;
            Item.useStyle = 5;
            Item.autoReuse = true;

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.LightPurple;

            Item.UseSound = SoundID.Item5;
            Item.shoot = 10;
            Item.shootSpeed = 6f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            type = ProjectileType<PainBowProjectile>();
            for (int i = 0; i < 6; i++)
            {
                float ai0 = (Main.MouseWorld.X > position.X) ? i : 5 - i;

                Projectile.NewProjectile(source, position, velocity.RotatedBy((i - 2.5f) * 0.02f), type, damage, knockback, player.whoAmI, ai0: ai0);
            }
            return false;
        }
    }

    public class PainBowProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rainbow Arrow");

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        private Color color
        {
            get
            {
                switch (Projectile.ai[0])
                {
                    case 0:
                        return Color.Red;
                    case 1:
                        return Color.OrangeRed;
                    case 2:
                        return Color.Yellow;
                    case 3:
                        return Color.Green;
                    case 4:
                        return Color.Blue;
                    case 5:
                        return Color.Purple;
                }
                //this should never happen but whatever
                return Color.White;
            }
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.arrow = true;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;

            Projectile.extraUpdates = 1;

            //uses local npc hit cooldown
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (Projectile.timeLeft > Projectile.oldPos.Length)
                Lighting.AddLight(Projectile.Center, color.ToVector3());

            Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2;

            Projectile.velocity.Y += 0.025f;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = Projectile.oldPos.Length;
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame();
            Vector2 drawCenter = new Vector2(7, 7);

            float alphaMult = DrawLayer.IsActive<DrawLayerAdditiveAfterProjectiles>() ? 0.75f : 0.25f;

            Color drawColor = color * alphaMult;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos.Length - 1 - i < Projectile.timeLeft)
                {
                    float alpha = 1 - i / (float)Projectile.oldPos.Length;
                    Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Projectile.position + Projectile.Center - Main.screenPosition, frame, new Color(Vector3.One - (Vector3.One - drawColor.ToVector3()) * alpha) * alpha * 0.5f, Projectile.oldRot[i], drawCenter, new Vector2(alpha, 1) * Projectile.scale, SpriteEffects.None, 0);
                }
            }

            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            DrawLayer.AddProjectile<DrawLayerAdditiveAfterProjectiles>(index);
        }
    }
}