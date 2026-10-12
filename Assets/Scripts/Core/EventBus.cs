using System;
using UnityEngine;

namespace PrettyTanks
{
    public static class EventBus
    {
        public static event Action<string> Message;

        public static event Action<Vector2, string> Effect;

        public static event Action StateChanged;

        public static void Say(string value) => Message?.Invoke(value);

        public static void FX(Vector2 point, string kind) => Effect?.Invoke(point, kind);

        public static void State() => StateChanged?.Invoke();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            Message = null;
            Effect = null;
            StateChanged = null;
        }
    }
}
