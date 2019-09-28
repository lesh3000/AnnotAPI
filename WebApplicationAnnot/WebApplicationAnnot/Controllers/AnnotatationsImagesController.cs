using System;
using System.Collections.Generic;
using System.Web.Http;
using GdPicture14;
using GdPicture14.Annotations;
using Newtonsoft.Json;
using WebApplicationAnnot.Models;

namespace WebApplicationAnnot.Controllers
{


    public class WebApplicationAnnotController : ApiController
    {

        
        [HttpPost]
        [Route("api/WebApplicationAnnotController/GetImages")]
        public string GetImages(Params par)
        {

            string path = System.Web.HttpContext.Current.Server.MapPath(par.path);
            List<AnnotationJSON> res = new List<AnnotationJSON>();
            GdPicturePDF pdf = new GdPicturePDF();
            AnnotationManager an = new AnnotationManager();
            GdPictureImaging im = new GdPictureImaging();
            int width = 0;
            int height = 0;
            int left = 0;
            int top = 0;
            pdf.SetOrigin(PdfOrigin.PdfOriginTopLeft);
            pdf.SetMeasurementUnit(PdfMeasurementUnit.PdfMeasurementUnitInch);


            //this API is only works for PDF files , if you need to process image based files you could implement similar logic using GdPictureImaging 
            //instead of GdPicturePDF
            pdf.LoadFromFile(path, false);
            an.InitFromGdPicturePDF(pdf);
                for (int i = 1; i <= pdf.GetPageCount(); i++)
                {
                    an.SelectPage(i);
                    int count = an.GetAnnotationCount();
                    if (count > 0)
                    {
                        List<Annotation> ans = new List<Annotation>();
                        for (int m = 0; m < count; m++)
                        {
                            Annotation annot = an.GetAnnotationFromIdx(m);
                            annot.Rotation = 0;
                            ans.Add(annot);
                        }
                    an.BurnAnnotationsToPage(false);
                        //we render PDF page to an image to crop
                            int imageNr = pdf.RenderPageToGdPictureImage(300, false);
                            for (int y = 0; y < count; y++)
                            {
                            //we copy the same image so we can crop it and send back 
                                int imageCrop = im.CreateClonedGdPictureImage(imageNr);

                                Annotation annot = ans[y];
                            //conversion from inches to pixels
                                left = (int)Math.Ceiling(annot.Left * 300 + 0.5);
                                top = (int)Math.Ceiling(annot.Top * 300 + 0.5);
                                width = (int)Math.Ceiling(annot.Width * 300 + 0.5);
                                height = (int)Math.Ceiling(annot.Height * 300 + 0.5);
                            //cropping the immage to get snapshot of the annotation
                                im.Crop(imageCrop, left - width / 2, top - height / 2, width, height);
                            //rescale the image to 20%
                                im.Scale(imageCrop, 20, System.Drawing.Drawing2D.InterpolationMode.High);
                            //prepare image and send it as base64
                                byte[] arr = { };
                                int length = 0;
                                im.SaveAsByteArray(imageCrop, ref arr, ref length, GdPicture14.DocumentFormat.DocumentFormatJPEG, 100);
                                string bs = System.Convert.ToBase64String(arr);
                                AnnotationJSON json = new AnnotationJSON();
                                json.Image = bs;
                                res.Add(json);
                            //releasing the resources
                                im.ReleaseGdPictureImage(imageCrop);

                            }
                            im.ReleaseGdPictureImage(imageNr);
                        }



                    }
                    pdf.Dispose();

                    return JsonConvert.SerializeObject(res);
               

        }
            
        }
    }


