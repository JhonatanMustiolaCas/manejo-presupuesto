using ManejoPresupuesto.Models;
using static ManejoPresupuesto.Models.ReportesTransaccionesDetalladas;
using System.Reflection;

namespace ManejoPresupuesto.Services
{
    public interface IServicioReportes
    {
		Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerReporteSemanal(int usuarioId, int mes, int agno, dynamic ViewBag);
		Task<ReportesTransaccionesDetalladas> ObtenerReporteTransaccionesDetalladasPorCuenta(int usuarioId, int CuentaId, int mes, int agno, dynamic ViewBag);
        Task<ReportesTransaccionesDetalladas> ObtenerReporteTransaccionesDetalladasPorUsuario(int usuarioId, int mes, int agno, dynamic ViewBag);
    }
    public class ServicioReportes : IServicioReportes
    {
        private readonly IRepoTransacciones repoTransacciones;
        private readonly HttpContext httpContext;

        public ServicioReportes(
            IRepoTransacciones repoTransacciones,
            IHttpContextAccessor httpContextAccessor)
        {
            this.repoTransacciones = repoTransacciones;
            this.httpContext = httpContextAccessor.HttpContext;
        }

        public async Task<ReportesTransaccionesDetalladas>
            ObtenerReporteTransaccionesDetalladasPorUsuario(int usuarioId, int mes, int agno, dynamic ViewBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechasInicioYFin(mes, agno);
            var transaccionesPorUsuario = new TransaccionesPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var transacciones = await repoTransacciones.ObtenerTransaccionesPorUsuario(transaccionesPorUsuario);
            var modelo = GenerarReporteTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);
            AsignarValoresAlViewBag(ViewBag, fechaInicio);
            return modelo;
        }

        public async Task<ReportesTransaccionesDetalladas>
            ObtenerReporteTransaccionesDetalladasPorCuenta(int usuarioId, int CuentaId, int mes, int agno, dynamic ViewBag)
        {
            (DateTime fechaInicio, DateTime fechaFin) = GenerarFechasInicioYFin(mes, agno);

            var obtenerTransaccionesPorCuenta = new TransaccionesPorCuenta()
            {
                CuentaId = CuentaId,
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var transacciones = await repoTransacciones.ObtenerTransaccionesPorCuenta(obtenerTransaccionesPorCuenta);
            var modelo = GenerarReporteTransaccionesDetalladas(fechaInicio, fechaFin, transacciones);
            AsignarValoresAlViewBag(ViewBag, fechaInicio);

            return modelo;
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerReporteSemanal(int usuarioId, int mes, int agno, dynamic ViewBag)
        {
			(DateTime fechaInicio, DateTime fechaFin) = GenerarFechasInicioYFin(mes, agno);

			var transaccionesPorUsuario = new TransaccionesPorUsuario()
			{
				UsuarioId = usuarioId,
				FechaInicio = fechaInicio,
				FechaFin = fechaFin
			};

            AsignarValoresAlViewBag(ViewBag, fechaInicio);
            var modelo = await repoTransacciones.ObtenerTransaccionesPorSemana(transaccionesPorUsuario);
            return modelo;

		}

        private void AsignarValoresAlViewBag(dynamic ViewBag, DateTime fechaInicio)
        {
            ViewBag.mesAnterior = fechaInicio.AddMonths(-1).Month;
            ViewBag.agnoAnterior = fechaInicio.AddMonths(-1).Year;
            ViewBag.mesPosterior = fechaInicio.AddMonths(1).Month;
            ViewBag.agnoAnterior = fechaInicio.AddMonths(-1).Year;
            ViewBag.urlRetorno = httpContext.Request.Path + httpContext.Request.QueryString;
        }

        private static ReportesTransaccionesDetalladas GenerarReporteTransaccionesDetalladas(DateTime fechaInicio, DateTime fechaFin, IEnumerable<Transaccion> transacciones)
        {
            var modelo = new ReportesTransaccionesDetalladas();

            var transaccionesPorFecha = transacciones
                .OrderByDescending(x => x.FechaTransaccion)
                .GroupBy(x => x.FechaTransaccion)
                .Select(grupo => new ReportesTransaccionesDetalladas.TransaccionesPorFecha()
                {
                    FechaTransaccion = grupo.Key,
                    Transacciones = grupo.AsEnumerable()
                });

            modelo.TransaccionesAgrupadas = transaccionesPorFecha;
            modelo.FechaInicio = fechaInicio;
            modelo.FechaFin = fechaFin;
            return modelo;
        }

        private (DateTime fechaInicio, DateTime fechaFin) GenerarFechasInicioYFin(int mes, int agno)
        {
            DateTime fechaInicio, fechaFin;

            if (mes <= 0 || mes > 12 || agno <= 1900)
            {
                var hoy = DateTime.Today;
                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
            }
            else
            {
                fechaInicio = new DateTime(agno, mes, 1);
            }
            fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            return (fechaInicio, fechaFin);
        }
    }
}
