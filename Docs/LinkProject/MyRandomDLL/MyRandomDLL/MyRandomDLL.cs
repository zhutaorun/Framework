using System;

namespace MyRandomDLL
{
    public class MyRandomDLL
    {
        public int GetRandom()
        {
            return new Random().Next();
        }
    }
}
