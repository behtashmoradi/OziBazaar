﻿using OziBazaar.DAL;
using OziBazaar.Web.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OziBazaar.Web.Controllers
{
    public class MediaController : Controller
    {
        private readonly IProductRepository productRepository;
        public MediaController(IProductRepository productRepository)
        {
            this.productRepository = productRepository;
        }
        
        public ActionResult Index(int productId)
        {
            ViewBag.ProductId = productId;
            return View(productRepository.GetProductImages(productId));
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase[] files, int productId)
        {
           // int productId =Int32.Parse(TempData["ProductId"].ToString());
            List<ProductImage> images=new List<ProductImage>();
            foreach (HttpPostedFileBase file in files)
            {
                string path = System.IO.Path.Combine(Server.MapPath("~/Content/Image/"), System.IO.Path.GetFileName(file.FileName));
                file.SaveAs(path);
                images.Add(new ProductImage() { ProductID = productId, MimeType = "image", ImagePath = "/Content/Image/"+ System.IO.Path.GetFileName(file.FileName), Description = "Description", ImageType = "Image", ImageOrder = 1 });

            }
            productRepository.AddAttachment(images);
            return RedirectToAction("ViewProduct", "Product", new { productId =productId});
        }
        public ActionResult ProductImages(int productId)
        {
               return View(productRepository.GetProductImages(productId));
        }
        public ActionResult Delete(int id,int productId)
        {
            productRepository.DeleteImage(id);
            return RedirectToAction("Index", new { productId = productId });

        }
	}
}