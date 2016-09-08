using CommerceServer.Core;
using CommerceServer.Core.Orders;
using CommerceServer.Core.Runtime.Orders;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Orders;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Pipelines;
using Sitecore.Commerce.Connect.CommerceServer.Pipelines;
using Sitecore.Commerce.Entities.WishLists;
using Sitecore.Commerce.Pipelines;
using Sitecore.Commerce.Pipelines.WishLists.Common;
using Sitecore.Commerce.Services;
using Sitecore.Commerce.Services.WishLists;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Reference.Storefront.Connect.Pipelines.Utilities;
using Sitecore.Reference.Storefront.Connect.Pipelines.WishLists;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Sitecore.Reference.Storefront.Connect.Pipelines.WishLists
{
    /// <summary>
    /// 
    /// </summary>
    public class GetWishList : WishListPipelineProcessor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public override void Process(ServicePipelineArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");
            Assert.ArgumentNotNull((object)args.Request, "args.Request");
            Assert.ArgumentNotNull((object)args.Result, "args.Result");
            Assert.IsTrue(args.Request is GetWishListRequest, "args.Request is GetWishListRequest");
            Assert.IsTrue(args.Result is GetWishListResult, "args.Result is GetWishListResult");

            var request = (GetWishListRequest)args.Request;
            var result = (GetWishListResult)args.Result;

            Assert.IsNotNullOrEmpty(request.UserId, "getWishListRequest.UserId");
            Assert.IsNotNullOrEmpty(request.ShopName, "getWishListRequest.ShopName");

            var userId = request.UserId;
            var shopName = request.ShopName;
            var wishListId = Guid.Parse(request.WishListId);

            Guid csUserId;
            if (!UserUtility.ResolveUserId(request.UserId, out csUserId))
            {
                result.Success = false;
                var systemMessage = new SystemMessage()
                {
                    Message = Translate.Text("GetWishList was unable to retrieve user \"{0}\".", (object)request.UserId)
                };
                result.SystemMessages.Add(systemMessage);

                return;
            }

            var instance = CommerceTypeLoader.CreateInstance<IOrderRepository>();
            var basket = instance.GetBasket(csUserId, wishListId);
            result.WishList = this.TranslateBasketToWishList(userId, shopName, basket);
        }

        /// <summary>
        /// Translates the commerce server basekt to the wish list entity 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="shopName"></param>
        /// <param name="basket"></param>
        /// <returns></returns>
        protected virtual WishList TranslateBasketToWishList(string userId, string shopName, Basket basket)
        {
            var request = new TranslateOrderGroupToWishListRequest(userId, shopName, basket);
            var result = PipelineUtility.RunCommerceConnectPipeline<TranslateOrderGroupToWishListRequest, TranslateOrderGroupToWishListResult>(Constants.TranslateOrderGroupToWishList, request);
            return result.WishList;
        }
    }
}