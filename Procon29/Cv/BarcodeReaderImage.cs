﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using OpenCvSharp;

namespace Procon29.Cv
{
    class BarcodeReaderImage : BarcodeReader<Mat>
    {

        /// <summary>

        /// define a custom function for creation of a luminance source with our specialized Mat-supporting class

        /// </summary>

        private static readonly Func<Mat, LuminanceSource> defaultCreateLuminanceSource =

           (image) => new ImageLuminanceSource(image);



        /// <summary>

        /// constructor which uses a custom luminance source with Mat support

        /// </summary>

        public BarcodeReaderImage() : base(null, defaultCreateLuminanceSource, null)

        {

        }
    }
}
