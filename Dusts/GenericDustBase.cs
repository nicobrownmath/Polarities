using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Polarities.Dusts
{
    public abstract class GenericDustBase : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = false;
            dust.noLight = true;
        }
    }

    public class SaltDust : GenericDustBase { }
    public class LimestoneDust : GenericDustBase { }
}

