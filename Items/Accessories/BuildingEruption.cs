using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Polarities.Effects;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Polarities.Items.Accessories
{
    public class BuildingEruption : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = (1);
        }

        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 46;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Expert;
            Item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PolaritiesPlayer>().convectiveDash = true;
        }
    }

    public class BuildingEruptionDrawLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new Between(PlayerDrawLayers.BeetleBuff, PlayerDrawLayers.EyebrellaCloud);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            return drawInfo.drawPlayer.GetModPlayer<PolaritiesPlayer>().convectiveDashing;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            int trailLength = Math.Min(10, PolaritiesSystem.timer - drawInfo.drawPlayer.GetModPlayer<PolaritiesPlayer>().convectiveDashStartTime);
            for (int i = 0; i < trailLength; i++)
            {
                float progress = i / 10f;
                Color color = ModUtils.ConvectiveFlameColor((1 - progress) * (1 - progress) * 0.5f) * (1 - progress);
                float scale = (1 - progress) * 0.25f;
                drawInfo.DrawDataCache.Add(new DrawData(Textures.Glow256.Value, drawInfo.drawPlayer.MountedCenter - Main.screenPosition - drawInfo.drawPlayer.velocity * i, Textures.Glow256.Frame(), color, drawInfo.drawPlayer.velocity.ToRotation(), Textures.Glow256.Size() / 2, new Vector2(0.25f, scale), SpriteEffects.None, 0));
            }
        }
    }

    public class BuildingEruptionChargingParticle : Particle
    {
        public override string Texture => "Polarities/NPCs/ConvectiveWanderer/ConvectiveWandererHeatVortex_Pulse";

        public override void Initialize()
        {
            Color = Color.White;
            Glow = true;
            TimeLeft = 60;
        }

        public float ScaleIncrement = -1f;
        public int playerOwner = -1;

        public override void AI()
        {
            Scale += ScaleIncrement / MaxTimeLeft;
            Alpha = 1 - (float)Math.Pow(TimeLeft / (float)MaxTimeLeft, 2);

            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);

            if (playerOwner > -1 && Main.player[playerOwner].GetModPlayer<PolaritiesPlayer>().convectiveDashing) Kill();
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            Asset<Texture2D> particleTexture = particleTextures[Type];

            Vector2 drawPosition = Position - Main.screenPosition;
            if (playerOwner > -1) drawPosition += Main.player[playerOwner].Center;

            Color drawColor = Glow ? Color * Alpha : Lighting.GetColor(drawPosition.ToTileCoordinates()).MultiplyRGBA(Color * Alpha);
            spritebatch.Draw(particleTexture.Value, drawPosition, particleTexture.Frame(), drawColor, Rotation, particleTexture.Size() / 2, Scale, SpriteEffects.None, 0f);
        }
    }
}