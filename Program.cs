namespace Create.CSP.GitHub.ScenarioEndToEnd
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using ScenarioEndToEnd.Entities;
    using ScenarioEndToEnd.Helpers;
    using GraphClient = Microsoft.Azure.ActiveDirectory.GraphClient;

    class Program
    {
        #region Members/Properties

        private static CrestApiHelper crestApiHelper { get; set; }
        private static AzureADGraphApiHelper azureADGraphApiHelper { get; set; }

        #endregion

        /// This is a sample console application that shows a End to End Provisioning scenario:
        ///  1. Create a Customer with the CREST API.
        ///  2. Add an Exchange Subscription with the CREST API.
        ///  3. Associate a custom domain with the Azure AD Graph API.
        ///  4. Get new domain TXT record for verification with the Azure AD Graph API.
        ///  5. Verify domain TXT record configuration with the Azure AD Graph API. 
        ///  6. Create a new user with the Azure AD Graph API.
        ///  7. Assign the new user a Exchange license with the Azure AD Graph API.
        ///  5. Create a email alias with the Exchange online cmdlets #########################!!!!!!!!!!!!
        static void Main(string[] args)
        {
            try
            {
                #region Initialize APIs & Tokens

                crestApiHelper = new CrestApiHelper();
                azureADGraphApiHelper = new AzureADGraphApiHelper();

                #endregion

                #region 1. Create a Customer with the CREST API

                // New customer information
                dynamic newCustomer = new
                {
                    domain_prefix = "createit" + new Random().Next(9999),
                    user_name = "admin",
                    password = "pass@word1",
                    profile = new
                    {
                        first_name = "Create",
                        last_name = "It",
                        email = "info@create.pt",
                        company_name = "Create It",
                        culture = "en-US",
                        language = "en",
                        type = "organization",

                        // Note: This data/location depends on your CSP tenant location
                        // It will be validated by Microsoft and will fail if not valid
                        default_address = new
                        {
                            first_name = "Create",
                            last_name = "It",
                            address_line1 = "One Microsoft Way",
                            city = "Redmond",
                            region = "WA",
                            postal_code = "98052-6399",
                            country = "US",
                            phone_number = "19165767760",
                        }
                    }
                };

                // Create customer
                Guid customerId = CreateCustomer(newCustomer);

                // Created customer base information
                var customer = new Customer()
                {
                    CustomerId = customerId,
                    TenantDomainName = newCustomer.domain_prefix + ".onmicrosoft.com"
                };

                #endregion

                #region 2. Add an Exchange Subscription with the CREST API.

                // New order information
                dynamic newOrder = new
                {
                    recipient_customer_id = customer.CustomerId,
                    line_items = new[]
                    {
                        new 
                        {
                            line_item_number = 0,
                            offer_uri = "/3c95518e-8c37-41e3-9627-0ca339200f53/offers/195416c1-3447-423a-b37b-ee59a99a19c4",
                            quantity = 20,
                            friendly_name = "Exchange Online (Plan 1)",
                        }
                    }
                };

                // Place order
                dynamic placedOrder = PlaceOrder(newOrder);

                #endregion

                #region 3. Associate a custom domain with the Azure AD Graph API.

                // NOTE: From here we initialize the Azure AD Graph API helper with the new customer tenant domain
                // Azure AD Graph API operations will be against the customer tenant, using the Pre-Consent delegated access
                azureADGraphApiHelper = new AzureADGraphApiHelper(customer.TenantDomainName);

                // New domain information
                var tenantDomainName = customer.TenantDomainName;
                //string domainName = new Random().Next(9999) + "." + tenantDomainName;
                string domainName = "create.pt";

                // Add domain
                dynamic newDomain = AddCustomerDomain(domainName);

                #endregion

                #region 4. Get new domain TXT record for verification with the Azure AD Graph API.

                dynamic newDomainVerificationRecords = GetCustomerDomainVerificationRecords(domainName);

                // Note: You now need to go to your DNS service provider and place the TXT record

                #endregion

                #region 5. Verify domain TXT record configuration with the Azure AD Graph API.

                // Note: Will only have success if the TXT record is present in the DNS
                VerifyCustomerDomain(domainName);

                #endregion

                #region 6. Create a new user with the Azure AD Graph API.

                // New user information
                GraphClient.IUser newUser = new GraphClient.User()
                {

                    DisplayName = "createitUser" + new Random().Next(9999),
                    AccountEnabled = true,
                    UsageLocation = "US",
                    PasswordProfile = new GraphClient.PasswordProfile()
                    {
                        ForceChangePasswordNextLogin = true,
                        Password = "pass@word1",
                    },
                };
                newUser.MailNickname = newUser.DisplayName;
                newUser.UserPrincipalName = newUser.DisplayName + "@" + tenantDomainName;

                GraphClient.IUser addedUser = AddCustomerUser(newUser);

                #endregion

                #region 7. Assign the new user a Exchange license with the Azure AD Graph API.

                // Get the customer subscribed Skus to determine the Sku id to use in the license assignment
                IReadOnlyList<GraphClient.ISubscribedSku> subscribedSkus = GetSubscribedSkus();

                if (subscribedSkus != null && subscribedSkus.Count > 0)
                {
                    // Filter the license to assign
                    GraphClient.ISubscribedSku subscribeSkuToUser = subscribedSkus
                        .FirstOrDefault(x => x.SkuPartNumber == "EXCHANGESTANDARD");

                    // License to assign information
                    GraphClient.AssignedLicense addLicense = new GraphClient.AssignedLicense()
                    {
                        SkuId = subscribeSkuToUser.SkuId
                    };

                    // Assign license
                    GraphClient.IUser newUserWithLicenseAssigned =
                        AssignOrRemoveLicensesToUser(newUser.UserPrincipalName, addLicense);

                    // Check proper license assignment
                    if (newUserWithLicenseAssigned != null && newUserWithLicenseAssigned.AssignedLicenses != null
                        && newUserWithLicenseAssigned.AssignedLicenses.Count > 0)
                    {
                        GraphClient.AssignedLicense checkAssignedLicense = newUserWithLicenseAssigned.AssignedLicenses
                            .SingleOrDefault(x => x.SkuId == subscribeSkuToUser.SkuId);

                        Debug.Assert(checkAssignedLicense != null, "User assigned license not validated");
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                Console.Write("Error: " + ex.ToString());
            }

            Console.Write("\n\n\nHit enter to exit the app...");
            Console.ReadLine();
        }

        #region Scenario methods

        /// <summary>
        /// Creates the new customer and returns its customer id.
        /// </summary>  
        private static Guid CreateCustomer(dynamic newCustomer)
        {
            try
            {
                // Get request client
                HttpWebRequest request = crestApiHelper.BuildCreateCustomerRequest(newCustomer);
                // Execute request
                dynamic requestResult = request.TryCatchRequest();

                // Return new customer id
                return Guid.Parse(requestResult.customer.id.ToString());
            }
            catch (WebException)
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Gets the customer subscribed skus.
        /// </summary>
        private static IReadOnlyList<GraphClient.ISubscribedSku> GetSubscribedSkus()
        {
            // Get the C# client
            GraphClient.ActiveDirectoryClient activeDirectoryClient = azureADGraphApiHelper.GetActiveDirectoryClient();

            // Get customer subscribed Skus
            var subscribedSkus = activeDirectoryClient.SubscribedSkus.ExecuteAsync().Result;

            return subscribedSkus.CurrentPage;

            // NOTE: More pages can be retrieved by the following loop logic
            //do
            //{
            //    foreach (GraphClient.ISubscribedSku subscribedSku in subscribedSkus.CurrentPage)
            //    {
            //        // Do something with the subscribed Sku
            //    }

            //    // Get next page
            //    subscribedSkus = subscribedSkus.GetNextPageAsync().Result;
            //}
            //// Loop until no more Skus and pages available
            //while (subscribedSkus != null && subscribedSkus.MorePagesAvailable);
        }

        /// <summary>
        /// Assigns or Removes licenses to a customer user.
        /// </summary>    
        private static GraphClient.IUser AssignOrRemoveLicensesToUser(string userPrincipalName, 
            GraphClient.AssignedLicense licenseToAdd, Guid? licenseToRemove = null)
        {
            // Create licenses objects
            IList<GraphClient.AssignedLicense> licensesToAdd = new GraphClient.AssignedLicense[] { };
            IList<Guid> licensesToRemove = new Guid[] { };

            GraphClient.IUser iUser = GetCustomerUser(userPrincipalName);

            if (licenseToAdd != null)
            {
                licensesToAdd = new[] { licenseToAdd };
            }

            if (licenseToRemove != null)
            {
                licensesToRemove = new Guid[] { licenseToRemove.Value };
            }
            // Assign/Remove licenses
            return iUser.AssignLicenseAsync(licensesToAdd, licensesToRemove).Result;
        }

        /// <summary>
        /// Adds a new user to the customer tenant.
        /// </summary>    
        private static GraphClient.IUser AddCustomerUser(GraphClient.IUser newUser)
        {
            // Get the C# client
            GraphClient.ActiveDirectoryClient activeDirectoryClient = azureADGraphApiHelper.GetActiveDirectoryClient();
            // Add user       
            activeDirectoryClient.Users.AddUserAsync(newUser).Wait();

            // Return added user
            return GetCustomerUser(newUser.UserPrincipalName);
        }

        /// <summary>
        /// Gets a user from the customer tenant.
        /// </summary>
        private static GraphClient.IUser GetCustomerUser(string userPrincipalName)
        {
            // Get the C# client
            GraphClient.ActiveDirectoryClient activeDirectoryClient = azureADGraphApiHelper.GetActiveDirectoryClient();

            // Get by user principal name
            return activeDirectoryClient.Users.Where(user => user.UserPrincipalName == userPrincipalName).ExecuteAsync().Result.CurrentPage.ToList().FirstOrDefault();
        }

        /// <summary>
        /// Add a new domain to the customer tenant.
        /// NOTE: The domain will have to be verified by means of the TXT record DNS configuration.
        /// </summary>
        private static dynamic AddCustomerDomain(string newCustomerDomain)
        {
            try
            {
                // Get request client
                HttpWebRequest request = azureADGraphApiHelper.BuildCreateCustomerDomainRequest(newCustomerDomain);
                // Make request
                dynamic requestResult = request.TryCatchRequest();

                return requestResult;
            }
            catch (WebException)
            {
                return null;
            }
        }

        /// <summary>
        /// This method is used to get a customer custom domain TXT verification record.
        /// </summary>   
        private static dynamic GetCustomerDomainVerificationRecords(string domainName)
        {
            try
            {
                // Get request client
                HttpWebRequest request = azureADGraphApiHelper.BuildGetCustomerDomainVerificationRecordsRequest(domainName);
                // Make request
                dynamic requestResult = request.TryCatchRequest();

                return requestResult;
            }
            catch (WebException)
            {
                return null;
            }
        }

        /// <summary>
        /// This method is used to verify a customer customer domain TXT record.
        /// </summary>   
        private static dynamic VerifyCustomerDomain(string domainName)
        {
            try
            {
                // Get request client
                HttpWebRequest request = azureADGraphApiHelper.BuildVerifyCustomerDomainRequest(domainName);
                // Make request
                dynamic requestResult = request.TryCatchRequest();

                return requestResult;
            }
            catch (WebException)
            {
                return null;
            }
        }

        /// <summary>
        /// This method is used to place order on behalf of a customer by a reseller
        /// </summary>     
        private static dynamic PlaceOrder(dynamic newOrder, bool waitForSuccessfulTransition = true)
        {
            try
            {
                // Get request client
                HttpWebRequest request = crestApiHelper.BuildPlaceOrderRequest(newOrder);
                // Make request
                dynamic requestResult = request.TryCatchRequest();

                if (waitForSuccessfulTransition)
                {
                    // Wait until subscription is provisioned to return
                    WaitForSuccessfulTransition(requestResult.line_items[0].resulting_subscription_uri.ToString(),
                        "subscription_provisioned");
                }

                return requestResult;
            }
            catch (WebException)
            {
                return null;
            }
        }

        /// <summary>
        /// Waits for successful transition event for the received subscription.
        /// </summary>
        private static void WaitForSuccessfulTransition(string subscriptionUri, string eventType)
        {
            Boolean transitionComplete = false;

            // Name for the subscription stream 
            string streamName = "SubscriptionStream-" + new Random().Next(999999);
            dynamic subscriptionStream = null;
            HttpWebRequest request;

            try
            {
                // Create subscription stream
                request = crestApiHelper.BuildCreateSubscriptionStreamRequest(streamName);
                subscriptionStream = request.TryCatchRequest();

                // Get subscription stream and wait for event
                request = crestApiHelper.BuildGetSubscriptionStreamRequest(streamName);
                dynamic subscriptionStreamEvents = request.TryCatchRequest();

                // Check for stream event items
                var items = subscriptionStreamEvents.items;

                // TODO: Place some guard so as not not wait here forever
                while (!transitionComplete)
                {
                    foreach (var item in items)
                    {
                        if (subscriptionUri.Equals(item.subscription_uri.ToString()) &&
                            eventType.Equals(item.type.ToString()))
                        {
                            transitionComplete = true;
                            break;
                        }
                    }
                    if (!transitionComplete)
                    {
                        // Wait for a while
                        Thread.Sleep(5000);

                        // Mark page as complete and retrieve the next page
                        request = crestApiHelper.BuildMarkPageAsCompletedInStreamRequest(
                            subscriptionStreamEvents.links.completion.href.ToString());

                        subscriptionStream = request.TryCatchRequest();
                        items = subscriptionStream.items;
                    }
                }
            }
            finally
            {
                if (subscriptionStream != null)
                {
                    // Delete subscription stream
                    request = crestApiHelper.BuildDeleteSubscriptionStreamRequest(streamName);
                    dynamic deletedStreamEvents = request.TryCatchRequest();
                }
            }
        }

        #endregion
    }
}
