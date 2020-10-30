using System.Collections.Generic;
using Halodi.PackageRegistry.Core;
using UnityEditor;
using UnityEditorInternal;

namespace Halodi.PackageRegistry.UI
{
    static class CredentialManagerSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            RegistryManager registryManager;
            ReorderableList credentialDrawer = null;
            ReorderableList registryDrawer = null;
            
            var provider = new SettingsProvider("Project/Package Manager/Credentials", SettingsScope.Project)
            {
                label = "Credentials",
                activateHandler = (str, v) =>
                {
                    registryManager = new RegistryManager();
                    credentialDrawer = CredentialManagerView.GetCredentialList(registryManager.credentialManager);
                    registryDrawer = RegistryManagerView.GetRegistryListView(registryManager);
                },
                guiHandler = (searchContext) =>
                {
                    EditorGUILayout.Space();
                    credentialDrawer.DoLayoutList();
                    EditorGUILayout.Space();
                    registryDrawer.DoLayoutList();
                },
                // Populate the search keywords to enable smart search filtering and label highlighting
                keywords = new HashSet<string>(new[] { "UPM", "NPM", "Credentials", "Packages", "Authentication", "Scoped Registry" })
            };

            return provider;
        }
    }
}