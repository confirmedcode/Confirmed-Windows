using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TrustWindowsWPF
{
    public class APIControl
    {
        private HttpClient client;
        private CookieContainer cookContainer;
        private bool signedIn = false;

        public APIControl()
        {
            cookContainer = new CookieContainer();

            HttpClientHandler handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = cookContainer
            };

            client = new HttpClient(handler);
        }

        public CookieContainer getCookies()
        {
            return cookContainer;
        }

        void incrementAPIVersion()
        {
            switch(ConfirmedConstants.versionNumber)
            {
                case 1:
                    Properties.Settings.Default.apiVersion = ConfirmedConstants.version2String;
                    break;
                case 2:
                    Properties.Settings.Default.apiVersion = ConfirmedConstants.version3String;
                    break;
                case 3:
                default:
                    Properties.Settings.Default.apiVersion = ConfirmedConstants.version1String;
                    break;
            }

            Properties.Settings.Default.Save();
        }

        void setLatestApiVersion()
        {
            Properties.Settings.Default.apiVersion = ConfirmedConstants.version3String;
            Properties.Settings.Default.Save();
        }

        void setVersion1API()
        {
            Properties.Settings.Default.apiVersion = ConfirmedConstants.version1String;
            Properties.Settings.Default.Save();
        }

        public enum SignInResponse
        {
            SignedIn,
            AwaitingVerification,
            AccountNotFound
        };

        public SignInResponse signIn(string username, string password)
        {
            return signIn(username, password, 0);
        }

        public SignInResponse signIn(string username, string password, int tries)
        {
            var values = new Dictionary<string, string>
                {
                    { "email", username },
                    { "password", password }
                };

            var content = new FormUrlEncodedContent(values);

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var signinResponse = client.PostAsync(ConfirmedConstants.apiSignIn, content);
                var signInResponseString = signinResponse.Result.Content.ReadAsStringAsync();

                if (signinResponse.Result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // actually check for cookies
                    CookieCollection cookies = cookContainer.GetCookies(new Uri(ConfirmedConstants.apiAddress));

                    bool jsonDeserialized = false;

                    Dictionary<string, string> testDict = null;

                    try
                    {
                        testDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(signInResponseString.Result);
                        jsonDeserialized = true;
                    }
                    catch(Exception e)
                    {
                        jsonDeserialized = false;
                    }

                    if(jsonDeserialized)
                    {
                        if (testDict.ContainsKey("code") && testDict["code"] == "1")
                        {
                            if (ConfirmedConstants.versionNumber == 3) // don't bother with awaiting verification on v1 or v2)
                            {
                                return SignInResponse.AwaitingVerification;
                            }
                        }
                        else if(cookies.Count > 0)
                        {
                            signedIn = true;
                            return SignInResponse.SignedIn;
                        }
                    }
                    // if v1, may not deserialize on success
                    else
                    {
                        if (cookies.Count > 0)
                        {
                            signedIn = true;
                            return SignInResponse.SignedIn;
                        }
                    }
                }

                tries++;
                if (tries < 3)
                {
                    incrementAPIVersion();
                    return signIn(username, password, tries);
                }

                return SignInResponse.AccountNotFound;
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public bool checkStoredLogin()
        {
            string username = Properties.Settings.Default.username;
            string password = Properties.Settings.Default.password;
            if (password != "")
            {
                password = DPAPIInterface.Unprotect(password, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
            }

            if (password == "" || username == "")
            {
                return false;
            }

            try
            {
                if (signIn(username, password) == SignInResponse.SignedIn)
                {
                    var getKeyResponse = getKey();

                    if (getKeyResponse.Result.StatusCode == HttpStatusCode.OK)
                    {
                        var getKeyResponseString = getKeyResponse.Result.Content.ReadAsStringAsync();
                        var getKeyDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(getKeyResponseString.Result);

                        if (!getKeyDict.ContainsKey("code"))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (AggregateException e)
            {
                throw e;
            }
        }

        public bool checkVersion1Login(string username, string password)
        {
            setVersion1API();

            var values = new Dictionary<string, string>
                {
                    { "email", username },
                    { "password", password }
                };

            var content = new FormUrlEncodedContent(values);

            try
            {
                var signinResponse = client.PostAsync(ConfirmedConstants.apiSignIn, content);
                var signInResponseString = signinResponse.Result.Content.ReadAsStringAsync();

                if (signinResponse.Result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    signedIn = true;

                    var getKeyResponse = getKey();

                    if (getKeyResponse.Result.StatusCode == HttpStatusCode.OK)
                    {
                        var getKeyResponseString = getKeyResponse.Result.Content.ReadAsStringAsync();
                        var getKeyDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(getKeyResponseString.Result);

                        if (!getKeyDict.ContainsKey("code"))
                        {
                            return true;
                        }
                    }
                }
               
                return false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Task<HttpResponseMessage> getKey()
        {
            return client.PostAsync(ConfirmedConstants.apiGetKey, null);
        }

        public Task<HttpResponseMessage> signUp(string username, string password)
        {
            setLatestApiVersion();

            var values = new Dictionary<string, string>
                    {
                        { "email", username },
                        { "password", password }
                    };

            var content = new FormUrlEncodedContent(values);

            return client.PostAsync(ConfirmedConstants.apiCreateUser, content);
        }
    }
}
