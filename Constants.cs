namespace Create.CSP.GitHub.ScenarioEndToEnd
{
    using System;
    using System.Configuration;

    public class Constants
    {
        public static string AAD_INSTANCE = ConfigurationManager.AppSettings["AADInstance"];
        public static string CSP_TENANT_NAME = ConfigurationManager.AppSettings["CSPTenantName"];
        public static string CSP_TENANT_ID = ConfigurationManager.AppSettings["CSPTenantId"];
        public static string AZURE_AD_APP_ID_NATIVE_APP = ConfigurationManager.AppSettings["AzureADAppId-NativeApplication"];
        public static string GRAPH_RESOURCE_URL = ConfigurationManager.AppSettings["GraphResourceUrl"];
        public static string CREST_RESOURCE_URL = ConfigurationManager.AppSettings["CrestResourceUrl"];
        public static string CSP_APP_ID = ConfigurationManager.AppSettings["CSPAppId"];
        public static string CSP_APP_KEY = ConfigurationManager.AppSettings["CSPAppKey"];
        public static string AUTHENTICATION_AUTHORITY = String.Format(AAD_INSTANCE, CSP_TENANT_NAME);
        public static string CSP_SERVICE_USERNAME = ConfigurationManager.AppSettings["CSPServiceUsername"];
        public static string CSP_SERVICE_PASSWORD = ConfigurationManager.AppSettings["CSPServicePassword"];
    }
}