using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

namespace Polarities.Items.Armor.MechaMayhemArmor
{
    [AutoloadEquip(EquipType.Body)]
    public class FlawlessMechChestplate : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);

            int equipSlotBody = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
            ArmorIDs.Body.Sets.HidesArms[equipSlotBody] = true;
            ArmorIDs.Body.Sets.HidesHands[equipSlotBody] = true;
            ArmorIDs.Body.Sets.HidesBottomSkin[equipSlotBody] = true;
            ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBody] = true;
            ArmorIDs.Body.Sets.shouldersAreAlwaysInTheBack[equipSlotBody] = true;
            PolaritiesItem.IsFlawless.Add(Type);
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 20;
            Item.value = Item.sellPrice(gold: 6);
            Item.defense = 20;

            Item.rare = RarityType<MechBossFlawlessRarity>();
        }

        public override void UpdateEquip(Player player)
        {
            player.endurance = player.endurance * 0.94f;
            player.GetModPlayer<PolaritiesPlayer>().flawlessMechChestplate = true;
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class FlawlessMechTail : ModItem, IDrawArmor
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

            ArmorIDs.Legs.Sets.OverridesLegs[EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs)] = true;
            PolaritiesItem.IsFlawless.Add(Type);
        }

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 14;
            Item.value = Item.sellPrice(gold: 6);
            Item.defense = 14;

            Item.rare = RarityType<MechBossFlawlessRarity>();
        }

        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.06f;
            player.GetModPlayer<PolaritiesPlayer>().flawlessMechTail = true;
        }

        public void DrawArmor(ref PlayerDrawSet drawInfo)
        {
            Player drawPlayer = drawInfo.drawPlayer;

            if (!drawPlayer.invis)
            {
                Texture2D texture = TextureAssets.ArmorLeg[EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs)].Value;

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
                legVect2.X = 44;
                legFrame3.Width = texture.Width;
                legFrame3.Height += 2;
                Vector2 legsOffset = drawInfo.legsOffset;
                Vector2 position = legsOffset + new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.legFrame.Width / 2 + drawInfo.drawPlayer.width / 2), (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.legFrame.Height + 4f)) + drawInfo.drawPlayer.legPosition + drawInfo.legVect;
                Color color = drawInfo.colorArmorLegs * drawPlayer.stealth * drawInfo.shadow;

                DrawData data = new DrawData(texture, position, legFrame3, color, drawInfo.drawPlayer.legRotation, legVect2, 1f, drawInfo.playerEffect, 0)
                {
                    shader = drawInfo.cLegs
                };
                drawInfo.DrawDataCache.Add(data);

                texture = GlowTexture.Value;
                color = Color.White * drawPlayer.stealth * drawInfo.shadow;

                data = new DrawData(texture, position, legFrame3, color, drawInfo.drawPlayer.legRotation, legVect2, 1f, drawInfo.playerEffect, 0)
                {
                    shader = drawInfo.cLegs
                };
                drawInfo.DrawDataCache.Add(data);
            }
        }

        public bool DoVanillaDraw() => false;
    }

    [AutoloadEquip(EquipType.Head)]
    public class FlawlessMechMask : ModItem, IDrawArmor
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
            PolaritiesItem.IsFlawless.Add(Type);
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 32;
            Item.value = Item.sellPrice(gold: 6);
            Item.defense = 15;

            Item.rare = RarityType<MechBossFlawlessRarity>();
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ItemType<FlawlessMechChestplate>() && legs.type == ItemType<FlawlessMechTail>();
        }

        public override void UpdateEquip(Player player)
        {
            player.endurance = player.endurance * 0.97f;
            player.moveSpeed += 0.03f;
            player.GetModPlayer<PolaritiesPlayer>().flawlessMechMask = true;
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = Language.GetTextValue("Mods.Polarities.ArmorSetBonus." + Name);

            player.GetModPlayer<PolaritiesPlayer>().flawlessMechArmorSet = true;
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

    public class FlawlessMechMaskDeathray : ModProjectile
    {
        private float minDistance = 7;
        private float DISTANCE;

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = PolaritiesPlayer.MECH_MASK_COOLDOWN;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;

            Projectile.hide = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (player.dead || !player.active)
            {
                Projectile.Kill();
            }

            if (Projectile.timeLeft == PolaritiesPlayer.MECH_MASK_COOLDOWN)
            {
                SoundEngine.PlaySound(SoundID.Item12, Projectile.Center);

                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            Projectile.velocity = player.MountedCenter + new Vector2(player.direction * 4, -9) - Projectile.Center;

            if (Projectile.timeLeft < 5)
            {
                Projectile.scale -= 0.2f;
            }

            GetDistance();
            CreateDusts();
        }

        private void GetDistance()
        {
            DISTANCE = minDistance;
            while (DISTANCE < 2000)
            {
                Vector2 updatedPosition = Projectile.Center + new Vector2(DISTANCE, 0).RotatedBy(Projectile.rotation);
                if (new Vector2(8, 0).RotatedBy(Projectile.rotation) != Collision.TileCollision(Projectile.Center + new Vector2(DISTANCE, 0).RotatedBy(Projectile.rotation), new Vector2(8, 0).RotatedBy(Projectile.rotation), 1, 1, true, true))
                {
                    DISTANCE += Collision.TileCollision(Projectile.Center + new Vector2(DISTANCE, 0).RotatedBy(Projectile.rotation), new Vector2(8, 0).RotatedBy(Projectile.rotation), 1, 1, true, true).Length();
                    break;
                }
                else
                    DISTANCE += 8;
            }
        }

        private void CreateDusts()
        {
            float scale = 0.75f;
            Dust.NewDustPerfect(Projectile.Center + new Vector2(minDistance, 0).RotatedBy(Projectile.rotation), 114, new Vector2(2, 1).RotatedBy(Projectile.rotation).RotatedByRandom(0.25f) * Main.rand.NextFloat(0.5f, 1f), Scale: scale).noGravity = true;
            Dust.NewDustPerfect(Projectile.Center + new Vector2(minDistance, 0).RotatedBy(Projectile.rotation), 114, new Vector2(2, -1).RotatedBy(Projectile.rotation).RotatedByRandom(0.25f) * Main.rand.NextFloat(0.5f, 1f), Scale: scale).noGravity = true;

            Dust.NewDustPerfect(Projectile.Center + new Vector2(DISTANCE, 0).RotatedBy(Projectile.rotation), 130, new Vector2(-2, 1).RotatedBy(Projectile.rotation).RotatedByRandom(0.25f) * Main.rand.NextFloat(0.5f, 1f), Scale: scale).noGravity = true;
            Dust.NewDustPerfect(Projectile.Center + new Vector2(DISTANCE, 0).RotatedBy(Projectile.rotation), 130, new Vector2(-2, -1).RotatedBy(Projectile.rotation).RotatedByRandom(0.25f) * Main.rand.NextFloat(0.5f, 1f), Scale: scale).noGravity = true;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center + new Vector2(minDistance, 0).RotatedBy(Projectile.rotation), Projectile.Center + new Vector2(DISTANCE, 0).RotatedBy(Projectile.rotation));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            float distance = minDistance;
            float distanceIncrement = 10 * Math.Max(0.1f, Projectile.scale);
            while (distance < DISTANCE)
            {
                Vector2 updatedPosition = Projectile.Center + new Vector2(distance, 0).RotatedBy(Projectile.rotation);

                Main.EntitySpriteDraw(texture, updatedPosition - Main.screenPosition, new Rectangle(0, 10, 10, 10), Color.White, Projectile.rotation - MathHelper.PiOver2, new Vector2(5, 5), Projectile.scale, SpriteEffects.None, 0);

                distance += distanceIncrement;
            }

            Main.EntitySpriteDraw(texture, Projectile.Center + new Vector2(DISTANCE, 0).RotatedBy(Projectile.rotation) - Main.screenPosition, new Rectangle(0, 20, 10, 10), Color.White, Projectile.rotation - MathHelper.PiOver2, new Vector2(5, 5), Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center + new Vector2(minDistance, 0).RotatedBy(Projectile.rotation) - Main.screenPosition, new Rectangle(0, 0, 10, 10), Color.White, Projectile.rotation - MathHelper.PiOver2, new Vector2(5, 5), Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }
    }

    public class MiniPrimeArmSlash : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 126;
            Projectile.height = 124;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;

            Projectile.hide = true;

            Projectile.light = 0.5f;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (player.controlUseItem && !player.dead && player.active)
            {
                Projectile.timeLeft = 2;
            }
            else
            {
                Projectile.Kill();
                return;
            }

            Projectile.velocity = player.MountedCenter - Projectile.Center;

            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.rotation = (Main.MouseWorld - Projectile.Center).ToRotation();
                Projectile.spriteDirection = Main.MouseWorld.X > Projectile.Center.X ? 1 : -1;
            }
            Projectile.netUpdate = true;

            bool isFrantic = player.GetModPlayer<PolaritiesPlayer>().flawlessMechSetBonusTime > 0;

            if (isFrantic)
            {
                Projectile.localNPCHitCooldown = 5;
            }
            else
            {
                Projectile.localNPCHitCooldown = 10;
            }

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 2)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;

                if (!isFrantic)
                {
                    if (Projectile.frame >= 31)
                    {
                        Projectile.frame = 0;
                    }
                }
                else
                {
                    if (Projectile.frame < 31)
                    {
                        Projectile.frame = 31;
                    }
                    else if (Projectile.frame == 40)
                    {
                        Projectile.frame = 37;
                    }
                }
            }

            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = isFrantic ? 3 : 12;
                SoundEngine.PlaySound(SoundID.Item1, player.position);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            bool isFrantic = Projectile.frame >= 31;

            Vector2 center = Projectile.Center;
            float radius = Projectile.width / 2;

            if (!isFrantic)
            {
                radius /= 2;
                center = Projectile.Center + new Vector2(radius, 0).RotatedBy(Projectile.rotation);
            }

            float nearestX = Math.Max(targetHitbox.X, Math.Min(center.X, targetHitbox.X + targetHitbox.Size().X));
            float nearestY = Math.Max(targetHitbox.Y, Math.Min(center.Y, targetHitbox.Y + targetHitbox.Size().Y));
            return (new Vector2(center.X - nearestX, center.Y - nearestY)).Length() < radius;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            int frameX = 126 * (Projectile.frame / 8);
            int frameY = 124 * (Projectile.frame % 8);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle(frameX, frameY, 126, 124), Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16), Projectile.rotation, new Vector2(63, 62), Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically, 0);

            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }
    }

    public class MiniProbe : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 360;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == 360)
            {
                SoundEngine.PlaySound(SoundID.Item61, Projectile.Center);
            }
            else if ((360 - Projectile.timeLeft) % 45 == 0)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(16, 0).RotatedBy(Projectile.rotation), ProjectileType<MiniProbeLaser>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                }
            }

            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.rotation = (Main.MouseWorld - Projectile.Center).ToRotation();

                Vector2 goalPosition = Main.MouseWorld + (Projectile.Center + Projectile.velocity * 8 - Main.MouseWorld).SafeNormalize(Vector2.Zero) * 120;
                Vector2 goalVelocity = (goalPosition - Projectile.Center).SafeNormalize(Vector2.Zero) * 12;
                Projectile.velocity += (goalVelocity - Projectile.velocity) / 30f;
            }
            Projectile.netUpdate = true;
        }

        public static Asset<Texture2D> MaskTexture;

        public override void Load()
        {
            MaskTexture = Request<Texture2D>(Texture + "_Mask");
        }

        public override void Unload()
        {
            MaskTexture = null;
        }

        public override void PostDraw(Color lightColor)
        {
            Main.EntitySpriteDraw(MaskTexture.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 16, 16), Color.White, Projectile.rotation, new Vector2(8, 8), Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically, 0);
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust.NewDustPerfect(Projectile.Center, 130, new Vector2(3, 0).RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(0.5f, 1f), Scale: 0.75f).noGravity = true;
            }
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override bool? CanDamage()
        {
            return false;
        }
    }

    public class MiniProbeLaser : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.penetrate = 1;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 2, 16), Color.White, Projectile.rotation, new Vector2(1, 1), Projectile.scale, SpriteEffects.None, 0);

            return false;
        }

        public override void Kill(int timeLeft)
        {
            Dust.NewDustPerfect(Projectile.Center, 130, Vector2.Zero, Scale: 0.75f).noGravity = true;
        }
    }
}