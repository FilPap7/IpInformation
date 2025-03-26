using Microsoft.IdentityModel.Tokens;
using ApiModels.DTO;

namespace IpInformation.Helpers
{
    public class DatabaseValidation
    {
        public static bool ValidateCredentials(Credentials credentials)
        {
            if (credentials.UserName.IsNullOrEmpty() || credentials.Password.IsNullOrEmpty())
                throw new Exception("Username or Password cannot be empty");

            //Place the Logic here

            return false;
        }
    }
}
