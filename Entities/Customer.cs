namespace Create.CSP.GitHub.ScenarioEndToEnd.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Customer (or contract).
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Customer Id (Cid)
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// The customer display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The customer tenant name
        /// </summary>
        public string TenantDomainName { get; set; }

        /// <summary>
        /// Customer Tenant Id or Customer Microsoft Id.
        /// </summary>
        public Guid CustomerTenantId { get; set; }
    }
}
