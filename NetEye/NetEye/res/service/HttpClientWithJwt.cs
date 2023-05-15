using NetEye.res.model;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace NetEye.res.service
{
    public class HttpClientWithJwt
    {
        #region Fields

        private static HttpClientWithJwt _instance;
        private static readonly object Lock = new object();
        private readonly RestClient _httpClient;
        private const string BaseUrl = "http://5.128.221.139:7119/api";

        #endregion

        private HttpClientWithJwt()
        {
            _httpClient = new RestClient(BaseUrl);
        }

        public static HttpClientWithJwt GetInstance()
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new HttpClientWithJwt();
                    }
                }
            }

            return _instance;
        }

        #region Users

        /// <summary>
        /// Отправляет запрос серверу на аутентификацию. Хэширование пароля выполняется внутри метода.
        /// </summary>
        /// <param name="email">Почта пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <returns>Аутентифицированного пользователя. Если сервер не смог аутентифицировать пользователя - null.</returns>
        public AuthUser Authorization(string email, string password)
        {
            var hashPass = GetHash(password);
            var request = new RestRequest("auth");
            request.AddQueryParameter("email", email);
            request.AddQueryParameter("password", hashPass);
            var response = _httpClient.ExecuteGet<AuthUser>(request);

            return !response.IsSuccessful ? null : response.Data;
        }

        /// <summary>
        /// Возвращает пользователя по id.
        /// </summary>
        /// <param name="id">Id интересующего пользователя.</param>
        /// <returns>Пользователя с укзанным id, если пользователь с таким id не был найден, возвращает null</returns>
        public User GetUserById(int id)
        {
            var request = new RestRequest($"users/{id}", Method.Get);
            return ExecuteRequest<User>(request).Item1;
        }

        #endregion

        #region RepairRequest

        /// <summary>
        /// Возвращает заявку на ремонт по указанному id.
        /// </summary>
        /// <param name="id">Id интересующей заявки.</param>
        /// <returns>Заявку на ремонт, если завявка с таким id не найдена, возвращает null.</returns>
        public RepairRequest GetRepairRequestById(int id)
        {
            var request = new RestRequest($"repairrequest/{id}", Method.Get);
            return ExecuteRequest<RepairRequest>(request).Item1;
        }

        /// <summary>
        /// Возвращает список всех заявок на ремонт.
        /// </summary>
        /// <returns>Список всех заявок на ремонт.</returns>
        public List<RepairRequest> GetAllRepairRequest()
        {
            var request = new RestRequest("repairrequest", Method.Get);
            return ExecuteRequest<List<RepairRequest>>(request).Item1;
        }

        /// <summary>
        /// Добавляет новую заявку на ремонт.
        /// </summary>
        /// <param name="repairRequest">Новая заявка.</param>
        /// <returns>Id добавленной заявки.</returns>
        public int PostRepairRequest(RepairRequest repairRequest)
        {
            var request = new RestRequest("repairrequest", Method.Post);
            request.AddJsonBody(repairRequest);
            return ExecuteRequest<int>(request).Item1;
        }

        /// <summary>
        /// Обновляет данные заявки. Можно назанчить исполнителя заявки или изменить статус заявки.
        /// </summary>
        /// <param name="newRepairRequest">Обновлённая заявка.</param>
        /// <returns>True, если данные о заявке успешно обновленны, иначе false.</returns>
        public bool PutRepairRequest(RepairRequest newRepairRequest)
        {
            var request = new RestRequest("repairrequest", Method.Put);
            request.AddJsonBody(newRepairRequest);
            return ExecuteRequest<RepairRequest>(request).Item2;
        }

        #endregion

        #region TechEquipment

        /// <summary>
        /// Возвращает сетевое оборудование по указанному id.
        /// </summary>
        /// <param name="id">Id интересующего оборудования.</param>
        /// <returns>Сетевое оборудование, если оборудование с таким id не найдено, возвращает null.</returns>
        public TechEquipment GetTechEquipmentById(string id)
        {
            var request = new RestRequest($"techequipment/{id}", Method.Get);
            return ExecuteRequest<TechEquipment>(request).Item1;
        }

        /// <summary>
        /// Возвращает список всего сетевого оборудования.
        /// </summary>
        /// <returns>Список всего сетевого оборудования.</returns>
        public List<TechEquipment> GetAllTechEquipment()
        {
            var request = new RestRequest("techequipment", Method.Get);
            return ExecuteRequest<List<TechEquipment>>(request).Item1;
        }

        /// <summary>
        /// Добавляет новое сетевое оборудование.
        /// </summary>
        /// <param name="techEquipment">Новое оборудование.</param>
        /// <returns>Id созданного оборудования.</returns>
        public string PostTechEquipment(TechEquipment techEquipment)
        {
            var request = new RestRequest("techequipment", Method.Post);
            request.AddJsonBody(techEquipment);
            return ExecuteRequest<string>(request).Item1;
        }

        /// <summary>
        /// Обновляет ip адресс сетевого оборудования.
        /// </summary>
        /// <param name="techEquipment">Оборудование с обновлённым ip адресом.</param>
        /// <returns>True, если данные о оборудовании успешно обновленны, иначе false.</returns>
        public bool PutTechEquipment(TechEquipment techEquipment)
        {
            var request = new RestRequest("techequipment", Method.Put);
            request.AddJsonBody(techEquipment);
            return ExecuteRequest<TechEquipment>(request).Item2;
        }

        /// <summary>
        /// Удаляет сетевое оборудование по id.
        /// </summary>
        /// <param name="id">Id удаляемого оборудования.</param>
        /// <returns>True, если оборудование было успешно удаленно, иначе false.</returns>
        public bool DeleteTechEquipmentById(string id)
        {
            var request = new RestRequest($"techequipment/{id}", Method.Delete);
            return ExecuteRequest<TechEquipment>(request).Item2;
        }

        #endregion

        #region PrivateMethods

        private (T, bool) ExecuteRequest<T>(RestRequest request)
        {
            var response = _httpClient.Execute<T>(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                UpdateTokens();
                response = _httpClient.Execute<T>(request);
            }

            return (response.Data, response.IsSuccessful);
        }

        private void UpdateTokens()
        {
            var request = new RestRequest("auth");
            var response = _httpClient.ExecutePut(request);

            if (!response.IsSuccessful)
                throw new Exception("Refresh token is not a valid");
        }

        private string GetHash(string inputData)
        {
            using (SHA256 sha256 = new SHA256Managed())
            {
                var data = Encoding.UTF8.GetBytes(inputData);
                var hash = sha256.ComputeHash(data);

                StringBuilder builder = new StringBuilder(128);

                foreach (var b in hash)
                {
                    builder.Append(b.ToString("X2"));
                }

                return builder.ToString().ToLower();
            }
        }

        #endregion
    }

}
