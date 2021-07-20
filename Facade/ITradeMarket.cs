using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Facade
{
    public static class MyExtension
    {
        public static T Response<T>(this T tname)
        {
            T ret = tname;

            return ret;
        }
        public static T1 RedirectToTheServer<T, T1>(this T1 response, T request)
        {
            T1 newReq = response;
            return newReq;
        }
    }
    //public static class MyClass 
    //{
    //    public static string Move(this string name)
    //    {
    //        return name += "privet";
    //    }
    //}
    //class Asd1
    //{
    //    public string asddas;
    //    public Asd1()
    //    {
    //        asddas.Move();
    //    }

    //}
}
