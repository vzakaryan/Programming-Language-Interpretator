
namespace Interpritator
{
    /// <summary>
    /// The list of Idents.
    /// </summary>
    class Tabl_ident
    {
        Ident[] p = new Ident[150]; // Idents list size
        int top = 1; // the first empty Ident index
        public int param_count = 0; // count of parameters(for procedure)
        public int prev_TID = -1; // the parent TID
        public int pl0, pl1; // temp variables that save the label for POLIZ_GO (POLIZ_FGO)

        public int get_top()
        {
            return top;
        }

        public Ident this[int i]
        {
            get { return p[i]; }
            set { p[i] = value; }
        }
        /// <summary>
        /// Create a new Ident if the name of Ident is not found or return the index of found Ident.
        /// </summary>
        /// <param name="buf"> A Buffer that sum chars in one word.</param>
        /// <returns>Returns a new variable index or an index of found variable.</returns>
        public int put(string buf)
        {
            for (int i = 0; i < p.Length; i++)
            {
                if (p[i] == null)
                    p[i] = new Ident();
                else break;
            }
            int tmp = Globals.curr_TID;
            while (tmp != -1)
            {
                for (int j = 1; j < top; j++)
                    if (buf.Equals(Globals.TID_List[tmp].p[j].get_name()))
                        return j;  
                tmp -= 1;
            }
            p[top].put_name(buf);
            top++;
            return top - 1;
        }

        public void put_size(int size)
        {
            p[top - 1].set_size(size);
        }


    }
}
