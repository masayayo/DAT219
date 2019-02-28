using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TournamentPage.Models
{
    public class SportType
    {
        public SportType(){}

        public SportType(string SportTypeName)
        {
            this.SportTypeName = SportTypeName;
        }

        [Key]
        public int SportTypeId {get;set;}
        
        public string SportTypeName {get;set;}

    }
}