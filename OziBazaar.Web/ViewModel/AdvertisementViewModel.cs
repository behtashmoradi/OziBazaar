﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OziBazaar.Web.ViewModel
{
    public class AdvertisementViewModel
    {
        public string Title { get; set; }
        public int CategoryId { get; set; }
        public int Category { get; set; }

        public string StartDate { get; set; }
        public string FinishDate { get; set; }
    }
}