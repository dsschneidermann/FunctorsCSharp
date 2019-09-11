using System;
using System.Linq;
using System.Threading.Tasks;
using Utf8Json;

namespace functors
{
    public static class Applicatives
    {
        public static async Task Run()
        {
            // Add two numbers and make a string
            var AddStr = FuncA.Create((int x, int y) => (x + y).ToString());

            // Two numbers with curry
            var Add = FuncA.Create((int x, int y) => (x + y));
            var Multiply = FuncA.Create((int x, int y) => (x * y));

            // String concat
            var StrCat = FuncA.Create((string x, string y) => $"{x}{y}");

            // Create a ListA with some FuncAs in it
            var add_1_then_2_then_3 = 
                ListA.Create(new[] { AddStr.Invoke(1), AddStr.Invoke(2), AddStr.Invoke(3) });

            var a = new ListA<int> { 1, 2, 3 };
            var res10_list = ((IApplicative<ListA<int>, ListA<object>, int>)a);

            // ApplyFrom is using reverse parameter order compared to Haskell's <*>
            // This is so we can restrict the type on the input parameter to need to be FuncA<>
            var res10_appliedFrom = res10_list
                .ApplyFromImpl<ListA<FuncA<int, string>>, FuncA<int, string>, ListA<string>, string>(
                    add_1_then_2_then_3
                );
            var res10 = res10_appliedFrom as ListA<string>;

            // <*> ApplyTo as an extension method is equivalent to Haskell <*>
            var res11 = add_1_then_2_then_3
                    .AppliedTo<int, string>(a);

            // AppliedTo with multiple lists and currying functions
            var b = new ListA<int> { 1, 2 };
            var c = new ListA<int> { 3, 4 };

            var add_mult = ListA.Create(Add.Invoke(1), Add.Invoke(2), Multiply.Invoke(1), Multiply.Invoke(2));
            var res12 = add_mult
                .AppliedTo<int, int>(c);

            // We can now do [(+),(*)] <*> [1,2] <*> [3,4]
            var add_mult_two_lists = ListA.Create(Add, Multiply);
            var res13 = add_mult_two_lists
                .AppliedTo<int, FuncA<int,int>>(b).Show(select: x => x.Count())
                .AppliedTo<int, int>(c);

            // // Don't even need the type parameters!
            // var res14 = add_mult_two_lists
            //     .AppliedTo(a)
            //     .AppliedTo(b);

            Console.WriteLine(JsonSerializer.PrettyPrint(JsonSerializer.Serialize(
                new
                {
                    res10 = await res10.Unbox(),
                    res11 = await res11.Unbox(),
                    res12 = await res12.Unbox(),
                    res13 = await res13.Unbox(),
                    // res14 = await res14.Unbox(),
                }
            )));
        }
    }
}
