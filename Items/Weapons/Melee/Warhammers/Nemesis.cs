using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Weapons.Melee.Warhammers
{
    public class Nemesis : WarhammerBase, IDrawHeldItem
    {
        public override int HammerLength => 97;
        public override int HammerHeadSize => 31;
        public override int DefenseLoss => 50;
        public override int DebuffTime => 1800;

        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;

            base.SetStaticDefaults();
        }

        public override void SetDefaults()
        {
            Item.SetWeaponValues(360, 20, 0);
            Item.DamageType = DamageClass.Melee;

            Item.width = 128;
            Item.height = 128;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = WarhammerUseStyle;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item1;
            Item.shootSpeed = 24f;

            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Red;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.shoot = ProjectileType<NemesisProjectile>();
                Item.noMelee = true;
                Item.UseSound = SoundID.Item34;
            }
            else
            {
                Item.useStyle = WarhammerUseStyle;
                Item.shoot = 0;
                Item.noMelee = false;
                Item.UseSound = SoundID.Item1;
            }
            return null;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            base.UseStyle(player, heldItemFrame);

            if (player.altFunctionUse == 2)
            {
                player.itemRotation = itemRotation;
                player.direction = itemDirection;
            }
        }

        private static bool hasHitSomething = false;

        public override void UseAnimation(Player player)
        {
            hasHitSomething = false;

            base.UseAnimation(player);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Main.dust[Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Torch, player.velocity.X / 2, player.velocity.Y / 2, 0, Color.Transparent, 2.5f)].noGravity = true;
            Main.dust[Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Torch, player.velocity.X / 2, player.velocity.Y / 2, 0, Color.Transparent, 2.5f)].noGravity = true;
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            if (!hasHitSomething)
            {
                hasHitSomething = true;

                Rectangle hitbox = GetHitbox(player);

                for (int i = 0; i < 10; i++)
                {
                    Main.dust[Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Torch, player.velocity.X / 2, player.velocity.Y / 2, 0, Color.Transparent, 2.5f)].noGravity = true;
                    Main.dust[Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Torch, player.velocity.X / 2, player.velocity.Y / 2, 0, Color.Transparent, 2.5f)].noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.NPCDeath14, player.itemLocation);

                for (int i = -2; i <= 2; i++)
                {
                    Vector2 position = player.Center + new Vector2(i * 250, -1000);
                    Vector2 velocity = (hitbox.Center() - position).SafeNormalize(Vector2.Zero).RotatedByRandom(0.12f) * Item.shootSpeed;
                    Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, velocity, ProjectileType<NemesisProjectile>(), Item.damage / 3, Item.knockBack / 3, player.whoAmI, Main.MouseWorld.Y);
                }
            }

            base.OnHitNPC(player, target, damage, knockBack, crit);
        }

        public override void OnHitTiles(Player player)
        {
            if (!hasHitSomething)
            {
                hasHitSomething = true;

                Rectangle hitbox = GetHitbox(player);

                for (int i = 0; i < 10; i++)
                {
                    Main.dust[Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Torch, player.velocity.X / 2, player.velocity.Y / 2, 0, Color.Transparent, 2.5f)].noGravity = true;
                    Main.dust[Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Torch, player.velocity.X / 2, player.velocity.Y / 2, 0, Color.Transparent, 2.5f)].noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.NPCDeath14, player.itemLocation);

                for (int i = -2; i <= 2; i++)
                {
                    Vector2 position = player.Center + new Vector2(i * 250, -1000);
                    Vector2 velocity = (hitbox.Center() - position).SafeNormalize(Vector2.Zero).RotatedByRandom(0.12f) * Item.shootSpeed;
                    Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, velocity, ProjectileType<NemesisProjectile>(), Item.damage / 3, Item.knockBack / 3, player.whoAmI, Main.MouseWorld.Y);
                }
            }
        }

        private float itemRotation;
        private int itemDirection;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                player.itemRotation = (new Vector2(Main.MouseWorld.X, player.Center.Y - 750) - player.itemLocation).ToRotation() + (player.direction == 1 ? 0 : 1) * MathHelper.Pi;
                itemRotation = player.itemRotation;
                itemDirection = Main.MouseWorld.X > player.Center.X ? 1 : -1;

                damage /= 3;
                knockback /= 3;

                for (int i = -2; i <= 2; i++)
                {
                    position = player.Center + new Vector2(i * 250, -1000);
                    Vector2 speed = (Main.MouseWorld - position).SafeNormalize(Vector2.Zero).RotatedByRandom(0.12f) * (velocity.Length());
                    Projectile.NewProjectile(source, position, speed, type, damage, knockback, player.whoAmI, Main.MouseWorld.Y);
                }
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FragmentSolar, 18)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public bool DoVanillaDraw() => false;

        public void DrawHeldItem(ref PlayerDrawSet drawInfo)
        {
            Texture2D texture = TextureAssets.Item[Type].Value;
            Player drawPlayer = drawInfo.drawPlayer;

            if (drawPlayer.altFunctionUse == 2)
            {
                SpriteEffects spriteEffects = (SpriteEffects)((drawPlayer.gravDir != 1f) ? ((drawPlayer.direction != 1) ? 3 : 2) : ((drawPlayer.direction != 1) ? 1 : 0));

                float num79 = drawPlayer.itemRotation + 0.785f * drawPlayer.direction;
                int num78 = 0;
                int num77 = 0;
                Vector2 origin5 = new Vector2(0f, texture.Height);
                if (drawPlayer.gravDir == -1f)
                {
                    if (drawPlayer.direction == -1)
                    {
                        num79 += 1.57f;
                        origin5 = new Vector2(texture.Width, 0f);
                        num78 -= texture.Width;
                    }
                    else
                    {
                        num79 -= 1.57f;
                        origin5 = Vector2.Zero;
                    }
                }
                else if (drawPlayer.direction == -1)
                {
                    origin5 = new Vector2(texture.Width, texture.Height);
                    num78 -= texture.Width;
                }
                Vector2 holdoutOrigin = Vector2.Zero;
                ItemLoader.HoldoutOrigin(drawPlayer, ref holdoutOrigin);
                DrawData drawData = new DrawData(texture, new Vector2((int)(drawPlayer.itemLocation.X - Main.screenPosition.X + origin5.X + num78), (int)(drawPlayer.itemLocation.Y - Main.screenPosition.Y + num77)), (Rectangle?)new Rectangle(0, 0, texture.Width, texture.Height), Color.White, num79, origin5 + holdoutOrigin, drawPlayer.inventory[drawPlayer.selectedItem].scale, spriteEffects, 0);
                drawInfo.DrawDataCache.Add(drawData);
            }
            else
            {
                SpriteEffects spriteEffects = (SpriteEffects)((drawPlayer.gravDir != 1f) ? ((drawPlayer.direction != 1) ? 3 : 2) : ((drawPlayer.direction != 1) ? 1 : 0));

                if (drawPlayer.gravDir == -1)
                {
                    DrawData drawData = new DrawData(texture, new Vector2((int)(drawPlayer.itemLocation.X - Main.screenPosition.X), (int)(drawPlayer.itemLocation.Y - Main.screenPosition.Y)), (Rectangle?)new Rectangle(0, 0, texture.Width, texture.Height), Color.White, drawPlayer.itemRotation, new Vector2(texture.Width * 0.5f - texture.Width * 0.5f * drawPlayer.direction, 0f), drawPlayer.inventory[drawPlayer.selectedItem].scale, spriteEffects, 0);
                    drawInfo.DrawDataCache.Add(drawData);
                }
                else
                {
                    Vector2 value21 = Vector2.Zero;
                    int type6 = drawPlayer.inventory[drawPlayer.selectedItem].type;
                    DrawData drawData = new DrawData(texture, new Vector2((int)(drawPlayer.itemLocation.X - Main.screenPosition.X), (int)(drawPlayer.itemLocation.Y - Main.screenPosition.Y)), (Rectangle?)new Rectangle(0, 0, texture.Width, texture.Height), Color.White, drawPlayer.itemRotation, new Vector2(texture.Width * 0.5f - texture.Height * 0.5f * drawPlayer.direction, texture.Height) + value21, drawPlayer.inventory[drawPlayer.selectedItem].scale, spriteEffects, 0);
                    drawInfo.DrawDataCache.Add(drawData);
                }
            }
        }
    }

    public class NemesisProjectile : ModProjectile
    {
        public override string Texture => "Polarities/Items/Weapons/Melee/Warhammers/Nemesis";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("{$Mods.Polarities.ItemName.Nemesis}");
        }

        public override void SetDefaults()
        {
            Projectile.width = 62;
            Projectile.height = 62;
            DrawOffsetX = -66;
            DrawOriginOffsetY = 0;
            DrawOriginOffsetX = 33;

            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 3600;
            Projectile.tileCollide = false;
        }
        public override void AI()
        {
            Projectile.tileCollide = Projectile.Center.Y > Projectile.ai[0];

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Projectile.velocity.X / 2, Projectile.velocity.Y / 2, 0, Color.Transparent, 2.5f)].noGravity = true;
            Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Projectile.velocity.X / 2, Projectile.velocity.Y / 2, 0, Color.Transparent, 2.5f)].noGravity = true;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Projectile.velocity.X / 2, Projectile.velocity.Y / 2, 0, Color.Transparent, 2.5f)].noGravity = true;
                Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Projectile.velocity.X / 2, Projectile.velocity.Y / 2, 0, Color.Transparent, 2.5f)].noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.NPCDeath14, Projectile.Center);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Daybreak, 300);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float trailLength = 4f;
            float trailSize = 8;
            for (int i = (int)trailLength - 1; i >= 0; i--)
            {
                Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Projectile.velocity * (i / trailLength) * trailSize - Main.screenPosition, new Rectangle(0, (TextureAssets.Projectile[Type].Height() * Projectile.frame) / Main.projFrames[Projectile.type], TextureAssets.Projectile[Type].Width(), TextureAssets.Projectile[Type].Height() / Main.projFrames[Projectile.type]), Color.White * (1 - i / trailLength), Projectile.rotation, new Vector2(TextureAssets.Projectile[Type].Width() / 2 + DrawOriginOffsetX, Projectile.height / 2), Projectile.scale, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}

