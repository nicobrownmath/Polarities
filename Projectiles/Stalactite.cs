using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Projectiles
{
    public class Stalactite : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Stalactite";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("{$Mods.Polarities.ProjectileName.SpeleothemStalactite}");

            ProjectileID.Sets.DontAttachHideToAlpha[Projectile.type] = true;
            Main.projFrames[Projectile.type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            DrawOffsetX = -9;
            DrawOriginOffsetY = -38;
            DrawOriginOffsetX = 0;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.spriteDirection = Main.rand.NextBool() ? 1 : -1;
            Projectile.hide = true;
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 0)
            {
                Projectile.frame = Main.rand.Next(3);
                Projectile.ai[0]++;
            }

            Projectile.velocity.Y += 0.2f;
            Projectile.velocity.X = 0;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = true;
            return true;
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Tink, Projectile.Center);
            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
            if (Main.rand.NextBool(200))
            {
                Item.NewItem(Projectile.GetSource_DropAsItem(), Projectile.Hitbox, ItemType<Items.Weapons.Magic.Speleothem>());
            }
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (Main.rand.NextBool())
                target.AddBuff(BuffID.BrokenArmor, 60 * 60);
        }
    }
}