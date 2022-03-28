using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioCuentas
    {
        Task Actualizar(CuentaCreacionViewModel cuenta);
        Task Borrar(int id);
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task Crear(Cuenta cuenta);
        Task<Cuenta> ObtenerPorId(int id, int usuarioId);
    }
    public class RepositorioCuentas : IRepositorioCuentas
    {
        private readonly string connectionString;
        public RepositorioCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Cuenta cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO Cuentas (Nombre,TipoCuentaId,Balance,Descripcion)
                                                       VALUES (@Nombre,@TipoCuentaId,@Balance,@Descripcion);
                                                       SELECT SCOPE_IDENTITY();", cuenta);
            cuenta.Id = id;
        }

        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<Cuenta>(@"select Cuentas.id, Cuentas.Nombre, balance, tc.Nombre as TipoCuenta
                                                      from cuentas
                                                      inner join TiposCuentas tc
                                                      on tc.id = cuentas.TipoCuentaId
                                                      where tc.UsuarioId = @UsuarioId
                                                      order by tc.Orden", new {usuarioId}); 
        }

        public async Task<Cuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Cuenta>(
                @"select Cuentas.id, Cuentas.Nombre, balance, Descripcion, tc.TipoCuentaId
                                                      from cuentas
                                                      inner join TiposCuentas tc
                                                      on tc.id = cuentas.TipoCuentaId
                                                      where tc.UsuarioId = @UsuarioId AND Cuentas.id @Id", new {id,usuarioId});
        }

        public async Task Actualizar (CuentaCreacionViewModel cuenta)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync("UPDATE Cuentas SET Nombre = @Nombre, Balance = @Balance, Descripcion = @Descripcion, TipoCuentaId = @TipoCuentaId WHERE Id = @Id;",
                cuenta);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("delete cuentas where Id = @Id", new { id });
        }
    }
}
