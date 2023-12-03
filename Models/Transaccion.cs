using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Models
{
	public class Transaccion
	{
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de la Transacción")]
        public DateTime FechaTransaccion { get; set; } = DateTime.Today;
        public decimal Monto { get; set; }
        [Range(minimum:1, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una categoría")]
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }
        [StringLength(maximumLength: 1000, ErrorMessage = "El campo {0} no puede tener mas de {1} caracteres")]
        public string? Nota { get; set; }
		[Range(minimum: 1, maximum: int.MaxValue, ErrorMessage = "Debe seleccionar una cuenta")]
		[Display(Name = "Cuenta")]
        public int CuentaId { get; set; }
        [Display(Name = "Tipo de Operación")]
        public TipoOperacion TipoOperacionId { get; set; } = TipoOperacion.Ingreso;
        public string? Cuenta { get; set; }
        public string? Categoria { get; set; }
    }
}

//con DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd hh:MM tt")) -> para parsear formulario a hora personalizada -> o con ("g"): general
//con atributo DateTime
