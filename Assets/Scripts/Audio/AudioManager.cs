using UnityEngine;

namespace PrettyTanks
{
    public sealed class AudioManager : MonoBehaviour
    {
        [SerializeField, Range(0, 1)]
        private float musicVolume = 0.22f;
        [SerializeField, Range(0, 1)]
        private float effectsVolume = 0.3f;
        private AudioSource music;
        private AudioSource sfx;
        private AudioClip calm;
        private AudioClip alarm;
        private AudioClip shot;
        private AudioClip hit;
        private AudioClip meow;
        private AudioClip repair;
        private MusicConfig config;

        private void Awake()
        {
            config = Resources.Load<MusicConfig>("Music");
            music = gameObject.AddComponent<AudioSource>();
            music.playOnAwake = false;
            music.spatialBlend = 0f;
            music.loop = true;
            sfx = gameObject.AddComponent<AudioSource>();
            calm = Resources.Load<AudioClip>("Audio/kawaii_loop");
            alarm = Resources.Load<AudioClip>("Audio/alarm_loop");
            shot = Resources.Load<AudioClip>("Audio/shot");
            hit = Resources.Load<AudioClip>("Audio/hit");
            meow = Resources.Load<AudioClip>("Audio/meow");
            repair = Resources.Load<AudioClip>("Audio/repair");
            UpdateMusic();
        }

        private void OnEnable()
        {
            EventBus.Effect += Effect;
        }

        private void OnDisable()
        {
            EventBus.Effect -= Effect;
        }

        private void Update()
        {
            UpdateMusic();
        }

        private void UpdateMusic()
        {
            var game = GameManager.I;
            bool inMission = game && game.Started;
            AudioClip desired = calm;
            if (config)
            {
                AudioClip selected = inMission ? config.ForLevel(game.LevelIndex) : config.MenuMusic;
                if (selected) desired = selected;
                music.volume = config.MusicVolume;
                effectsVolume = config.EffectsVolume;
            }
            else
            {
                music.volume = musicVolume;
            }

            // Сохраняем прежнюю тревожную музыку; её можно отключить в Music.asset.
            bool low = inMission && game.Truck &&
                       game.Truck.Health.Current < game.Truck.Health.Maximum * 0.3f;
            if (low && (!config || config.UseAlarmMusic))
            {
                AudioClip alert = config && config.AlarmMusic ? config.AlarmMusic : alarm;
                if (alert) desired = alert;
            }

            // Не перезапускаем один и тот же трек каждый кадр или при паузе.
            if (music.clip == desired) return;
            music.Stop();
            music.clip = desired;
            if (desired) music.Play();
        }

        private float nextHit;
        private float nextMeow;

        private void Effect(Vector2 point, string kind)
        {
            AudioClip clip = null;
            if (kind == "shot")
            {
                clip = shot;
            }
            else if (kind == "meow" && Time.time >= nextMeow)
            {
                clip = meow;
                nextMeow = Time.time + 0.6f;
            }
            else if (kind == "repair")
            {
                clip = repair;
            }
            else if ((kind == "hit" || kind == "burst" || kind == "bomb") && Time.time >= nextHit)
            {
                clip = hit;
                nextHit = Time.time + 0.06f;
            }
            if (clip)
            {
                sfx.PlayOneShot(clip, effectsVolume);
            }
        }

        public void ToggleMute()
        {
            AudioListener.volume = AudioListener.volume > 0 ? 0 : 1;
        }
    }
}
