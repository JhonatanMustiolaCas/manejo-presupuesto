using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
	public class LoginViewModel
	{
		[Required(ErrorMessage = "El campo {0} es requerido")]
		[EmailAddress(ErrorMessage = "El campo {0} debe ser un correo electónico válido")]
		public string Email { get; set; }
		[Required(ErrorMessage = "El campo {0} es requerido")]
		[DataType(DataType.Password)]
		public string Password { get; set; }
        public bool Recuerdame { get; set; }
    }
}
