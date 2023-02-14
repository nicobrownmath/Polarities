using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Materials
{
    public class Rattle : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (5);
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 28;
            Item.maxStack = 9999;
            Item.value = 100;
            Item.rare = ItemRarityID.White;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
        }

        public override bool? UseItem(Player player)
        {
            SoundEngine.PlaySound(Sounds.Rattle, player.position);
            return true;
        }
    }
}