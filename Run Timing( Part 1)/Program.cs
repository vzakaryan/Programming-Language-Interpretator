
namespace Interpritator
{
    class Program
    {
        static void Main(string[] args)
        {
            string t = "ArloopaScript  a,c:int; void update()  i:int; { i=3; if i < 4  { i=--7; write(i) } else ; } { write(\" update \"); update() }";
            Interpretator inter = new Interpretator(t + "@");
            inter.interpretation();
        } 
    }
}