using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Polarities.Dusts
{
    public class AequoreanBubble : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = true;
            dust.velocity *= 1.5f;

            dust.frame = Main.rand.Next(new Rectangle[]
            {
                new Rectangle(0,0,10,10),
                new Rectangle(0,20,14,14),
                new Rectangle(0,40,20,20),
            });
            dust.position -= dust.frame.Size() / 2;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return Color.White;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation += dust.velocity.X * 0.1f;
            dust.velocity *= 0.95f;
            dust.velocity.Y -= 0.1f;
            dust.scale -= 0.01f;
            if (dust.scale < 0.25f)
            {
                dust.active = false;
            }
            return false;
        }
    }
}