# Overview

Self-expression is core to Teams culture. This app template is a message extension that enables your users to use custom stickers and GIFs within the Teams chat experience. This template provides an easy web-based configuration experience where IT admins can upload the GIFs/stickers/images they want their end-users to have.

# Legal notice

Please read the license terms applicable to this [here](https://github.com/OfficeDev/microsoft-teams-stickers-app/blob/master/LICENSE).  In addition to these terms, you agree to the following.  You are responsible for complying with all privacy and security regulations, as well as all internal privacy and security policies of your company.  You must also include your own privacy statement and terms of use for the app if you choose to deploy or share it broadly. Finally, you must ensure you have the IP rights necessary to show any image/GIF/stickers through this app to your users. Usage of this functionality is entirely your choice.  Use and management of any personal data collected is your responsibility.  Microsoft will not have any access to this data through this app.

# Deploying the application

## Prerequisites

To begin, you will need:
* A name for your Teams app. For example, "Contoso Stickers"
* Sticker images, which must be:
    * PNG, JPEG, or GIF format
    * Less than 1MB in total size
    * No more than 1000px in width or height. For best results, we recommend a maximum size of 300x300.
* An Azure subscription where you can create the following kinds of resources:
    * Azure Function
    * App service
    * App service plan
    * Bot channels registration
    * Storage account
    * Application Insights
* A copy of the Stickers app GitHub repo (https://github.com/officedev/microsoft-teams-stickers-app)

## Step 1: Register Azure AD applications

Register two Azure AD applications in your tenant’s directory: one for the messaging extension, and another for the configuration app.

1. Log in to the Azure Portal for your subscription, and go to the “App registrations (Preview)” blade at https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredAppsPreview.

2. Click on "New registration", and create an Azure AD application.
    1. **Name**: The name of your Teams app. For example, "Contoso Stickers".
    1. **Supported account types**: Select "Account in this organizational directory only"
    1. Leave the "Redirect URI" field blank.

![Azure AD app registration page](/docs/images/register_aad_application.png)

3. Click on the "Register" button.

4. When the app is registered, you'll be taken to the app's "Overview" page. Copy the following IDs; we will need them later.
    * Application (client) ID
    * Directory (tenant) ID

![Azure AD app overview page](/docs/images/aad_application_overview.png)

5. Go back to “App registrations (Preview)”, then repeat steps 2-4 to create another Azure AD application for the configuration app. We advise appending “Configuration” to the name of this app; for example, “Contoso Stickers Configuration”.

At this point you have 3 unique GUIDs:
* Two application (client) IDs: one for the messaging extension, and another for the configuration app
* Tenant ID, which is the same for the two apps that you registered

## Step 2: Deploy to your Azure subscription

1. Go to the [Stickers app on GitHub](https://github.com/officedev/microsoft-teams-stickers-app) and click on the "Deploy to Azure" button below.

[![Deploy to Azure](https://azuredeploy.net/deploybutton.png)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FOfficeDev%2Fmicrosoft-teams-stickers-app2%2Fmaster%2Fdeployment%2Fazuredeploy.json)

2. When prompted, log in to your Azure subscription.

3. Azure will create a "Custom deployment" based on the ARM template and ask you to fill in the template parameters.

![Custom deployment page](/docs/images/custom_deployment.png)

4. Select a subscription and resource group.
    * We recommend creating a new resource group.
    * The resource group location MUST be in a datacenter that supports Application Insights. For an up-to-date list, refer to https://azure.microsoft.com/en-us/global-infrastructure/services/?products=monitor, under "Log Analytics".

5. Enter a "Base Resource Name", which the template uses to generate names for the other resources.
    * The app service names `[Base Resource Name]` and `[Base Resource Name]-config` must be available. For example, if you select `contosostickers` as the base name, the names `contosostickers` and `contosostickers-config` must be free; otherwise, the deployment will fail with a Conflict error.
    * Remember the base resource name that you selected. We will need it later.

6. Fill in the various IDs in the template:
    1. **Messaging Extension App Id**: The application (client) ID of the messaging extension app
    1. **Config App Client Id**: The application (client) ID of the configuration app
    1. **Tenant Id**: The tenant ID

    Make sure that the GUIDs are copied as-is, with no extra spaces. The template checks that the IDs are exactly 36 characters.

7. Fill in the "Config Admin UPN List", which is a semicolon-delimited list of users who will be allowed to access the configuration app.
    * For example, to allow Megan Bowen (meganb@contoso.com) and Adele Vance (adelev@contoso.com) to access the configuration app, set this parameter to  `meganb@contoso.com;adelev@contoso.com`.
    * You can change this list later by going to the configuration app service's "Configuration" blade.

8. Agree to the Azure terms and conditions by clicking on the check box “I agree to the terms and conditions stated above” located at the bottom of the page.

9. Click on “Purchase” to start the deployment.

10. Wait for the deployment to finish. You can check the progress of the deployment from the "Notifications" pane of the Azure Portal.

## Step 3: Finish setting up the configuration app

1.	Note the location of the configuration app that you deployed, which is `https://[BaseResourceName]-config.azurewebites.net`. For example, if you chose “contosostickers” as the base name, the configuration app will be at `https://contosostickers-config.azurewebsites.net`.

2.	Go to the Application Registrations page at https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredAppsPreview. Select the configuration app that you created in Step 1.5.

3.	Under “Manage”, click on “Authentication” to bring up authentication settings.

4.	Add a new entry to “Redirect URIs”:
    1. **Type**: Web
    1. **Redirect URI**: Location (URL) of your configuration app. This is the URL from Step 3.1.

![Redirect URIs section of app registration page](/docs/images/configuration_auth_1.png)

5. Under "Implicit grant", check "ID tokens".

![Implicit grant section of app registration page](/docs/images/configuration_auth_2.png)

6. Click “Save” to commit your changes.

## Step 4: Add your custom stickers

Upload your custom stickers. To get you started, we have a set of sample stickers in the `deployment\stickers` folder.

1.	Go to the configuration app, which is at `https://[BaseResourceName]-config.azurewebsites.net`. For example, if you chose “contosostickers” as the base name, the configuration app will be at `https://contosostickers-config.azurewebsites.net`.

2.	When prompted, log in as one of the users that you specified in “Config Admin UPN List”.

3. For each sticker:
    1. Click on "Add new".
    ![Sticker configuration home page](/docs/images/stickers_configuration_home.png)
    2. Click on “Choose file” and select the image.
    3. Update name and keywords. Keywords should be separated by commas.
    4. Click on “Create” to save the image.
    ![Create sticker page](/docs/images/stickers_configuration_add.png)

4.	When you’re done adding stickers, click on “Update messaging extension” so that your changes are reflected in the messaging extension.

![Update messaging extension](/docs/images/stickers_configuration_publish.png)

## Step 5: Create the Teams app package

1. Open the `manifest\manifest.json` file in a text editor.

2.	Change the “botId” to your messaging extension’s application ID. This is the same GUID that you entered in the template under “Messaging Extension App Id”.

3. Change the fields in the manifest to values appropriate for your organization.
    * `packageName` (https://docs.microsoft.com/en-us/microsoftteams/platform/resources/schema/manifest-schema#packagename)
    * `developer.name` (https://docs.microsoft.com/en-us/microsoftteams/platform/resources/schema/manifest-schema#developer)
    * `developer.websiteUrl`
    * `developer.privacyUrl`
    * `developer.termsOfUseUrl`
    * `name.short` (https://docs.microsoft.com/en-us/microsoftteams/platform/resources/schema/manifest-schema#name)
    * `description.short` (https://docs.microsoft.com/en-us/microsoftteams/platform/resources/schema/manifest-schema#description)
    * `description.full`

4. Create a ZIP package with `manifest.json`, `color.png`, and `outline.png`. This is your Teams app package.
    * Make sure that the 3 files are the *top level* of the ZIP package, with no nested folders.
    ![Teams app package ZIP file](/docs/images/package_zip.png)

## Step 6: Run the app in Microsoft Teams

1.	If your tenant has sideloading apps enabled, you can install your app personally or to a team by following the instructions below.
    * Upload package to a team or personally using Store: https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/apps/apps-upload#upload-your-package-into-a-team-or-conversation-using-the-store
    * Upload package to a team using the Apps tab: https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/apps/apps-upload#upload-your-package-into-a-team-using-the-apps-tab

2.	You can also upload it to your tenant app catalog, so that it available for everyone in your tenant to install: https://docs.microsoft.com/en-us/microsoftteams/tenant-apps-catalog-teams

Have fun sharing custom stickers!

## Troubleshooting

Common issues when deploying or installing the app.

### 1. Location doesn't support Application Insights.
![Error when Application Insights is not supported](/docs/images/troubleshooting_appinsights_location.png)
```
The subscription is not registered for the resource type 'components' in the region '<region>'. Please re-register for this provider in order to have access to this location.
```
* The resources of type "microsoft.insights/components" failed with status "Conflict"

To resolve this issue, create the resource group in a location where Application Insights is available. For an up-to-date list of these locations, refer to https://azure.microsoft.com/en-us/global-infrastructure/services/?products=monitor, under "Log Analytics".

### 2. App service names are not available.


# Architecture

# Feedback

Thoughts? Questions? Ideas? Share them with us on [Teams UserVoice](https://microsoftteams.uservoice.com/forums/555103-public)!

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
