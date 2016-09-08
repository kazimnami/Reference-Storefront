using Sitecore.Commerce.Pipelines.WishLists.Common;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Commerce.Pipelines;
using Sitecore.Commerce.Services.WishLists;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Pipelines;
using Sitecore.Commerce.Services;
using Sitecore.Commerce.Entities.WishLists;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Models;
using Sitecore.Commerce.Connect.CommerceServer.Connect.Utility;
using CommerceServer.Core.Runtime.Orders;
using System.Collections.ObjectModel;
using Sitecore.Commerce;
using Sitecore.Commerce.Connect.CommerceServer.Orders;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Reference.Storefront.Connect.Pipelines.Utilities;
using Sitecore.Globalization;
using Sitecore.Commerce.Connect.CommerceServer.Pipelines;

namespace Sitecore.Reference.Storefront.Connect.Pipelines.WishLists
{
    /// <summary>
    /// 
    /// </summary>
    public class AddLinesToWishList : WishListPipelineProcessor
    {
        /// <summary>Processes the specified arguments.</summary>
        /// <param name="args">The arguments.</param>
        public override void Process(ServicePipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.ArgumentNotNull(args.Request, "args.Request");
            Assert.ArgumentNotNull(args.Result, "args.Result");
            Assert.IsTrue(args.Request is AddLinesToWishListRequest, "args.Request is AddLinesToWishListRequest");
            Assert.IsTrue(args.Result is AddLinesToWishListResult, "args.Result is AddLinesToWishListResult");

            var request = (AddLinesToWishListRequest)args.Request;
            var result = (AddLinesToWishListResult)args.Result;

            Assert.ArgumentNotNull(request.Lines, "request.Lines");
            Assert.ArgumentNotNull(request.WishList, "request.WishList");
            Assert.IsNotNullOrEmpty(request.WishList.ExternalId, "request.WishList.ExternalId");

            var userId = request.WishList.UserId;
            var wishListId = Guid.Parse(request.WishList.ExternalId);

            Guid csUserId;
            if (!UserUtility.ResolveUserId(userId, out csUserId))
            {
                result.Success = false;
                var systemMessage = new SystemMessage()
                {
                    Message = Translate.Text("GetWishList was unable to retrieve user \"{0}\".", (object)userId)
                };
                result.SystemMessages.Add(systemMessage);

                return;
            }

            var instance = CommerceTypeLoader.CreateInstance<IOrderRepository>();
            var basket = instance.GetBasket(csUserId, wishListId);

            foreach (WishListLine line in request.Lines)
            {
                if (line != null)
                {
                    var commerceCartProduct = (CommerceCartProduct)line.Product;
                    var lineItem = OrderUtility.CreateLineItem(commerceCartProduct.ProductCatalog, line.Product.ProductId, commerceCartProduct.ProductVariantId, (Decimal)line.Quantity);
                    this.AppendedLineItemProperties(line, lineItem);

                    if (basket.OrderForms == null || basket.OrderForms.Count == 0)
                    {
                        basket.OrderForms.Add(OrderUtility.CreateOrderForm());
                    }

                    basket.OrderForms[0].LineItems.Add(lineItem, true);
                }
            }

            basket.Save();

            result.AddedLines = request.Lines.ToList().AsReadOnly();
            result.WishList = this.TranslateBasketToWishList(userId,basket);
        }

        /// <summary>
        /// Add any appended custom properties to the <see cref="T:CommerceServer.Core.Runtime.Orders.LineItem" />
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cartLine">The cart line.</param>
        /// <param name="lineItem">The line item.</param>
        protected virtual void AppendedLineItemProperties(WishListLine cartLine, LineItem lineItem)
        {
            foreach (PropertyItem property in (Collection<PropertyItem>)cartLine.Properties)
                lineItem[property.Key] = property.Value;
        }

        /// <summary>
        /// Translates the commerce server basekt to the wish list entity 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="shopName"></param>
        /// <param name="basket"></param>
        /// <returns></returns>
        protected virtual WishList TranslateBasketToWishList(string userId, Basket basket)
        {
            var request = new TranslateOrderGroupToWishListRequest(userId, "", basket);
            var result = PipelineUtility.RunCommerceConnectPipeline<TranslateOrderGroupToWishListRequest, TranslateOrderGroupToWishListResult>(Constants.TranslateOrderGroupToWishList, request);
            return result.WishList;
        }
    }
}