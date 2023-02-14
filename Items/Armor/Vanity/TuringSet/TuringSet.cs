using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Items.Accessories.Wings;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Armor.Vanity.TuringSet
{
    [AutoloadEquip(EquipType.Head)]
    public class TuringHead : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 22;
            Item.value = 0;
            Item.rare = ItemRarityID.Cyan;
            Item.vanity = true;
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class TuringBody : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 20;
            Item.value = 0;
            Item.rare = ItemRarityID.Cyan;
            Item.vanity = true;
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class TuringLegs : ModItem, IDrawArmor
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);

            //registers a legs glowmask
            ArmorMasks.legIndexToArmorDraw.TryAdd(EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs), this);

            ArmorIDs.Legs.Sets.OverridesLegs[EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs)] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 24;
            Item.value = 0;
            Item.rare = ItemRarityID.Cyan;
            Item.vanity = true;
        }

        public override void UpdateEquip(Player player)
        {
            player.slowFall = true;
        }

        public override void UpdateVanity(Player player)
        {
            player.shoe = 0;
            player.hermesStepSound.Style = SoundID.Item24;
        }

        public bool DoVanillaDraw()
        {
            return false;
        }

        public void DrawArmor(ref PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;

            if (!drawPlayer.invis)
            {
                Texture2D texture = TuringWings.PulseTexture.Value;

                float alpha = (255 - drawPlayer.immuneAlpha) / 255f;
                Color color = Color.White * drawPlayer.stealth;
                Rectangle frame = new Rectangle(0, 0, 28, 28);
                float rotation = drawPlayer.legRotation;
                SpriteEffects spriteEffects = SpriteEffects.None;

                float drawX = (int)drawInfo.Position.X + drawPlayer.width / 2;
                float drawY = (int)drawInfo.Position.Y + drawPlayer.height / 2;
                Vector2 origin = new Vector2(14, -14);

                if (!drawPlayer.mount.Active && (drawPlayer.grapCount <= 0 || drawPlayer.velocity.Y == 0))
                {
                    PolaritiesPlayer modPlayer = drawPlayer.GetModPlayer<PolaritiesPlayer>();
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 pulsePosition = modPlayer.oldCenters[i] - new Vector2(0, drawPlayer.gravDir * 6) + modPlayer.oldVelocities[i] * 0.5f * i;

                        float pulseScale = i * 0.125f + 0.5f;

                        float pulseRotation = drawPlayer.gravDir == 1 ? 0 : MathHelper.Pi;

                        DrawData drawData = new DrawData(texture, drawPlayer.position - drawPlayer.oldPosition + pulsePosition - Main.screenPosition, frame, color * alpha * ((4.5f - pulseScale) / 4.5f), rotation + pulseRotation, origin, pulseScale, spriteEffects, 0)
                        {
                            shader = drawInfo.cWings
                        };
                        drawInfo.DrawDataCache.Add(drawData);
                    }
                }

                // The texture we want to display on our player
                Rectangle legFrame3 = drawPlayer.legFrame;
                Vector2 legVect2 = drawInfo.legVect;
                if (drawPlayer.gravDir == 1f)
                {
                    legFrame3.Height -= 4;
                }
                else
                {
                    legVect2.Y -= 4f;
                    legFrame3.Height -= 4;
                }
                Vector2 legsOffset = drawInfo.legsOffset;
                DrawData data = new DrawData(TextureAssets.ArmorLeg[EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs)].Value, legsOffset + new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawPlayer.legFrame.Width / 2 + drawPlayer.width / 2), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.legFrame.Height + 4f)) + drawPlayer.legPosition + drawInfo.legVect, legFrame3, drawInfo.colorArmorLegs, drawPlayer.legRotation, legVect2, 1f, drawInfo.playerEffect, 0)
                {
                    shader = drawInfo.cLegs
                };
                drawInfo.DrawDataCache.Add(data);
            }
        }
    }
}

