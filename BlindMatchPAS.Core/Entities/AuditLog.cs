using System;
using System.ComponentModel.DataAnnotations;

namespace BlindMatchPAS.Core.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        public string AdminEmail { get; set; } = string.Empty;

        [Required]
        public string Action { get; set; } = string.Empty; // e.g., "Promoted User", "Reassigned Match"

        [Required]
        public string Details { get; set; } = string.Empty; // e.g., "Promoted user@example.com to Admin"

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
