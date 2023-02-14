using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Items.Materials;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Items.Armor.StormcloudArmor
{
    [AutoloadEquip(EquipType.Body)]
    public class StormcloudArmor : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 20;
            Item.value = 7500;
            Item.defense = 3;
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Summon) += 0.04f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<StormChunk>(), 12)
                .AddIngredient(ItemID.RainCloud, 16)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class StormcloudGreaves : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 18;
            Item.value = 7500;
            Item.defense = 2;
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.10f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<StormChunk>(), 10)
                .AddIngredient(ItemID.RainCloud, 12)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    [AutoloadEquip(EquipType.Head)]
    public class StormcloudMask : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 22;
            Item.value = 7500;
            Item.defense = 2;
            Item.rare = ItemRarityID.Blue;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ItemType<StormcloudArmor>() && legs.type == ItemType<StormcloudGreaves>();
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage(DamageClass.Summon) += 0.04f;
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = Language.GetTextValue("Mods.Polarities.ArmorSetBonus." + Name);
            player.maxMinions += 1;
            player.GetModPlayer<PolaritiesPlayer>().stormcloudArmor = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemType<StormChunk>(), 8)
                .AddIngredient(ItemID.RainCloud, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class StormcloudArmorRaincloud : ModProjectile
    {
        public override string Texture => "Polarities/NPCs/StormCloudfish/StormCloudfishRaincloud";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("{$Mods.Polarities.ProjectileName.StormCloudfishRaincloud}");

            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width = 56;
            Projectile.height = 22;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.timeLeft = 480;
        }

        public override void AI()
        {
            if (Projectile.timeLeft % 25 == 0 && Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center.X + Main.rand.NextFloat(-20, 20), Projectile.Center.Y, 0, 6, ProjectileType<StormcloudArmorRaincloudRain>(), Projectile.damage, 1f, Main.myPlayer, 0, 0);
            }
            Projectile.alpha = Math.Max(4 * Projectile.timeLeft - (4 * 480 - 255), Math.Max(0, 255 - 4 * Projectile.timeLeft));

            Projectile.frameCounter++;
            if (Projectile.frameCounter == 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 4;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Color color = Projectile.GetAlpha(lightColor);
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(3, 4, 0, Projectile.frame);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, color, Projectile.rotation, frame.Size() / 2, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }

    public class StormcloudArmorRaincloudRain : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainFriendly;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("{$Mods.Polarities.ProjectileName.CloudfishRain}");

            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.MinionShot[Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.RainFriendly);
            Projectile.DamageType = DamageClass.Summon;
        }
    }
}