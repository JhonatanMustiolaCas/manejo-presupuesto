using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Services
{

	public interface IRepoCuenta
	{
		Task Actualizar(CuentaViewModel cuenta);
		Task BorrarCuenta(int id);
		Task<IEnumerable<Cuenta>> BuscarCuentas(int usuarioId);
		Task Crear(Cuenta cuenta);
		Task<Cuenta> ObtenerCuentaPorId(int id, int usuarioId);
	}
	public class RepoCuenta : IRepoCuenta
	{
		private readonly string connectionString;

		public RepoCuenta(IConfiguration configuration)
        {
			this.connectionString = configuration.GetConnectionString("DefaultConnection");
		}

		public async Task Crear(Cuenta cuenta)
		{
			using var connection = new SqlConnection(connectionString);

			var id = await connection.QuerySingleAsync<int>
				(@"
				INSERT INTO Cuentas (Nombre, TipoCuentaId, Descripcion, Balance)
				VALUES (@Nombre, @TipoCuentaId, @Descripcion, @Balance)
				
				SELECT SCOPE_IDENTITY()",
				cuenta);

			cuenta.Id = id;
		}

		public async Task<IEnumerable<Cuenta>> BuscarCuentas(int usuarioId)
		{
			using var connection = new SqlConnection(connectionString);
			return await connection.QueryAsync<Cuenta>
				(@"
				SELECT Cuentas.id, Cuentas.Nombre, Balance, tc.Nombre AS TipoCuenta
				FROM Cuentas
				INNER JOIN TiposCuentas tc
				ON tc.id = TipoCuentaId
				WHERE tc.UsuarioId = @UsuarioId
				ORDER BY tc.Orden",
				new {usuarioId});
		}

		public async Task<Cuenta> ObtenerCuentaPorId(int id, int usuarioId)
		{
			using var connection = new SqlConnection(connectionString);
			return await connection.QueryFirstOrDefaultAsync<Cuenta>
				(@"
				SELECT Cuentas.id, Cuentas.Nombre, Balance, Descripcion, TipoCuentaId
				FROM Cuentas
				INNER JOIN TiposCuentas tc
				ON tc.id = TipoCuentaId
				WHERE Cuentas.id = @id AND tc.UsuarioId = @UsuarioId",
				new {id, usuarioId});
		}

		public async Task Actualizar(CuentaViewModel cuenta)
		{
			using var connection = new SqlConnection(connectionString);
			await connection.ExecuteAsync
				(@"
				UPDATE Cuentas
				SET Nombre = @Nombre, Balance = @Balance, Descripcion = @Descripcion, TipoCuentaId = @TipoCuentaId
				WHERE id = @id",
				cuenta);
		}

		public async Task BorrarCuenta(int id)
		{
			using var connection = new SqlConnection(connectionString);
			await connection.ExecuteAsync
				(@"
				DELETE Cuentas
				WHERE id = @id",
				new {id});
		}
    }
}
