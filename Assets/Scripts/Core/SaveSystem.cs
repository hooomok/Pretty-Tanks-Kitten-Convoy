using UnityEngine;

namespace PrettyTanks
{
    public static class SaveSystem
    {
        private const string Key = "PrettyTanks.Unlocked.v1";

        public static int Unlocked => Mathf.Clamp(PlayerPrefs.GetInt(Key, 0), 0, GameManager.Names.Length - 1);

        public static void Complete(int level)
        {
            PlayerPrefs.SetInt(Key, Mathf.Max(Unlocked, Mathf.Min(GameManager.Names.Length - 1, level + 1)));
            PlayerPrefs.Save();
        }
    }
}
