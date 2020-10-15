using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Nebula.Identity.Data;
using Nebula.Identity.Model;

namespace IdentityServer4.Quickstart.UI
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage="用户名不能为空")]
        public string Username { get; set; }
        [Required(ErrorMessage="密码不能为空")]
        public string Password { get; set; }
        [Required(ErrorMessage="确认密码不能为空")]
        public string ConfirmedPassword { get; set; }
        [Required(ErrorMessage="部门不能为空")]
        public int Department { get; set; }
        public string ReturnUrl { get; set; }
        public ICollection<Department> Departments { get; set; }
    }
}