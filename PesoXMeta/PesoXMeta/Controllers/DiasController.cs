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
            List<DateTime> datas = new List<DateTime>();
            List<double> pesos = new List<double>();
            List<double> porcentagem = new List<double>();
            List<int> id = new List<int>();
            double anterior;
            double porcentual;
            double dif;

            var user = User.Identity.Name;
            var dias = (from c in _context.Controle
                        join usuario in _context.Users
                        on c.IdentityUserID equals usuario.Id
                        where usuario.UserName == user
                        select c.DataInicio).FirstOrDefault();
            datas.Add(dias);

            var idDias1 = (from c in _context.Controle
                           join usuario in _context.Users
                           on c.IdentityUserID equals usuario.Id
                           where usuario.UserName == user
                           && c.DataInicio == dias
                           select c.Id).FirstOrDefault();
            id.Add(idDias1);

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
                    datas.Add(diaEspecificado);

                    var idDias = (from c in _context.Dias
                                  join usuario in _context.Users
                                  on c.IdentityUserId equals usuario.Id
                                  where usuario.UserName == user
                                  && c.Data == diaEspecificado
                                  select c.Id).FirstOrDefault();
                    id.Add(idDias);
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
            ViewBag.Id = id;
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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var diasPeso = await _context.Dias.FindAsync(id);
            if (diasPeso == null)
            {
                return NotFound();
            }
            return View(diasPeso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Data, Peso, IdentityUserId")] PesoDias pesoDias)
        {
            if (id != pesoDias.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pesoDias);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PesoDiasExists(pesoDias.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Acompanhar));
            }
            return View(pesoDias);
        }

        //public IActionResult RetornaId(DateTime data)
        //{
        //    var dataString = data.ToString("dd-MM-yyyy");
        //    var id = (from pesos in _context.Dias
        //              where pesos.Data.ToString("dd-MM-yyyy") == dataString
        //              select pesos.Id).FirstOrDefault();

        //    return RedirectToAction(nameof(Edit));
        //}

        private bool PesoDiasExists(int id)
        {
            return _context.Dias.Any(e => e.Id == id);
        }
    }
}