using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TournamentPage.Models
{
    public class RegisterType
    {
        public RegisterType(){}

        public RegisterType(string RegisterTypeName)
        {
            this.RegisterTypeName = RegisterTypeName;
        }

        [Key]
        public int RegisterTypeId {get;set;}

        public string RegisterTypeName {get;set;}
    }
}