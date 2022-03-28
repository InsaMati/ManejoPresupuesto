using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Models
{
    public class CuentaCreacionViewModel : Cuenta
    {
        // es una clase que nos permita crear select de una manera mas facil
        public IEnumerable<SelectListItem> TiposCuentas { get; set; }
    }
}
