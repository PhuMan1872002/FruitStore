using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DepartMentStore.Models
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService()
        {
            Account account = new Account(
                System.Configuration.ConfigurationManager.AppSettings["CloudinaryCloudName"],
                System.Configuration.ConfigurationManager.AppSettings["CloudinaryApiKey"],
                System.Configuration.ConfigurationManager.AppSettings["CloudinaryApiSecret"]
            );

            _cloudinary = new Cloudinary(account);
        }
        public string UploadImage(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
                return null;

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, file.InputStream)
            };

            var uploadResult = _cloudinary.Upload(uploadParams);

            return uploadResult.SecureUrl.AbsoluteUri;
        }

    }
}