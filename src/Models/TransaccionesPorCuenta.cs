namespace ManejoPresupuesto.Models
{
	public class TransaccionesPorCuenta
	{
		public int UsuarioId { get; set; }
		public int CuentaId { get; set; }
		public DateTime FechaInicio { get; set; }
		public DateTime FechaFin { get; set; }
	}
}
