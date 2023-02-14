using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Polarities.Effects
{
    public class ParticleLayer : ILoadable
    {
        public static ParticleLayer BeforeNPCsAdditive;
        public static ParticleLayer BeforePlayersAdditive;
        public static ParticleLayer AfterLiquidsAdditive;
        public static ParticleLayer WarpParticles;

        public void Load(Mod mod)
        {
            //load our particle layers
            BeforeNPCsAdditive = new ParticleLayer();
            BeforePlayersAdditive = new ParticleLayer();
            AfterLiquidsAdditive = new ParticleLayer();
            WarpParticles = new ParticleLayer();

            On.Terraria.Main.UpdateParticleSystems += Main_UpdateParticleSystems;

            On.Terraria.Main.DrawPlayers_AfterProjectiles += Main_DrawPlayers_AfterProjectiles;
        }

        private void Main_UpdateParticleSystems(On.Terraria.Main.orig_UpdateParticleSystems orig, Main self)
        {
            orig(self);

            BeforeNPCsAdditive.Update();
            BeforePlayersAdditive.Update();
            AfterLiquidsAdditive.Update();
            WarpParticles.Update();
        }

        private void Main_DrawPlayers_AfterProjectiles(On.Terraria.Main.orig_DrawPlayers_AfterProjectiles orig, Main self)
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            BeforePlayersAdditive.Draw(Main.spriteBatch);
            Main.spriteBatch.End();

            orig(self);
        }

        public void Unload()
        {
            BeforeNPCsAdditive = null;
            BeforePlayersAdditive = null;
            AfterLiquidsAdditive = null;
            WarpParticles = null;
        }

        public HashSet<Particle> particles = new HashSet<Particle>();

        public void Add(Particle particle)
        {
            particles.Add(particle);
        }

        public void Update()
        {
            HashSet<Particle> toRemove = new HashSet<Particle>();
            foreach (Particle p in particles)
            {
                p.Update();
                if (!p.active) toRemove.Add(p);
            }
            foreach (Particle p in toRemove) particles.Remove(p);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Particle p in particles) p.Draw(spriteBatch);
        }
    }

    public abstract class Particle : ModTexturedType
    {
        public int Type;
        public static Dictionary<int, Asset<Texture2D>> particleTextures = new Dictionary<int, Asset<Texture2D>>();

        public Vector2 Position;
        public Vector2 Velocity;
        public float Rotation;
        public float AngularVelocity;
        public Color Color = Color.White;
        public float Alpha = 1f;
        public float InitialScale = 1f;
        public float Scale = 1f;
        public int MaxTimeLeft = 0;
        public int TimeLeft = 0;
        public bool Glow = false;

        public bool active = true;

        public static int NumTypes = 0;

        public override void Load()
        {
            Type = NumTypes;
            NumTypes++;
        }

        protected override void Register()
        {
            particleTextures[Type] = Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad);
        }

        public override void Unload()
        {
            particleTextures = null;
        }

        public Particle() { Initialize(); }

        public static T NewParticle<T>(Vector2 Position, Vector2 Velocity, float Rotation, float AngularVelocity, Color? Color = null, float? Alpha = null, float? Scale = null, int? TimeLeft = null, bool? Glow = null) where T : Particle, new()
        {
            T particle = new T
            {

                //get type from the sample instance
                Type = GetInstance<T>().Type
            };

            particle.Initialize();

            particle.Position = Position;
            particle.Velocity = Velocity;
            particle.Rotation = Rotation;
            particle.AngularVelocity = AngularVelocity;
            particle.Color = Color ?? particle.Color;
            particle.Alpha = Alpha ?? particle.Alpha;
            particle.Scale = Scale ?? particle.Scale;
            particle.TimeLeft = TimeLeft ?? particle.TimeLeft;
            particle.Glow = Glow ?? particle.Glow;

            particle.MaxTimeLeft = particle.TimeLeft;
            particle.InitialScale = particle.Scale;

            particle.OnSpawn();
            particle.Update();

            return particle;
        }


        public virtual void Initialize() { }
        public virtual void AI() { }
        public virtual void OnSpawn() { }


        public virtual void Draw(SpriteBatch spritebatch)
        {
            Asset<Texture2D> particleTexture = particleTextures[Type];

            Vector2 drawPosition = Position - Main.screenPosition;
            Color drawColor = Glow ? Color * Alpha : Lighting.GetColor(drawPosition.ToTileCoordinates()).MultiplyRGBA(Color * Alpha);

            spritebatch.Draw(particleTexture.Value, drawPosition, particleTexture.Frame(), drawColor, Rotation, particleTexture.Size() / 2, Scale, SpriteEffects.None, 0f);
        }

        public void Update()
        {
            AI();

            Position += Velocity;
            Rotation += AngularVelocity;

            TimeLeft--;
            if (TimeLeft <= 0) Kill();
        }

        public void Kill()
        {
            active = false;
        }
    }
}

