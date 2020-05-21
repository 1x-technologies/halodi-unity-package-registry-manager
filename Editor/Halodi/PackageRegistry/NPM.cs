
using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using Random = System.Random;

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

        public string reason;
    }

    public class ExpectContinueAware : System.Net.WebClient
    {
        protected override System.Net.WebRequest GetWebRequest(Uri address)
        {
            System.Net.WebRequest request = base.GetWebRequest(address);
            if (request is System.Net.HttpWebRequest)
            {
                var hwr = request as System.Net.HttpWebRequest;
                hwr.ServicePoint.Expect100Continue = false;
                hwr.AllowAutoRedirect = false;
            }
            return request;
        }
    }


    public class NPM
    {

        internal static string UrlCombine(string start, string more)
        {
            if (string.IsNullOrEmpty(start))
            {
                return more;
            }
            else if (string.IsNullOrEmpty(more))
            {
                return start;
            }

            return start.TrimEnd('/') + "/" + more.TrimStart('/');
        }

        public static string GetBintrayToken(string user, string apiKey)
        {
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(user + ":" + apiKey));
        }

        public static NPMResponse GetLoginToken(string url, string user, string password)
        {
            using (var client = new WebClient())
            {
                string loginUri = UrlCombine(url, "/-/user/org.couchdb.user:" + user);
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




            using (var client = new ExpectContinueAware())
            {
                string upload = UrlCombine(registry, manifest.name);


                client.Encoding = Encoding.UTF8;
                client.Headers.Add(HttpRequestHeader.Accept, "application/json");
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                client.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);


                // Headers set by the NPM client, but not by us. Option to try with compatibility issues.
                
                // client.Headers.Add("npm-in-ci", "false");
                // client.Headers.Add("npm-scope", "");
                // client.Headers.Add(HttpRequestHeader.UserAgent, "npm/6.14.4 node/v12.16.2 linux x64");
                // var random = new Random();
                // string a = String.Format("{0:X8}", random.Next(0x10000000, int.MaxValue)).ToLower();
                // string b = String.Format("{0:X8}", random.Next(0x10000000, int.MaxValue)).ToLower();

                // client.Headers.Add("npm-session", a + b);
                // client.Headers.Add("referer", "publish");


                try
                {
                    string responseString = client.UploadString(upload, WebRequestMethods.Http.Put, manifest.Request);

                    try
                    {
                        NPMResponse response = JsonUtility.FromJson<NPMResponse>(responseString);
                        if (string.IsNullOrEmpty(response.ok))
                        {
                            throw new System.IO.IOException(responseString);
                        }
                    }
                    catch (Exception)
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


                        try
                        {
                            NPMResponse response = JsonUtility.FromJson<NPMResponse>(responseString);

                            if (string.IsNullOrEmpty(response.error))
                            {
                                throw new System.IO.IOException(responseString);
                            }
                            else
                            {
                                string reason = string.IsNullOrEmpty(response.reason) ? "" : Environment.NewLine + response.reason;

                                throw new System.IO.IOException(response.error + reason);
                            }
                        }
                        catch (Exception)
                        {
                            throw new System.IO.IOException(responseString);
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
