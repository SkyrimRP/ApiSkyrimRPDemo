using Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Server
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        public Guid Key { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(25)]
        public string Address { get; set; }

        [Required]
        public ServerLanguages Language { get; set; }

        [Required]
        public ServerType Type { get; set; }

        [Required]
        public ServerFlags Flags { get; set; }
    }
}
