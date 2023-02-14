using Microsoft.Xna.Framework;
using Polarities.Items.Placeable.Bars;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Armor.SunplateArmor
{
    [AutoloadEquip(EquipType.Body)]
    public class SunplateArmor : ModItem, IGetBodyMaskColor
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);

            //registers a body glowmask color
            ArmorMasks.bodyIndexToBodyMaskColor.Add(EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body), this);
        }

        public override void SetDefaults()
        {
            Item.defense = 7;

            Item.width = 30;
            Item.height = 22;

            Item.value = 10000;
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.03f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<SunplateBar>(), 8)
                .AddIngredient(ItemID.FallenStar, 8)
                .AddTile(TileID.SkyMill)
                .Register();
        }

        public Color BodyColor(ref PlayerDrawSet drawInfo)
        {
            return Color.White * drawInfo.shadow;
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class SunplateBoots : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 18;
            Item.value = 10000;
            Item.rare = 1;
            Item.defense = 6;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.03f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<SunplateBar>(), 6)
                .AddIngredient(ItemID.FallenStar, 6)
                .AddTile(TileID.SkyMill)
                .Register();
        }
    }

    [AutoloadEquip(EquipType.Head)]
    public class SunplateMask : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.value = 10000;
            Item.rare = 1;
            Item.defense = 6;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ItemType<SunplateArmor>() && legs.type == ItemType<SunplateBoots>();
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Generic) += 0.03f;
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = Language.GetTextValue("Mods.Polarities.ArmorSetBonus." + Name);

            player.GetModPlayer<PolaritiesPlayer>().dashIndex = GetInstance<SunplateDash>().Index;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<SunplateBar>(), 5)
                .AddIngredient(ItemID.FallenStar, 5)
                .AddTile(TileID.SkyMill)
                .Register();
        }
    }

    public class SunplateDash : Dash
    {
        public override int Index => 1;

        public override float Speed => 12f;
        public override int Cooldown => 60;
        public override int Duration => 30;

        public override void OnDash(Player player)
        {
            Color newColor7 = Color.CornflowerBlue;
            if (Main.tenthAnniversaryWorld)
            {
                newColor7 = Color.HotPink;
                newColor7.A = (byte)(newColor7.A / 2);
            }
            float scaleMult = 1;
            Vector2 useVelocity = player.velocity;
            for (int num573 = 0; num573 < 7 * scaleMult; num573++)
            {
                Dust.NewDust(player.position, player.width, player.height, 58, useVelocity.X * 0.1f, useVelocity.Y * 0.1f, 150, default(Color), 0.8f);
            }
            for (float num574 = 0f; num574 < 1f * scaleMult; num574 += 0.125f)
            {
                Vector2 center25 = player.Center;
                Vector2 unitY11 = Vector2.UnitY;
                double radians36 = num574 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f;
                Vector2 center2 = default(Vector2);
                Dust.NewDustPerfect(center25, 278, unitY11.RotatedBy(radians36, center2) * (4f + Main.rand.NextFloat() * 4f), 150, newColor7).noGravity = true;
            }
            for (float num575 = 0f; num575 < 1f * scaleMult; num575 += 0.25f)
            {
                Vector2 center26 = player.Center;
                Vector2 unitY12 = Vector2.UnitY;
                double radians37 = num575 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f;
                Vector2 center2 = default(Vector2);
                Dust.NewDustPerfect(center26, 278, unitY12.RotatedBy(radians37, center2) * (2f + Main.rand.NextFloat() * 3f), 150, Color.Gold).noGravity = true;
            }
            Vector2 value15 = new Vector2(Main.screenWidth, Main.screenHeight);
            if (player.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + value15 / 2f, value15 + new Vector2(400f))))
            {
                for (int num576 = 0; num576 < 7 * scaleMult; num576++)
                {
                    Vector2 val29 = player.Center - new Vector2(11) + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)) * (player.Size - new Vector2(22)) / 2f;
                    Vector2 val30 = Main.rand.NextVector2CircularEdge(0.5f, 0.5f) * useVelocity.Length();
                    int[] array18 = new int[8] { 16, 17, 17, 17, 17, 17, 17, 17 };
                    Gore.NewGore(player.GetSource_FromThis(), val29, val30, Utils.SelectRandom(Main.rand, array18));
                }
            }
        }

        public override void Update(Player player, int timeLeft)
        {
            Vector2 value34 = new Vector2(Main.screenWidth, Main.screenHeight);
            if (player.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + value34 / 2f, value34 + new Vector2(400f))) && Main.rand.Next(6) == 0)
            {
                int[] array6 = new int[4] { 16, 17, 17, 17 };
                int num855 = Utils.SelectRandom(Main.rand, array6);
                if (Main.tenthAnniversaryWorld)
                {
                    int[] array7 = new int[4] { 16, 16, 16, 17 };
                    num855 = Utils.SelectRandom(Main.rand, array7);
                }
                Gore.NewGore(player.GetSource_FromThis(), player.Center - new Vector2(11) + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)) * (player.Size - new Vector2(22)) / 2f, player.velocity * 0.2f, num855);
            }
            if (Main.rand.Next(20) == 0 || (Main.tenthAnniversaryWorld && Main.rand.Next(15) == 0))
            {
                Dust.NewDust(player.position, player.width, player.height, 58, player.velocity.X * 0.5f, player.velocity.Y * 0.5f, 150, default(Color), 1.2f);
            }
        }
    }
}