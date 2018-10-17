
namespace Interpritator
{
    class Interpretator
    {
        Parser pars;
        Executer E = new Executer();
        public Interpretator(string program)
        {
            pars = new Parser(program);
        }

        public void interpretation()
        {
            pars.analyze();
            E.execute(pars.prog);
        }
    }
}
