using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TournamentPage.Models
{
    public class TournamentType
    {
        public TournamentType(){}

        public TournamentType(string TournamentTypeName)
        {
            this.TournamentTypeName = TournamentTypeName;
        }

        [Key]
        public int TournamentTypeId {get;set;}

        public string TournamentTypeName {get;set;}
        
    }
}