using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Effects;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Melee
{
    public class UnfoldingBlossom : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
            PolaritiesItem.IsFlawless.Add(Type);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(160, 3, 0);
            Item.DamageType = DamageClass.Melee;

            Item.width = 42;
            Item.height = 40;

            Item.useTime = 1;
            Item.useAnimation = 1;
            Item.noUseGraphic = true;
            Item.useStyle = 5;
            Item.noMelee = true;
            Item.autoReuse = false;
            Item.channel = true;

            Item.shoot = ProjectileType<UnfoldingBlossomProjectile>();
            Item.shootSpeed = 1f;

            Item.value = Item.sellPrice(gold: 7, silver: 50);
            Item.rare = RarityType<PlanteraFlawlessRarity>();
        }

        private int time;
        private float startRotation;

        public override void HoldItem(Player player)
        {
            if (player.channel)
            {
                player.direction = (Main.MouseWorld.X - player.Center.X > 0) ? 1 : -1;

                if (time % 10 == 0)
                {
                    float angleOffsetIncrement = MathHelper.Pi * (3 - (float)Math.Sqrt(5));
                    Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, new Vector2(Item.shootSpeed, 0).RotatedBy(time / 10f * angleOffsetIncrement + startRotation), Item.shoot, Item.damage, Item.knockBack, player.whoAmI);
                }

                player.itemTime = Item.useTime;
                player.itemAnimation = Item.useAnimation;
                time++;
            }
            else
            {
                startRotation = (Main.MouseWorld - player.MountedCenter).ToRotation();
                time = 0;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return false;
        }
    }

    public class UnfoldingBlossomProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.alpha = 0;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = false;

            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;

            Projectile.timeLeft = 900;

            Projectile.hide = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (Projectile.ai[1] == 0)
            {
                Projectile.ai[0]++;
                Projectile.scale = 0.025f * (float)Math.Sqrt(Projectile.ai[0]);

                Projectile.Center = player.Center + Projectile.velocity * 255 * Projectile.scale;
                Projectile.rotation = Projectile.velocity.ToRotation();

                if (!player.channel || (Main.rand.NextBool() && player.GetModPlayer<PolaritiesPlayer>().justHit) || Projectile.timeLeft == 1)
                {
                    Projectile.ai[1] = 1;
                    Projectile.timeLeft = 1800;
                    Projectile.velocity = player.velocity;

                    //this becomes angular momentum
                    Projectile.ai[0] = 0;
                }
            }
            if (Projectile.ai[1] == 1)
            {
                Vector2 direction = new Vector2(1, 0).RotatedBy(Projectile.rotation);

                //gravity
                Projectile.velocity.Y += 0.3f;

                //drag:
                float drag = (float)Math.Abs(Math.Sin(Projectile.velocity.ToRotation() - direction.ToRotation())) / (Projectile.scale * 4);
                float sidewaysForce = (float)Math.Sin(2 * (Projectile.velocity.ToRotation() - direction.ToRotation())) / (Projectile.scale * 4);

                Projectile.velocity -= 0.1f * (Projectile.velocity * drag + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * sidewaysForce);

                //you're probably hitting the front more or somehting which affects angular momentum
                Projectile.ai[0] -= 0.001f * Projectile.velocity.Length() * sidewaysForce;

                //angular momentum drag:
                Projectile.ai[0] -= 0.1f * Projectile.ai[0];

                Projectile.rotation += Projectile.ai[0];
                //update angular momentum
            }
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            damage = (int)(damage * Projectile.scale / 0.75f);
        }

        public override void ModifyHitPvp(Player target, ref int damage, ref bool crit)
        {
            damage = (int)(damage * Projectile.scale / 0.75f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 lineStart = Projectile.Center + new Vector2(1, 0).RotatedBy(Projectile.rotation) * 255 * Projectile.scale;
            Vector2 lineEnd = Projectile.Center - new Vector2(1, 0).RotatedBy(Projectile.rotation) * 255 * Projectile.scale;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), Color.White, Projectile.rotation, new Vector2(texture.Width / 2f, texture.Height / 2f), Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            RenderTargetLayer.AddProjectile<BehindTilesWithLightingTarget>(index);
        }

        public override bool ShouldUpdatePosition()
        {
            return Projectile.ai[1] == 1;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }
    }
}