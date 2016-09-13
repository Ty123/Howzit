using System.Threading.Tasks;
using Microsoft.AspNet.Identity;


namespace Howzit.DAL.Repositories
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here _taskOwner send an email.
            return Task.FromResult(0);
        }
    }
}