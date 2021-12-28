using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Player
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(45)]
        public string Name { get; set; }
        [MaxLength(45)]
        public Guid Session { get; set; }
        public int ServerSession { get; set; }
        [Required]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdate { get; set; }

        public int UserId { get; set; }
        [Required]
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
    }
}
