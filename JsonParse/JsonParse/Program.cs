using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonParse
{
    class Program
    {
        static void Main(string[] args)
        {
             List<string> women = new List<string>()
             {

                 "unhappy","dirty","rude"

             };



            Regex rgx = new Regex(string.Join("|", women.ToArray()));





            MatchCollection mCol = rgx.Matches("I had a very unhappy experience; the waiter was rude; I was unhappy");

            foreach (Match m in mCol)
            {

                //Displays 'cat' and 'tooth' - instead of 'caterpillar' and 'tooth'

    //            Console.WriteLine(m);

            }

            int counter = 0;
            string line;

            // Read the file and display it line by line.
            System.IO.StreamReader file = new System.IO.StreamReader("c:\\temp\\yelp_academic_dataset_user.json");
            System.IO.StreamWriter outFile =  new System.IO.StreamWriter("c:\\temp\\user.txt");
  
            while ((line = file.ReadLine()) != null)
            {
  //              Console.WriteLine(line);
                dynamic js = JsonConvert.DeserializeObject(line);
                foreach (var item in js)
                {
                    if ((item.Name == "review_count"))
                        outFile.Write("{0}\t", item.Value); // last item
                    else
                    if ((item.Name != "votes") && (item.Name != "type"))
 //                       Console.WriteLine("{0}\n {1}", item.Name, item.Value);
 //                         Console.Write("{0}\t",item.Value);
                        outFile.Write("{0}\t", item.Value);
                }
 //               Console.WriteLine();
                outFile.WriteLine();
                counter++;
            }
            System.IO.StreamReader file2 = new System.IO.StreamReader("c:\\temp\\yelp_academic_dataset_business.JSON");
            System.IO.StreamWriter outFile2 = new System.IO.StreamWriter("c:\\temp\\businessx.txt");

            while ((line = file2.ReadLine()) != null)
            {
                //              Console.WriteLine(line);
                dynamic js = JsonConvert.DeserializeObject(line);
                foreach (var item in js)
                {
                    if ((item.Name == "latitude"))
                        outFile2.Write("{0}\t", item.Value); // last item
                    else
                    if ((item.Name != "categories") && (item.Name != "type")  && (item.Name != "neighborhoods") )

                        //                       Console.WriteLine("{0}\n {1}", item.Name, item.Value);
                        //                         Console.Write("{0}\t",item.Value);
                        outFile2.Write("{0}\t", item.Value);
                }
                //               Console.WriteLine();
                outFile2.WriteLine();
                counter++;
            }
            System.IO.StreamReader file3 = new System.IO.StreamReader("c:\\temp\\yelp_academic_dataset_review.JSON");
            System.IO.StreamWriter outFile3 = new System.IO.StreamWriter("c:\\temp\\reviewx.txt");

            while ((line = file3.ReadLine()) != null)
            {
                //              Console.WriteLine(line);
                dynamic js = JsonConvert.DeserializeObject(line);
                foreach (var item in js)
                {
                    if ((item.Name == "business_id"))
                        outFile3.Write("{0}", item.Value); //last item
                    else

                    if (((item.Name != "votes") && (item.Name != "type") && (item.Name != "text")))
                        //                       Console.WriteLine("{0}\n {1}", item.Name, item.Value);
                        //                         Console.Write("{0}\t",item.Value);
                        outFile3.Write("{0}\t", item.Value);
                }
                //               Console.WriteLine();
                outFile3.WriteLine();
                counter++;
            }
            file.Close();
            file2.Close();
            file3.Close();
        }
    }
}
