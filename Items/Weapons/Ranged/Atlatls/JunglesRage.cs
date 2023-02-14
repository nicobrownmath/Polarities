using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Weapons.Ranged.Atlatls
{
    public class JunglesRage : AtlatlBase
    {
        public override Vector2[] ShotDistances => new Vector2[] { new Vector2(44) };

        public override void SetDefaults()
        {
            Item.SetWeaponValues(30, 3, 0);
            Item.DamageType = DamageClass.Ranged;

            Item.width = 68;
            Item.height = 62;

            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            Item.shoot = 10;
            Item.shootSpeed = 4.5f;
            Item.useAmmo = AmmoID.Dart;

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Pink;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return Main.rand.NextBool();
        }
    }
}
