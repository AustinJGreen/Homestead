using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Homestead.Services
{
    public class AttomData
    {
        private ILogger log;

        private string apiKey;
        private RestClient rs;

        public int GetHouseEstimate(string houseAddress, string city)
        {
            var request = new RestRequest("property/basicprofile", DataFormat.Json);
            request.AddHeader("apikey", apiKey);
            request.AddHeader("accept", "application/json");
            request.AddParameter("address1", houseAddress);
            request.AddParameter("address2", city);

            var response = rs.Get(request);
            if (!response.IsSuccessful)
            {
                log.LogWarning(response.ErrorException, "Property request was unsuccessful");
                return -1;
            }

            JObject jo = (JObject)JsonConvert.DeserializeObject(response.Content);
            var properties = (JArray)jo["property"];
            if (properties.Count > 0)
            {
                var property = properties[0];
                return property["assessment"]["assessed"]["assdTtlValue"].Value<int>();
            }

            return -1;
        }

        public AttomData(string apiKey, ILoggerFactory factory)
        {
            rs = new RestClient("https://api.gateway.attomdata.com/propertyapi/v1.0.0/");
            this.apiKey = apiKey;
        }
    }
}
