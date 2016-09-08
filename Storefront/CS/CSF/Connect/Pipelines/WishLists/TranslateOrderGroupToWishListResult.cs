using Sitecore.Commerce.Connect.CommerceServer.Pipelines;
using Sitecore.Commerce.Entities.WishLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Reference.Storefront.Connect.Pipelines.WishLists
{
    /// <summary>
    /// 
    /// </summary>
    public class TranslateOrderGroupToWishListResult : CommerceResult
    {
        /// <summary>
        /// 
        /// </summary>
        public WishList WishList { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TranslateOrderGroupToWishListResult()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wishList"></param>
        public TranslateOrderGroupToWishListResult(WishList wishList)
        {
            this.WishList = wishList;
        }
    }
}