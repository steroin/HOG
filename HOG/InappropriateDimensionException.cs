using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HOG
{
    class InappropriateDimensionException : Exception
    {
        public InappropriateDimensionException(string message) : base(message) { }
    }
}
