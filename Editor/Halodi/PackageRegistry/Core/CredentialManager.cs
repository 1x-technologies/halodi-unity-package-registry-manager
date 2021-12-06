
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Syntax;
using UnityEngine;

namespace Halodi.PackageRegistry.Core
{
    public class NPMCredential
    {
        public string url;
        public string token;
        public string _auth;
        public string email;
        public bool alwaysAuth;
    }

    public class CredentialManager
    {
        private string upmconfigFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".upmconfig.toml");
        private List<NPMCredential> credentials = new List<NPMCredential>();
        
        public List<NPMCredential> CredentialSet
        {
            get
            {
                return credentials;
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
                            if (value.TryGetValue(nameof(NPMCredential.token), out var token))
                                cred.token = (string) token;
                            if (value.TryGetValue(nameof(NPMCredential._auth), out var _auth))
                                cred._auth = (string) _auth;
                            if (value.TryGetValue(nameof(NPMCredential.email), out var email))
                                cred.email = (string)email;
                            if (value.TryGetValue(nameof(NPMCredential.alwaysAuth), out var alwaysAuth))
                                cred.alwaysAuth = (bool)alwaysAuth;

                            credentials.Add(cred);
                        }
                    }
                }
            }
        }

        public void Write()
        {
            var doc = new DocumentSyntax();

            foreach (var credential in credentials)
            {
                if (string.IsNullOrEmpty(credential.token))
                {
                    credential.token = "";
                }

                var table = new TableSyntax(new KeySyntax("npmAuth", credential.url));

                if(!string.IsNullOrEmpty(credential.token))
                    table.Items.Add(nameof(NPMCredential.token), credential.token);
                if(!string.IsNullOrEmpty(credential._auth))
                    table.Items.Add(nameof(NPMCredential._auth), credential._auth);
                if(!string.IsNullOrEmpty(credential.email))
                    table.Items.Add(nameof(NPMCredential.email), credential.email);
                table.Items.Add(nameof(NPMCredential.alwaysAuth), credential.alwaysAuth);

                doc.Tables.Add(table);
            }


            File.WriteAllText(upmconfigFile, doc.ToString());
        }

        public bool HasRegistry(string url)
        {
            return credentials.Any(x => x.url.Equals(url, StringComparison.Ordinal));
        }

        public NPMCredential GetCredential(string url)
        {
            return credentials.FirstOrDefault(x => x.url?.Equals(url, StringComparison.Ordinal) ?? false);
        }

        public void SetCredential(NPMCredential inputCred)
        {
            if (HasRegistry(inputCred.url))
            {
                var cred = GetCredential(inputCred.url);
                cred.url = inputCred.url;
                cred.alwaysAuth = inputCred.alwaysAuth;
                cred.token = inputCred.token;
                cred.email = inputCred.email;
                cred._auth = inputCred._auth;
            }
            else
            {
                NPMCredential newCred = new NPMCredential();
                newCred.url = inputCred.url;
                newCred.alwaysAuth = inputCred.alwaysAuth;
                newCred.token = inputCred.token;
                newCred.email = inputCred.email;
                newCred._auth = inputCred._auth;

                credentials.Add(newCred);
            }
        }

        public void RemoveCredential(string url)
        {
            if (HasRegistry(url))
            {
                credentials.RemoveAll(x => x.url.Equals(url, StringComparison.Ordinal));
            }
        }
    }

}