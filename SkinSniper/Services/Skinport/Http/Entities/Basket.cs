using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinSniper.Services.Skinport.Http.Entities
{
    internal class Basket
    {
        internal class BasketNotification
        {
            public bool Error { get; set; }
            public string Title { get; set; }
            public string Text { get; set; }
        }

        public bool Success { get; set; }
        public string? Message { get; set; }
        public BasketNotification? Notification { get; set; }
    }
}
