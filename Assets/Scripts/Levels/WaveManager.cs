using System.Collections.Generic;
using UnityEngine;

namespace PrettyTanks
{
    /// <summary>Планирует волны и выдаёт врагов по одному после предупреждения.</summary>
    public sealed class WaveManager : MonoBehaviour
    {
        [SerializeField]
        private float crateInterval = 17f;

        private readonly List<TankController> enemies = new List<TankController>();
        private DifficultyProfile difficulty;
        private PowerUpConfig powerUps;
        private bool logBalance;
        private float nextWave;
        private float nextCrate;
        private float nextSpawn;
        private float spawnAt;
        private int pendingEnemies;
        private int waveSpawnIndex;
        private int level;
        private bool warningActive;
        private Vector2 warnedPosition;
        private GameObject warningMarker;

        public int Wave { get; private set; }

        public float SecondsUntilWave => Mathf.Max(0f, nextWave - Time.time);

        public DifficultyProfile Difficulty => difficulty;

        public int AliveCount
        {
            get {
                enemies.RemoveAll(tank => !tank);
                return enemies.Count;
            }
        }

        public void Init(int levelIndex)
        {
            level = levelIndex;
            var balance = Resources.Load<DifficultyData>("Difficulty");
            difficulty = balance.ForLevel(level);
            logBalance = balance.LogBalance;
            powerUps = Resources.Load<PowerUpConfig>("PowerUps");
            nextWave = Time.time + difficulty.FirstWaveDelay;
            nextCrate = Time.time + (powerUps ? powerUps.FirstCrateDelay : 8f);
            if (logBalance)
            {
                Debug.Log($"[Balance] Level {level + 1}: enemies={difficulty.CountForWave(1)}, " +
                          $"spawn={difficulty.SpawnSpacing:F2}s, wave={difficulty.WaveInterval:F1}s, " +
                          $"HP x{difficulty.HealthMultiplier:F2}, damage x{difficulty.DamageMultiplier:F2}");
            }
        }

        private void Update()
        {
            if (!GameManager.I.Playing)
            {
                return;
            }

            UpdateWaveSchedule();
            UpdateSpawnQueue();
            UpdateCrates();
        }

        private void UpdateWaveSchedule()
        {
            // Задержанная волна не создаёт очередь дополнительных волн.
            if (Time.time < nextWave || pendingEnemies > 0 || warningActive)
            {
                return;
            }

            if (AliveCount >= difficulty.MaxAlive)
            {
                nextWave = Time.time + 3f;
                return;
            }

            Wave++;
            pendingEnemies = difficulty.CountForWave(Wave);
            waveSpawnIndex = 0;
            nextSpawn = Time.time;
            nextWave = Time.time + difficulty.WaveInterval;
            EventBus.Say("Волна " + Wave + ". Следи за розовыми метками появления!");
        }

        private void UpdateSpawnQueue()
        {
            if (warningActive)
            {
                if (Time.time < spawnAt)
                {
                    return;
                }

                // За время предупреждения игрок мог подъехать к метке.
                bool canSpawn = AliveCount < difficulty.MaxAlive && IsSafeSpawn(warnedPosition);
                RemoveWarning();
                if (canSpawn)
                {
                    Spawn(ChooseEnemy(), warnedPosition);
                    pendingEnemies--;
                    waveSpawnIndex++;
                }

                nextSpawn = Time.time + difficulty.SpawnPause;
                return;
            }

            if (pendingEnemies <= 0 || Time.time < nextSpawn || AliveCount >= difficulty.MaxAlive)
            {
                return;
            }

            if (!TryFindSpawn(out Vector2 position))
            {
                nextSpawn = Time.time + 2f;
                return;
            }

            warnedPosition = position;
            warningActive = true;
            spawnAt = Time.time + difficulty.WarningSeconds;
            warningMarker = new GameObject("Предупреждение о появлении");
            warningMarker.transform.SetParent(GameManager.I.World);
            warningMarker.transform.position = position;
            Art.Part("Кольцо", warningMarker.transform, "round", new Color(1f, 0.4f, 0.65f, 0.45f), Vector2.zero,
                     Vector2.one * 1.7f, 2);
            Art.Part("Звезда", warningMarker.transform, "star", new Color(1f, 0.42f, 0.62f), Vector2.zero,
                     Vector2.one * 0.5f, 3);
        }

        private EnemyKind ChooseEnemy()
        {
            // В поздних волнах гарантированно показываем оба новых типа.
            if (difficulty.Allows(EnemyKind.Mother, Wave) && waveSpawnIndex == 0)
            {
                return EnemyKind.Mother;
            }
            if (difficulty.Allows(EnemyKind.Ghost, Wave) && waveSpawnIndex == 1)
            {
                return EnemyKind.Ghost;
            }
            if (Random.value >= difficulty.StrongShare)
            {
                return EnemyKind.Scooter;
            }

            // Только разрешённые на этом этапе типы; доля сильных растёт по уровням.
            var candidates = new List<EnemyKind>();
            foreach (EnemyKind kind in new[] { EnemyKind.Sniper, EnemyKind.Ricochet, EnemyKind.Mother, EnemyKind.Ghost })
            {
                if (difficulty.Allows(kind, Wave)) candidates.Add(kind);
            }
            return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : EnemyKind.Scooter;
        }

        private bool TryFindSpawn(out Vector2 position)
        {
            var points = GameManager.I.Level.SpawnPoints;
            for (int attempt = 0; attempt < 32; attempt++)
            {
                int index = (Wave + waveSpawnIndex + attempt) % points.Count;
                Vector2 candidate = points[index] + Random.insideUnitCircle * 1.1f;
                if (IsSafeSpawn(candidate))
                {
                    position = candidate;
                    return true;
                }
            }

            position = default;
            return false;
        }

        private bool IsSafeSpawn(Vector2 position)
        {
            var game = GameManager.I;
            if (Mathf.Abs(position.x) > 13f || Mathf.Abs(position.y) > 7f)
            {
                return false;
            }
            if (game.Level.Blocked(position) || Physics2D.OverlapCircle(position, 0.8f) != null)
            {
                return false;
            }

            return Vector2.Distance(position, game.Player.transform.position) >= difficulty.SafeSpawnDistance &&
                   Vector2.Distance(position, game.Truck.transform.position) >= difficulty.SafeSpawnDistance;
        }

        public void Spawn(EnemyKind kind, Vector2 point)
        {
            TankController tank = TankFactory.Create(false, point, GameManager.I.World);
            EnemyData data = Resources.Load<EnemyData>("Enemy_" + kind);
            tank.gameObject.AddComponent<AIBrain>().Init(data, difficulty);
            enemies.Add(tank);
        }

        public void SpawnMinion(Vector2 parentPosition)
        {
            // Детёныши используют тот же лимит, что и обычная волна.
            if (AliveCount >= difficulty.MaxAlive)
            {
                return;
            }

            for (int attempt = 0; attempt < 16; attempt++)
            {
                Vector2 candidate = parentPosition + Random.insideUnitCircle.normalized * 1.7f;
                if (!IsSafeSpawn(candidate))
                {
                    continue;
                }

                TankController minion = TankFactory.Create(false, candidate, GameManager.I.World);
                minion.transform.localScale = Vector3.one * 0.65f;
                minion.gameObject.AddComponent<AIBrain>().Init(Resources.Load<EnemyData>("Enemy_Scooter"),
                                                               difficulty);
                minion.Health.Init(Resources.Load<EnemyData>("Enemy_Mother").MinionHealth * difficulty.HealthMultiplier);
                enemies.Add(minion);
                return;
            }
        }

        private void UpdateCrates()
        {
            if (Time.time < nextCrate)
            {
                return;
            }

            nextCrate = Time.time + (powerUps ? powerUps.CrateInterval : crateInterval);
            if (!powerUps)
            {
                CreateCrate((PowerKind)Random.Range(0, 7), GameManager.I.Level.FreePoint());
            }
            else if (powerUps.TryPick(out PowerKind kind))
            {
                CreateCrate(kind, GameManager.I.Level.FreePoint());
            }
        }

        public void CreateCrate(PowerKind kind, Vector2 point)
        {
            var crate = new GameObject("Подарок: " + kind);
            crate.transform.SetParent(GameManager.I.World);
            crate.transform.position = point;
            Color color = PowerUpLook.ColorFor(kind);
            Art.Part("Коробка", crate.transform, "gift", color, Vector2.zero, Vector2.one * 0.8f, 3);
            var collider = crate.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one * 0.8f;
            collider.isTrigger = true;
            crate.AddComponent<PowerUpCrate>().Init(kind);
        }

        private void RemoveWarning()
        {
            warningActive = false;
            if (warningMarker)
            {
                Destroy(warningMarker);
            }
        }

        private void OnDisable()
        {
            RemoveWarning();
        }
    }
}
