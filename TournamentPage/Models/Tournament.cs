using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TournamentPage.Models
{
    public class Tournament
    {
        public Tournament(){}

        public Tournament(string TournamentName, string FeaturedImage, DateTime RegisterDateStart, DateTime RegisterDateEnd, DateTime TournamentDateStart, 
        DateTime TournamentDateEnd, string Location, int RegisterFee, SportType SportType, TournamentType TournamentType, RegisterType RegisterType,
        GenderType GenderType, string AgeFrom, string AgeTo, bool TermsAccepted, string BracketsJSON, ApplicationUser User)
        {
            this.TournamentName = TournamentName;
            this.FeaturedImage = "/featured-default.jpg";
            this.RegisterDateStart = RegisterDateStart;
            this.RegisterDateEnd = RegisterDateEnd;
            this.TournamentDateStart = TournamentDateStart;
            this.TournamentDateEnd = TournamentDateEnd;
            this.Location = Location;
            this.RegisterFee = RegisterFee;
            this.SportType = SportType;
            this.TournamentType = TournamentType;
            this.RegisterType = RegisterType;
            this.GenderType = GenderType;
            this.AgeFrom = AgeFrom;
            this.AgeTo = AgeTo;
            this.TermsAccepted = TermsAccepted;
            this.BracketsJSON = BracketsJSON;
            this.User = User;
        }

        [Key]
        public int TournamentId {get;set;}

        [Required(ErrorMessage = "Spesifiser turneringsnavnet")]
        [StringLength(40)]
        public string TournamentName {get;set;}

        /*[RegularExpression(@"^([0-9a-zA-Z_\-~ :\\])+(.jpg|.JPG|.jpeg|.JPEG|.png|.PNG)$", ErrorMessage = "Denne filtypen er ikke tillatt")]*/
        public string FeaturedImage {get;set;}

        [Required(ErrorMessage = "Spesifiser når påmeldingen starter")]
        public DateTime RegisterDateStart {get;set;}

        [Required(ErrorMessage = "Spesifiser når påmeldingen slutter")]
        public DateTime RegisterDateEnd {get;set;}

        [Required(ErrorMessage = "Spesifiser når turneringen starter")]
        public DateTime TournamentDateStart {get;set;}

        [Required(ErrorMessage = "Spesifiser når turneringen slutter")]
        public DateTime TournamentDateEnd {get;set;}

        [Required(ErrorMessage = "Spesifiser hvor turneringen skal holdes")]
        public string Location {get;set;}

        [Required(ErrorMessage = "Påmeldingsavgift må settes. Kan være gratis, dvs null.")]
        [Range(0,1000, ErrorMessage = "Påmeldingsavgiften må være mellom 0 og 1000")]
        public int? RegisterFee {get;set;}

        [ForeignKey("SportTypeId")]
        [Required(ErrorMessage = "Spesifiser sportstype")]
        public SportType SportType {get;set;}

        [ForeignKey("TournamentTypeId")]
        [Required(ErrorMessage = "Spesifiser turneringstype")]
        public TournamentType TournamentType {get;set;}

        [ForeignKey("RegisterTypeId")]
        [Required(ErrorMessage = "Spesifiser påmeldingstype")]
        public RegisterType RegisterType {get;set;}

        [ForeignKey("GenderTypeId")]
        [Required(ErrorMessage = "Må velge kjønn")]
        public GenderType GenderType {get;set;}
        
        [Required(ErrorMessage = "Alder fra må velges")]
        public String AgeFrom {get;set;}
        
        [Required(ErrorMessage = "Alder til må velges")]
        public String AgeTo {get;set;}

        [Range(typeof(bool), "true", "true", ErrorMessage="Du må akseptere vilkårene!")]
        public bool TermsAccepted {get; set;}

        public string BracketsJSON {get;set;}

        [ForeignKey("ApplicationUserId")]
        public ApplicationUser User {get;set;}

    }
}