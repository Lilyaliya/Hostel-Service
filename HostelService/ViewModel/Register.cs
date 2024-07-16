using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace HostelService.ViewModel
{
    public class Register
    {
        [Required(ErrorMessage ="Заполните Имя!")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage ="Заполните Фамилию!")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        //Required attribute implements validation on Model item that this fields is mandatory for user
        [Required(ErrorMessage ="Укажите почту для входа!")]
        //We are also implementing Regular expression to check if email is valid like a1@test.com
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Некорректно задан почтовый адрес!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поставьте пароль!")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}