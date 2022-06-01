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
using Terraria.Audio;

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
