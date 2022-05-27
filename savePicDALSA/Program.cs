using System;
using System.IO;
using DALSA.SaperaLT.SapClassBasic;
using DALSA.SaperaLT.Examples.NET.Utils;

namespace DALSA.SaperaLT.Examples.NET.CSharp.GrabConsole
{
    class GrabConsole
    {

        //static float lastFrameRate = 0.0f;

        static void xfer_XferNotify(object sender, SapXferNotifyEventArgs args)
        {
            SapBufferWithTrash Buffers = args.Context as SapBufferWithTrash;
            //If file already exists create new file name.
            string FileName = "im0"; // This var will be edited to FileName1, FileName2 and so on...
            string BaseFileName = "im"; // To prevent cases like "FileName12345", we 'reset' it by having a "base name". 
            int i = 0;
            while (File.Exists($"{"C:\\Users\\rafae\\Documents\\NET\\savePicDALSA\\savedPics\\"}/{FileName}.bmp"))
            {
                i = i + 1;
                FileName = $"{BaseFileName}{i}";
            }
            // save the File
            bool picSaved = Buffers.Save("C:\\Users\\rafae\\Documents\\NET\\savePicDALSA\\savedPics\\" + FileName + ".bmp", "-format bmp");
            if (picSaved)
            {
                Console.WriteLine("Picture saved.");
            }
            else
            {
                Console.WriteLine("Picture NOT saved.");
            }
        }


        static void Main(string[] args)
        {
            SapAcquisition Acq = null;
            SapAcqDevice AcqDevice = null;
            SapBuffer Buffers = null;
            SapTransfer Xfer = null;
            SapView View = null;

            Console.WriteLine("Initiated comm cam");

            MyAcquisitionParams acqParams = new MyAcquisitionParams();

            // Call GetOptions to determine which acquisition device to use and which config
            // file (CCF) should be loaded to configure it.
            if (!GetOptions(args, acqParams))
            {
                //Console.WriteLine("\nPress any key to terminate\n");
                //Console.ReadKey(true);
                return;
            }

            SapLocation loc = new SapLocation(acqParams.ServerName, acqParams.ResourceIndex);

            if (SapManager.GetResourceCount(acqParams.ServerName, SapManager.ResourceType.Acq) > 0)
            {
                Acq = new SapAcquisition(loc, acqParams.ConfigFileName);
                Buffers = new SapBufferWithTrash(2, Acq, SapBuffer.MemoryType.ScatterGather);
                Xfer = new SapAcqToBuf(Acq, Buffers);

                // Create acquisition object
                if (!Acq.Create())
                {
                    Console.WriteLine("Error during SapAcquisition creation!\n");
                    DestroysObjects(Acq, AcqDevice, Buffers, Xfer, View);
                    return;
                }
                Acq.EnableEvent(SapAcquisition.AcqEventType.StartOfFrame);

            }

            else if (SapManager.GetResourceCount(acqParams.ServerName, SapManager.ResourceType.AcqDevice) > 0)
            {
                AcqDevice = new SapAcqDevice(loc, acqParams.ConfigFileName);
                Buffers = new SapBufferWithTrash(2, AcqDevice, SapBuffer.MemoryType.ScatterGather);
                Xfer = new SapAcqDeviceToBuf(AcqDevice, Buffers);


                // Create acquisition object
                if (!AcqDevice.Create())
                {
                    Console.WriteLine("Error during SapAcqDevice creation!\n");
                    DestroysObjects(Acq, AcqDevice, Buffers, Xfer, View);
                    return;
                }
            }

            //View = new SapView(Buffers);

            // End of frame event
            Xfer.Pairs[0].EventType = SapXferPair.XferEventType.EndOfFrame;
            Xfer.XferNotify += new SapXferNotifyHandler(xfer_XferNotify);
            Xfer.XferNotifyContext = View;
            Xfer.XferNotifyContext = Buffers;

            // Create buffer object
            if (!Buffers.Create())
            {
                Console.WriteLine("Error during SapBuffer creation!\n");
                DestroysObjects(Acq, AcqDevice, Buffers, Xfer, View);
                return;
            }

            // Create buffer object
            if (!Xfer.Create())
            {
                Console.WriteLine("Error during SapTransfer creation!\n");
                DestroysObjects(Acq, AcqDevice, Buffers, Xfer, View);
                return;
            }

            // Create buffer object
            //if (!View.Create())
            //{
            //   Console.WriteLine("Error during SapView creation!\n");
            //   DestroysObjects(Acq, AcqDevice, Buffers, Xfer, View);
            //   return;
            //}

            

            Xfer.Snap();
            Xfer.Wait(2000);
            Console.WriteLine("Snapped");
            //Console.WriteLine("\nPress any key to terminate\n");
            //Console.ReadKey(true);

            DestroysObjects(Acq, AcqDevice, Buffers, Xfer, View);
            loc.Dispose();
        }


        static bool GetOptions(string[] args, MyAcquisitionParams acqParams)
        {
            // Check if arguments were passed
            if (args.Length > 1)
                return ExampleUtils.GetOptionsFromCommandLine(args, acqParams);
            else
                return ExampleUtils.GetOptionsFromQuestions(acqParams);
        }


        static void DestroysObjects(SapAcquisition acq, SapAcqDevice camera, SapBuffer buf, SapTransfer xfer, SapView view)
        {

            if (xfer != null)
            {
                xfer.Destroy();
                xfer.Dispose();
            }

            if (camera != null)
            {
                camera.Destroy();
                camera.Dispose();
            }

            if (acq != null)
            {
                acq.Destroy();
                acq.Dispose();
            }

            if (buf != null)
            {
                buf.Destroy();
                buf.Dispose();
            }

            if (view != null)
            {
                view.Destroy();
                view.Dispose();
            }


        }
    }
}