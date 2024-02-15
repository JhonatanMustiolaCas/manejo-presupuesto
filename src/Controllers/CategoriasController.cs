using ManejoPresupuesto.Model;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
	public class CategoriasController : Controller
	{
		private readonly IServicioUsuario servicioUsuario;
		private readonly IRepoCategorias repoCategorias;

		public CategoriasController(
            IServicioUsuario servicioUsuario,
            IRepoCategorias repoCategorias)
        {
			this.servicioUsuario = servicioUsuario;
			this.repoCategorias = repoCategorias;
		}

		[HttpGet]
		public async Task<IActionResult> Index(PaginacionViewModel paginacionViewModel)
		{
			var usuarioId = servicioUsuario.ObtenerUsuarioId();
			var categorias = await repoCategorias.ObtenerCategorias(usuarioId, paginacionViewModel);

			var totalCategorias = await repoCategorias.ContarCategorias(usuarioId);

			var respuestaVM = new PaginacionRespuesta<Categoria>
			{
				Elementos = categorias,
				Pagina = paginacionViewModel.Pagina,
				RecordsPorPagina = paginacionViewModel.RecordsPorPagina,
				CantidadTotalRecords = totalCategorias,
				BaseURL = Url.Action()
			};

			return View(respuestaVM);
		}

		[HttpGet]
		public IActionResult Crear()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Crear(Categoria categoria)
		{
			if (!ModelState.IsValid)
			{
				return View(categoria);
			}

			var usuarioId = servicioUsuario.ObtenerUsuarioId();

			categoria.UsuarioId = usuarioId;
			await repoCategorias.Crear(categoria);
			return RedirectToAction("Index");
		}

		[HttpGet]
		public async Task<IActionResult> Editar(int id)
		{
			var usuarioId = servicioUsuario.ObtenerUsuarioId();
			var categoria = await repoCategorias.ObtenerCategoriaPorId(id, usuarioId);

			if (categoria is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			return View(categoria);
		}

		[HttpPost]
		public async Task<IActionResult> Editar(Categoria categoriaEditar)
		{
			if (!ModelState.IsValid)
			{
				return View(categoriaEditar);
			}
			var usuarioId = servicioUsuario.ObtenerUsuarioId();
			var categoria = await repoCategorias.ObtenerCategoriaPorId(categoriaEditar.Id, usuarioId);

			if (categoria is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			categoriaEditar.UsuarioId = usuarioId;
			await repoCategorias.Actualizar(categoriaEditar);
			return RedirectToAction("Index");
		}

		[HttpGet]
		public async Task<IActionResult> Borrar(int id)
		{
			var usuarioId = servicioUsuario.ObtenerUsuarioId();
			var categoria = await repoCategorias.ObtenerCategoriaPorId(id, usuarioId);

			if (categoria is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			return View(categoria);
		}

		[HttpPost]
		public async Task<IActionResult> BorrarCategoria(int id)
		{
			var usuarioId = servicioUsuario.ObtenerUsuarioId();
			var categoria = await repoCategorias.ObtenerCategoriaPorId(id, usuarioId);

			if (categoria is null)
			{
				return RedirectToAction("NoEncontrado", "Home");
			}

			await repoCategorias.Borrar(id);
			return RedirectToAction("Index");
		}
    }
}
