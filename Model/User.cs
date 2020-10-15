using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Nebula.Identity.Model
{
    public class User:IdentityUser<int>
    {
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Dep { get; set; }
    }
}