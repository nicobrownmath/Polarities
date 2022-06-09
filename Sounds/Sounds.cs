using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Bestiary;
using Terraria.Localization;

namespace Polarities
{
    public class Sounds : ILoadable
    {
        public static SoundStyle Rattle;

        public static SoundStyle ConvectiveWandererRoar;
        public static SoundStyle ConvectiveWandererFlamethrowerStart;
        public static SoundStyle ConvectiveWandererFlamethrowerLoop;

        public void Load(Mod mod)
        {
            Rattle = new SoundStyle("Polarities/Sounds/Rattle") { Volume = 0.7f, PitchVariance = 0.5f };

            ConvectiveWandererRoar = new SoundStyle("Polarities/Sounds/ConvectiveWandererRoar");
            ConvectiveWandererFlamethrowerStart = new SoundStyle("Polarities/Sounds/ConvectiveWandererFlamethrowerStart");
            ConvectiveWandererFlamethrowerLoop = new SoundStyle("Polarities/Sounds/ConvectiveWandererFlamethrowerLoop") { IsLooped = false };
        }

        public void Unload()
        {
        }
    }
}

