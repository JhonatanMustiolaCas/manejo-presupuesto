using ManejoPresupuesto.Validations;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
	public class Categoria
	{
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength:50, ErrorMessage = "El campo {0} no puede tener mas de {1} caracteres")]
        [Capitalised]
        public string Nombre { get; set; }
        [Display(Name = "Tipo de Operación")]
        public TipoOperacion TipoOperacionId { get; set; }
        public int UsuarioId { get; set; }
    }
}
