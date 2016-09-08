using CommerceServer.Core.Runtime.Orders;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Connect.CommerceServer.Pipelines;
using Sitecore.Commerce.Entities;
using Sitecore.Commerce.Entities.Carts;
using Sitecore.Commerce.Entities.WishLists;
using Sitecore.Commerce.Pipelines;
using Sitecore.Commerce.Services;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Reference.Storefront.Connect.Pipelines.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace Sitecore.Reference.Storefront.Connect.Pipelines.WishLists
{
    /// <summary>
    /// Translates the commerce server order group to the wish list entity 
    /// </summary>
    public class TranslateOrderGroupToWishListHeader : CommerceTranslateProcessor
    {
        /// <summary>Gets the entity factory.</summary>
        /// <value>The entity factory.</value>
        public IEntityFactory EntityFactory { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sitecore.Reference.Storefront.Connect.Pipelines.WishLists.TranslateOrderGroupToWishListHeader" /> class.
        /// </summary>
        /// <param name="entityFactory">The entity factory.</param>
        public TranslateOrderGroupToWishListHeader(IEntityFactory entityFactory)
        {
            this.EntityFactory = entityFactory;
        }

        /// <summary>Processes the specified arguments.</summary>
        /// <param name="args">The arguments.</param>
        public override void Process(ServicePipelineArgs args)
        {
            Assert.IsNotNull((object)args, "args");
            Assert.IsNotNull((object)args.Request, "args.Request");
            Assert.IsNotNull((object)args.Result, "args.Result");
            Assert.IsTrue(args.Request is TranslateOrderGroupToWishListHeaderRequest, "args.Request is TranslateOrderGroupToWishListHeaderRequest");
            Assert.IsTrue(args.Result is TranslateOrderGroupToWishListHeaderResult, "args.Result is TranslateOrderGroupToWishListHeaderResult");

            TranslateOrderGroupToWishListHeaderRequest request = (TranslateOrderGroupToWishListHeaderRequest)args.Request;
            TranslateOrderGroupToWishListHeaderResult result = (TranslateOrderGroupToWishListHeaderResult)args.Result;

            if (!OrderGroupUtility.ValidOrderGroupForWishList(request, result))
            {
                result.Success = false;
                return;
            }

            WishListHeader destination = this.EntityFactory.Create<WishListHeader>("WishListHeader");
            this.TranslateToWishListHeader(request, request.OrderGroup, destination);
            ((TranslateOrderGroupToWishListHeaderResult)args.Result).WishList = destination;
        }

        /// <summary>Translates properties.</summary>
        /// <param name="request">The request.</param>
        /// <param name="origin">The order group</param>
        /// <param name="destination">The wish list header</param>
        protected virtual void TranslateToWishListHeader(TranslateOrderGroupToWishListHeaderRequest request, OrderGroup origin, WishListHeader destination)
        {
            destination.Name = origin.Name.Replace(Constants.WishListNameBeginning, "");
            destination.ExternalId = origin.OrderGroupId.ToString("B");
            destination.ShopName = request.ShopName;
            destination.CustomerId = origin.SoldToId.ToString("B");
            this.MapWeaklyTypedProperties(origin, (Entity)destination);
        }

        /// <summary>
        /// Maps the weakly types properties from a Commerce Server basket to an CommerceConnect entity
        /// </summary>
        /// <param name="origin">the basket to copy from</param>
        /// <param name="destination">the entity to copy to</param>
        protected virtual void MapWeaklyTypedProperties(OrderGroup origin, Entity destination)
        {
            foreach (string key in origin)
                destination.Properties.Add(key, origin[key]);
        }
    }
}