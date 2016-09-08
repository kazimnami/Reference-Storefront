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
    public class TranslateOrderGroupToWishListHeaderResult : CommerceResult
    {
        /// <summary>
        /// 
        /// </summary>
        public WishListHeader WishList { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TranslateOrderGroupToWishListHeaderResult()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wishList"></param>
        public TranslateOrderGroupToWishListHeaderResult(WishListHeader wishList)
        {
            this.WishList = wishList;
        }
    }
}