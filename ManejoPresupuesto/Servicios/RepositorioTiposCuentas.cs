using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTiposCuentas
    {
        Task Actualizar(TipoCuenta tipocuenta);
        Task Crear(TipoCuenta tipocuenta);
        Task<bool> Existe(string nombre, int usuarioId);
        Task<IEnumerable<TipoCuenta>> GetAll(int usuarioId);
        Task<TipoCuenta> ObtenerPorId(int id, int usuarioId);
    }
    public class RepositorioTiposCuentas : IRepositorioTiposCuentas
    {
        private readonly string connectionString;
        public RepositorioTiposCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(TipoCuenta tipocuenta)
        {   
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>
                   (@"INSERT INTO TiposCuentas (Nombre, UsuarioId, Orden) values (@Nombre,@UsuarioId,0);
                   SELECT SCOPE_IDENTITY();",tipocuenta);

            tipocuenta.Id = id; 
        }

        public async Task<bool> Existe(string nombre, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            var existe = await connection.QueryFirstOrDefaultAsync<int>(@"SELECT 1 FROM TiposCuentas WHERE Nombre = @Nombre AND UsuarioId = @UsuarioId",
            new { nombre, usuarioId});

            return existe == 1;
        }

        public async Task<IEnumerable<TipoCuenta>> GetAll(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<TipoCuenta>(@"SELECT Id, Nombre, Orden FROM TiposCuentas WHERE UsuarioId = @UsuarioId", new { usuarioId });
        }

        public async Task Actualizar(TipoCuenta tipocuenta)
        {
            using var connection = new SqlConnection(connectionString);
            // Permite ejecutar una Query que no va a retornar nada
            await connection.ExecuteAsync(@"UPDATE TiposCuentas SET Nombre = @Nombre where Id = @Id",tipocuenta);
        }

        public async Task<TipoCuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>(@"Select Id,Nombre,Orden from TiposCuentas where Id = @Id and UsuarioId = @UsuarioId",
                new {id,usuarioId});
        }
    }
}

