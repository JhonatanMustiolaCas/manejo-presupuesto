using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

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
			using var connection = new SqlConnection(connectionString);
			var usuarioId = await connection.QuerySingleAsync<int>
				(@"
				INSERT INTO Usuarios (Email, EmailNormalizado, PasswordHash)
				VALUES (@Email, @EmailNormalizado, @PasswordHash);
				SELECT SCOPE_IDENTITY()",
				usuario);

			await connection.ExecuteAsync
				("CrearDataUsuarioNuevo", new { usuarioId }, commandType: System.Data.CommandType.StoredProcedure);


			return usuarioId;
		}

		public async Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado)
		{
			using var connection = new SqlConnection(connectionString);
			return await connection.QuerySingleOrDefaultAsync<Usuario>
				("SELECT * FROM Usuarios WHERE EmailNormalizado = @emailNormalizado", new {emailNormalizado});
		}
    }
}
