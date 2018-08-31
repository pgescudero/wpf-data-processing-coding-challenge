using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessingCodingChallenge
{
    public static class ExtensionMethods
    {
        // Returns the object popped from the queue to make room for the added object,
        // or null if capacity has not been reached.
        public static T AddObjectAndGetOverflow<T>(this Queue<T> self, T newObject, int capacity) 
            where T : class
        {
            T oldObject = null;
            
            if (self.Count >= capacity) 
                oldObject = self.Dequeue();

            self.Enqueue(newObject);
            return oldObject;
        }
    }

}
