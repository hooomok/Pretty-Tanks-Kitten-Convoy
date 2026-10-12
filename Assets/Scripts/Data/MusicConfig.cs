using UnityEngine;

namespace PrettyTanks
{
    /// <summary>Треки выбираются по номеру миссии, а не по повторяющейся карте.</summary>
    [CreateAssetMenu(menuName = "Pretty Tanks/Music")]
    public sealed class MusicConfig : ScriptableObject
    {
        [SerializeField] private AudioClip menuMusic;
        [Header("Перетащите сюда треки из окна Project")]
        [SerializeField, InspectorName("Level 1 Music")] private AudioClip level1Music;
        [SerializeField, InspectorName("Level 2 Music")] private AudioClip level2Music;
        [SerializeField, InspectorName("Level 3 Music")] private AudioClip level3Music;
        [SerializeField, InspectorName("Level 4 Music")] private AudioClip level4Music;
        [SerializeField, InspectorName("Level 5 Music")] private AudioClip level5Music;
        [SerializeField, InspectorName("Level 6 Music")] private AudioClip level6Music;
        [Header("Звук")]
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.22f;
        [SerializeField, Range(0f, 1f)] private float effectsVolume = 0.3f;
        [Tooltip("Заменять музыку миссии тревожной, когда HP фургона ниже 30%.")]
        [SerializeField] private bool useAlarmMusic = true;
        [SerializeField] private AudioClip alarmMusic;
        public AudioClip MenuMusic => menuMusic;
        public AudioClip AlarmMusic => alarmMusic;
        public bool UseAlarmMusic => useAlarmMusic;
        public float MusicVolume => Mathf.Clamp01(musicVolume);
        public float EffectsVolume => Mathf.Clamp01(effectsVolume);
        public AudioClip ForLevel(int index)
        {
            switch (index)
            {
                case 0: return level1Music;
                case 1: return level2Music;
                case 2: return level3Music;
                case 3: return level4Music;
                case 4: return level5Music;
                case 5: return level6Music;
                default: return null;
            }
        }
    }
}
