using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Items.Materials;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Armor.LimestoneArmor
{
    [AutoloadEquip(EquipType.Body)]
    public class LimestoneChestplate : ModItem, IGetBodyMaskColor
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);

            //registers a body glowmask color
            ArmorMasks.bodyIndexToBodyMaskColor.Add(EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body), this);
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 24;
            Item.value = 50000;
            Item.defense = 15;
            Item.rare = ItemRarityID.Pink;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetCritChance(DamageClass.Generic) += 3;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<LimestoneCarapace>(), 12)
                .AddIngredient(ItemType<AlkalineFluid>(), 10)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public Color BodyColor(ref PlayerDrawSet drawInfo)
        {
            return Color.White * drawInfo.shadow;
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class LimestoneGreaves : ModItem, IDrawArmor
    {
        private Asset<Texture2D> GlowTexture;

        public override void Load()
        {
            GlowTexture = Request<Texture2D>(Texture + "_Legs_Mask");
        }

        public override void Unload()
        {
            GlowTexture = null;
        }

        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);

            //registers a head glowmask
            ArmorMasks.legIndexToArmorDraw.TryAdd(EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs), this);
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 18;
            Item.value = 50000;
            Item.defense = 12;
            Item.rare = ItemRarityID.Pink;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetCritChance(DamageClass.Generic) += 3;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<LimestoneCarapace>(), 10)
                .AddIngredient(ItemType<AlkalineFluid>(), 10)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public void DrawArmor(ref PlayerDrawSet drawInfo)
        {
            if (!drawInfo.drawPlayer.invis)
            {
                Rectangle legFrame3 = drawInfo.drawPlayer.legFrame;
                Vector2 legVect2 = drawInfo.legVect;
                if (drawInfo.drawPlayer.gravDir == 1f)
                {
                    legFrame3.Height -= 4;
                }
                else
                {
                    legVect2.Y -= 4f;
                    legFrame3.Height -= 4;
                }
                Vector2 legsOffset = drawInfo.legsOffset;
                DrawData data = new DrawData(GlowTexture.Value, legsOffset + new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.legFrame.Width / 2 + drawInfo.drawPlayer.width / 2), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.legFrame.Height + 4f)) + drawInfo.drawPlayer.legPosition + drawInfo.legVect, legFrame3, Color.White * drawInfo.shadow, drawInfo.drawPlayer.legRotation, legVect2, 1f, drawInfo.playerEffect, 0)
                {
                    shader = drawInfo.cLegs
                };
                drawInfo.DrawDataCache.Add(data);
            }
        }
    }

    [AutoloadEquip(EquipType.Head)]
    public class LimestoneHelmet : ModItem, IDrawArmor
    {
        private Asset<Texture2D> GlowTexture;

        public override void Load()
        {
            GlowTexture = Request<Texture2D>(Texture + "_Head_Mask");
        }

        public override void Unload()
        {
            GlowTexture = null;
        }

        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);

            int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;

            //registers a head glowmask
            ArmorMasks.headIndexToArmorDraw.TryAdd(equipSlotHead, this);
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 24;
            Item.value = 50000;
            Item.defense = 13;
            Item.rare = ItemRarityID.Pink;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetCritChance(DamageClass.Generic) += 3;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ItemType<LimestoneChestplate>() && legs.type == ItemType<LimestoneGreaves>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = Language.GetTextValue("Mods.Polarities.ArmorSetBonus." + Name);

            player.statDefense += 40;
            player.GetModPlayer<PolaritiesPlayer>().limestoneSetBonus = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<LimestoneCarapace>(), 8)
                .AddIngredient(ItemType<AlkalineFluid>(), 10)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public void DrawArmor(ref PlayerDrawSet drawInfo)
        {
            if (!drawInfo.drawPlayer.invis)
            {
                Rectangle bodyFrame3 = drawInfo.drawPlayer.bodyFrame;
                Vector2 headVect2 = drawInfo.headVect;
                if (drawInfo.drawPlayer.gravDir == 1f)
                {
                    bodyFrame3.Height -= 4;
                }
                else
                {
                    headVect2.Y -= 4f;
                    bodyFrame3.Height -= 4;
                }
                Vector2 helmetOffset = drawInfo.helmetOffset;
                DrawData data = new DrawData(GlowTexture.Value, helmetOffset + new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.bodyFrame.Width / 2 + drawInfo.drawPlayer.width / 2), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.headPosition + drawInfo.headVect, bodyFrame3, Color.White * drawInfo.shadow, drawInfo.drawPlayer.headRotation, headVect2, 1f, drawInfo.playerEffect, 0)
                {
                    shader = drawInfo.cHead
                };
                drawInfo.DrawDataCache.Add(data);
            }
        }
    }
}