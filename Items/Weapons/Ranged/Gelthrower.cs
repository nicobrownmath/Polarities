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
using Terraria.GameContent.Creative;
using Terraria.GameContent;

namespace Polarities.Items.Weapons.Ranged
{
    public class Gelthrower : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(15, 5, 0);
            Item.DamageType = DamageClass.Ranged;

            Item.width = 42;
            Item.height = 26;

            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item61;
            Item.autoReuse = true;

            Item.shoot = 10;
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Gel;

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = RarityType<KingSlimeFlawlessRarity>();
            Item.GetGlobalItem<PolaritiesItem>().flawless = true;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return Main.rand.NextBool(3);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            type = ProjectileType<GelthrowerProjectile>();
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
    }

    public class GelthrowerProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Gel;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;

            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 2;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation += Projectile.velocity.X / 40f;
            Projectile.velocity.Y += 0.15f;

            Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.t_Slime, 0, 0, 64, newColor: new Color(0.5f, 0.5f, 1f), Scale: 1.5f)].noGravity = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Color color = new Color(lightColor.ToVector3() * Lighting.GetColor(Projectile.Center.ToTileCoordinates()).ToVector3() * new Vector3(0.5f, 0.5f, 1f));
            if (Projectile.spriteDirection == -1)
            {
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, texture.Frame(), color, Projectile.rotation, texture.Frame().Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
            }
            else
            {
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, texture.Frame(), color, Projectile.rotation, texture.Frame().Size() / 2, Projectile.scale, SpriteEffects.FlipHorizontally, 0f);
            }
            return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Slimed, 600);
        }

        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Slimed, 600);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            for (int i = 0; i < 5; i++)
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.t_Slime, 0, 0, 64, newColor: new Color(0.5f, 0.5f, 1f), Scale: 1.5f)].noGravity = true;
            }

            if (Projectile.velocity.X != oldVelocity.X || Main.rand.NextBool())
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Projectile.velocity.Y != oldVelocity.Y || Main.rand.NextBool())
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            Projectile.velocity = Projectile.velocity.RotatedByRandom(MathHelper.PiOver4);
            return false;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.t_Slime, 0, 0, 64, newColor: new Color(0.5f, 0.5f, 1f), Scale: 1.5f)].noGravity = true;
            }
        }
    }
}