using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Howzit.Domains.Models
{
    public class Customer
    {
        public Customer()
        {
            this.CustomerContacts = new HashSet<CustomerContact>();
            this.Projects = new HashSet<Project>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="company"></param>
        /// <param name="website"></param>
        public Customer(string name, string company, string website, ApplicationUser actionLogger) : this()
        {
            Name = name;
            CompanyName = company;
            Website = website;
            Created = DateTime.Now;
            IsActive = true;
            CreateBy = actionLogger.Id;
        }

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public String Website { get; set; }
        public DateTime Created { get; set; }
        public Nullable<DateTime> Updated { get; set; }
        public int CreateBy { get; set; }
        public int? UpateBy { get; set; }
        public bool IsActive { get; set; }
        [Timestamp]
        public byte[] Version { get; set; }

        public virtual ICollection<CustomerContact> CustomerContacts { get; set; }
        public virtual ICollection<Project> Projects { get; set; }
    }
}
