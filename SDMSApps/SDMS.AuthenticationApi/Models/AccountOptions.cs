// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;

namespace SDMS.AuthenticationApi.Models
{
    public class AccountOptions
    {
        public static bool AllowLocalLogin = true;
        public static bool AllowRememberLogin = true;
        public static TimeSpan RememberMeLoginDuration = TimeSpan.FromDays(30);

        public static bool ShowLogoutPrompt =false;// true;
        public static bool AutomaticRedirectAfterSignOut = true; //false;
        public static string LockedOutAccount = "Account is locked out";
        public static string InvalidCredentialsErrorMessage = "Invalid username or password";
    }
}
