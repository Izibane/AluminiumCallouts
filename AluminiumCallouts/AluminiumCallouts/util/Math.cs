using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AluminiumCallouts
{
    public class Math
    {

        public double divideNumber(int a, int b)
        {
            if(a == 0 || b == 0)
            {
                return 0;
            } else
            {
                if(a != 0 && b != 0)
                {
                    return a / b;
                } else
                {
                    return 0;
                }
            }
        }

        public int getRandomInt(int min, int max)
        {
            Random r = new Random();
            int done = r.Next(min, max);
            return done;
        }

    }
}
