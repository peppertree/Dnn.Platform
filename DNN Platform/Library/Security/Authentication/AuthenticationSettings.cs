﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetNuke.Collections;

namespace DotNetNuke.Security.Authentication
{
    public class AuthenticationSettings
    {
        #region "Constructors"
        public AuthenticationSettings(int portalId)
        {
            Settings = AuthenticationController.GetAuthenticationSettings(portalId);
            ReadSettings(portalId);
        }
        #endregion

        #region "Private Members"
        private Dictionary<string, string> Settings { get; set; }
        #endregion

        #region "Public Members"
        public CredentialMode CredentialMode { get; set; }
        public List<AuthenticationMethod> SupportedAuthenticationMethods { get; set; }
        #endregion

        #region "Public Methods"
        private void ReadSettings(int portalId)
        {
            CredentialMode = (CredentialMode)(Settings.GetValueOrDefault("AuthenticationSettings_CredentialMode", 1));
            //SupportedAuthenticationMethods = (AuthenticationMethod Settings.GetValueOrDefault("AuthenticationSettings_SupportedAuthenticationMethods", "DNN;").ToList();
        }

        public void UpdateSettings(int portalId)
        {
            AuthenticationController.UpdateAuthenticationSettings(this, portalId);
        }
        #endregion

    }
}
