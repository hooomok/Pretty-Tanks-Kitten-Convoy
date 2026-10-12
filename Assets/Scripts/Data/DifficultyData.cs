using System;
using UnityEngine;

namespace PrettyTanks
{
    /// <summary>Баланс уровней. Меняется в Inspector, но не во время игры.</summary>
    [CreateAssetMenu(menuName = "Pretty Tanks/Difficulty")]
    public sealed class DifficultyData : ScriptableObject
    {
        [SerializeField]
        private DifficultyProfile[] levels = new DifficultyProfile[4];

        [Header("Прогрессия: индекс первого уровня равен 0")]
        [SerializeField] private bool useProgression = true;
        [SerializeField, Min(0.1f)] private float baseDifficulty = 0.85f;
        [SerializeField, Range(0f, 0.5f)] private float growthRate = 0.12f;
        [SerializeField, Min(0.5f), InspectorName("SpawnInterval")] private float spawnInterval = 2f;
        [SerializeField, Min(1), InspectorName("EnemiesPerWave")] private int enemiesPerWave = 2;
        [SerializeField, Min(0f), InspectorName("WaveStartDelay")] private float waveStartDelay = 8f;
        [SerializeField, Range(0f, 1f), InspectorName("WaveGrowthPerLevel")] private float waveGrowthPerLevel = 0.3f;
        [SerializeField, Min(3f)] private float baseWaveInterval = 16f;
        [SerializeField, Min(1)] private int wavesPerIncrease = 2;
        [SerializeField, Min(1)] private int baseMaxAlive = 5;
        [SerializeField, Min(1)] private int enemyCapPerWave = 8;
        [SerializeField, Range(0.2f, 2f)] private float spawnWarning = 0.75f;
        [SerializeField, Min(2f)] private float safeDistance = 5f;
        [Header("Пределы темпа и брони")]
        [SerializeField, Min(0.2f)] private float minimumSpawnInterval = 1.5f;
        [SerializeField, Min(1f)] private float minimumWaveInterval = 8f;
        [SerializeField, Min(0f)] private float minimumStartDelay = 3f;
        [SerializeField, Min(0)] private int extraEnemiesWithinLevel = 2;
        [SerializeField, Range(0.18f, 1f)] private float frontalDamageFraction = 0.3f;

        [Header("Открытие типов: номера уровней с 1")]
        [SerializeField, Min(1)] private int sniperFromLevel = 3;
        [SerializeField, Min(1)] private int ricochetFromLevel = 3;
        [SerializeField, Min(1)] private int motherFromLevel = 6;
        [SerializeField, Min(1)] private int ghostFromLevel = 6;
        [SerializeField, Range(0f, 1f)] private float strongEnemyShare = 0.35f;
        [SerializeField, Range(0f, 0.3f)] private float strongShareGrowth = 0.1f;
        [SerializeField] private bool logBalance = true;

        public bool LogBalance => logBalance;

        private DifficultyProfile BuildProgression(int index)
        {
            int level = Mathf.Max(0, index);
            float factor = Mathf.Max(0.1f, baseDifficulty) * (1f + Mathf.Max(0f, growthRate) * level);
            return DifficultyProfile.Progression(
                level, factor, Mathf.Max(1, enemiesPerWave), Mathf.Max(0f, waveGrowthPerLevel),
                Mathf.Max(0.5f, spawnInterval), Mathf.Max(0f, waveStartDelay), Mathf.Max(3f, baseWaveInterval),
                Mathf.Max(1, wavesPerIncrease), Mathf.Max(1, baseMaxAlive), Mathf.Max(1, enemyCapPerWave),
                Mathf.Clamp(spawnWarning, 0.2f, 2f), Mathf.Max(2f, safeDistance),
                sniperFromLevel, ricochetFromLevel, motherFromLevel, ghostFromLevel,
                Mathf.Clamp01(strongEnemyShare + strongShareGrowth * Mathf.Max(0, level - 2)),
                1f + Mathf.Max(0f, growthRate) * level, Mathf.Max(0.2f, minimumSpawnInterval),
                Mathf.Max(1f, minimumWaveInterval), Mathf.Max(0f, minimumStartDelay),
                Mathf.Max(0, extraEnemiesWithinLevel), Mathf.Clamp(frontalDamageFraction, 0.18f, 1f));
        }

        public DifficultyProfile ForLevel(int index)
        {
            if (useProgression)
            {
                return BuildProgression(index);
            }
            if (levels == null || levels.Length == 0)
            {
                throw new InvalidOperationException("В Difficulty не настроены уровни.");
            }

            return levels[Mathf.Clamp(index, 0, levels.Length - 1)];
        }
    }

    [Serializable]
    public sealed class DifficultyProfile
    {
        [Header("Волны")]
        [SerializeField, Min(1)]
        private float firstWaveDelay = 18f;
        [SerializeField, Min(5)]
        private float waveInterval = 26f;
        [SerializeField, Min(1)]
        private int initialEnemies = 1;
        [SerializeField, Min(1)]
        private int wavesPerIncrease = 6;
        [SerializeField, Min(1)]
        private int maxEnemiesPerWave = 2;
        [SerializeField, Min(1)]
        private int maxAlive = 3;
        [SerializeField, Min(0.5f)]
        private float spawnSpacing = 4f;
        [SerializeField, Min(1)]
        private float warningSeconds = 3f;
        [SerializeField, Min(2)]
        private float safeSpawnDistance = 7f;

        [Header("Параметры врагов")]
        [SerializeField, Range(0.1f, 2f)]
        private float healthMultiplier = 0.65f;
        [SerializeField, Range(0.1f, 2f)]
        private float damageMultiplier = 0.5f;
        [SerializeField, Range(0.1f, 2f)]
        private float speedMultiplier = 0.72f;
        [SerializeField, Min(1)]
        private float reloadMultiplier = 1.8f;
        [SerializeField, Range(0.18f, 1f)]
        private float frontalDamageFraction = 0.35f;
        [SerializeField, Min(0)]
        private int ricochetFromWave;
        [SerializeField, Min(0)]
        private int ghostFromWave;
        [SerializeField, Min(0)]
        private int motherFromWave;

        private bool progressive;
        private bool sniperAllowed = true;
        private float strongShare = 0.35f;

        public float StrongShare => strongShare;

        // Создаём профиль сессии, не меняя исходный ScriptableObject.
        public static DifficultyProfile Progression(
            int level, float factor, int count, float countGrowth, float spacing, float delay,
            float interval, int wavesPerGrowth, int alive, int waveCap, float warning, float safe,
            int sniperLevel, int ricochetLevel, int motherLevel, int ghostLevel, float share,
            float tempo, float minSpawn, float minWave, float minDelay, int extraCount, float frontDamage)
        {
            int initial = Mathf.Min(waveCap, Mathf.CeilToInt(count * (1f + countGrowth * level)));
            return new DifficultyProfile
            {
                progressive = true,
                firstWaveDelay = Mathf.Max(minDelay, delay / tempo),
                waveInterval = Mathf.Max(minWave, interval / tempo),
                initialEnemies = initial,
                wavesPerIncrease = wavesPerGrowth,
                maxEnemiesPerWave = Mathf.Min(waveCap, initial + extraCount),
                maxAlive = Mathf.Max(initial, alive + level),
                spawnSpacing = Mathf.Max(minSpawn, spacing / tempo),
                warningSeconds = Mathf.Min(warning, Mathf.Max(minSpawn, spacing / tempo)),
                safeSpawnDistance = safe,
                healthMultiplier = factor,
                damageMultiplier = factor,
                speedMultiplier = Mathf.Min(1f, factor),
                reloadMultiplier = Mathf.Max(1f, 1f / factor),
                frontalDamageFraction = frontDamage,
                sniperAllowed = level + 1 >= sniperLevel,
                ricochetFromWave = level + 1 >= ricochetLevel ? 1 : 0,
                motherFromWave = level + 1 >= motherLevel ? 1 : 0,
                ghostFromWave = level + 1 >= ghostLevel ? 1 : 0,
                strongShare = share
            };
        }

        // В новом режиме SpawnInterval — время между появлениями, включая предупреждение.
        // Старые профили сохраняют прежнее значение паузы после появления.
        public float SpawnPause => progressive ? Mathf.Max(0f, spawnSpacing - warningSeconds) : spawnSpacing;

        public float FirstWaveDelay => firstWaveDelay;

        public float WaveInterval => waveInterval;

        public int MaxAlive => maxAlive;

        public float SpawnSpacing => spawnSpacing;

        public float WarningSeconds => warningSeconds;

        public float SafeSpawnDistance => safeSpawnDistance;

        public float HealthMultiplier => healthMultiplier;

        public float DamageMultiplier => damageMultiplier;

        public float SpeedMultiplier => speedMultiplier;

        public float ReloadMultiplier => reloadMultiplier;

        public float FrontalDamageFraction => frontalDamageFraction;

        public int CountForWave(int wave)
        {
            int growth = Mathf.Max(0, wave - 1) / Mathf.Max(1, wavesPerIncrease);
            return Mathf.Min(maxEnemiesPerWave, initialEnemies + growth);
        }

        public bool Allows(EnemyKind kind, int wave)
        {
            switch (kind)
            {
            case EnemyKind.Sniper:
                return sniperAllowed;
            case EnemyKind.Ricochet:
                return ricochetFromWave > 0 && wave >= ricochetFromWave;
            case EnemyKind.Ghost:
                return ghostFromWave > 0 && wave >= ghostFromWave;
            case EnemyKind.Mother:
                return motherFromWave > 0 && wave >= motherFromWave;
            default:
                return true;
            }
        }
    }
}
