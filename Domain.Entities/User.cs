using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(30)]
        public string Username { get; set; }
        [Required]
        [EmailAddress]
        [MaxLength(60)]
        public string Email { get; set; }
        [Required]
        [MaxLength(100)]
        public string Password { get; set; }

        [NotMapped]
        [MaxLength(100)]
        public string NewPassword { get; set; }

        public bool IsEmailConfirmed { get; set; } = false;
        public bool IsBlocked { get; set; } = false;

        public Guid Code { get; set; }

        [Required]
        public double Balance { get; set; } = 0;

        [Required]
        public DateTime RegisterDate { get; set; } = DateTime.UtcNow;

        public List<Role> Roles { get; set; } = new();
        public List<Player> Players { get; set; } = new();
    }
}
