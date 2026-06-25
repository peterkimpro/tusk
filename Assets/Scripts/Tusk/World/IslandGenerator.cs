using System.Collections.Generic;
using UnityEngine;

namespace Tusk.World
{
    /// <summary>
    /// Procedurally builds a stylized island: domed ground plane with raised edges,
    /// scattered prop placement, deterministic from a seed (daily seed = same island
    /// for every player on the same UTC day). Uses primitives as placeholders;
    /// swap to Synty Nature prefabs once imported.
    /// </summary>
    public class IslandGenerator : MonoBehaviour
    {
        [Header("Island shape")]
        [SerializeField] private float radius = 100f;
        [SerializeField] private int segmentCount = 64;
        [SerializeField] private float centerHeight = 6f;
        [SerializeField] private Material groundMaterial;

        [Header("Props")]
        [SerializeField] private int treeCount = 80;
        [SerializeField] private int rockCount = 40;
        [SerializeField] private GameObject treePrefab;
        [SerializeField] private GameObject rockPrefab;
        [SerializeField] private float edgeBufferFraction = 0.08f; // keep props away from cliff

        [Header("Seeding")]
        [SerializeField] private int seed = -1; // -1 = derive from today's UTC date

        private Transform _propsParent;

        public int ActiveSeed { get; private set; }
        public float Radius => radius;

        public void Generate()
        {
            ActiveSeed = seed >= 0 ? seed : DailySeed();
            Random.InitState(ActiveSeed);
            BuildGround();
            ClearProps();
            PlaceProps();
        }

        private static int DailySeed()
        {
            var today = System.DateTime.UtcNow.Date;
            return today.Year * 10000 + today.Month * 100 + today.Day;
        }

        private void BuildGround()
        {
            // Domed mesh: a circle with a peak in the middle.
            var go = new GameObject("Ground");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;

            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            var mc = go.AddComponent<MeshCollider>();

            int vertCount = segmentCount + 1;
            var verts = new Vector3[vertCount];
            var uvs   = new Vector2[vertCount];
            verts[0] = new Vector3(0f, centerHeight, 0f);
            uvs[0]   = new Vector2(0.5f, 0.5f);

            for (int i = 0; i < segmentCount; i++)
            {
                float a = (i / (float)segmentCount) * Mathf.PI * 2f;
                float x = Mathf.Cos(a) * radius;
                float z = Mathf.Sin(a) * radius;
                verts[i + 1] = new Vector3(x, 0f, z);
                uvs[i + 1]   = new Vector2(0.5f + 0.5f * Mathf.Cos(a), 0.5f + 0.5f * Mathf.Sin(a));
            }

            var tris = new int[segmentCount * 3];
            for (int i = 0; i < segmentCount; i++)
            {
                tris[i * 3 + 0] = 0;
                tris[i * 3 + 1] = i + 1;
                tris[i * 3 + 2] = i + 2 > segmentCount ? 1 : i + 2;
            }

            var mesh = new Mesh { name = "IslandGround" };
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            mf.sharedMesh = mesh;
            mc.sharedMesh = mesh;
            if (groundMaterial == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                groundMaterial = new Material(shader);
                if (groundMaterial.HasProperty("_BaseColor"))
                    groundMaterial.SetColor("_BaseColor", new Color(0.36f, 0.52f, 0.28f));
                else
                    groundMaterial.color = new Color(0.36f, 0.52f, 0.28f);
            }
            mr.sharedMaterial = groundMaterial;
        }

        private void ClearProps()
        {
            if (_propsParent != null)
            {
                if (Application.isPlaying) Destroy(_propsParent.gameObject);
                else DestroyImmediate(_propsParent.gameObject);
            }
            var go = new GameObject("Props");
            go.transform.SetParent(transform);
            _propsParent = go.transform;
        }

        private void PlaceProps()
        {
            float spawnRadius = radius * (1f - edgeBufferFraction);
            for (int i = 0; i < treeCount; i++)  SpawnProp(treePrefab, spawnRadius, "Tree");
            for (int i = 0; i < rockCount; i++)  SpawnProp(rockPrefab, spawnRadius, "Rock");
        }

        private void SpawnProp(GameObject prefab, float spawnRadius, string fallbackName)
        {
            // Uniform sample over disk
            float r = spawnRadius * Mathf.Sqrt(Random.value);
            float a = Random.value * Mathf.PI * 2f;
            Vector3 pos = new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
            // Project onto dome surface: y = centerHeight * (1 - r/radius)
            pos.y = Mathf.Lerp(centerHeight, 0f, r / radius);

            GameObject go;
            if (prefab != null)
            {
                go = Instantiate(prefab, _propsParent);
            }
            else
            {
                go = GameObject.CreatePrimitive(fallbackName == "Tree" ? PrimitiveType.Cylinder : PrimitiveType.Cube);
                go.name = fallbackName;
                go.transform.SetParent(_propsParent);
                if (fallbackName == "Tree")
                {
                    go.transform.localScale = new Vector3(0.8f, Random.Range(2.5f, 4f), 0.8f);
                    Tint(go, new Color(0.20f, 0.42f, 0.12f));
                }
                else
                {
                    float s = Random.Range(0.8f, 1.8f);
                    go.transform.localScale = new Vector3(s, s * 0.7f, s);
                    Tint(go, new Color(0.45f, 0.45f, 0.48f));
                }
                var col = go.GetComponent<Collider>();
                if (col != null) col.enabled = true;
            }
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, Random.value * 360f, 0f);
        }

        private static void Tint(GameObject go, Color color)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var m = new Material(shader);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            else m.color = color;
            r.sharedMaterial = m;
        }
    }
}
