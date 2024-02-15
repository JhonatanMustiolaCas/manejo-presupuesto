using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ManejoPresupuesto.Services
{
	public interface IServicioUsuario
	{
		int ObtenerUsuarioId();
	}
	public class ServicioUsuario: IServicioUsuario
	{
		private readonly HttpContext httpContext;
        public ServicioUsuario(IHttpContextAccessor httpContextAccesor)
        {
			this.httpContext = httpContextAccesor.HttpContext;
        }

        public int ObtenerUsuarioId()
		{
			if (httpContext.User.Identity.IsAuthenticated)
			{
				var idClaim = httpContext.User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
				var id = int.Parse(idClaim.Value);
				return id;
			}
			else
			{
				throw new ApplicationException("El usuario no esta autenticado");
			}
		}
	}
}
