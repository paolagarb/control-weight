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
            List<PesoDias> pd = new List<PesoDias>();
            List<double> porcentagem = new List<double>();
            double anterior;
            double porcentual;
            double dif;
            int idDias = 0;
            DateTime diaEspecificado;
            int total = 0;
            DateTime datax;

            var user = User.Identity.Name;
            var userId = (from id in _context.User
                          where id.UserName == user
                          select id.Id).Single();

            var dias = (from c in _context.Controle
                        join usuario in _context.Users
                        on c.IdentityUserID equals usuario.Id
                        where usuario.UserName == user
                        select c.DataInicio).FirstOrDefault();

            var idDias1 = (from c in _context.Controle
                           join usuario in _context.Users
                           on c.IdentityUserID equals usuario.Id
                           where usuario.UserName == user
                           && c.DataInicio == dias
                           select c.Id).FirstOrDefault();

            var peso = (from c in _context.Controle
                        join usuario in _context.Users
                        on c.IdentityUserID equals usuario.Id
                        where usuario.UserName == user
                        select c.Peso).FirstOrDefault();

            pd.Add(new PesoDias
            {
                Id = idDias1,
                IdentityUserId = userId,
                Data = dias,
                Peso = peso
            });

            var primeiroDia = (from c in _context.Dias
                               join usuario in _context.Users
                                on c.IdentityUserId equals usuario.Id
                               orderby c.Data
                               where usuario.UserName == user
                               select c.Data).FirstOrDefault();

            DateTime atual = DateTime.Today;

            if (primeiroDia < dias)
            {
                total = Convert.ToInt32((atual.Subtract(primeiroDia)).TotalDays);
            }
            else
            {
                total = Convert.ToInt32((atual.Subtract(dias)).TotalDays);
            }

            for (int i = 1; i <= total; i++)
            {
                if (primeiroDia < dias)
                {
                    datax = primeiroDia.AddDays(i);
                }
                else
                {
                    datax = dias.AddDays(i);
                }


                diaEspecificado = (from c in _context.Dias
                                   join usuario in _context.Users
                                   on c.IdentityUserId equals usuario.Id
                                   where usuario.UserName == user &&
                                   c.Data == datax
                                   select c.Data).FirstOrDefault();

                if (diaEspecificado.ToString("dd-MM-yyyy") != "01-01-0001")
                {
                    idDias = (from c in _context.Dias
                              join usuario in _context.Users
                              on c.IdentityUserId equals usuario.Id
                              where usuario.UserName == user
                              && c.Data == diaEspecificado
                              select c.Id).FirstOrDefault();
                }

                var pesoEspecificado = (from c in _context.Dias
                                        join usuario in _context.Users
                                        on c.IdentityUserId equals usuario.Id
                                        where usuario.UserName == user &&
                                        c.Data == datax
                                        select c.Peso).FirstOrDefault();
                if (pesoEspecificado != 0)
                {
                    pd.Add(new PesoDias
                    {
                        Id = idDias,
                        IdentityUserId = userId,
                        Data = diaEspecificado,
                        Peso = pesoEspecificado
                    });
                }
            }
            var orderDateId = (from dates in pd
                               orderby dates.Data
                               select dates.Id).ToList();
            var orderDateData = (from dates in pd
                                 orderby dates.Data
                                 select dates.Data).ToList();
            var orderDatePeso = (from dates in pd
                                 orderby dates.Data
                                 select dates.Peso).ToList();
            var orderDate = (from dates in pd
                             orderby dates.Data
                             select dates).ToList();

            var orderDateDataFirst = (from dates in pd
                                      orderby dates.Data
                                      select dates.Data).FirstOrDefault();
            var orderDatePesoFirst = (from dates in pd
                                      orderby dates.Data
                                      select dates.Peso).FirstOrDefault();
            anterior = orderDatePesoFirst;

            foreach (var diasPorcentual in orderDate)
            {
                if (diasPorcentual.Data != orderDateDataFirst)
                {
                    porcentual = (100 * diasPorcentual.Peso) / anterior;
                    dif = 100 - porcentual;
                    porcentagem.Add(dif);
                }
            }

            ViewBag.Datas = orderDateData;
            ViewBag.Pesos = orderDatePeso;
            ViewBag.Porcentagem = porcentagem;
            ViewBag.Id = orderDateId;
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

        private bool PesoDiasExists(int id)
        {
            return _context.Dias.Any(e => e.Id == id);
        }
    }
}