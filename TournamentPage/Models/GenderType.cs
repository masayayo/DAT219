using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TournamentPage.Models
{
    public class GenderType
    {
        public GenderType(){}

        public GenderType(string GenderTypeName)
        {
            this.GenderTypeName = GenderTypeName;
        }

        [Key]
        public int GenderTypeId {get;set;}

        public string GenderTypeName {get;set;}
    }
}