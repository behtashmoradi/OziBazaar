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
    
    public partial class NotificationType
    {
        public NotificationType()
        {
            this.NotificationTemplates = new HashSet<NotificationTemplate>();
        }
    
        public int NotificationTypeID { get; set; }
        public string Description { get; set; }
        public byte[] Version { get; set; }
    
        public virtual ICollection<NotificationTemplate> NotificationTemplates { get; set; }
    }
}