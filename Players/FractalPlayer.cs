using Polarities.Buffs;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace Polarities // Yep, shut up!
{
    public partial class PolaritiesPlayer : ModPlayer
    {
        public bool hasBeenInFractalDimension;

        public int fractalization;

        public int fractalSubworldDebuffRate;
        public int fractalSubworldDebuffResistance;
        public float fractalSubworldDebuffLifeLossResistance = 1f;
        public int oldFractalSubworldDebuffResistance;
        public float fractalLifeMaxMultiplier = 1f;
        public bool suddenFractalizationChange;
        public int originalStatLifeMax2;
        public bool noFractalAntidote;

        public bool fractalDimensionRespawn;

        public void UpdateFractalizationTimer()
        {
            if (FractalSubworld.Active)
            {
                hasBeenInFractalDimension = true;
                fractalization += fractalSubworldDebuffRate;
            }
            else if (fractalization > 0)
            {
                fractalization--;
            }

            if (fractalization > 0)
            {
                int fractalizing = Player.FindBuffIndex(ModContent.BuffType<Fractalizing>());
                if (fractalizing == -1)
                {
                    Player.AddBuff(ModContent.BuffType<Fractalizing>(), fractalization, quiet: true);
                }
                else
                {
                    Player.buffTime[fractalizing] = fractalization;
                }
            }
            else
            {
                Player.ClearBuff(ModContent.BuffType<Fractalizing>());
            }
            fractalSubworldDebuffRate = (int)Math.Min(CreativePowerManager.Instance.GetPower<CreativePowers.ModifyTimeRate>().TargetTimeRate, 24.0);
            if (CreativePowerManager.Instance.GetPower<CreativePowers.FreezeTime>().Enabled)
            {
                fractalSubworldDebuffRate = 0;
            }
        }

        public void UpdateFractalHP()
        {
            int fractalization = GetFractalization();
            if (fractalization > 0)
            {
                float fractalizationKillTime = 18000 * fractalSubworldDebuffLifeLossResistance;

                float goalLifeMaxMultiplier = Math.Min(1, 1f - (fractalization - fractalSubworldDebuffResistance) / fractalizationKillTime);

                float maxMultiplierChange = 0.0025f;
                if (fractalLifeMaxMultiplier > goalLifeMaxMultiplier + maxMultiplierChange && !suddenFractalizationChange)
                {
                    fractalLifeMaxMultiplier -= maxMultiplierChange;
                }
                else
                {
                    fractalLifeMaxMultiplier = goalLifeMaxMultiplier;
                }

                Player.statLifeMax2 = Math.Max(1, (int)Math.Ceiling(Player.statLifeMax2 * fractalLifeMaxMultiplier));

                if (fractalLifeMaxMultiplier <= 0)
                {
                    Player.KillMe(PlayerDeathReason.ByCustomReason(Player.name + "'s physics broke."), 1.0, 0, false);
                }
            }
            else
            {
                fractalLifeMaxMultiplier = 1f;
            }
            oldFractalSubworldDebuffResistance = fractalSubworldDebuffResistance;
            fractalSubworldDebuffResistance = 0;
            fractalSubworldDebuffLifeLossResistance = 1f;
        }
    }
}