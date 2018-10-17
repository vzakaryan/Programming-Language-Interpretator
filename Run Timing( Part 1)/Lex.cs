
namespace Interpritator
{
    /// <summary>
    /// Create Lexem for Parser and Executer.
    /// </summary>
    class Lex
    {
        type_of_lex t_lex; // type of lexem
        object v_lex; // value of lexem
        int index; // index of values array 
        int tid_index; // index of Table Ident array 

        public Lex(type_of_lex t = type_of_lex.LEX_NULL, object v = null, int i = 0)
        {
            
            t_lex = t;
            v_lex = v;
            index = i;
            tid_index = Globals.curr_TID;
        }

        public type_of_lex get_type()
        {
            return t_lex;
        }

        public object get_value()
        {
            return v_lex;
        }

        public int get_index()
        {
            return index;
        }

        public int get_tid_index()
        {
            return tid_index;
        }
        public override string ToString()
        {
            return ("(" + t_lex + "," + v_lex + ");");
        }

    }
}
