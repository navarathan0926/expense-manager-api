using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseManager.Application.DTOs
{
    public class LoginRequestDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}