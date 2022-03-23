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

        public TiposCuentasController(IRepositorioTiposCuentas repositorioTiposCuentas )
        {
            this.repositorioTiposCuentas = repositorioTiposCuentas;
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

            tipocuenta.UsuarioId = 1;

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

        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre)
        {
            var usuarioId = 1;

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
            var usuarioId = 1;

            var tiposcuentas = await repositorioTiposCuentas.GetAll(usuarioId);

            return View(tiposcuentas);
        }
    }
}
