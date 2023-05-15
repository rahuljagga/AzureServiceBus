using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POCPubSub
{
// Write Code to generate Person Class using RahulJ repo
    class Person
    {
        public string ID { get; set;}
        public string name { get; set; }
        public int age { get; set; }

        public Person()
        {
            ID = Guid.NewGuid().ToString();
            name = "Test";
            age = 28;

        }

    }
}
