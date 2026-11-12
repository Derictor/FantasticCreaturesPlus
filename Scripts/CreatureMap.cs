using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DBDef;
using TheHoney;
using Thea2.Common;
using Thea2.General;
using Thea2.Server;
using UnityEngine;

namespace GameScript
{
    public class CreatureMap : ScriptBase
    {
        private const string PatchId = "com.derictor.fcp.creature-map";
        private static readonly string[] Species = { "Griffin", "Kirin", "Mermaid", "Phoenix", "Unicorn", "Ent", "Basilisk", "Wolf" };
        private static readonly string[] Phases = { "Child", "Young", "Apex" };
        private static readonly string[] ModelNames = Species.SelectMany(species =>
            Phases.Select(phase => "FCP" + species + "Map" + phase)).ToArray();
        private static readonly GameObject[] Templates = new GameObject[ModelNames.Length];
        private static readonly bool[] ModelFailed = new bool[ModelNames.Length];
        private static readonly object PatchLock = new object();
        private static bool patchAttempted;

        public static object WAct_FCP_MapAttributes(InterpolatedSkillAttributes skillAttributes,
            Dictionary<Tag, FInt> addons, Dictionary<Tag, float> multipliers, long changeID,
            List<Multitype<long, Tag, FInt, bool>> changeLog, Character character)
        {
            if (UseProceduralModels())
                InstallPatch();
            return Globals.CallFunction("WAct_AttributeChange", skillAttributes, addons, multipliers,
                changeID, changeLog, character);
        }

        private static bool UseProceduralModels()
        {
            // The patch is installed through a server-side passive, so guests
            // cannot rely on it. Use the same native assets on both online peers.
            return NetworkManager.instance != null && NetworkManager.IsOfflineMode();
        }

        private static void InstallPatch()
        {
            lock (PatchLock)
            {
                if (patchAttempted)
                    return;
                patchAttempted = true;

                try
                {
                    var assembly = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.GetName().Name == "0Harmony") ?? Assembly.Load("0Harmony");
                    var modern = assembly.GetType("HarmonyLib.Harmony");
                    var harmony = modern ?? assembly.GetType("Harmony.HarmonyInstance", true);
                    var harmonyMethod = assembly.GetType(modern != null ? "HarmonyLib.HarmonyMethod" : "Harmony.HarmonyMethod", true);
                    var instance = modern != null
                        ? Activator.CreateInstance(harmony, PatchId)
                        : harmony.GetMethod("Create", new[] { typeof(string) }).Invoke(null, new object[] { PatchId });

                    var original = typeof(AssetManager).GetMethod("GetCharacterModel", new[] { typeof(string) });
                    var prefix = typeof(CreatureMap).GetMethod("CreatureAssetPrefix", BindingFlags.Static | BindingFlags.NonPublic);
                    var patch = harmony.GetMethods().Single(method => method.Name == "Patch" &&
                        method.GetParameters().Length >= 4 &&
                        method.GetParameters()[0].ParameterType == typeof(MethodBase));
                    var arguments = new object[patch.GetParameters().Length];
                    arguments[0] = original;
                    arguments[1] = Activator.CreateInstance(harmonyMethod, prefix);
                    patch.Invoke(instance, arguments);
                    Debug.Log("[FCP] Creature map model hook registered.");
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("[FCP] Creature models use the native fallback: " + exception.GetBaseException().Message);
                }
            }
        }

        private static bool CreatureAssetPrefix(string name, ref GameObject __result)
        {
            int index = Array.IndexOf(ModelNames, name);
            if (index < 0 || !UseProceduralModels() || ModelFailed[index])
                return true;

            if (Templates[index] == null)
            {
                try
                {
                    Templates[index] = Build(index);
                    Debug.Log("[FCP] Built map model " + name);
                }
                catch (Exception exception)
                {
                    ModelFailed[index] = true;
                    Debug.LogWarning("[FCP] Creature model failed; using the native fallback: " + exception.Message);
                    return true;
                }
            }
            __result = Templates[index];
            return false;
        }

        private static GameObject Build(int index)
        {
            int stage = index % Phases.Length;
            string species = Species[index / Phases.Length];
            var shader = Shader.Find("Standard");
            if (shader == null)
                shader = Shader.Find("Diffuse");
            if (shader == null)
                throw new InvalidOperationException("No compatible character shader was found.");

            var root = new GameObject(ModelNames[index]);
            root.transform.position = new Vector3(0f, -10000f, 0f);
            try
            {
                if (species != "Kirin")
                {
                    BuildOther(root, species, stage, shader);
                    UnityEngine.Object.DontDestroyOnLoad(root);
                    return root;
                }

                float scale = new[] { 0.63f, 0.81f, 1f }[stage];
                var body = Pivot(root.transform, "Kirin", Vector3.zero);
                body.localScale = Vector3.one * scale;
                var gold = Tint(shader, stage == 0 ? new Color(0.76f, 0.45f, 0.19f) : new Color(0.62f, 0.33f, 0.10f));
                var cream = Tint(shader, new Color(0.90f, 0.73f, 0.42f));
                var copper = Tint(shader, new Color(0.37f, 0.18f, 0.09f));
                var dark = Tint(shader, new Color(0.11f, 0.06f, 0.04f));
                var eye = Tint(shader, new Color(0.24f, 0.78f, 0.60f));

                Oval(body, new Vector3(0, 0.73f, 0), new Vector3(0.46f, 0.49f, 0.93f), gold);
                Oval(body, new Vector3(0, 0.64f, 0.05f), new Vector3(0.36f, 0.34f, 0.77f), cream);
                Oval(body, new Vector3(0, 0.80f, 0.34f), new Vector3(0.43f, 0.48f, 0.37f), gold);
                Beam(body, new Vector3(0, 0.79f, 0.29f), new Vector3(0, 1.18f, 0.57f), 0.12f, gold);
                Oval(body, new Vector3(0, 1.19f, 0.58f), new Vector3(0.29f, 0.33f, 0.28f), gold);
                Oval(body, new Vector3(0, 1.12f, 0.76f), new Vector3(0.23f, 0.18f, 0.29f), cream);
                Oval(body, new Vector3(0, 1.11f, 0.88f), new Vector3(0.19f, 0.12f, 0.10f), dark);

                Beam(body, new Vector3(0, 0.84f, -0.41f), new Vector3(0, 0.99f, -0.76f), 0.05f, copper);
                Oval(body, new Vector3(0, 1.01f, -0.79f), new Vector3(0.15f, 0.15f, 0.23f), cream);
                for (int i = 0; i < 4; i++)
                    Oval(body, new Vector3(0, 0.97f + i * 0.09f, 0.25f + i * 0.06f),
                        new Vector3(0.16f, 0.25f, 0.18f), copper);

                var legs = new Transform[4];
                for (int side = -1; side <= 1; side += 2)
                {
                    float x = side * 0.14f;
                    Oval(body, new Vector3(x, 1.36f, 0.51f), new Vector3(0.10f, 0.22f, 0.08f), copper,
                        Quaternion.Euler(0, 0, -side * 29));
                    Oval(body, new Vector3(side * 0.135f, 1.23f, 0.68f), new Vector3(0.065f, 0.062f, 0.054f), dark);
                    Oval(body, new Vector3(side * 0.162f, 1.232f, 0.69f), new Vector3(0.022f, 0.036f, 0.027f), eye);
                    Antler(body, side, stage, cream);
                    int sideIndex = side < 0 ? 0 : 1;
                    legs[sideIndex] = Leg(body, new Vector3(x, 0.67f, 0.30f), false, gold, dark);
                    legs[sideIndex + 2] = Leg(body, new Vector3(x, 0.67f, -0.32f), true, gold, dark);
                }

                var motion = root.AddComponent<FcpCreatureMotion>();
                motion.Body = body;
                motion.Legs = legs;
                UnityEngine.Object.DontDestroyOnLoad(root);
                return root;
            }
            catch
            {
                UnityEngine.Object.Destroy(root);
                throw;
            }
        }

        private static void BuildOther(GameObject root, string species, int stage, Shader shader)
        {
            var body = Pivot(root.transform, species, Vector3.zero);
            body.localScale = Vector3.one * (stage == 0 ? 0.63f : stage == 1 ? 0.81f : 1f);
            var motion = root.AddComponent<FcpCreatureMotion>();
            motion.Body = body;

            switch (species)
            {
                case "Griffin": BuildGriffin(body, motion, stage, shader); break;
                case "Mermaid": BuildMermaid(body, motion, stage, shader); break;
                case "Phoenix": BuildPhoenix(body, motion, stage, shader); break;
                case "Unicorn": BuildUnicorn(body, motion, stage, shader); break;
                case "Ent": BuildEnt(body, motion, stage, shader); break;
                case "Basilisk": BuildBasilisk(body, motion, stage, shader); break;
                case "Wolf": BuildWolf(body, motion, stage, shader); break;
                default: throw new ArgumentException("Unknown map model: " + species);
            }
        }

        private static void BuildGriffin(Transform body, FcpCreatureMotion motion, int stage, Shader shader)
        {
            var tawny = Tint(shader, new Color(0.58f, 0.30f, 0.10f));
            var ivory = Tint(shader, new Color(0.91f, 0.80f, 0.55f));
            var dark = Tint(shader, new Color(0.28f, 0.13f, 0.06f));
            var beak = Tint(shader, new Color(0.97f, 0.62f, 0.14f));
            Oval(body, new Vector3(0, .68f, 0), new Vector3(.54f, .55f, .94f), tawny);
            Oval(body, new Vector3(0, .91f, .33f), new Vector3(.44f, .56f, .42f), ivory);
            Oval(body, new Vector3(0, 1.18f, .49f), new Vector3(.37f, .36f, .39f), ivory);
            Taper(body, new Vector3(0, 1.16f, .59f), new Vector3(0, 1.08f, .80f), .12f, beak);
            Beam(body, new Vector3(0, .76f, -.39f), new Vector3(0, .96f, -.79f), .045f, tawny);
            Oval(body, new Vector3(0, .99f, -.80f), new Vector3(.14f, .16f, .22f), dark);
            motion.Legs = FourLegs(body, .17f, .65f, .29f, -.29f, tawny, dark);
            motion.Wings = FeatherWings(body, .90f, .17f, stage, ivory, dark);
            Eyes(body, 1.22f, .62f, .14f, dark);
        }

        private static void BuildPhoenix(Transform body, FcpCreatureMotion motion, int stage, Shader shader)
        {
            var red = Tint(shader, new Color(.81f, .16f, .06f));
            var gold = Tint(shader, new Color(1f, .63f, .13f));
            var ember = Tint(shader, new Color(.47f, .055f, .035f));
            Oval(body, new Vector3(0, .83f, 0), new Vector3(.38f, .49f, .67f), red);
            Oval(body, new Vector3(0, 1.05f, .23f), new Vector3(.29f, .38f, .36f), gold);
            Oval(body, new Vector3(0, 1.23f, .39f), new Vector3(.27f, .28f, .31f), red);
            Taper(body, new Vector3(0, 1.20f, .49f), new Vector3(0, 1.13f, .68f), .08f, gold);
            Taper(body, new Vector3(0, 1.32f, .34f), new Vector3(0, 1.55f, .11f), .085f, gold);
            for (int i = -1; i <= 1; i++)
                Taper(body, new Vector3(i * .06f, .82f, -.22f),
                    new Vector3(i * .18f, .51f, -.65f - .10f * stage), .07f, i == 0 ? gold : red);
            motion.Wings = FeatherWings(body, .98f, .06f, stage, red, gold);
            motion.Hovers = true;
            Eyes(body, 1.27f, .48f, .105f, ember);
        }

        private static void BuildUnicorn(Transform body, FcpCreatureMotion motion, int stage, Shader shader)
        {
            var pearl = Tint(shader, new Color(.85f, .90f, .98f));
            var silver = Tint(shader, new Color(.45f, .62f, .80f));
            var dark = Tint(shader, new Color(.18f, .25f, .37f));
            Oval(body, new Vector3(0, .72f, -.02f), new Vector3(.46f, .48f, 1.02f), pearl);
            Beam(body, new Vector3(0, .76f, .31f), new Vector3(0, 1.24f, .49f), .15f, pearl);
            Oval(body, new Vector3(0, 1.24f, .56f), new Vector3(.28f, .31f, .40f), pearl);
            Oval(body, new Vector3(0, 1.16f, .75f), new Vector3(.23f, .18f, .31f), pearl);
            Taper(body, new Vector3(0, 1.34f, .64f), new Vector3(0, 1.52f + .13f * stage, .83f), .057f, silver);
            for (int i = 0; i < 4; i++)
                Oval(body, new Vector3(0, .96f + i * .105f, .21f + i * .062f), new Vector3(.18f, .24f, .22f), silver);
            Taper(body, new Vector3(0, .86f, -.43f), new Vector3(0, .39f, -.87f), .13f, silver);
            Ears(body, 1.39f, .48f, pearl);
            Eyes(body, 1.27f, .64f, .115f, dark);
            motion.Legs = FourLegs(body, .14f, .67f, .31f, -.34f, pearl, dark);
        }

        private static void BuildWolf(Transform body, FcpCreatureMotion motion, int stage, Shader shader)
        {
            var slate = Tint(shader, new Color(.34f, .44f, .57f));
            var light = Tint(shader, new Color(.72f, .81f, .91f));
            var dark = Tint(shader, new Color(.11f, .16f, .24f));
            var eye = Tint(shader, new Color(.55f, .84f, .98f));
            Oval(body, new Vector3(0, .59f, -.06f), new Vector3(.45f, .49f, .99f), slate);
            Oval(body, new Vector3(0, .69f, .28f), new Vector3(.55f + stage * .05f, .59f, .49f), light);
            Oval(body, new Vector3(0, .96f, .48f), new Vector3(.35f, .32f, .43f), slate);
            Oval(body, new Vector3(0, .89f, .71f), new Vector3(.25f, .17f, .34f), light);
            Oval(body, new Vector3(0, .91f, .86f), new Vector3(.15f, .09f, .11f), dark);
            Taper(body, new Vector3(0, .74f, -.43f), new Vector3(0, .45f, -.97f), .15f, slate);
            Ears(body, 1.05f, .44f, dark);
            Eyes(body, .99f, .66f, .13f, eye);
            motion.Legs = FourLegs(body, .14f, .58f, .28f, -.33f, slate, dark);
            foreach (Transform leg in motion.Legs)
                leg.localScale = new Vector3(1, .86f, 1);
        }

        private static void BuildEnt(Transform body, FcpCreatureMotion motion, int stage, Shader shader)
        {
            var bark = Tint(shader, new Color(.35f, .22f, .12f));
            var wood = Tint(shader, new Color(.57f, .38f, .19f));
            var leaf = Tint(shader, new Color(.37f, .56f, .17f));
            var eye = Tint(shader, new Color(.68f, .81f, .26f));
            Beam(body, new Vector3(0, .51f, 0), new Vector3(0, 1.34f, .02f), .20f, bark);
            Oval(body, new Vector3(0, 1.37f, .07f), new Vector3(.38f, .40f, .34f), wood);
            Eyes(body, 1.42f, .22f, .10f, eye);
            motion.Legs = new Transform[2];
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * .13f;
                motion.Legs[side < 0 ? 0 : 1] = Leg(body, new Vector3(x, .65f, 0), false, bark, wood);
                var shoulder = new Vector3(side * .16f, 1.17f, 0);
                var branch = new Vector3(side * .48f, 1.34f, .03f);
                Beam(body, shoulder, branch, .085f, wood);
                Taper(body, branch, new Vector3(side * .62f, .77f, .11f), .073f, bark);
                for (int i = 0; i <= stage; i++)
                {
                    var start = new Vector3(side * (.13f + .06f * i), 1.44f + i * .07f, 0);
                    var end = start + new Vector3(side * .14f, .30f - .025f * i, -.07f);
                    Taper(body, start, end, .061f, wood);
                    Oval(body, end, new Vector3(.29f, .22f, .26f), leaf);
                }
            }
        }

        private static void BuildMermaid(Transform body, FcpCreatureMotion motion, int stage, Shader shader)
        {
            var teal = Tint(shader, new Color(.07f, .42f, .45f));
            var fin = Tint(shader, new Color(.27f, .76f, .68f));
            var skin = Tint(shader, new Color(.82f, .65f, .47f));
            var hair = Tint(shader, new Color(.10f, .24f, .29f));
            Oval(body, new Vector3(0, .93f, .12f), new Vector3(.39f, .54f, .25f), teal);
            Oval(body, new Vector3(0, 1.30f, .14f), new Vector3(.29f, .32f, .25f), skin);
            Oval(body, new Vector3(0, 1.30f, .045f), new Vector3(.34f, .42f, .25f), hair);
            var tail = Pivot(body, "Tail", new Vector3(0, .72f, .10f));
            Beam(tail, Vector3.zero, new Vector3(0, -.30f, -.20f), .15f, teal);
            Taper(tail, new Vector3(0, -.28f, -.19f), new Vector3(0, -.54f, -.54f), .145f, teal);
            for (int side = -1; side <= 1; side += 2)
            {
                Taper(tail, new Vector3(0, -.52f, -.52f),
                    new Vector3(side * (.24f + .05f * stage), -.46f, -.84f), .12f, fin);
                Beam(body, new Vector3(side * .15f, 1.12f, .11f),
                    new Vector3(side * .30f, .79f, .24f), .055f, skin);
            }
            Eyes(body, 1.32f, .248f, .073f, hair);
            motion.Tail = tail;
            motion.Hovers = true;
        }

        private static void BuildBasilisk(Transform body, FcpCreatureMotion motion, int stage, Shader shader)
        {
            var green = Tint(shader, new Color(.23f, .43f, .16f));
            var belly = Tint(shader, new Color(.66f, .64f, .28f));
            var dark = Tint(shader, new Color(.16f, .29f, .11f));
            var eye = Tint(shader, new Color(.95f, .68f, .12f));
            var tail = Pivot(body, "Coils", new Vector3(0, .16f, -.13f));
            var from = new Vector3(0, 0, .32f);
            for (int i = 1; i <= 13; i++)
            {
                float angle = i * .48f;
                float radius = .31f - .014f * i;
                var to = new Vector3(Mathf.Sin(angle) * radius, .018f * Mathf.Sin(angle), Mathf.Cos(angle) * radius);
                Beam(tail, from, to, .123f - .005f * i, i % 4 == 0 ? belly : green);
                from = to;
            }
            Beam(body, new Vector3(0, .21f, .10f), new Vector3(0, .67f, .22f), .12f, green);
            Beam(body, new Vector3(0, .62f, .21f), new Vector3(0, .96f, .35f), .102f, belly);
            Oval(body, new Vector3(0, .99f, .43f), new Vector3(.38f, .29f, .38f), green);
            Oval(body, new Vector3(0, .91f, .57f), new Vector3(.29f, .14f, .29f), dark);
            for (int i = -stage; i <= stage; i++)
                Taper(body, new Vector3(i * .065f, 1.08f, .33f), new Vector3(i * .095f, 1.24f + .045f * stage, .22f), .047f, belly);
            Eyes(body, 1.00f, .57f, .146f, eye);
            motion.Tail = tail;
        }

        private static Transform[] FourLegs(Transform body, float width, float height, float front, float back,
            Material primary, Material foot)
        {
            var legs = new Transform[4];
            for (int i = 0; i < legs.Length; i++)
            {
                bool rear = i >= 2;
                legs[i] = Leg(body, new Vector3(i % 2 == 0 ? -width : width, height, rear ? back : front), rear, primary, foot);
            }
            return legs;
        }

        private static Transform[] FeatherWings(Transform body, float height, float depth, int stage,
            Material top, Material tips)
        {
            var wings = new Transform[2];
            for (int i = 0; i < wings.Length; i++)
            {
                int side = i == 0 ? -1 : 1;
                var wing = Pivot(body, "Wing", new Vector3(side * .16f, height, depth));
                wings[i] = wing;
                float span = .46f + stage * .16f;
                Beam(wing, Vector3.zero, new Vector3(side * span, .28f, -.08f), .11f, top);
                for (int feather = 0; feather < 4 + stage; feather++)
                {
                    float fraction = (feather + 1f) / (4f + stage);
                    var basePoint = new Vector3(side * span * fraction, .28f * fraction, -.07f);
                    var tip = basePoint + new Vector3(side * (.16f + .10f * fraction), -.03f - .12f * fraction, -.31f + .11f * fraction);
                    Taper(wing, basePoint, tip, .071f - .025f * fraction, feather % 2 == 0 ? tips : top);
                }
            }
            return wings;
        }

        private static void Ears(Transform body, float height, float depth, Material material)
        {
            for (int side = -1; side <= 1; side += 2)
                Taper(body, new Vector3(side * .09f, height, depth),
                    new Vector3(side * .16f, height + .21f, depth - .04f), .07f, material);
        }

        private static void Eyes(Transform body, float height, float depth, float width, Material material)
        {
            for (int side = -1; side <= 1; side += 2)
                Oval(body, new Vector3(side * width, height, depth), new Vector3(.059f, .058f, .056f), material);
        }

        private static void Taper(Transform parent, Vector3 from, Vector3 to, float radius, Material material)
        {
            Vector3 direction = to - from;
            var tip = Pivot(parent, "Taper", from);
            tip.localRotation = Quaternion.FromToRotation(Vector3.up, direction);
            tip.localScale = new Vector3(radius, direction.magnitude, radius);
            var filter = tip.gameObject.AddComponent<MeshFilter>();
            filter.sharedMesh = GetTaperMesh();
            tip.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static Mesh taperMesh;

        private static Mesh GetTaperMesh()
        {
            if (taperMesh != null)
                return taperMesh;

            const int sides = 8;
            var vertices = new Vector3[sides + 2];
            var indices = new int[sides * 6];
            vertices[sides] = Vector3.up;
            for (int i = 0; i < sides; i++)
            {
                float angle = i * Mathf.PI * 2 / sides;
                vertices[i] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                int next = (i + 1) % sides;
                int index = i * 6;
                indices[index] = i;
                indices[index + 1] = sides;
                indices[index + 2] = next;
                indices[index + 3] = i;
                indices[index + 4] = next;
                indices[index + 5] = sides + 1;
            }
            taperMesh = new Mesh { name = "FCP Taper" };
            taperMesh.vertices = vertices;
            taperMesh.triangles = indices;
            taperMesh.RecalculateNormals();
            taperMesh.RecalculateBounds();
            return taperMesh;
        }

        private static Material Tint(Shader shader, Color color)
        {
            return new Material(shader) { color = color };
        }

        private static Transform Pivot(Transform parent, string name, Vector3 position)
        {
            var pivot = new GameObject(name).transform;
            pivot.SetParent(parent, false);
            pivot.localPosition = position;
            return pivot;
        }

        private static Transform Leg(Transform parent, Vector3 hip, bool rear, Material gold, Material dark)
        {
            var leg = Pivot(parent, "Leg", hip);
            var knee = new Vector3(0, -0.31f, rear ? -0.10f : 0.02f);
            var foot = new Vector3(0, -0.61f, rear ? -0.04f : 0.02f);
            Beam(leg, Vector3.zero, knee, 0.058f, gold);
            Beam(leg, knee, foot, 0.037f, gold);
            Oval(leg, foot + new Vector3(0, -0.016f, 0.019f), new Vector3(0.10f, 0.083f, 0.14f), dark);
            return leg;
        }

        private static void Antler(Transform body, int side, int stage, Material material)
        {
            var start = new Vector3(side * 0.09f, 1.34f, 0.56f);
            float size = new[] { 0.31f, 0.65f, 1f }[stage];
            var elbow = start + new Vector3(side * 0.12f, 0.20f, -0.12f) * size;
            var top = start + new Vector3(side * 0.18f, 0.41f, -0.20f) * size;
            Beam(body, start, elbow, 0.034f, material);
            Beam(body, elbow, top, 0.022f, material);
            if (stage > 0)
                Beam(body, elbow, elbow + new Vector3(side * 0.11f, 0.18f, 0.12f) * size, 0.019f, material);
            if (stage == 2)
                Beam(body, Vector3.Lerp(elbow, top, 0.54f), top + new Vector3(side * 0.10f, -0.015f, 0.10f), 0.014f, material);
        }

        private static void Beam(Transform parent, Vector3 from, Vector3 to, float radius, Material material)
        {
            Vector3 difference = to - from;
            Part(parent, PrimitiveType.Cylinder, (from + to) * 0.5f,
                new Vector3(radius * 2, difference.magnitude * 0.5f, radius * 2),
                Quaternion.FromToRotation(Vector3.up, difference), material);
        }

        private static void Oval(Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            Oval(parent, position, scale, material, Quaternion.identity);
        }

        private static void Oval(Transform parent, Vector3 position, Vector3 scale, Material material, Quaternion rotation)
        {
            Part(parent, PrimitiveType.Sphere, position, scale, rotation, material);
        }

        private static void Part(Transform parent, PrimitiveType type, Vector3 position, Vector3 scale, Quaternion rotation, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localRotation = rotation;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            var collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                UnityEngine.Object.Destroy(collider);
            }
        }
    }

    public class FcpCreatureMotion : MonoBehaviour
    {
        public Transform Body;
        public Transform[] Legs;
        public Transform[] Wings;
        public Transform Tail;
        public bool Hovers;
        private Vector3 previous;
        private float phase;

        private void Start()
        {
            previous = transform.position;
        }

        private void LateUpdate()
        {
            if (transform.position.y < -9000f || Body == null)
                return;

            Vector3 current = transform.position;
            float distance = new Vector2(current.x - previous.x, current.z - previous.z).magnitude;
            previous = current;
            bool walking = distance > 0.0001f && distance < 2f;
            if (walking)
                phase += distance * 18f;

            float swing = walking ? Mathf.Sin(phase) * 18f : 0f;
            if (Legs != null)
            {
                for (int i = 0; i < Legs.Length; i++)
                {
                    float direction = i == 0 || i == 3 ? 1f : -1f;
                    Legs[i].localRotation = Quaternion.Euler(swing * direction, 0, 0);
                }
            }
            if (Wings != null)
            {
                float flap = Mathf.Sin(Time.time * (walking ? 9f : 5f)) * (Hovers || walking ? 25f : 6f);
                Wings[0].localRotation = Quaternion.Euler(0, 0, flap);
                Wings[1].localRotation = Quaternion.Euler(0, 0, -flap);
            }
            if (Tail != null)
                Tail.localRotation = Quaternion.Euler(0, Mathf.Sin(Time.time * (walking ? 7f : 3f)) * 14f, 0);

            float bob = Hovers ? Mathf.Sin(Time.time * 3f) * 0.045f :
                (walking ? Mathf.Abs(Mathf.Sin(phase)) * 0.02f : 0f);
            Body.localPosition = new Vector3(0, bob, 0);
        }
    }
}
