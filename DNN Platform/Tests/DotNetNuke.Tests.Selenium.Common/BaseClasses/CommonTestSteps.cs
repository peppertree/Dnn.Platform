﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.BaseClasses.BasePages;
using DNNSelenium.Common.CorePages;
using DNNSelenium.Common.Properties;
using NUnit.Framework;
using OpenQA.Selenium;
using InstallerPage = DNNSelenium.Common.CorePages.InstallerPage;

namespace DNNSelenium.Common.BaseClasses
{
	public class CommonTestSteps : TestBase
	{
		protected IWebDriver _driver = null;
		protected string _baseUrl = null;

		public const string DNNSeleniumPrefix = "DNNSelenium.";
		public string _logContent;

		public BasePage OpenPage(string assyName, string pageClassName, string openMethod)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Navigation To " + pageClassName + "'");

			string fullAssyName = DNNSeleniumPrefix + assyName;
			Type pageClassType = Type.GetType(fullAssyName + "." + pageClassName + ", " + fullAssyName);
			object navToPage = Activator.CreateInstance(pageClassType, new object[] { _driver });

			MethodInfo miOpen = pageClassType.GetMethod(openMethod);
			if (miOpen != null)
			{
				miOpen.Invoke(navToPage, new object[] { _baseUrl });
			}
			else
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "ERROR: cannot call " + openMethod + "for class " + pageClassName);
			}

			return (BasePage)navToPage;
		}

		public void OpenMainPageAndLoginAsHost()
		{
			var mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			LoginPage loginPage = new LoginPage(_driver);
			loginPage.LoginAsHost(_baseUrl);
		}

		public void CreateAdminAndLoginAsAdmin(string userName, string displayName, string email, string password)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Create Admin User and Login as Admin user'");

			ManageUsersPage manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingControlPanel(_baseUrl);
			manageUsersPage.AddNewUser(userName, displayName, email, password);
			manageUsersPage.ManageRoles(userName);
			manageUsersPage.AssignRoleToUser("Administrators");

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			LoginPage loginPage = new LoginPage(_driver);
			loginPage.LoginUsingLoginLink(userName, password);
		}

		public string LogContent()
		{
			HostSettingsPage hostSettingsPage = new HostSettingsPage(_driver);
			hostSettingsPage.SetDictionary("en");
			hostSettingsPage.OpenUsingUrl(_baseUrl);

			hostSettingsPage.WaitAndClick(By.XPath(HostSettingsPage.LogsTab));
			SlidingSelect.SelectByValue(_driver, By.XPath("//a[contains(@id, '" + HostSettingsPage.LogFilesDropDownArrow + "')]"),
													By.XPath(HostSettingsPage.LogFilesDropDownList),
													HostSettingsPage.OptionOne,
													SlidingSelect.SelectByValueType.Contains);
			hostSettingsPage.WaitForElement(By.XPath(HostSettingsPage.LogContent), 30);

			return hostSettingsPage.FindElement(By.XPath(HostSettingsPage.LogContent)).Text.ToLower();
		}

		public void VerifyLogs(string logContentBeforeTests)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Verify Logs on Host Settings page: '");

			var loginPage = new LoginPage(_driver);
			loginPage.LoginAsHost(_baseUrl);

			string logContentAfterTests = LogContent();
			Utilities.SoftAssert(() => StringAssert.AreEqualIgnoringCase(logContentAfterTests, logContentBeforeTests, "ERROR in the Log"));
		}

		public void DisablePopups(string url)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Disable Popups: '");

			var adminSiteSettingsPage = new AdminSiteSettingsPage(_driver);
			adminSiteSettingsPage.OpenUsingButtons(url);
			adminSiteSettingsPage.DisablePopups();
		}

		public void CreateChildSiteAndPrepareSettings(string childSiteName, string childSiteTitle)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Create Child Site And Prepare Settings: '");

			HostSiteManagementPage hostSiteMgmtPage = new HostSiteManagementPage(_driver);
			hostSiteMgmtPage.OpenUsingButtons(_baseUrl);
			hostSiteMgmtPage.AddNewChildSite(_baseUrl, childSiteName, childSiteTitle);

			DisablePopups(_baseUrl + "/" + childSiteName);
		}

		public void CreateParentSiteAndPrepareSettings(string parentSiteName, string parentSiteTitle)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Create Parent Site And Prepare Settings: '");

			HostSiteManagementPage hostSiteMgmtPage = new HostSiteManagementPage(_driver);
			hostSiteMgmtPage.OpenUsingButtons(_baseUrl);
			hostSiteMgmtPage.AddNewParentSite(parentSiteName, parentSiteTitle);

			LoginPage loginPage = new LoginPage(_driver);
			loginPage.OpenUsingUrl(parentSiteName);
			loginPage.DoLogin("host", "dnnhost");
			
			DisablePopups(parentSiteName);
		}

		public void CreatePageAndSetViewPermission(string pageName, string option, string permissionOption)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Create A Page And Set View Permissions: '");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenAddNewPageFrameUsingControlPanel(_baseUrl);

			blankPage.AddNewPage(pageName);

			//blankPage.OpenUsingUrl(_baseUrl, pageName);
			blankPage.SetPageViewPermissions(option, permissionOption);

			//blankPage.CloseEditMode();
		}

		public void AddModule(string pageName, Dictionary<string, Modules.ModuleIDs> modulesDescription, string moduleName, string pane)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Add a new Module to Page: '");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);
			
			var module = new Modules(_driver);
			string moduleNameOnPage = modulesDescription[moduleName].IdWhenOnPage;
			string moduleNameOnBanner = modulesDescription[moduleName].IdWhenOnBanner;

			module.OpenModulePanelUsingControlPanel();
			module.AddNewModuleUsingMenu(moduleNameOnBanner, moduleNameOnPage, pane);

			blankPage.CloseEditMode();
		}
	}
}
