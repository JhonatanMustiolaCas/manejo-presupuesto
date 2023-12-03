namespace ManejoPresupuesto.Models
{
    public class TransaccionActualizarViewModel : TransaccionViewModel
    {
        public int CuentaAnteriorId { get; set; }
        public decimal MontoAnterior { get; set; }
        public string? URLRetorno { get; set; }
    }
}
