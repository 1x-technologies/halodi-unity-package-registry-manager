using System.Collections;
using System.Collections.Generic;
using Halodi.PackageRegistry.Core;
using UnityEditor;
using UnityEngine;

namespace Halodi.PackageRegistry.UI
{
    static class CredentialManagerSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            CredentialManager credentialManager = null;
            var provider = new SettingsProvider("Project/Package Manager/Credentials", SettingsScope.Project)
            {
                label = "Credentials",
                activateHandler = (str, v) =>
                {
                    credentialManager = new CredentialManager();
                },
                deactivateHandler = () =>
                {
                    credentialManager = null;
                },
                guiHandler = (searchContext) =>
                {
                    if (GUILayout.Button("Edit Scoped Registry Credentials"))
                    {
                        EditorWindow.GetWindow<CredentialManagerView>(true, "Credential Manager", true);
                    }
                },
                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "UPM", "NPM", "Credentials", "Packages", "Authentication", "Scoped Registry" })
            };

            return provider;
        }
    }
}