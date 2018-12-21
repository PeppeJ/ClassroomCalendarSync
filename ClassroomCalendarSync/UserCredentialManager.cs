using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

namespace ClassroomCalendarSync
{
    class UserCredentialManager
    {
        public UserCredential Credential { get; private set; }
        private string CredentialPath { get; set; }

        public UserCredentialManager(IEnumerable<string> scopes)
        {
            InitializeCredential(scopes);
        }

        private void InitializeCredential(IEnumerable<string> scopes)
        {
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                CredentialPath = "token.json";
                Credential =
                    GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(CredentialPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + CredentialPath);
            }
            ConsoleHelper.Info($"Initialized Credentials for {Credential.UserId}");
        }

        public void RefreshCredentials() => Credential.RefreshTokenAsync(CancellationToken.None);
    }
}
