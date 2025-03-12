using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MediaControlDistributionCenter.Services.ApiImps
{
    public abstract class Proxy
    {
        private JsonSerializerSettings jsonSerializerSettings;
        public Uri HttpClientBaseAddress { get; set; }

        public Proxy(string serviceUrl)
        {
            var key = GetType().Namespace;
            HttpClientBaseAddress = new Uri(serviceUrl);
            if (HttpClientBaseAddress == null)
            {
                throw new ArgumentNullException(key);
            }
        }

        protected JsonSerializerSettings JsonSerializerSettings => jsonSerializerSettings ?? (jsonSerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        protected async Task<T> GetResponse<T>(string requestUri, string token = null)
        {
            return await GetResponse<T>(requestUri, 0, token);
        }

        protected async Task<T> GetResponse<T>(string requestUri, int timeOut, string token = null)
        {
            using var client = new HttpClient();
            var webApiRequestTimeout = 1000;
            if (timeOut > 0)
            {
                webApiRequestTimeout = timeOut;
            }
            client.Timeout = new TimeSpan(0, 0, webApiRequestTimeout);
            client.BaseAddress = HttpClientBaseAddress;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            string encryptedUrl = GetEncryptedUrl(requestUri);
            var response = await client.GetAsync(encryptedUrl);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
            throw GetAPIException(response, requestUri);
        }
        protected async Task GetResponse(string requestUri, string token = null)
        {
            int timeOut = 0;
            using var client = new HttpClient();
            var webApiRequestTimeout = 3600;
            if (timeOut > 0)
            {
                webApiRequestTimeout = timeOut;
            }
            client.Timeout = new TimeSpan(0, 0, webApiRequestTimeout);
            client.BaseAddress = HttpClientBaseAddress;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            string encryptedUrl = GetEncryptedUrl(requestUri);
            var response = await client.GetAsync(encryptedUrl);
            if (!response.IsSuccessStatusCode)
            {
                throw GetAPIException(response, requestUri);
            }
        }

        protected async Task<T> PostMultipleFiles<T>(string requestUri, List<IFormFile> formFiles)
        {
            var client = new HttpClient
            {
                BaseAddress = HttpClientBaseAddress
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
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
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
            }
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
            throw GetAPIException(response, requestUri);

        }
        protected async Task<T> Post<T>(string requestUri, T parameter) where T : class
        {
            using var client = new HttpClient
            {
                BaseAddress = HttpClientBaseAddress
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var content = JsonConvert.SerializeObject(parameter, JsonSerializerSettings);
            var response =
                await client.PostAsync(requestUri, new StringContent(content, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
            }
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
            throw GetAPIException(response, requestUri);
        }
        protected async Task<T> Post<T>(string requestUri) where T : class
        {
            using var client = new HttpClient
            {
                BaseAddress = HttpClientBaseAddress
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var content = JsonConvert.SerializeObject(JsonSerializerSettings);
            var response =
                await client.PostAsync(requestUri, new StringContent(content, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
            }
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
            throw GetAPIException(response, requestUri);
        }
        protected async Task<TOutput> Post<TOutput, TInput>(string requestUri, TInput parameter) where TInput : class
        {
            using var client = new HttpClient
            {
                BaseAddress = HttpClientBaseAddress
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var content = JsonConvert.SerializeObject(parameter, JsonSerializerSettings);
            var response = await client.PostAsync(requestUri, new StringContent(content, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<TOutput>(response.Content.ReadAsStringAsync().Result);
            }
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
            throw GetAPIException(response, requestUri);
        }

        protected async Task<Stream> PostAttachedFile<T>(string requestUri, T parameter) where T : class
        {
            using var client = new HttpClient
            {
                BaseAddress = HttpClientBaseAddress
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var content = JsonConvert.SerializeObject(parameter, JsonSerializerSettings);
            var response = await client.PostAsync(requestUri, new StringContent(content, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStreamAsync().Result;
            }
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
            throw GetAPIException(response, requestUri);
        }
        protected async Task<T> Put<T>(string requestUri, T parameter) where T : class
        {
            using var client = new HttpClient
            {
                BaseAddress = HttpClientBaseAddress
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var content = JsonConvert.SerializeObject(parameter, JsonSerializerSettings);
            var response =
                await client.PutAsync(requestUri, new StringContent(content, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
            }
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
            throw GetAPIException(response, requestUri);
        }
        protected async Task<TOutput> Put<TOutput, TInput>(string requestUri, TInput parameter) where TInput : class
        {
            using var client = new HttpClient
            {
                BaseAddress = HttpClientBaseAddress
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var content = JsonConvert.SerializeObject(parameter, JsonSerializerSettings);
            var response =
                await client.PutAsync(requestUri, new StringContent(content, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<TOutput>(response.Content.ReadAsStringAsync().Result);
            }
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }
            throw GetAPIException(response, requestUri);
        }
        protected async Task<T> Delete<T>(string requestUri)
        {
            using var client = new HttpClient
            {
                BaseAddress = HttpClientBaseAddress
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.DeleteAsync(requestUri);
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
            }
            throw GetAPIException(response, requestUri);
        }
        protected async Task Delete(string requestUri)
        {
            using var client = new HttpClient
            {
                BaseAddress = HttpClientBaseAddress
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.DeleteAsync(requestUri);
            if (!response.IsSuccessStatusCode)
            {
                throw GetAPIException(response, requestUri);
            }
        }
        protected async Task<TOutput> DeleteWithBody<TOutput, TInput>(string requestUri, TInput requestBody) where T : class
        {
            using var client = new HttpClient
            {
                BaseAddress = HttpClientBaseAddress
            };

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
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
            }
            throw GetAPIException(response, requestUri);
        }

        protected Exception GetAPIException(HttpResponseMessage response, string requestUri)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return new Exception(requestUri);
            }
            else
            {
                return new Exception(requestUri);
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
