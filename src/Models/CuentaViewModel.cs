using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Models
{
	public class CuentaViewModel: Cuenta
	{
        public IEnumerable<SelectListItem>? TiposCuentas { get; set; }
    }
}
