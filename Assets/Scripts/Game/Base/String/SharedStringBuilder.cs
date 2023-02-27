namespace GameFrame
{
    using System;
    using System.Text;

    public static class SharedStringBuilder
    {
        [ThreadStatic]
        private static StringBuilder s_instance = new StringBuilder();
        private static object s_lock = new object();

        public static StringBuilder Instance
        {
            get 
            {
                if(s_instance == null)
                {
                    object obj2 = s_lock;
                    lock(obj2)
                    {
                        if(s_instance == null)
                        {
                            s_instance = new StringBuilder();
                        }
                    }
                }
                s_instance.Length = 0;
                return s_instance;
            }
        }
    }
}
