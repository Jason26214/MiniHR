using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHR.Application.POCOs.DTOs
{
    public class RegisterDTO
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        // To test the "AdminOnly" policy later, roles are needed to be specify
        // In real-world projects, roles are usually assigned by the backend, not filled in by the user.
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
