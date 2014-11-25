using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetNuke.Entities.Users;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using System.Web.Security;
using DotNetNuke.Entities.Host;

namespace DotNetNuke.Security.Authentication
{
    class AuthenticationController
    {

        #region "Public Methods"

        AuthenticationStatus Authenticate(UserInfo user, AuthenticationMethod method)
        {
            return Authenticate(user, method, Null.NullString);
        }

        AuthenticationStatus Authenticate(UserInfo user, AuthenticationMethod method, string VerificationCode)
        {
            AuthenticationStatus authenticationStatus = AuthenticationStatus.InvalidCredentials;

            int portalId = Null.NullInteger;
            bool userExists = false;

            if (user != null)
            {
                switch (method)
                {
                    case AuthenticationMethod.DNN:
                        userExists = (GetUserInternal(user) != null);
                        break;
                    case AuthenticationMethod.Facebook:
                        userExists = (GetUserInternal(user) != null);
                        break;
                    case AuthenticationMethod.Google:
                        userExists = (GetUserInternal(user) != null);
                        break;
                    case AuthenticationMethod.Twitter:
                        userExists = (GetUserInternal(user) != null);
                        break;
                    case AuthenticationMethod.Live:
                        userExists = (GetUserInternal(user) != null);
                        break;
                }

                if (userExists)
                {
                    MembershipUser aspnetUser = GetMembershipUser(user.Username);
                    FillUserMembership(aspnetUser, user);

                    //Check if the User is Locked Out (and unlock if AutoUnlock has expired)
                    if (aspnetUser.IsLockedOut)
                    {
                        if (AutoUnlockUser(aspnetUser))
                        {
                            //Unlock User
                            user.Membership.LockedOut = false;
                        }
                        else
                        {
                            authenticationStatus = AuthenticationStatus.LockedOut;
                        }
                    }

                    //Check in a verified situation whether the user is Approved
                    if (IsApproved(user) == false)
                    {
                        //Check Verification code (only with DNN authentication)
                        if (method == AuthenticationMethod.DNN && (!string.IsNullOrEmpty(VerificationCode)))
                        {
                            var ps = new PortalSecurity();
                            if (VerificationCode == ps.EncryptString(portalId + "-" + user.UserID, Config.GetDecryptionkey()))
                            {
                                UserController.ApproveUser(user);
                            }
                            else
                            {
                                authenticationStatus = AuthenticationStatus.NotApproved;
                            }
                        }
                        else
                        {
                            user.Membership.Approved = true;
                            UserController.UpdateUser(portalId, user);
                            UserController.ApproveUser(user);
                        }
                    }

                    //now verify actual user credentials
                    bool isValid = false;
                    if (authenticationStatus != AuthenticationStatus.NotApproved && authenticationStatus != AuthenticationStatus.LockedOut)
                    {
                        isValid = System.Web.Security.Membership.ValidateUser(user.Username, user.Membership.Password);
                    }

                    if (isValid)
                    {
                        authenticationStatus = AuthenticationStatus.Authenticated;
                    }
                    else
                    {
                        authenticationStatus = AuthenticationStatus.InvalidCredentials;
                        user = null;
                    }

                }
            }

            return AuthenticationStatus.InvalidCredentials;
        }

        #endregion

        #region "Public Members"

        public static Dictionary<string, string> GetAuthenticationSettings(int portalID)
        {
            return PortalController.GetPortalSettingsDictionary(portalID);
        }

        public static void UpdateAuthenticationSettings(AuthenticationSettings settings, int portalId)
        {
            PortalController.UpdatePortalSetting(portalId, "AuthenticationSettings_SupportedAuthenticationMethods", settings.SupportedAuthenticationMethods.ToString());
            PortalController.UpdatePortalSetting(portalId, "AuthenticationSettings_CredentialMode", settings.CredentialMode.ToString());
        }

        #endregion

        #region "Private Methods"

        private static bool IsApproved(UserInfo user)
        {

            //host accounts are always approved
            if (user.IsSuperUser)
            {
                return true;
            }

            //either membership.approved might be set to false or the user is in the unverified role
            string unapprovedRole = "Unverified Users";
            if (user.Membership.Approved == false || user.IsInRole(unapprovedRole))
            {
                return false;
            }

            // must be approved
            return true;

        }

        private UserInfo GetUserInternal(UserInfo user)
        {
            UserInfo validatedUser = null;
            int portalId = user.PortalID;
            AuthenticationSettings settings = new AuthenticationSettings(portalId);
            
            switch(settings.CredentialMode)
            {
                case CredentialMode.Email:
                    validatedUser = UserController.GetUserByEmail(portalId, user.Email);
                    break;
                case CredentialMode.Username:
                    validatedUser = UserController.GetUserByName(portalId, user.Username);
                    break;
                case CredentialMode.UserId:
                    validatedUser = UserController.GetUserById(portalId, user.UserID);
                    break;
            }

            return validatedUser;

        }

        private static MembershipUser GetMembershipUser(string userName)
        {
            return
                CBO.GetCachedObject<MembershipUser>(
                    new CacheItemArgs(GetCacheKey(userName), DataCache.UserCacheTimeOut, DataCache.UserCachePriority,
                                      userName), GetMembershipUserCallBack);
        }

        private static object GetMembershipUserCallBack(CacheItemArgs cacheItemArgs)
        {
            string userName = cacheItemArgs.ParamList[0].ToString();

            return System.Web.Security.Membership.GetUser(userName);
        }

        private static string GetCacheKey(string userName)
        {
            return String.Format("MembershipUser_{0}", userName);
        }

        private static void FillUserMembership(MembershipUser aspNetUser, UserInfo user)
        {
            //Fill Membership Property
            if (aspNetUser != null)
            {
                if (user.Membership == null)
                {
                    user.Membership = new UserMembership(user);
                }
                user.Membership.CreatedDate = aspNetUser.CreationDate;
                user.Membership.LastActivityDate = aspNetUser.LastActivityDate;
                user.Membership.LastLockoutDate = aspNetUser.LastLockoutDate;
                user.Membership.LastLoginDate = aspNetUser.LastLoginDate;
                user.Membership.LastPasswordChangeDate = aspNetUser.LastPasswordChangedDate;
                user.Membership.LockedOut = aspNetUser.IsLockedOut;
                user.Membership.PasswordQuestion = aspNetUser.PasswordQuestion;
                user.Membership.IsDeleted = user.IsDeleted;

                if (user.IsSuperUser)
                {
                    //For superusers the Approved info is stored in aspnet membership
                    user.Membership.Approved = aspNetUser.IsApproved;
                }
            }
        }

        private static bool AutoUnlockUser(MembershipUser aspNetUser)
        {
            if (Host.AutoAccountUnlockDuration != 0)
            {
                if (aspNetUser.LastLockoutDate < DateTime.Now.AddMinutes(-1 * Host.AutoAccountUnlockDuration))
                {
                    //Unlock user in Data Store
                    if (aspNetUser.UnlockUser())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool ValidateUser(string username, string password)
        {
            return System.Web.Security.Membership.ValidateUser(username, password);
        }

        #endregion

    }
}
