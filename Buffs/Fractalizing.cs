using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Polarities.Buffs
{
    public class Fractalizing : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void ModifyBuffTip(ref string tip, ref int rare)
        {
            if (!FractalSubworld.Active)
            {
                return;
            }
            var player = Main.LocalPlayer;
            int time = player.GetFractalization();
            if (time > FractalSubworld.HARDMODE_DANGER_TIME)
            {
                tip += $"\n{Language.GetTextValue("Mods.Polarities.BuffDescription.Fractalizing.PostDendriticEnergy")}";
            }
            else
            {
                tip += $"\n{Language.GetTextValue("Mods.Polarities.BuffDescription.Fractalizing.PreDendriticEnergy")}";
            }
            if (time > FractalSubworld.POST_SENTINEL_TIME)
            {
                tip += $"\n{Language.GetTextValue("Mods.Polarities.BuffDescription.Fractalizing.PostSentinel")}";
            }
        }

        public override bool ReApply(Player player, int time, int buffIndex)
        {
            player.buffTime[buffIndex] += time;
            return false;
        }
    }
}