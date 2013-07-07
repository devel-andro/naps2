﻿/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2013  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using NAPS2.Scan;

namespace NAPS2
{
    public class ImageSaver
    {
        private readonly ImageFileNamer imageFileNamer;

        public ImageSaver(ImageFileNamer imageFileNamer)
        {
            this.imageFileNamer = imageFileNamer;
        }

        public void SaveImages(string fileName, ICollection<IScannedImage> images)
        {
            ImageFormat format = GetImageFormat(fileName);

            int i = 0;

            if (images.Count == 1)
            {
                using (Bitmap baseImage = images.First().GetImage())
                {
                    baseImage.Save(fileName, format);
                }
                return;
            }

            if (format == ImageFormat.Tiff)
            {
                Image[] bitmaps = images.Select(x => x.GetImage()).ToArray();
                TiffHelper.SaveMultipage(bitmaps, fileName);
                foreach (Image bitmap in bitmaps)
                {
                    bitmap.Dispose();
                }
                return;
            }

            var fileNames = imageFileNamer.GetFileNames(fileName, images.Count).GetEnumerator();
            foreach (ScannedImage img in images)
            {
                using (Bitmap baseImage = img.GetImage())
                {
                    fileNames.MoveNext();
                    baseImage.Save(fileNames.Current, format);
                }
                i++;
            }
        }

        private static ImageFormat GetImageFormat(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            Debug.Assert(extension != null);
            switch (extension.ToLower())
            {
                case ".bmp":
                    return ImageFormat.Bmp;
                case ".emf":
                    return ImageFormat.Emf;
                case ".gif":
                    return ImageFormat.Gif;
                case ".ico":
                    return ImageFormat.Icon;
                case ".jpg":
                case ".jpeg":
                    return ImageFormat.Jpeg;
                case ".png":
                    return ImageFormat.Png;
                case ".tif":
                case ".tiff":
                    return ImageFormat.Tiff;
                case ".wmf":
                    return ImageFormat.Wmf;
                default:
                    return ImageFormat.Jpeg;
            }
        }
    }
}
