using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace ManejoPresupuesto.Services
{
	//Interfaz para RepoTiposCuentas
	public interface IRepoTiposCuentas
	{
		Task Actualizar(TipoCuenta tipoCuenta);
		Task Borrar(int id);
		Task Crear(TipoCuenta tipoCuenta);
		Task<bool> Existe(string nombre, int usuarioId, int id = 0);
		Task<TipoCuenta> ObtenerPorId(int id, int usuarioId);
		Task<IEnumerable<TipoCuenta>> ObtenerTiposCuentas(int usuarioId);
		Task Ordenar(IEnumerable<TipoCuenta> tiposCuentasOrdenados);
	}
	public class RepoTiposCuentas : IRepoTiposCuentas
	{
		private readonly string connectionString;
		public RepoTiposCuentas(IConfiguration config)
		{
			connectionString = config.GetConnectionString("DefaultConnection");
		}
		//Metodo para crear un nuevo tipo de cuenta en la base de datos
		public async Task Crear(TipoCuenta tipoCuenta)
		{
			using var connection = new MySqlConnection(connectionString);
			var id = await connection.QuerySingleAsync<int>("TiposCuentas_Insertar",
				new
				{
					_UsuarioId = tipoCuenta.UsuarioId,
					_Nombre = tipoCuenta.Nombre
				},
				commandType: System.Data.CommandType.StoredProcedure);
			tipoCuenta.Id = id;
		}
		//Metodo para verificar si ya existe un tipo de cuenta por usuario
		//en la base de datos. Si existe retorna true, si no false
		public async Task<bool> Existe(string nombre, int usuarioId, int id = 0)
		{
			using var connection = new MySqlConnection(connectionString);
			var existe = await connection.QueryFirstOrDefaultAsync<int>
				(@"
				SELECT 1
				FROM TiposCuentas
				WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId AND id <> @id",
				new { nombre, usuarioId, id });
			return existe == 1;

		}
		//Metodo para obtener un iterable de todos los elementos <TipoCuenta>
		//mapeados desde la base de datos, segun los campos consultados
		public async Task<IEnumerable<TipoCuenta>> ObtenerTiposCuentas(int usuarioId)
		{
			using var connection = new MySqlConnection(connectionString);
			return await connection.QueryAsync<TipoCuenta>
				(@"
				SELECT Id, Nombre, Orden
				FROM TiposCuentas
				WHERE UsuarioId = @UsuarioId
				ORDER BY Orden",
				new { usuarioId });
		}

		//metodo que permite actualizar el campo Nombre de un tipo de cuenta en la base de datos
		public async Task Actualizar(TipoCuenta tipoCuenta)
		{
			using var connection = new MySqlConnection(connectionString);
			await connection.ExecuteAsync
				(@"
				UPDATE TiposCuentas
				SET Nombre = @Nombre
				WHERE Id = @Id",
				tipoCuenta);
		}

		//metodo que permite borrar un registro de tipo de cuenta segun su id
		public async Task Borrar(int id)
		{
			using var connection = new MySqlConnection(connectionString);
			await connection.ExecuteAsync
				(@"
				DELETE FROM TiposCuentas
				WHERE Id = @Id",
				new { id });
		}

		//metodo para obtener Id, Nombre y Orden de un tipo de cuenta por id de usuario, caso que exista
		public async Task<TipoCuenta> ObtenerPorId(int id, int usuarioId)
		{
			using var connection = new MySqlConnection(connectionString);
			return await connection.QueryFirstOrDefaultAsync<TipoCuenta>
				(@"
				SELECT Id, Nombre, Orden
				FROM TiposCuentas
				WHERE Id = @Id AND UsuarioId = @UsuarioId",
				new { id, usuarioId });
		}

		//metodo para actualizar el Orden de un tipo de cuenta
		public async Task Ordenar(IEnumerable<TipoCuenta> tiposCuentasOrdenados)
		{
			var query = "UPDATE TiposCuentas SET Orden = @Orden WHERE Id = @Id;";
			using var connection = new MySqlConnection(connectionString);
			await connection.ExecuteAsync(query, tiposCuentasOrdenados);

		}
	}
}
