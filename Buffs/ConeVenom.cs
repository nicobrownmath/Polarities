using Polarities.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Buffs
{
    public class ConeVenom : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Venom;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cone Venom");
            Description.SetDefault("You cannot move\nThis includes your lungs");

            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<PolaritiesPlayer>().coneVenom = true;

            player.noItems = true;

            player.controlJump = false;
            player.controlDown = false;
            player.controlLeft = false;
            player.controlRight = false;
            player.controlUp = false;
            player.controlUseItem = false;
            player.controlUseTile = false;
            player.controlThrow = false;
            player.controlHook = false;
            player.controlMount = false;
            player.gravDir = 1f;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            if (!npc.buffImmune[BuffID.Venom])
            {
                npc.GetGlobalNPC<PolaritiesNPC>().coneVenom = true;
                if (!npc.buffImmune[BuffID.Confused])
                {
                    npc.confused = true;
                }
            }
        }
    }
}