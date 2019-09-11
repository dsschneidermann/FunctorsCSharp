using System.Threading.Tasks;

namespace functors
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Functors.Run();
            //await Applicatives.Run();
        }
    }
}
