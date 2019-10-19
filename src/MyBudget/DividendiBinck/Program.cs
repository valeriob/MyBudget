using System.Linq;
using System.Threading.Tasks;

namespace DividendiBinck
{
    class Program
    {
        static async Task Main(string[] args)
        {

            //var folder = @"C:\Users\Valerio\Downloads\binck\";
            //var folder = @"E:\Skydrive\Documents\Finanze\Investimenti\Dividendi Bink\";
            //var folder = "";
            //if (args.Length > 0)
            //{
            //    folder = args[0];
            //}

            var svc = new ElaboraDividendiBinck();
            await svc.ElaboraDividendi(args.FirstOrDefault());

          

        }

    }

}
