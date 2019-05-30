
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace DDPA.Web.TokenProvider
{
    public class ConfirmEmailDataProtectorTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public ConfirmEmailDataProtectorTokenProvider(IDataProtectionProvider dataProtectionProvider, IOptions<ConfirmEmailDataProtectionTokenProviderOptions> options) : base(dataProtectionProvider, options)
        {
        }
    }

    public class ConfirmEmailDataProtectionTokenProviderOptions : DataProtectionTokenProviderOptions { }
}