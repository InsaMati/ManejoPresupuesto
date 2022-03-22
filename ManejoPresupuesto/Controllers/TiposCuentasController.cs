using ManejoPresupuesto.Models;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
    public class TiposCuentasController : Controller
    {

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]

        public IActionResult Crear(TipoCuenta tipocuenta)
        {
            if (!ModelState.IsValid)
            {
                return View(tipocuenta);
            }

            return View();
        }
    }
}
