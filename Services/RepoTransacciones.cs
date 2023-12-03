using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{
	public interface IRepoTransacciones
	{
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId);
		Task BorrarTransaccion(int id);
		Task Crear(Transaccion transaccion);
		Task<IEnumerable<Transaccion>> ObtenerTransaccionesPorCuenta(TransaccionesPorCuenta modelo);
		Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerTransaccionesPorMes(int usuarioId, int agno);
		Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerTransaccionesPorSemana(TransaccionesPorUsuario modelo);
		Task<IEnumerable<Transaccion>> ObtenerTransaccionesPorUsuario(TransaccionesPorUsuario modelo);
		Task<Transaccion> ObtenerTransaccionPorId(int id, int usuarioId);
    }
	public class RepoTransacciones : IRepoTransacciones
	{
		private string connectionString;

		public RepoTransacciones(IConfiguration configuration)
        {
			this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

		public async Task Crear(Transaccion transaccion)
		{
			using var connection = new SqlConnection(connectionString);
			var id = await connection.QuerySingleAsync<int>
				("Transacciones_Insertar",
				new 
				{
					transaccion.UsuarioId,
					transaccion.FechaTransaccion,
					transaccion.Monto,
					transaccion.CategoriaId,
					transaccion.CuentaId,
					transaccion.Nota
				},
				commandType: System.Data.CommandType.StoredProcedure);

			transaccion.Id = id;
		}

		public async Task Actualizar(Transaccion  transaccion, decimal montoAnterior, int cuentaAnteriorId)
		{
            using var connection = new SqlConnection(connectionString);
			await connection.ExecuteAsync
				("Transacciones_Actualizar",
				new
				{
                    transaccion.Id,
                    transaccion.FechaTransaccion,
					montoAnterior,
                    transaccion.Monto,
                    transaccion.CuentaId,
					cuentaAnteriorId,
                    transaccion.CategoriaId,
                    transaccion.Nota
                },
				commandType: System.Data.CommandType.StoredProcedure);
        }

		public async Task<Transaccion> ObtenerTransaccionPorId(int id, int usuarioId)
		{
            using var connection = new SqlConnection(connectionString);
			return await connection.QueryFirstOrDefaultAsync<Transaccion>
				(@"
				SELECT Transacciones.*, cat.TipoOperacionId
				FROM Transacciones
				INNER JOIN Categorias cat
				ON cat.Id = Transacciones.CategoriaId
				WHERE Transacciones.id = @id AND Transacciones.UsuarioId = @UsuarioId",
				new {id, usuarioId});
        }

		public async Task BorrarTransaccion(int id)
		{
			using var connection = new SqlConnection(connectionString);
			await connection.ExecuteAsync
				("Transacciones_Borrar",
				new {id},
				commandType: System.Data.CommandType.StoredProcedure);
		}

		public async Task<IEnumerable<Transaccion>> ObtenerTransaccionesPorCuenta(TransaccionesPorCuenta modelo)
		{
			using var connection = new SqlConnection(connectionString);
			return await connection.QueryAsync<Transaccion>
				(@"
				SELECT t.Id, t.Monto, t.FechaTransaccion, c.Nombre as Categoria, cu.Nombre as Cuenta, c.TipoOperacionId
				FROM Transacciones t
				INNER JOIN Categorias c
				ON c.Id = t.CategoriaId
				INNER JOIN Cuentas cu
				ON cu.Id = t.CuentaId
				WHERE t.CuentaId = @CuentaId AND t.UsuarioId = @UsuarioId
				AND FechaTransaccion BETWEEN @FechaInicio AND @FechaFin;
				",
				modelo);
		}
		public async Task<IEnumerable<Transaccion>> ObtenerTransaccionesPorUsuario(TransaccionesPorUsuario modelo)
		{
			using var connection = new SqlConnection(connectionString);
			return await connection.QueryAsync<Transaccion>
				(@"
				SELECT t.Id, t.Monto, t.FechaTransaccion, c.Nombre as Categoria, cu.Nombre as Cuenta, c.TipoOperacionId, Nota
				FROM Transacciones t
				INNER JOIN Categorias c
				ON c.Id = t.CategoriaId
				INNER JOIN Cuentas cu
				ON cu.Id = t.CuentaId
				WHERE t.UsuarioId = @UsuarioId
				AND t.FechaTransaccion BETWEEN @FechaInicio AND @FechaFin
				ORDER BY t.FechaTransaccion DESC;",
				modelo);
		}

		public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerTransaccionesPorSemana(TransaccionesPorUsuario modelo)
		{
			using var connection = new SqlConnection(connectionString);
			return await connection.QueryAsync<ResultadoObtenerPorSemana>
				(@"
				SELECT datediff(d, @fechaInicio, @fechaFin) / 7 + 1 as Semana,
				SUM (Monto) as Monto, cat.TipoOperacionId
				FROM Transacciones
				INNER JOIN Categorias cat
				ON cat.id = Transacciones.CategoriaId
				WHERE Transacciones.UsuarioId = @usuarioId AND
				FechaTransaccion BETWEEN @fechaInicio AND @fechaFin
				GROUP BY datediff(d, @fechaInicio, FechaTransaccion) / 7 + 1, cat.TipoOperacionId",
				modelo);
		}

		public async Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerTransaccionesPorMes(int usuarioId, int agno)
		{
			using var connection = new SqlConnection(connectionString);
			return await connection.QueryAsync<ResultadoObtenerPorMes>
				(@"
				SELECT MONTH(FechaTransaccion) as mes,
				SUM(Monto) as Monto, cat.TipoOperacionId
				FROM Transacciones
				INNER JOIN Categorias cat
				ON cat.Id = Transacciones.CategoriaId
				WHERE Transacciones.UsuarioId = @usuarioId AND YEAR(FechaTransaccion) = @agno
				GROUP BY Month(FechaTransaccion), cat.TipoOperacionId",
				new {usuarioId, agno});
		}
	}
}
