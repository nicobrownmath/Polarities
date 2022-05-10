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
using Terraria.Localization;

namespace Polarities.Items
{
    public abstract class ColorLerpCycleRarity : ModRarity
    {
        public abstract Color[] ColorCycle { get; }
        public virtual float LengthInSeconds => 3f;

        public override Color RarityColor => ModUtils.ColorLerpCycle(Main.GlobalTimeWrappedHourly, LengthInSeconds, ColorCycle);

        public override int GetPrefixedRarity(int offset, float valueMult)
        {
            return Type;
        }
    }

    public class KingSlimeFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { Color.Gold, Color.RoyalBlue }; }
    public class EyeOfCthulhuFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { Color.Blue, Color.White }; }
    public class StarConstructFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { Color.Blue, Color.Orange }; }
    public class GigabatFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { new Color(108, 94, 81), new Color(78, 70, 88) }; }
    public class QueenBeeFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { Color.Yellow, Color.Black }; }
    public class WallOfFleshFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { new Color(179, 68, 63), new Color(179, 99, 125) }; }
    public class SunPixieFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { Color.Yellow, Color.HotPink }; }
    public class EsophageFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { Color.Red, Color.Purple }; }
    public class EmpressFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { Color.Magenta, Color.White, Color.Yellow, Color.White, Color.Cyan, Color.White }; }

    //an overly complex random walk-based rarity that doesn't actually look that good in this instance
    //could be good to try things like this for other rarities though
    /*public class EmpressFlawlessRarity : ModRarity
    {
        static float lastUpdateTime = 0;
        static float rarityWalkVal = 0f;

        public override Color RarityColor
        {
            get {
                float currentTime = (float)Main.GlobalTimeWrappedHourly * 60f;
                float elapsedTime = currentTime - lastUpdateTime;
                lastUpdateTime = currentTime;

                rarityWalkVal += Main.rand.NextNormallyDistributedFloat(elapsedTime / 360f);
                rarityWalkVal = (rarityWalkVal % 1 + 1) % 1;
                return ModUtils.ColorLerpCycle(rarityWalkVal, 1f, Color.Magenta, Color.Yellow, Color.Cyan);
            }
        }

        public override int GetPrefixedRarity(int offset, float valueMult)
        {
            return Type;
        }
    }*/
}

