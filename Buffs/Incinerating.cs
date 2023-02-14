using Microsoft.Xna.Framework;
using Polarities.Effects;
using Polarities.NPCs;
using System;
using Terraria;
using Terraria.ModLoader;

namespace Polarities.Buffs
{
    public class Incinerating : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<PolaritiesPlayer>().incineration = player.buffTime[buffIndex];

            for (int i = 0; i < 4; i++)
            {
                ParticleLayer layer = i < 2 ? ParticleLayer.BeforePlayersAdditive : ParticleLayer.AfterLiquidsAdditive;
                Vector2 position = player.position + new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat()) * player.Size;

                float angling = Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);
                Vector2 velocity = new Vector2(0, -Main.rand.NextFloat(6f, 12f) * (float)Math.Pow(Math.Cos(angling), 4)).RotatedBy(angling) + player.velocity;

                int effectiveBuffTime = Math.Min(120, player.buffTime[buffIndex]);

                layer.Add(Particle.NewParticle<IncineratingParticle>(position, velocity, 0f, 0f, Scale: Main.rand.NextFloat(0.5f, 1f), TimeLeft: Main.rand.Next(effectiveBuffTime * 3 / 4, effectiveBuffTime)));
            }
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<PolaritiesNPC>().incineration = npc.buffTime[buffIndex];

            for (int i = 0; i < 4; i++)
            {
                ParticleLayer layer = i < 2 ? ParticleLayer.BeforeNPCsAdditive : ParticleLayer.AfterLiquidsAdditive;
                Vector2 position = npc.position + new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat()) * npc.Size;

                float angling = Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);
                Vector2 velocity = new Vector2(0, -Main.rand.NextFloat(6f, 12f) * (float)Math.Pow(Math.Cos(angling), 4)).RotatedBy(angling) + npc.velocity;

                int effectiveBuffTime = Math.Min(120, npc.buffTime[buffIndex] / 5);

                layer.Add(Particle.NewParticle<IncineratingParticle>(position, velocity, 0f, 0f, Scale: Main.rand.NextFloat(0.5f, 1f), TimeLeft: Main.rand.Next(effectiveBuffTime * 3 / 4, effectiveBuffTime)));
            }
        }

        public override bool ReApply(Player player, int time, int buffIndex)
        {
            player.buffTime[buffIndex] += time;
            return false;
        }

        public override bool ReApply(NPC npc, int time, int buffIndex)
        {
            npc.buffTime[buffIndex] += time;
            return false;
        }
    }

    public class IncineratingParticle : Particle
    {
        public override string Texture => "Polarities/Textures/Glow58";

        public override void Initialize()
        {
            Color = Color.White;
            Glow = true;
        }

        private const float maxPossibleTimeLeft = 90; //it can actually go higher than this but this is the cap for visual effects

        public override void AI()
        {
            Velocity *= 0.95f;

            Scale = InitialScale * (float)(1 - Math.Pow(1 - Math.Min(1, TimeLeft / maxPossibleTimeLeft), 2));

            Color = ModUtils.ConvectiveFlameColor((float)Math.Pow(Math.Min(1, TimeLeft / maxPossibleTimeLeft), 2));
            Alpha = TimeLeft / (float)MaxTimeLeft;
        }
    }
}

