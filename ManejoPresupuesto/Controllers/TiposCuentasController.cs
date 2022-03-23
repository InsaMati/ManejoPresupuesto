using Dapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Controllers
{
    public class TiposCuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTiposCuentas;
        private readonly IServiciosUsuarios serviciosUsuarios;

        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas, IServiciosUsuarios serviciosUsuarios )
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
            this.serviciosUsuarios = serviciosUsuarios;
        }

        [HttpGet]   
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Crear(TipoCuenta tipocuenta)
        {
            if (!ModelState.IsValid)
            {
                return View(tipocuenta);
            }

            tipocuenta.UsuarioId = serviciosUsuarios.ObtenerUsuarioId();

            var yaExisteTipoCuenta = await repositorioTiposCuentas.Existe(tipocuenta.Nombre, tipocuenta.UsuarioId);

            if (yaExisteTipoCuenta)
            {
                ModelState.AddModelError(nameof(tipocuenta.Nombre), $"El nombre {tipocuenta.Nombre} ya existe.");

                return View(tipocuenta);
            }

            await repositorioTiposCuentas.Crear(tipocuenta);

            return RedirectToAction("Index");
        }

        [HttpGet]

        public async Task<ActionResult> Editar(int id)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTiposCuentas.ObtenerPorId(id,usuarioId);

            if(tipoCuenta == null)
            {
                return RedirectToAction("NoEncontrado","Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost]

        public async Task<ActionResult> Editar(TipoCuenta tipocuenta)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            var tipoCuentaExiste = await repositorioTiposCuentas.ObtenerPorId(tipocuenta.Id,usuarioId);

            if(tipoCuentaExiste == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioTiposCuentas.Actualizar(tipocuenta);
            return RedirectToAction("Index");
        }

        [HttpGet]

        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre)
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();

            var yaExisteTipoCuenta = await repositorioTiposCuentas.Existe(nombre, usuarioId);

            if (yaExisteTipoCuenta)
            {
                return Json($"El nombre {nombre} ya existe");
            }

            // Googlear
            return Json(true);
        }

        [HttpGet]

        public async Task<IActionResult> Index()
        {
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();

            var tiposcuentas = await repositorioTiposCuentas.GetAll(usuarioId);

            return View(tiposcuentas);
        }
    }
}
