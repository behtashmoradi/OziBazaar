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
    
    public partial class Property
    {
        public int PropertyID { get; set; }
        public string KeyName { get; set; }
        public string Title { get; set; }
        public string ControlType { get; set; }
        public string DataType { get; set; }
        public string LookupType { get; set; }
        public string DependsOn { get; set; }
        public byte[] Version { get; set; }
    }
}
