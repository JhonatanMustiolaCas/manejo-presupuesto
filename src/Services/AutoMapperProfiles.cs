using AutoMapper;
using ManejoPresupuesto.Models;

namespace ManejoPresupuesto.Services
{
	public class AutoMapperProfiles : Profile
	{
        public AutoMapperProfiles()
        {
            CreateMap<Cuenta, CuentaViewModel>();
            CreateMap<Transaccion, TransaccionActualizarViewModel>().ReverseMap();
        }
    }
}
