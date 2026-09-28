using System.Collections.Generic;
using UnityEngine;

namespace CastleAssault.Core
{
    public enum Language { English = 0, Spanish = 1 }

    /// <summary>
    /// Sistema de idiomas sencillo. Índice 0 = inglés, 1 = español.
    /// El idioma elegido se guarda en PlayerPrefs.
    /// </summary>
    public static class Loc
    {
        private const string PrefKey = "castle_assault_language";
        private static int _current = -1;

        private static readonly Dictionary<string, string[]> Table = new Dictionary<string, string[]>
        {
            // ── Menú principal ──
            { "title",        new[] { "CASTLE ASSAULT", "CASTLE ASSAULT" } },
            { "play",         new[] { "PLAY", "JUGAR" } },
            { "credits",      new[] { "CREDITS", "CRÉDITOS" } },
            { "back",         new[] { "BACK", "VOLVER" } },
            { "language",     new[] { "ENGLISH", "ESPAÑOL" } },
            { "hint",         new[] { "Block with [Space] and dodge the arrows",
                                      "Bloquea con [Espacio] y esquiva las flechas" } },
            { "hint_mobile",  new[] { "Tap the shield button and dodge the arrows",
                                      "Toca el botón del escudo y esquiva las flechas" } },

            // ── Tutorial ──
            { "tut_move",     new[] { "Use [A][D] or [Arrows] to move sideways",
                                      "Usa [A][D] o [Flechas] para moverte a los lados" } },
            { "tut_shield",   new[] { "Use [Space] to cover with the shield / dodge",
                                      "Usa [Espacio] para cubrirte con el escudo / esquivar" } },
            { "tut_move_mobile",   new[] { "Hold the < > buttons to move sideways",
                                           "Mantén los botones < > para moverte a los lados" } },
            { "tut_shield_mobile", new[] { "Tap the shield button to cover / dodge",
                                           "Toca el botón del escudo para cubrirte / esquivar" } },
            { "tut_go",       new[] { "Get as far as you can!",
                                      "¡Llega lo más lejos posible!" } },

            // ── HUD ──
            { "distance",     new[] { "Distance: {0} m", "Distancia: {0} m" } },
            { "shield_key",   new[] { "[Space]", "[Espacio]" } },
            { "phase1",       new[] { "Phase 1 - Get ready!", "Fase 1 - ¡Prepárate!" } },
            { "phase2",       new[] { "Phase 2 - Speed up!", "Fase 2 - ¡Velocidad!" } },
            { "phase3",       new[] { "Phase 3 - EXTREME DANGER!", "Fase 3 - ¡PELIGRO EXTREMO!" } },
            { "bonus",        new[] { "+{0}m!", "+{0}m!" } },

            // ── Game Over ──
            { "gameover",     new[] { "You fell before\nbreaking the defenses!",
                                      "¡Caíste antes de\nromper las defensas!" } },
            { "record",       new[] { "NEW RECORD!", "¡NUEVO RÉCORD!" } },
            { "stat_distance",new[] { "Distance reached: {0} meters", "Distancia alcanzada: {0} metros" } },
            { "stat_blocked", new[] { "Arrows blocked: {0}", "Flechas bloqueadas: {0}" } },
            { "stat_dodged",  new[] { "Arrows dodged: {0}", "Flechas esquivadas: {0}" } },
            { "retry",        new[] { "RETRY", "REINTENTAR" } },
            { "menu",         new[] { "MAIN MENU", "MENÚ PRINCIPAL" } },
        };

        public static Language Current
        {
            get
            {
                if (_current < 0)
                    _current = PlayerPrefs.GetInt(PrefKey, (int)Language.English);
                return (Language)_current;
            }
        }

        public static string Get(string key)
        {
            if (!Table.TryGetValue(key, out string[] values)) return key;
            return values[(int)Current];
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }

        public static void Set(Language language)
        {
            _current = (int)language;
            PlayerPrefs.SetInt(PrefKey, _current);
            PlayerPrefs.Save();
        }

        public static void Toggle()
        {
            Set(Current == Language.English ? Language.Spanish : Language.English);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _current = -1;
        }
    }
}