using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Polarities.Dusts
{
    public abstract class GenericDustBase : ModDust
    {
        public virtual bool Glow => false;

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = false;
            dust.noLight = !Glow;

            dust.frame = Main.rand.Next(new Rectangle[]
            {
                new Rectangle(0,0,6,6),
                new Rectangle(0,8,8,8),
                new Rectangle(0,16,8,8),
            });
            dust.position -= dust.frame.Size() / 2;
        }
    }

    public class SaltDust : GenericDustBase { }
    public class LimestoneDust : GenericDustBase { }
    public class SunplateBarDust : GenericDustBase { }
    public class MantellarDust : GenericDustBase { public override bool Glow => true; }
}

