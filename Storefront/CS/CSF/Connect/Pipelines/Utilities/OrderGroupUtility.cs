using CommerceServer.Core.Runtime.Orders;
using Sitecore.Commerce.Connect.CommerceServer.Pipelines;
using Sitecore.Commerce.Services;
using Sitecore.Globalization;
using Sitecore.Reference.Storefront.Connect.Pipelines.WishLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Reference.Storefront.Connect.Pipelines.Utilities
{
    public static class OrderGroupUtility
    {

        /// <summary>
        /// Executes vaidation of the wish list order group 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool ValidOrderGroupForWishList(TranslateOrderGroupRequest request, CommerceResult result)
        {
            SystemMessage systemMessage = null;

            if (!(request.OrderGroup is Basket))
            {
                systemMessage = new SystemMessage()
                {
                    Message = Translate.Text("TranslateOrderGroupToEntity recieved an invalid OrderGroup type \"{0}\".", (object)request.OrderGroup.GetType().Name)
                };
            }
            else if (request.OrderGroup.OrderForms != null && request.OrderGroup.OrderForms.Count > 1)
            {
                systemMessage = new SystemMessage()
                {
                    Message = Translate.Text("TranslateOrderGroupToEntity recieved an invalid OrderGroup with more than one OrderForm \"{0}\".", request.OrderGroup.OrderForms.Count)
                };
            }
            else
            {
                return true;
            }

            result.SystemMessages.Add(systemMessage);
            return false;
        }
    }
}