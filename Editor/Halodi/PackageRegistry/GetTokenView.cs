using UnityEditor;
using UnityEngine;

namespace Halodi.PackageRegistry
{

    internal class GetTokenView : EditorWindow
    {

        private string username;
        private string password;

        private bool initialized = false;


        private ScopedRegistry registry;

        void OnEnable()
        {
        }

        void OnDisable()
        {
            initialized = false;
        }

        private void SetRegistry(ScopedRegistry registry)
        {
            this.registry = registry;
            this.initialized = true;
        }

        void OnGUI()
        {
            if (initialized)
            {
                EditorGUILayout.LabelField("Login to registry.");
                username = EditorGUILayout.TextField("Registry username: ", username);
                password = EditorGUILayout.PasswordField("Registry password: ", password);

                if (GUILayout.Button("Login"))
                {
                    GetToken();
                }

                if (GUILayout.Button("Close"))
                {
                    CloseWindow();
                }
            }
        }

        internal static void CreateWindow(ScopedRegistry registry)
        {
            EditorApplication.delayCall += () =>
            {
                GetTokenView getTokenView = EditorWindow.GetWindow<GetTokenView>(true, "Get token", true);
                getTokenView.SetRegistry(registry);
            };

        }


        private void GetToken()
        {
            NPMResponse response = NPM.GetLoginToken(registry.url, username, password);

            if (string.IsNullOrEmpty(response.ok))
            {
                EditorUtility.DisplayDialog("Cannot get token", response.error, "Ok");
            }
            else
            {
                registry.token = response.token;
            }

            CloseWindow();
        }

        private void CloseWindow()
        {

            Close();
            GUIUtility.ExitGUI();
        }
    }
}