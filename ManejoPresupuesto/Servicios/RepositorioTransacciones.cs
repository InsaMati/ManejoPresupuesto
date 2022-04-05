using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioTransacciones
    {
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task Borrar(int id);
        Task Crear(Transaccion transaccion);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int Año);
        Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo);
    }

    public class RepositorioTransacciones : IRepositorioTransacciones
    {
        private readonly string connectionString;

        public RepositorioTransacciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>("Transacciones_Insertar",
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

        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior,
            int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Actualizar",
                new
                {
                    transaccion.Id,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota,
                    montoAnterior,
                    cuentaAnteriorId
                }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(
                @"SELECT Transacciones.*, cat.TipoOperacionId
                FROM Transacciones
                INNER JOIN Categorias cat
                ON cat.Id = Transacciones.CategoriaId
                WHERE Transacciones.Id = @Id AND Transacciones.UsuarioId = @UsuarioId",
                new { id, usuarioId });
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Borrar",
                new { id }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>
                (@"SELECT t.Id, t.Monto,t.FechaTransaccion,c.Nombre as Categoria, cu.Nombre as Cuenta, c.TipoOperacionId
                   FROM Transacciones t
                   inner join Categorias c
                   on c.id = t.CategoriaId
                   inner join Cuentas cu
                   on cu.id = t.CuentaId
                   where t.CuentaId = @CuentaId and t.UsuarioId = @UsuarioId
                   and FechaTransaccion BETWEEN @FechaInicio and @FechaFin ", modelo);
        }


        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>
                (@"SELECT t.Id, t.Monto,t.FechaTransaccion,c.Nombre as Categoria, cu.Nombre as Cuenta, c.TipoOperacionId
                   FROM Transacciones t
                   inner join Categorias c
                   on c.id = t.CategoriaId
                   inner join Cuentas cu
                   on cu.id = t.CuentaId
                   where t.UsuarioId = @UsuarioId
                   and FechaTransaccion BETWEEN @FechaInicio and @FechaFin
                   ORDER BY t.fechaTransaccion DESC",
                   modelo);
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<ResultadoObtenerPorSemana>(@"
                         select DATEDIFF(d,@fechainicio, FechaTransaccion) / 7 + 1 as semana,
                         sum(Monto) as monto, cat.TipoOperacionId
                         from Transacciones
                         inner join categorias cat
                         on cat.id = Transacciones.CategoriaId
                         WHERE transacciones.UsuarioId = @usuarioId AND
                         FechaTransaccion between @fechainicio and @fechafin
                         group by DATEDIFF(d,@fechainicio, FechaTransaccion) / 7, cat.TipoOperacionId", modelo);
        }

        public async Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int Año)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<ResultadoObtenerPorMes>(@"select MONTH(FechaTransaccion) as mes,
                         sum(monto) as Monto, cat.TipoOperacionId
                         from Transacciones
                         inner join categorias cat
                         on cat.id = Transacciones.CategoriaId
                         where Transacciones.UsuarioId = @usuarioId and year (FechaTransaccion) = @Año
                         group by MONTH(FechaTransaccion), cat.TipoOperacionId", new { usuarioId, Año });
        }

    }
}