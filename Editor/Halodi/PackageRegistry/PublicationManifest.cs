using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using System.Linq;

namespace Halodi.PackageRegistry
{
    /// <summary>
    /// Helper class to create the JSON data to upload to the package server
    /// </summary>
    internal class PublicationManifest
    {
        private JObject j = new JObject();

        public string name 
        {
            get; private set;
        }

        private string base64Data;
        private long size;
        private string sha512;
        private string sha1;

        public string Request
        {
            get
            {
                return j.ToString();
            }
        }



        internal PublicationManifest(string packageFolder, string registry)
        {
            string manifestPath = Path.Combine(packageFolder, "package.json");

            if(!File.Exists(manifestPath))
            {
                throw new System.IO.IOException("Invalid package folder. Cannot find package.json in " + packageFolder);
            }

            JObject manifest = JObject.Parse(File.ReadAllText(manifestPath));

            if (manifest["name"] == null)
            {
                throw new System.IO.IOException("Package name not set");
            }

            if (manifest["version"] == null)
            {
                throw new System.IO.IOException("Package version not set");
            }

            if (manifest["description"] == null)
            {
                throw new System.IO.IOException("Package description not set");
            }

            CreateTarball(packageFolder);

            name = manifest["name"].ToString();
            string version = manifest["version"].ToString();
            string description = manifest["description"].ToString();

            string tarballName = name + "-" + version + ".tgz";
            string tarballPath = name + "/-/" + tarballName;


            Uri registryUri = new Uri(registry);
            Uri tarballUri = new Uri(registryUri, tarballPath);

            string readme = GetReadme(packageFolder);

            j["versions"] = new JObject();
            j["versions"][version] = manifest;



            j["_id"] = name;
            j["name"] = name;
            j["description"] = description;
            if(!string.IsNullOrEmpty(readme))
            {
                j["readme"] = readme;
            }
            

            j["dist-tags"] = new JObject();
            j["dist-tags"]["latest"] = version;


            j["versions"][version]["_id"] = name + "@" + version;
            j["versions"][version]["dist"] = new JObject();
            j["versions"][version]["dist"]["shasum"] = sha1;
            j["versions"][version]["dist"]["integrity"] = sha512;
            j["versions"][version]["dist"]["tarball"] = tarballUri.ToString();


            j["_attachments"] = new JObject();
            j["_attachments"][tarballName] = new JObject();
            j["_attachments"][tarballName]["content_type"] = "application/octet-stream";
            j["_attachments"][tarballName]["length"] = size.ToString();
            j["_attachments"][tarballName]["data"] = base64Data;

        }

        private string GetReadme(string packageFolder)
        {

            foreach(var path in Directory.EnumerateFiles(packageFolder))
            {
                string file = Path.GetFileName(path);
                if(file.Equals("readme.md", StringComparison.InvariantCultureIgnoreCase) || 
                file.Equals("readme.txt", StringComparison.InvariantCultureIgnoreCase) || 
                file.Equals("readme", StringComparison.InvariantCultureIgnoreCase))
                {
                    return File.ReadAllText(path);
                }
            }

            return null;
            
        }

        private string SHA512(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                var sha = new SHA512Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", string.Empty).ToLower();
            }
        }
        private string SHA1(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                var sha = new SHA1Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", string.Empty).ToLower();
            }
        }


        public static string Data(string file)
        {
            Byte[] bytes = File.ReadAllBytes(file);
            return Convert.ToBase64String(bytes);
        }


        public void CreateTarball(string packageFolder)
        {
            string folder = FileUtil.GetUniqueTempPathInProject();
            PackRequest request = UnityEditor.PackageManager.Client.Pack(packageFolder, folder);
            while (!request.IsCompleted)
            {
                Thread.Sleep(100);
            }

            if (request.Status != StatusCode.Success)
            {
                if (request.Error != null)
                {
                    throw new IOException(request.Error.message);
                }
                else
                {
                    throw new IOException("Cannot pack package");
                }
            }


            PackOperationResult result = request.Result;
            base64Data = Data(result.tarballPath);
            size = new FileInfo(result.tarballPath).Length;
            sha1 = SHA1(result.tarballPath);
            sha512 = SHA512(result.tarballPath);

            File.Delete(result.tarballPath);
            Directory.Delete(folder);

        }


    }
}

