using System.Collections.Immutable;

using Old = RecordImmutability.Before;
using New = RecordImmutability.After;

Console.WriteLine("BEFORE: record Order(int Id, string Customer, List<string> Items)");

var a = new Old.Order(1, "Asha", ["Keyboard"]);
var copy = a with { Id = 2 };
copy.Items.Add("Mouse");
Console.WriteLine($"  'with' copy shares the list?      original now has {a.Items.Count} items");

var x = new Old.Order(1, "Asha", ["Keyboard"]);
var y = new Old.Order(1, "Asha", ["Keyboard"]);
Console.WriteLine($"  same data, x == y?                {x == y}");

var set = new HashSet<Old.Order> { x };
x.Status = "Paid";
Console.WriteLine($"  in HashSet after Status changed?  {set.Contains(x)}");

Console.WriteLine();
Console.WriteLine("ImmutableArray only, no Equals override");
var p = new New.OrderNoOverride(1, "Asha", ["Keyboard"]);
var q = new New.OrderNoOverride(1, "Asha", ["Keyboard"]);
Console.WriteLine($"  same data, p == q?                {p == q}");

Console.WriteLine();
Console.WriteLine("AFTER: ImmutableArray + init + sequence equality");

var b = new New.Order(1, "Asha", ["Keyboard"]);
var updated = b with { Items = b.Items.Add("Mouse") };
Console.WriteLine($"  original after 'with' + Add:      {b.Items.Length} item, copy has {updated.Items.Length}");

var m = new New.Order(1, "Asha", ["Keyboard"]);
var n = new New.Order(1, "Asha", ["Keyboard"]);
Console.WriteLine($"  same data, m == n?                {m == n}");

var set2 = new HashSet<New.Order> { m };
var paid = m with { Status = "Paid" };
Console.WriteLine($"  original still in HashSet?        {set2.Contains(m)}");
Console.WriteLine($"  paid copy is a different value?   {paid != m}");