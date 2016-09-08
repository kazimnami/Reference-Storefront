using Sitecore.Commerce.Pipelines.WishLists.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Commerce.Pipelines;
using Sitecore.Diagnostics;
using Sitecore.Commerce.Services.WishLists;
using CommerceServer.Core;
using Sitecore.Commerce.Connect.CommerceServer;
using CommerceServer.Core.Orders;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Pipelines;
using System.Data;
using Sitecore.Commerce.Services;
using Sitecore.Commerce.Connect.CommerceServer.Orders;
using System.Globalization;
using Sitecore.Commerce.Entities.WishLists;
using CommerceServer.Core.Runtime.Orders;
using Sitecore.Reference.Storefront.Connect.Pipelines.Utilities;
using Sitecore.Globalization;
using Sitecore.Commerce.Connect.CommerceServer.Pipelines;

namespace Sitecore.Reference.Storefront.Connect.Pipelines.WishLists
{
    /// <summary>
    /// 
    /// </summary>
    public class GetWishLists : WishListPipelineProcessor
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
            Assert.IsTrue(args.Request is GetWishListsRequest, "args.Request is GetWishListsRequest");
            Assert.IsTrue(args.Result is GetWishListsResult, "args.Result is GetWishListsResult");

            var getWishListsRequest = (GetWishListsRequest)args.Request;
            var getWishListsResult = (GetWishListsResult)args.Result;

            Assert.IsNotNullOrEmpty(getWishListsRequest.UserId, "getWishListsRequest.UserId");
            Assert.IsNotNullOrEmpty(getWishListsRequest.ShopName, "getWishListsRequest.ShopName");

            var userId = getWishListsRequest.UserId;
            var shopName = getWishListsRequest.ShopName;

            Guid csUserId;
            if (!UserUtility.ResolveUserId(getWishListsRequest.UserId, out csUserId))
            {
                getWishListsResult.Success = false;
                var systemMessage = new SystemMessage()
                {
                    Message = Translate.Text("GetWishLists was unable to retrieve user \"{0}\".", (object)getWishListsRequest.UserId)
                };
                getWishListsResult.SystemMessages.Add(systemMessage);

                return;
            }

            var basketManager = CommerceTypeLoader.CreateInstance<ICommerceServerContextManager>().OrderManagementContext.BasketManager;

            var requestInformation = GetCartsRequestInformation.Get((ServiceProviderRequest)getWishListsRequest);
            var searchableProperties = basketManager.GetSearchableProperties(requestInformation == null || string.IsNullOrWhiteSpace(requestInformation.SearchLanguage) ? "en-us" : requestInformation.SearchLanguage);

            var searchClauseFactory = basketManager.GetSearchClauseFactory(searchableProperties, "Basket");
            var searchClause = this.PrepareSearchClauses(searchClauseFactory, csUserId);

            var searchOptions = new SearchOptions();
            searchOptions.PropertiesToReturn = string.Format(
                (IFormatProvider)CultureInfo.InvariantCulture,
                "{0},{1}",
                new object[2]{
                        (object) "OrderGroupId",
                        (object) "SoldToId"
                }
            );

            var dataSet = basketManager.SearchBaskets(searchClause, searchOptions);

            if (dataSet == null)
            {
                return;
            }

            var wishLists = new List<WishListHeader>();
            foreach (DataRow row in (InternalDataCollectionBase)dataSet.Tables["Baskets"].Rows)
            {
                var orderGroupId = (Guid)row["OrderGroupId"];
                var basket = CommerceTypeLoader.CreateInstance<IOrderRepository>().GetBasket(csUserId, orderGroupId);
                wishLists.Add(this.TranslateBasketToWishListHeader(userId, shopName, basket));
            }

            getWishListsResult.WishLists = wishLists.AsReadOnly();


            //List<WishList> wishListList = new List<WishList>();
            //foreach (DataRow row in (InternalDataCollectionBase)dataSet.Tables["Baskets"].Rows)
            //{
            //    Guid orderGroupId = (Guid)row["OrderGroupId"];
            //    Basket basket = CommerceTypeLoader.CreateInstance<IOrderRepository>().GetBasket(csUserId, orderGroupId);
            //    wishListList.Add(this.TranslateBasketToWishList(userId, shopName, basket));
            //}
        }

        /// <summary>
        /// Adds the search clauses to the given searchClauseList.
        /// </summary>
        /// <param name="searchClauseFactory">The search clause factory.</param>
        /// <param name="csUserId">The user id.</param>
        protected virtual SearchClause PrepareSearchClauses(SearchClauseFactory searchClauseFactory, Guid csUserId)
        {
            var userIdClause = searchClauseFactory.CreateClause(ExplicitComparisonOperator.Equal, "SoldToId", csUserId.ToString());

            // I'm not sure if it's more efficient to do this in the search or in memory after retrieving all carts for the sure.
            var nameClause = searchClauseFactory.CreateClause(ExplicitComparisonOperator.BeginsWith, "Name", Constants.WishListNameBeginning);

            return searchClauseFactory.IntersectClauses(userIdClause, nameClause);
        }

        /// <summary>
        /// Translates the commerce server basekt to the wish list entity 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="shopName"></param>
        /// <param name="basket"></param>
        /// <returns></returns>
        protected virtual WishListHeader TranslateBasketToWishListHeader(string userId, string shopName, Basket basket)
        {
            var request = new TranslateOrderGroupToWishListHeaderRequest(userId, shopName, basket);
            var result = PipelineUtility.RunCommerceConnectPipeline<TranslateOrderGroupToWishListHeaderRequest, TranslateOrderGroupToWishListHeaderResult>(Constants.TranslateOrderGroupToWishListHeader, request);
            return result.WishList;
        }

        ///// <summary>
        ///// Translates the commerce server basekt to the wish list entity 
        ///// </summary>
        ///// <param name="userId"></param>
        ///// <param name="shopName"></param>
        ///// <param name="basket"></param>
        ///// <returns></returns>
        //protected virtual WishList TranslateBasketToWishList(string userId, string shopName, Basket basket)
        //{
        //    var request = new TranslateOrderGroupToEntityRequest(userId, shopName, basket);
        //    var result = PipelineUtility.RunCommerceConnectPipeline<TranslateOrderGroupToEntityRequest, TranslateOrderGroupToEntityResult>(Constants.TranslateOrderGroupToEntityWishList, request);
        //    return result.WishList;
        //}
    }
}