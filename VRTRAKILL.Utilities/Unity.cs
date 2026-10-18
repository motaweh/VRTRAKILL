using UnityEngine;

namespace VRTRAKILL.Utilities
{
    public static class Unity
    {
        // Returns the first GameObject with this name, including inactive ones, or null.
        public static GameObject ForceFindGameObject(string Name)
        {
            foreach (GameObject GO in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
                if (GO.name == Name) return GO;
            return null;
        }

        // Sets the layer of a GameObject and all of its children.
        public static void RecursiveChangeLayer(this GameObject GM, int Layer)
        {
            GM.layer = Layer;
            if (GM.transform.childCount > 0)
                for (int i = 0; i < GM.transform.childCount; i++)
                    try { RecursiveChangeLayer(GM.transform.GetChild(i).gameObject, Layer); } catch { }
        }

        // Raycasts forward from the transform and returns the hit, optionally filtered by a layer mask.
        public static RaycastHit ForwardRaycast(this Transform T, float Length, int? Layer = null)
        {
            if (Layer != null) { Physics.Raycast(new Ray(T.position, T.forward), out RaycastHit Hit, Length, (int)Layer); return Hit; }
            else { Physics.Raycast(new Ray(T.position, T.forward), out RaycastHit Hit, Length); return Hit; }
        }

        // Makes skinned meshes update off screen: all under T, one renderer, or all in the scene.
        public static void EnableOffscreenRendering(Transform T = null, SkinnedMeshRenderer SMR = null)
        {
            if (T) foreach (SkinnedMeshRenderer _SMR in T.GetComponentsInChildren<SkinnedMeshRenderer>()) _SMR.updateWhenOffscreen = true;
            else if (SMR) SMR.updateWhenOffscreen = true;
            else foreach (SkinnedMeshRenderer _SMR in Object.FindObjectsOfType<SkinnedMeshRenderer>()) _SMR.updateWhenOffscreen = true;
        }

        // Copies clip planes, depth, target eye, background, culling mask, clear flags and FOV to another camera.
        public static void CopyCameraValues(Camera CopyTo, Camera CopyFrom)
        {
            CopyTo.nearClipPlane = CopyFrom.nearClipPlane;
            CopyTo.farClipPlane = CopyFrom.farClipPlane;
            CopyTo.depth = CopyFrom.depth;
            CopyTo.stereoTargetEye = CopyFrom.stereoTargetEye;
            CopyTo.backgroundColor = CopyFrom.backgroundColor;
            CopyTo.cullingMask = CopyFrom.cullingMask;
            CopyTo.clearFlags = CopyFrom.clearFlags;
            CopyTo.fieldOfView = CopyFrom.fieldOfView;
        }

        // Returns the number of non-trigger colliders on a layer inside a sphere.
        public static int DetectCollisions(Vector3 Pos, float Radius, int Layer)
        {
            int Hits = 0;
            Collider[] Things = Physics.OverlapSphere(Pos, Radius, 1 << Layer, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < Things.Length; i++) Hits++;
            return Hits;
        }
    }

    public static class UnityExtensions
    {
        // Returns true if the GameObject has a component of type T.
        public static bool HasComponent<T>(this GameObject GM) where T : Component
        { return GM.GetComponent<T>() != null; }
    }
}
