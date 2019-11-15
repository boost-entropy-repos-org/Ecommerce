using System.Collections.Generic;
using MrCMS.Web.Apps.Ecommerce.Entities.Orders;

namespace MrCMS.Web.Apps.Ecommerce.Payment.Elavon.Models
{
    public class ElavonResponse
    {
        public bool Success { get; set; }
        public string ErrorDescription { get; set; }
        public Order Order { get; set; }
    }
}