using UnityEditor;
using UnityEngine;

namespace Tusk.EditorTools
{
    /// <summary>
    /// AssetPostprocessor that automatically configures Meshy FBX imports
    /// when they land in Assets/Art/Meshy/. Sets:
    /// - Humanoid rig type for the player character (enables Mecanim retargeting)
    /// - Generic rig type for creatures (case-by-case in week 2)
    /// - Mesh compression for static props (trees, rocks)
    /// - Sensible scale factor (Meshy default models are oversized)
    /// </summary>
    public class MeshyAssetImporter : AssetPostprocessor
    {
        private const string MeshyFolder = "Assets/Art/Meshy/";

        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(MeshyFolder)) return;

            var importer = (ModelImporter)assetImporter;
            string name = System.IO.Path.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();

            // Default settings for all Meshy imports
            importer.globalScale = 1.0f;
            importer.useFileScale = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
            importer.importBlendShapes = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importVisibility = true;

            if (name.Contains("character") || name.Contains("player") || name.Contains("hunter"))
            {
                // Player / humanoid character — Humanoid rig allows Mixamo retargeting too
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.optimizeGameObjects = false; // keep bone hierarchy explorable
                importer.meshCompression = ModelImporterMeshCompression.Off;
            }
            else if (name.Contains("creature") || name.Contains("wolf") || name.Contains("boar") ||
                     name.Contains("lizard") || name.Contains("beast"))
            {
                // Creatures — Generic rig (non-humanoid skeletons)
                importer.animationType = ModelImporterAnimationType.Generic;
                importer.meshCompression = ModelImporterMeshCompression.Low;
            }
            else
            {
                // Static props (trees, rocks, etc.) — no rig needed
                importer.animationType = ModelImporterAnimationType.None;
                importer.meshCompression = ModelImporterMeshCompression.High;
                importer.isReadable = false; // saves memory at runtime
            }
        }

        private void OnPostprocessModel(GameObject root)
        {
            if (!assetPath.StartsWith(MeshyFolder)) return;
            // Strip animation if rig type ended up None — keeps the import lean
            var importer = (ModelImporter)assetImporter;
            if (importer.animationType == ModelImporterAnimationType.None)
            {
                var animators = root.GetComponentsInChildren<Animator>(true);
                foreach (var a in animators) Object.DestroyImmediate(a);
            }
        }
    }
}
