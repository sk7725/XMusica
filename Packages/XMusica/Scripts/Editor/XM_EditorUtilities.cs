using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace XMusica.EditorUtilities {
    public static class XM_EditorUtilities {
        /// <summary>
        /// Returns the relative path of the package.
        /// </summary>
        public static string packageRelativePath {
            get {
                if (string.IsNullOrEmpty(m_PackagePath))
                    m_PackagePath = GetPackageRelativePath();

                return m_PackagePath;
            }
        }
        [SerializeField]
        private static string m_PackagePath;

        /// <summary>
        /// Returns the fully qualified path of the package.
        /// </summary>
        public static string packageFullPath {
            get {
                if (string.IsNullOrEmpty(m_PackageFullPath))
                    m_PackageFullPath = GetPackageFullPath();

                return m_PackageFullPath;
            }
        }
        [SerializeField]
        private static string m_PackageFullPath;
        // Static Fields Related to locating the TextMesh Pro Asset
        private static string folderPath = "Not Found";

        public static string GetActivePathOrFallback(bool forceAssets = false) {
            string path = "Assets";

            if (TryGetActiveFolderPath(out string p)) path = p;

            if (forceAssets && !path.StartsWith("Assets")) path = "Assets";
            return path;
        }

        private static bool TryGetActiveFolderPath(out string path) {
            var _tryGetActiveFolderPath = typeof(ProjectWindowUtil).GetMethod("TryGetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);

            object[] args = new object[] { null };
            bool found = (bool)_tryGetActiveFolderPath.Invoke(null, args);
            path = (string)args[0];

            return found;
        }

        /// <summary>
        /// Gets unique colors required for some diagram.
        /// </summary>
        /// <param name="size">The number of total different colors required for the diagram</param>
        /// <param name="index">The current color index [0, size)</param>
        /// <returns></returns>
        public static Color GetDiagramColor(int size, int index, float saturation = 1, float brightness = 1) {
            if (size <= 10) return Color.HSVToRGB(index * 0.1f, saturation, brightness);
            return Color.HSVToRGB(index / (float)size, saturation, brightness);
        }

        private static string GetPackageRelativePath() {
            // Check for potential UPM package
            string packagePath = Path.GetFullPath("Packages/com.xreal.xmusica");
            if (Directory.Exists(packagePath)) {
                return "Packages/com.xreal.xmusica";
            }

            packagePath = Path.GetFullPath("Assets/..");
            if (Directory.Exists(packagePath)) {
                // Search default location for development package
                if (Directory.Exists(packagePath + "/Assets/Packages/com.xreal.xmusica/Editor Resources")) {
                    return "Assets/Packages/com.xreal.xmusica";
                }

                // Search for default location of normal AssetStore package
                if (Directory.Exists(packagePath + "/Assets/XMusica/Editor Resources")) {
                    return "Assets/XMusica";
                }

                // Search for potential alternative locations in the user project
                string[] matchingPaths = Directory.GetDirectories(packagePath, "XMusica", SearchOption.AllDirectories);
                packagePath = ValidateLocation(matchingPaths, packagePath);
                if (packagePath != null) return packagePath;
            }

            return null;
        }

        private static string GetPackageFullPath() {
            // Check for potential UPM package
            string packagePath = Path.GetFullPath("Packages/com.xreal.xmusica");
            if (Directory.Exists(packagePath)) {
                return packagePath;
            }

            packagePath = Path.GetFullPath("Assets/..");
            if (Directory.Exists(packagePath)) {
                // Search default location for development package
                if (Directory.Exists(packagePath + "/Assets/Packages/com.xreal.xmusica/Editor Resources")) {
                    return packagePath + "/Assets/Packages/com.xreal.xmusica";
                }

                // Search for default location of normal TextMesh Pro AssetStore package
                if (Directory.Exists(packagePath + "/Assets/XMusica/Editor Resources")) {
                    return packagePath + "/Assets/XMusica";
                }

                // Search for potential alternative locations in the user project
                string[] matchingPaths = Directory.GetDirectories(packagePath, "XMusica", SearchOption.AllDirectories);
                string path = ValidateLocation(matchingPaths, packagePath);
                if (path != null) return packagePath + path;
            }

            return null;
        }

        /// <summary>
        /// Method to validate the location of the asset folder by making sure the GUISkins folder exists.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        private static string ValidateLocation(string[] paths, string projectPath) {
            for (int i = 0; i < paths.Length; i++) {
                // Check if any of the matching directories contain a GUISkins directory.
                if (Directory.Exists(paths[i] + "/Editor Resources")) {
                    folderPath = paths[i].Replace(projectPath, "");
                    folderPath = folderPath.TrimStart('\\', '/');
                    return folderPath;
                }
            }

            return null;
        }

        public static Type[] GetDockTargets() {
            return new Type[] { typeof(VIBWindow), typeof(VISampleWindow), typeof(VITestWindow), typeof(SceneView) };
        }
    }
}