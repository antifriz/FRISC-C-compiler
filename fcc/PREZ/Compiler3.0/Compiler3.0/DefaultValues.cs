using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace Compiler3_0
{
    public class DefaultValues
    {
        public string userPath = null;
        public string cygwinPath = null;
        public string atlasPath = null;
        public DefaultValues(string cygwin,string user,string atlas)
        {
            userPath = user;
            cygwinPath = cygwin;
            atlasPath = atlas;
        }
        public DefaultValues()
        {
        }

    }
}
