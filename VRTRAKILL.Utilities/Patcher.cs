using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace VRTRAKILL.Utilities
{
    // If you wanna use it for your own purposes - you have my full permission :)

    // Applies HarmonyX patches by assembly, namespace or type.
    public class Patcher
    {
        public Harmony Harmony { get; set; }
        public Assembly Ass { get; set; } = Assembly.GetCallingAssembly();

        public string Namespace { get; set; }
        public string[] Namespaces { get; set; }
        public Type Type { get; set; }
        public Type[] Types { get; set; }

        public BepInEx.Logging.ManualLogSource Log { get; set; }

        // Creates a patcher that applies every patch Harmony finds.
        public Patcher(Harmony _Harmony) { Harmony = _Harmony; }
        // Creates a patcher for one namespace of the calling assembly.
        public Patcher(Harmony _Harmony, string _Namespace) : this(_Harmony) { Namespace = _Namespace; }
        // Creates a patcher for several namespaces of the calling assembly.
        public Patcher(Harmony _Harmony, string[] _Namespaces) : this(_Harmony) { Namespaces = _Namespaces; }
        // Creates a patcher for one patch type.
        public Patcher(Harmony _Harmony, Type _Type) : this(_Harmony) { Type = _Type; }
        // Creates a patcher for several patch types.
        public Patcher(Harmony _Harmony, Type[] _Types) : this(_Harmony) { Types = _Types.ToArray(); }
        // Creates a patcher that searches the given assembly.
        public Patcher(Harmony _Harmony, Assembly _ASS) : this(_Harmony) { Ass = _ASS; }
        // Creates a patcher for one namespace of the given assembly.
        public Patcher(Harmony _Harmony, Assembly _ASS, string _Namespace) : this(_Harmony, _ASS) { Namespace = _Namespace; }
        // Creates a patcher for several namespaces of the given assembly.
        public Patcher(Harmony _Harmony, Assembly _ASS, string[] _Namespaces) : this(_Harmony, _ASS) { Namespaces = _Namespaces; }
        // Creates a patcher for one patch type and sets the search assembly.
        public Patcher(Harmony _Harmony, Assembly _ASS, Type _Type) : this(_Harmony, _ASS) { Type = _Type; }
        // Creates a patcher for several patch types and sets the search assembly.
        public Patcher(Harmony _Harmony, Assembly _ASS, Type[] _Types) : this(_Harmony, _ASS) { Types = _Types.ToArray(); }

        // Returns the Harmony patch types in the assembly, optionally only from one namespace.
        private List<Type> GetPatches(string _Namespace = null)
        {
            IEnumerable<Type> Q;
            if (_Namespace == null)
            {
                Q = from T in Ass.GetTypes()
                    where T.IsDefined(typeof(HarmonyPatch), false)
                    select T;
            }
            else
            {
                Q = from T in Ass.GetTypes()
                    where T.Namespace == _Namespace && T.IsDefined(typeof(HarmonyPatch), false)
                    select T;
            }
            return Q.ToList();
        }
        // Returns the Harmony patch types from each of the given namespaces.
        private List<Type> GetPatches(string[] _Namespaces)
        {
            List<Type> QL = new List<Type>();
            foreach (string _Namespace in _Namespaces)
                QL.AddRange(GetPatches(_Namespace));
            return QL;
        }
        // Returns the patch types in one namespace.
        private IEnumerable<Type> GetTypes(string _Namespace)
        { return GetPatches(_Namespace); }
        // Returns the patch types in the given namespaces.
        private IEnumerable<Type> GetTypes(string[] _Namespaces)
        { return GetPatches(_Namespaces); }

        // Applies the set types and namespaces, or every patch when none are set.
        public void PatchAll()
        {
            List<Type> L = new List<Type>();

            if (Namespace == null && Namespaces == null && Type == null && Types == null)
                try { Harmony.PatchAll(); } catch (NullReferenceException) { Log.LogFatal("Could not find any patches(???) wtf?"); }
            else
            {
                if (Type != null) L.Add(Type);
                if (Types != null && Types.Length > 0) L.AddRange(Types.ToArray());
                if (Namespace != null) L.AddRange(GetTypes(Namespace));
                if (Namespaces != null && Namespaces.Length > 0) L.AddRange(GetTypes(Namespaces));
                Patch(L.ToArray());
            }
        }


        // Applies every patch type in one namespace and logs the types that fail.
        public void Patch(string _Namespace)
        {
            Log.LogInfo($"Patching Namespace \"{_Namespace}\"...");
            IEnumerable<Type> Q = GetTypes(_Namespace);
            foreach (Type T in Q) try { Harmony.PatchAll(T); } catch { Log.LogError($"Nullref with type {T}"); }
        }
        // Applies every patch type in the given namespaces and logs the types that fail.
        public void Patch(string[] _Namespaces)
        {
            IEnumerable<Type> Q = GetTypes(_Namespaces);
            foreach (Type T in Q) try { Harmony.PatchAll(T); } catch { Log.LogError($"Nullref with type {T}"); }
        }
        // Applies one patch type.
        public void Patch(Type _T)
        {
            Log.LogInfo($"Patching Type \"{nameof(_T)}\"...");
            Harmony.PatchAll(_T);
        }
        // Applies each given patch type.
        public void Patch(Type[] _T)
        { foreach (Type T in _T) Harmony.PatchAll(T); }

        // Removes every patch this Harmony instance applied.
        public void UnpatchAll()
        => Harmony.UnpatchSelf();
    }
}
