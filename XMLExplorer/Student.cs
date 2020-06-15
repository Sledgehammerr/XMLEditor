using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace WindowsFormsApp3
{
    public class Student
    {
        public string Name;
        public Group Group;
        [XmlArray]
        [XmlArrayItem("SingleMark")]
        public List<byte> Marks;
    }

    public class Group
    {
        [XmlAttribute]
        public int Course;
        [XmlText]
        public int Number;
        [XmlAttribute]
        public string Track;
    }
}
