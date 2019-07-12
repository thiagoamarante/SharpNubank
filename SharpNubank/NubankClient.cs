using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SharpNubank.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace SharpNubank
{
    public class NubankClient : IDisposable
    {
        #region Fields
        private readonly string _EndPointDiscoveryUrl = "https://prod-s0-webapp-proxy.nubank.com.br/api/discovery";
        private readonly string _EndPointDiscoveryAppUrl = "https://prod-s0-webapp-proxy.nubank.com.br/api/app/discovery";
        private string _EndPointLogin = null;
        private string _EndPointLift = null;
        private AccessToken _AccessToken = null;

        private readonly HttpClient _HttpClient;
        #endregion

        public NubankClient(string accessToken = null)
        {
            _HttpClient = new HttpClient();
            if (!string.IsNullOrEmpty(accessToken))
            {
                var jsonAccessToken = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(accessToken));
                _AccessToken = JsonConvert.DeserializeObject<AccessToken>(jsonAccessToken);
                SetAuthentication();
            } 
        }

        #region Properties
        public bool Authenticated => _AccessToken != null ? _AccessToken.Authenticated : false;
        #endregion

        #region Methods
        public string GetAccessToken()
        {
            if (_AccessToken == null || !_AccessToken.Authenticated)
                return null;
            else
                return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_AccessToken)));
        }

        public async Task<Response<LoginResponse>> Login(string login, string password)
        {
            var result = new Response<LoginResponse>();
            try
            {
                await DiscoveryUrl();
                var content = new StringContent(JsonConvert.SerializeObject(new
                {
                    client_id = "other.conta",
                    client_secret = "yQPeLzoHuJzlMMSAjC-LgNUJdUecx8XO",
                    grant_type = "password",
                    login,
                    password
                }), Encoding.UTF8, "application/json");

                var httpResponse = await _HttpClient.PostAsync(_EndPointLogin, content);
                if (httpResponse.StatusCode != System.Net.HttpStatusCode.OK)
                    result.Message = httpResponse.ReasonPhrase;
                else
                {
                    var json = await httpResponse.Content.ReadAsStringAsync();
                    var jObject = JObject.Parse(json);
                    result = CreateLoginResponse(jObject);
                }
            }
            catch(Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }

        public async Task<Response<LoginResponse>> AutenticateWithQrCode(string code)
        {
            var result = new Response<LoginResponse>();
            try
            {
                await DiscoveryAppUrl();
                var content = new StringContent(JsonConvert.SerializeObject(new
                {
                    qr_code_id = code,
                    type = "login-webapp"
                }), Encoding.UTF8, "application/json");
                var httpResponse = await _HttpClient.PostAsync(_EndPointLift, content);
                if (httpResponse.StatusCode != System.Net.HttpStatusCode.OK)
                    result.Message = httpResponse.ReasonPhrase;
                else
                {
                    var json = await httpResponse.Content.ReadAsStringAsync();
                    var jObject = JObject.Parse(json);
                    result = CreateLoginResponse(jObject);
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }

        public async Task<Response<CustomerDetails>> CustomerDetails()
        {
            var result = new Response<CustomerDetails>();
            try
            {
                if (CheckAuthentication(result))
                {
                    var httpResponse = await _HttpClient.GetAsync(_AccessToken.EndPointCustomer);
                    if (httpResponse.StatusCode != System.Net.HttpStatusCode.OK)
                        result.Message = httpResponse.ReasonPhrase;
                    else
                    {
                        var json = await httpResponse.Content.ReadAsStringAsync();
                        var jObject = JObject.Parse(json);
                        result.Data = jObject["customer"].ToObject<CustomerDetails>();
                        result.Success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }

        public async Task<Response<List<CreditCardEvent>>> CreditCardEvents()
        {
            var result = new Response<List<CreditCardEvent>>();
            try
            {
                if (CheckAuthentication(result))
                {
                    var httpResponse = await _HttpClient.GetAsync(_AccessToken.EndPointEvents);
                    if (httpResponse.StatusCode != System.Net.HttpStatusCode.OK)
                        result.Message = httpResponse.ReasonPhrase;
                    else
                    {
                        var json = await httpResponse.Content.ReadAsStringAsync();
                        var jObject = JObject.Parse(json);
                        result.Data = jObject["events"].ToObject<List<CreditCardEvent>>();
                        result.Success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }

        public async Task<Response> CreditCardDetails()
        {
            var result = new Response();
            if (CheckAuthentication(result))
            {

            }
            return result;
        }

        public async Task<Response> NuContaEvents()
        {
            var result = new Response();
            if (CheckAuthentication(result))
            {

            }
            return result;
        }

        public async Task<Response> NuContaBalance()
        {
            var result = new Response();
            if (CheckAuthentication(result))
            {

            }
            return result;
        }

        public void Dispose()
        {
            _HttpClient.Dispose();
        }
        #endregion

        #region Method Private
        private Response<LoginResponse> CreateLoginResponse(JObject jObject)
        {
            var result = new Response<LoginResponse>();
            if (jObject.ContainsKey("access_token"))
            {
                result.Success = true;
                _AccessToken = new AccessToken()
                {
                    Token = jObject["access_token"].ToString(),
                    EndPointEvents = jObject["_links"]?["events"]?["href"]?.ToString(),
                    EndPointCustomer = jObject["_links"]?["customer"]?["href"]?.ToString(),
                    EndPointSavingsAccount = jObject["_links"]?["savings_account"]?["href"]?.ToString(),
                    EndPointAccount = jObject["_links"]?["account"]?["href"]?.ToString()
                };

                if (!string.IsNullOrEmpty(_AccessToken.EndPointEvents) && !string.IsNullOrEmpty(_AccessToken.EndPointCustomer) && !string.IsNullOrEmpty(_AccessToken.EndPointSavingsAccount) && !string.IsNullOrEmpty(_AccessToken.EndPointAccount))
                    _AccessToken.Authenticated = true;                    

                SetAuthentication();
                result.Data = new LoginResponse()
                {
                    Token = _AccessToken.Token,
                    NeedsDeviceAuthorization = !_AccessToken.Authenticated,
                    Code = !_AccessToken.Authenticated ? Guid.NewGuid().ToString() : null
                };
            }
            else
            {
                result.Message = jObject["error"].ToString();
            }
            return result;
        }

        private bool CheckAuthentication(Response response)
        {
            if (_AccessToken == null || !_AccessToken.Authenticated)
            {
                response.Success = false;
                response.Message = "You must be logged in to use this feature";
                return false;
            }
            return true;
        }

        private async Task DiscoveryUrl()
        {
            if (string.IsNullOrEmpty(_EndPointLogin))
            {
                var httpResponse = await _HttpClient.GetAsync(_EndPointDiscoveryUrl);
                if (httpResponse.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception(httpResponse.ReasonPhrase);

                var json = await httpResponse.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(json);
                _EndPointLogin = jObject["login"].ToString();
            }
        }

        private async Task DiscoveryAppUrl()
        {
            if (string.IsNullOrEmpty(_EndPointLift))
            {
                var httpResponse = await _HttpClient.GetAsync(_EndPointDiscoveryAppUrl);
                if (httpResponse.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception(httpResponse.ReasonPhrase);

                var json = await httpResponse.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(json);
                _EndPointLift = jObject["lift"].ToString();
            }
        }

        private void SetAuthentication()
        {
            if (_AccessToken == null || !_AccessToken.Authenticated)
                _HttpClient.DefaultRequestHeaders.Authorization = null;
            else
            {
                _HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _AccessToken.Token);
            }
        }
        #endregion
    }
}
