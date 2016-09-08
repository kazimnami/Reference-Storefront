using CommerceServer.Core.Runtime.Profiles;
using Sitecore.Commerce.Connect.CommerceServer;
using Sitecore.Commerce.Connect.CommerceServer.Profiles.Pipelines;
using Sitecore.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Reference.Storefront.Connect.Pipelines.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public static class UserUtility
    {
        /// <summary>Resolves the user identifier.</summary>
        /// <param name="givenUserId">The given user identifier.</param>
        /// <param name="csUserId">The cs user identifier.</param>
        /// <returns>true if the userId has been resolved; Otherwise false.</returns>
        public static bool ResolveUserId(string givenUserId, out Guid csUserId)
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
    }
}