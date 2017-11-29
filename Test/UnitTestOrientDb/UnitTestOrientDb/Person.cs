using System;
using RevStackCore.Pattern;


namespace UnitTestOrientDb
{
    public class Person : IEntity<string>
    {
        public string Id { get; set; }

        public int Age { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        #region "orientdb meta"
        public string _rid { get; set; }
        public string _class { get; set; }
        public int _version { get; set; }
        #endregion
    }
}
