# Create.CSP.ScenarioEndToEnd
|create|it| Microsoft Cloud Solution Provider API Integration Samples

These are samples in C# for using the commerce APIs for Microsoft Partner Center, and the Azure AD Graph APIs. 

The CREST APIs are documented at https://msdn.microsoft.com/en-us/library/partnercenter/dn974944.aspx.
The Azure AD Graph APIs are documented at https://msdn.microsoft.com/Library/Azure/Ad/Graph/api/api-catalog.

# Samples
| Name  | Description |
| ------------- | ------------- |
| End-to-End  | Provides a sample for a End-to-End provisioning scenario including:<br> 1. Create a Customer with the CREST API.<br> 2. Add an Exchange Subscription with the CREST API.<br> 3. Associate a custom domain with the Azure AD Graph API.<br> 4. Get new domain TXT record for verification with the Azure AD Graph API.<br> 5. Verify domain TXT record configuration with the Azure AD Graph API.<br> 6. Create a new user with the Azure AD Graph API.<br> 7. Assign the new user a Exchange license with the Azure AD Graph API.|

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


