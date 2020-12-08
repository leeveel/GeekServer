/****************************************************************************
Copyright (c) Geek

https://github.com/leeveel/GeekServer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/

namespace Base
{
    public abstract class Param
    {
    }

    public class OneParam<T> : Param
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

    public class TwoParam<T1, T2> : Param
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

    public class ThreeParam<T1, T2, T3> : Param
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

    public class FourParam<T1, T2, T3, T4> : Param
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

    public class FiveParam<T1, T2, T3, T4, T5> : Param
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

