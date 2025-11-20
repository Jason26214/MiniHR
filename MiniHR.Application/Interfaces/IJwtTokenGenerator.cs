using MiniHR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHR.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        // Generates a JWT token for the given user
        string GenerateToken(User user);
    }
}
