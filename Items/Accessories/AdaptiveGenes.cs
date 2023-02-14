using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Accessories
{
    public class AdaptiveGenes : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 50;

            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 4);
            Item.rare = ItemRarityID.Expert;
            Item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Generic) += 0.10f * (player.statLife / (float)player.statLifeMax2);
            player.GetCritChance(DamageClass.Generic) += (int)(10 * (player.statLife / (float)player.statLifeMax2));

            player.moveSpeed += 0.20f * (1 - player.statLife / (float)player.statLifeMax2);
            player.GetModPlayer<PolaritiesPlayer>().runSpeedBoost += 0.20f * (1 - player.statLife / (float)player.statLifeMax2);
            player.lifeRegen += (int)(10 * (1 - player.statLife / (float)player.statLifeMax2));
        }
    }
}
