﻿namespace DragDropPhoneApp.Service
{
   

    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Threading.Tasks;
    using System.Windows.Media.Imaging;

    using Build.DataLayer.Model;

    #endregion

   

    public static class DataService
    {
        #region Public Methods and Operators

        public static byte[] ConvertToBytes(this BitmapImage bitmapImage)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                WriteableBitmap btmMap = new WriteableBitmap(bitmapImage.PixelWidth, bitmapImage.PixelHeight);

                btmMap.SaveJpeg(ms, bitmapImage.PixelWidth, bitmapImage.PixelHeight, 0, 100);

                return ms.ToArray();
            }
        }

        public static BitmapImage FetchImage(Photo photo)
        {
            BitmapImage image = null;

            image = new BitmapImage();

            using (var imageStream = LoadImage(photo.ImageSource))
            {
                if (imageStream != null)
                {
                    image.SetSource(imageStream);
                }
            }

            return image;
        }

        public static Photo GetImage(string imgName)
        {
            DateTime start = new DateTime(2010, 1, 1);

            Photo imageData = new Photo { ImageSource = imgName, Title = imgName, TimeStamp = start };

            imageData.Image = FetchImage(imageData);
            return imageData;
        }

        public static async Task<List<Photo>> GetImages()
        {
            List<Photo> imageList = new List<Photo>();
            DateTime start = new DateTime(2010, 1, 1);
            foreach (var imgName in GetImagesNamesList(true))
            {
                Photo imageData = new Photo { ImageSource = imgName, Title = imgName, TimeStamp = start };
                imageData.Image = FetchImage(imageData);
                imageList.Add(imageData);
            }

            return imageList;
        }

        public static List<string> GetImagesNamesList(bool withPath)
        {
            IsolatedStorageFile storeFile = IsolatedStorageFile.GetUserStoreForApplication();
            string imageFolder = Consts.ImageFolder;
            if (!storeFile.DirectoryExists(Consts.ImageFolderSlash))
            {
                storeFile.CreateDirectory(Consts.ImageFolderSlash);
            }

            List<string> fileList = new List<string>(storeFile.GetFileNames(Consts.ImageFolderSlash));
            List<string> imgNameList = new List<string>();
            if (withPath)
            {
                foreach (string file in fileList)
                {
                    imgNameList.Add(Path.Combine(imageFolder, file));
                }
            }
            else
            {
                return fileList;
            }

            return imgNameList;
        }

        #endregion

        #region Methods

        private static Stream LoadImage(string filename)
        {
            if (filename == null)
            {
                throw new ArgumentException("one of parameters is null");
            }

            Stream stream = null;

            using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isoStore.FileExists(filename))
                {
                    stream = isoStore.OpenFile(filename, FileMode.Open, FileAccess.Read);
                }
            }

            return stream;
        }

        #endregion
    }
}