using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.ApiImps
{
    public abstract class Proxy
    {
        private JsonSerializerSettings jsonSerializerSettings;
        public Uri HttpClientBaseAddress { get; set; }

        private ConnectionMode connectionMode;

        public Proxy(ConnectionMode options)
        {
            connectionMode = options;
            var key = GetType().Namespace;
            HttpClientBaseAddress = string.IsNullOrEmpty(connectionMode.ServiceUri) ? null : new Uri(connectionMode.ServiceUri);
            //if (HttpClientBaseAddress == null)
            //{
            //    throw new ArgumentNullException(key);
            //}
        }

        protected JsonSerializerSettings JsonSerializerSettings => jsonSerializerSettings ?? (jsonSerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        protected async Task<T> GetResponse<T>(string requestUri)
        {
            return await GetResponse<T>(requestUri, 0);
        }

        protected async Task<T> GetResponse<T>(string requestUri, int timeOut)
        {
            try
            {
                using var client = new HttpClient();
                var webApiRequestTimeout = 5000;
                if (timeOut > 0)
                {
                    webApiRequestTimeout = timeOut;
                }
                client.Timeout = TimeSpan.FromMilliseconds(webApiRequestTimeout);
                client.BaseAddress = HttpClientBaseAddress;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(connectionMode.RemoteToken))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", connectionMode.RemoteToken);
                }

                string encryptedUrl = GetEncryptedUrl(requestUri);
                var response = await client.GetAsync(encryptedUrl);
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent);

            }
            catch(Exception ex)
            {
                Log.Error(ex.Message);
                return default;
            }            
        }

        protected async Task<T> PostMultipleFiles<T>(string requestUri, List<IFormFile> formFiles)
        {
            try
            {
                var client = new HttpClient
                {
                    BaseAddress = HttpClientBaseAddress
                };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
                if (!string.IsNullOrEmpty(connectionMode.RemoteToken))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", connectionMode.RemoteToken);
                }
                byte[] data;
                MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
                foreach (var FormFile in formFiles)
                {
                    using (var br = new BinaryReader(FormFile.OpenReadStream()))
                    {
                        data = br.ReadBytes((int)FormFile.OpenReadStream().Length);
                    }
                    ByteArrayContent bytes = new ByteArrayContent(data);

                    multipartFormDataContent.Add(bytes, "FileName", FormFile.FileName);
                }
                var response = await client.PostAsync(requestUri, multipartFormDataContent);
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return default;
            }
        }

        protected async Task<TOutput> Post<TOutput, TInput>(string requestUri, TInput parameter) where TInput : class
        {
            try
            {
                using var client = new HttpClient
                {
                    BaseAddress = HttpClientBaseAddress
                };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(connectionMode.RemoteToken))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", connectionMode.RemoteToken);
                }
                var content = JsonConvert.SerializeObject(parameter, JsonSerializerSettings);
                var response = await client.PostAsync(requestUri, new StringContent(content, Encoding.UTF8, "application/json"));

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TOutput>(responseContent);
            }
            catch(Exception ex)
            {
                Log.Error(ex.Message);
                return default;
            }            
        }

        protected async Task<T> Put<T>(string requestUri, T parameter) where T : class
        {
            try
            {
                using var client = new HttpClient
                {
                    BaseAddress = HttpClientBaseAddress
                };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(connectionMode.RemoteToken))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", connectionMode.RemoteToken);
                }
                var content = JsonConvert.SerializeObject(parameter, JsonSerializerSettings);
                var response =
                    await client.PutAsync(requestUri, new StringContent(content, Encoding.UTF8, "application/json"));
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return default;
            }

        }
        protected async Task<TOutput> Put<TOutput, TInput>(string requestUri, TInput parameter) where TInput : class
        {
            try
            {
                using var client = new HttpClient
                {
                    BaseAddress = HttpClientBaseAddress
                };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(connectionMode.RemoteToken))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", connectionMode.RemoteToken);
                }
                var content = JsonConvert.SerializeObject(parameter, JsonSerializerSettings);
                var response =
                    await client.PutAsync(requestUri, new StringContent(content, Encoding.UTF8, "application/json"));
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TOutput>(responseContent);
            }
            catch(Exception ex)
            {
                Log.Error(ex.Message);
                return default;
            }
        }
        protected async Task<T> Delete<T>(string requestUri)
        {
            try
            {
                using var client = new HttpClient
                {
                    BaseAddress = HttpClientBaseAddress
                };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(connectionMode.RemoteToken))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", connectionMode.RemoteToken);
                }
                var response = await client.DeleteAsync(requestUri);
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return default;
            }
        }

        protected async Task<TOutput> DeleteWithBody<TOutput, TInput>(string requestUri, TInput requestBody) where TInput : class
        {
            try
            {
                using var client = new HttpClient
                {
                    BaseAddress = HttpClientBaseAddress
                };

                if (!string.IsNullOrEmpty(connectionMode.RemoteToken))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", connectionMode.RemoteToken);
                }

                // 创建自定义请求
                var content = JsonConvert.SerializeObject(requestBody, JsonSerializerSettings);
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri(requestUri),
                    Content = new StringContent(content, Encoding.UTF8, "application/json") // 设置Body
                };

                // 发送请求
                HttpResponseMessage response = await client.SendAsync(request);

                // 处理响应
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TOutput>(responseContent);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return default;
            }
        }

        protected async Task<Exception> GetAPIException(HttpResponseMessage response, string requestUri)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                var content = await response.Content.ReadAsStringAsync();
                return new Exception($"Fail to access api: {requestUri}, error: {content}");
            }
            else
            {
                return new Exception($"Fail to access api: {requestUri}, status: {response.StatusCode}");
            }
        }
        private string GetEncryptedUrl(string requestUri)
        {
            //if (AppSettings.Get<bool>("Encryption:EnableEncryption") == true)
            //{
            //    if (requestUri.Contains('?'))
            //    {
            //        if (!string.IsNullOrEmpty(requestUri.Split('?')[1]))
            //        {
            //            var keybytes = Encoding.UTF8.GetBytes(AppSettings.Get<string>("Encryption:EncryptionKey"));
            //            requestUri = requestUri.Split('?')[0] + "?q=" + AESEncryption.Encrypt(requestUri.Split('?')[1].ToString(), keybytes);
            //        }
            //    }
            //}
            return requestUri;
        }
        public async Task<string> GetQueryString(List<Tuple<string, object>> parameterList)
        {
            string querystring = string.Empty;
            if (parameterList != null)
            {
                if (parameterList.Count > 0)
                {
                    querystring = await Task.Run(() =>
                    {
                        foreach (Tuple<string, object> parameter in parameterList)
                        {
                            //if (AppSettings.Get<bool>("Encryption:EnableEncryption") == true)
                            //{
                            //    querystring += parameter.Item1 + "#" + parameter.Item3.FullName + "=" + parameter.Item2 + "&";
                            //}
                            //else
                            //{
                                querystring += parameter.Item1 + "=" + parameter.Item2 + "&";
                            //}
                        }
                        querystring = "?" + querystring.TrimEnd('&');
                        return querystring;
                    });
                }
            }
            return querystring;
        }
    }
}
