using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Howzit.Domains.Models
{
    public class CustomerContact : Contact
    {
        public CustomerContact() { }

        public CustomerContact(string firstName, string lastName, String middleName, string initial, String position, bool isPrimary, string phone, string email,Customer customer ,ApplicationUser actionLogger )
            : base(firstName, lastName, middleName, initial, isPrimary, phone, email, actionLogger)
        {
            Customer = customer;
            Position = position;
        }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
        public String Position { get; set; }

    }
}
