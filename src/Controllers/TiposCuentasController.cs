using ManejoPresupuesto.Models;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
    public class TiposCuentasController : Controller
    {
        private readonly IRepoTiposCuentas repoTiposCuentas;
        private readonly IServicioUsuario servicioUsuario;

        public TiposCuentasController(IRepoTiposCuentas repoTiposCuentas,
                                        IServicioUsuario servicioUsuario)
        {
            this.repoTiposCuentas = repoTiposCuentas;
            this.servicioUsuario = servicioUsuario;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var tiposCuentas = await repoTiposCuentas.ObtenerTiposCuentas(usuarioId);
            return View(tiposCuentas);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var tipoCuenta = await repoTiposCuentas.ObtenerPorId(id, usuarioId);
            
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }
		[HttpPost]
		public async Task<IActionResult> Editar(TipoCuenta tipoCuenta)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var tipoCuentaExiste = await repoTiposCuentas.ObtenerPorId(tipoCuenta.Id, usuarioId);

            if (tipoCuentaExiste is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repoTiposCuentas.Actualizar(tipoCuenta);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var tipoCuenta = await repoTiposCuentas.ObtenerPorId(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(tipoCuenta);
        }
        [HttpPost]
		public async Task<IActionResult> EliminarTipoCuenta(int id)
        {
			var usuarioId = servicioUsuario.ObtenerUsuarioId();
			var tipoCuentaExiste = await repoTiposCuentas.ObtenerPorId(id, usuarioId);

			if (tipoCuentaExiste is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}
			await repoTiposCuentas.Borrar(id);
			return RedirectToAction("Index");

		}

		[HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta)
        {
            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }
            tipoCuenta.UsuarioId = servicioUsuario.ObtenerUsuarioId();

            var existeTipoCuenta = await repoTiposCuentas.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);
            
            if (existeTipoCuenta)
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre), $"El nombre {tipoCuenta.Nombre} ya existe");
                return View(tipoCuenta);
            }
            await repoTiposCuentas.Crear(tipoCuenta);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre, int id)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
			var existeTipoCuenta = await repoTiposCuentas.Existe(nombre, usuarioId, id);

            if (existeTipoCuenta)
            {
                return Json($"El nombre {nombre} ya existe");
            }
            return Json(true);
		}

        [HttpPost]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
			var usuarioId = servicioUsuario.ObtenerUsuarioId();
			var tiposCuentas = await repoTiposCuentas.ObtenerTiposCuentas(usuarioId);
            var idsTiposCuentas = tiposCuentas.Select(x => x.Id);

            var idsTiposCuentasNoPertenecenAlUsuario = ids.Except(idsTiposCuentas).ToList();

            if (idsTiposCuentasNoPertenecenAlUsuario.Count > 0)
            {
                return Forbid();
            }

            var tiposCuentasOrdenados = ids.Select((valor, indice) => new TipoCuenta() { Id = valor, Orden = indice + 1 }).AsEnumerable();
            await repoTiposCuentas.Ordenar(tiposCuentasOrdenados);
			return Ok();
        }


    }
}
