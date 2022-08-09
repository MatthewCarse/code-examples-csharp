using DocuSign.CodeExamples.Models;
using Microsoft.AspNetCore.Mvc;
using eSignature.Examples;
using DocuSign.eSign.Client;
using DocuSign.CodeExamples.eSignature.Models;

namespace DocuSign.CodeExamples.Controllers
{
    [Area("eSignature")]
    [Route("eg020")]
    public class RecipientAuthPhone : EgController
    {
        private CodeExampleText codeExampleText;
        public RecipientAuthPhone(DSConfiguration config, LauncherTexts launcherTexts, IRequestItemsService requestItemsService)
            : base(config, launcherTexts, requestItemsService)
        {
            codeExampleText = GetExampleText(EgNumber);
            ViewBag.title = codeExampleText.PageTitle;
        }

        public const int EgNumber = 20;

        public override string EgName => "eg020";

        [HttpPost]
        public IActionResult Create(string signerEmail, string signerName, string signerCountryCode, string signerPhoneNumber)
        {
            try
            {
                // Check the token with minimal buffer time.
                bool tokenOk = CheckToken(3);
                if (!tokenOk)
                {
                    // We could store the parameters of the requested operation so it could be 
                    // restarted automatically. But since it should be rare to have a token issue
                    // here, we'll make the user re-enter the form data after authentication.
                    RequestItemsService.EgName = EgName;
                    return Redirect("/ds/mustAuthenticate");
                }

                // Data for this method:
                // signerEmail 
                // signerName            
                var basePath = RequestItemsService.Session.BasePath + "/restapi";

                // Obtain your OAuth token
                var accessToken = RequestItemsService.User.AccessToken; // Represents your {ACCESS_TOKEN}
                var accountId = RequestItemsService.Session.AccountId; // Represents your {ACCOUNT_ID}

                // Call the Examples API method to create an envelope and 
                // add recipient that is to be authenticated with phone call
                string envelopeId = global::eSignature.Examples.RecipientAuthPhone.CreateEnvelopeWithRecipientUsingPhoneAuth(signerEmail, signerName,
                    accessToken, basePath, accountId, signerCountryCode, signerPhoneNumber, Config.docPdf);

                // Process results
                ViewBag.h1 = codeExampleText.ResultsPageHeader;
                ViewBag.message = codeExampleText.ResultsPageHeader + envelopeId + ".";
                return View("example_done");
            }
            catch (ApiException apiException)
            {
                if (apiException.Message.Contains("IDENTITY_WORKFLOW_INVALID_ID"))
                {
                    // This may indicate that this account is not yet enabled for the new phone auth workflow
                    ViewBag.SupportMessage = "Please contact <a target='_blank' href='https://support.docusign.com'>Support</a> to enable recipient phone authentication in your account.";
                }
                ViewBag.errorCode = apiException.ErrorCode;
                ViewBag.errorMessage = apiException.Message;

                return View("Error");
            }
        }
    }
}