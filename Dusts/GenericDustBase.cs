using System;
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
        }
    }

    public class SaltDust : GenericDustBase { }
    public class LimestoneDust : GenericDustBase { }
    public class ConvectiveDust : GenericDustBase { public override bool Glow => true; }
}

