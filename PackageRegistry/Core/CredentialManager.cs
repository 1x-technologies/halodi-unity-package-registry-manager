
using System;
using System.Collections.Generic;
using System.IO;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Syntax;
using UnityEngine;

namespace Halodi.PackageRegistry.Core
{
    internal class NPMCredential
    {
        public string url;
        public string token;
        public bool alwaysAuth;
    }

    public class CredentialManager
    {

        private string upmconfigFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".upmconfig.toml");


        private Dictionary<string, NPMCredential> credentials = new Dictionary<string, NPMCredential>();


        internal ICollection<NPMCredential> CredentialSet
        {
            get
            {
                return credentials.Values;
            }
        }


        public String[] Registries
        {
            get
            {
                String[] urls = new String[credentials.Count];
                int index = 0;
                foreach (NPMCredential cred in CredentialSet)
                {
                    urls[index] = cred.url;
                    ++index;
                }
                return urls;
            }
        }

        public CredentialManager()
        {
            if (File.Exists(upmconfigFile))
            {

                var upmconfig = Toml.Parse(File.ReadAllText(upmconfigFile));
                if (upmconfig.HasErrors)
                {
                    Debug.LogError("Cannot load upmconfig, invalid format");
                    return;
                }


                TomlTable table = upmconfig.ToModel();

                if(table != null && table.ContainsKey("npmAuth"))
                {
                    TomlTable auth = (TomlTable)table["npmAuth"];
                    if (auth != null)
                    {
                        foreach (var registry in auth)
                        {
                            NPMCredential cred = new NPMCredential();
                            cred.url = registry.Key;
                            TomlTable value = (TomlTable)registry.Value;
                            cred.token = (string)value["token"];
                            cred.alwaysAuth = (bool)value["alwaysAuth"];

                            credentials.Add(cred.url, cred);
                        }
                    }
                }
            }
        }

        internal void Write()
        {
            var doc = new DocumentSyntax();

            foreach (var credential in credentials.Values)
            {
                if (string.IsNullOrEmpty(credential.token))
                {
                    credential.token = "";
                }

                doc.Tables.Add(new TableSyntax(new KeySyntax("npmAuth", credential.url))
                {
                    Items =
                    {
                        {"token", credential.token},
                        {"alwaysAuth", credential.alwaysAuth}
                    }
                });

            }


            File.WriteAllText(upmconfigFile, doc.ToString());
        }

        internal bool HasRegistry(string url)
        {
            return credentials.ContainsKey(url);
        }

        internal NPMCredential GetCredential(string url)
        {
            return credentials[url];
        }

        internal void SetCredential(string url, bool alwaysAuth, string token)
        {
            if (HasRegistry(url))
            {
                credentials[url].url = url;
                credentials[url].alwaysAuth = alwaysAuth;
                credentials[url].token = token;
            }
            else
            {
                NPMCredential newCred = new NPMCredential();
                newCred.url = url;
                newCred.alwaysAuth = alwaysAuth;
                newCred.token = token;

                credentials.Add(url, newCred);
            }


        }

        internal void RemoveCredential(string url)
        {
            if (HasRegistry(url))
            {
                credentials.Remove(url);
            }
        }
    }

}