using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.Bestiary;
using Terraria.Localization;
using ReLogic.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;

namespace Polarities
{
    public class Sounds : ILoadable
    {
        public static SoundStyle Rattle;

        public static SoundStyle ConvectiveWandererRoar;
        public static SoundStyle ConvectiveWandererFlamethrowerStart;
        public static SoundStyle StarConstructScream;
        public static SoundStyle StarConstructRoar;

        public static LoopedSound ConvectiveWandererFlamethrowerLoop;

        public void Load(Mod mod)
        {
            Rattle = new SoundStyle("Polarities/Sounds/Rattle") { Volume = 0.7f, PitchVariance = 0.5f };

            ConvectiveWandererRoar = new SoundStyle("Polarities/Sounds/ConvectiveWandererRoar");
            ConvectiveWandererFlamethrowerStart = new SoundStyle("Polarities/Sounds/ConvectiveWandererFlamethrowerStart");

            StarConstructScream = new SoundStyle("Polarities/Sounds/StarConstructScream");
            StarConstructRoar = new SoundStyle("Polarities/Sounds/StarConstructRoar");

            //sound loops
            ConvectiveWandererFlamethrowerLoop = new LoopedSound(new SoundStyle("Polarities/Sounds/ConvectiveWandererFlamethrowerLoop"));
        }

        public void Unload()
        {
            ConvectiveWandererFlamethrowerLoop = null;
        }

        //adapted from vanilla sound playing
        public static SlotId PlaySoundWith(SoundStyle style, Vector2? position = null, float volume = 1f)
        {
            if (Main.dedServ || !SoundEngine.IsAudioSupported)
            {
                return SlotId.Invalid;
            }
            if (position.HasValue && Vector2.DistanceSquared(Main.screenPosition + new Vector2((float)(Main.screenWidth / 2), (float)(Main.screenHeight / 2)), position.Value) > 100000000f)
            {
                return SlotId.Invalid;
            }
            if (style.PlayOnlyIfFocused && !Main.hasFocus)
            {
                return SlotId.Invalid;
            }
            if (!Program.IsMainThread)
            {
                TaskCompletionSource<SlotId> taskCompletionSource = new TaskCompletionSource<SlotId>();
                SoundStyle styleCopy = style;
                Main.QueueMainThreadAction((Action)delegate
                {
                    taskCompletionSource.SetResult(PlayWith_Inner(styleCopy, position, volume));
                });
                taskCompletionSource.Task.Wait();
                return taskCompletionSource.Task.Result;
            }
            return PlayWith_Inner(in style, position, volume);
        }

        static SlotId PlayWith_Inner(in SoundStyle style, Vector2? position, float volume)
        {
            SlotVector<ActiveSound> trackedSounds = (SlotVector<ActiveSound>)typeof(SoundPlayer).GetField("_trackedSounds", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(SoundEngine.SoundPlayer);

            int maxInstances = style.MaxInstances;
            if (maxInstances > 0)
            {
                int instanceCount = 0;
                foreach(SlotVector<ActiveSound>.ItemPair activeSoundPair in trackedSounds)
                {
                    ActiveSound activeSound = activeSoundPair.Value;
                    if (activeSound.IsPlaying && style.IsTheSameAs(activeSound.Style) && ++instanceCount >= maxInstances)
                    {
                        if (style.SoundLimitBehavior != SoundLimitBehavior.ReplaceOldest)
                        {
                            return SlotId.Invalid;
                        }
                        activeSound.Sound.Stop(true);
                    }
                }
            }
            SoundStyle styleCopy = style;
            /*if ((bool)typeof(SoundStyle).GetProperty("UsesMusicPitch", BindingFlags.Instance | BindingFlags.NonPublic).GetMethod.Invoke(style, null))
            {
                styleCopy.Pitch += Main.musicPitch;
            }*/
            ActiveSound value = new ActiveSound(styleCopy, position) { Volume = volume };
            return trackedSounds.Add(value);
        }
    }

    //system for looped sounds based on that from overhaul
    public class LoopedSound : ILoadable
    {
        public SoundStyle Style { get; private set; }
        public SlotId SlotId { get; private set; }

        public bool Active { get; private set; }

        public LoopedSound(SoundStyle Style)
        {
            Style.IsLooped = true;
            Style.PlayOnlyIfFocused = true;
            Style.MaxInstances = 1;
            Style.SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest;
            this.Style = Style;
            SlotId = SlotId.Invalid;
            Active = false;

            loopedSounds.Add(this);
        }

        void Reset()
        {
            if (!Active && SlotId.IsValid)
            {
                SoundEngine.TryGetActiveSound(SlotId, out ActiveSound sound);

                sound?.Stop();
                SlotId = SlotId.Invalid;
            }
            else
            {
                Active = false;
            }
        }

        //TODO: Support for pitch variation
        public void UpdateWith(float volume, Vector2? position = null)
        {
            SoundEngine.TryGetActiveSound(SlotId, out ActiveSound sound);

            if (volume > 0)
            {
                Active = true;

                if (sound == null)
                {
                    //TODO: This still sometimes has hiccuping when a sound starts
                    SlotId = Sounds.PlaySoundWith(Style, position, volume);
                }
                else
                {
                    sound.Position = position;
                    sound.Volume = volume;
                }
            }
            else
            {
                Active = false;

                sound?.Stop();
                SlotId = SlotId.Invalid;
            }
        }

        static HashSet<LoopedSound> loopedSounds = new HashSet<LoopedSound>();

        public void Load(Mod mod)
        {

        }

        public void Unload()
        {
            loopedSounds = null;
        }

        public static void UpdateLoopedSounds()
        {
            foreach(LoopedSound loopedSound in loopedSounds)
            {
                loopedSound.Reset();
            }
        }
    }
}

