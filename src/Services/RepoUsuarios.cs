using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace ManejoPresupuesto.Services
{
	public interface IRepoUsuarios
	{
		Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado);
		Task<int> CrearUsuario(Usuario usuario);
	}
	public class RepoUsuarios : IRepoUsuarios
	{
		private readonly string connectionString;
		public RepoUsuarios(IConfiguration configuration)
		{
			this.connectionString = configuration.GetConnectionString("DefaultConnection");
		}

		public async Task<int> CrearUsuario(Usuario usuario)
		{
			using var connection = new MySqlConnection(connectionString);
			var usuarioId = await connection.QuerySingleAsync<int>
				(@"
				INSERT INTO Usuarios (Email, EmailNormalizado, PasswordHash)
				VALUES (@Email, @EmailNormalizado, @PasswordHash);
				SELECT LAST_INSERT_ID()",
				usuario);

			await connection.ExecuteAsync
				("CrearDatosUsuarioNuevo", new { _UsuarioId = usuarioId }, commandType: System.Data.CommandType.StoredProcedure);


			return usuarioId;
		}

		public async Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado)
		{
			using var connection = new MySqlConnection(connectionString);
			return await connection.QuerySingleOrDefaultAsync<Usuario>
				("SELECT * FROM Usuarios WHERE EmailNormalizado = @emailNormalizado", new { emailNormalizado });
		}
	}
}
