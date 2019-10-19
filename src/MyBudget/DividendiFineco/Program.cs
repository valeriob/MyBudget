using System.Linq;
using System.Threading.Tasks;

namespace DividendiFineco
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var elabora = new ElaboraDividendiFineco();
            await elabora.ElaboraDividendi(args.FirstOrDefault());

        }

    }

}
