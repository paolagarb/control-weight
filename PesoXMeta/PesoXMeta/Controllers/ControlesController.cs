﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PesoXMeta.Data;
using PesoXMeta.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PesoXMeta.Controllers
{
    public class ControlesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ControlesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Controles
        public async Task<IActionResult> Index()
        {
            return View(await _context.Controle.ToListAsync());
        }

        // GET: Controles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var controle = await _context.Controle
                .FirstOrDefaultAsync(m => m.Id == id);
            if (controle == null)
            {
                return NotFound();
            }

            return View(controle);
        }

        // GET: Controles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Controles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Peso,Altura,Meta,DataInicio,DataMeta")] Controle controle)
        {
            if (ModelState.IsValid)
            {
                _context.Add(controle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(controle);
        }

        // GET: Controles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var controle = await _context.Controle.FindAsync(id);
            if (controle == null)
            {
                return NotFound();
            }
            return View(controle);
        }

        // POST: Controles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Peso,Altura,Meta,DataInicio,DataMeta")] Controle controle)
        {
            if (id != controle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(controle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ControleExists(controle.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(controle);
        }

        // GET: Controles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var controle = await _context.Controle
                .FirstOrDefaultAsync(m => m.Id == id);
            if (controle == null)
            {
                return NotFound();
            }

            return View(controle);
        }

        // POST: Controles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var controle = await _context.Controle.FindAsync(id);
            _context.Controle.Remove(controle);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult SalvarPeso(double altura, double pesoAtual, double pesoMeta, DateTime dataMeta)
        {
            var user = User.Identity.Name;
            var userId = (from id in _context.User
                          where id.UserName == user
                          select id.Id).Single();

            Controle c1 = new Controle();
            c1.IdentityUserID = userId;
            c1.Altura = altura;
            c1.DataInicio = DateTime.Today;
            c1.DataMeta = dataMeta;
            c1.Meta = pesoMeta;
            c1.Peso = pesoAtual;

            _context.Add(c1);
            _context.SaveChanges();
            return RedirectToAction(nameof(Home));
        }

        public IActionResult Home()
        {
            var user = User.Identity.Name;

            var controle = (from control in _context.Controle
                            join usuario in _context.User
                            on control.IdentityUserID equals usuario.Id
                            where usuario.UserName == user
                            select control.Id).FirstOrDefault();
            ViewBag.Conteudo = controle;

            ViewBag.PesoInicial = (from control in _context.Controle
                                   join usuario in _context.User
                                   on control.IdentityUserID equals usuario.Id
                                   where usuario.UserName == user
                                   select control.Peso).FirstOrDefault();
            ViewBag.Meta = (from control in _context.Controle
                            join usuario in _context.User
                            on control.IdentityUserID equals usuario.Id
                            where usuario.UserName == user
                            select control.Meta).FirstOrDefault();
            ViewBag.Inicio = (from control in _context.Controle
                              join usuario in _context.User
                              on control.IdentityUserID equals usuario.Id
                              where usuario.UserName == user
                              select control.DataInicio.Date).FirstOrDefault();
            var meta = (from control in _context.Controle
                                join usuario in _context.User
                                on control.IdentityUserID equals usuario.Id
                                where usuario.UserName == user
                                select control.DataMeta.Date).FirstOrDefault();
            ViewBag.DataMeta = meta;

            ViewBag.DiasRestantes = meta.Day - DateTime.Today.Day;

            return View();

        }

        private bool ControleExists(int id)
        {
            return _context.Controle.Any(e => e.Id == id);
        }
    }
}
