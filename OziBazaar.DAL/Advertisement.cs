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
    
    public partial class Advertisement
    {
        public Advertisement()
        {
            this.WishLists = new HashSet<WishList>();
        }
    
        public int AdvertisementID { get; set; }
        public int ProductID { get; set; }
        public string Price { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public int OwnerID { get; set; }
        public byte[] Version { get; set; }
        public string Title { get; set; }
        public Nullable<bool> IsActive { get; set; }
    
        public virtual Product Product { get; set; }
        public virtual ICollection<WishList> WishLists { get; set; }
    }
}