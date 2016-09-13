using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Howzit.Domains.Models
{
    public class SupplierContact : Contact
    {
        public SupplierContact() { }

        public SupplierContact(string firstName, string lastName, String middleName, string initial, String position, bool isPrimary, string phone, string email, Supplier supplier, ApplicationUser actionLogger)
            : base(firstName, lastName, middleName, initial, isPrimary, phone, email, actionLogger)
        {
            Supplier = supplier;
            Position = position;
        }

        [ForeignKey("Supplier")]
        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }
        public String Position { get; set; }

    }
}
