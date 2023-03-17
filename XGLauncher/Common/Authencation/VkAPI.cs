using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xNet;

namespace XGL.Networking.Authencation {

    public class VkAPI {

        public static string __APPID = App.VKConnectorData;  //ID приложения.
        const string __VKAPIURL = "https://api.vk.com/method/";  //Ссылка для запросов.
        readonly string _Token;  //Токен, использующийся при запросах.

        public VkAPI(string AccessToken) => _Token = AccessToken;

        //Получение заданной информации о пользователе с указанным ID. 
        public Dictionary<string, string> GetInformation(string UserId, string[] Fields) {
            HttpRequest GetInformation = new HttpRequest();
            GetInformation.AddUrlParam("user_ids", UserId);
            GetInformation.AddUrlParam("access_token", _Token);
            GetInformation.AddUrlParam("v", "5.52");
            string Params = "";
            foreach (string i in Fields) {
                Params += i + ",";
            }
            Params = Params.Remove(Params.Length - 1);
            GetInformation.AddUrlParam("fields", Params);
            string Result = GetInformation.Get(__VKAPIURL + "users.get").ToString();
            Result = Result.Substring(13, Result.Length - 15);
            Dictionary<string, string> Dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(Result);
            return Dict;
        }

        //Перевод ID города в название.
        public string GetCityById(string CityId) {
            HttpRequest GetCityById = new HttpRequest();
            GetCityById.AddUrlParam("city_ids", CityId);
            GetCityById.AddUrlParam("access_token", _Token);
            GetCityById.AddUrlParam("v", "5.52");
            string Result = GetCityById.Get(__VKAPIURL + "database.getCitiesById").ToString();
            Result = Result.Substring(13, Result.Length - 15);
            Dictionary<string, string> Dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(Result);
            return Dict["name"];
        }

        //Перевод ID страны в название.
        public string GetCountryById(string CountryId) {
            HttpRequest GetCountryById = new HttpRequest();
            GetCountryById.AddUrlParam("country_ids", CountryId);
            GetCountryById.AddUrlParam("access_token", _Token);
            GetCountryById.AddUrlParam("v", "5.52");
            string Result = GetCountryById.Get(__VKAPIURL + "database.getCountriesById").ToString();
            Result = Result.Substring(13, Result.Length - 15);
            Dictionary<string, string> Dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(Result);
            return Dict["name"];
        }

    }
}
