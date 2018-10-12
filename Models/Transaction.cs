using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace bank_accounts.Models
{
    public class Transaction
    {
        [Key]
        public int Transactionid { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public DateTime Created_at { get; set; } = DateTime.Now;
        public int Userid { get; set; }
        public User Handler { get; set; }
    }
}