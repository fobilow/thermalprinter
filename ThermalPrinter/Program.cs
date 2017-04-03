using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptPrinter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string receiptData = args[0];
                Console.WriteLine(receiptData);

                BookingReceipt r = JsonConvert.DeserializeObject<BookingReceipt>(receiptData);

                Console.WriteLine(r.servedBy);
                Print(PrinterName, GetDocument(r));
            }
            else
            {
                Console.WriteLine("you must pass receipt data as json string");
                Environment.Exit(1);
            }
        }

        public static string PrinterName
        {
            get { return @"\\"+Environment.MachineName+"\\POS58"; }
        }

        private static void Print(string printerName, byte[] document)
        {

            NativeMethods.DOC_INFO_1 documentInfo;
            IntPtr printerHandle;

            documentInfo = new NativeMethods.DOC_INFO_1();
            documentInfo.pDataType = "RAW";
            documentInfo.pDocName = "Receipt";

            printerHandle = new IntPtr(0);

            if (NativeMethods.OpenPrinter(printerName.Normalize(), out printerHandle, IntPtr.Zero))
            {
                if (NativeMethods.StartDocPrinter(printerHandle, 1, documentInfo))
                {
                    int bytesWritten;
                    byte[] managedData;
                    IntPtr unmanagedData;

                    managedData = document;
                    unmanagedData = Marshal.AllocCoTaskMem(managedData.Length);
                    Marshal.Copy(managedData, 0, unmanagedData, managedData.Length);

                    if (NativeMethods.StartPagePrinter(printerHandle))
                    {
                        NativeMethods.WritePrinter(
                            printerHandle,
                            unmanagedData,
                            managedData.Length,
                            out bytesWritten);
                        NativeMethods.EndPagePrinter(printerHandle);
                    }
                    else
                    {
                        throw new Win32Exception();
                    }

                    Marshal.FreeCoTaskMem(unmanagedData);

                    NativeMethods.EndDocPrinter(printerHandle);
                }
                else
                {
                    throw new Win32Exception();
                }

                NativeMethods.ClosePrinter(printerHandle);
            }
            else
            {
                throw new Win32Exception();
            }

        }

        private static byte[] GetDocument(BookingReceipt r)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                // Reset the printer bws (NV images are not cleared)
                bw.Write(AsciiControlChars.Escape);
                bw.Write('@');

                // Render the logo
                //RenderLogo(bw);
                PrintReceipt(r, bw);

                // Feed 3 vertical motion units and cut the paper with a 1 point cut
                bw.Write(AsciiControlChars.GroupSeparator);
                bw.Write('V');
                bw.Write((byte)66);
                bw.Write((byte)3);

                bw.Flush();

                return ms.ToArray();
            }
        }

        /// <summary>
        /// This is the method we print the receipt the way we want. Note the spaces. 
        /// Wasted a lot of paper on this to get it right.
        /// </summary>
        /// <param name="bw"></param>
        private static void PrintReceipt(BookingReceipt r, BinaryWriter bw)
        {
            bw.Start();
            bw.NormalFont("Served By: "+r.servedBy);

            bw.FeedLines(1);
            bw.NormalFont("..............................");
            bw.FeedLines(1);
        
            bw.NormalFont("Date: " + r.date);
            bw.NormalFont("Invoice #: " + r.invoiceNumber);
            bw.NormalFont("Customer: " + r.customerName);
            bw.FeedLines(1);

            bw.NormalFont(r.booking+" "+r.amountDue);
            bw.NormalFont("Discount:              "+r.discount);

            bw.FeedLines(2);
            bw.High("    Total:         "+r.totalDue);

            bw.FeedLines(2);
            bw.NormalFont("Payment:              "+r.amountPaid);
            bw.NormalFont("Balance:              "+r.balance);

            bw.Finish();
        }

        private static void PrintBarReceipt(BinaryWriter bw)
        {

            bw.Start();

            bw.NormalFont("Served By: " + "Oke Ugwu");

            bw.FeedLines(1);
            bw.NormalFont(".................................");
            bw.FeedLines(1);

            bw.NormalFont("Date: " + "04/03/2017");
            bw.NormalFont("Invoice #: " + "00011");
            bw.FeedLines(1);

            bw.NormalFont("Itm     Qty     Price    Tot");
            bw.NormalFont("-----------------------------");
            /*foreach (var item in _mappedInvoice.InvoiceItems)
            {*/
                
                bw.NormalFont("Star     5     N1,000     N5,000");
                bw.NormalFont("kronenbourg     5     N1,000     N5,000");

            /*}*/

            bw.NormalFont("Exec Room - 5 nights    N50,000");
            bw.NormalFont("Discount:                N5,000");

            bw.High("Total:                   N45,000");

            bw.FeedLines(2);
            bw.NormalFont("  Payment:  " + "N35,000");
            bw.NormalFont("  Balance:  " + "N10,000");

           
            bw.Finish();
        }
    }


}
