using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Specifications
{
  public class UserByEmailSpec : BaseSpecifications<User, int>
  {
            public UserByEmailSpec(string email) : base(u => u.Email == email) { }
  }
}
