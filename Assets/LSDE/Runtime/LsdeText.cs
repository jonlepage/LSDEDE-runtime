using System.Collections.Generic;
using System.Text.RegularExpressions;
using LsdeDialogEngine;
using UnityEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Turns an exported line into something ready to show: it picks the language, applies this
    /// game's fallback policy, and resolves this game's own markers.
    ///
    /// <para>The engine deliberately decides none of that. <c>LsdeUtils.GetLocalizedText</c>
    /// answers about the locale it was asked for and stops: absent means absent. And it never
    /// looks INSIDE a string — <c>{{@l3}}</c>, <c>{{#ui.hud.label}}</c> and <c>{:a2}</c> are the
    /// client game's syntax, in the client's own keys, and an engine that started interpreting
    /// them would be deciding what a game's text means.</para>
    ///
    /// <para>Both decisions therefore live here, in the game, in one place — and they share a call
    /// site so no screen can ever show one and miss the other. This is the C# counterpart of the
    /// reference demo's <c>src/engine/i18n.ts</c> and <c>src/engine/text-markers.ts</c>.</para>
    /// </summary>
    public static class LsdeText
    {
        /// <summary><c>{{@</c> … <c>}}</c> — an actor citation. The capture is a card name or id.</summary>
        private static readonly Regex ActorMarker = new Regex(
            @"\{\{@([^}]+)\}\}",
            RegexOptions.Compiled
        );

        private static DialogueEngine _activeEngine;
        private static List<string> _declaredLocales = new List<string>();
        private static string _referenceLocale = "";
        private static string _activeLocale = "";

        private static readonly Dictionary<string, Card> CardsByName =
            new Dictionary<string, Card>();
        private static readonly Dictionary<string, Card> CardsById = new Dictionary<string, Card>();
        private static readonly HashSet<string> AlreadyWarnedMarkers = new HashSet<string>();

        /// <summary>
        /// Point this module at the engine and the payload it just loaded. Call it after every
        /// <c>Init()</c>.
        ///
        /// <para><c>Locales</c>, <c>ReferenceLocale</c> and <c>Cards</c> are header fields of the
        /// file the game loaded itself, so there is nothing to ask the engine for.</para>
        /// </summary>
        /// <param name="dialogueEngine">The engine that will be told about locale switches.</param>
        /// <param name="blueprintData">The payload just parsed.</param>
        public static void UseBlueprint(
            DialogueEngine dialogueEngine,
            BlueprintExport blueprintData
        )
        {
            _activeEngine = dialogueEngine;
            _declaredLocales = blueprintData?.Locales ?? new List<string>();
            _referenceLocale = blueprintData?.ReferenceLocale ?? "";
            _activeLocale = "";

            CardsByName.Clear();
            CardsById.Clear();
            AlreadyWarnedMarkers.Clear();

            if (blueprintData?.Cards == null)
            {
                return;
            }

            foreach (var card in blueprintData.Cards)
            {
                // A card has two identities and a marker may cite either: the NAME a writer typed
                // (l3) or the id that survives a rename (0f16b34e-…, or var3 in a project written
                // natively in LSDE 2).
                if (!string.IsNullOrEmpty(card.Name))
                {
                    CardsByName[card.Name] = card;
                }
                if (!string.IsNullOrEmpty(card.Id))
                {
                    CardsById[card.Id] = card;
                }
            }
        }

        /// <summary>
        /// The locale in force. Falls back to the payload's own language before any switch.
        /// </summary>
        public static string CurrentLanguage =>
            !string.IsNullOrEmpty(_activeLocale) ? _activeLocale
            : !string.IsNullOrEmpty(_referenceLocale) ? _referenceLocale
            : "en";

        /// <summary>
        /// Switch language.
        ///
        /// <para>A code the payload does not declare is refused by the engine, which THROWS. It is
        /// checked here first and reported instead: a language dropdown is not worth crashing a
        /// scene over.</para>
        /// </summary>
        /// <param name="language">The locale code, e.g. "fr".</param>
        /// <returns>True when the switch happened.</returns>
        public static bool SetCurrentLanguage(string language)
        {
            if (_activeEngine == null || string.IsNullOrEmpty(language))
            {
                return false;
            }

            if (_declaredLocales.Count > 0 && !_declaredLocales.Contains(language))
            {
                Debug.LogWarning(
                    $"[LSDE] Locale \"{language}\" is not in this payload — it declares "
                        + $"{string.Join(", ", _declaredLocales)}."
                );
                return false;
            }

            _activeEngine.SetLocale(language);
            _activeLocale = language;
            return true;
        }

        /// <summary>
        /// Read a line, applying this game's fallback policy, and resolve its markers.
        ///
        /// <para>Only an ABSENT key counts as a missing translation. A line a writer deliberately
        /// emptied is a beat with no dialogue — a silent actor — so it stays empty. That is why
        /// the check below is on null and not on emptiness.</para>
        ///
        /// <para>The fallback is the language the payload was WRITTEN in
        /// (<c>referenceLocale</c>), because writing comes first and translation comes later: a
        /// project translated halfway still plays, and a translation added afterwards starts
        /// winning on its own. Another game might rather show <c>[MISSING: en]</c> — this is the
        /// single method to change.</para>
        /// </summary>
        /// <param name="text">The inline Text map of a block or of one option.</param>
        /// <returns>Display-ready text, or an empty string when no locale carries the line.</returns>
        public static string Localized(Dictionary<string, string> text)
        {
            var asked = LsdeUtils.GetLocalizedText(text, CurrentLanguage);
            if (asked != null)
            {
                return ResolveMarkers(asked);
            }

            if (string.IsNullOrEmpty(_referenceLocale))
            {
                return "";
            }

            var fallback = LsdeUtils.GetLocalizedText(text, _referenceLocale);
            return fallback == null ? "" : ResolveMarkers(fallback);
        }

        /// <summary>
        /// Resolve every marker this game knows how to resolve, and leave the rest alone.
        ///
        /// <para>Runs on text that is ALREADY localized: a marker is written the same way in every
        /// language, so substituting after the locale is picked means writing the resolution once
        /// instead of four times.</para>
        ///
        /// <para>Supported: <c>{{@l3}}</c> → the name the game gives that card, cited by name or
        /// by id. Left verbatim on purpose: <c>{{#ui.hud.label}}</c> (a key into the game's OWN ui
        /// strings) and <c>{:a2}</c>. A marker nobody can resolve stays on screen rather than
        /// vanishing — a writer must be able to see their typo.</para>
        /// </summary>
        /// <param name="text">Already-localized text.</param>
        /// <returns>The same text with the actor citations replaced.</returns>
        public static string ResolveMarkers(string text)
        {
            if (string.IsNullOrEmpty(text) || !text.Contains("{{"))
            {
                return text;
            }

            return ActorMarker.Replace(
                text,
                match =>
                {
                    var citation = match.Groups[1].Value.Trim();

                    if (
                        CardsByName.TryGetValue(citation, out var byName)
                        && !string.IsNullOrEmpty(byName.Name)
                    )
                    {
                        return byName.Name;
                    }

                    if (
                        CardsById.TryGetValue(citation, out var byId)
                        && !string.IsNullOrEmpty(byId.Name)
                    )
                    {
                        return byId.Name;
                    }

                    if (AlreadyWarnedMarkers.Add(match.Value))
                    {
                        Debug.LogWarning(
                            $"[LSDE] no card answers to {match.Value} — left on screen as written."
                        );
                    }

                    return match.Value;
                }
            );
        }
    }
}
