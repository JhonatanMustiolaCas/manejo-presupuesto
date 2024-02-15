using ManejoPresupuesto.Validations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
    public class TipoCuenta
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 50, MinimumLength = 3, ErrorMessage = "Campo {0} debe tener entre {2} y {1} caracteres")]
        [Display(Name = "Nombre del tipo de cuenta")]
        [Capitalised]
        [Remote(action:"VerificarExisteTipoCuenta", controller:"TiposCuentas", AdditionalFields = nameof(Id))]
        public string Nombre { get; set; }

        public int UsuarioId { get; set; }

        public int Orden { get; set; }

		//public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		//{
		//	if (Nombre != null && Nombre.Length > 0)
  //          {
  //              var primeraLetra = Nombre[0].ToString();

  //              if (primeraLetra != primeraLetra.ToUpper())
  //              {
  //                  yield return new ValidationResult("La primera letra debe ser mayúscula", new[] { nameof(Nombre) } );
  //              }
  //          }
		//}

		/*Probando otras validaciones*/
		//[Required(ErrorMessage = "El campo {0} es requerido")]
  //      [EmailAddress(ErrorMessage = "El campo debe ser un correo electrónico válido")]
		//public string Email { get; set; }

  //      [Range(minimum:18, maximum:120, ErrorMessage = "El valor de {0} debe estar entre {1} y {2}")]
  //      public int Edad { get; set; }

  //      [Url(ErrorMessage = "El campo {0} debe ser una url válida")]
  //      public string URL { get; set; }

  //      [CreditCard(ErrorMessage = "La tarjeta de credito no es válida")]
  //      [Display(Name = "Tarjeta de crédito")]
  //      public string TarjetaDeCredito { get; set; }

	}
}
