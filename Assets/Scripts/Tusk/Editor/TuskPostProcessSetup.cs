using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tusk.EditorTools
{
    /// <summary>
    /// Builds the stylized URP Volume profile that hides AI-generated mesh artifacts:
    /// strong contrast + saturation, soft bloom, vignette to draw eye to center.
    /// Reference: Hades / Vampire Survivors / Slay the Spire mobile post-process recipes.
    /// </summary>
    public static class TuskPostProcessSetup
    {
        const string ProfilePath = "Assets/Settings/TuskVolumeProfile.asset";

        public static VolumeProfile EnsureProfile()
        {
            EnsureFolder("Assets/Settings");
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }
            else
            {
                foreach (var c in profile.components.ToList()) profile.Remove(c.GetType());
            }

            // Bloom — soft halo around bright spots, rims look more pronounced
            var bloom = profile.Add<Bloom>(true);
            SetFloat(bloom.threshold, 0.95f);
            SetFloat(bloom.intensity, 0.45f);
            SetFloat(bloom.scatter, 0.75f);
            SetColor(bloom.tint, new Color(1.00f, 0.95f, 0.85f));

            // Color Adjustments — punch contrast + saturation hides AI mesh artifacts
            var color = profile.Add<ColorAdjustments>(true);
            SetFloat(color.postExposure, 0.15f);
            SetFloat(color.contrast, 18f);
            SetFloat(color.saturation, 22f);
            SetColor(color.colorFilter, new Color(1.00f, 0.98f, 0.92f));

            // Shadow/Midtone/Highlight — warm shadows, cool highlights for cinematic feel
            var smh = profile.Add<ShadowsMidtonesHighlights>(true);
            SetVec4(smh.shadows,    new Vector4(1.05f, 0.94f, 0.84f, 0f));
            SetVec4(smh.midtones,   new Vector4(1.00f, 1.00f, 1.00f, 0f));
            SetVec4(smh.highlights, new Vector4(0.96f, 0.98f, 1.05f, 0f));

            // Vignette — gentle darken at corners, focuses eye
            var vignette = profile.Add<Vignette>(true);
            SetColor(vignette.color, new Color(0.04f, 0.04f, 0.06f));
            SetFloat(vignette.intensity, 0.30f);
            SetFloat(vignette.smoothness, 0.45f);

            // Tonemapping — neutral filmic, prevents blown highlights
            var tone = profile.Add<Tonemapping>(true);
            tone.mode.overrideState = true;
            tone.mode.value = TonemappingMode.Neutral;

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            return profile;
        }

        public static void AttachGlobalVolume(GameObject root, VolumeProfile profile)
        {
            var go = new GameObject("TuskPostProcess");
            go.transform.SetParent(root.transform);
            var vol = go.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.priority = 0f;
            vol.profile = profile;
        }

        private static void SetFloat(VolumeParameter<float> p, float v)  { p.overrideState = true; p.value = v; }
        private static void SetColor(VolumeParameter<Color> p, Color v)   { p.overrideState = true; p.value = v; }
        private static void SetVec4(VolumeParameter<Vector4> p, Vector4 v) { p.overrideState = true; p.value = v; }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            AssetDatabase.CreateFolder("Assets", path.Substring("Assets/".Length));
        }
    }
}
