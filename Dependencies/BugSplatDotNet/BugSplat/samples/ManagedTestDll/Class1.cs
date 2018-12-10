using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManagedTestDll
{
  public class Test
  {
    public static unsafe void MemoryException()
    {
        MemoryException1();
    }

    public static unsafe void MemoryException1()
    {
        MemoryException2();

    }

    public static unsafe void MemoryException2()
    {
        MemoryException3();

    }

    public static unsafe void MemoryException3()
    {
        byte* p = (byte*)0;
        *p = 0;
    }
  }
}
