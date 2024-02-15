using Dapper;
using ManejoPresupuesto.Model;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace ManejoPresupuesto.Services
{
	public interface IRepoCategorias
	{
		Task Actualizar(Categoria categoria);
		Task Borrar(int id);
		Task<int> ContarCategorias(int usuarioId);
		Task Crear(Categoria categoria);
		Task<Categoria> ObtenerCategoriaPorId(int id, int usuarioId);
		Task<IEnumerable<Categoria>> ObtenerCategorias(int usuarioId, PaginacionViewModel paginacionViewModel);
		Task<IEnumerable<Categoria>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion);
	}
	public class RepoCategorias : IRepoCategorias
	{
		private readonly string connectionString;

		public RepoCategorias(IConfiguration configuration)
		{
			this.connectionString = configuration.GetConnectionString("DefaultConnection");
		}

		public async Task Crear(Categoria categoria)
		{
			using var connection = new MySqlConnection(connectionString);
			var id = await connection.QuerySingleAsync<int>
				(@"
				INSERT INTO Categorias (Nombre, TipoOperacionId, UsuarioId)
				VALUES (@Nombre, @TipoOperacionId, @UsuarioId);
				SELECT LAST_INSERT_ID();",
				categoria);

			categoria.Id = id;
		}

		public async Task<IEnumerable<Categoria>>
			ObtenerCategorias(
				int usuarioId,
				PaginacionViewModel paginacionViewModel)
		{
			using var connection = new MySqlConnection(connectionString);
			return await connection.QueryAsync<Categoria>
				(@$"
				SELECT * FROM Categorias
				WHERE UsuarioId = @UsuarioId
				ORDER BY Nombre
				LIMIT {paginacionViewModel.RecordsPorPagina}
				OFFSET {paginacionViewModel.RecordsASaltar}",
				new { usuarioId });
		}

		public async Task<int> ContarCategorias(int usuarioId)
		{
			using var connection = new MySqlConnection(connectionString);
			return await connection.ExecuteScalarAsync<int>
			("SELECT COUNT(*) FROM Categorias WHERE UsuarioId = @usuarioId", new { usuarioId });
		}

		public async Task<IEnumerable<Categoria>>
			ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacionId)
		{
			using var connection = new MySqlConnection(connectionString);
			return await connection.QueryAsync<Categoria>
				(@"
				SELECT * FROM Categorias
				WHERE UsuarioId = @UsuarioId AND TipoOperacionId = @TipoOperacionId",
				new { usuarioId, tipoOperacionId });
		}

		public async Task<Categoria> ObtenerCategoriaPorId(int id, int usuarioId)
		{
			using var connection = new MySqlConnection(connectionString);
			return await connection.QueryFirstOrDefaultAsync<Categoria>
				(@"
				SELECT * FROM Categorias
				WHERE id = @id AND UsuarioId = @UsuarioId",
				new { id, usuarioId });
		}

		public async Task Actualizar(Categoria categoria)
		{
			using var connection = new MySqlConnection(connectionString);
			await connection.ExecuteAsync
				(@"
				UPDATE Categorias
				SET Nombre = @Nombre, TipoOperacionId = @TipoOperacionId
				WHERE id = @id",
				categoria);
		}

		public async Task Borrar(int id)
		{
			using var connection = new MySqlConnection(connectionString);
			await connection.ExecuteAsync
				(@"
				DELETE FROM Categorias
				WHERE Id = @id",
				new { id });
		}

	}
}
