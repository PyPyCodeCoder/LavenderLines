using System.ComponentModel.DataAnnotations;

namespace BlogInfrastructure.ViewModel
{
    public class ChangePasswordViewModel
    {
        [Required]
        [Display(Name = "Старий пароль")]
        public string OldPassword { get; set; }
        
        [Required]
        [Display(Name = "Новий пароль")]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        [Display(Name = "Підтвердження нового паролю")]
        [DataType(DataType.Password)]
        public string PasswordConfirm { get; set; }
    }
}