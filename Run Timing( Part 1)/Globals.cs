using System.Collections;
using System.Collections.Generic;

namespace Interpritator
{
    class Globals
    {
        public static List<Tabl_ident> TID_List = new List<Tabl_ident> { new Tabl_ident() }; // List of Table Idents
        public static int curr_TID = 0; // index of current TID
        public static Queue proc_queue = new Queue(); // Queue for procedure parameters
        public static Queue proc_index = new Queue(); // Queue for procedures TID's index
        public static bool isUpdateFunction = false; // Cheking is Unity Update function called
    }
}
