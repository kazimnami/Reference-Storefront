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
    public class TranslateOrderGroupToWishListHeaderRequest : TranslateOrderGroupRequest
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="shopName"></param>
        /// <param name="orderGroup"></param>
        public TranslateOrderGroupToWishListHeaderRequest(string userId, string shopName, OrderGroup orderGroup)
            : base(userId, shopName, orderGroup) { }
    }

    /// <summary>
    /// 
    /// </summary>
    public class TranslateOrderGroupRequest : CommerceRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ShopName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public OrderGroup OrderGroup { get; set; }

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="shopName"></param>
        /// <param name="orderGroup"></param>
        public TranslateOrderGroupRequest(string userId, string shopName, OrderGroup orderGroup)
        {
            this.UserId = userId;
            this.ShopName = shopName;
            this.OrderGroup = orderGroup;
        }
    }
}