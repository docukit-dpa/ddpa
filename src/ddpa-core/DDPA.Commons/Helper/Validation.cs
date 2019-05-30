using System.Text.RegularExpressions;

namespace DDPA.Commons.Helper
{
    public static class Validation
    {
        /// <summary>
        /// Mininum of 8 characters and maximum of 255 characters.
        /// One digit, one lower case, and one upper case.
        /// </summary>
        private const string PASSWORD_PATTERN = @"^.*(?=.{8,255})(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).*$";

        public static bool IsValidPassword(string password)
        {
            bool isValid = false;
            if (!string.IsNullOrEmpty(password))
            {
                var match = Regex.Match(password, PASSWORD_PATTERN);
                if (match.Success)
                    isValid = true;
            }

            return isValid;
        }
    }
}