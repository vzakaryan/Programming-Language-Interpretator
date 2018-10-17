using System;

namespace Interpritator
{
    /// <summary>
    /// Array that contains lexemas to execute.
    /// </summary>
    class Poliz
    {
        Lex[] p; // array of Lexemas
        int size, // Poliz max size, which assigning in constructor
            free; // first empty slot of 'p'

        public Poliz(int max_size)
        {
            p = new Lex[size = max_size];

            if(p[0]==null)
            for (int i = 0; i < p.Length; i++)
            {
                    p[i] = new Lex();
            }
        }

        public void put_lex(Lex l)
        {
            p[free] = l;
            free++;
        }

        public void put_lex(Lex l, object place)
        {
            p[(int)place] = l;
        }

        public void blank()
        {
            free++;
        }

        public int get_free()
        {
            return free;
        }

        public Lex this[int i]
        {
            get
            {
                if (i > size)
                    throw new Exception(" POLIZ: out of array");
                else if (i > free)
                    throw new Exception(" POLIZ: indefinite element of array");
                else
                    return p[i];
            }
        }

    }
}
