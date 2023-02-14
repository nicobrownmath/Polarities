using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

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
    public class StormCloudfishFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { Color.LightSkyBlue, Color.DarkSlateGray }; }
    public class EyeOfCthulhuFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { Color.Blue, Color.White }; }
    public class EaterOfWorldsFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { new Color(94, 64, 124), new Color(135, 201, 58) }; }
    public class BrainOfCthulhuFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { new Color(208, 64, 65), new Color(255, 236, 96) }; }
    public class StarConstructFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { Color.Blue, Color.Orange }; }
    public class GigabatFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { new Color(108, 94, 81), new Color(78, 70, 88) }; }
    public class QueenBeeFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { Color.Yellow, Color.Black }; }
    public class SkeletronFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { new Color(41, 34, 27), new Color(180, 179, 143) }; }
    public class WallOfFleshFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { new Color(179, 68, 63), new Color(179, 99, 125) }; }
    public class MechBossFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { new Color(166, 166, 166), new Color(245, 122, 122) }; }
    public class SunPixieFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { Color.Yellow, Color.HotPink }; }
    public class EsophageFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { Color.Red, Color.Purple }; }
    public class PlanteraFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { Color.Lime, Color.HotPink }; }
    public class BetsyFlawlessRarity : ColorLerpCycleRarity { public override Color[] ColorCycle => new Color[] { new Color(188, 62, 68), new Color(137, 85, 169), new Color(255, 254, 182) }; }
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

