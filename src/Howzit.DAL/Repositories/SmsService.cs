using System.Threading.Tasks;
using Microsoft.AspNet.Identity;


namespace Howzit.DAL.Repositories
{
    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your sms service here _taskOwner send a text error.
            return Task.FromResult(0);
        }
    }
}
