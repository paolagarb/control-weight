using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PesoXMeta.Data;
using PesoXMeta.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PesoXMeta.Controllers
{
    public class DiasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Dias.Include(p => p.IdentityUser);
            return View(await applicationDbContext.ToListAsync());
        }

        public IActionResult Adicionar()
        {
            return View();
        }

        public IActionResult Acompanhar()
        {
            List<string> datas = new List<string>();
            List<double> pesos = new List<double>();
            List<double> porcentagem = new List<double>();
            double anterior;
            double porcentual;
            double dif;

            var user = User.Identity.Name;
            var dias = (from c in _context.Controle
                        join usuario in _context.Users
                        on c.IdentityUserID equals usuario.Id
                        where usuario.UserName == user
                        select c.DataInicio).FirstOrDefault();
            datas.Add(dias.ToString("dd-MM"));

            var peso = (from c in _context.Controle
                        join usuario in _context.Users
                        on c.IdentityUserID equals usuario.Id
                        where usuario.UserName == user
                        select c.Peso).FirstOrDefault();
            anterior = peso;
            pesos.Add(peso);

            DateTime atual = DateTime.Today;
            var total = Convert.ToInt32((atual.Subtract(dias)).TotalDays);

            for (int i = 1; i <= total; i++)
            {
                var datax = dias.AddDays(i);

                var diaEspecificado = (from c in _context.Dias
                                       join usuario in _context.Users
                                       on c.IdentityUserId equals usuario.Id
                                       where usuario.UserName == user &&
                                       c.Data == datax
                                       select c.Data).FirstOrDefault();
                if (diaEspecificado.ToString("dd-MM-yyyy") != "01-01-0001")
                {
                    datas.Add(diaEspecificado.ToString("dd-MM"));
                }
                var pesoEspecificado = (from c in _context.Dias
                                        join usuario in _context.Users
                                        on c.IdentityUserId equals usuario.Id
                                        where usuario.UserName == user &&
                                        c.Data == datax
                                        select c.Peso).FirstOrDefault();
                if (pesoEspecificado != 0)
                {
                    porcentual = (100 * pesoEspecificado) / anterior;
                    dif = 100 - porcentual;

                    pesos.Add(pesoEspecificado);
                    porcentagem.Add(dif);

                    anterior = pesoEspecificado;
                }
            }
            ViewBag.Datas = datas;
            ViewBag.Pesos = pesos;
            ViewBag.Porcentagem = porcentagem;
            return View();
        }

        public IActionResult AdicionarDia(DateTime data, double peso)
        {
            var user = User.Identity.Name;
            var userId = (from id in _context.User
                          where id.UserName == user
                          select id.Id).Single();

            PesoDias pd = new PesoDias();
            pd.IdentityUserId = userId;
            pd.Data = data;
            pd.Peso = peso;

            _context.Add(pd);
            _context.SaveChanges();

            return RedirectToAction("Home", "Controles");
        }

        private bool PesoDiasExists(int id)
        {
            return _context.Dias.Any(e => e.Id == id);
        }
    }
}