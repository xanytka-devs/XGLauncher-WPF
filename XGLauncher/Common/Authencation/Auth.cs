using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using XGL.Dialogs;
using XGL.Dialogs.Login;
using MySqlX.XDevAPI;

namespace XGL.Networking.Authencation {

    public class GoogleAuthencator {

            // Copyright 2016 Google Inc.
            // 
            // Licensed under the Apache License, Version 2.0 (the "License");
            // you may not use this file except in compliance with the License.
            // You may obtain a copy of the License at
            // 
            //     http://www.apache.org/licenses/LICENSE-2.0
            // 
            // Unless required by applicable law or agreed to in writing, software
            // distributed under the License is distributed on an "AS IS" BASIS,
            // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
            // See the License for the specific language governing permissions and
            // limitations under the License.

        //Client configuration
        readonly static string clientID = App.GoogleCID;
        readonly static string clientSecret = App.GoogleCS;
        const string authorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        //const string tokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";
        //const string userInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
        public static string log = string.Empty;
        static TempDialog tempDialog;

        static void Output(string txt, bool callbackW = false, WebResponse response = null) { 
            tempDialog.Log(txt);
            if(callbackW) {
                LoginWindow.Instance?.ReturnRequest("gg", response);
                MainWindow.Instance?.ReturnRequest("gg", response);
            }
        }

        public static int GetRandomUnusedPort() {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public static async void Login() {
            tempDialog = new TempDialog();
            tempDialog.Show();
            // Generates state and PKCE values.
            string state = RandomDataBase64url(32);
            string code_verifier = RandomDataBase64url(32);
            string code_challenge = Base64urlencodeNoPadding(SHA256(code_verifier));
            const string code_challenge_method = "S256";
            // Creates a redirect URI using an available port on the loopback address.
            string redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, GetRandomUnusedPort());
            Output("redirect URI: " + redirectURI);
            //Creates an HttpListener to listen for requests on that redirect URI.
            var http = new HttpListener();
            http.Prefixes.Add(redirectURI);
            Output("Listening...");
            http.Start();
            //Creates the OAuth 2.0 authorization request.
            string authorizationRequest = string.Format("{0}?response_type=code&scope=openid%20profile&redirect_uri={1}&client_id={2}&state={3}&code_challenge={4}&code_challenge_method={5}",
                authorizationEndpoint,
                Uri.EscapeDataString(redirectURI),
                clientID,
                state,
                code_challenge,
                code_challenge_method);
            //Opens request in the browser.
            System.Diagnostics.Process.Start(authorizationRequest);
            //Waits for the OAuth authorization response.
            var context = await http.GetContextAsync();
            //Sends an HTTP response to the browser.
            var response = context.Response;
            string responseString = string.Format("<html><head><meta http-equiv='refresh' content='1;url=https://google.com'></head><body>Please return to the app.</body></html>");
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) => {
                responseOutput.Close();
                http.Stop();
                Console.WriteLine("HTTP server stopped.");
            });
            //Checks for errors.
            if (context.Request.QueryString.Get("error") != null) {
                Output(string.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error")));
                return;
            }
            if (context.Request.QueryString.Get("code") == null
                || context.Request.QueryString.Get("state") == null) {
                Output("Malformed authorization response. " + context.Request.QueryString);
                return;
            }
            //Extracts the code.
            var code = context.Request.QueryString.Get("code");
            var incoming_state = context.Request.QueryString.Get("state");
            //Compares the receieved state to the expected value, to ensure that
            //this app made the request which resulted in authorization.
            if (incoming_state != state) {
                Output(string.Format("Received request with invalid state ({0})", incoming_state));
                return;
            }
            Output("Authorization code: " + code);
            //Starts the code exchange at the Token Endpoint.
            PerformCodeExchange(code, code_verifier, redirectURI);
        }

        static async void PerformCodeExchange(string code, string code_verifier, string redirectURI) {
            Output("Exchanging code for tokens...");
            //Builds the request.
            string tokenRequestURI = "https://www.googleapis.com/oauth2/v4/token";
            string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&client_secret={4}&scope=&grant_type=authorization_code",
                code,
                Uri.EscapeDataString(redirectURI),
                clientID,
                code_verifier,
                clientSecret);
            //Sends the request.
            HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(tokenRequestURI);
            tokenRequest.Method = "POST";
            tokenRequest.ContentType = "application/x-www-form-urlencoded";
            tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            byte[] _byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
            tokenRequest.ContentLength = _byteVersion.Length;
            Stream stream = tokenRequest.GetRequestStream();
            await stream.WriteAsync(_byteVersion, 0, _byteVersion.Length);
            stream.Close();

            try {
                //Gets the response.
                WebResponse tokenResponse = await tokenRequest.GetResponseAsync();
                using (StreamReader reader = new StreamReader(tokenResponse.GetResponseStream())) {
                    //Reads response body.
                    string responseText = await reader.ReadToEndAsync();
                    Output(responseText);
                    //Converts to dictionary.
                    Dictionary<string, string> tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

                    string access_token = tokenEndpointDecoded["access_token"];
                    UserInfoCall(access_token);
                }
            }
            catch (WebException ex) {
                if (ex.Status == WebExceptionStatus.ProtocolError) {
                    if (ex.Response is HttpWebResponse response) {
                        Output("HTTP: " + response.StatusCode);
                        using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
                            //Reads response body.
                            string responseText = await reader.ReadToEndAsync();
                            Output(responseText);
                        }
                    }

                }
            }
        }


        static async void UserInfoCall(string access_token) {
            Output("Making API Call to Userinfo...");
            //Builds the request.
            string userinfoRequestURI = "https://www.googleapis.com/oauth2/v3/userinfo";
            //Sends the request.
            HttpWebRequest userinfoRequest = (HttpWebRequest)WebRequest.Create(userinfoRequestURI);
            userinfoRequest.Method = "GET";
            userinfoRequest.Headers.Add(string.Format("Authorization: Bearer {0}", access_token));
            userinfoRequest.ContentType = "application/x-www-form-urlencoded";
            userinfoRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

            //Gets the response.
            WebResponse userinfoResponse = await userinfoRequest.GetResponseAsync();
            using (StreamReader userinfoResponseReader = new StreamReader(userinfoResponse.GetResponseStream())) {
                //Reads response body.
                string userinfoResponseText = await userinfoResponseReader.ReadToEndAsync();
                Output(userinfoResponseText, true);
            }
        }

        /// <summary>
        /// Returns URI-safe data with a given input length.
        /// </summary>
        /// <param name="length">Input length (nb. output will be longer)</param>
        /// <returns></returns>
        public static string RandomDataBase64url(uint length) {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[length];
            rng.GetBytes(bytes);
            return Base64urlencodeNoPadding(bytes);
        }

        /// <summary>
        /// Returns the SHA256 hash of the input string.
        /// </summary>
        /// <param name="inputStirng"></param>
        /// <returns></returns>
        public static byte[] SHA256(string inputStirng) {
            byte[] bytes = Encoding.ASCII.GetBytes(inputStirng);
            SHA256Managed sha256 = new SHA256Managed();
            return sha256.ComputeHash(bytes);
        }

        /// <summary>
        /// Base64url no-padding encodes the given input buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string Base64urlencodeNoPadding(byte[] buffer) {
            string base64 = Convert.ToBase64String(buffer);
            //Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            //Strips padding.
            base64 = base64.Replace("=", "");
            return base64;
        }

    }

    public class VKAuthencator {

        public static void Login() {

            //Creates the OAuth 2.0 authorization request.
            string authorizationRequest = string.Format("{0}authorize?client_id={1}&display=page&redirect_uri={0}blank.html&scope=friends&response_type=token&v=5.52",
                "https://oauth.vk.com/",
                VkAPI.__APPID);
            //Opens request in the browser.
            System.Diagnostics.Process.Start(authorizationRequest);

        }

    }

}
