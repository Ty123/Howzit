﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Howzit.API.Models
{
    public class MaterialViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }
        
        [Required]
        public decimal Quantity { get; set; }

        [Required]
        public int SupplierId { get; set; }
    }
}