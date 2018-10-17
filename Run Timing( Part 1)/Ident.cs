
namespace Interpritator
{
    /// <summary>
    /// Create an Identificator.
    /// </summary>
    class Ident
    {
        string name=""; // variable name
        bool declare; // is variable declared
        type_of_lex type; // variable type from type_of_lex
        bool assign; // is varible assigned
        object[] value = new object[1]; // the value of variable
        int tid = -1; // saving procedure TID index. If tid = -1 then it's main TID.

        public int get_TID()
        {
            return tid;
        }
        public void set_TID(int new_TID)
        {
            tid = new_TID;
        }

        public Ident()
        {
            declare = false;
            assign = false;
        }

        public void set_size(int size)
        {
            value = new object[size];

            for (int i = 0; i < value.Length; i++)
            {
                value[i] = null;
            }
        }
        public int get_size()
        {
            return value.Length;
        }


        public string get_name() { return name; }

        public void put_name(string n)
        {
            name = n;
        }

        public bool get_declare() { return declare; }
        public void put_declare() { declare = true; }
        public type_of_lex get_type() { return type; }
        public void put_type(type_of_lex t) { type = t; }
        public bool get_assign() { return assign; }
        public void put_assign() { assign = true; }
        public object get_value(int index = 0) { return value[index]; }
        public void put_value(object v, int index = 0) {value[index] = v; }
    }
}
