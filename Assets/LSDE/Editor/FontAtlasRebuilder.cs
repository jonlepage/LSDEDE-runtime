using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace LSDE.EditorTools
{
    /// <summary>
    /// Rebuilds the TextMesh Pro atlases so each font carries exactly the glyphs this game can
    /// display, and nothing else.
    ///
    /// <para><b>Why this exists.</b> The two CJK fonts used to be <c>Dynamic</c>: they carried no
    /// glyphs and rasterized them at runtime, which means the complete <c>.ttf</c> had to ship —
    /// 15 MB for two files, in a build meant for players without fast internet. Turning them
    /// <c>Static</c> drops the <c>.ttf</c> and ships a single atlas instead.</para>
    ///
    /// <para><b>The trap this tool closes.</b> A static font can only draw what was baked into it.
    /// The day the blueprint is re-exported with a Japanese line using a kanji that was not there
    /// before, that kanji renders as an empty box — silently, with nothing in the console. So the
    /// character set is not written down anywhere by hand: it is READ BACK from the blueprint every
    /// time this runs. Re-export the blueprint, run this, and the atlases match the text again.</para>
    ///
    /// <para>Run <see cref="Verify"/> first — it reports coverage without writing anything.</para>
    /// </summary>
    public static class FontAtlasRebuilder
    {
        /// <summary>
        /// One font to rebuild. <see cref="TakesCjk"/> splits the work: the Latin font keeps the
        /// glyph set it already has, the CJK fonts take what their locale needs.
        /// </summary>
        private sealed class Target
        {
            public string AssetPath;
            public string LocaleForCjk;
            public int AtlasSize;
            public int PointSize;
            public int Padding;

            public bool TakesCjk => LocaleForCjk != null;
        }

        private static readonly Target[] Targets =
        {
            new Target
            {
                AssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset",
                LocaleForCjk = null,
                AtlasSize = 512,
                PointSize = 32,
                Padding = 4,
            },
            new Target
            {
                AssetPath = "Assets/TextMesh Pro/Fonts/NotoSansJP-Medium SDF.asset",
                LocaleForCjk = "ja",
                AtlasSize = 1024,
                PointSize = 36,
                Padding = 3,
            },
            new Target
            {
                AssetPath = "Assets/TextMesh Pro/Fonts/NotoSansSC-Medium SDF.asset",
                LocaleForCjk = "zh",
                AtlasSize = 1024,
                PointSize = 32,
                Padding = 3,
            },
        };

        /// <summary>
        /// Below this code point a character is served by the Latin font, at or above it by a CJK
        /// font.
        /// </summary>
        private const int FirstCjkCodePoint = 0x2E80;

        // -----------------------------------------------------------------------------------
        // Menu
        // -----------------------------------------------------------------------------------

        [MenuItem("LSDE/Polices/Vérifier la couverture des glyphes", false, 10)]
        public static void Verify()
        {
            Harvest harvest = Gather();
            var report = new StringBuilder();
            report.AppendLine("=== Couverture des glyphes ===");
            report.Append(harvest.Describe());

            foreach (Target target in Targets)
            {
                HashSet<int> wanted = WantedFor(target, harvest);
                report.AppendLine(Path.GetFileNameWithoutExtension(target.AssetPath) + " : "
                    + DryRun(target, wanted, out int _));
            }

            foreach (string line in Uncovered(harvest))
            {
                report.AppendLine(line);
            }

            Debug.Log(report.ToString());
        }

        [MenuItem("LSDE/Polices/Régénérer les atlas", false, 11)]
        public static void Rebuild()
        {
            Harvest harvest = Gather();
            var report = new StringBuilder();
            report.AppendLine("=== Régénération des atlas ===");
            report.Append(harvest.Describe());

            // Nothing is written until every font has been proven to fit in one atlas. A partial
            // rebuild would leave one font baked and another still dynamic, which is the hardest
            // state to diagnose afterwards.
            var plans = new List<KeyValuePair<Target, HashSet<int>>>();
            foreach (Target target in Targets)
            {
                HashSet<int> wanted = WantedFor(target, harvest);
                report.AppendLine(Path.GetFileNameWithoutExtension(target.AssetPath) + " : "
                    + DryRun(target, wanted, out int missing));

                if (missing > 0)
                {
                    report.AppendLine("ABANDON : aucun asset modifié. Augmente AtlasSize ou baisse PointSize.");
                    Debug.LogError(report.ToString());
                    return;
                }

                plans.Add(new KeyValuePair<Target, HashSet<int>>(target, wanted));
            }

            foreach (KeyValuePair<Target, HashSet<int>> plan in plans)
            {
                report.AppendLine(Transplant(plan.Key, plan.Value));
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(report.ToString());
        }

        // -----------------------------------------------------------------------------------
        // What each font must carry
        // -----------------------------------------------------------------------------------

        private static HashSet<int> WantedFor(Target target, Harvest harvest)
        {
            var wanted = new HashSet<int>();

            if (!target.TakesCjk)
            {
                // The Latin font keeps the exact set it already has. Anything outside it already
                // falls through to the fallback font today, so reproducing the set to the glyph is
                // what makes this rebuild a pure size change and not a behaviour change.
                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(target.AssetPath);
                if (font != null && font.characterTable != null)
                {
                    foreach (TMP_Character character in font.characterTable)
                    {
                        wanted.Add((int)character.unicode);
                    }
                }

                // Plus whatever the interface and the payload need that is not CJK — a handful of
                // glyphs, and it covers an accent or a punctuation mark added to a line later.
                foreach (int code in AllHarvested(harvest))
                {
                    if (code < FirstCjkCodePoint)
                    {
                        wanted.Add(code);
                    }
                }

                return wanted;
            }

            if (harvest.PerLocale.TryGetValue(target.LocaleForCjk, out HashSet<int> fromLocale))
            {
                foreach (int code in fromLocale)
                {
                    if (code >= FirstCjkCodePoint)
                    {
                        wanted.Add(code);
                    }
                }
            }

            // A CJK character can also sit outside a localised node — a card name, a dictionary
            // key. There is no way to tell which font will be asked for it, so both fonts get it.
            foreach (int code in harvest.NonLocalised)
            {
                if (code >= FirstCjkCodePoint)
                {
                    wanted.Add(code);
                }
            }

            foreach (int code in harvest.Interface)
            {
                if (code >= FirstCjkCodePoint)
                {
                    wanted.Add(code);
                }
            }

            // Safety net: the whole CJK punctuation block and the fullwidth forms. Around 160 extra
            // glyphs, invisible in the atlas budget, and it covers any punctuation a future export
            // introduces without needing a rebuild.
            for (int code = 0x3000; code <= 0x303F; code++)
            {
                wanted.Add(code);
            }

            for (int code = 0xFF01; code <= 0xFF60; code++)
            {
                wanted.Add(code);
            }

            return wanted;
        }

        private static IEnumerable<int> AllHarvested(Harvest harvest)
        {
            foreach (int code in harvest.NonLocalised)
            {
                yield return code;
            }

            foreach (int code in harvest.Interface)
            {
                yield return code;
            }

            foreach (KeyValuePair<string, HashSet<int>> pair in harvest.PerLocale)
            {
                foreach (int code in pair.Value)
                {
                    yield return code;
                }
            }
        }

        /// <summary>Characters the payload asks for that no font in the chain can draw.</summary>
        private static IEnumerable<string> Uncovered(Harvest harvest)
        {
            var covered = new HashSet<int>();
            foreach (Target target in Targets)
            {
                foreach (int code in WantedFor(target, harvest))
                {
                    covered.Add(code);
                }
            }

            // The Latin fallback stays dynamic, so it can still rasterize anything its face holds.
            var fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset");

            foreach (KeyValuePair<string, HashSet<int>> pair in harvest.PerLocale)
            {
                foreach (int code in pair.Value)
                {
                    if (covered.Contains(code))
                    {
                        continue;
                    }

                    bool rescued = fallback != null && fallback.HasCharacter(code);
                    yield return "  [" + pair.Key + "] U+" + code.ToString("X4") + " "
                        + char.ConvertFromUtf32(code) + " "
                        + (rescued
                            ? "servi par la police de repli dynamique"
                            : "AUCUNE POLICE — s'affichera en carré vide");
                }
            }
        }

        // -----------------------------------------------------------------------------------
        // Building
        // -----------------------------------------------------------------------------------

        private static string DryRun(Target target, HashSet<int> wanted, out int missing)
        {
            missing = 0;
            Font source = SourceFontOf(target);
            if (source == null)
            {
                missing = wanted.Count;
                return "POLICE SOURCE INTROUVABLE";
            }

            TMP_FontAsset probe = MakeProbe(target, source, wanted, out missing);
            int atlases = probe.atlasTextureCount;
            DestroyProbe(probe);

            return missing == 0
                ? wanted.Count + " glyphes, " + target.AtlasSize + "x" + target.AtlasSize
                    + " @" + target.PointSize + "pt pad" + target.Padding + ", "
                    + atlases + " planche(s) — OK"
                : (wanted.Count - missing) + "/" + wanted.Count + " glyphes — IL EN MANQUE " + missing;
        }

        private static TMP_FontAsset MakeProbe(Target target, Font source, HashSet<int> wanted, out int missing)
        {
            TMP_FontAsset probe = TMP_FontAsset.CreateFontAsset(
                source,
                target.PointSize,
                target.Padding,
                GlyphRenderMode.SDFAA,
                target.AtlasSize,
                target.AtlasSize,
                AtlasPopulationMode.Dynamic,
                false);

            // Multi-atlas would silently spill into a second texture and hide the real answer to
            // "does this fit?". Off, so a set that is too big FAILS instead of quietly doubling.
            probe.isMultiAtlasTexturesEnabled = false;

            uint[] codes = wanted.OrderBy(c => c).Select(c => (uint)c).ToArray();
            probe.TryAddCharacters(codes, out uint[] notAdded, false);
            missing = notAdded == null ? 0 : notAdded.Length;
            return probe;
        }

        private static void DestroyProbe(TMP_FontAsset probe)
        {
            if (probe.material != null)
            {
                UnityEngine.Object.DestroyImmediate(probe.material);
            }

            if (probe.atlasTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(probe.atlasTexture);
            }

            UnityEngine.Object.DestroyImmediate(probe);
        }

        /// <summary>
        /// Replaces a font asset's contents in place. In place matters: the asset keeps its GUID,
        /// so the fallback chain that names it, and every material and text component pointing at
        /// it, stay wired up.
        /// </summary>
        private static string Transplant(Target target, HashSet<int> wanted)
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(target.AssetPath);
            Font source = SourceFontOf(target);
            string name = Path.GetFileNameWithoutExtension(target.AssetPath);

            // The two sub-assets that must survive: the material, which the scene references, and
            // the texture, which is about to be replaced.
            Texture2D oldAtlas = null;
            Material material = null;
            foreach (UnityEngine.Object sub in AssetDatabase.LoadAllAssetsAtPath(target.AssetPath))
            {
                if (sub is Texture2D texture)
                {
                    oldAtlas = texture;
                }
                else if (sub is Material asMaterial)
                {
                    material = asMaterial;
                }
            }

            long before = oldAtlas == null ? 0 : UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(oldAtlas);

            // CopySerialized overwrites everything, these two included. Keep them by hand.
            var fallbacks = new List<TMP_FontAsset>(
                font.fallbackFontAssetTable ?? new List<TMP_FontAsset>());
            string sourceGuid = new SerializedObject(font).FindProperty("m_SourceFontFileGUID").stringValue;

            TMP_FontAsset built = MakeProbe(target, source, wanted, out int _);
            Texture2D newAtlas = built.atlasTexture;
            Material builtMaterial = built.material;

            EditorUtility.CopySerialized(built, font);

            // CopySerialized carries the probe's blank name over too. The name is not decoration:
            // a rich text <font="LiberationSans SDF"> tag resolves through it.
            font.name = name;

            if (oldAtlas != null)
            {
                AssetDatabase.RemoveObjectFromAsset(oldAtlas);
                UnityEngine.Object.DestroyImmediate(oldAtlas, true);
            }

            newAtlas.name = name + " Atlas";
            AssetDatabase.AddObjectToAsset(newAtlas, font);

            var serialized = new SerializedObject(font);
            serialized.FindProperty("m_AtlasPopulationMode").enumValueIndex = (int)AtlasPopulationMode.Static;

            // THE line that removes the .ttf from the build. A GUID string is not a dependency, so
            // keeping it costs nothing and is how this tool finds the face again next time.
            serialized.FindProperty("m_SourceFontFile").objectReferenceValue = null;
            serialized.FindProperty("m_SourceFontFileGUID").stringValue = sourceGuid;

            SerializedProperty atlases = serialized.FindProperty("m_AtlasTextures");
            atlases.arraySize = 1;
            atlases.GetArrayElementAtIndex(0).objectReferenceValue = newAtlas;
            serialized.FindProperty("m_AtlasTextureIndex").intValue = 0;
            serialized.FindProperty("m_IsMultiAtlasTexturesEnabled").boolValue = false;
            serialized.FindProperty("m_Material").objectReferenceValue = material;

            SerializedProperty table = serialized.FindProperty("m_FallbackFontAssetTable");
            table.arraySize = fallbacks.Count;
            for (int i = 0; i < fallbacks.Count; i++)
            {
                table.GetArrayElementAtIndex(i).objectReferenceValue = fallbacks[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();

            // The shader reads the atlas size and the spread from the MATERIAL, not from the font
            // asset. _GradientScale must equal padding + 1 or every glyph renders at the wrong
            // weight — too thin, or blurred.
            if (material != null)
            {
                material.SetTexture(ShaderUtilities.ID_MainTex, newAtlas);
                material.SetFloat(ShaderUtilities.ID_TextureWidth, newAtlas.width);
                material.SetFloat(ShaderUtilities.ID_TextureHeight, newAtlas.height);
                material.SetFloat(ShaderUtilities.ID_GradientScale, target.Padding + 1);
                EditorUtility.SetDirty(material);
            }

            UnityEngine.Object.DestroyImmediate(builtMaterial);
            UnityEngine.Object.DestroyImmediate(built);
            EditorUtility.SetDirty(font);

            long after = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(newAtlas);
            return name + " : " + wanted.Count + " glyphes, planche "
                + (before / 1024) + " KB -> " + (after / 1024) + " KB, police source détachée du build";
        }

        private static Font SourceFontOf(Target target)
        {
            var serialized = new SerializedObject(
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(target.AssetPath));

            var direct = serialized.FindProperty("m_SourceFontFile").objectReferenceValue as Font;
            if (direct != null)
            {
                return direct;
            }

            // Already static: the object reference was cleared on purpose, the GUID is the way back.
            string guid = serialized.FindProperty("m_SourceFontFileGUID").stringValue;
            return string.IsNullOrEmpty(guid)
                ? null
                : AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(guid));
        }

        // -----------------------------------------------------------------------------------
        // Reading the characters the game can display
        // -----------------------------------------------------------------------------------

        private sealed class Harvest
        {
            public readonly Dictionary<string, HashSet<int>> PerLocale =
                new Dictionary<string, HashSet<int>>();

            public readonly HashSet<int> NonLocalised = new HashSet<int>();
            public readonly HashSet<int> Interface = new HashSet<int>();

            public string Describe()
            {
                var text = new StringBuilder();
                foreach (KeyValuePair<string, HashSet<int>> pair in PerLocale.OrderBy(p => p.Key))
                {
                    int cjk = pair.Value.Count(c => c >= FirstCjkCodePoint);
                    text.AppendLine("  locale " + pair.Key + " : " + pair.Value.Count
                        + " caractères dont " + cjk + " CJK");
                }

                text.AppendLine("  hors texte localisé : " + NonLocalised.Count);
                text.AppendLine("  interface (scène et prefabs) : " + Interface.Count);
                return text.ToString();
            }
        }

        private static Harvest Gather()
        {
            var harvest = new Harvest();

            string[] blueprints = AssetDatabase
                .FindAssets("t:TextAsset", new[] { "Assets/LSDEv2/blueprints" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.EndsWith(".blueprints.json", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (blueprints.Length == 0)
            {
                Debug.LogWarning(
                    "FontAtlasRebuilder : aucun *.blueprints.json sous Assets/LSDEv2/blueprints.");
            }

            foreach (string path in blueprints)
            {
                ScanJson(File.ReadAllText(path, Encoding.UTF8), harvest);
            }

            foreach (TMP_Text text in UnityEngine.Object.FindObjectsByType<TMP_Text>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                AddCodePoints(harvest.Interface, text.text);
            }

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("TextMesh Pro/Examples"))
                {
                    continue;
                }

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                foreach (TMP_Text text in prefab.GetComponentsInChildren<TMP_Text>(true))
                {
                    AddCodePoints(harvest.Interface, text.text);
                }
            }

            return harvest;
        }

        /// <summary>
        /// Attributes every string in the payload to the key that introduced it.
        ///
        /// <para>A hand-rolled scan rather than a JSON library: the only question asked of the file
        /// is "which key sits immediately before this string", which is exactly what a localised
        /// node looks like — <c>{"fr": "...", "ja": "..."}</c> — at any depth. Reading the raw text
        /// instead would lose the <c>\uXXXX</c> escapes, which is where most of the Japanese
        /// is.</para>
        /// </summary>
        private static void ScanJson(string json, Harvest harvest)
        {
            HashSet<string> locales = ReadDeclaredLocales(json);
            string lastKey = null;

            for (int i = 0; i < json.Length; i++)
            {
                if (json[i] != '"')
                {
                    continue;
                }

                string value = ReadJsonString(json, ref i);

                int after = i + 1;
                while (after < json.Length && char.IsWhiteSpace(json[after]))
                {
                    after++;
                }

                if (after < json.Length && json[after] == ':')
                {
                    lastKey = value;
                    AddCodePoints(harvest.NonLocalised, value);
                    continue;
                }

                if (lastKey != null && locales.Contains(lastKey))
                {
                    if (!harvest.PerLocale.TryGetValue(lastKey, out HashSet<int> bucket))
                    {
                        bucket = new HashSet<int>();
                        harvest.PerLocale[lastKey] = bucket;
                    }

                    AddCodePoints(bucket, value);
                }
                else
                {
                    AddCodePoints(harvest.NonLocalised, value);
                }
            }
        }

        /// <summary>
        /// The locales the payload declares, read from its own <c>locales</c> array.
        ///
        /// <para>Guessing from the shape of the key instead does not work: <c>id</c>, <c>to</c>,
        /// <c>op</c> and <c>fn</c> are all real keys in this format and all look exactly like a
        /// two-letter language tag. Asking the payload which locales it has is the only answer that
        /// stays right when a locale is added.</para>
        /// </summary>
        private static HashSet<string> ReadDeclaredLocales(string json)
        {
            var locales = new HashSet<string>(StringComparer.Ordinal);
            string lastKey = null;

            for (int i = 0; i < json.Length; i++)
            {
                if (json[i] != '"')
                {
                    continue;
                }

                string value = ReadJsonString(json, ref i);

                int after = i + 1;
                while (after < json.Length && char.IsWhiteSpace(json[after]))
                {
                    after++;
                }

                if (after < json.Length && json[after] == ':')
                {
                    lastKey = value;
                    continue;
                }

                // Every string of the "locales": [...] array shares that key as its last one.
                if (lastKey == "locales")
                {
                    locales.Add(value);
                }
            }

            if (locales.Count == 0)
            {
                Debug.LogWarning(
                    "FontAtlasRebuilder : le blueprint ne déclare aucune liste \"locales\". "
                    + "Aucun texte ne sera attribué à une langue.");
            }

            return locales;
        }

        /// <summary>
        /// Reads one JSON string starting at the opening quote, leaving <paramref name="index"/> on
        /// the closing quote. Decoding the escapes is the whole point.
        /// </summary>
        private static string ReadJsonString(string json, ref int index)
        {
            var text = new StringBuilder();
            int i = index + 1;

            while (i < json.Length && json[i] != '"')
            {
                if (json[i] != '\\')
                {
                    text.Append(json[i]);
                    i++;
                    continue;
                }

                i++;
                if (i >= json.Length)
                {
                    break;
                }

                switch (json[i])
                {
                    case 'n':
                        text.Append('\n');
                        break;
                    case 't':
                        text.Append('\t');
                        break;
                    case 'r':
                        text.Append('\r');
                        break;
                    case 'b':
                        text.Append('\b');
                        break;
                    case 'f':
                        text.Append('\f');
                        break;
                    case 'u':
                        if (i + 4 < json.Length && ushort.TryParse(
                            json.Substring(i + 1, 4),
                            NumberStyles.HexNumber,
                            CultureInfo.InvariantCulture,
                            out ushort code))
                        {
                            text.Append((char)code);
                            i += 4;
                        }

                        break;
                    default:
                        text.Append(json[i]);
                        break;
                }

                i++;
            }

            index = i;
            return text.ToString();
        }

        /// <summary>
        /// Adds the code points of a string. Walking chars would split an emoji into its two
        /// surrogates and ask the atlas for two glyphs that do not exist.
        /// </summary>
        private static void AddCodePoints(HashSet<int> into, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsHighSurrogate(text[i])
                    && i + 1 < text.Length
                    && char.IsLowSurrogate(text[i + 1]))
                {
                    into.Add(char.ConvertToUtf32(text[i], text[i + 1]));
                    i++;
                }
                else
                {
                    into.Add(text[i]);
                }
            }
        }
    }
}
