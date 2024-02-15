using AutoMapper;
using ClosedXML.Excel;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Abstractions;
using System.Data;
using System.Reflection;

namespace ManejoPresupuesto.Controllers
{
	public class TransaccionesController : Controller
	{
		private readonly IServicioUsuario servicioUsuario;
		private readonly IRepoCuenta repoCuenta;
		private readonly IRepoTransacciones repoTransacciones;
		private readonly IRepoCategorias repoCategorias;
        private readonly IServicioReportes servicioReportes;
        private readonly IMapper mapper;

        public TransaccionesController(
            IServicioUsuario servicioUsuario,
            IRepoCuenta repoCuenta,
            IRepoTransacciones repoTransacciones,
			IRepoCategorias repoCategorias,
			IServicioReportes servicioReportes,
			IMapper mapper)
        {
			this.servicioUsuario = servicioUsuario;
			this.repoCuenta = repoCuenta;
			this.repoTransacciones = repoTransacciones;
			this.repoCategorias = repoCategorias;
            this.servicioReportes = servicioReportes;
            this.mapper = mapper;
        }

		[HttpGet]
		public async Task<IActionResult> Index(int mes, int agno)
		{
			var usuarioId = servicioUsuario.ObtenerUsuarioId();

			var modelo = await servicioReportes.ObtenerReporteTransaccionesDetalladasPorUsuario(usuarioId, mes, agno, ViewBag);
			return View(modelo);
		}

		public async Task<IActionResult> Semanal(int mes, int agno)
		{
			var usuarioId = servicioUsuario.ObtenerUsuarioId();
			IEnumerable<ResultadoObtenerPorSemana>
				transaccionesPorSemana = await  servicioReportes.ObtenerReporteSemanal(usuarioId, mes, agno, ViewBag);

			var agrupado = transaccionesPorSemana.GroupBy(x => x.Semana).Select(x => new ResultadoObtenerPorSemana()
			{
				Semana = x.Key,
				Ingresos = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
				Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto).Select(x => x.Monto).FirstOrDefault()
			}).ToList();

			if (agno == 0 || mes == 0)
			{
				var hoy = DateTime.Today;
				agno = hoy.Year;
				mes = hoy.Month;
			}

			var fechaReferencia = new DateTime(agno, mes, 1);
			var diasDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);
			var diasSegmentados = diasDelMes.Chunk(7).ToList();

			for (int i = 0; i < diasSegmentados.Count(); i++)
			{
				var semana = i + 1;
				var fechaInicio = new DateTime(agno, mes, diasSegmentados[i].First());
				var fechaFin = new DateTime(agno, mes, diasSegmentados[i].Last());
				var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);

				if (grupoSemana is null)
				{
					agrupado.Add(new ResultadoObtenerPorSemana()
					{
						Semana = semana,
						FechaInicio = fechaInicio,
						FechaFin = fechaFin
					});
				}
				else
				{
					grupoSemana.FechaInicio = fechaInicio;
					grupoSemana.FechaFin = fechaFin;
				}
			}

			agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();

			var modelo = new ReporteSemanalViewModel();
			modelo.TransaccionesPorSemana = agrupado;
			modelo.FechaReferencia = fechaReferencia;

			return View(modelo);
		}

        public async Task<IActionResult> Mensual(int agno)
        {

			var usuarioId = servicioUsuario.ObtenerUsuarioId();

			if (agno == 0)
			{
				agno = DateTime.Today.Year;
			}

			var transaccionesPorMes = await repoTransacciones.ObtenerTransaccionesPorMes(usuarioId, agno);

			var transaccionesAgrupadas = transaccionesPorMes.GroupBy(x => x.Mes)
				.Select(x => new ResultadoObtenerPorMes()
				{
					Mes = x.Key,
					Ingreso = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
					Gasto = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto).Select(x => x.Monto).FirstOrDefault(),

				}).ToList();

			for (int mes = 1; mes <= 12; mes++)
			{
				var transaccion = transaccionesAgrupadas.FirstOrDefault(x => x.Mes == mes);
				var fechaReferencia = new DateTime(agno, mes, 1);
				if (transaccion is null)
				{
					transaccionesAgrupadas.Add(new ResultadoObtenerPorMes()
					{
						Mes = mes,
						FechaReferencia = fechaReferencia
					});
				}
				else
				{
					transaccion.FechaReferencia = fechaReferencia;
				}
			}

			transaccionesAgrupadas = transaccionesAgrupadas.OrderByDescending(x => x.Mes).ToList();

			var modelo = new ReporteMensualViewModel();
			modelo.Agno = agno;
			modelo.TransaccionesPorMes = transaccionesAgrupadas;

            return View(modelo);
        }

        public IActionResult ReporteExcel()
        {
            return View();
        }

		[HttpGet]
		public async Task<FileResult> ExportarExcelPorMes(int mes, int agno)
		{
			var fechaInicio = new DateTime(agno, mes, 1);
			var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
			var usuarioId = servicioUsuario.ObtenerUsuarioId();

			var transacciones = await repoTransacciones.ObtenerTransaccionesPorUsuario(
				new TransaccionesPorUsuario()
				{
					UsuarioId = usuarioId,
					FechaInicio = fechaInicio,
					FechaFin = fechaFin
				});

			var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("MMM-yyyy")}.xlsx";

			return GenerarExcel(nombreArchivo, transacciones);
		}

		[HttpGet]
		public async Task<FileResult> ExportarExcelPorAgno(int agno)
		{
			var fechaInicio = new DateTime(agno, 1, 1);
			var fechaFin = fechaInicio.AddYears(1).AddDays(-1);

			var usuarioId = servicioUsuario.ObtenerUsuarioId();

			var transacciones = await repoTransacciones.ObtenerTransaccionesPorUsuario(
				new TransaccionesPorUsuario()
				{
					UsuarioId = usuarioId,
					FechaInicio = fechaInicio,
					FechaFin = fechaFin
				});

			var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("yyyy")}.xlsx";

			return GenerarExcel(nombreArchivo, transacciones);
		}

		[HttpGet]
		public async Task<FileResult> ExportarExcelTodo()
		{
			var fechaInicio = DateTime.Today.AddYears(-100);
			var fechaFin = DateTime.Today.AddYears(1000);
			var usuarioId = servicioUsuario.ObtenerUsuarioId();

			var transacciones = await repoTransacciones.ObtenerTransaccionesPorUsuario(
				new TransaccionesPorUsuario()
				{
					UsuarioId = usuarioId,
					FechaInicio = fechaInicio,
					FechaFin = fechaFin
				});

			var nombreArchivo = $"Manejo Presupuesto - {DateTime.Today.ToString("dd-MM-yyyy")}.xlsx";

			return GenerarExcel(nombreArchivo, transacciones);
		}

		private FileResult GenerarExcel(string nombreArchivo, IEnumerable<Transaccion> transacciones)
		{
			DataTable dt = new DataTable("Transacciones");
			dt.Columns.AddRange(new DataColumn[]
			{
				new DataColumn("Fecha"),
				new DataColumn("Cuenta"),
				new DataColumn("Categoria"),
				new DataColumn("Nota"),
				new DataColumn("Monto"),
				new DataColumn("Ingreso/Gasto"),

			});

			foreach (var transaccion in transacciones)
			{
				dt.Rows.Add
					(
						transaccion.FechaTransaccion,
						transaccion.Cuenta,
						transaccion.Categoria,
						transaccion.Nota,
						transaccion.Monto,
						transaccion.TipoOperacionId
					);
			}

			using (XLWorkbook wb = new XLWorkbook())
			{
				wb.Worksheets.Add(dt);

				using (MemoryStream stream = new MemoryStream())
				{
					wb.SaveAs(stream);
					return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
				}
			}
		}

        public IActionResult Calendario()
        {
            return View();
        }

		public async Task<JsonResult> ObtenerTransaccionesCalendario(DateTime start, DateTime end)
		{
			var usuarioId = servicioUsuario.ObtenerUsuarioId();

			var transacciones = await repoTransacciones.ObtenerTransaccionesPorUsuario(
				new TransaccionesPorUsuario()
				{
					UsuarioId = usuarioId,
					FechaInicio = start,
					FechaFin = end
				});

			var eventosCalendarios = transacciones.Select(x => new EventoCalendario()
			{
				Title = x.Monto.ToString("N"),
				Start = x.FechaTransaccion.ToString("yyyy-MM-dd"),
				End = x.FechaTransaccion.ToString("yyyy-MM-dd"),
				Color = (x.TipoOperacionId == TipoOperacion.Gasto) ? "red" : "#0d6efd"
			});

			return Json(eventosCalendarios);

		}

		public async Task<JsonResult> ObtenerTransaccionesCalendarioPorFecha(DateTime fecha)
		{
			var usuarioId = servicioUsuario.ObtenerUsuarioId();
			var transacciones = await repoTransacciones.ObtenerTransaccionesPorUsuario(
				new TransaccionesPorUsuario()
				{
					UsuarioId = usuarioId,
					FechaInicio = fecha,
					FechaFin = fecha
				});

			return Json(transacciones);

		}

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
			var usuarioId = servicioUsuario.ObtenerUsuarioId();
			var modelo = new TransaccionViewModel();
			modelo.Cuentas = await ObtenerCuentas(usuarioId);
			modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
			return View(modelo);

        }

		[HttpPost]
		public  async Task<IActionResult> Crear(TransaccionViewModel modelo)
		{
			var usuarioId = servicioUsuario.ObtenerUsuarioId();
			if (!ModelState.IsValid)
			{
				modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
				modelo.Cuentas = await ObtenerCuentas(usuarioId);
				return View(modelo);
			}

			var cuenta = await repoCuenta.ObtenerCuentaPorId(modelo.CuentaId, usuarioId);
			if (cuenta is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			var categoria = await repoCategorias.ObtenerCategoriaPorId(modelo.CategoriaId, usuarioId);
			if (categoria is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			modelo.UsuarioId = usuarioId;

			if (modelo.TipoOperacionId == TipoOperacion.Gasto)
			{
				modelo.Monto *= -1;
			}

			await repoTransacciones.Crear(modelo);
			return RedirectToAction("Index");

		}

		[HttpPost]
		public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
		{
			var usuarioId = servicioUsuario.ObtenerUsuarioId();
			var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);

			return Ok(categorias);

		}

		[HttpGet]
		public async Task<IActionResult> Editar(int id, string? urlRetorno = null)
		{
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
			var transaccion = await repoTransacciones.ObtenerTransaccionPorId(id, usuarioId);

			if (transaccion is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			var modelo = mapper.Map<TransaccionActualizarViewModel>(transaccion);
			modelo.MontoAnterior = modelo.Monto;
			if (modelo.TipoOperacionId == TipoOperacion.Gasto)
			{
				modelo.MontoAnterior *= -1;
			}
			modelo.CuentaAnteriorId = modelo.CuentaId;
			modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
			modelo.Cuentas = await ObtenerCuentas(usuarioId);
			modelo.URLRetorno = urlRetorno;

			return View(modelo);
        }

		[HttpPost]
		public async Task<IActionResult> Editar(TransaccionActualizarViewModel modelo)
		{
			var usuarioId = servicioUsuario.ObtenerUsuarioId();

			if (!ModelState.IsValid)
			{
				modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
				modelo.Cuentas = await ObtenerCuentas(usuarioId);
				return View(modelo);
			}

			var cuenta = await repoCuenta.ObtenerCuentaPorId(modelo.CuentaId, usuarioId);
			if (cuenta is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			var categoria = await repoCategorias.ObtenerCategoriaPorId(modelo.CategoriaId, usuarioId);
			if (categoria == null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			var transaccion = mapper.Map<Transaccion>(modelo);

			if (modelo.TipoOperacionId == TipoOperacion.Gasto)
			{
				transaccion.Monto *= -1;
			}

			await repoTransacciones.Actualizar(transaccion, modelo.MontoAnterior, modelo.CuentaAnteriorId);

			if (string.IsNullOrEmpty(modelo.URLRetorno))
			{
				return RedirectToAction("Index");
			}
			else
			{
				return LocalRedirect(modelo.URLRetorno);
			}

		}

		[HttpPost]
		public async Task<IActionResult> Borrar(int id, string? urlRetorno = null)
		{
			var usuarioId = servicioUsuario.ObtenerUsuarioId();
			var transaccion = await repoTransacciones.ObtenerTransaccionPorId(id, usuarioId);

			if (transaccion is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			await repoTransacciones.BorrarTransaccion(id);
			if (string.IsNullOrEmpty(urlRetorno))
			{
				return RedirectToAction("Index");
			}
			else
			{
				return LocalRedirect(urlRetorno);
			}
		}

		private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
		{
			var cuentas = await repoCuenta.BuscarCuentas(usuarioId);
			return cuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
		}

		private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion)
		{
			var categorias = await repoCategorias.ObtenerCategorias(usuarioId, tipoOperacion);
			var resultado = categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString())).ToList();
			var opcionPorDefecto = new SelectListItem("--Seleccione una categoría--", "0", true);
			resultado.Insert(0, opcionPorDefecto);
			return resultado;
		}
	}
}
