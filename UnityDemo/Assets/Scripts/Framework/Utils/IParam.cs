using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geek.Client
{

    public interface IParam { }

    public class OneParam<T> : IParam
    {
        public T value;
        public OneParam(T t)
        {
            value = t;
        }
        public OneParam()
        {

        }

    }

    public class TwoParam<T1, T2> : IParam
    {
        public T1 value1;
        public T2 value2;
        public TwoParam(T1 t1, T2 t2)
        {
            value1 = t1;
            value2 = t2;
        }

        public TwoParam()
        {

        }

    }

    public class ThreeParam<T1, T2, T3> : IParam
    {
        public T1 value1;
        public T2 value2;
        public T3 value3;
        public ThreeParam(T1 t1, T2 t2, T3 t3)
        {
            value1 = t1;
            value2 = t2;
            value3 = t3;
        }

        public ThreeParam()
        {

        }

    }

    public class FourParam<T1, T2, T3, T4> : IParam
    {
        public T1 value1;
        public T2 value2;
        public T3 value3;
        public T4 value4;
        public FourParam(T1 t1, T2 t2, T3 t3, T4 t4)
        {
            value1 = t1;
            value2 = t2;
            value3 = t3;
            value4 = t4;
        }

        public FourParam()
        {

        }
    }

    public class FiveParam<T1, T2, T3, T4, T5> : IParam
    {
        public T1 value1;
        public T2 value2;
        public T3 value3;
        public T4 value4;
        public T5 value5;
        public FiveParam(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
        {
            value1 = t1;
            value2 = t2;
            value3 = t3;
            value4 = t4;
            value5 = t5;
        }

        public FiveParam()
        {

        }
    }

}
