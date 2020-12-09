using Microsoft.AspNetCore.Identity;
using System;

namespace PesoXMeta.Models
{
    public class Controle
    {
        public int Id { get; set; }
        public double Peso { get; set; }
        public double Altura { get; set; }
        public double Meta { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataMeta { get; set; }
        public IdentityUser IdentityUser { get; set; }
        public string IdentityUserID { get; set; }

        public Controle()
        {

        }
    }
}
