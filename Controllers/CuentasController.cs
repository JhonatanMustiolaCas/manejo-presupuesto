using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;

namespace ManejoPresupuesto.Controllers
{
	public class CuentasController: Controller
	{
		private readonly IRepoTiposCuentas repoTiposCuentas;
		private readonly IServicioUsuario servicioUsuario;
		private readonly IRepoCuenta repoCuenta;
		private readonly IRepoTransacciones repoTransacciones;
        private readonly IServicioReportes servicioReportes;
        private readonly IMapper mapper;

		public CuentasController(
			IRepoTiposCuentas repoTiposCuentas,
			IServicioUsuario servicioUsuario,
			IRepoCuenta repoCuenta,
			IRepoTransacciones repoTransacciones,
			IServicioReportes  servicioReportes,
			IMapper mapper)
        {
			this.repoTiposCuentas = repoTiposCuentas;
			this.servicioUsuario = servicioUsuario;
			this.repoCuenta = repoCuenta;
			this.repoTransacciones = repoTransacciones;
            this.servicioReportes = servicioReportes;
            this.mapper = mapper;
		}

		public async Task<IActionResult> Index()
		{
			var usuarioId = servicioUsuario.ObtenerUsuarioId();
			var cuentasConTipoCuenta = await repoCuenta.BuscarCuentas(usuarioId);

			var modelo = cuentasConTipoCuenta
				.GroupBy(x => x.TipoCuenta)
				.Select(grupo => new IndexCuentaViewModel
				{
					TipoCuenta = grupo.Key,
					Cuentas = grupo.AsEnumerable(),

				}).ToList();
			return View(modelo);
		}

		[HttpGet]
		public async Task<IActionResult> Detalle(int id, int mes, int agno)
		{
			var usuarioId = servicioUsuario.ObtenerUsuarioId();
			var cuenta = await repoCuenta.ObtenerCuentaPorId(id, usuarioId);

			if (cuenta is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			ViewBag.Cuenta = cuenta.Nombre;
			var modelo = await servicioReportes.ObtenerReporteTransaccionesDetalladasPorCuenta(usuarioId, id, mes, agno, ViewBag);
			return View(modelo);
		}


        [HttpGet]
		public async Task<IActionResult> Crear()
		{
			var usuarioId = servicioUsuario.ObtenerUsuarioId();
			
			var modelo = new CuentaViewModel();
			modelo.TiposCuentas = await ObtenerTiposCuentasSelect(usuarioId);
			return View(modelo);
		}

		[HttpPost]
		public async Task<IActionResult> Crear(CuentaViewModel cuenta)
		{
			var usuarioId = servicioUsuario.ObtenerUsuarioId();
			var tipoCuenta = await repoTiposCuentas.ObtenerPorId(cuenta.TipoCuentaId, usuarioId);

			if (tipoCuenta is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			if (!ModelState.IsValid)
			{
				cuenta.TiposCuentas = await ObtenerTiposCuentasSelect(usuarioId);
				return View(cuenta);
			}

			await repoCuenta.Crear(cuenta);
			return RedirectToAction("Index");
		}

		[HttpGet]
		public async Task<IActionResult> Editar(int id)
		{
			var usuarioId = servicioUsuario.ObtenerUsuarioId();
			var cuenta = await repoCuenta.ObtenerCuentaPorId(id, usuarioId);

			if (cuenta  is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}
			var modelo = mapper.Map<CuentaViewModel>(cuenta);
			modelo.TiposCuentas = await ObtenerTiposCuentasSelect(usuarioId);
			return View(modelo);
		}

		[HttpPost]
		public async Task<IActionResult> Editar(CuentaViewModel cuentaEditar)
		{
			var usuarioId = servicioUsuario.ObtenerUsuarioId();

			var cuenta = await repoCuenta.ObtenerCuentaPorId(cuentaEditar.Id, usuarioId);
			if (cuenta is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			var tipoCuenta = await repoTiposCuentas.ObtenerPorId(cuentaEditar.TipoCuentaId, usuarioId);
			 if (tipoCuenta is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			await repoCuenta.Actualizar(cuentaEditar);
			return RedirectToAction("Index");
		}

		[HttpGet]
		public async Task<IActionResult> Borrar(int id)
		{
			var usuarioId = servicioUsuario.ObtenerUsuarioId();
			var cuenta = await repoCuenta.ObtenerCuentaPorId(id, usuarioId);

			if (cuenta is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}
			return View(cuenta);
		}

		[HttpPost]
		public async Task<IActionResult> BorrarCuenta(int id)
		{
			var usuarioId = servicioUsuario.ObtenerUsuarioId();
			var cuenta = await repoCuenta.ObtenerCuentaPorId(id, usuarioId);

			if (cuenta is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			await repoCuenta.BorrarCuenta(id);
			return RedirectToAction("Index");
		}

		private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentasSelect(int usuarioId)
		{
			var tiposCuentas = await repoTiposCuentas.ObtenerTiposCuentas(usuarioId);
			return tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
		}
	}
}
