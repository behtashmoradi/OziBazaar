//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OziBazaar.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserProfile
    {
        public UserProfile()
        {
            this.webpages_Roles = new HashSet<webpages_Roles>();
        }
    
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public Nullable<bool> Activated { get; set; }
        public Nullable<System.DateTime> ActivationDate { get; set; }
        public byte[] Version { get; set; }
        public Nullable<int> CountryID { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string PostCode { get; set; }
    
        public virtual ICollection<webpages_Roles> webpages_Roles { get; set; }
        public virtual Country Country { get; set; }
    }
}
