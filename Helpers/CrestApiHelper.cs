namespace Create.CSP.GitHub.ScenarioEndToEnd.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Create.CSP.GitHub.ScenarioEndToEnd.Entities;

    public class CrestApiHelper
    {
        #region Members/Properties

        private AuthorizationToken adAuthorizationToken { get; set; }
        private AuthorizationToken saAuthorizationToken { get; set; }
        private string resellerCid { get; set; }

        #endregion

        #region Constructor & Client Initialization

        /// <summary>
        /// Get a object instance with the tokens for requests initialized for the CSP tenant.
        /// (if empty parameter).
        /// </summary>
        public CrestApiHelper()
        {
            // Get the AD Token
            adAuthorizationToken = ResellerHelper.GetAD_Token(Constants.CSP_TENANT_NAME, Constants.CSP_APP_ID, Constants.CSP_APP_KEY);
            // Using the ADToken get the sales agent token
            saAuthorizationToken = ResellerHelper.GetSA_Token(adAuthorizationToken);
            // Get the Reseller Cid, you can cache this value
            resellerCid = ResellerHelper.GetCid(Constants.CSP_TENANT_ID, saAuthorizationToken.AccessToken);
        }

        #endregion

        #region Request builders/helpers

        /// <summary>
        /// Generates a request with the received parameters, for the CREST API.
        /// </summary>   
        public static HttpWebRequest GenerateRequest(string method, string requestUri, string token)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(
                Constants.CREST_RESOURCE_URL + requestUri);

            request.Method = method;
            request.ContentType = "application/json";
            request.Accept = "application/json";

            request.Headers.Add("api-version", "2015-03-31");
            request.Headers.Add("x-ms-correlation-id", Guid.NewGuid().ToString());
            request.Headers.Add("x-ms-tracking-id", Guid.NewGuid().ToString());
            request.Headers.Add("Authorization", "Bearer " + token);

            return request;
        }

        /// <summary>
        /// Generates a GET request.
        /// </summary>   
        public static HttpWebRequest GenerateGetRequest(string requestUri, string token)
        {
            return GenerateRequest("GET", requestUri, token);
        }

        /// <summary>
        /// Generates a POST request.
        /// </summary>   
        public static HttpWebRequest GeneratePostRequest(string requestUri, string token)
        {
            return GenerateRequest("POST", requestUri, token);
        }

        /// <summary>
        /// Generates a PUT request.
        /// </summary>   
        public static HttpWebRequest GeneratePutRequest(string requestUri, string token)
        {
            return GenerateRequest("PUT", requestUri, token);
        }

        /// <summary>
        /// Generates a DELETE request.
        /// </summary>     
        public static HttpWebRequest GenerateDeleteRequest(string requestUri, string token)
        {
            return GenerateRequest("DELETE", requestUri, token);
        }

        #endregion

        #region Customer CREST API request builders/helpers

        /// <summary>
        /// Generates a POST (Create) customer request.
        /// </summary>    
        public HttpWebRequest BuildCreateCustomerRequest(dynamic newCustomer)
        {
            // Get request client
            HttpWebRequest request = CrestApiHelper.GeneratePostRequest(
               string.Format("/{0}/customers/create-reseller-customer", resellerCid),
               saAuthorizationToken.AccessToken);

            // Request body
            string content = JsonConvert.SerializeObject(newCustomer);
            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(content);
            }

            return request;
        }

        #endregion

        #region Order CREST API request builders/helpers

        /// <summary>
        /// Generates a POST (Place) order request.
        /// </summary>    
        public HttpWebRequest BuildPlaceOrderRequest(dynamic newOrder)
        {
            // Get request client
            HttpWebRequest request = CrestApiHelper.GeneratePostRequest(
                  string.Format("/{0}/orders", resellerCid),
                  saAuthorizationToken.AccessToken);

            // Request body
            string content = JsonConvert.SerializeObject(newOrder);
            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(content);
            }

            return request;
        }

        /// <summary> 
        /// This method is to create a stream for a reseller to hear all subscription events 
        /// </summary>    
        public HttpWebRequest BuildCreateSubscriptionStreamRequest(string streamName)
        {
            HttpWebRequest request = CrestApiHelper.GeneratePutRequest(
                    string.Format("/{0}/subscription-streams/{0}/{1}", resellerCid, streamName),
                    saAuthorizationToken.AccessToken);

            string content = string.Format("{{\"start_time\": \"{0}\", \"page_size\": \"{1}\"}}",
                                    string.Format("{0:MM/dd/yyyy HH:mm:ss}", DateTime.UtcNow), "100");

            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(content);
            }

            return request;
        }

        /// <summary>
        /// Builds the mark page as completed in stream request.
        /// </summary>
        public HttpWebRequest BuildMarkPageAsCompletedInStreamRequest(string completedUri)
        {
            HttpWebRequest request = CrestApiHelper.GenerateGetRequest(
                 string.Format("/{0}", completedUri), saAuthorizationToken.AccessToken);

            return request;
        }

        /// <summary>
        /// Builds the delete subscription stream request.
        /// </summary>
        public HttpWebRequest BuildDeleteSubscriptionStreamRequest(string streamName)
        {
            HttpWebRequest request = CrestApiHelper.GenerateDeleteRequest(
                   string.Format("/{0}/subscription-streams/{0}/{1}", resellerCid, streamName),
                   saAuthorizationToken.AccessToken);

            return request;
        }

        /// <summary>
        /// Builds the get subscription stream request.
        /// </summary>
        public HttpWebRequest BuildGetSubscriptionStreamRequest(string streamName)
        {
            HttpWebRequest request = CrestApiHelper.GenerateGetRequest(
                   string.Format("/{0}/subscription-streams/{0}/{1}/pages", resellerCid, streamName),
                   saAuthorizationToken.AccessToken);

            return request;
        }

        #endregion
    }
}
