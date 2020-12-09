using Microsoft.AspNetCore.Identity;
using System;

namespace PesoXMeta.Models
{
    public class PesoDias
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public double Peso { get; set; }
        public IdentityUser IdentityUser { get; set; }
        public string IdentityUserId { get; set; }

        public PesoDias()
        {

        }
    }
}
