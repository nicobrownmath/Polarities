using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Effects;
using Polarities.Items.Materials;
using Polarities.Items.Placeable.Bars;
using Polarities.Projectiles;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Armor.ConvectiveArmor
{
    #region General Armor
    [AutoloadEquip(EquipType.Body)]
    public class ConvectiveArmor : ModItem, IGetBodyMaskColor
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);

            //registers a body glowmask color
            ArmorMasks.bodyIndexToBodyMaskColor.Add(EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body), this);
        }

        public override void SetDefaults()
        {
            Item.defense = 18;

            Item.width = 30;
            Item.height = 24;

            Item.value = Item.sellPrice(gold: 6);
            Item.rare = ItemRarityID.Yellow;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<PolaritiesPlayer>().light += new Vector3(0.25f, 0.25f, 0.25f);

            player.GetDamage(DamageClass.Generic) += 0.06f;
            player.GetCritChance(DamageClass.Generic) += 0.06f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<MantellarBar>(), 16)
                .AddIngredient(ItemType<WandererPlating>(), 3)
                .AddIngredient(ItemID.MoltenBreastplate)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }

        public Color BodyColor(ref PlayerDrawSet drawInfo)
        {
            return Color.White * drawInfo.shadow;
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class ConvectiveLeggings : ModItem, IDrawArmor
    {
        private Asset<Texture2D> GlowTexture;

        public override void Load()
        {
            GlowTexture = Request<Texture2D>(Texture + "_Legs");
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
            Item.defense = 12;

            Item.width = 22;
            Item.height = 16;

            Item.value = Item.sellPrice(gold: 4);
            Item.rare = ItemRarityID.Yellow;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<PolaritiesPlayer>().light += new Vector3(0.25f, 0.25f, 0.25f);

            player.moveSpeed += 0.1f;
            player.GetModPlayer<PolaritiesPlayer>().runSpeedBoost += 0.1f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<MantellarBar>(), 12)
                .AddIngredient(ItemType<WandererPlating>(), 2)
                .AddIngredient(ItemID.MoltenGreaves)
                .AddTile(TileID.MythrilAnvil)
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
    #endregion

    #region Set Bonus Draw
    public class ConvectiveSetBonusDrawLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.WebbedDebuffBack);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return drawInfo.drawPlayer.GetModPlayer<PolaritiesPlayer>().convectiveSetBonusCharge > 0;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            float progress = drawInfo.drawPlayer.GetModPlayer<PolaritiesPlayer>().convectiveSetBonusCharge / 600f;
            float pulseScale = (progress >= 1 ? (progress % 0.05f) * 20f : (progress % 0.1f) * 10f);
            Color color = ModUtils.ConvectiveFlameColor(Math.Min(1, progress * progress) / 2f * ((2 - pulseScale) / 2f)) * ((1 - pulseScale) * 2f);
            float scale = (float)Math.Min(1, Math.Sqrt(progress)) * pulseScale * 0.5f;
            drawInfo.DrawDataCache.Add(new DrawData(Textures.Glow256.Value, drawInfo.drawPlayer.MountedCenter - Main.screenPosition, Textures.Glow256.Frame(), color, 0f, Textures.Glow256.Size() / 2, scale, SpriteEffects.None, 0));
        }
    }
    #endregion

    #region Melee
    [AutoloadEquip(EquipType.Head)]
    public class ConvectiveHelmetMelee : ModItem, IDrawArmor
    {
        private Asset<Texture2D> GlowTexture;

        public override void Load()
        {
            GlowTexture = Request<Texture2D>(Texture + "_Head");
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
            Item.defense = 28;

            Item.width = 24;
            Item.height = 26;

            Item.value = Item.sellPrice(gold: 4, silver: 80);
            Item.rare = ItemRarityID.Yellow;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<PolaritiesPlayer>().light += new Vector3(0.25f, 0.25f, 0.25f);

            player.GetDamage(DamageClass.Melee) += 0.06f;
            player.GetCritChance(DamageClass.Melee) += 0.06f;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ItemType<ConvectiveArmor>() && legs.type == ItemType<ConvectiveLeggings>();
        }

        public override void UpdateArmorSet(Player player)
        {
            string hotkey = Polarities.ArmorSetBonusHotkey.GetAssignedKeys().ToArray().Length > 0 ? Polarities.ArmorSetBonusHotkey.GetAssignedKeys()[0] : Language.GetTextValue("Mods.Polarities.Misc.UnboundHotkey");
            player.setBonus = Language.GetTextValueWith("Mods.Polarities.ArmorSetBonus." + Name, new { Hotkey = hotkey });

            player.GetModPlayer<PolaritiesPlayer>().light += new Vector3(0.25f, 0.25f, 0.25f);

            player.GetDamage(DamageClass.Melee) += 0.17f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.05f;

            player.GetModPlayer<PolaritiesPlayer>().convectiveSetBonusType = DamageClass.Melee;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<MantellarBar>(), 14)
                .AddIngredient(ItemType<WandererPlating>(), 2)
                .AddIngredient(ItemID.MoltenHelmet)
                .AddTile(TileID.MythrilAnvil)
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

    public class ConvectiveArmorMeleeExplosion : ModProjectile
    {
        public override string Texture => "Polarities/Items/Weapons/Ranged/ContagunProjectile";

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 324;
            Projectile.height = 324;
            Projectile.scale = 0f;
            Projectile.timeLeft = 30;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
            Projectile.DamageType = DamageClass.Melee;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath14, Projectile.Center);
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Projectile.ai[0] = Main.rand.NextFloat() + 2f;
        }

        public override void AI()
        {
            Vector2 oldCenter = Projectile.Center;

            Projectile.scale = (1 - Projectile.timeLeft / 30f) * 2f;
            Projectile.width = (int)(324 * Projectile.scale);
            Projectile.height = (int)(324 * Projectile.scale);

            Projectile.Center = oldCenter;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return CustomCollision.CheckAABBvDisc(targetHitbox, new Circle(Projectile.Center, Projectile.width / 2));
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.OnFire3, 600);
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            DrawLayer.AddProjectile<DrawLayerAdditiveAfterProjectiles>(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float progress = Projectile.timeLeft / 30f;

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame();
            Vector2 center = frame.Center();

            for (int i = 1; i <= 4; i++)
            {
                Color color = ModUtils.ConvectiveFlameColor(progress * progress * (5 - i) / 4f) * (progress * 2f);
                float drawScale = Projectile.scale * i / 4f;
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, color, Projectile.rotation + (float)Math.Pow(Projectile.ai[0], 10), center, drawScale, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(Textures.Glow256.Value, Projectile.Center - Main.screenPosition, Textures.Glow256.Frame(), color, 0f, Textures.Glow256.Size() / 2, drawScale * 2, SpriteEffects.None, 0);
            }

            return false;
        }
    }
    #endregion

    #region Ranged
    [AutoloadEquip(EquipType.Head)]
    public class ConvectiveHelmetRanged : ModItem, IDrawArmor
    {
        private Asset<Texture2D> GlowTexture;

        public override void Load()
        {
            GlowTexture = Request<Texture2D>(Texture + "_Head");
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
            Item.defense = 16;

            Item.width = 28;
            Item.height = 28;

            Item.value = Item.sellPrice(gold: 4, silver: 80);
            Item.rare = ItemRarityID.Yellow;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<PolaritiesPlayer>().light += new Vector3(0.25f, 0.25f, 0.25f);

            player.GetDamage(DamageClass.Ranged) += 0.06f;
            player.GetCritChance(DamageClass.Ranged) += 0.06f;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ItemType<ConvectiveArmor>() && legs.type == ItemType<ConvectiveLeggings>();
        }

        public override void UpdateArmorSet(Player player)
        {
            string hotkey = Polarities.ArmorSetBonusHotkey.GetAssignedKeys().ToArray().Length > 0 ? Polarities.ArmorSetBonusHotkey.GetAssignedKeys()[0] : Language.GetTextValue("Mods.Polarities.Misc.UnboundHotkey");
            player.setBonus = Language.GetTextValueWith("Mods.Polarities.ArmorSetBonus." + Name, new { Hotkey = hotkey });

            player.GetModPlayer<PolaritiesPlayer>().light += new Vector3(0.25f, 0.25f, 0.25f);

            player.GetDamage(DamageClass.Ranged) += 0.17f;
            player.GetCritChance(DamageClass.Ranged) += 0.05f;

            player.GetModPlayer<PolaritiesPlayer>().convectiveSetBonusType = DamageClass.Ranged;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<MantellarBar>(), 14)
                .AddIngredient(ItemType<WandererPlating>(), 2)
                .AddIngredient(ItemID.MoltenHelmet)
                .AddTile(TileID.MythrilAnvil)
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

    public class ConvectiveArmorRangedDeathray : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Glow58";

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.timeLeft = 30;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item88, Projectile.Center);
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            float progress = Projectile.timeLeft / 30f;
            Projectile.scale = 4 * progress * (1 - progress) * 50f;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            DrawLayer.AddProjectile<DrawLayerAdditiveAfterProjectiles>(index);
        }

        public override bool ShouldUpdatePosition() => false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float length = 2000;

            float collisionPoint = 0;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + new Vector2(length, 0).RotatedBy(Projectile.rotation), Projectile.scale, ref collisionPoint);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.OnFire3, 600);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            float length = 2000;

            Rectangle lineFrame = new Rectangle(29, 0, 1, 58);
            Vector2 lineCenter = new Vector2(0, lineFrame.Height / 2);

            Rectangle capFrame = new Rectangle(0, 0, 29, 58);
            Vector2 capCenter = new Vector2(capFrame.Width, capFrame.Height / 2);

            Rectangle discFrame = new Rectangle(0, 0, 58, 58);
            Vector2 discCenter = new Vector2(discFrame.Width / 2, discFrame.Height / 2);

            float progress = Projectile.timeLeft / 30f;

            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(10, 0).RotatedBy(Projectile.rotation);

            for (int i = 1; i <= 4; i++)
            {
                Color color = ModUtils.ConvectiveFlameColor(progress * progress * (5 - i) / 4f) * (progress * 2f);
                float drawScale = Projectile.scale * i / 4f;

                Main.EntitySpriteDraw(texture, drawPos, lineFrame, color, Projectile.rotation, lineCenter, new Vector2(length, drawScale / lineFrame.Height), SpriteEffects.None, 0);
                Main.EntitySpriteDraw(texture, drawPos, capFrame, color, Projectile.rotation, capCenter, drawScale / capFrame.Height, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(texture, drawPos, discFrame, color, Projectile.rotation, discCenter, new Vector2(2, 4) * drawScale / discFrame.Height, SpriteEffects.None, 0);
            }

            return false;
        }
    }
    #endregion

    #region Magic
    [AutoloadEquip(EquipType.Head)]
    public class ConvectiveHelmetMagic : ModItem, IDrawArmor
    {
        private Asset<Texture2D> GlowTexture;

        public override void Load()
        {
            GlowTexture = Request<Texture2D>(Texture + "_Head");
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
            Item.defense = 10;

            Item.width = 26;
            Item.height = 26;

            Item.value = Item.sellPrice(gold: 4, silver: 80);
            Item.rare = ItemRarityID.Yellow;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<PolaritiesPlayer>().light += new Vector3(0.25f, 0.25f, 0.25f);

            player.GetDamage(DamageClass.Magic) += 0.06f;
            player.GetCritChance(DamageClass.Magic) += 0.06f;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ItemType<ConvectiveArmor>() && legs.type == ItemType<ConvectiveLeggings>();
        }

        public override void UpdateArmorSet(Player player)
        {
            string hotkey = Polarities.ArmorSetBonusHotkey.GetAssignedKeys().ToArray().Length > 0 ? Polarities.ArmorSetBonusHotkey.GetAssignedKeys()[0] : Language.GetTextValue("Mods.Polarities.Misc.UnboundHotkey");
            player.setBonus = Language.GetTextValueWith("Mods.Polarities.ArmorSetBonus." + Name, new { Hotkey = hotkey });

            player.GetModPlayer<PolaritiesPlayer>().light += new Vector3(0.25f, 0.25f, 0.25f);

            player.GetDamage(DamageClass.Magic) += 0.17f;
            player.statManaMax2 += 60;

            player.GetModPlayer<PolaritiesPlayer>().convectiveSetBonusType = DamageClass.Magic;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<MantellarBar>(), 14)
                .AddIngredient(ItemType<WandererPlating>(), 2)
                .AddIngredient(ItemID.MoltenHelmet)
                .AddTile(TileID.MythrilAnvil)
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

    public class ConvectiveArmorMagicEruption : ModProjectile
    {
        public override string Texture => "Polarities/Textures/Glow58";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.scale = 2f;
            Projectile.timeLeft = 240;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.light = 0.8f;
            Projectile.DamageType = DamageClass.Magic;

            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Vector2 oldCenter = Projectile.Center;

            Projectile.scale = Main.rand.NextFloat(1f, 2f);
            Projectile.width = (int)(36 * Projectile.scale);
            Projectile.height = (int)(36 * Projectile.scale);

            Projectile.Center = oldCenter;

            Projectile.timeLeft += Main.rand.Next(-60, 60);
        }

        public override void AI()
        {
            Projectile.velocity = Projectile.velocity.RotatedByRandom(Math.Min(0.1f, 1 / Projectile.velocity.Length()));

            if (Projectile.velocity.Length() > 1)
                Projectile.velocity *= 0.97f;

            Projectile.velocity.Y += 0.15f;

            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return CustomCollision.CheckAABBvDisc(targetHitbox, new Circle(Projectile.Center, Projectile.width / 2));
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.OnFire3, 600);
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            DrawLayer.AddProjectile<DrawLayerAdditiveAfterProjectiles>(index);
        }

        public override bool? CanCutTiles() => false;

        private const float FADE_TIME = 30f;

        public override bool PreDraw(ref Color lightColor)
        {
            float drawAlpha = 0.5f;
            if (Projectile.timeLeft < FADE_TIME) drawAlpha = 0.5f * Projectile.timeLeft / FADE_TIME;

            Texture2D texture = Textures.Glow256.Value;
            Rectangle frame = texture.Frame();
            Vector2 center = frame.Center();

            Texture2D mainTexture = TextureAssets.Projectile[Type].Value;
            Rectangle mainFrame = mainTexture.Frame();
            Vector2 mainCenter = mainFrame.Center();

            for (int i = 1; i <= 4; i++)
            {
                float progress = drawAlpha;
                Color color = ModUtils.ConvectiveFlameColor(progress * progress * (5 - i) / 4f) * (progress * 2f);
                float drawScale = Projectile.scale * i / 4f;
                Vector2 scale = new Vector2(1 + Projectile.velocity.Length() / 16f, 1) * drawScale;

                Main.EntitySpriteDraw(mainTexture, Projectile.Center - Main.screenPosition, mainFrame, color, Projectile.rotation, mainCenter, scale, SpriteEffects.None, 0);
            }

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] != Vector2.Zero)
                {
                    float progress = 1 - i / (float)Projectile.oldPos.Length;
                    Vector2 scale = new Vector2((Projectile.oldPos[i] - Projectile.oldPos[i - 1]).Length() * 0.02f, progress * 0.1f) * Projectile.scale;
                    Color color = ModUtils.ConvectiveFlameColor(progress * progress / 2f) * drawAlpha;
                    Main.EntitySpriteDraw(texture, Projectile.oldPos[i] + Projectile.Center - Projectile.position - Main.screenPosition, frame, color, Projectile.oldRot[i], center, scale, SpriteEffects.None, 0);
                }
            }

            return false;
        }
    }
    #endregion

    #region Summon
    [AutoloadEquip(EquipType.Head)]
    public class ConvectiveHelmetSummon : ModItem, IDrawArmor
    {
        private Asset<Texture2D> GlowTexture;

        public override void Load()
        {
            GlowTexture = Request<Texture2D>(Texture + "_Head");
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
            Item.defense = 4;

            Item.width = 32;
            Item.height = 28;

            Item.value = Item.sellPrice(gold: 4, silver: 80);
            Item.rare = ItemRarityID.Yellow;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<PolaritiesPlayer>().light += new Vector3(0.25f, 0.25f, 0.25f);

            player.GetDamage(DamageClass.Summon) += 0.06f;
            player.maxMinions++;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ItemType<ConvectiveArmor>() && legs.type == ItemType<ConvectiveLeggings>();
        }

        public override void UpdateArmorSet(Player player)
        {
            string hotkey = Polarities.ArmorSetBonusHotkey.GetAssignedKeys().ToArray().Length > 0 ? Polarities.ArmorSetBonusHotkey.GetAssignedKeys()[0] : Language.GetTextValue("Mods.Polarities.Misc.UnboundHotkey");
            player.setBonus = Language.GetTextValueWith("Mods.Polarities.ArmorSetBonus." + Name, new { Hotkey = hotkey });

            player.GetModPlayer<PolaritiesPlayer>().light += new Vector3(0.25f, 0.25f, 0.25f);

            player.GetDamage(DamageClass.Summon) += 0.17f;
            player.maxMinions++;

            player.GetModPlayer<PolaritiesPlayer>().convectiveSetBonusType = DamageClass.Summon;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<MantellarBar>(), 14)
                .AddIngredient(ItemType<WandererPlating>(), 2)
                .AddIngredient(ItemID.MoltenHelmet)
                .AddTile(TileID.MythrilAnvil)
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

    //TODO: Give this the convective minion's explosion on hit effect, rework that minion
    public class ConvectiveArmorSummonVortex : ModProjectile
    {
        public override string Texture => "Polarities/NPCs/ConvectiveWanderer/ConvectiveWandererHeatVortex";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 54;
            Projectile.height = 54;
            Projectile.scale = 1f;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.light = 0.8f;
            Projectile.DamageType = DamageClass.Summon;

            Projectile.GetGlobalProjectile<PolaritiesProjectile>().usesGeneralHitCooldowns = true;
            Projectile.GetGlobalProjectile<PolaritiesProjectile>().generalHitCooldownTime = -1;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (Projectile.ai[0] != -2)
            {
                int targetID = -1;
                Projectile.Minion_FindTargetInRange(800, ref targetID, false);
                Projectile.ai[0] = targetID;

                int index = 0;
                int numProjectiles = 0;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].type == Type && Main.projectile[i].owner == Projectile.owner && Main.projectile[i].ai[0] == targetID)
                    {
                        numProjectiles++;
                        if (i < Projectile.whoAmI) index++;
                    }
                }

                if (targetID == -1)
                {
                    Player player = Main.player[Projectile.owner];

                    Vector2 goalPosition = player.Center + player.velocity * 20f + new Vector2(0, -96) + new Vector2(128, 0).RotatedBy(PolaritiesSystem.timer * 0.05f + index * MathHelper.TwoPi / numProjectiles) * new Vector2(1, 0.25f);
                    Vector2 goalVelocity = (goalPosition - Projectile.Center) / 20f;
                    Projectile.velocity += (goalVelocity - Projectile.velocity) / 20f;

                    Projectile.ai[1] = 0;
                }
                else
                {
                    NPC target = Main.npc[targetID];

                    Vector2 goalPosition = target.Center + target.velocity * 20f + new Vector2(256, 0).RotatedBy(PolaritiesSystem.timer * 0.05f + index * MathHelper.TwoPi / numProjectiles);
                    Vector2 goalVelocity = (goalPosition - Projectile.Center) / 20f;
                    Projectile.velocity += (goalVelocity - Projectile.velocity) / 20f;

                    Projectile.ai[1]++;
                    if (Projectile.ai[1] >= 60)
                    {
                        Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 32f + target.velocity;

                        for (int i = 0; i < Main.maxProjectiles; i++)
                        {
                            if (Main.projectile[i].active && Main.projectile[i].type == Type && Main.projectile[i].owner == Projectile.owner && Main.projectile[i].ai[0] == targetID)
                            {
                                if (Main.projectile[i].ai[1] > 40) Main.projectile[i].ai[1] = 40;
                            }
                        }

                        Projectile.ai[0] = -2;
                    }
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return CustomCollision.CheckAABBvDisc(targetHitbox, new Circle(Projectile.Center, Projectile.width / 2));
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.OnFire3, 600);
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            DrawLayer.AddProjectile<DrawLayerAdditiveAfterProjectiles>(index);
        }

        public override bool? CanCutTiles() => false;

        public override bool? CanDamage() => Projectile.ai[0] == -2;

        private const float FADE_TIME = 30f;

        public override bool PreDraw(ref Color lightColor)
        {
            float drawAlpha = 0.5f;
            if (Projectile.timeLeft < FADE_TIME) drawAlpha = 0.5f * Projectile.timeLeft / FADE_TIME;

            Texture2D texture = Textures.Glow256.Value;
            Rectangle frame = texture.Frame();
            Vector2 center = frame.Center();

            Texture2D mainTexture = TextureAssets.Projectile[Type].Value;
            Rectangle mainFrame = mainTexture.Frame();
            Vector2 mainCenter = mainFrame.Center();

            for (int i = 1; i <= 4; i++)
            {
                float progress = drawAlpha;
                Color color = ModUtils.ConvectiveFlameColor(progress * progress * (5 - i) / 4f) * (progress * 2f);
                float drawScale = Projectile.scale * i / 4f;
                float scale = (float)Math.Sqrt(1 + Projectile.velocity.Length() / 16f) * drawScale * 0.22f;

                float rotation = PolaritiesSystem.timer * 0.2f * i / 4f;

                Main.EntitySpriteDraw(mainTexture, Projectile.Center - Main.screenPosition, mainFrame, color, rotation, mainCenter, scale, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(mainTexture, Projectile.Center - Main.screenPosition, mainFrame, color, rotation + MathHelper.PiOver2, mainCenter, scale, SpriteEffects.None, 0);
            }

            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] != Vector2.Zero)
                {
                    float progress = 1 - i / (float)Projectile.oldPos.Length;
                    Vector2 scale = new Vector2((Projectile.oldPos[i] - Projectile.oldPos[i - 1]).Length() * 0.02f, progress * 0.1f) * Projectile.scale;
                    Color color = ModUtils.ConvectiveFlameColor(progress * progress / 2f) * drawAlpha;
                    Main.EntitySpriteDraw(texture, Projectile.oldPos[i] + Projectile.Center - Projectile.position - Main.screenPosition, frame, color, Projectile.oldRot[i], center, scale, SpriteEffects.None, 0);
                }
            }

            return false;
        }
    }
    #endregion
}