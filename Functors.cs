using System;
using System.Linq;
using System.Threading.Tasks;
using Utf8Json;

namespace functors
{
    public static class Functors
    {
        public static async Task Run()
        {
            // Create a functor Lazy<int> containing a 5.
            var a = new LazyF<int>(() => 5);

            // Example for direct use of IFunctor interface.
            var res1_functor = ((IFunctor<LazyF<int>, int>)a);
            var res1_exec = res1_functor.FmapImpl<LazyF<string>, string>(
                x => (x.Value + 2).ToString());
            var res1 = ((LazyF<string>)res1_exec);

            // Instead of x.Value as above, we can call Unbox on the input to the function.
            var res2 = a.Fmap<int, string>(x => (x + 3).ToString());

            // Type argument are inferred now that we have simplified it
            var res3 = a.Fmap(x => (x + 4).ToString());

            // There is also support for chaining!
            var res4 = a.Fmap(x => x + 5).Fmap(x => x.ToString());

            Console.WriteLine(JsonSerializer.PrettyPrint(JsonSerializer.Serialize(
                new
                {
                    res1 = await res1.Unbox(),
                    res2 = await res2.Unbox(),
                    res3 = await res3.Unbox(),
                    res4 = await res4.Unbox()
                }
            )));

            // // Let's spice it up and use a Task
            var b = new LazyTaskF<int>(() => Task.FromResult(5));
            var res5 = b.Fmap(x => x + 6).Fmap(x => x.ToString());
            // Unsurprising, it behaves exactly like Lazy from before..

            // // Lets actually do something then.
            var requestUri = "https://www.dennis-s.dk";
            var res6 = LazyTaskF.Create(
                () => new System.Net.Http.HttpClient().GetStreamAsync(requestUri))
                .Fmap(x => WriteToFileAsync("index.html", x))
                .Fmap(x => x.Close())
                .Fmap(_ => System.IO.File.ReadAllTextAsync("index.html"))
                .Fmap(x => x.ToCharArray().Count());

            // Not impressive huh.. Looks a lot like we could just use a few "await" instead of all those Fmaps.
            // Lets try a sequence.
            var c = new[] { 1, 2, 3 };
            var res7 = ListA.Create(c)
                .Fmap(x => x + 1)
                .Fmap(x => x.ToString());
            // As expected, it looks exactly like Linq.

            // We can use it to do multiple things, but again, not very interesting
            var requestUris = new[] { "https://www.dennis-s.dk", "https://www.dennis-s.dk/404" };
            var res8 = ListA.Create(requestUris)
                .Fmap(
                    url => LazyTaskF.Create<System.IO.Stream>(
                    () => new System.Net.Http.HttpClient().GetStreamAsync(url))
                    .Fmap(x => WriteToFileAsync("index.html", x))
                    .Fmap(x => x.Close())
                    .Fmap(_ => System.IO.File.ReadAllTextAsync("index.html"))
                    .Fmap(x => x.ToCharArray().Count())
                );

            Console.WriteLine(JsonSerializer.PrettyPrint(JsonSerializer.Serialize(
                new
                {
                    res5 = await res5.Unbox(),
                    res6 = await res6.Unbox(),
                    res7 = await res7.Unbox(),
                    res8 = await res8.Unbox()
                }
            )));

            // Helper method to return a written filestream
            async Task<System.IO.FileStream> WriteToFileAsync(string filename, System.IO.Stream x)
            {
                var file = new System.IO.FileStream(filename, System.IO.FileMode.Create);
                await x.CopyToAsync(file);
                return file;
            }
        }
    }
}
