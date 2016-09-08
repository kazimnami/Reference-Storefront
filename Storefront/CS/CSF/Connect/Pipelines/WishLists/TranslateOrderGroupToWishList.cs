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
    public class TranslateOrderGroupToWishList : CommerceTranslateProcessor
    {
        /// <summary>Gets the entity factory.</summary>
        /// <value>The entity factory.</value>
        public IEntityFactory EntityFactory { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sitecore.Reference.Storefront.Connect.Pipelines.WishLists.TranslateOrderGroupToWishList" /> class.
        /// </summary>
        /// <param name="entityFactory">The entity factory.</param>
        public TranslateOrderGroupToWishList(IEntityFactory entityFactory)
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
            Assert.IsTrue(args.Request is TranslateOrderGroupToWishListRequest, "args.Request is TranslateOrderGroupToWishListRequest");
            Assert.IsTrue(args.Result is TranslateOrderGroupToWishListResult, "args.Result is TranslateOrderGroupToWishListResult");

            TranslateOrderGroupToWishListRequest request = (TranslateOrderGroupToWishListRequest)args.Request;
            TranslateOrderGroupToWishListResult result = (TranslateOrderGroupToWishListResult)args.Result;

            if (!OrderGroupUtility.ValidOrderGroupForWishList(request, result))
            {
                result.Success = false;
                return;
            }

            WishList destination = this.EntityFactory.Create<WishList>("WishList");
            this.TranslateToWishList(request, request.OrderGroup, destination);
            ((TranslateOrderGroupToWishListResult)args.Result).WishList = destination;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        protected virtual void TranslateToWishList(TranslateOrderGroupToWishListRequest request, OrderGroup origin, WishList destination)
        {
            this.TranslateCommerceConnectProperties(request, (WishList)destination);
            destination.Name = origin.Name.Replace(Constants.WishListNameBeginning, "");
            destination.ExternalId = origin.OrderGroupId.ToString("B");
            destination.CustomerId = origin.SoldToId.ToString("B");
            this.MapWeaklyTypedProperties(origin, (Entity)destination);
            destination.Lines = this.GetTranslatedOrderForms(origin, destination);
        }

        /// <summary>Translates the CommerceConnect properties.</summary>
        /// <param name="request">The request.</param>
        /// <param name="destination">The wish list.</param>
        protected virtual void TranslateCommerceConnectProperties(TranslateOrderGroupToWishListRequest request, WishList destination)
        {
            destination.UserId = request.UserId;
            destination.ShopName = request.ShopName;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderGroup"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        protected virtual ReadOnlyCollection<WishListLine> GetTranslatedOrderForms(OrderGroup orderGroup, WishList destination)
        {
            if (orderGroup.OrderForms == null || orderGroup.OrderForms.Count == 0)
            {
                return (new List<WishListLine>()).AsReadOnly();
            }

            var origin = orderGroup.OrderForms[0];
            return this.GetTranslateLineItems(origin);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        protected virtual ReadOnlyCollection<WishListLine> GetTranslateLineItems(OrderForm origin)
        {
            List<WishListLine> wishListLineList = new List<WishListLine>();
            foreach (LineItem lineItem in origin.LineItems)
            {
                WishListLine destination = this.EntityFactory.Create<WishListLine>("WishListLine");
                // destination.ExternalId = lineItem.?;
                destination.Quantity = (uint)lineItem.Quantity;
                this.MapWeaklyTypedProperties(lineItem, (Entity)destination);
                this.AppendCartProduct(lineItem, destination);

                wishListLineList.Add(destination);
            }
            return wishListLineList.AsReadOnly();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        protected virtual void MapWeaklyTypedProperties(LineItem origin, Entity destination)
        {
            foreach (string key in origin)
                destination.Properties.Add(key, origin[key]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        protected virtual void AppendCartProduct(LineItem origin, WishListLine destination)
        {
            CommerceCartProduct commerceCartProduct = this.EntityFactory.Create<CommerceCartProduct>("CartProduct");
            commerceCartProduct.Description = origin.Description;
            commerceCartProduct.DisplayName = origin.DisplayName;
            commerceCartProduct.ProductName = origin.DisplayName;
            commerceCartProduct.LineNumber = (uint)origin.Index;
            CommercePrice commercePrice = this.EntityFactory.Create<CommercePrice>("Price");
            commercePrice.Amount = origin.PlacedPrice;
            commercePrice.ListPrice = origin.ListPrice;
            commerceCartProduct.Price = commercePrice;
            commerceCartProduct.ProductCatalog = origin.ProductCatalog;
            commerceCartProduct.ProductCategory = origin.ProductCategory;
            commerceCartProduct.ProductId = origin.ProductId;
            commerceCartProduct.ProductVariantId = origin.ProductVariantId;
            destination.Product = (CartProduct)commerceCartProduct;
        }
    }
}