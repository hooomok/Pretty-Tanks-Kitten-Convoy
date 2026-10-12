using UnityEngine;

namespace PrettyTanks
{
    public static class PowerUpLook
    {
        private static readonly Color[] Colors =
        {
            new Color(1f, 0.35f, 0.65f),
            new Color(0.65f, 0.40f, 0.95f),
            new Color(1f, 0.55f, 0.20f),
            new Color(1f, 0.88f, 0.25f),
            new Color(0.35f, 0.75f, 1f),
            new Color(0.97f, 0.97f, 1f),
            new Color(0.40f, 0.90f, 0.70f)
        };

        private static readonly string[] Titles =
        {
            "Розовый — тройной выстрел",
            "Фиолетовый — лазер",
            "Оранжевый — бомба",
            "Жёлтый — ускорение",
            "Голубой — щит",
            "Белый — лечение фургона",
            "Мятный — ремонт танка"
        };

        private static readonly string[] Hints =
        {
            "Три снаряда за один выстрел.",
            "Луч пробивает препятствия.",
            "Нажми Пробел до окончания таймера бонуса.",
            "Быстрее движение; снимает лимит числа рикошетов.",
            "Временно защищает твой танк от урона.",
            "Постепенно восстанавливает здоровье фургона.",
            "Чинит модуль; если чинить нечего — +25 HP танку."
        };

        public static Color ColorFor(PowerKind kind) => Colors[(int)kind];
        public static string Title(PowerKind kind) => Titles[(int)kind];
        public static string Hint(PowerKind kind) => Hints[(int)kind];
    }
}