using System.Collections.Generic;
using UnityEngine;

namespace KhaleefWorld
{
    public enum Sfx { Tap, Correct, Wrong, Coin, Jump, Fanfare, Flip }

    /// <summary>
    /// Music + sound effects. Sound effects are synthesised at start-up so the MVP has friendly
    /// sounds with zero audio assets. Replace with recorded kompang / gamelan / sape clips later
    /// (assign StateDefinition.music for each state).
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        const int Rate = 44100;
        GameManager gm;
        AudioSource sfxSource, musicSource;
        readonly Dictionary<Sfx, AudioClip> clips = new Dictionary<Sfx, AudioClip>();

        public void Init(GameManager g)
        {
            gm = g;
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.45f;
            musicSource.playOnAwake = false;

            clips[Sfx.Tap] = Tone("tap", new[] { 880f }, 0.06f, 0.25f);
            clips[Sfx.Correct] = Tone("correct", new[] { 660f, 880f, 1320f }, 0.09f, 0.35f);
            clips[Sfx.Wrong] = Tone("wrong", new[] { 300f, 240f }, 0.12f, 0.25f);
            clips[Sfx.Coin] = Tone("coin", new[] { 1320f, 1760f }, 0.05f, 0.3f);
            clips[Sfx.Jump] = Tone("jump", new[] { 440f, 660f }, 0.05f, 0.2f);
            clips[Sfx.Fanfare] = Tone("fanfare", new[] { 523f, 659f, 784f, 1047f }, 0.13f, 0.35f);
            clips[Sfx.Flip] = Tone("flip", new[] { 700f }, 0.04f, 0.2f);
        }

        bool SoundOn => gm == null || gm.Save == null || gm.Save.parental.soundOn;

        public void Play(Sfx s)
        {
            if (!SoundOn || sfxSource == null) return;
            if (clips.TryGetValue(s, out var clip)) sfxSource.PlayOneShot(clip);
        }

        public void PlayMusic(AudioClip clip)
        {
            if (musicSource == null) return;
            if (clip == null || !SoundOn) { musicSource.Stop(); return; }
            if (musicSource.clip == clip && musicSource.isPlaying) return;
            musicSource.clip = clip;
            musicSource.Play();
        }

        public void RefreshMute()
        {
            if (musicSource == null) return;
            if (!SoundOn) musicSource.Stop();
            else if (musicSource.clip != null && !musicSource.isPlaying) musicSource.Play();
        }

        /// <summary>Soft sine "marimba" notes played one after another.</summary>
        static AudioClip Tone(string name, float[] notes, float noteLength, float volume)
        {
            int perNote = Mathf.RoundToInt(Rate * noteLength);
            var data = new float[perNote * notes.Length];
            for (int n = 0; n < notes.Length; n++)
            {
                for (int i = 0; i < perNote; i++)
                {
                    float t = i / (float)Rate;
                    float env = Mathf.Exp(-6f * i / perNote) * Mathf.Clamp01(i / 200f);
                    float s = Mathf.Sin(2 * Mathf.PI * notes[n] * t) + 0.3f * Mathf.Sin(4 * Mathf.PI * notes[n] * t);
                    data[n * perNote + i] = s * env * volume;
                }
            }
            var clip = AudioClip.Create(name, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
