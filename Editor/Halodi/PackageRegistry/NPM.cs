
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Halodi.PackageRegistry
{
    [System.Serializable]
    internal class NPMLoginRequest
    {
        public string name;
        public string password;
    }

    [System.Serializable]
    public class NPMResponse
    {
        public string error;
        public string ok;
        public string token;

        public bool success;
    }
    public class NPM
    {

        public static NPMResponse GetLoginToken(string url, string user, string password)
        {
            using (var client = new WebClient())
            {
                Uri registryUri = new Uri(url);
                Uri loginUri = new Uri(registryUri, "/-/user/org.couchdb.user:" + user);
                client.Headers.Add(HttpRequestHeader.Accept, "application/json");
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                client.Headers.Add(HttpRequestHeader.Authorization, "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(user + ":" + password)));



                NPMLoginRequest request = new NPMLoginRequest();
                request.name = user;
                request.password = password;



                string requestString = JsonUtility.ToJson(request);


                try
                {
                    string responseString = client.UploadString(loginUri, WebRequestMethods.Http.Put, requestString);
                    NPMResponse response = JsonUtility.FromJson<NPMResponse>(responseString);
                    return response;
                }
                catch (WebException e)
                {

                    if (e.Response != null)
                    {
                        Stream receiveStream = e.Response.GetResponseStream();
                        // Pipes the stream to a higher level stream reader with the required encoding format.
                        StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                        string responseString = readStream.ReadToEnd();
                        e.Response.Close();
                        readStream.Close();

                        return JsonUtility.FromJson<NPMResponse>(responseString);


                    }
                    else
                    {
                        NPMResponse response = new NPMResponse();
                        response.error = e.Message;
                        return response;
                    }
                }
            }
        }

        public static void Publish(string packageFolder, string registry)
        {
            CredentialManager manager = new CredentialManager();
            if (!manager.HasRegistry(registry))
            {
                throw new System.IO.IOException("Credentials not set for registry " + registry);
            }

            string token = manager.GetCredential(registry).token;

            PublicationManifest manifest = new PublicationManifest(packageFolder, registry); ;




            using (var client = new WebClient())
            {
                Uri registryUri = new Uri(registry);
                Uri upload = new Uri(registryUri, manifest.name);


                client.Encoding = Encoding.ASCII;
                client.Headers.Add(HttpRequestHeader.Accept, "application/json");
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                client.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);

                try
                {
                    string responseString = client.UploadString(upload, WebRequestMethods.Http.Put, manifest.Request);
                    NPMResponse response = JsonUtility.FromJson<NPMResponse>(responseString);

                    if (!response.success)
                    {
                        throw new System.IO.IOException(responseString);
                    }
                }
                catch (WebException e)
                {
                    if (e.Response != null)
                    {
                        Stream receiveStream = e.Response.GetResponseStream();
                        // Pipes the stream to a higher level stream reader with the required encoding format.
                        StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                        string responseString = readStream.ReadToEnd();
                        e.Response.Close();
                        readStream.Close();

                        NPMResponse response = JsonUtility.FromJson<NPMResponse>(responseString);
                        if (string.IsNullOrEmpty(response.error))
                        {
                            throw new System.IO.IOException(responseString);
                        }
                        else
                        {
                            throw new System.IO.IOException(response.error);
                        }
                    }
                    else
                    {
                        if (e.InnerException != null)
                        {
                            throw new System.IO.IOException(e.InnerException.Message);
                        }
                        else
                        {
                            throw new System.IO.IOException(e.Message);
                        }
                    }
                }
            }

        }




    }

}
