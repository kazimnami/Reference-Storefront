using CommerceServer.Core.Runtime.Orders;
using Sitecore.Commerce.Connect.CommerceServer.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Reference.Storefront.Connect.Pipelines.WishLists
{
    /// <summary>
    /// 
    /// </summary>
    public class TranslateOrderGroupToWishListRequest : TranslateOrderGroupRequest
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="shopName"></param>
        /// <param name="orderGroup"></param>
        public TranslateOrderGroupToWishListRequest(string userId, string shopName, OrderGroup orderGroup)
            : base(userId, shopName, orderGroup) { }
    }
}