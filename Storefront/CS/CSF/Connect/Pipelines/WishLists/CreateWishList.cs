using Sitecore.Commerce.Pipelines.WishLists.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Commerce.Pipelines;
using Sitecore.Diagnostics;
using Sitecore.Commerce.Services.WishLists;
using Sitecore.Commerce.Connect.CommerceServer.Orders.Pipelines;
using Sitecore.Commerce.Connect.CommerceServer.Orders;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Profiles.Pipelines;
using Sitecore.Pipelines;
using CommerceServer.Core.Runtime.Profiles;
using Sitecore.Commerce.Services;
using Sitecore.Globalization;
using Sitecore.Commerce.Entities.WishLists;
using CommerceServer.Core.Runtime.Orders;
using Sitecore.Commerce.Connect.CommerceServer.Pipelines;

namespace Sitecore.Reference.Storefront.Connect.Pipelines.WishLists
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateWishList : WishListPipelineProcessor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public override void Process(ServicePipelineArgs args)
        {
            try
            {
                Assert.ArgumentNotNull(args, "args");
                Assert.ArgumentNotNull(args.Request, "args.Request");
                Assert.ArgumentNotNull(args.Result, "args.Result");
                Assert.IsTrue(args.Request is CreateWishListRequest, "args.Request is CreateWishListRequest");
                Assert.IsTrue(args.Result is CreateWishListResult, "args.Result is CreateWishListResult");

                var createWishListRequest = (CreateWishListRequest)args.Request;
                var createWishListResult = (CreateWishListResult)args.Result;

                Assert.IsNotNullOrEmpty(createWishListRequest.UserId, "createWishListRequest.UserId");
                Assert.IsNotNullOrEmpty(createWishListRequest.ShopName, "createWishListRequest.ShopName");
                Assert.IsNotNullOrEmpty(createWishListRequest.WishListName, "createWishListRequest.WishListName");

                string userId = createWishListRequest.UserId;
                string shopName = createWishListRequest.ShopName;
                string wishListName = Constants.WishListNameBeginning + createWishListRequest.WishListName;

                Guid csUserId;
                if (!this.ResolveUserId(createWishListRequest.UserId, out csUserId))
                {
                    createWishListResult.Success = false;
                    SystemMessage systemMessage = new SystemMessage()
                    {
                        Message = Translate.Text("CreateWishList was unable to retrieve user \"{0}\".", (object)createWishListRequest.UserId)
                    };
                    createWishListResult.SystemMessages.Add(systemMessage);

                    return;
                }

                var instance = CommerceTypeLoader.CreateInstance<IOrderRepository>();
                var basket = instance.GetBasket(csUserId, wishListName);
                basket.Save();
                createWishListResult.WishList = this.TranslateBasketToWishList(userId, shopName, basket);
            }
            catch (Exception ex)
            {
                Log.Error("Unhandled Exception in CreateWishList.", ex, (object)this);
                args.Result.Success = false;
                throw;
            }
        }

        /// <summary>Resolves the user identifier.</summary>
        /// <param name="givenUserId">The given user identifier.</param>
        /// <param name="csUserId">The cs user identifier.</param>
        /// <returns>true if the userId has been resolved; Otherwise false.</returns>
        protected virtual bool ResolveUserId(string givenUserId, out Guid csUserId)
        {
            bool flag = Guid.TryParse(givenUserId, out csUserId);
            if (!flag)
            {
                GetProfileArgs getProfileArgs = new GetProfileArgs();
                getProfileArgs.InputParameters.Name = "UserObject";
                getProfileArgs.InputParameters.Id = givenUserId;
                getProfileArgs.InputParameters.Field = CommerceTypeLoader.ConfigurationProvider.CurrentConfiguration.Profiles.SitecoreLinkProperty;
                CorePipeline.Run("GetProfile", (PipelineArgs)getProfileArgs);
                Profile commerceProfile = getProfileArgs.OutputParameters.CommerceProfile;
                if (commerceProfile != null)
                {
                    csUserId = Guid.Parse(commerceProfile["GeneralInfo.user_id"].Value as string);
                    flag = true;
                }
            }
            return flag;
        }

        /// <summary>
        /// 
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