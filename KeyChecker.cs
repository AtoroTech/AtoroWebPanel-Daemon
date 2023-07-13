using System;


namespace McControllerX {
    public class KeyChecker
    {
        public static bool isStrongKey(string password, int minimumLength = 8)
        {
            bool hasUppercase = false;
            bool hasLowercase = false;
            bool hasDigit = false;
            bool hasSpecialCharacter = false;

            if (password.Length < minimumLength)
                return false;

            foreach (char c in password)
            {
                if (char.IsUpper(c))
                    hasUppercase = true;
                else if (char.IsLower(c))
                    hasLowercase = true;
                else if (char.IsDigit(c))
                    hasDigit = true;
                else if (!char.IsLetterOrDigit(c))
                    hasSpecialCharacter = true;
            }

            return hasUppercase && hasLowercase && hasDigit && hasSpecialCharacter;
        }

        public static string GenerateStrongKey(int length = 12)
        {
            string uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
            string digitChars = "0123456789";
            string specialChars = "!@#$%^&*()-=_+[]{}|;:,.<>/?";

            string validChars = uppercaseChars + lowercaseChars + digitChars + specialChars;

            Random random = new Random();
            string password = new string(Enumerable.Repeat(validChars, length)
                                      .Select(s => s[random.Next(s.Length)]).ToArray());

            return password;
        }
    }
}