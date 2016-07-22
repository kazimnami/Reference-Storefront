namespace Sitecore.Reference.Storefront.Connect.Pipelines.Customers
{
    using Sitecore.Commerce.Data.Customers;
    using Sitecore.Commerce.Pipelines.Customers.UpdateUser;
    using Sitecore.Commerce.Services.Customers;
    using Sitecore.Diagnostics;
    using Sitecore.Security;
    using Sitecore.Security.Accounts;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public class UpdateUser : UpdateUserInSitecore
    {
        public UpdateUser(IUserRepository userRepository)
            : base(userRepository)
        {
        }

        public override void Process(Commerce.Pipelines.ServicePipelineArgs args)
        {
            var request = (UpdateUserRequest)args.Request;

            if (request.CommerceUser == null)
            {
                return;
            }
            
            // if we found a user, add some addition info
            var userProfile = GetUserProfile(request.CommerceUser.UserName);
            Assert.IsNotNull(userProfile, "profile");

            UpdateCustomer(request.CommerceUser, userProfile);

            base.Process(args);
        }

        protected void UpdateCustomer(Sitecore.Commerce.Entities.Customers.CommerceUser commerceUser, Sitecore.Security.UserProfile userProfile)
        {
            userProfile["skype_name"] = commerceUser.GetPropertyValue("skype_name").ToString();
            userProfile.Save();
        }

        protected UserProfile GetUserProfile(string userName)
        {
            return User.FromName(userName, true).Profile;
        }
    }
}