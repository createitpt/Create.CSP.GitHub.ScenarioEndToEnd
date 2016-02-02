# Create.CSP
|create|it| Microsoft Cloud Solution Provider API Integration Samples

These are samples in C# for using the commerce APIs for Microsoft Partner Center, and the Azure AD Graph APIs. 
These CREST APIs are documented at https://msdn.microsoft.com/en-us/library/partnercenter/dn974944.aspx.
The Azure AD Graph API are documented at https://msdn.microsoft.com/Library/Azure/Ad/Graph/api/api-catalog.

# Prerequisites
The prerequisites are similar to the other Partner Center and Azure available samples.
Please refer to these references for detailed information:
 - CREST API: https://github.com/PartnerCenterSamples/Commerce-API-DotNet (Don't forget to configure pre-consent)
 - Azure AD Graph API: https://github.com/Azure-Samples/active-directory-dotnet-graphapi-console

# App Configuration
The sample application has several configurable settings in the app.config file. Set the values as appropriate for your tenant.

| Configuration Key  | Description |
| ------------- | ------------- |
| AzureADAppId-NativeApplication  | The id of the native tenant application registered in the CSP directory that provisions resources in the customer tenant  |
| CSPServiceUsername  | Partner Center service account username  |
| CSPServicePassword  | Partner Center service account password  |
| CSPTenantName  | The default domain of the reseller in Microsoft Azure. (This is typically an "onmicrosoft.com" domain.)  |
| CSPTenantId  | The Microsoft Id of the reseller. Used to access CREST APIs.  |
| CSPAppId  | The id of the application registered in the Partner Center. This id is used to access the CREST APIs.  |
| CSPAppKey  | The key of the application registered in the Partner Center.  |


