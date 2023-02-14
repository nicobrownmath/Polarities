using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Pets
{
    public class StormCloudfishPetItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.DefaultToVanitypet(ProjectileType<StormCloudfishPet>(), BuffType<StormCloudfishPetBuff>());

            Item.width = 26;
            Item.height = 20;

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Master;
            Item.master = true;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(Item.buffType, 3600, true);
            }
        }
    }

    public class StormCloudfishPetBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.vanityPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.buffTime[buffIndex] = 18000;
            int projType = ModContent.ProjectileType<StormCloudfishPet>();
            if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0)
            {
                Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.Center, Vector2.Zero, projType, 0, 0f, player.whoAmI);
            }
        }
    }

    public class StormCloudfishPet : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 8;
            Main.projPet[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            DrawOffsetX = -22;
            DrawOriginOffsetY = -16;
            DrawOriginOffsetX = 8;

            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active)
            {
                Projectile.active = false;
                return;
            }
            if (player.dead)
            {
                player.ClearBuff(BuffType<StormCloudfishPetBuff>());
            }
            if (player.HasBuff(BuffType<StormCloudfishPetBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            Projectile.ai[0]++;
            Vector2 goalPosition = player.MountedCenter + new Vector2(-player.direction * 64 + Projectile.spriteDirection * 64 + Projectile.spriteDirection * 8 * (float)Math.Cos(0.1f * Projectile.ai[0]), -64 + 8 * (float)Math.Sin(0.1f * Projectile.ai[0]));
            Vector2 goalVelocity = player.velocity / 2f + (goalPosition - Projectile.Center) / 16f;
            Projectile.velocity += (goalVelocity - Projectile.velocity) / 24f;

            if (Projectile.Distance(player.MountedCenter) > 1000)
            {
                Projectile.Center = player.MountedCenter;
                Projectile.velocity = player.velocity;
            }

            Projectile.spriteDirection = Projectile.velocity.X + player.direction * 0.25f > 0 ? 1 : -1;

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            return false;
        }

        public static Asset<Texture2D> MaskTexture;

        public override void Load()
        {
            MaskTexture = Request<Texture2D>(Texture + "_Mask");
        }

        public override void Unload()
        {
            MaskTexture = null;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D altTexture = MaskTexture.Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 center = frame.Size() / 2;
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, center, Projectile.scale, spriteEffects, 0);

            if (Math.Abs(Projectile.velocity.X) > 5.5f)
                Main.EntitySpriteDraw(altTexture, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, center, Projectile.scale, spriteEffects, 0);

            return false;
        }
    }
}

