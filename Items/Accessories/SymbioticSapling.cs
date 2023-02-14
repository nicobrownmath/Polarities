using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Polarities.Items.Accessories
{
    public class SymbioticSapling : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 32;
            Item.accessory = true;
            Item.value = 750000;
            Item.rare = 7;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            HashSet<int> minionTypes = new HashSet<int>();
            float minionDiversity = 0f;
            for (int i = 0; i < Main.projectile.Length; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (projectile.owner == player.whoAmI && projectile.minion)
                {
                    if (!minionTypes.Contains(projectile.type))
                    {
                        minionTypes.Add(projectile.type);
                        minionDiversity += Math.Min(projectile.minionSlots, 1);
                    }
                }
            }

            player.GetDamage(DamageClass.Summon) += (float)Math.Max(0, 0.2f * (minionDiversity - 1));
        }
    }
}
